using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.Drawing_area;
using varai2d_surface.global_static;

namespace varai2d_surface.Geometry_class.geometry_store
{
    [Serializable]
    public class arcs_store
    {
        // The Arc is parameterized by radians from 0 to 2 Pi given by
        // t -> center + cos(t)*radius* xaxis + sin(t)*radius* yaxis
        private int _arc_id;
        private int _pt_chord_start_id;
        private int _pt_chord_end_id;
        private double _arc_radius;
        private double _arc_length;
        // private double _arc_radius_controlpt_angle;
        private double _chord_length;
        //  private double _chord_angle;
        private double _arc_start_angle;
        private double _arc_sweep_angle;
        private int _arc_orientation;
        private HashSet<int> associated_to_surf = new HashSet<int>();

        private RectangleF _arc_bounding_box = new RectangleF(0, 0, 0, 0);
        private PointF chord_start_pt = new PointF(0, 0);
        private PointF _cntrl_pt_on_arc = new PointF(0, 0);
        private PointF chord_end_pt = new PointF(0, 0);
        private PointF _arc_crown_pt = new PointF(0, 0);
        private PointF _arc_center_pt = new PointF(0, 0);

        private PointF str_location = new PointF(0, 0);
        private PointF str_vector = new PointF(0, 0);

        private PointF[] selection_check_pts = new PointF[0]; // Points to check if the selection occurs or not

        public int arc_id { get { return this._arc_id; } }

        public int pt_chord_start_id { get { return this._pt_chord_start_id; } }

        public int pt_chord_end_id { get { return this._pt_chord_end_id; } }

        public double arc_radius { get { return this._arc_radius; } }

        public double arc_length { get { return this._arc_length; } }

        public RectangleF arc_bounding_box { get { return this._arc_bounding_box; } }

        public double arc_start_angle { get { return this._arc_start_angle; } }

        public double arc_sweep_angle { get { return this._arc_sweep_angle; } }

        public List<PointF> discretized_pts { get { return this.selection_check_pts.ToList(); } }

        public PointF arc_crown_pt { get { return this._arc_crown_pt; } }

        public PointF cntrl_pt_on_arc { get { return this._cntrl_pt_on_arc; } }

        public PointF arc_center_pt { get { return this._arc_center_pt; } }

        public string str_get_arc_id { get { return "(" + arc_id.ToString() + ") "; } }

        public string str_get_diameter { get { return "\u00D8" + ((2.0 * this._arc_radius) / gvariables.scale_factor).ToString(gvariables.ln_length_pres); } }

        public string str_get_txt
        {
            get
            {
                // Send the node details as string to paint
                string str_rslt = "";
                if (gvariables.Is_paint_memberid == true)
                {
                    str_rslt = str_get_arc_id;
                }

                if (gvariables.Is_paint_memberlength == true)
                {
                    str_rslt = str_rslt + str_get_diameter;
                }
                return str_rslt;
            }
        }

