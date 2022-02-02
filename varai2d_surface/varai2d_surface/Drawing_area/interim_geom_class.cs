using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.global_static;
using varai2d_surface.Geometry_class;
using varai2d_surface.Geometry_class.geometry_store;

namespace varai2d_surface.Drawing_area
{
    [Serializable]
    public class interim_geom_class
    {
        bool _Is_addline = false;
        bool _Is_addarc1 = false;
        bool _Is_addbezier = false;

        bool _Is_selection = false;
        bool _Is_selected = false;
        bool _Is_translate = false;
        bool _Is_rotate = false;
        bool _Is_mirror = false;

        HashSet<points_store> _selected_points = new HashSet<points_store>();
        HashSet<lines_store> _selected_lines = new HashSet<lines_store>();
        HashSet<arcs_store> _selected_arcs = new HashSet<arcs_store>();
        HashSet<beziers_store> _selected_beziers = new HashSet<beziers_store>();

        // Polylines are used in surface creation API
      //  HashSet<surface_store> _selected_surfaces = new HashSet<surface_store>();

        List<points_store> _click_pts = new List<points_store>();

        private double _current_length = 0.0;
        private double _current_angle = 0.0;

        PointF _cursor_pt = new PointF(0, 0);
        int click_count;

        public HashSet<points_store> selected_points { get { return this._selected_points; } }

        public HashSet<lines_store> selected_lines { get { return this._selected_lines; } }

        public HashSet<arcs_store> selected_arcs { get { return this._selected_arcs; } }

        public HashSet<beziers_store> selected_beziers { get { return this._selected_beziers; } }

        //public HashSet<surface_store> selected_surfaces { get { return this._selected_surfaces; } }

        public List<points_store> click_pts { get { return this._click_pts; } }

        public string str_txt_box
        {
            get
            {
                if (_Is_addline == true || _Is_addbezier == true || _Is_translate == true || (_Is_addarc1 == true && click_count < 2))
                {
                    return this._current_length.ToString(gvariables.ln_length_pres) + ", " +
                            this._current_angle.ToString("F1");
                }
                else if (_Is_addarc1 == true && click_count == 2)
                {
                    return this._current_length.ToString(gvariables.ln_length_pres);
                }
                else if (_Is_mirror == true || _Is_rotate == true)
                {
                    return this._current_angle.ToString("F1");
                }
                return "";
            }
        }

        public bool Is_addline { get { return this._Is_addline; } }

        public bool Is_addarc1 { get { return this._Is_addarc1; } }

        public bool Is_addbezier { get { return this._Is_addbezier; } }

        public bool Is_translate { get { return this._Is_translate; } }

        public bool Is_rotate { get { return this._Is_rotate; } }

        public bool Is_mirror { get { return this._Is_mirror; } }

        public bool Is_selection { get { return this._Is_selection; } set { this._Is_selection = value; } }

        public bool Is_selected { get { return this._Is_selected; } }

        public PointF cursor_pt
        {
            // writeonly property to send the X,Y co-ordinate in canvas
            set { this._cursor_pt = value; }
        }

        public interim_geom_class()
        {
            // Empty constructor
        }

        public void paint_background_axis(Graphics gr0)
        {
            gr0.FillRectangle(new Pen(gvariables.color_mainpic, 1.0f).Brush, 0, 0, gvariables.mainpic_size.Width, gvariables.mainpic_size.Height);

            // Paint Axis X - line
            using (GraphicsPath end_cap_path = new GraphicsPath())
            {
                end_cap_path.AddLine(0, 0, -2, -2);
                end_cap_path.AddLine(0, 0, 2, -2);

                using (CustomLineCap end_cap = new CustomLineCap(null, end_cap_path))
                {
                    PointF o_pt = new PointF(50, gvariables.mainpic_size.Height - 50);
                    PointF x_pt = new PointF(o_pt.X + 60, o_pt.Y);
                    PointF y_pt = new PointF(o_pt.X, o_pt.Y - 60);

                    using (Pen axis_pen = new Pen(Color.Black, 5))
                    {
                        axis_pen.CustomEndCap = end_cap;
                        // Red x_line
                        axis_pen.Color = Color.Red;
                        gr0.DrawLine(axis_pen, o_pt, x_pt);
                        gr0.DrawString("X", new Font("Verdana", 16), axis_pen.Brush, x_pt);

                        // Blue y_line
                        axis_pen.Color = Color.Blue;
                        gr0.DrawLine(axis_pen, o_pt, y_pt);
                        gr0.DrawString("Y", new Font("Verdana", 16), axis_pen.Brush, new PointF(y_pt.X, y_pt.Y - 30));

                        // Green z line (just an ellipse)
                        axis_pen.Color = Color.Green;
                        gr0.FillEllipse(axis_pen.Brush, gfunctions.get_ellipse_rectangle(o_pt, 6));
                    }
                }
            }
        }

