// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace dxf_to_surface.src.opentk_control.opentk_buffer
{
    public class VertexBuffer : IDisposable
    {
        private int _rendererId;
        private int _capacity;  // Current capacity in bytes
        private int _size;      // Current used size in bytes
        private bool _disposed = false;
        private List<float> _localBuffer = new List<float>();  // Local copy of all vertex data

        public int Size => _size;
        public int Capacity => _capacity;


        public VertexBuffer(int vertexbuffer_count = 10)  // Note: Data count is the number of float count
        {
            // Main Constructor
            _rendererId = GL.GenBuffer();
            _capacity = vertexbuffer_count * sizeof(float);
            _size = 0;

            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, _capacity, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            UnBind();
        }

        public void AppendVertexBuffer(float[] vertexbuffer_data)
        {
            if (vertexbuffer_data == null || vertexbuffer_data.Length == 0)
                return;

            // Add to local buffer
            _localBuffer.AddRange(vertexbuffer_data);

            int vertexbuffer_size = _localBuffer.Count * sizeof(float);

            Bind();

            // Grow buffer if needed
            if (vertexbuffer_size > _capacity)
            {
                // Grow the GPU buffer to accommodate new data
                int newCapacity = Math.Max(_capacity * 2, vertexbuffer_size);

                // Reallocate GPU buffer with new size
                GL.BufferData(BufferTarget.ArrayBuffer, newCapacity, IntPtr.Zero, BufferUsageHint.DynamicDraw);

                _capacity = newCapacity;
            }

            // Upload ALL data to GPU (this is the key - upload everything, not just new data)
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertexbuffer_size, _localBuffer.ToArray());
            _size = _localBuffer.Count;

            UnBind();
        }


        public void updateVertexBuffer(float[] vertexbuffer_data)
        {
            if (vertexbuffer_data == null || vertexbuffer_data.Length == 0)
                return;

            int vertexbuffer_size = vertexbuffer_data.Length * sizeof(float);

            // Replace entire local buffer
            _localBuffer.Clear();
            _localBuffer.AddRange(vertexbuffer_data);

            // Important!! Call only in Dynamic Buffer case
            // Update the vertex data
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._rendererId);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertexbuffer_size, vertexbuffer_data);

        }


        public void ClearVertexBuffer()
        {
            _localBuffer.Clear();
            _size = 0;

            //// Optional: Clear GPU memory (not strictly necessary since we'll overwrite)
            //// But if you want to be thorough:
            //Bind();
            //byte[] zeros = new byte[_capacity];
            //GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, zeros.Length, zeros);
            //UnBind();
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, _rendererId);
        public void UnBind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteBuffer(_rendererId);
                _disposed = true;
            }
        }

    }
}