        public arcs_store(int c_arc_id, int c_pt_start_id, int c_pt_end_id, PointF c_control_pt, PointF c_center_pt, HashSet<points_store> all_perm_pts)
        {
            // Main constructor for arc store
            this._arc_id = c_arc_id;

            // Set the ids for the start and end point
            this._pt_chord_start_id = c_pt_start_id;
            this._pt_chord_end_id = c_pt_end_id;

            this.chord_start_pt = all_perm_pts.Last(obj => obj.Equals(c_pt_start_id)).get_point;
            this.chord_end_pt = all_perm_pts.Last(obj => obj.Equals(c_pt_end_id)).get_point;

            this._chord_length = gfunctions.get_length(this.chord_start_pt, this.chord_end_pt);

            this._cntrl_pt_on_arc = c_control_pt;
            this._arc_center_pt = c_center_pt;

            this._arc_radius = gfunctions.get_length(this._cntrl_pt_on_arc, this._arc_center_pt);

            // Set the arc angles
            Tuple<double, double> arc_angles = gfunctions.get_arc_angles(this.chord_start_pt, this.chord_end_pt, this._cntrl_pt_on_arc, this.arc_center_pt);
            this._arc_start_angle = arc_angles.Item1;
            this._arc_sweep_angle = arc_angles.Item2;

            // Arc length
            this._arc_length = this._arc_radius * this._arc_sweep_angle * (Math.PI / 180.0f);

            // Bounding rectangle
            this._arc_bounding_box = new RectangleF((float)(this._arc_center_pt.X - this._arc_radius), (float)(this._arc_center_pt.Y - this._arc_radius),
                (float)(this._arc_radius * 2.0), (float)(this._arc_radius * 2.0));

            this._arc_orientation = gfunctions.ordered_orientation(this.chord_start_pt, this.chord_end_pt, this._cntrl_pt_on_arc);
            //Arc angles for parameterization of t [0 to 1]
            // Set the points inbetween start and end to use for selection
            double[] param_t_list = new double[39] {0.025 ,0.05,0.075, 0.1, 
                0.125,0.15,0.175, 0.2,
                0.225,0.25,0.275, 0.3,
                0.325,0.35,0.375, 0.4,
                0.425,0.45,0.475, 0.5,
                0.525,0.55,0.575, 0.6,
                0.625,0.65,0.675, 0.7,
                0.725,0.75,0.775, 0.8,
                0.825,0.85,0.875, 0.9,
                0.925,0.95,0.975 };

            this.selection_check_pts = get_parameterized_intermediate_pts(param_t_list);

            this._arc_crown_pt = get_point_at_t(0.5);

            this.str_location = this._arc_crown_pt;
            this.str_vector = gfunctions.get_vector(this.chord_start_pt, this.chord_end_pt);

            this.associated_to_surf = new HashSet<int>();
        }

        public void Scale_arcs(double scale, PointF transL, HashSet<points_store> all_perm_pts)
        {
            //// Main constructor for arc store
            //this._arc_id = c_arc_id;

            //// Set the ids for the start and end point
            //this._pt_chord_start_id = c_pt_start_id;
            //this._pt_chord_end_id = c_pt_end_id;

            this.chord_start_pt = all_perm_pts.Last(obj => obj.Equals(this._pt_chord_start_id)).get_point;
            this.chord_end_pt = all_perm_pts.Last(obj => obj.Equals(this._pt_chord_end_id)).get_point;

            this._chord_length = gfunctions.get_length(this.chord_start_pt, this.chord_end_pt);

            /////______________________________________________________________________________////////
            // Translate the control point and arc center
            this._cntrl_pt_on_arc.X = this._cntrl_pt_on_arc.X + transL.X;
            this._cntrl_pt_on_arc.Y = this._cntrl_pt_on_arc.Y + transL.Y;

            this._arc_center_pt.X = this._arc_center_pt.X + transL.X;
            this._arc_center_pt.Y = this._arc_center_pt.Y + transL.Y;

            // Scale the control point and arc center
            this._cntrl_pt_on_arc.X = (float)(this._cntrl_pt_on_arc.X * scale);
            this._cntrl_pt_on_arc.Y = (float)(this._cntrl_pt_on_arc.Y * scale);

            this._arc_center_pt.X = (float)(this._arc_center_pt.X * scale);
            this._arc_center_pt.Y = (float)(this._arc_center_pt.Y * scale);
            /////______________________________________________________________________________////////

            this._arc_radius = gfunctions.get_length(this._cntrl_pt_on_arc, this._arc_center_pt);

            // Set the arc angles
            Tuple<double, double> arc_angles = gfunctions.get_arc_angles(this.chord_start_pt, this.chord_end_pt, this._cntrl_pt_on_arc, this.arc_center_pt);
            this._arc_start_angle = arc_angles.Item1;
            this._arc_sweep_angle = arc_angles.Item2;

            // Arc length
            this._arc_length = this._arc_radius * this._arc_sweep_angle * (Math.PI / 180.0f);

            // Bounding rectangle
            this._arc_bounding_box = new RectangleF((float)(this._arc_center_pt.X - this._arc_radius), (float)(this._arc_center_pt.Y - this._arc_radius),
                (float)(this._arc_radius * 2.0), (float)(this._arc_radius * 2.0));

            this._arc_orientation = gfunctions.ordered_orientation(this.chord_start_pt, this.chord_end_pt, this._cntrl_pt_on_arc);
            //Arc angles for parameterization of t [0 to 1]
            // Set the points inbetween start and end to use for selection
            double[] param_t_list = new double[11] { 0.01, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.99 };

            this.selection_check_pts = get_parameterized_intermediate_pts(param_t_list);
            this._arc_crown_pt = get_point_at_t(0.5);

            this.str_location = this._arc_crown_pt;
            this.str_vector = gfunctions.get_vector(this.chord_start_pt, this.chord_end_pt);

        }