        public bool paint_interim_objects(Graphics gr0)
        {
            // Paint Graphics
            // Add line in progress
            if (this._Is_addline == true)
            {
                return paint_interim_add_line(gr0);
            }

            // Add Arc1 in progress
            if (this._Is_addarc1 == true)
            {

                return paint_interim_add_arc(gr0);
            }

            // Add bezier in progress
            if (this._Is_addbezier == true)
            {
                return paint_interim_add_bezier(gr0);
            }

            // Paint the selection rectangle
            if (this._Is_selection == true)
            {
                // Selection rectange border points
                Pen border_pen = new Pen(Color.IndianRed, 2.0f / (float)gvariables.zoom_factor);

                // Select Rectangle
                float min_x, min_y, max_x, max_y;
                float rect_width, rect_height;
                min_x = Math.Min(gvariables.cursor_on_clickpt.X, gvariables.cursor_on_movept.X);
                min_y = Math.Min(gvariables.cursor_on_clickpt.Y, gvariables.cursor_on_movept.Y);
                max_x = Math.Max(gvariables.cursor_on_clickpt.X, gvariables.cursor_on_movept.X);
                max_y = Math.Max(gvariables.cursor_on_clickpt.Y, gvariables.cursor_on_movept.Y);

                // rectangle size
                rect_width = max_x - min_x;
                rect_height = max_y - min_y;

                // Selection rectangle
                gr0.DrawRectangle(border_pen, min_x, min_y, rect_width, rect_height);
                gr0.FillRectangle(new Pen(Color.FromArgb(50, Color.Crimson)).Brush, min_x, min_y, rect_width, rect_height);
            }

            // Highlight the selected lines
            if (this._Is_selected == true)
            {
                // Paint the selected lines
                foreach (lines_store selected_line in this.selected_lines)
                {
                    selected_line.paint_lines(gr0);
                    selected_line.paint_selected_lines(gr0);

                }
                // Paint the selected arcs
                foreach (arcs_store selected_arc in this.selected_arcs)
                {
                    selected_arc.paint_arcs(gr0);
                    selected_arc.paint_selected_arcs(gr0);
                }

                // Paint the selected beziers
                foreach (beziers_store selected_bz in this.selected_beziers)
                {
                    selected_bz.paint_beziers(gr0);
                    selected_bz.paint_beziers_selected(gr0);
                }

                // Paint the nodes connected to selected lines
                foreach (points_store selected_points in this.selected_points)
                {
                    selected_points.paint_point(gr0);
                    selected_points.paint_selected_point(gr0);
                }

                if (this._Is_translate == true)
                {
                    // Is translation in progress
                    paint_translate_UI_lines(gr0);

                    // Find the translation
                    double transl_x = (double)(this._cursor_pt.X - this._click_pts[click_count - 1].get_point.X);
                    double transl_y = (double)(this._cursor_pt.Y - this._click_pts[click_count - 1].get_point.Y);

                    // Paint all temp_pts
                    foreach (points_store pts in this._click_pts)
                    {
                        pts.paint_modify_base_pt(gr0);
                    }
                    // Paint lines at translation
                    foreach (lines_store selected_line in this._selected_lines)
                    {
                        selected_line.paint_temp_line_during_translation(gr0, transl_x, transl_y);
                    }
                    // Paint arcs at translation
                    foreach (arcs_store selected_arc in this._selected_arcs)
                    {
                        selected_arc.paint_temp_arc_during_translation(gr0, transl_x, transl_y);
                    }
                    // Paint beziers at translation
                    foreach (beziers_store selected_beziers in this._selected_beziers)
                    {
                        selected_beziers.paint_temp_bezier_during_translation(gr0, transl_x, transl_y);
                    }

                    // Paint points at translation
                    foreach (points_store selected_pt in this._selected_points)
                    {
                        selected_pt.paint_temp_point_during_translation(gr0, transl_x, transl_y);
                    }
                }

                if (this._Is_rotate == true)
                {
                    // Is Rotation in progress
                    paint_rotate_UI_lines(gr0);

                    // Find the rotation
                    PointF rotation_pt = this._click_pts[click_count - 1].get_point;
                    double rot_angle_rad = -1.0 * gfunctions.get_angle_rad(this._click_pts[click_count - 1].get_point, this._cursor_pt);

                    // Paint all temp_pts
                    foreach (points_store pts in this._click_pts)
                    {
                        pts.paint_modify_base_pt(gr0);
                    }
                    // Paint all lines
                    foreach (lines_store selected_line in this._selected_lines)
                    {
                        selected_line.paint_temp_line_during_rotation(gr0, rotation_pt, rot_angle_rad);
                    }
                    // Paint all arcs
                    foreach (arcs_store selected_arc in this._selected_arcs)
                    {
                        selected_arc.paint_temp_arc_during_rotation(gr0, rotation_pt, rot_angle_rad);
                    }
                    // Paint all bezier
                    foreach (beziers_store selected_beziers in this._selected_beziers)
                    {
                        selected_beziers.paint_temp_bezier_during_rotation(gr0, rotation_pt, rot_angle_rad);
                    }

                    foreach (points_store selected_pt in this._selected_points)
                    {
                        selected_pt.paint_temp_point_during_rotation(gr0, rotation_pt, rot_angle_rad);
                    }

                }

                if (this._Is_mirror == true)
                {
                    // Is Mirror in progress
                    paint_mirror_UI_lines(gr0);

                    // Find the mirror
                    PointF rotation_pt = this._click_pts[click_count - 1].get_point;
                    double rot_angle_rad = -1.0 * gfunctions.get_angle_rad(this._click_pts[click_count - 1].get_point, this._cursor_pt);

                    // Paint all temp_pts
                    foreach (points_store pts in this._click_pts)
                    {
                        pts.paint_modify_base_pt(gr0);
                    }
                    // Paint all lines
                    foreach (lines_store selected_line in this._selected_lines)
                    {
                        selected_line.paint_temp_line_during_mirror(gr0, rotation_pt, rot_angle_rad);
                    }
                    // Paint all arcs
                    foreach (arcs_store selected_arc in this._selected_arcs)
                    {
                        selected_arc.paint_temp_arc_during_mirror(gr0, rotation_pt, rot_angle_rad);
                    }
                    // Paint all bezier
                    foreach (beziers_store selected_beziers in this._selected_beziers)
                    {
                        selected_beziers.paint_temp_bezier_during_mirror(gr0, rotation_pt, rot_angle_rad);
                    }

                    foreach (points_store selected_pt in this._selected_points)
                    {
                        selected_pt.paint_temp_point_during_mirror(gr0, rotation_pt, rot_angle_rad);
                    }
                }
                return true;
            }

           // // Surface addition in progress
           //if(gvariables.Is_surface_frm_open == true)
           // {
           //     foreach (surface_store surf in selected_surfaces)
           //     {
           //         surf.paint_temp_surface(gr0);
           //     }
           // }

            return false;
        }

