using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.global_static;

namespace varai2d_surface.Drawing_area
{
    public static class string_drawing_control
    {
        public static Font font_paintstring = new Font("Cambria Math", gvariables.str_font_size);
        public static int rotate_transform_fail_count = 0;

        
        public static void paint_string(Graphics gr0, String str, PointF vector, PointF location)
        {
            // Save the graphics state before transformation
            GraphicsState g_state = gr0.Save();

            //gr0.SmoothingMode = SmoothingMode.HighQuality;

            // Translate string location to origin
            gr0.TranslateTransform(location.X, location.Y);

            float vector_angle = (float)(Math.Atan2((-1.0f * vector.Y), vector.X) * (180 / Math.PI));

            // Position the string above the line
            if (vector_angle < -89.0f || vector_angle > 91.0f)
            {
                vector_angle += 180.0f;
            }

            try
            {
                // Rotate transform is failing for no reason !!!!!
                gr0.RotateTransform(-1.0f * vector_angle);
            }
            catch { rotate_transform_fail_count++; }

            //measure the string
            SizeF size_of_str = gr0.MeasureString(str, font_paintstring);
            PointF adjusted_location = new PointF(-1.0f * (size_of_str.Width * 0.5f), -1.0f * size_of_str.Height);

            gr0.DrawString(str, font_paintstring, gvariables.pen_string.Brush, adjusted_location);

            // Restore the graphics state to original
            gr0.Restore(g_state);
        }
    }
}