        public void paint_arcs(Graphics gr0)
        {
            // Draw Arc
            gr0.DrawArc(gvariables.pen_curves,this._arc_bounding_box, (float)(this._arc_start_angle), (float)(this._arc_sweep_angle));

            //  Draw string (Arc diameter)
            string_drawing_control.paint_string(gr0, str_get_txt, this.str_vector, this._arc_crown_pt);

            // Paint center
            //points_store pt = new points_store(-1, arc_center_pt.X, arc_center_pt.Y);
            //pt.paint_point(gr0);

            //// Paint the selection points
            //foreach (PointF pts in selection_check_pts)
            //    gr0.DrawEllipse(Pens.Black, gfunctions.get_ellipse_rectangle(pts, 3));

            //double chrd_s_t = 0.0;

            //for (int i = 0; i < 10; i++)
            //{
            //    // chord segment
            //    PointF chrd_p = get_point_at_t(chrd_s_t);
            //    PointF chrd_q = get_point_at_t(chrd_s_t + 0.1);
            //    double chrd_length = gfunctions.get_length(chrd_p, chrd_q);
            //    double chrd_slope = (chrd_q.Y - chrd_p.Y) / (chrd_q.X - chrd_p.X);

            //    gr0.DrawLine(Pens.Black, chrd_p, chrd_q);

            //    // tangent segment
            //    PointF tngt_pt = get_point_at_t(chrd_s_t + 0.05);
            //    PointF tngt_pt_p = new PointF((float)(tngt_pt.X - ((chrd_q.X - chrd_p.X) * 0.5)), (float)(tngt_pt.Y - ((chrd_q.Y - chrd_p.Y) * 0.5)));
            //    PointF tngt_pt_q = new PointF((float)(tngt_pt.X + ((chrd_q.X - chrd_p.X) * 0.5)), (float)(tngt_pt.Y + ((chrd_q.Y - chrd_p.Y) * 0.5)));

            //    gr0.DrawLine(Pens.Black, tngt_pt_p, tngt_pt_q);

            //    chrd_s_t = chrd_s_t + 0.1;
            //}

            //chrd_s_t = 0;
            //for (int i = 0; i < 9; i++)
            //{
            //    // chord segment
            //    PointF chrd_p = get_point_at_t(chrd_s_t+0.05);
            //    PointF chrd_q = get_point_at_t(chrd_s_t + 0.15);
            //    double chrd_length = gfunctions.get_length(chrd_p, chrd_q);
            //    double chrd_slope = (chrd_q.Y - chrd_p.Y) / (chrd_q.X - chrd_p.X);

            //    gr0.DrawLine(Pens.Black, chrd_p, chrd_q);

            //    // tangent segment
            //    PointF tngt_pt = get_point_at_t(chrd_s_t + 0.1);
            //    PointF tngt_pt_p = new PointF((float)(tngt_pt.X - ((chrd_q.X - chrd_p.X) * 0.5)), (float)(tngt_pt.Y - ((chrd_q.Y - chrd_p.Y) * 0.5)));
            //    PointF tngt_pt_q = new PointF((float)(tngt_pt.X + ((chrd_q.X - chrd_p.X) * 0.5)), (float)(tngt_pt.Y + ((chrd_q.Y - chrd_p.Y) * 0.5)));

            //    gr0.DrawLine(Pens.Black, tngt_pt_p, tngt_pt_q);

            //    chrd_s_t = chrd_s_t + 0.1;
            //}


        }