        public bool paint_interim_add_line(Graphics gr0)
        {
            // Draw line
            gr0.DrawLine(gvariables.pen_curves, this._click_pts[click_count - 1].get_point, this._cursor_pt);

            // Draw end point (current cursor point)
            gr0.FillEllipse(gvariables.pen_points.Brush, gfunctions.get_ellipse_rectangle(this._cursor_pt, gvariables.radius_points));

            foreach (points_store pt in this._click_pts)
            {
                // Paint the points so far added (which is only one point)
                pt.paint_point(gr0);
            }

            // Draw Length string
            PointF dir_vector = new PointF(this._cursor_pt.X - this._click_pts[click_count - 1].get_point.X, this._cursor_pt.Y - this._click_pts[click_count - 1].get_point.Y);
            this._current_length = gfunctions.get_length(this._click_pts[click_count - 1].get_point, this._cursor_pt)/gvariables.scale_factor;
            this._current_angle = gfunctions.get_angle_rad(this._click_pts[click_count - 1].get_point, this._cursor_pt) * (180.0 / Math.PI);

            string_drawing_control.paint_string(gr0, this._current_length.ToString(gvariables.ln_length_pres), dir_vector, gfunctions.get_midpoint(this._click_pts[click_count - 1].get_point, this._cursor_pt));
            return true;
        }

