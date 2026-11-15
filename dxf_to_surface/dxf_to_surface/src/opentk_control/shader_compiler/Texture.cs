using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;





namespace dxf_to_surface.src.opentk_control.shader_compiler
{
    public class Texture : IDisposable
    {
        private int texture_id;
       //  private string texture_name;
        private int texture_width;
        private int texture_height;
        private int texture_bpp;
        private bool disposed = false;

        //public int GetWidth() => texture_width;
        //public int GetHeight() => texture_height;
        //public int GetTextureId() => texture_id;

        public Texture()
        {
            texture_id = 0;
            // texture_name = "";
            texture_width = 0;
            texture_height = 0;
            texture_bpp = 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (texture_id != 0)
                {
                    GL.DeleteTextures(1, ref texture_id);
                    texture_id = 0;
                }
                disposed = true;
            }
        }

        public void LoadTexture(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentException("Bitmap cannot be null");
            }

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                texture_id = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture_id);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                              bitmap.Width, bitmap.Height, 0,
                              OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte,
                              data.Scan0);

                GL.BindTexture(TextureTarget.Texture2D, 0);

                texture_width = bitmap.Width;
                texture_height = bitmap.Height;
                texture_bpp = 32;
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        public void Bind(uint slot = 0)
        {
            // Activate texture slot
            GL.ActiveTexture(TextureUnit.Texture0 + (int)slot);

            // Bind the texture
            GL.BindTexture(TextureTarget.Texture2D, texture_id);
        }

        public void UnBind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }



    }
}