        public void paint_selected_arcs(Graphics gr0)
        {
            // Draw Arc
            gr0.DrawArc(gvariables.pen_selected_curves, this._arc_bounding_box, (float)(this._arc_start_angle), (float)(this._arc_sweep_angle));

            // Paint the Arc center point and crown point when selected
            PointF[] center_tr_pts = gfunctions.get_equilateral_triangle(gvariables.radius_points * 4, this._arc_center_pt);
            gr0.FillPolygon(gvariables.pen_points.Brush, center_tr_pts);

            PointF[] crown_tr_pts = gfunctions.get_equilateral_triangle(gvariables.radius_points * 4, this._arc_crown_pt);
            gr0.FillPolygon(gvariables.pen_points.Brush, crown_tr_pts);
        }

        public void paint_temp_arc_during_translation(Graphics gr0, double transl_x, double transl_y)
        {
            // Paint the arcs while being translated (to give user the idea how the arcs will be after translating)
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(this._arc_bounding_box, (float)(this._arc_start_angle), (float)(this._arc_sweep_angle));

            //Initailize a identity matrix(3x2) (1,0, 0,1, 0,0) (default last column 0,0,1)
            Matrix mm = new Matrix();
            // Apply translation
            mm.Translate((float)transl_x, (float)transl_y);
            gp.Transform(mm);

            gr0.DrawPath(gvariables.pen_curves, gp);
        }

        public void paint_temp_arc_during_rotation(Graphics gr0, PointF rotation_pt, double rot_angle_rad)
        {
            // Paint the arcs while being rotated (to give user the idea how the arcs will be after rotation)
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(this._arc_bounding_box, (float)(this._arc_start_angle), (float)(this._arc_sweep_angle));

            //Initailize a identity matrix(3x2) (1,0, 0,1, 0,0) (default last column 0,0,1)
            Matrix mm = new Matrix();
            // Apply rotation
            mm.RotateAt((float)(rot_angle_rad * (180 / Math.PI)), rotation_pt);
            gp.Transform(mm);

            gr0.DrawPath(gvariables.pen_curves, gp);
        }

        public void paint_temp_arc_during_mirror(Graphics gr0, PointF rotation_pt, double rot_angle_rad)
        {
            // Paint the arcs while being mirrored (to give user the idea how the arcs will be after mirroring)
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(this._arc_bounding_box, (float)(this._arc_start_angle), (float)(this._arc_sweep_angle));


            //Initailize a identity matrix(3x2) (1,0, 0,1, 0,0) (default last column 0,0,1)
            Matrix km = new Matrix();
            // Translate to rotation point
            km.Translate(-rotation_pt.X, -rotation_pt.Y);

            // Apply mirror rotation
            Matrix mm = new Matrix((float)Math.Cos(-2 * rot_angle_rad),
                (float)Math.Sin(2 * rot_angle_rad),
                (float)Math.Sin(2 * rot_angle_rad),
                -(float)(Math.Cos(2 * rot_angle_rad)), 0, 0);

            // Multiply with the original tranlated matrix
            km.Multiply(mm, MatrixOrder.Append);

            // Translate back 
            km.Translate(rotation_pt.X, rotation_pt.Y, MatrixOrder.Append);

            // Apply mirror transformation
            gp.Transform(km);

            gr0.DrawPath(gvariables.pen_curves, gp);
        }