        public bool paint_interim_add_arc(Graphics gr0)
        {
            if (click_count == 1)
            {
                // After First click
                // Draw chord line
                using (Pen pen_transl_line = new Pen(gvariables.pen_snapline.Color, 3))
                {
                    pen_transl_line.DashStyle = DashStyle.Dot;
                    gr0.DrawLine(pen_transl_line, this._click_pts[click_count - 1].get_point, this._cursor_pt);
                }

                // Draw end point (current cursor point)
                gr0.FillEllipse(gvariables.pen_points.Brush, gfunctions.get_ellipse_rectangle(this._cursor_pt, gvariables.radius_points));

                foreach (points_store pt in this._click_pts)
                {
                    // Paint the points so far added (which is only one point)
                    pt.paint_point(gr0);
                }

                // Draw Length string
                PointF dir_vector = new PointF(this._cursor_pt.X - this._click_pts[click_count - 1].get_point.X, this._cursor_pt.Y - this._click_pts[click_count - 1].get_point.Y);
                this._current_length = gfunctions.get_length(this._click_pts[click_count - 1].get_point, this._cursor_pt) / gvariables.scale_factor;
                this._current_angle = gfunctions.get_angle_rad(this._click_pts[click_count - 1].get_point, this._cursor_pt) * (180.0 / Math.PI);

                string_drawing_control.paint_string(gr0, this._current_length.ToString(gvariables.ln_length_pres), dir_vector, gfunctions.get_midpoint(this._click_pts[click_count - 1].get_point, this._cursor_pt));

            }
            else
            {
                // After second click
                // Draw chord line
                using (Pen pen_transl_line = new Pen(gvariables.pen_snapline.Color, 3))
                {
                    pen_transl_line.DashStyle = DashStyle.Dot;
                    gr0.DrawLine(pen_transl_line, this._click_pts[click_count - 2].get_point, this._click_pts[click_count - 1].get_point);
                }

                double chord_length = Math.Sqrt(Math.Pow(this._click_pts[click_count - 1].get_point.X - this._click_pts[click_count - 2].get_point.X, 2) +
                    Math.Pow(this._click_pts[click_count - 1].get_point.Y - this._click_pts[click_count - 2].get_point.Y, 2));

                // Basic checks before proceeding with the circle
                bool is_collinear = false;
                PointF norm_intersection_pt = gfunctions.get_normal_intersecton_pt(this._click_pts[click_count - 2].get_point, this._click_pts[click_count - 1].get_point, this._cursor_pt, ref is_collinear);

                // Check the normal distance
                double norm_distance = gfunctions.get_length(norm_intersection_pt, this._cursor_pt);

                // Check for degenerate case 1
                if (is_collinear == true || norm_distance < chord_length * 0.01)
                    return true;

                // Get the center of the circle
                PointF circle_center_pt = gfunctions.get_circle_center_pt(this._click_pts[click_count - 2].get_point,
                   this._click_pts[click_count - 1].get_point, this._cursor_pt);
                double circle_radius = gfunctions.get_length(circle_center_pt, this._cursor_pt);

                // Check for degenerate case 2
                if (circle_radius > 100 * chord_length)
                    return true;

                // Start and sweep angle
                double start_angle, sweep_angle;
                Tuple<double, double> arc_angles = gfunctions.get_arc_angles(this._click_pts[click_count - 2].get_point,
                    this._click_pts[click_count - 1].get_point, this._cursor_pt, circle_center_pt);

                start_angle = arc_angles.Item1;
                sweep_angle = arc_angles.Item2;

                // Draw Radius line
                using (GraphicsPath end_cap_path = new GraphicsPath())
                {
                    end_cap_path.AddLine(0, 0, -2, -2);
                    end_cap_path.AddLine(0, 0, 2, -2);

                    using (CustomLineCap end_cap = new CustomLineCap(null, end_cap_path))
                    {
                        using (Pen pen_transl_line = new Pen(gvariables.pen_snapline.Color, 3))
                        {
                            pen_transl_line.CustomEndCap = end_cap;
                            pen_transl_line.DashStyle = DashStyle.Dot;
                            gr0.DrawLine(pen_transl_line, circle_center_pt, this._cursor_pt);

                            // Draw Length string
                            PointF dir_vector = new PointF(this._cursor_pt.X - circle_center_pt.X, this._cursor_pt.Y - circle_center_pt.Y);
                            this._current_length = gfunctions.get_length(circle_center_pt, this._cursor_pt) / gvariables.scale_factor;
                            this._current_angle = gfunctions.get_angle_rad(circle_center_pt, this._cursor_pt) * (180.0 / Math.PI);

                            string_drawing_control.paint_string(gr0, this._current_length.ToString(gvariables.ln_length_pres), dir_vector, gfunctions.get_midpoint(circle_center_pt, this._cursor_pt));
                        }
                    }
                }

                // create circle bounding box
                RectangleF bounding_rect = new RectangleF((float)(circle_center_pt.X - circle_radius), (float)(circle_center_pt.Y - circle_radius), (float)(circle_radius * 2.0), (float)(circle_radius * 2.0));
                // Draw the center point (control point)
                PointF[] tri_pts = gfunctions.get_equilateral_triangle(gvariables.radius_points * 4, circle_center_pt);
                gr0.FillPolygon(gvariables.pen_points.Brush, tri_pts);

                // Draw the Arc
                gr0.DrawArc(gvariables.pen_curves, bounding_rect, (float)(start_angle), (float)(sweep_angle));

                // Draw end point (current cursor point)
                gr0.FillEllipse(gvariables.pen_points.Brush, gfunctions.get_ellipse_rectangle(this._cursor_pt, gvariables.radius_points));


            }
            return true;
        }

