using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using dxf_to_surface.src.global_variables;

namespace dxf_to_surface.src.opentk_control.opentk_buffer
{
    public class Character
    {
        public Vector2 Size;        // Glyph width & height
        public Vector2 Bearing;     // Offset from baseline to left/top
        public int Advance;         // Horizontal advance (in 1/64 pixels)
        public Vector2 TopLeft;     // Top-left texture coordinate
        public Vector2 BottomRight; // Bottom-right texture coordinate
    }


    public class GDIFontAtlas : IDisposable
    {
        public int TextureID { get; private set; } = 0;
        public int TextureWidth { get; private set; } = 0;
        public int TextureHeight { get; private set; } = 0;
        public Dictionary<char, Character> Glyphs { get; private set; } = new Dictionary<char, Character>();

        public void CreateAtlas(string fontFamilyName)
        {

            // Background color
            Color backgroundColor = Color.Transparent;

            // Define the characters we need (printable ASCII 32-126)
            List<char> chars = new List<char>();
            for (char c = (char)0; c <= (char)126; c++)
            {
                chars.Add(c);
            }

            const int font_size = 64;

            // First pass: Measure all characters to determine atlas size
            using (Font font = new Font(fontFamilyName, font_size, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Bitmap tempBmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                // Calculate individual character sizes and total atlas dimensions
                List<CharMetrics> metrics = new List<CharMetrics>();
                int maxHeight = 0;
                int totalWidth = 0;

                foreach (char c in chars)
                {
                    string text = c.ToString();
                    SizeF size = g.MeasureString(text, font);

                    CharMetrics metric = new CharMetrics
                    {
                        Char = c,
                        Width = (int)Math.Ceiling(size.Width),
                        Height = (int)Math.Ceiling(size.Height)
                    };

                    metrics.Add(metric);
                    totalWidth += metric.Width;
                    maxHeight = Math.Max(maxHeight, metric.Height);
                }

                // Create atlas with some padding between characters
                int padding = 2;
                TextureWidth = totalWidth + (chars.Count * padding);
                TextureHeight = maxHeight + (padding * 2);

                // Create the atlas bitmap
                using (Bitmap atlas = new Bitmap(TextureWidth, TextureHeight))
                using (Graphics gAtlas = Graphics.FromImage(atlas))
                {
                    gAtlas.Clear(backgroundColor);

                    int xOffset = padding;
                    int yOffset = padding;

                    foreach (var metric in metrics)
                    {
                        // Render the character to the atlas
                        using (Brush textBrush = new SolidBrush(Color.White))
                        {
                            gAtlas.DrawString(metric.Char.ToString(), font, textBrush, xOffset, yOffset);
                        }

                        // Calculate texture coordinates
                        float u1 = (float)xOffset / TextureWidth;
                        float v1 = (float)yOffset / TextureHeight;
                        float u2 = (float)(xOffset + metric.Width) / TextureWidth;
                        float v2 = (float)(yOffset + metric.Height) / TextureHeight;

                        // Create character info (you'll need to adjust bearing and advance)
                        Character ch = new Character
                        {
                            Size = new Vector2(metric.Width, metric.Height),
                            Bearing = new Vector2(0, -metric.Height / 2), // Approximate
                            Advance = metric.Width,
                            TopLeft = new Vector2(u1, v1),
                            BottomRight = new Vector2(u2, v2)
                        };

                        Glyphs[metric.Char] = ch;

                        xOffset += metric.Width + padding;
                    }

                    // Upload to OpenGL
                    TextureID = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, TextureID);

                    // Convert bitmap to byte array
                    byte[] pixels = BitmapToBytes(atlas);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                                  TextureWidth, TextureHeight, 0,
                                  OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
            }
        }

        private byte[] BitmapToBytes(Bitmap bitmap)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] bytes = new byte[data.Height * data.Stride];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(data);

            return bytes;
        }

        public void Dispose()
        {
            if (TextureID != 0)
            {
                GL.DeleteTexture(TextureID);
                TextureID = 0;
            }
        }

        private class CharMetrics
        {
            public char Char;
            public int Width;
            public int Height;
        }


    }
}
