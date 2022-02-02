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
    public class beziers_store
    {
        private int _bezier_id;
        private int _poly_count;
        private int _pt_bz_start_id;
        private int _pt_bz_end_id;
        private List<PointF> _cntrl_pts = new List<PointF>();
        private double _bezier_length;
        private HashSet<int> associated_to_surf = new HashSet<int>();

        private PointF str_location = new PointF(0, 0);
        private PointF str_vector = new PointF(0, 0);

        private PointF[] selection_check_pts = new PointF[0]; // Points to check if the selection occurs or not

        public int bezier_id { get { return this._bezier_id; } }

        public int poly_count { get { return this._poly_count; } }

        public int pt_bz_start_id { get { return this._pt_bz_start_id; } }

        public int pt_bz_end_id { get { return this._pt_bz_end_id; } }

        public double bezier_length { get { return this._bezier_length; } }

        public List<PointF> discretized_pts { get { return this.selection_check_pts.ToList(); } }

        public List<PointF> bezier_cntrl_pts { get { return this._cntrl_pts; } }

        public string str_get_bezier_id { get { return "(" + bezier_id.ToString() + ") "; } }

        public string str_get_bezier_length { get { return (this.bezier_length / gvariables.scale_factor).ToString(gvariables.ln_length_pres); } }

        public string str_get_txt
        {
            get
            {
                // Send the node details as string to paint
                string str_rslt = "";
                if (gvariables.Is_paint_memberid == true)
                {
                    str_rslt = str_get_bezier_id;
                }

                if (gvariables.Is_paint_memberlength == true)
                {
                    str_rslt = str_rslt + str_get_bezier_length;
                }
                return str_rslt;
            }
        }

        public beziers_store(int bz_id, int t_poly_count, int pt_start_id, int pt_end_id, List<PointF> cntrl_points, HashSet<points_store> all_perm_pts)
        {
            this._bezier_id = bz_id;
            this._poly_count = t_poly_count;
            this._pt_bz_start_id = pt_start_id;
            this._pt_bz_end_id = pt_end_id;

            _cntrl_pts.AddRange(cntrl_points);
            selection_check_pts = gfunctions.get_bezier_polynomial_pts(this._cntrl_pts, 30);

            // Find the bezier length
            this._bezier_length = 0;
            for (int i = 1; i < selection_check_pts.Length; i++)
            {
                this._bezier_length = this._bezier_length + Math.Sqrt(Math.Pow(selection_check_pts[i].X - selection_check_pts[i - 1].X, 2) +
                    Math.Pow(selection_check_pts[i].Y - selection_check_pts[i - 1].Y, 2));

            }

            this.str_location = get_point_at_t(0.5);
            this.str_vector = gfunctions.get_vector(get_point_at_t(0.48), get_point_at_t(0.52));

            this.associated_to_surf = new HashSet<int>();
        }

        public void Scale_beziers(double scale, PointF transL, HashSet<points_store> all_perm_pts)
        {

            for (int i = 0; i < this._cntrl_pts.Count; i++)
            {
                // Translate and Scale
                PointF temp_pt = new PointF((float)((this._cntrl_pts[i].X + transL.X) * scale),
                    (float)((this._cntrl_pts[i].Y + transL.Y) * scale));

                this._cntrl_pts[i] = temp_pt;
            }

            selection_check_pts = gfunctions.get_bezier_polynomial_pts(this._cntrl_pts, 30);

            // Find the bezier length
            this._bezier_length = 0;
            for (int i = 1; i < selection_check_pts.Length; i++)
            {
                this._bezier_length = this._bezier_length + Math.Sqrt(Math.Pow(selection_check_pts[i].X - selection_check_pts[i - 1].X, 2) +
                    Math.Pow(selection_check_pts[i].Y - selection_check_pts[i - 1].Y, 2));

            }

            this.str_location = get_point_at_t(0.5);
            this.str_vector = gfunctions.get_vector(get_point_at_t(0.48), get_point_at_t(0.52));
        }


        public void paint_beziers(Graphics gr0)
        {
            // Draw Bezier curve
            gr0.DrawCurve(gvariables.pen_curves, selection_check_pts);

            // Draw string
            string_drawing_control.paint_string(gr0, str_get_txt, this.str_vector, this.str_location);
        }

        public void paint_beziers_selected(Graphics gr0)
        {
            // Draw Selected Bezier curve
            gr0.DrawCurve(gvariables.pen_selected_curves, selection_check_pts);

            // Paint the control points
            for (int i = 1; i < this._cntrl_pts.Count - 1; i++)
            {
                PointF[] center_tr_pts = gfunctions.get_equilateral_triangle(gvariables.radius_points * 4, this._cntrl_pts[i]);
                gr0.FillPolygon(gvariables.pen_points.Brush, center_tr_pts);
            }
        }

        public void paint_temp_bezier_during_translation(Graphics gr0, double transl_x, double transl_y)
        {
            // Paint the Beziers while being translated (to give user the idea how the arcs will be after translating)
            GraphicsPath gp = new GraphicsPath();
            gp.AddCurve(selection_check_pts);

            //Initailize a identity matrix(3x2) (1,0, 0,1, 0,0) (default last column 0,0,1)
            Matrix mm = new Matrix();
            // Apply translation
            mm.Translate((float)transl_x, (float)transl_y);
            gp.Transform(mm);

            gr0.DrawPath(gvariables.pen_curves, gp);
        }

        public void paint_temp_bezier_during_rotation(Graphics gr0, PointF rotation_pt, double rot_angle_rad)
        {
            // Paint the Beziers while being rotate (to give user the idea how the arcs will be after rotating)
            GraphicsPath gp = new GraphicsPath();
            gp.AddCurve(selection_check_pts);

            //Initailize a identity matrix(3x2) (1,0, 0,1, 0,0) (default last column 0,0,1)
            Matrix mm = new Matrix();
            // Apply rotation
            mm.RotateAt((float)(rot_angle_rad * (180 / Math.PI)), rotation_pt);
            gp.Transform(mm);

            gr0.DrawPath(gvariables.pen_curves, gp);
        }

        public void paint_temp_bezier_during_mirror(Graphics gr0, PointF rotation_pt, double rot_angle_rad)
        {
            // Paint the Beziers while being mirrored (to give user the idea how the arcs will be after mirroring)
            GraphicsPath gp = new GraphicsPath();
            gp.AddCurve(selection_check_pts);

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
            if (this.associated_to_surf.Count> 0)
                return false;

            // Check whether the paramererized internal points between the end points of bezier curve lies inside the selection rectangle
            for (int i = 0; i < selection_check_pts.Length; i++)
            {
                if (selection_rect.Contains(selection_check_pts[i]) == true)
                {
                    return true;
                }
            }
            return false;
        }

        public PointF get_point_at_t(double param_t)
        {
            return getCasteljauPoint(this._cntrl_pts, this._cntrl_pts.Count - 1, 0, param_t);
        }

        private PointF getCasteljauPoint(List<PointF> cntrl_pts, int r, int i, double t)
        {
            if (r == 0) return cntrl_pts[i];

            PointF p1 = getCasteljauPoint(cntrl_pts, r - 1, i, t);
            PointF p2 = getCasteljauPoint(cntrl_pts, r - 1, i + 1, t);

            return new PointF((float)((1 - t) * p1.X + t * p2.X), (float)((1 - t) * p1.Y + t * p2.Y));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as beziers_store);
        }

        public bool Equals(beziers_store other_bz)
        {
            // Check 1 (Bezier Ids)
            if (this.Equals(other_bz.bezier_id))
            {
                // Bezier ids are matching
                return true;
            }

            // Check 2 (Control point count and end points matching)
            if ((other_bz.discretized_pts[0] == this.discretized_pts[0] && other_bz.discretized_pts[discretized_pts.Count-1] == this.discretized_pts[discretized_pts.Count - 1]) ||
                (other_bz.discretized_pts[0] == this.discretized_pts[discretized_pts.Count - 1] && other_bz.discretized_pts[0] == this.discretized_pts[discretized_pts.Count - 1]))
            {
                if ((other_bz.poly_count == this.poly_count) && (this.selection_check_pts.Length == other_bz.selection_check_pts.Length))
                {
                    // Now check wehter the control points are matching
                    for (int i = 0; i < bezier_cntrl_pts.Count; i++)
                    {
                        if (this.bezier_cntrl_pts[i].Equals(other_bz.bezier_cntrl_pts[i]) == false)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool Equals(int other_bz_id)
        {
            if (this.bezier_id == other_bz_id)
            {
                // Bezier ids are matching
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            // Hashcode takes in the combination of Arc id, chord ends, radius & orientation
            return HashCode.Combine(this.bezier_id, this.bezier_cntrl_pts);
        }
    }


    // Custom comparer for the Bezier_store class
    class BeziersComparer : IEqualityComparer<beziers_store>
    {
        // Beziers are equal if their bezier_ids are equal or both beziers have same point id (either direction) also all the control points.
        public bool Equals(beziers_store first_bz, beziers_store second_bz)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(first_bz, second_bz)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(first_bz, null) || Object.ReferenceEquals(second_bz, null))
                return false;

            //Check whether the beziers are equal.
            return first_bz.Equals(second_bz);
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(beziers_store other_bz)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(other_bz, null)) return 0;

            //Calculate the hash code for the product.
            return other_bz.GetHashCode();
        }
    }




}
