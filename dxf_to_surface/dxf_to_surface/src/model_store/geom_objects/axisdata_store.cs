using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.model_store.geom_objects;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace dxf_to_surface.src.model_store.geom_objects
{
    public class axisdata_store : IDisposable
    {

        // XY labels
        private label_list_store XY_label; // = new label_list_store();

        // Constants
        private const int VERTEX_COUNT = 8; // 4 points for single arrow, 8 points total, 
        private const int LINE_INDEX_COUNT = 12;  // 6 lines * 2 indices

        // Drawing area rectangle point
        private Vector2 _leftCornerPoint = Vector2.Zero;


        // Rendering resources
        private VertexArray _axisarrowVAO;
        private VertexBuffer _axisarrowVBO;
        private IndexBuffer _axisarrowIBO;

        private Shader _axisshader;
        private bool _disposed = false;

        private bool IsInitialized = false;

        public axisdata_store()
        {
            // Empty constructor
        }


        public void InitializeAxisData(int window_width, int window_height)
        {
            // Initialize axis data here
            InitializeShader();
            InitializeBuffers();
            SetupIndexBuffers();

            IsInitialized = true;

            XY_label = new label_list_store();

            UpdateAxisArrowCenter(window_width, window_height);
        }

        private void InitializeShader()
        {
            // Create Shader
            _axisshader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.DrawingAxisShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.DrawingAxisShader));


        }


        private void InitializeBuffers()
        {
            // Axis arrows 
            _axisarrowVAO = new VertexArray();
            _axisarrowVBO = new VertexBuffer(VERTEX_COUNT * 5);
            _axisarrowIBO = new IndexBuffer(LINE_INDEX_COUNT);

            var axisarrowLayout = new VertexBufferLayout();
            axisarrowLayout.AddFloat(2);
            axisarrowLayout.AddFloat(3);

            _axisarrowVAO.Add_vertexBuffer(_axisarrowVBO, axisarrowLayout);
        }


        private void SetupIndexBuffers()
        {
            // Setup axis arrow indices
            int[] lineIndices = {
            0, 1,  // X arrow line
            1, 2,  // X arrow head1
            1, 3,  // X arrow head2
            4, 5,  // Y arrow line
            5, 6,  // Y arrow head1
            5, 7  // Y arrow head2
        };
            _axisarrowIBO.AppendIndexBuffer(lineIndices);
        }


        public void UpdateAxisArrowCenter(int window_width, int window_height)
        {
            if (!IsInitialized)
                return;


            // 1. Determine max dimension
            int max_drawing_area_size = Math.Max(window_width, window_height);

            int drawing_area_center_x = (int)((window_width - max_drawing_area_size) * 0.5f);
            int drawing_area_center_y = (int)((window_height - max_drawing_area_size) * 0.5f);

            // 2. Normalize screen dimensions
            double normalizedScreenWidth = 2.0d * ((double)window_width / (double)max_drawing_area_size);
            double normalizedScreenHeight = 2.0d * ((double)window_height / (double)max_drawing_area_size);

            float leftX = -(float)normalizedScreenWidth * 0.5f;
            float leftY = -(float)normalizedScreenHeight * 0.5f;

            // Assign to private rectangle points
            this._leftCornerPoint = new Vector2(leftX, leftY);


            UpdateVertexBuffers();

            // Update the label shader uniforms for the new window size
            Matrix4 uMVP = Matrix4.Identity;

            XY_label.update_openTK_uniforms(uMVP, 1.0f, 1.0f);

        }


        private void UpdateVertexBuffers()
        {
            // Define the 4 corners of the rectangle
            float[] vertices = new float[VERTEX_COUNT * 5];

            float OriginX = _leftCornerPoint.X + 0.06f;
            float OriginY = _leftCornerPoint.Y + 0.06f;

            // Arrow 1 (X-axis) - Red
            // Arrow 1 Origin Point (0,0) - This is the origin of the arrow
            vertices[0] = OriginX;
            vertices[1] = OriginY;
            vertices[2] = 1.0f; // Red
            vertices[3] = 0.0f; // Green
            vertices[4] = 0.0f; // Blue

            // Arrow 1 Tip Point (1,0)
            vertices[5] = OriginX + 0.1f;
            vertices[6] = OriginY;
            vertices[7] = 1.0f; // Red
            vertices[8] = 0.0f; // Green
            vertices[9] = 0.0f; // Blue

            // Arrow 1 Head Point (0.08,0.015)
            vertices[10] = OriginX + 0.08f;
            vertices[11] = OriginY + 0.015f;
            vertices[12] = 1.0f; // Red
            vertices[13] = 0.0f; // Green
            vertices[14] = 0.0f; // Blue

            // Arrow 1 Head Point (0.08,-0.015)
            vertices[15] = OriginX + 0.08f;
            vertices[16] = OriginY - 0.015f;
            vertices[17] = 1.0f; // Red
            vertices[18] = 0.0f; // Green
            vertices[19] = 0.0f; // Blue

            // Arrow 2 (Y-axis) - Blue
            // Arrow 2 Origin Point (0,0) - This is the origin of the arrow
            vertices[20] = OriginX;
            vertices[21] = OriginY;
            vertices[22] = 0.0f; // Red
            vertices[23] = 0.0f; // Green
            vertices[24] = 1.0f; // Blue

            // Arrow 2 Tip Point (0,1)
            vertices[25] = OriginX;
            vertices[26] = OriginY + 0.1f;
            vertices[27] = 0.0f; // Red
            vertices[28] = 0.0f; // Green
            vertices[29] = 1.0f; // Blue

            // Arrow 2 Head Point (0.015,0.08)
            vertices[30] = OriginX + 0.015f;
            vertices[31] = OriginY + 0.08f;
            vertices[32] = 0.0f; // Red
            vertices[33] = 0.0f; // Green
            vertices[34] = 1.0f; // Blue

            // Arrow 2 Head Point (-0.015,0.08)
            vertices[35] = OriginX - 0.015f;
            vertices[36] = OriginY + 0.08f;
            vertices[37] = 0.0f; // Red
            vertices[38] = 0.0f; // Green
            vertices[39] = 1.0f; // Blue

            // Update both VBOs with the same vertices
            _axisarrowVBO.updateVertexBuffer(vertices);

            // Add the labels for the X and Y axes
             XY_label.clear_labels();

             XY_label.add_label(0, "X", new Vector2(OriginX + 0.11f, OriginY - 0.001f), new Vector3(1.0f, 0.0f, 0.0f));
             XY_label.add_label(1, "Y", new Vector2(OriginX + 0.018f, OriginY + 0.135f), new Vector3(0.0f, 0.0f, 1.0f));

            XY_label.update_buffer(1.4f);

        }



        public void draw_axis_arrows()
        {

            _axisshader.Bind();

            // Draw Axis Arrows
            _axisarrowVAO.Bind();
            _axisarrowIBO.Bind();

            GL.LineWidth(3.0f);
            GL.DrawElements(PrimitiveType.Lines, LINE_INDEX_COUNT,
                           DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.LineWidth(1.0f);

            // Cleanup
            _axisarrowVAO.UnBind();
            _axisarrowIBO.UnBind();
            _axisshader.UnBind();


            XY_label.paint_static_labels();

        }



        public void Dispose()
        {
            if (!_disposed)
            {
                _axisarrowVAO?.Dispose();
                _axisarrowVBO?.Dispose();
                _axisarrowIBO?.Dispose();
                // _shader?.Dispose();
                _disposed = true;
            }
        }



    }
}
