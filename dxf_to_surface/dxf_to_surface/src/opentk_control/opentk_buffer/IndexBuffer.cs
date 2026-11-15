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
    public class IndexBuffer : IDisposable
    {

        private int _rendererId;
        private int _capacity;  // Capacity in indices
        private int _size;     // Current size of indices in bytes
        private bool _disposed;
        private int _bufferCount;
        private List<int> _localBuffer = new List<int>();  // Local copy of all index data

        public int Size => _size;
        public int Capacity => _capacity;

        public int BufferCount => _bufferCount;


        public IndexBuffer(int indexbuffer_count = 10)
        {
            _rendererId = GL.GenBuffer();
            _capacity = indexbuffer_count * sizeof(uint);
            _size = 0;
            _bufferCount = 0;

            Bind();
            GL.BufferData(BufferTarget.ElementArrayBuffer, _capacity ,
                         IntPtr.Zero, BufferUsageHint.DynamicDraw);
            UnBind();
        }


        public void AppendIndexBuffer(int[] indexbuffer_indices)
        {
            int indexbuffer_size = indexbuffer_indices.Length * sizeof(uint);

            if (_size + indexbuffer_size > _capacity)
            {
                Grow(Math.Max(_capacity * 2, _size + indexbuffer_size));
            }

            Bind();


            // Append at current size position
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)_size,
                            indexbuffer_size, indexbuffer_indices);

            _bufferCount += indexbuffer_indices.Length;
            _size += indexbuffer_size;
            UnBind();
        }


        public void UpdateIndexBuffer(int[] indexbuffer_data)
        {
            int indexbuffer_size = indexbuffer_data.Length * sizeof(uint);

            // Important!! Call only in Dynamic Buffer case
            // Update the index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._rendererId);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, indexbuffer_size, indexbuffer_data);

        }



        private void Grow(int newCapacity)
        {
            int newBufferId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, newBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, newCapacity,
                         IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Copy existing data
            GL.BindBuffer(BufferTarget.CopyReadBuffer, _rendererId);
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, newBufferId);
            GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer,
                                IntPtr.Zero, IntPtr.Zero, _size);

            GL.DeleteBuffer(_rendererId);
            _rendererId = newBufferId;
            _capacity = newCapacity;
        }

        public void ClearIndexBuffer()
        {
            _size = 0;
            _bufferCount = 0;
            Bind();
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero,
                            _capacity, IntPtr.Zero);
            UnBind();
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, _rendererId);
        public void UnBind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

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
