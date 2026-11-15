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
    public class selectcircle_store : IDisposable
    {

        // Constants
        private const int FLOATS_PER_VERTEX = 2;
        private const int NUM_SEGMENTS = 36;  // Renamed for clarity
        private const int VERTEX_COUNT = NUM_SEGMENTS + 1;  // +1 for center point
        private const int LINE_INDEX_COUNT = 2 * NUM_SEGMENTS;  // Each line needs 2 indices
        private const int TRIANGLE_INDEX_COUNT = 3 * NUM_SEGMENTS;  // Each triangle needs 3 indices



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



        public selectcircle_store()
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
            // Setup boundary indices (circle outline)
            int[] lineIndices = new int[LINE_INDEX_COUNT];

            for (int i = 0; i < NUM_SEGMENTS; i++)
            {
                int currentIdx = i + 1;  // +1 because vertex 0 is center
                int nextIdx = ((i + 1) % NUM_SEGMENTS) + 1;

                lineIndices[i * 2] = currentIdx;
                lineIndices[i * 2 + 1] = nextIdx;
            }

            _boundaryIBO.AppendIndexBuffer(lineIndices);

            // Setup interior indices (triangles from center to edges)
            int[] triangleIndices = new int[TRIANGLE_INDEX_COUNT];

            for (int i = 0; i < NUM_SEGMENTS; i++)
            {
                int currentIdx = i + 1;
                int nextIdx = ((i + 1) % NUM_SEGMENTS) + 1;

                triangleIndices[i * 3] = 0;          // Center point
                triangleIndices[i * 3 + 1] = currentIdx;
                triangleIndices[i * 3 + 2] = nextIdx;
            }

            _interiorIBO.AppendIndexBuffer(triangleIndices);

        }



        public void UpdateSelectionCircle(Vector2 origin, Vector2 current, bool isDrawing)
        {
            if (!isDrawing)
            {
                _isDrawing = false;
                return;
            }

            // Assign to private circle points
            this._originPoint = origin;
            this._currentPoint = current;
            this._isDrawing = true;

            UpdateVertexBuffers();

        }


        private void UpdateVertexBuffers()
        {

            // Calculate circle properties
            Vector2 center = new Vector2(
                (_originPoint.X + _currentPoint.X) * 0.5f,
                (_originPoint.Y + _currentPoint.Y) * 0.5f
            );

            float circle_diameter = (float)Math.Sqrt(Math.Pow((_currentPoint.X - _originPoint.X), 2) + 
                Math.Pow((_currentPoint.Y - _originPoint.Y), 2));
            float circle_radius = circle_diameter / 2.0f;


            // Create vertices array (center + perimeter points)
            float[] vertices = new float[VERTEX_COUNT * FLOATS_PER_VERTEX];

            // Center point (index 0)
            vertices[0] = center.X;
            vertices[1] = center.Y;

            // Generate perimeter points
            for (int i = 0; i < NUM_SEGMENTS; i++)
            {
                double angle = 2 * Math.PI * i / NUM_SEGMENTS;
                float x = center.X + (circle_radius * (float)Math.Cos(angle));
                float y = center.Y + (circle_radius * (float)Math.Sin(angle));

                int vertexIndex = (i + 1) * FLOATS_PER_VERTEX;
                vertices[vertexIndex] = x;
                vertices[vertexIndex + 1] = y;
            }

            // Update both VBOs with the same vertices
            _boundaryVBO.updateVertexBuffer(vertices);
            _interiorVBO.updateVertexBuffer(vertices);

        }



        public void draw_selection_circle()
        {
            if (!_isDrawing)
                return;

            _shader.Bind();

            // Draw interior (filled) first so outline renders on top
            _interiorVAO.Bind();
            _interiorIBO.Bind();
            GL.DrawElements(PrimitiveType.Triangles, TRIANGLE_INDEX_COUNT,
                           DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Draw boundary (wireframe)
            _boundaryVAO.Bind();
            _boundaryIBO.Bind();
            GL.DrawElements(PrimitiveType.Lines, LINE_INDEX_COUNT,
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
