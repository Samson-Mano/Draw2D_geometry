using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;



namespace dxf_to_surface.src.opentk_control.opentk_buffer
{
    public class VertexArray : IDisposable
    {
        private int _m_renderer_id;
        private bool _disposed = false;

        public VertexArray()
        {
            // Main Constructor: generates a unique vertex array object ID.
            this._m_renderer_id = GL.GenVertexArray();

        }

        public void Add_vertexBuffer(VertexBuffer vb, VertexBufferLayout layout)
        {
            // Add and Bind a vertex buffer to an appropriate layout
            // Vertex Buffer layout  contains the information about co-ordinates, normals, color etc

            // Bind the vertex array object.
            Bind();

            // Bind the vertex buffer object.
            vb.Bind();

            // Set up the layout here
            IReadOnlyList<VertexBufferElement> elements = layout.GetElements;
            int offset = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                VertexBufferElement element = elements[i];

                // Enable the vertex attribute array for the specified buffer index.
                GL.EnableVertexAttribArray(i);

                // Set up the vertex attribute pointer for the specified buffer index.
                // This specifies how to interpret the vertex data in the vertex buffer object.
                GL.VertexAttribPointer(i, element.count, element.type, element.normalized, layout.GetStride, offset);


                // Offset is the previous layout count (most likely the stride will remain the same)
                offset = offset + (element.count * VertexBufferElement.GetSizeOfType(element.type));
            }

            // Unbind the vertex buffer object.
            vb.UnBind();

            // Unbind the vertex array object.
            UnBind();

        }

        public void Bind()
        {
            // Binds the vertex array object for use with subsequent OpenGL calls.
            GL.BindVertexArray(this._m_renderer_id);

        }

        public void UnBind()
        {
            // Unbinds the currently bound vertex array object.
            GL.BindVertexArray(0);

        }


        public void Dispose()
        {
            if (!_disposed)
            {
                // Delete this buffer (acts like a  destructor)
                GL.DeleteVertexArray(this._m_renderer_id);
                _disposed = true;
            }
        }


    }
}