        public bool paint_interim_add_bezier(Graphics gr0)
        {
            // Draw lines for control points already added
            int i = 0;
            PointF dir_vector;
            double l_length;
            List<PointF> bezier_pts = new List<PointF>();

            bezier_pts.Add(this._click_pts[0].get_point);// Add the zeroth point (first click point)

            for (i = 0; i < (click_count - 1); i++)
            {
                gr0.DrawLine(gvariables.pen_snapline, this._click_pts[i].get_point, this._click_pts[i + 1].get_point);

                // Draw Length string
                dir_vector = new PointF(this._click_pts[i + 1].get_point.X - this._click_pts[i].get_point.X,
                    this._click_pts[i + 1].get_point.Y - this._click_pts[i].get_point.Y);
                l_length = gfunctions.get_length(this._click_pts[i].get_point, this._click_pts[i + 1].get_point) / gvariables.scale_factor;
                string_drawing_control.paint_string(gr0, l_length.ToString(gvariables.ln_length_pres), dir_vector, gfunctions.get_midpoint(this._click_pts[i].get_point, this._click_pts[i + 1].get_point));

                bezier_pts.Add(this._click_pts[i + 1].get_point);
            }

            // Draw the current line for control point
            gr0.DrawLine(gvariables.pen_snapline, this._click_pts[click_count - 1].get_point, this._cursor_pt);

            // Draw Length string
            dir_vector = new PointF(this._cursor_pt.X - this._click_pts[click_count - 1].get_point.X, this._cursor_pt.Y - this._click_pts[click_count - 1].get_point.Y);
            this._current_length = gfunctions.get_length(this._click_pts[click_count - 1].get_point, this._cursor_pt) / gvariables.scale_factor;
            this._current_angle = gfunctions.get_angle_rad(this._click_pts[click_count - 1].get_point, this._cursor_pt) * (180.0 / Math.PI);

            string_drawing_control.paint_string(gr0, this._current_length.ToString(gvariables.ln_length_pres), dir_vector, gfunctions.get_midpoint(this._click_pts[click_count - 1].get_point, this._cursor_pt));

            // Draw bezier line
            bezier_pts.Add(this._cursor_pt);// Add the n point (current mouse location)


            if (click_count > 1)
            {
                gr0.DrawCurve(gvariables.pen_curves, gfunctions.get_bezier_polynomial_pts(bezier_pts,10));
            }

            return true;
        }

        public void paint_translate_UI_lines(Graphics gr0)
        {
            // Draw translation line
            using (GraphicsPath end_cap_path = new GraphicsPath())
            {
                end_cap_path.AddLine(0, 0, -2, -2);
                end_cap_path.AddLine(0, 0, 2, -2);

                using (CustomLineCap end_cap = new CustomLineCap(null, end_cap_path))
                {
                    using (Pen pen_transl_line = new Pen(gvariables.pen_snapline.Color, 3))
                    {
                        pen_transl_line.CustomEndCap = end_cap;
                        gr0.DrawLine(pen_transl_line, this._click_pts[click_count - 1].get_point, this._cursor_pt);

                        //Draw translation length
                        PointF dir_vector = new PointF(this._cursor_pt.X - this._click_pts[click_count - 1].get_point.X, this._cursor_pt.Y - this._click_pts[click_count - 1].get_point.Y);

                        this._current_length = gfunctions.get_length(this._click_pts[click_count - 1].get_point, this._cursor_pt) / gvariables.scale_factor;
                        this._current_angle = gfunctions.get_angle_rad(this._click_pts[click_count - 1].get_point, this._cursor_pt) * (180.0 / Math.PI);
                        string_drawing_control.paint_string(gr0, this._current_length.ToString(gvariables.ln_length_pres), dir_vector,
                            gfunctions.get_midpoint(this._click_pts[click_count - 1].get_point,
                            this._cursor_pt));
                    }
                }
            }
        }

        public void paint_rotate_UI_lines(Graphics gr0)
        {
            // Draw rotation line
            using (Pen pen_transl_line = new Pen(gvariables.pen_snapline.Color, 3))
            {
                // pen_transl_line.CustomEndCap = end_cap;
                double v_angle_rad = -1.0 * gfunctions.get_angle_rad(this._click_pts[click_count - 1].get_point, this._cursor_pt);
                double bound_length = Math.Sqrt(Math.Pow(gvariables.mainpic_size.Width, 2) + Math.Pow(gvariables.mainpic_size.Height, 2)) / gvariables.zoom_factor;

                PointF v_ept = new PointF((float)(this._click_pts[click_count - 1].get_point.X + (bound_length * Math.Cos(v_angle_rad))),
                    (float)(this._click_pts[click_count - 1].get_point.Y + (bound_length * Math.Sin(v_angle_rad))));
                gr0.DrawLine(pen_transl_line, this._click_pts[click_count - 1].get_point, v_ept);

                // covert radians to angle
                this._current_angle = v_angle_rad * (180 / Math.PI);
                gr0.DrawArc(pen_transl_line, (int)(this._click_pts[click_count - 1].get_point.X - 60), (int)(this._click_pts[click_count - 1].get_point.Y - 60), 120, 120, 0, (int)this._current_angle);

                pen_transl_line.DashStyle = DashStyle.Dot;
                gr0.DrawLine(pen_transl_line, this._click_pts[click_count - 1].get_point, new PointF((float)(this._click_pts[click_count - 1].get_point.X + bound_length), this._click_pts[click_count - 1].get_point.Y));

                // Paint the angle as string
                PointF vector_str = gfunctions.get_vector(this._click_pts[click_count - 1].get_point, this._cursor_pt);
                PointF pt_str = new PointF((float)(this._click_pts[click_count - 1].get_point.X + (60 * Math.Cos(v_angle_rad))),
                    (float)(this._click_pts[click_count - 1].get_point.Y + (60 * Math.Sin(v_angle_rad))));

                this._current_angle = -1.0f * this._current_angle;
                string_drawing_control.paint_string(gr0, this._current_angle.ToString("F1"), vector_str, pt_str);

            }

        }

