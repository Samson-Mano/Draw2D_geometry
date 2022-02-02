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
    public class lines_store : IEquatable<lines_store>
    {
        private int _line_id;
        private int _pt_start_id;
        private int _pt_end_id;
        private double _line_length;
        private HashSet<int> associated_to_surf = new HashSet<int>();

        private PointF start_pt = new PointF(0, 0);
        private PointF end_pt = new PointF(0, 0);

        private PointF str_location = new PointF(0, 0);
        private PointF str_vector = new PointF(0, 0);

        private PointF[] selection_check_pts = new PointF[0]; // Points to check if the selection occurs or not

        public int line_id { get { return this._line_id; } }

        public int pt_start_id { get { return this._pt_start_id; } }

        public int pt_end_id { get { return this._pt_end_id; } }

        public double line_length { get { return this._line_length; } }

        public List<PointF> discretized_pts { get { return this.selection_check_pts.ToList(); } }

        public string str_get_line_id { get {return "(" + this._line_id.ToString() + ") ";} } 

        public string str_get_length { get { return (this._line_length/gvariables.scale_factor).ToString(gvariables.ln_length_pres); } }

        public string str_get_txt
        {
            get
            {
                // Send the node details as string to paint
                string str_rslt = "";
                if (gvariables.Is_paint_memberid == true)
                {
                    str_rslt = str_get_line_id;
                }

                if (gvariables.Is_paint_memberlength == true)
                {
                    str_rslt = str_rslt + str_get_length;
                }
                return str_rslt;
            }
        }

        public lines_store(int ln_id,int pt_start_id, int pt_end_id, HashSet<points_store> all_perm_pts)
        {
            // Constructor to control line addition
            this._line_id = ln_id;
         
            this._pt_start_id = pt_start_id;
            this._pt_end_id = pt_end_id;

            this.start_pt = all_perm_pts.Last(obj => obj.Equals(pt_start_id)).get_point;
            this.end_pt = all_perm_pts.Last(obj => obj.Equals(pt_end_id)).get_point;

            this._line_length = gfunctions.get_length(this.start_pt,this.end_pt);

            // Set the points inbetween start and end to use for selection
            double[] param_t_list = new double[9] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9};

            this.selection_check_pts = get_parameterized_intermediate_pts(param_t_list);

            this.str_vector = gfunctions.get_vector(start_pt, end_pt);
            this.str_location = gfunctions.get_midpoint(start_pt, end_pt);

            this.associated_to_surf = new HashSet<int>();
        }

        public void Scale_lines(double scale,PointF transL, HashSet<points_store> all_perm_pts)
        {
            //this._line_id = ln_id;

            //this._pt_start_id = pt_start_id;
            //this._pt_end_id = pt_end_id;

            this.start_pt = all_perm_pts.Last(obj => obj.Equals(pt_start_id)).get_point;
            this.end_pt = all_perm_pts.Last(obj => obj.Equals(pt_end_id)).get_point;

            this._line_length = gfunctions.get_length(this.start_pt, this.end_pt);

            // Set the points inbetween start and end to use for selection
            double[] param_t_list = new double[11] { 0.01, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.99 };

            this.selection_check_pts = get_parameterized_intermediate_pts(param_t_list);

            this.str_vector = gfunctions.get_vector(start_pt, end_pt);
            this.str_location = gfunctions.get_midpoint(start_pt, end_pt);


        }

        public void paint_lines(Graphics gr0)
        {
            // Draw line
            gr0.DrawLine(gvariables.pen_curves, start_pt, end_pt);

            // Draw String
            string_drawing_control.paint_string(gr0, str_get_txt, this.str_vector, this.str_location);
        }

        public void paint_selected_lines(Graphics gr0)
        {
            // Draw line
            gr0.DrawLine(gvariables.pen_selected_curves, start_pt, end_pt);
        }

        public void paint_temp_line_during_translation(Graphics gr0, double transl_x, double transl_y)
        {
            // Paint the lines while being translated (to give user the idea how the lines will be after translation)
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(start_pt,end_pt);

            //Initailize a identity matrix(3x2) (1,0, 0,1, 0,0) (default last column 0,0,1)
            Matrix mm = new Matrix();
            // Apply translation
            mm.Translate((float)transl_x,(float) transl_y);
            gp.Transform(mm);

            gr0.DrawPath(gvariables.pen_curves, gp);
        }

        public void paint_temp_line_during_rotation(Graphics gr0, PointF rotation_pt, double rot_angle_rad)
        {
            // Paint the lines while being rotated (to give user the idea how the lines will be after rotation)
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(start_pt, end_pt);

            //Initailize a identity matrix(3x2) (1,0, 0,1, 0,0) (default last column 0,0,1)
            Matrix mm = new Matrix();
            // Apply rotation
            mm.RotateAt((float)(rot_angle_rad * (180 / Math.PI)), rotation_pt);
            gp.Transform(mm);

            gr0.DrawPath(gvariables.pen_curves, gp);
        }

        public void paint_temp_line_during_mirror(Graphics gr0, PointF rotation_pt, double rot_angle_rad)
        {
            // Paint the lines while being mirrored (to give user the idea how the lines will be after mirroring)

            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(start_pt, end_pt);

            //Initailize a identity matrix(3x2) (1,0, 0,1, 0,0) (default last column 0,0,1)
            Matrix km = new Matrix();
            // Translate to rotation point
            km.Translate(-rotation_pt.X, -rotation_pt.Y);

            // Apply mirror rotation
            Matrix mm = new Matrix((float)Math.Cos(-2*rot_angle_rad),
                (float)Math.Sin(2*rot_angle_rad),
                (float)Math.Sin(2* rot_angle_rad),
                -(float)(Math.Cos(2* rot_angle_rad)), 0, 0);

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
            double t_x = (start_pt.X * (1 - t)) + (end_pt.X * t);
            double t_y = (start_pt.Y * (1 - t)) + (end_pt.Y * t);
            return new PointF((float)t_x, (float)t_y);
        }

        public void set_association_to_surface(bool is_add, int surf_id)
        {
            // Add or Remove surface association of the arc
            if (is_add == true)
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
            if (this.associated_to_surf.Count > 0)
                return false;

            if (selection_rect.Contains(this.start_pt) == true || selection_rect.Contains(this.end_pt) == true)
                return true;

            // Check whether the paramererized internal points between the end points of line lies inside the selection rectangle
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
            return Equals(obj as lines_store);
        }

        public bool Equals(lines_store other_line)
        {
            // Check 1 (Line Ids)
            if (this.Equals(other_line.line_id))
            {
                // Lines ids are matching
                return true;
            }

            // Check 2
            if (((this.pt_start_id == other_line.pt_start_id) && (this.pt_end_id == other_line.pt_end_id))||
                ((this.pt_start_id == other_line.pt_end_id) && (this.pt_end_id == other_line.pt_start_id)))
            {
                //Both the lines have same start and end points (same direction)
                return true;
            }

            return false;
        }

        public bool Equals(int other_line_id)
        {
            if (this.line_id == other_line_id)
            {
                // Lines ids are matching
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.line_id, this.pt_start_id, this.pt_end_id);
        }

    }

    // Custom comparer for the lines_store class
    class LinesComparer : IEqualityComparer<lines_store>
    {
        // lines are equal if their line_ids are equal or both lines have same point id (either direction).
        public bool Equals(lines_store first_line, lines_store second_line)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(first_line, second_line)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(first_line, null) || Object.ReferenceEquals(second_line, null))
                return false;

            //Check whether the lines are equal.
            return first_line.Equals(second_line);
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(lines_store other_line)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(other_line, null)) return 0;

            //Calculate the hash code for the product.
            return other_line.GetHashCode();
        }
    }
}