        private PointF[] get_parameterized_intermediate_pts(double[] param_t)
        {
            // Exit if no paramaterized t is requested // never triggered
            if (param_t.Length == 0)
                return null;

            PointF[] get_param_pts = new PointF[param_t.Length];
            // Get the list of points
            for (int i = 0; i < param_t.Length; i++)
            {
                get_param_pts[i] = get_point_at_t(param_t[i]);
            }
            return get_param_pts;
        }

        public PointF get_point_at_t(double t)
        {
            if (this._arc_orientation < 0)
            {
                t = 1 - t;
            }

            double t_theta = (this._arc_start_angle * (1 - t)) + ((this._arc_start_angle + this._arc_sweep_angle) * t);
            double x_p = this._arc_center_pt.X + this._arc_radius * Math.Cos(t_theta * (Math.PI / 180));
            double y_p = this._arc_center_pt.Y + this._arc_radius * Math.Sin(t_theta * (Math.PI / 180));

            return new PointF((float)x_p, (float)y_p);
        }

        public void set_association_to_surface(bool is_add, int surf_id)
        {
            // Add or Remove surface association of the arc
            if(is_add == true)
            {
                this.associated_to_surf.Add(surf_id);
            }
            else
            {
                this.associated_to_surf.Remove(surf_id);
            }
        }

        public bool Is_selected(RectangleF selection_rect)
        {
            if (this.associated_to_surf.Count> 0)
                return false;

            if (selection_rect.Contains(this.chord_start_pt) == true || selection_rect.Contains(this.chord_end_pt) == true)
                return true;

            // Check whether the paramererized internal points between the end points of arc lies inside the selection rectangle
            for (int i = 0; i < selection_check_pts.Length; i++)
            {
                if (selection_rect.Contains(selection_check_pts[i]) == true)
                {
                    return true;
                }
            }
            return false;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as arcs_store);
        }

        public bool Equals(arcs_store other_arc)
        {
            // Check 1 (Arc Ids)
            if (this.Equals(other_arc.arc_id))
            {
                // Arc ids are matching
                return true;
            }

            // Check 2 
            //Both the Arc have same start and end points 
            if (((this.pt_chord_start_id == other_arc.pt_chord_start_id) && (this.pt_chord_end_id == other_arc.pt_chord_end_id)) ||
                    ((this.pt_chord_start_id == other_arc.pt_chord_end_id) && (this.pt_chord_end_id == other_arc.pt_chord_start_id)))
            {
                //And the Arcs have common arc crown point
                points_store a_crownpt = new points_store(-200, this._arc_crown_pt.X, this._arc_crown_pt.Y);
                points_store other_a_crownpt = new points_store(-200, other_arc.arc_crown_pt.X, other_arc.arc_crown_pt.Y);
                if (a_crownpt.check_point_snap(other_a_crownpt) == true)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Equals(int other_arc_id)
        {
            if (this.arc_id == other_arc_id)
            {
                // Arc ids are matching
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            // Hashcode takes in the combination of Arc id, chord ends, radius & orientation
            return HashCode.Combine(this.arc_id, this.pt_chord_start_id, this.pt_chord_end_id, this.arc_radius);
        }

    }

    // Custom comparer for the arc_store class
    class ArcsComparer : IEqualityComparer<arcs_store>
    {
        // Arcs are equal if their arc_ids are equal or both lines have same point id (either direction).
        public bool Equals(arcs_store first_arc, arcs_store second_arc)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(first_arc, second_arc)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(first_arc, null) || Object.ReferenceEquals(second_arc, null))
                return false;

            //Check whether the arcs are equal.
            return first_arc.Equals(second_arc);
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(arcs_store other_arc)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(other_arc, null)) return 0;

            //Calculate the hash code for the product.
            return other_arc.GetHashCode();
        }
    }
}