        public void paint_mirror_UI_lines(Graphics gr0)
        {
            // Draw mirror line
            using (Pen pen_transl_line = new Pen(gvariables.pen_snapline.Color, 3))
            {
                // pen_transl_line.CustomEndCap = end_cap;
                double v_angle_rad = -1.0 * gfunctions.get_angle_rad(this._click_pts[click_count - 1].get_point, this._cursor_pt);
                double bound_length = Math.Sqrt(Math.Pow(gvariables.mainpic_size.Width, 2) + Math.Pow(gvariables.mainpic_size.Height, 2)) / gvariables.zoom_factor;

                PointF v_ept2 = new PointF((float)(this._click_pts[click_count - 1].get_point.X + (bound_length * Math.Cos(v_angle_rad))),
                    (float)(this._click_pts[click_count - 1].get_point.Y + (bound_length * Math.Sin(v_angle_rad))));

                PointF v_ept1 = new PointF((float)(this._click_pts[click_count - 1].get_point.X + (bound_length * Math.Cos(v_angle_rad + Math.PI))),
                    (float)(this._click_pts[click_count - 1].get_point.Y + (bound_length * Math.Sin(v_angle_rad + Math.PI))));

                gr0.DrawLine(pen_transl_line, this._click_pts[click_count - 1].get_point, v_ept2);
                gr0.DrawLine(pen_transl_line, this._click_pts[click_count - 1].get_point, v_ept1);

                // covert radians to angle
                this._current_angle = v_angle_rad * (180 / Math.PI);
                gr0.DrawArc(pen_transl_line, (int)(this._click_pts[click_count - 1].get_point.X - 60), (int)(this._click_pts[click_count - 1].get_point.Y - 60), 120, 120, 0, (int)this._current_angle);

                pen_transl_line.DashStyle = DashStyle.Dot;
                gr0.DrawLine(pen_transl_line, this._click_pts[click_count - 1].get_point, new PointF((float)(this._click_pts[click_count - 1].get_point.X + bound_length), this._click_pts[click_count - 1].get_point.Y));

                // Paint the angle as string
                PointF vector_str = gfunctions.get_vector(this._click_pts[click_count - 1].get_point, this._cursor_pt);
                PointF pt_str = new PointF((float)(this._click_pts[click_count - 1].get_point.X + (60 * Math.Cos(v_angle_rad))),
                    (float)(this._click_pts[click_count - 1].get_point.Y + (60 * Math.Sin(v_angle_rad))));

                this._current_angle = -1.0f * this._current_angle;
                string_drawing_control.paint_string(gr0, ((float)this._current_angle).ToString("F1"), vector_str, pt_str);

                // Draw reflection line
                using (GraphicsPath end_cap_path = new GraphicsPath())
                {
                    end_cap_path.AddLine(0, 0, -2, -2);
                    end_cap_path.AddLine(0, 0, 2, -2);

                    using (CustomLineCap end_cap = new CustomLineCap(null, end_cap_path))
                    {
                        pen_transl_line.CustomEndCap = end_cap;

                        PointF refl_ept1 = new PointF((float)(this._cursor_pt.X + (40 * Math.Cos(v_angle_rad + (Math.PI * 0.5)))),
                                        (float)(this._cursor_pt.Y + (40 * Math.Sin(v_angle_rad + (Math.PI * 0.5)))));

                        PointF refl_ept2 = new PointF((float)(this._cursor_pt.X + (40 * Math.Cos(v_angle_rad - (Math.PI * 0.5)))),
                                        (float)(this._cursor_pt.Y + (40 * Math.Sin(v_angle_rad - (Math.PI * 0.5)))));

                        gr0.DrawLine(pen_transl_line, this._cursor_pt, refl_ept1);
                        gr0.DrawLine(pen_transl_line, this._cursor_pt, refl_ept2);
                    }
                }
            }
        }

