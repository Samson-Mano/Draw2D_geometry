using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace varai2d_surface.global_static
{
    public static class gvariables
    {
        // Epsilon value for geometry space
       public const double epsilon_g = 0.001;

        // Painting area details
        public static SizeF mainpic_size;
        public static PointF mainpic_center;
        public static double zoom_factor =1.01f;
        public static double scale_factor=1.0f;
        public static double scale_margin = 0.9f;

        // User options variables
        public static Color color_mainpic = Color.White;
        public static Color color_txtforecolor = Color.DarkMagenta;
        public static Color color_memberclr = Color.BlueViolet;
        public static Color color_pointsclr = Color.DarkRed;
        public static Color color_snaplineclr = Color.DarkOrange;
        public static Color color_stringforeclr = Color.DarkSeaGreen;
        
        public static int linewidth_curves = 3;
        public static int radius_points = 3;
        public static int linewidth_snapline = 1;

        public static int Trans_PV = 0;
        public static Pen pen_curves = new Pen(Color.FromArgb(255 - Trans_PV, color_memberclr), linewidth_curves);
        public static Pen pen_selected_curves = new Pen(Color.FromArgb(50, 0, 200, 0), linewidth_curves + 3);
        public static Pen pen_snapline = new Pen(Color.FromArgb(180, color_snaplineclr), linewidth_snapline);
        public static Pen pen_points = new Pen(Color.FromArgb(225 - Trans_PV, color_pointsclr), 2);
        public static Pen pen_string = new Pen(Color.FromArgb(255 - Trans_PV, color_stringforeclr), 3);
        
        public static int bezier_n_count = 4;

        public static string pt_coord_pres = "F3";
        public static string ln_length_pres = "F3";
        public static int str_font_size = 10;

        // snap option control
        public static bool Is_hvsnap = true;
        public static double hvsnap_intensity = 2.0;

        public static bool Is_xysnap = true;
        public static double xysnap_intensity = 4.0;

        // Paint string options
        public static bool Is_paint_pt = true;
        public static bool Is_paint_ptid = true;
        public static bool Is_paint_ptcoord = false;
        public static bool Is_paint_memberid = true;
        public static bool Is_paint_memberlength = true;
        public static bool Is_paint_surfaceid = true;

        // Temporary mouse point
        public static PointF cursor_on_clickpt;
        public static PointF cursor_on_movept;

        // Flag variables to control operation
        public static bool Is_duplicate = false;
        public static bool Is_panflg = false;
        public static bool Is_cntrldown = false;
        public static bool Is_shiftdown = false;
        public static bool Is_selectflg = false;
        public static bool Is_txtboxliveflg = false;
        public static bool Is_txtboxfocusflg = false;
        public static bool Is_surface_frm_open = false;
   }
}
