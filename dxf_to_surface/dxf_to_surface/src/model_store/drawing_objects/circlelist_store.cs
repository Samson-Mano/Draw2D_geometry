using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store.geom_objects;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public class circle_store
    {
        public int circle_id;
        public Vector2 circleCenterPt;
        public double circleRadius;
        public double startAngle;// radians, CCW from +X, [0, 2π)
        public double endAngle; // radians, CCW from +X, [0, 2π)
        public int type; // 0 for circle, 1 for arc

        // When type is 1 (arc), we can use the following properties to store the start and end points of the arc
        public Vector2 arcStartPt;
        // public Vector2 arcMidPt; // Optional: midpoint of the arc, can be calculated if needed
        public Vector2 arcEndPt;

        // P(θ), θ in radians
        public Vector2 GetCirclePoint(double angle)
        {
            double x = circleCenterPt.X + circleRadius * Math.Cos(angle);
            double y = circleCenterPt.Y + circleRadius * Math.Sin(angle);
            return new Vector2((float)x, (float)y);
        }

        // dP/dθ, unnormalized. Not the unit tangent.
        public Vector2 GetCircleDerivative(double angle)
        {
            double dx = -circleRadius * Math.Sin(angle);
            double dy = circleRadius * Math.Cos(angle);
            return new Vector2((float)dx, (float)dy);
        }

        // d²P/dθ², unnormalized.
        public Vector2 GetCircleSecondDerivative(double angle)
        {
            double ddx = -circleRadius * Math.Cos(angle);
            double ddy = -circleRadius * Math.Sin(angle);
            return new Vector2((float)ddx, (float)ddy);
        }

        // Arc length of the sweep [startAngle, endAngle] in CCW direction.
        // For a full circle (type == 0), this is 2πr.
        public double GetCircleArcLength(double fromAngle, double toAngle)
        {
            double sweep = CCWSweep(fromAngle, toAngle);
            return circleRadius * sweep;
        }

        // Keep this for code that wants the unit tangent directly; it's just
        // GetCircleDerivative normalized. Useful in tests.
        public Vector2 GetCircleTangent(double angle)
        {
            Vector2 d = GetCircleDerivative(angle);
            double len = Math.Sqrt(d.X * d.X + d.Y * d.Y);
            return len < 1e-12 ? new Vector2(0, 0) : new Vector2((float)(d.X / len), (float)(d.Y / len));
        }


        public static double CCWSweep(double from, double to)
        {
            double twoPi = 2.0 * Math.PI;
            double d = ((to % twoPi) + twoPi) % twoPi - ((from % twoPi) + twoPi) % twoPi;
            if (d < 0) d += twoPi;
            return d;
        }
    }


    public class circlelist_store
    {

        public Dictionary<int, circle_store> circleMap { get; } = new Dictionary<int, circle_store>();
        private int circleCount = 0;

        private const int CIRCLE_VERTEX_COUNT = 200; // Number of vertices to approximate a circle or arc

        // Rendering resources
        private VertexArray _circleVAO;
        private VertexBuffer _circleVBO;
        private IndexBuffer _circleIBO;

        private Shader _circleShader;



        public circlelist_store()
        {
            circleMap = new Dictionary<int, circle_store>();
            circleCount = 0;

            InitializeShader();
            InitializeBuffers();

        }


        public void add_circle(int circleID, Vector2 centerPt, double radius, double startAngle, double endAngle, int type)
        {
            Vector2 startPt = new Vector2(0);
            Vector2 endPt = new Vector2(0);

            if (type == 0) // Circle
            {
                startAngle = 0.0;
                endAngle = 360.0;

                startPt = new Vector2(centerPt.X + (float)radius, centerPt.Y);
                endPt = new Vector2(centerPt.X + (float)radius, centerPt.Y); // same as start point for a full circle
            }
            else if (type == 1) // Arc
            {
                // Calculate the start and end points of the arc based on the center point, radius, and angles
                startPt = new Vector2(centerPt.X + (float)(radius * Math.Cos(startAngle * Math.PI / 180.0)),
                                              centerPt.Y + (float)(radius * Math.Sin(startAngle * Math.PI / 180.0)));
                endPt = new Vector2(centerPt.X + (float)(radius * Math.Cos(endAngle * Math.PI / 180.0)),
                                            centerPt.Y + (float)(radius * Math.Sin(endAngle * Math.PI / 180.0)));
            }


            var circle = new circle_store
            {
                circle_id = circleID,
                circleCenterPt = centerPt,
                circleRadius = radius,
                startAngle = startAngle,
                endAngle = endAngle,
                type = type,
                arcStartPt = startPt,
                arcEndPt = endPt
            };

            circleMap.Add(circleID, circle);
        }


        public void remove_circle(int circleID)
        {
            if (circleMap.ContainsKey(circleID))
            {
                circleMap.Remove(circleID);
            }


        }


        private void InitializeShader()
        {
            // Create Shader
            _circleShader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.LineShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.LineShader));
        }



        private void InitializeBuffers()
        {
            // Label buffer
            _circleVAO = new VertexArray();
            _circleVBO = new VertexBuffer(10);
            _circleIBO = new IndexBuffer(10);

            var circleLayout = new VertexBufferLayout();
            circleLayout.AddFloat(2);  // Circle vertex position (x, y)

            _circleVAO.Add_vertexBuffer(_circleVBO, circleLayout);
        }


        public void UpdateBuffers()
        {
            // Set the buffer for index (2 indices per line)
            List<int> circlelinesegment_indices = new List<int>();

            // Set the line index buffers
            int t_id = 0;
            foreach (var circle in circleMap)
            {
                get_circle_index_buffer(circle.Value, ref circlelinesegment_indices, ref t_id);
            }

            // Update the index buffer
            _circleIBO.ClearIndexBuffer();
            _circleIBO.AppendIndexBuffer(circlelinesegment_indices.ToArray());


            // Define the vertex buffer size for a character
            List<float> circlelinesegment_vertices = new List<float>();

            // Set the line vertex buffers
            foreach (var circle in circleMap)
            {
                // Add vertex buffers
                get_circle_vertex_buffer(circle.Value, ref circlelinesegment_vertices);
            }


            // Update the vertex buffer
            _circleVBO.ClearVertexBuffer();
            _circleVBO.AppendVertexBuffer(circlelinesegment_vertices.ToArray());

        }


        public void clear_circles()
        {
            // clear the circleMap and reset the count
            circleMap.Clear();
            circleCount = 0;

            // clear the buffers
            _circleVBO.ClearVertexBuffer();
            _circleIBO.ClearIndexBuffer();

        }

        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                            graphic_events_control.viewMatrix *
                            graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;


            // Update the openGl uniform matrices
            _circleShader.SetMatrix4("uMVP", uMVP);

            Vector3 circleColor = gvariables_static.ColorUtils.get_CircleColor();
            Vector4 circleVertexColor = new Vector4(circleColor.X, circleColor.Y, circleColor.Z, gvariables_static.geom_transparency);

            _circleShader.SetVector4("lineVertexColor", circleVertexColor);

        }

        public void paint_circles()
        {
            // Bind the shader
            _circleShader.Bind();
            _circleVAO.Bind();
            _circleIBO.Bind();

            GL.LineWidth(gvariables_static.LineWidth); // Set the line width

            // Draw the lines
            GL.DrawElements(PrimitiveType.Lines, _circleIBO.BufferCount, DrawElementsType.UnsignedInt, 0);

            _circleShader.UnBind();
            _circleVAO.UnBind();
            _circleIBO.UnBind();

        }



        private void get_circle_vertex_buffer(circle_store circle, ref List<float> circle_vertices)
        {
            if (circle.type == 0) // Circle
            {
                for (int i = 0; i < CIRCLE_VERTEX_COUNT; i++)
                {

                    double angle = (2.0 * Math.PI * i) / (double)CIRCLE_VERTEX_COUNT;

                    float x = circle.circleCenterPt.X + (float)(circle.circleRadius * Math.Cos(angle));
                    float y = circle.circleCenterPt.Y + (float)(circle.circleRadius * Math.Sin(angle));

                    circle_vertices.Add(x);
                    circle_vertices.Add(y);
                }
            }
            else if (circle.type == 1) // Arc
            {

                double startAngleRad = circle.startAngle * Math.PI / 180.0;
                double endAngleRad = circle.endAngle * Math.PI / 180.0;

                // DXF arcs sweep counter-clockwise from start to end
                double sweep = endAngleRad - startAngleRad;

                // Wrap into (0, 2π]
                sweep = sweep % (2.0 * Math.PI);
                if (sweep <= 0.0)
                {
                    sweep += 2.0 * Math.PI;
                }


                // Vertex count proportional to sweep, with a minimum of 2 so the loop is valid
                int arcVertexCount = Math.Max(2, (int)Math.Ceiling(CIRCLE_VERTEX_COUNT * (sweep / (2.0 * Math.PI))));


                for (int i = 0; i < arcVertexCount; i++)
                {
                    double t = (double)i / (double)(arcVertexCount - 1);
                    double angle = startAngleRad + t * sweep;

                    float x = circle.circleCenterPt.X + (float)(circle.circleRadius * Math.Cos(angle));
                    float y = circle.circleCenterPt.Y + (float)(circle.circleRadius * Math.Sin(angle));

                    circle_vertices.Add(x);
                    circle_vertices.Add(y);
                }
            }

        }



        private void get_circle_index_buffer(circle_store circle, ref List<int> circle_indices, ref int t_id)
        {

            if (circle.type == 0) // Circle
            {

                for (int i = 0; i < CIRCLE_VERTEX_COUNT-1; i++)
                {
                    circle_indices.Add(t_id + i);
                    circle_indices.Add(t_id + (i + 1));
                
                }

                // Last index to first index to close the circle
                circle_indices.Add(t_id + CIRCLE_VERTEX_COUNT-1);
                circle_indices.Add(t_id);

                t_id += CIRCLE_VERTEX_COUNT;

            }
            else if (circle.type == 1) // Arc
            {
                double startAngleRad = circle.startAngle * Math.PI / 180.0;
                double endAngleRad = circle.endAngle * Math.PI / 180.0;

                // DXF arcs sweep counter-clockwise from start to end
                double sweep = endAngleRad - startAngleRad;

                // Wrap into (0, 2π]
                sweep = sweep % (2.0 * Math.PI);
                if (sweep <= 0.0)
                {
                    sweep += 2.0 * Math.PI;
                }


                // Vertex count proportional to sweep, with a minimum of 2 so the loop is valid
                int arcVertexCount = Math.Max(2, (int)Math.Ceiling(CIRCLE_VERTEX_COUNT * (sweep / (2.0 * Math.PI))));

                for (int i = 0; i < arcVertexCount - 1; i++)
                {
                    circle_indices.Add(t_id + i);
                    circle_indices.Add(t_id + (i + 1));
                  
                }

                t_id += arcVertexCount;

            }
        }



        public void getModelScalingPoints(ref List<Vector2> scalingPoints)
        {
            List<float> circle_vertices = new List<float>();

            foreach (var circle in circleMap.Values)
            {
                get_circle_vertex_buffer(circle, ref circle_vertices);
            }

            for (int i = 0; i < circle_vertices.Count; i += 2)
            {
                scalingPoints.Add(new Vector2(circle_vertices[i], circle_vertices[i + 1]));
            }

        }


    }
}
