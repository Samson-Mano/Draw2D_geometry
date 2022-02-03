using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.Geometry_class.geometry_store;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Forms;
using varai2d_surface.Properties;
using varai2d_surface.Drawing_area;

namespace varai2d_surface.global_static
{
    public static class gfunctions
    {
        public static void update_pen()
        {
            // Update the static pen because of transpernecy change
            gvariables.pen_curves = new Pen(Color.FromArgb(255 - gvariables.Trans_PV, gvariables.color_memberclr), gvariables.linewidth_curves);
            gvariables.pen_points = new Pen(Color.FromArgb(225 - gvariables.Trans_PV, gvariables.color_pointsclr), 2);
            gvariables.pen_snapline = new Pen(Color.FromArgb(180, gvariables.color_snaplineclr), gvariables.linewidth_snapline);
            gvariables.pen_snapline.DashStyle = DashStyle.DashDotDot;
            gvariables.pen_string = new Pen(Color.FromArgb(255 - gvariables.Trans_PV, gvariables.color_stringforeclr), 3);
            string_drawing_control.font_paintstring = new Font("Cambria Math", gvariables.str_font_size);
        }

        public static void set_default_global_variable()
        {
            gvariables.color_mainpic = Settings.Default.Sett_mainpic;
            gvariables.color_memberclr = Settings.Default.Sett_member_clr;
            gvariables.color_pointsclr = Settings.Default.Sett_point_clr;
            gvariables.color_snaplineclr = Settings.Default.Sett_snapline_clr;
            gvariables.color_stringforeclr = Settings.Default.Sett_stringfore_clr;

            gvariables.linewidth_curves = Settings.Default.Sett_member_width;
            gvariables.radius_points = Settings.Default.Sett_point_radius;
            gvariables.linewidth_snapline = Settings.Default.Sett_snapline_width;

            gvariables.Is_hvsnap = Settings.Default.Sett_horiz_vert_snap;
            gvariables.Is_xysnap = Settings.Default.Sett_point_snap;
            gvariables.hvsnap_intensity = Settings.Default.Sett_horiz_vert_intensity;
            gvariables.xysnap_intensity = Settings.Default.Sett_point_intensity;

            gvariables.Is_paint_pt = Settings.Default.Sett_is_paint_pt;
            gvariables.Is_paint_ptcoord = Settings.Default.Sett_is_paint_ptcoord;
            gvariables.Is_paint_ptid = Settings.Default.Sett_is_paint_ptid;
            gvariables.Is_paint_memberid = Settings.Default.Sett_is_paint_memid;
            gvariables.Is_paint_memberlength = Settings.Default.Sett_is_paint_memlength;
            gvariables.Is_paint_surfaceid = Settings.Default.Sett_is_paint_surfid;

            gvariables.pt_coord_pres = Settings.Default.Sett_coord_pres;
            gvariables.ln_length_pres = Settings.Default.Sett_length_pres;

            gvariables.str_font_size = Settings.Default.Sett_font_size;

            update_pen();
        }

        public static PointF get_normal_pt_at_dist(PointF sPt, PointF ePt, PointF frm_pt, double dist)
        {
            // Get mid point of direction 
            PointF mid_pt = get_midpoint(sPt, ePt);
            double scaled = dist / (gfunctions.get_length(sPt, ePt) * 0.5);

            PointF vect1 = new PointF((float)(scaled*(ePt.X - mid_pt.X)), (float)(scaled*(ePt.Y - mid_pt.Y)));

            PointF vect1_90deg = new PointF(-vect1.Y, vect1.X);

            return new PointF(frm_pt.X + vect1_90deg.X, frm_pt.Y + vect1_90deg.Y);
        }


