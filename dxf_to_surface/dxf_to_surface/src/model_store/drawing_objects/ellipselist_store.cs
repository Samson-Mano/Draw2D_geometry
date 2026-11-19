using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store.geom_objects;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public class ellipse_store
    {
        public int ellipse_id;
        public Vector2 ellipseCenterPt;
        public double ellipseMajorAxis;
        public double ellipseMinorAxis;
        public double ellipseRotationAngle;

        public int type; // 0 for full ellipse, 1 for elliptical arc

        public double startParameter;
        public double endParameter;

        // When type is 1 (arc), we can use the following properties to store the start and end points of the arc
        public Vector2 arcStartPt;
        public Vector2 arcEndPt;

    }



    public class ellipselist_store
    {

        public Dictionary<int, ellipse_store> ellipseMap { get; } = new Dictionary<int, ellipse_store>();

        private int ellipseCount = 0;

        private const int ELLIPSE_VERTEX_COUNT = 200; // Number of vertices to approximate a circle or arc

        // Rendering resources
        private VertexArray _ellipseVAO;
        private VertexBuffer _ellipseVBO;
        private IndexBuffer _ellipseIBO;

        private Shader _ellipseShader;



        public ellipselist_store()
        {
            ellipseMap = new Dictionary<int, ellipse_store>();
            ellipseCount = 0;

            InitializeShader();
            InitializeBuffers();

        }


        public void add_ellipse(int t_ellipse_id, Vector2 t_ellipseCenterPt, double t_ellipseMajorAxis, double t_ellipseMinorAxis,
            double t_ellipseRotationAngle, int t_type, double t_startParameter = 0.0, double t_endParameter = 0.0)
        {
            Vector2 startPt = new Vector2(0);
            Vector2 endPt = new Vector2(0);

            if (t_type == 0)
            {
                // Create the rotation matrix for the ellipse
                double cosTheta = Math.Cos(t_ellipseRotationAngle);
                double sinTheta = Math.Sin(t_ellipseRotationAngle);

                // Calculate the start and end points of the ellipse
                startPt = new Vector2(
                    (float)(t_ellipseCenterPt.X + t_ellipseMajorAxis * cosTheta),
                    (float)(t_ellipseCenterPt.Y + t_ellipseMajorAxis * sinTheta)
                );

                endPt = startPt; // For a full ellipse, the start and end points are the same


            }
            else if (t_type == 1)
            {
                // Create the rotation matrix for the ellipse
                double cosTheta = Math.Cos(t_ellipseRotationAngle);
                double sinTheta = Math.Sin(t_ellipseRotationAngle);


                // Calculate the start and end points of the arc based on the parameters
                double startAngleRad = t_startParameter;
                double endAngleRad = t_endParameter;

                // Calculate the start and end points of the arc
                startPt = new Vector2(
                    (float)(t_ellipseCenterPt.X + t_ellipseMajorAxis * Math.Cos(startAngleRad) * cosTheta - t_ellipseMinorAxis * Math.Sin(startAngleRad) * sinTheta),
                    (float)(t_ellipseCenterPt.Y + t_ellipseMajorAxis * Math.Cos(startAngleRad) * sinTheta + t_ellipseMinorAxis * Math.Sin(startAngleRad) * cosTheta)
                );
                endPt = new Vector2(
                    (float)(t_ellipseCenterPt.X + t_ellipseMajorAxis * Math.Cos(endAngleRad) * cosTheta - t_ellipseMinorAxis * Math.Sin(endAngleRad) * sinTheta),
                    (float)(t_ellipseCenterPt.Y + t_ellipseMajorAxis * Math.Cos(endAngleRad) * sinTheta + t_ellipseMinorAxis * Math.Sin(endAngleRad) * cosTheta)
                );
            }


            // Add the ellipse to the ellipseMap
            ellipse_store temp_ellipse = new ellipse_store
            {
                ellipse_id = t_ellipse_id,
                ellipseCenterPt = t_ellipseCenterPt,
                ellipseMajorAxis = t_ellipseMajorAxis,
                ellipseMinorAxis = t_ellipseMinorAxis,
                ellipseRotationAngle = t_ellipseRotationAngle,
                type = t_type,
                startParameter = t_startParameter,
                endParameter = t_endParameter,
                arcStartPt = startPt,
                arcEndPt = endPt
            };


            ellipseMap.Add(t_ellipse_id, temp_ellipse);
            ellipseCount++;

        }



        public void remove_ellipse(int t_ellipse_id)
        {
            // Remove the ellipse from the ellipseMap
            if (ellipseMap.ContainsKey(t_ellipse_id))
            {
                ellipseMap.Remove(t_ellipse_id);
                ellipseCount--;
            }
        }


        private void InitializeShader()
        {
            // Create Shader
            _ellipseShader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.LineShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.LineShader));
        }

        private void InitializeBuffers()
        {
            // Label buffer
            _ellipseVAO = new VertexArray();
            _ellipseVBO = new VertexBuffer(10);
            _ellipseIBO = new IndexBuffer(10);

            var ellipseLayout = new VertexBufferLayout();
            ellipseLayout.AddFloat(2);  // Ellipse vertex position (x, y)

            _ellipseVAO.Add_vertexBuffer(_ellipseVBO, ellipseLayout);
        }


        public void UpdateBuffers()
        {
            // Set the buffer for index (2 indices per line)
            List<int> ellipselinesegment_indices = new List<int>();

            // Set the line index buffers
            int t_id = 0;
            foreach (var ellipse in ellipseMap)
            {
                get_ellipse_index_buffer(ellipse.Value, ref ellipselinesegment_indices, ref t_id);
            }

            // Update the index buffer
            _ellipseIBO.ClearIndexBuffer();
            _ellipseIBO.AppendIndexBuffer(ellipselinesegment_indices.ToArray());


            // Define the vertex buffer size for a character
            List<float> ellipselinesegment_vertices = new List<float>();

            // Set the line vertex buffers
            foreach (var ellipse in ellipseMap)
            {
                // Add vertex buffers
                get_ellipse_vertex_buffer(ellipse.Value, ref ellipselinesegment_vertices);
            }


            // Update the vertex buffer
            _ellipseVBO.ClearVertexBuffer();
            _ellipseVBO.AppendVertexBuffer(ellipselinesegment_vertices.ToArray());

        }


        public void clear_ellipses()
        {
            // clear the ellipseMap and reset the count
            ellipseMap.Clear();
            ellipseCount = 0;

            // clear the buffers
            _ellipseVBO.ClearVertexBuffer();
            _ellipseIBO.ClearIndexBuffer();

        }

        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                            graphic_events_control.viewMatrix *
                            graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;


            // Update the openGl uniform matrices
            _ellipseShader.SetMatrix4("uMVP", uMVP);

            Vector3 ellipseColor = gvariables_static.ColorUtils.get_EllipseColor();
            Vector4 ellipseVertexColor = new Vector4(ellipseColor.X, ellipseColor.Y, ellipseColor.Z, gvariables_static.geom_transparency);

            _ellipseShader.SetVector4("lineVertexColor", ellipseVertexColor);

        }


        public void paint_ellipses()
        {
            // Bind the shader
            _ellipseShader.Bind();
            _ellipseVAO.Bind();
            _ellipseIBO.Bind();

            GL.LineWidth(gvariables_static.LineWidth); // Set the line width

            // Draw the lines
            GL.DrawElements(PrimitiveType.Lines, _ellipseIBO.BufferCount, DrawElementsType.UnsignedInt, 0);

            _ellipseShader.UnBind();
            _ellipseVAO.UnBind();
            _ellipseIBO.UnBind();

        }



        private void get_ellipse_vertex_buffer(ellipse_store ellipse, ref List<float> ellipse_vertices)
        {
            if (ellipse.type == 0) // Circle
            {
                // Create the rotation matrix for the ellipse
                double cosTheta = Math.Cos(ellipse.ellipseRotationAngle);
                double sinTheta = Math.Sin(ellipse.ellipseRotationAngle);


                for (int i = 0; i < ELLIPSE_VERTEX_COUNT; i++)
                {

                    double angle = (2.0 * Math.PI * i) / (double)ELLIPSE_VERTEX_COUNT;

                    float x = (float)(ellipse.ellipseCenterPt.X +
                        ellipse.ellipseMajorAxis * Math.Cos(angle) * cosTheta - ellipse.ellipseMinorAxis * Math.Sin(angle) * sinTheta);

                    float y = (float)(ellipse.ellipseCenterPt.Y +
                        ellipse.ellipseMajorAxis * Math.Cos(angle) * sinTheta + ellipse.ellipseMinorAxis * Math.Sin(angle) * cosTheta);


                    ellipse_vertices.Add(x);
                    ellipse_vertices.Add(y);
                }
            }
            else if (ellipse.type == 1) // Arc
            {

                // Create the rotation matrix for the ellipse
                double cosTheta = Math.Cos(ellipse.ellipseRotationAngle);
                double sinTheta = Math.Sin(ellipse.ellipseRotationAngle);


                double startAngleRad = ellipse.startParameter;
                double endAngleRad = ellipse.endParameter;

                // DXF arcs sweep counter-clockwise from start to end
                double sweep = endAngleRad - startAngleRad;

                // Wrap into (0, 2π]
                sweep = sweep % (2.0 * Math.PI);
                if (sweep <= 0.0)
                {
                    sweep += 2.0 * Math.PI;
                }


                // Vertex count proportional to sweep, with a minimum of 2 so the loop is valid
                int arcVertexCount = Math.Max(2, (int)Math.Ceiling(ELLIPSE_VERTEX_COUNT * (sweep / (2.0 * Math.PI))));


                for (int i = 0; i < arcVertexCount; i++)
                {
                    double t = (double)i / (double)(arcVertexCount - 1);
                    double angle = startAngleRad + t * sweep;

                    float x = (float)(ellipse.ellipseCenterPt.X +
                    ellipse.ellipseMajorAxis * Math.Cos(angle) * cosTheta - ellipse.ellipseMinorAxis * Math.Sin(angle) * sinTheta);

                    float y = (float)(ellipse.ellipseCenterPt.Y +
                        ellipse.ellipseMajorAxis * Math.Cos(angle) * sinTheta + ellipse.ellipseMinorAxis * Math.Sin(angle) * cosTheta);

                    ellipse_vertices.Add(x);
                    ellipse_vertices.Add(y);
                }
            }

        }



        private void get_ellipse_index_buffer(ellipse_store ellipse, ref List<int> ellipse_indices, ref int t_id)
        {

            if (ellipse.type == 0) // Ellipse
            {

                for (int i = 0; i < ELLIPSE_VERTEX_COUNT - 1; i++)
                {
                    ellipse_indices.Add(t_id + i);
                    ellipse_indices.Add(t_id + (i + 1));

                }

                // Last index to first index to close the ellipse
                ellipse_indices.Add(t_id + ELLIPSE_VERTEX_COUNT - 1);
                ellipse_indices.Add(t_id);

                t_id += ELLIPSE_VERTEX_COUNT;

            }
            else if (ellipse.type == 1) // Elliptical Arc
            {
                double startAngleRad = ellipse.startParameter;
                double endAngleRad = ellipse.endParameter;

                // DXF arcs sweep counter-clockwise from start to end
                double sweep = endAngleRad - startAngleRad;

                // Wrap into (0, 2π]
                sweep = sweep % (2.0 * Math.PI);
                if (sweep <= 0.0)
                {
                    sweep += 2.0 * Math.PI;
                }


                // Vertex count proportional to sweep, with a minimum of 2 so the loop is valid
                int arcVertexCount = Math.Max(2, (int)Math.Ceiling(ELLIPSE_VERTEX_COUNT * (sweep / (2.0 * Math.PI))));

                for (int i = 0; i < arcVertexCount - 1; i++)
                {
                    ellipse_indices.Add(t_id + i);
                    ellipse_indices.Add(t_id + (i + 1));

                }

                t_id += arcVertexCount;

            }
        }


        public void getModelScalingPoints(ref List<Vector2> scalingPoints)
        {
            List<float> ellipse_vertices = new List<float>();

            foreach (var ellipse in ellipseMap.Values)
            {
                get_ellipse_vertex_buffer(ellipse, ref ellipse_vertices);
            }

            for (int i = 0; i < ellipse_vertices.Count; i += 2)
            {
                scalingPoints.Add(new Vector2(ellipse_vertices[i], ellipse_vertices[i + 1]));
            }
        }



    }

}
