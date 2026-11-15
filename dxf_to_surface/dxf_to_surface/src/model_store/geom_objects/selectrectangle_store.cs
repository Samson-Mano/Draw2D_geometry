// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace dxf_to_surface.src.model_store.geom_objects
{
    public class selectrectangle_store : IDisposable
    {

        // Constants
        private const int VERTEX_COUNT = 4;
        private const int LINE_INDEX_COUNT = 8;  // 4 lines * 2 indices
        private const int TRIANGLE_INDEX_COUNT = 6;  // 2 triangles * 3 indices
        private const int FLOATS_PER_VERTEX = 2;

        // Rectangle state
        private Vector2 _originPoint = Vector2.Zero;
        private Vector2 _currentPoint = Vector2.Zero;
        private bool _isDrawing = false;

        // Rendering resources
        private VertexArray _boundaryVAO;
        private VertexBuffer _boundaryVBO;
        private IndexBuffer _boundaryIBO;

        private VertexArray _interiorVAO;
        private VertexBuffer _interiorVBO;
        private IndexBuffer _interiorIBO;

        private Shader _shader;
        private bool _disposed = false;

        // Properties
        public bool IsDrawing => _isDrawing;


        public selectrectangle_store()
        {
            InitializeShader();
            InitializeBuffers();
            SetupIndexBuffers();
        }

        private void InitializeShader()
        {
            // Create Shader
            _shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.SelectionShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.SelectionShader));

        }


        private void InitializeBuffers()
        {
            // Boundary (wireframe)
            _boundaryVAO = new VertexArray();
            _boundaryVBO = new VertexBuffer(VERTEX_COUNT * FLOATS_PER_VERTEX);
            _boundaryIBO = new IndexBuffer(LINE_INDEX_COUNT);

            var boundaryLayout = new VertexBufferLayout();
            boundaryLayout.AddFloat(FLOATS_PER_VERTEX);

            _boundaryVAO.Add_vertexBuffer(_boundaryVBO, boundaryLayout);

            //_________________________________________________________________________
            // Interior (filled)
            _interiorVAO = new VertexArray();
            _interiorVBO = new VertexBuffer(VERTEX_COUNT * FLOATS_PER_VERTEX);
            _interiorIBO = new IndexBuffer(TRIANGLE_INDEX_COUNT);

            var interiorLayout = new VertexBufferLayout();
            interiorLayout.AddFloat(FLOATS_PER_VERTEX);

            _interiorVAO.Add_vertexBuffer(_interiorVBO, interiorLayout);

        }


        private void SetupIndexBuffers()
        {
            // Setup boundary indices (line loop)
            int[] lineIndices = {
            0, 1,  // Bottom edge
            1, 2,  // Right edge
            2, 3,  // Top edge
            3, 0   // Left edge
        };
            _boundaryIBO.AppendIndexBuffer(lineIndices);

            // Setup interior indices (two triangles)
            int[] triangleIndices = {
            0, 1, 3,  // First triangle
            3, 1, 2   // Second triangle
        };
            _interiorIBO.AppendIndexBuffer(triangleIndices);
        }



        public void UpdateSelectionRectangle(Vector2 origin, Vector2 current, bool isDrawing)
        {
            if (!isDrawing)
            {
                _isDrawing = false;
                return;
            }

            // Assign to private rectangle points
            this._originPoint = origin;
            this._currentPoint = current;
            this._isDrawing = true;

            UpdateVertexBuffers();

        }


        private void UpdateVertexBuffers()
        {
            // Define the 4 corners of the rectangle
            float[] vertices = new float[VERTEX_COUNT * FLOATS_PER_VERTEX];

            // Bottom-left
            vertices[0] = _originPoint.X;
            vertices[1] = _originPoint.Y;

            // Bottom-right
            vertices[2] = _currentPoint.X;
            vertices[3] = _originPoint.Y;

            // Top-right
            vertices[4] = _currentPoint.X;
            vertices[5] = _currentPoint.Y;

            // Top-left
            vertices[6] = _originPoint.X;
            vertices[7] = _currentPoint.Y;

            // Update both VBOs with the same vertices
            _boundaryVBO.updateVertexBuffer(vertices);
            _interiorVBO.updateVertexBuffer(vertices);
        }



        public void draw_selection_rectangle()
        {
            if (!_isDrawing)
                return;

            _shader.Bind();

            // Draw boundary (wireframe)
            _boundaryVAO.Bind();
            _boundaryIBO.Bind();
            GL.DrawElements(PrimitiveType.Lines, LINE_INDEX_COUNT,
                           DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Draw interior (filled with transparency)
            _interiorVAO.Bind();
            _interiorIBO.Bind();
            GL.DrawElements(PrimitiveType.Triangles, TRIANGLE_INDEX_COUNT,
                           DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Cleanup
            _boundaryVAO.UnBind();
            _interiorVAO.UnBind();
            _shader.UnBind();

        }


        public void Dispose()
        {
            if (!_disposed)
            {
                _boundaryVAO?.Dispose();
                _boundaryVBO?.Dispose();
                _boundaryIBO?.Dispose();
                _interiorVAO?.Dispose();
                _interiorVBO?.Dispose();
                _interiorIBO?.Dispose();
                // _shader?.Dispose();
                _disposed = true;
            }
        }
         


    }
}