        public static PointF get_point_at_l_m(PointF from_pt, double slope_m, double length_l, int orientation_o)
        {
            // slope_m is the slope of line, and the required Point lies distance length_l
            // away from the from_pt
            PointF f_pt, s_pt;

            if (slope_m == 0)
            {
                // Slope is zero
                f_pt = new PointF((float)(from_pt.X + length_l), from_pt.Y);
                s_pt = new PointF((float)(from_pt.X - length_l), from_pt.Y);
            }
            else if (Double.IsInfinity(slope_m))
            {
                // Slope is infinity
                f_pt = new PointF(from_pt.X, (float)(from_pt.Y + length_l));
                s_pt = new PointF(from_pt.X, (float)(from_pt.Y - length_l));
            }
            else
            {
                // All the other slopes
                double dx = (float)(length_l / Math.Sqrt(1 + (slope_m * slope_m)));
                double dy = slope_m * dx;
                f_pt = new PointF((float)(from_pt.X + dx), (float)(from_pt.Y + dy));
                s_pt = new PointF((float)(from_pt.X - dx), (float)(from_pt.Y - dy));
            }

            // Orientation
            if (orientation_o == 1)
            {
                return f_pt;
            }
            else
            {
                return s_pt;
            }
        }

        public static PointF[] get_bezier_polynomial_pts(List<PointF> cntrl_pts, int n_pt_count)
        {
            List<PointF> polynomial_pt = new List<PointF>();
            double t_iteration = (1.0f / (double)n_pt_count);

            double t = 0;
            for (int i = 0; i <= n_pt_count; i++)
            {
                polynomial_pt.Add(getCasteljauPoint(cntrl_pts, cntrl_pts.Count - 1, 0, t));
                t = t + t_iteration;
            }

            return polynomial_pt.ToArray();
        }

        private static PointF getCasteljauPoint(List<PointF> cntrl_pts, int r, int i, double t)
        {
            if (r == 0) return cntrl_pts[i];

            PointF p1 = getCasteljauPoint(cntrl_pts, r - 1, i, t);
            PointF p2 = getCasteljauPoint(cntrl_pts, r - 1, i + 1, t);

            return new PointF((float)((1 - t) * p1.X + t * p2.X), (float)((1 - t) * p1.Y + t * p2.Y));
        }

        public static PointF get_midpoint(PointF pt1, PointF pt2)
        {
            // Return mid pt of two point
            return new PointF((pt1.X + pt2.X) * 0.5f, (pt1.Y + pt2.Y) * 0.5f);
        }

        public static PointF get_vector(PointF pt1, PointF pt2)
        {
            // Return vector from point 1 to point 2
            return new PointF(pt2.X - pt1.X, pt2.Y - pt1.Y);
        }

        public static double get_length(PointF pt1, PointF pt2)
        {
            // Returns length between points
            return (Math.Sqrt(Math.Pow((pt1.X - pt2.X), 2) + Math.Pow((pt1.Y - pt2.Y), 2)));
        }

        public static double get_angle_deg(PointF pt1, PointF pt2)
        {
            // returns angle made by two points with horizontal line (y= constant)
            return (Math.Atan2(pt1.Y - pt2.Y, pt2.X - pt1.X) * (180 / Math.PI));
        }

        public static double get_angle_rad(PointF pt1, PointF pt2)
        {
            // returns angle made by two points with horizontal line (y= constant)
            return (Math.Atan2(pt1.Y - pt2.Y, pt2.X - pt1.X));
        }

        public static double get_angle_ABX(PointF A_pt, PointF B_pt, bool is_deg = false)
        {
            // Angle made with X -axis (assuming pt B lies on x_axis)
            double delta_X, delta_Y;
            delta_X = Math.Abs(B_pt.X - A_pt.X);
            delta_Y = Math.Abs(B_pt.Y - A_pt.Y);

            // Angle with x-line
            double angle_with_xaxis;
            angle_with_xaxis = Math.Atan2(delta_Y, delta_X);

            if (B_pt.Y < A_pt.Y) // Second point is lower than first, angle goes down (180-360)
            {
                if (B_pt.X < A_pt.X) // Second point is to the left of first (180-270)
                {
                    angle_with_xaxis = angle_with_xaxis + Math.PI; // 180 degree to 270 degree
                }
                else // Angle range (270 to 360)
                {
                    angle_with_xaxis = (2 * Math.PI) - angle_with_xaxis;  // 270 degree to 360 degree
                }
            }
            else if (B_pt.X < A_pt.X) //Second point is top left of first (90-180)
            {
                angle_with_xaxis = Math.PI - angle_with_xaxis; // 90 degree to 180 degree
            }

            if (is_deg == true)
                angle_with_xaxis = angle_with_xaxis * (180 / Math.PI);

            return angle_with_xaxis;
        }