        public Tuple<bool, int> update_click_pts(points_store snaped_pt)
        {
            // result as (0,1),(0 to 8)
            Tuple<bool, int> rslt = new Tuple<bool, int>(false, 0);

            // Add points
            click_count++;
            this._click_pts.Add(snaped_pt);


            switch (toolbarstate.get_toolchecked_state)
            {
                // Select the case based on the tool checked state
                case 0:
                    {
                        // Selection operation in progress
                        click_count = 0;
                        this._click_pts.Clear();
                        rslt = new Tuple<bool, int>(true, 0);
                        break;
                    }
                case 1:
                    {
                        // Add line is inprogress
                        if (click_count > 1)
                        {
                            // Second click and the line adding is complete
                            rslt = new Tuple<bool, int>(true, 1);
                            this._Is_addline = false;
                            return rslt;
                        }
                        else
                        {
                            // s_pt = snaped_pt;
                            this._Is_addline = true;
                        }
                        break;
                    }
                case 2:
                    {
                        // Add circle is inprogress
                        break;
                    }
                case 3:
                    {
                        // Add arc 1 (chord length & radius) is in progress
                        if (click_count > 2)
                        {
                            // Third click
                            // Get the center of the circle
                            PointF circle_center_pt = gfunctions.get_circle_center_pt(this._click_pts[click_count - 3].get_point,
                               this._click_pts[click_count - 2].get_point, this._click_pts[click_count - 1].get_point);

                            click_count++;
                            this._click_pts.Add(new points_store(-100, circle_center_pt.X, circle_center_pt.Y));

                            // Third click and the arc adding is complete
                            rslt = new Tuple<bool, int>(true, 3);
                            this._Is_addarc1 = false;
                            return rslt;
                        }
                        else if (click_count > 1)
                        {
                            // Second click
                            this._Is_addarc1 = true;
                        }
                        else
                        {
                            // First click
                            this._Is_addarc1 = true;
                        }
                        break;
                    }
                case 4:
                    {
                        // Add arc 2 (Radius and sector angle) is in progress
                        break;
                    }
                case 5:
                    {
                        // Add bezier is in progress
                        if (click_count > (gvariables.bezier_n_count - 1))
                        {
                            // Final click and the bezier adding is complete
                            rslt = new Tuple<bool, int>(true, 5);
                            this._Is_addbezier = false;
                            return rslt;
                        }
                        else if (click_count > 1)
                        {
                            // Second click
                            this._Is_addbezier = true;
                        }
                        else
                        {
                            // First click
                            this._Is_addbezier = true;
                        }
                        break;
                    }
                case 6:
                    {
                        // Modify -> Translate member in progress
                        if (this._Is_selected == true)
                        {
                            // Check whether members are selected before proceeding the translate transformation progress
                            if (click_count > 1)
                            {
                                    // Second click and the Translation is complete
                                rslt = new Tuple<bool, int>(true, 6);
                                this._Is_translate = false;
                                return rslt;
                            }
                            else
                            {
                                this._Is_translate = true;
                            }
                        }
                        break;
                    }
                case 7:
                    {
                        // Modify -> Rotate member in progress
                        if (this._Is_selected == true)
                        {
                            // Check whether members are selected before proceeding the rotate transformation progress
                            if (click_count > 1)
                            {
                                // Second click and the Translation is complete
                                rslt = new Tuple<bool, int>(true, 7);
                                this._Is_rotate = false;
                                return rslt;
                            }
                            else
                            {
                                this._Is_rotate = true;
                            }
                        }
                        break;
                    }
                case 8:
                    {
                        // Modify -> Mirror member in progress
                        if (this._Is_selected == true)
                        {
                            // Check whether members are selected before proceeding the mirror transformation progress
                            if (click_count > 1)
                            {
                                // Second click and the Translation is complete
                                rslt = new Tuple<bool, int>(true, 8);
                                this._Is_mirror = false;
                                return rslt;
                            }
                            else
                            {
                                this._Is_mirror = true;
                            }
                        }
                        break;
                    }
            }
            return rslt;
        }

