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
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace dxf_to_surface.src.model_store.drawing_objects
{

    public class line_store
    {
        public int line_id;

        public Vector2 lineStartPt;
        public Vector2 lineEndPt;
    }



    public class linelist_store
    {

        public Dictionary<int, line_store> lineMap { get; } = new Dictionary<int, line_store>();
        private int lineCount = 0;

        // Rendering resources
        private VertexArray _lineVAO;
        private VertexBuffer _lineVBO;
        private IndexBuffer _lineIBO;

        private Shader _lineShader;


        public linelist_store()
        {
            // (Re)Initialize the data
            lineMap = new Dictionary<int, line_store>();
            lineCount = 0;

            InitializeShader();
            InitializeBuffers();

        }


        public void add_line(int t_line_id, Vector2 t_lineStartPt, Vector2 t_lineEndPt)
        {
            // Add the line to the lineMap
            line_store temp_line = new line_store
            {
                line_id = t_line_id,
                lineStartPt = t_lineStartPt,
                lineEndPt = t_lineEndPt
            };

            lineMap[temp_line.line_id] = temp_line;
            lineCount++;

            // Update the vertex buffer and index buffer
           // UpdateBuffers();
        }


        public void remove_line(int t_line_id)
        {
            // Remove the line from the lineMap
            if (lineMap.ContainsKey(t_line_id))
            {
                lineMap.Remove(t_line_id);
                lineCount--;

                // Update the vertex buffer and index buffer
               // UpdateBuffers();
            }
        }



        private void InitializeShader()
        {
            // Create Shader
            _lineShader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.LineShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.LineShader));

        }


        private void InitializeBuffers()
        {
            // Label buffer
            _lineVAO = new VertexArray();
            _lineVBO = new VertexBuffer(10);
            _lineIBO = new IndexBuffer(10);

            var lineLayout = new VertexBufferLayout();
            lineLayout.AddFloat(2);  // Line vertex position (x, y)

            _lineVAO.Add_vertexBuffer(_lineVBO, lineLayout);

        }


        public void UpdateBuffers()
        {

            // Set the buffer for index (2 indices per line)
            List<int> line_indices = new List<int>();

            // Set the line index buffers
            int t_id = 0;
            foreach (var line in lineMap)
            {
                get_line_index_buffer(line.Value, ref line_indices, ref t_id);
            }

            // Update the index buffer
            _lineIBO.ClearIndexBuffer();
            _lineIBO.AppendIndexBuffer(line_indices.ToArray());


            // Define the vertex buffer size for a character
            List<float> line_vertices = new List<float>();

            // Set the line vertex buffers
            foreach (var line in lineMap)
            {
                // Add vertex buffers
                get_line_vertex_buffer(line.Value, ref line_vertices);
            }


            // Update the vertex buffer
            _lineVBO.ClearVertexBuffer();
            _lineVBO.AppendVertexBuffer(line_vertices.ToArray());

        }


        public void clear_lines()
        {
            // Clear the lineMap and reset lineCount
            lineMap.Clear();
            lineCount = 0;

            // Clear the buffers
            _lineIBO.ClearIndexBuffer();
            _lineVBO.ClearVertexBuffer();

        }


        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                            graphic_events_control.viewMatrix *
                            graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;


            // Update the openGl uniform matrices
            _lineShader.SetMatrix4("uMVP", uMVP);

            Vector3 lineColor = gvariables_static.ColorUtils.get_LineColor();
            Vector4 lineVertexColor = new Vector4(lineColor.X, lineColor.Y, lineColor.Z, gvariables_static.geom_transparency);

            _lineShader.SetVector4("lineVertexColor", lineVertexColor);

        }


        public void paint_lines()
        {
            // Bind the shader
            _lineShader.Bind();
            _lineVAO.Bind();
            _lineIBO.Bind();

            GL.LineWidth(gvariables_static.LineWidth); // Set the line width

            // Draw the lines
            GL.DrawElements(PrimitiveType.Lines, _lineIBO.BufferCount, DrawElementsType.UnsignedInt, 0);

            _lineShader.UnBind();
            _lineVAO.UnBind();
            _lineIBO.UnBind();

        }



        private void get_line_vertex_buffer(line_store line, ref List<float> line_vertices)
        {

            // Start point
            line_vertices.Add(line.lineStartPt.X);
            line_vertices.Add(line.lineStartPt.Y);

            // End point
            line_vertices.Add(line.lineEndPt.X);
            line_vertices.Add(line.lineEndPt.Y);

        }



        private void get_line_index_buffer(line_store line, ref List<int> line_indices, ref int t_id)
        {
 
            line_indices.Add((t_id * 2) + 0);     // Start point index
            line_indices.Add((t_id * 2) + 1); // End point index

            t_id++;

        }



    }
}