        public static Tuple<double, double> get_arc_angles(PointF chord_start_pt, PointF chord_end_pt, PointF pt_on_arc, PointF arc_center_pt)
        {
            // Start and sweep angle
            double start_angle, sweep_angle;
            if (gfunctions.ordered_orientation(chord_start_pt, chord_end_pt, pt_on_arc) > 0)
            {
                // Counter clockwise in screen co-ordinates
                start_angle = -1.0 * (360 - gfunctions.get_angle_ABX(arc_center_pt, chord_start_pt, true));
                sweep_angle = (gfunctions.angle_between_2lines(arc_center_pt, chord_start_pt, arc_center_pt, chord_end_pt, true));

                if (gfunctions.ordered_orientation(chord_start_pt, chord_end_pt, arc_center_pt) > 0)
                {
                    sweep_angle = 360 - sweep_angle;
                }
            }
            else
            {
                // Clockwise in screen co-ordinates
                start_angle = -1.0 * (360 - gfunctions.get_angle_ABX(arc_center_pt, chord_end_pt, true));
                sweep_angle = (gfunctions.angle_between_2lines(arc_center_pt, chord_end_pt, arc_center_pt, chord_start_pt, true));

                if (gfunctions.ordered_orientation(chord_end_pt, chord_start_pt, arc_center_pt) > 0)
                {
                    sweep_angle = 360 - sweep_angle;
                }
            }

            return new Tuple<double, double>(start_angle, sweep_angle);
        }

        public static bool is_leftofline(PointF ln_pt1, PointF ln_pt2, PointF pt)
        {
            PointF vect_1 = new PointF(ln_pt2.X - ln_pt1.X, ln_pt2.Y - ln_pt1.Y);
            PointF vect_2 = new PointF(pt.X - ln_pt1.X, pt.Y - ln_pt1.Y);

            // Determining cross Product
            int cross_prd = (int)(vect_1.X * vect_2.Y) - (int)(vect_2.Y * vect_1.X);

            if (cross_prd < 0)
            {
                return true;
            }

            return false;
        }

        public static double get_inclination(PointF pt1, PointF pt2)
        {
            double t_tx = pt2.X - pt1.X;
            double t_ty = pt2.Y - pt2.Y;

            double incl = Math.Atan2(t_ty, t_tx);

            if (incl < 0)
            {
                incl = (2 * Math.PI) + incl;
            }

            return incl;
        }

        public static RectangleF get_ellipse_rectangle(PointF pt, int ellipse_radii)
        {
            return new RectangleF(pt.X - ellipse_radii, pt.Y - ellipse_radii, ellipse_radii * 2.0f, ellipse_radii * 2.0f);
        }