        public void update_selection_list(RectangleF select_rectangle, geom_class geom, bool is_ShiftSelect)
        {
            HashSet<lines_store> save_selected_lines = new HashSet<lines_store>();
            HashSet<arcs_store> save_selected_arcs = new HashSet<arcs_store>();
            HashSet<beziers_store> save_selected_beziers = new HashSet<beziers_store>();

            if (is_ShiftSelect == true)
            {
                if (this.selected_lines.Count != 0)
                {
                    // Save the previously selected line
                    save_selected_lines.UnionWith(this._selected_lines);
                }

                if (this.selected_arcs.Count != 0)
                {
                    // Save the previously selected arcs
                    save_selected_arcs.UnionWith(this._selected_arcs);
                }

                if (this.selected_beziers.Count != 0)
                {
                    // Save the previously selected beziers
                    save_selected_beziers.UnionWith(this._selected_beziers);
                }
            }

            // clear the line list which are selected before
            this._selected_lines.Clear();
            foreach (lines_store line in geom.all_lines)
            {
                if (line.Is_selected(select_rectangle) == true)
                {
                    // Add to the list if the lines are selected
                    this._selected_lines.Add(line);
                }
            }

            // clear the arc list which are selected before
            this._selected_arcs.Clear();
            foreach (arcs_store arc in geom.all_arcs)
            {
                if (arc.Is_selected(select_rectangle) == true)
                {
                    // Add to the list if the arcs are selected
                    this._selected_arcs.Add(arc);
                }
            }

            // clear the beziers list which are selected before
            this._selected_beziers.Clear();
            foreach (beziers_store bz in geom.all_beziers)
            {
                if (bz.Is_selected(select_rectangle) == true)
                {
                    // Add to the list if the arcs are selected
                    this._selected_beziers.Add(bz);
                }
            }


            if (is_ShiftSelect == true)
            {
                // Lines
                if (save_selected_lines.Count != 0)
                {
                    // Find except list with already selected lines
                    // Takes the data from the first result set, but not in the second result set
                    IEnumerable<lines_store> except_first_set = this._selected_lines.Except(save_selected_lines, new LinesComparer());
                    HashSet<lines_store> first_set_h = except_first_set.ToHashSet();

                    // Takes the data from the second result set, but not in the first result set
                    IEnumerable<lines_store> except_second_set = save_selected_lines.Except(this._selected_lines, new LinesComparer());
                    HashSet<lines_store> second_set_h = except_second_set.ToHashSet();

                    // Combine as new data result
                    IEnumerable<lines_store> except_union_set = first_set_h.Union(second_set_h);

                    this._selected_lines.Clear();
                    this._selected_lines = except_union_set.ToHashSet();
                }

                // Arcs
                if (save_selected_arcs.Count != 0)
                {
                    // Find except list with already selected arcs
                    // Takes the data from the first result set, but not in the second result set
                    IEnumerable<arcs_store> except_first_set = this._selected_arcs.Except(save_selected_arcs, new ArcsComparer());
                    HashSet<arcs_store> first_set_h = except_first_set.ToHashSet();

                    // Takes the data from the second result set, but not in the first result set
                    IEnumerable<arcs_store> except_second_set = save_selected_arcs.Except(this._selected_arcs, new ArcsComparer());
                    HashSet<arcs_store> second_set_h = except_second_set.ToHashSet();

                    // Combine as new data result
                    IEnumerable<arcs_store> except_union_set = first_set_h.Union(second_set_h);

                    this._selected_arcs.Clear();
                    this._selected_arcs = except_union_set.ToHashSet();
                }

                // Beziers
                if (save_selected_beziers.Count != 0)
                {
                    // Find except list with already selected arcs
                    // Takes the data from the first result set, but not in the second result set
                    IEnumerable<beziers_store> except_first_set = this._selected_beziers.Except(save_selected_beziers, new BeziersComparer());
                    HashSet<beziers_store> first_set_h = except_first_set.ToHashSet();

                    // Takes the data from the second result set, but not in the first result set
                    IEnumerable<beziers_store> except_second_set = save_selected_beziers.Except(this._selected_beziers, new BeziersComparer());
                    HashSet<beziers_store> second_set_h = except_second_set.ToHashSet();

                    // Combine as new data result
                    IEnumerable<beziers_store> except_union_set = first_set_h.Union(second_set_h);

                    this._selected_beziers.Clear();
                    this._selected_beziers = except_union_set.ToHashSet();
                }
            }

            // Clear the points list which are selected before
            this._selected_points.Clear();
            foreach (lines_store line in this._selected_lines)
            {
                // Add the points of the line as new selected points
                // Hashset automatically adds distinct items
                this._selected_points.Add(geom.all_end_pts.Last(obj => obj.Equals(line.pt_start_id)));
                this._selected_points.Add(geom.all_end_pts.Last(obj => obj.Equals(line.pt_end_id)));
            }

            foreach (arcs_store arc in this._selected_arcs)
            {
                // Add the points of the arcs as new selected points
                // Hashset automatically adds distinct items
                this._selected_points.Add(geom.all_end_pts.Last(obj => obj.Equals(arc.pt_chord_start_id)));
                this._selected_points.Add(geom.all_end_pts.Last(obj => obj.Equals(arc.pt_chord_end_id)));
            }

            foreach (beziers_store bz in this._selected_beziers)
            {
                // Add the points of the beziers as new selected points
                // Hashset automatically adds distinct items
                this._selected_points.Add(geom.all_end_pts.Last(obj => obj.Equals(bz.pt_bz_start_id)));
                this._selected_points.Add(geom.all_end_pts.Last(obj => obj.Equals(bz.pt_bz_end_id)));
            }

            // Update the selected flag
            this._Is_selected = false;
            if (this.selected_points.Count > 0)
            {
                // Get the distinct list for the nodes in the selected lines
                this._Is_selected = true;
            }
        }

        //public void update_surface_lines(HashSet<surface_store> t_ply_surfaces)
        //{
        //    // called from Surface creation API
        //    this._selected_surfaces.Clear();
        //    this._selected_surfaces.UnionWith(t_ply_surfaces);
        //}

        public void clear_interim()
        {
            // Clear the click points and control points
            this._click_pts.Clear();
            click_count = 0;

            this._selected_arcs.Clear();
            this._selected_lines.Clear();
            this._selected_points.Clear();
            this._selected_beziers.Clear();
            // this._selected_surfaces.Clear();

            // Add in progress
            this._Is_addline = false;
            this._Is_addarc1 = false;
            this._Is_addbezier = false;

            // Modify in progress
            this._Is_selection = false;
            this._Is_selected = false;
            this._Is_translate = false;
            this._Is_rotate = false;
            this._Is_mirror = false;
        }

    }
}
