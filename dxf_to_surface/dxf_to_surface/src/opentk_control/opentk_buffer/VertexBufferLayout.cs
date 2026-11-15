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

    public struct VertexBufferElement
    {
        public VertexAttribPointerType type { get; }
        public int count { get; }
        public bool normalized { get; }

        public VertexBufferElement(VertexAttribPointerType t_type, int t_count, bool t_normalized)
        {
            type = t_type;
            count = t_count;
            normalized = t_normalized;
        }

        public static int GetSizeOfType(VertexAttribPointerType type)
        {
            switch (type)
            {
                case VertexAttribPointerType.Float:
                    return sizeof(float);
                case VertexAttribPointerType.UnsignedInt:
                    return sizeof(uint);
                case VertexAttribPointerType.UnsignedByte:
                    return sizeof(byte);
                case VertexAttribPointerType.Byte:
                    return sizeof(sbyte);
                case VertexAttribPointerType.Int:
                    return sizeof(int);
                default:
                    throw new ArgumentException("Unsupported VertexAttribPointerType: " + type);
            }
        }
    }




    public class VertexBufferLayout
    {
        private readonly List<VertexBufferElement> m_elements = new List<VertexBufferElement>();
        private int m_stride;

        public IReadOnlyList<VertexBufferElement> GetElements => m_elements;
        public int GetStride => m_stride;

        public void AddFloat(int count)
        {
            Push(VertexAttribPointerType.Float, count, false);
        }

        public void AddUnsignedInt(int count)
        {
            Push(VertexAttribPointerType.UnsignedInt, count, false);
        }

        public void AddUnsignedByte(int count)
        {
            Push(VertexAttribPointerType.UnsignedByte, count, true);
        }

        public void AddInt(int count)
        {
            Push(VertexAttribPointerType.Int, count, false);
        }


        private void Push(VertexAttribPointerType type, int count, bool normalized)
        {
            var element = new VertexBufferElement(type, count, normalized);
            m_elements.Add(element);
            m_stride += count * VertexBufferElement.GetSizeOfType(type);
        }



    }
}