        public static int RoundOff(this int i)
        {
            // Roundoff to nearest 10 (used to display zoom value)
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        public static PointF[] get_equilateral_triangle(double side_a, PointF center_pt)
        {
            PointF[] tri_coords = new PointF[3];

            // Point 1
            tri_coords[0] = new PointF(center_pt.X, (center_pt.Y + (float)(side_a * Math.Sqrt(3) * 0.25)));
            // Point 2
            tri_coords[1] = new PointF(center_pt.X - (float)(side_a * 0.5), (center_pt.Y - (float)(side_a * Math.Sqrt(3) * 0.25)));
            // Point 3
            tri_coords[2] = new PointF(center_pt.X + (float)(side_a * 0.5), (center_pt.Y - (float)(side_a * Math.Sqrt(3) * 0.25)));

            return tri_coords;
        }

        public static double angle_between_2lines(PointF line1_pt1, PointF line1_pt2, PointF line2_pt1, PointF line2_pt2, bool to_deg = false)
        {
            double dx1, dy1;
            double norm;
            dx1 = line1_pt2.X - line1_pt1.X;
            dy1 = line1_pt2.Y - line1_pt1.Y;
            norm = Math.Sqrt((dx1 * dx1) + (dy1 * dy1));
            // vector 1
            dx1 = dx1 / norm;
            dy1 = dy1 / norm;

            double dx2, dy2;
            dx2 = line2_pt2.X - line2_pt1.X;
            dy2 = line2_pt2.Y - line2_pt1.Y;
            norm = Math.Sqrt((dx2 * dx2) + (dy2 * dy2));
            // vector 2
            dx2 = dx2 / norm;
            dy2 = dy2 / norm;

            // Dot product
            double angle_in_rad = Math.Acos((dx1 * dx2) + (dy1 * dy2));

            if (to_deg == true)
                angle_in_rad = angle_in_rad * (180 / Math.PI);

            return angle_in_rad;
        }


        public static PointF get_normal_intersecton_pt(PointF ln_pt1, PointF ln_pt2, PointF from_pt, ref bool is_collinear)
        {
            // Gives the intersection point of a normal line from a point (from_pt) to the line
            double line_slope = ((ln_pt2.Y - ln_pt1.Y) / (ln_pt2.X - ln_pt1.X));
            double norm_slope = -1.0 / line_slope;

            // General Line equation from pt1 (x1,y1) to pt2 (x2,y2) is of form ax + by + c = 0
            //a = y1 - y2
            //b = x2 - x1
            //c = (y2 - y1)x1 + (x1 - x2)y1
            double a, b, c;
            a = ln_pt1.Y - ln_pt2.Y;
            b = ln_pt2.X - ln_pt1.X;
            c = ((ln_pt2.Y - ln_pt1.Y) * ln_pt1.X) + ((ln_pt1.X - ln_pt2.X) * ln_pt1.Y);

            // Perpendicular line form is bx - ay = D
            double D = ((b * from_pt.X) - (a * from_pt.Y));

            Matrix<double> A_mat = DenseMatrix.OfArray(new double[,] { { a, b }, { b, -1.0 * a } });
            Matrix<double> b_mat = DenseMatrix.OfColumnArrays(new double[] { -1.0 * c, D });

            PointF intersection_pt = new PointF(0, 0);
            // check for degenerate case (Collinear cases are not considered !!!)
            if (A_mat.Determinant() != 0)
            {

                Matrix<double> x_mat = A_mat.Solve(b_mat);
                is_collinear = false;
                intersection_pt = new PointF((float)x_mat[0, 0], (float)x_mat[1, 0]);
            }
            else
            {
                is_collinear = true;
            }
            return intersection_pt;
        }

        public static PointF get_circle_center_pt(PointF pt1, PointF pt2, PointF pt3)
        {
            // Get the center of the circle from three points
            // Form the A_matrix
            // | 2x1 2y1 1 |
            // | 2x2 2y3 1 |
            // | 2x3 2y3 1 |

            double[] a_mat_row1 = new double[3] { 2 * pt1.X, 2 * pt1.Y, 1.0 };
            double[] a_mat_row2 = new double[3] { 2 * pt2.X, 2 * pt2.Y, 1.0 };
            double[] a_mat_row3 = new double[3] { 2 * pt3.X, 2 * pt3.Y, 1.0 };

            Matrix<double> A_mat = DenseMatrix.OfRowArrays(a_mat_row1, a_mat_row2, a_mat_row3);

            // Form the B_matrix
            // | -(x1^2 + y1^2)|
            // | -(x2^2 + y2^2)|
            // | -(x3^2 + y3^2)|

            Matrix<double> b_mat = DenseMatrix.OfColumnArrays(new double[] { -1.0*(Math.Pow(pt1.X,2) +Math.Pow(pt1.Y,2)),
                                                                             -1.0*(Math.Pow(pt2.X,2) +Math.Pow(pt2.Y,2)),
                                                                                -1.0*(Math.Pow(pt3.X,2) +Math.Pow(pt3.Y,2))});


            // Check for degenerate case
            PointF center_pt = new PointF(0, 0);
            // double circle_radius = 0.0;
            if (A_mat.Determinant() != 0)
            {
                Matrix<double> x_mat = A_mat.Solve(b_mat);

                center_pt = new PointF((float)(-1.0 * x_mat[0, 0]), (float)(-1.0 * x_mat[1, 0]));
                // circle_radius = Math.Sqrt(Math.Pow(x_mat[0, 0], 2) + Math.Pow(x_mat[1, 0], 2) - Math.Pow(x_mat[2, 0], 2));
            }


            return center_pt;
        }

        public static bool txtbox_value_IsNumeric(this object value)
        {
            if (value == null || value is DateTime)
            {
                return false;
            }

            if (value is Int16 || value is Int32 || value is Int64 || value is Decimal || value is Single || value is Double || value is Boolean)
            {
                return true;
            }

            try
            {
                if (value is string)
                    Double.Parse(value as string);
                else
                    Double.Parse(value.ToString());
                return true;
            }
            catch { }
            return false;
        }

        public static DialogResult InputBox(string title, string promptText, ref double value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value.ToString();

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 46, 372, 20);
            buttonOk.SetBounds(228, 90, 75, 30);
            buttonCancel.SetBounds(309, 90, 75, 30);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 150);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();

