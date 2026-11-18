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

    public class spline_store
    {
        public int spline_id;
        public int degree;
        public List<Vector2> controlPoints;
        public List<double> weights;     // 1.0 for non-rational
        public List<double> knots;       // length == controlPoints.Count + degree + 1
        public bool isClosed;
        public bool isPeriodic;
        public bool isRational;

        // fit-point splines (LibreCAD "spline through points"):
        // set controlPoints/weights/knots by *interpolation* at import, OR
        // keep fitPoints and interpolate lazily. Prefer the former.
        public List<Vector2> fitPoints;  // informational / fallback
    }


    public class splinelist_store
    {

        public Dictionary<int, spline_store> splineMap { get; } = new Dictionary<int, spline_store>();
        private int splineCount = 0;

        private const int SPLINE_VERTEX_COUNT = 200; // Number of vertices to approximate a spline

        // Rendering resources
        private VertexArray _splineVAO;
        private VertexBuffer _splineVBO;
        private IndexBuffer _splineIBO;

        private Shader _splineShader;


        public splinelist_store()
        {
            splineMap = new Dictionary<int, spline_store>();
            splineCount = 0;

            InitializeShader();
            InitializeBuffers();

        }


        public void add_spline(int splineID, List<Vector2> controlPoints, List<double> weights,
            List<double> knots, bool isClosed, bool isPeriodic, bool isRational, List<Vector2> fitPoints)
        {
            var spline = new spline_store
            {
                spline_id = splineID,
                degree = controlPoints.Count - 1,
                controlPoints = controlPoints,
                weights = weights,
                knots = knots,
                isClosed = isClosed,
                isPeriodic = isPeriodic,
                isRational = isRational,
                fitPoints = fitPoints
            };

            splineMap.Add(splineID, spline);
        }



        public void remove_spline(int splineID)
        {
            if (splineMap.ContainsKey(splineID))
            {
                splineMap.Remove(splineID);
            }
        }



        private void InitializeShader()
        {
            // Create Shader
            _splineShader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.LineShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.LineShader));
        }



        private void InitializeBuffers()
        {
            // Label buffer
            _splineVAO = new VertexArray();
            _splineVBO = new VertexBuffer(10);
            _splineIBO = new IndexBuffer(10);

            var splineLayout = new VertexBufferLayout();
            splineLayout.AddFloat(2);  // Spline vertex position (x, y)

            _splineVAO.Add_vertexBuffer(_splineVBO, splineLayout);
        }


        public void UpdateBuffers()
        {
            // Set the buffer for index (2 indices per line)
            List<int> splinelinesegment_indices = new List<int>();

            // Set the line index buffers
            int t_id = 0;
            foreach (var spline in splineMap)
            {
                get_spline_index_buffer(spline.Value, ref splinelinesegment_indices, ref t_id);
            }

            // Update the index buffer
            _splineIBO.ClearIndexBuffer();
            _splineIBO.AppendIndexBuffer(splinelinesegment_indices.ToArray());


            // Define the vertex buffer size for a character
            List<float> splinelinesegment_vertices = new List<float>();

            // Set the line vertex buffers
            foreach (var spline in splineMap)
            {
                // Add vertex buffers
                get_spline_vertex_buffer(spline.Value, ref splinelinesegment_vertices);
            }


            // Update the vertex buffer
            _splineVBO.ClearVertexBuffer();
            _splineVBO.AppendVertexBuffer(splinelinesegment_vertices.ToArray());

        }


        public void clear_splines()
        {
            // clear the splineMap and reset the count
            splineMap.Clear();
            splineCount = 0;

            // clear the buffers
            _splineVBO.ClearVertexBuffer();
            _splineIBO.ClearIndexBuffer();

        }

        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                            graphic_events_control.viewMatrix *
                            graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;


            // Update the openGl uniform matrices
            _splineShader.SetMatrix4("uMVP", uMVP);

            Vector3 splineColor = gvariables_static.ColorUtils.get_SplineColor();
            Vector4 splineVertexColor = new Vector4(splineColor.X, splineColor.Y, splineColor.Z, gvariables_static.geom_transparency);

            _splineShader.SetVector4("lineVertexColor", splineVertexColor);

        }

        public void paint_splines()
        {
            // Bind the shader
            _splineShader.Bind();
            _splineVAO.Bind();
            _splineIBO.Bind();

            GL.LineWidth(gvariables_static.LineWidth); // Set the line width

            // Draw the lines
            GL.DrawElements(PrimitiveType.Lines, _splineIBO.BufferCount, DrawElementsType.UnsignedInt, 0);

            _splineShader.UnBind();
            _splineVAO.UnBind();
            _splineIBO.UnBind();

        }



        private void get_spline_vertex_buffer(spline_store spline, ref List<float> spline_vertices)
        {
            for (int i = 0; i < SPLINE_VERTEX_COUNT; i++)
            {

                float x = 0.0f;
                float y = 0.0f;

                spline_vertices.Add(x);
                spline_vertices.Add(y);
            }

        }

        private void get_spline_index_buffer(spline_store spline, ref List<int> spline_indices, ref int t_id)
        {
            for (int i = 0; i < SPLINE_VERTEX_COUNT - 1; i++)
            {
                spline_indices.Add(t_id + i);
                spline_indices.Add(t_id + (i + 1));

            }

            t_id += SPLINE_VERTEX_COUNT;

        }


    }
}