            if (txtbox_value_IsNumeric(textBox.Text) == false)
            {
                dialogResult = DialogResult.Cancel;
                return dialogResult;
            }


            double.TryParse(textBox.Text, out value);


            return dialogResult;
        }

        #region "Line segment intersection"
        public static bool is_lines_share_pt(PointF p1, PointF q1, PointF p2, PointF q2)
        {
            // Degenerate case 1 ( lines share a point is not considered intersection)
            if (Math.Abs(p1.X - p2.X) < gvariables.epsilon_g && Math.Abs(p1.Y - p2.Y) < gvariables.epsilon_g)
            {
                return true;
            }
            else if (Math.Abs(p1.X - q2.X) < gvariables.epsilon_g && Math.Abs(p1.Y - q2.Y) < gvariables.epsilon_g)
            {
                return true;
            }
            else if (Math.Abs(q1.X - p2.X) < gvariables.epsilon_g && Math.Abs(q1.Y - p2.Y) < gvariables.epsilon_g)
            {
                return true;
            }
            else if (Math.Abs(q1.X - q2.X) < gvariables.epsilon_g && Math.Abs(q1.Y - q2.Y) < gvariables.epsilon_g)
            {
                return true;
            }

            return false;
        }

        public static bool is_intersection_line_valid(PointF p1, PointF q1)
        {
            // Degenerate case 2 ( lines share with zero length check)
            if (Math.Abs(p1.X - q1.X) < gvariables.epsilon_g && Math.Abs(p1.Y - q1.Y) < gvariables.epsilon_g)
            {
                return false;
            }
            return true;
        }

        public static int ordered_orientation(PointF p, PointF q, PointF r)
        {
            // To find orientation of ordered triplet (p, q, r).
            // The function returns following values
            // 0 --> p, q and r are collinear
            // 1 -->  Clockwise
            // -1 --> Counter clockwise

            double val = (((q.Y - p.Y) * (r.X - q.X)) - ((q.X - p.X) * (r.Y - q.Y)));

            if (Math.Round(val, 3) == 0) return 0; // collinear

            return (val > 0) ? 1 : -1; // clock or counterclock wise
        }

        public static int ordered_orientation(points_store p, points_store q, points_store r)
        {
            // To find orientation of ordered triplet (p, q, r).
            // The function returns following values
            // 0 --> p, q and r are collinear
            // 1 --> Clockwise
            // 2 --> Counterclockwise

            double val = (((q.y - p.y) * (r.x - q.x)) - ((q.x - p.x) * (r.y - q.y)));

            if (Math.Round(val, 2) == 0) return 0; // collinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise

        }

        public static bool Is_lines_intersect(PointF p1, PointF q1, PointF p2, PointF q2)
        {
            // check whether the lines share points
            if (is_lines_share_pt(p1, q1, p2, q2) == true)
            {
                return false;
            }

            // Check whether the bounding box intersect
            if (Math.Max(p1.X, q1.X) >= Math.Min(p2.X, q2.X) &&
               Math.Max(p2.X, q2.X) >= Math.Min(p1.X, q1.X) &&
               Math.Max(p1.Y, q1.Y) >= Math.Min(p2.Y, q2.Y) &&
               Math.Max(p2.Y, q2.Y) >= Math.Min(p1.Y, q1.Y))
            {
                // The function that returns true if line segment 'p1q1'
                // and 'p2q2' intersect.
                int o1 = ordered_orientation(p1, q1, p2);
                int o2 = ordered_orientation(p1, q1, q2);
                int o3 = ordered_orientation(p2, q2, p1);
                int o4 = ordered_orientation(p2, q2, q1);

                // General case
                if (o1 != o2 && o3 != o4)
                    return true;
            }
            // colinear cases are not considered !!!!

            return false; // Doesn't fall in any of the above cases
        }


        public static PointF intersection_point(PointF ln1_p, PointF ln1_q, PointF ln2_p, PointF ln2_q)
        {
            PointF intersection_pt = new PointF(0, 0);
            // Line segment 1 (p1 -> q1) and line segment 2 (p2 -> q2)
            // the below code works based on parameterization of line segment
            // line 1: p1(1-t) + q1(t)
            // line 2: p2(1-s) + q2(s)
            // At intersection p1(1-t) + q1(t) = p2(1-s) + q2(s)
            // Simplifying gives (p1-q1)t + (q2-p2)s = (p1-p2)
            // In matrix form below
            //   |(p1_x - q1_x) (q2_x - p2_x)||t| = |p1_x - p2_x|
            //   |(p1_y - q1_y) (q2_y - p2_y)||s|   |p1_y - p2_y|
            // Solving the above for s& t gives the intersection point
            // Math.Net Numerics is used to solve (Ax = b)

            Matrix<double> A_mat = DenseMatrix.OfArray(new double[,] { { ln1_p.X - ln1_q.X, ln2_q.X - ln2_p.X }, { ln1_p.Y - ln1_q.Y, ln2_q.Y - ln2_p.Y } });
            Matrix<double> b_mat = DenseMatrix.OfColumnArrays(new double[] { ln1_p.X - ln2_p.X, ln1_p.Y - ln2_p.Y });

            double int_x, int_y;

            // check for degenerate case (Collinear cases are not considered !!!) still check
            if (A_mat.Determinant() != 0)
            {

                Matrix<double> x_mat = A_mat.Solve(b_mat);
                // resulting x_mat values must be between 0 & 1 (otherwise something wrong with the intersection check !!) 
                int_x = ln1_p.X * (1 - x_mat[0, 0]) + (ln1_q.X * x_mat[0, 0]);
                int_y = ln1_p.Y * (1 - x_mat[0, 0]) + (ln1_q.Y * x_mat[0, 0]);

                intersection_pt = new PointF((float)int_x, (float)int_y);
            }
            return intersection_pt;
        }

        public static double get_param_t_for_pt(PointF pt0,PointF pt1,PointF pt)
        {
            // convert a point in line segment to parameter t
            double tot_length = gfunctions.get_length(pt0, pt1);
            double seg_length = gfunctions.get_length(pt0, pt);

            return (seg_length / tot_length);
        }

        public static Tuple<bool, PointF, PointF> two_circle_intersection_pts(PointF A_center_pt, double A_r, PointF B_center_pt, double B_r)
        {
            double d = Math.Sqrt(Math.Pow((A_center_pt.X - B_center_pt.X), 2) + Math.Pow((A_center_pt.Y - B_center_pt.Y), 2));

            bool is_intersect = false;
            PointF inters_pt1 = new PointF(0, 0);
            PointF inters_pt2 = new PointF(0, 0);

            // Check whether radius 
            if (d <= (A_r + B_r) && d >= Math.Abs(A_r - B_r))
            {
                is_intersect = true;

                double ex = (B_center_pt.X - A_center_pt.X) / d;
                double ey = (B_center_pt.Y - A_center_pt.Y) / d;

                double x = ((A_r * A_r) - (B_r * B_r) + (d * d)) / (2 * d);
                double y = Math.Sqrt((A_r * A_r) - (x * x));

                inters_pt1 = new PointF((float)(A_center_pt.X + (x * ex) - (y * ey)),
                    (float)(A_center_pt.Y + (x * ey) + (y * ex)));

                inters_pt2 = new PointF((float)(A_center_pt.X + (x * ex) + (y * ey)),
                    (float)(A_center_pt.Y + (x * ey) - (y * ex)));
            }
            else
            {
                is_intersect = false;
            }


            return new Tuple<bool, PointF, PointF>(is_intersect, inters_pt1, inters_pt2);
        }



        #endregion


    }
}
