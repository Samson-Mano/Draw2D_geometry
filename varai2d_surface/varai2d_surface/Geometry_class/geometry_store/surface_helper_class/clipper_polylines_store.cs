using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.Drawing_area;
using varai2d_surface.global_static;
using System.Windows.Forms;


namespace varai2d_surface.Geometry_class.geometry_store.surface_helper_class
{
    [Serializable]
    public class clipper_polylines_store
    {
        private int _poly_id;
        private int _poly_start_id;
        private int _poly_end_id;
        private int _ptype; //1 - line, 2- Arc, 3- Bezier
        private clipper_polypts_store start_pt; // start points
        private clipper_polypts_store mid_pt; // Mid points
        private clipper_polypts_store end_pt; // End points
        private List<clipper_polypts_store> intermediate_pts = new List<clipper_polypts_store>(); // From start point to end point (excluding the start and end)
        private GraphicsPath _poly_gpath = new GraphicsPath();


        private int _start_w_edge;
        private int _start_ccw_edge;

        private int _end_w_edge;
        private int _end_ccw_edge;

        private HashSet<int> _loop_ccw_poly_ids = new HashSet<int>();
        private HashSet<int> _loop_ccw_endpt_ids = new HashSet<int>();
        private List<clipper_polypts_store> _ccw_loop_pts = new List<clipper_polypts_store>();

        private HashSet<int> _loop_w_poly_ids = new HashSet<int>();
        private HashSet<int> _loop_w_endpt_ids = new HashSet<int>();
        private List<clipper_polypts_store> _w_loop_pts = new List<clipper_polypts_store>();

        public int poly_id { get { return this._poly_id; } }

        public int poly_start_id { get { return this._poly_start_id; } }

        public int poly_end_id { get { return this._poly_end_id; } }

        public int poly_type { get { return this._ptype; } }

        public clipper_polypts_store start_offset_pt { get { return this.intermediate_pts[1]; } }

        public clipper_polypts_store end_offset_pt { get { return this.intermediate_pts[this.intermediate_pts.Count - 2]; } }

        public int start_w_edge { get { return this._start_w_edge; } }

        public int start_ccw_edge { get { return this._start_ccw_edge; } }

        public int end_w_edge { get { return this._end_w_edge; } }

        public int end_ccw_edge { get { return this._end_ccw_edge; } }

        public HashSet<clipper_polypts_store> get_all_poly_pts
        {
            get
            {
                HashSet<clipper_polypts_store> all_ply_line_pts = new HashSet<clipper_polypts_store>();
                all_ply_line_pts.Add(start_pt);
                all_ply_line_pts.UnionWith(intermediate_pts);
                all_ply_line_pts.Add(end_pt);
                return all_ply_line_pts;
            }
        }

        private HashSet<PointF> get_all_Points
        {
            get
            {
                HashSet<PointF> pt_list = new HashSet<PointF>();
                pt_list.Add(start_pt.get_pt);

                foreach (clipper_polypts_store pt in intermediate_pts)
                {
                    pt_list.Add(pt.get_pt);
                }

                pt_list.Add(end_pt.get_pt);

                return pt_list;
            }
        }

        public HashSet<int> loop_ccw_poly_ids { get { return this._loop_ccw_poly_ids; } }

        public HashSet<int> loop_ccw_endpt_ids { get { return this._loop_ccw_endpt_ids; } }

        public List<clipper_polypts_store> ccw_loop_pts { get { return this._ccw_loop_pts; } }

        public HashSet<int> loop_w_poly_ids { get { return this._loop_w_poly_ids; } }

        public HashSet<int> loop_w_endpt_ids { get { return this._loop_w_endpt_ids; } }

        public List<clipper_polypts_store> w_loop_pts { get { return this._w_loop_pts; } }

        public clipper_polylines_store(int tp_id, int ts_id, int te_id, int t_ptype, double s_x, double s_y, List<PointF> t_inter_pts, double e_x, double e_y)
        {
            this._poly_id = tp_id;
            this._poly_start_id = ts_id;
            this._poly_end_id = te_id;

            // Set the type of polyline
            this._ptype = t_ptype;
            // initialize start and end points
            this.start_pt = new clipper_polypts_store(ts_id, s_x, s_y);
            this.end_pt = new clipper_polypts_store(te_id, e_x, e_y);

            // initialize the intermediate points
            clipper_polypts_store prev_pt = new clipper_polypts_store(-100, s_x, s_y);

            foreach (PointF int_pts in t_inter_pts)
            {
                clipper_polypts_store temp_pt = new clipper_polypts_store(-100, int_pts.X, int_pts.Y);
                // Check with the previously added point 
                // Degenerate case when the points are too close because of splitting 
                if (temp_pt.Equals(prev_pt) == false)
                {
                    intermediate_pts.Add(temp_pt);
                    prev_pt = new clipper_polypts_store(-100, temp_pt.x, temp_pt.y);
                }
            }

            // Check whether the last point is too close to the end point
            if (this.end_pt.Equals(intermediate_pts[intermediate_pts.Count - 1]) == true)
            {
                intermediate_pts.Remove(this.end_pt);
            }

            // Set the mid point
            int mid_index = (int)(intermediate_pts.Count * 0.5f);
            this.mid_pt = intermediate_pts[mid_index];

            // Set the polylines graphics path
            this._poly_gpath = new GraphicsPath();
            this._poly_gpath.AddLine(this.start_pt.get_pt, this.intermediate_pts[0].get_pt);
            for (int i = 1; i < intermediate_pts.Count; i++)
            {
                this._poly_gpath.AddLine(this.intermediate_pts[i - 1].get_pt, this.intermediate_pts[i].get_pt);
            }
            this._poly_gpath.AddLine(this.intermediate_pts[intermediate_pts.Count - 1].get_pt, this.end_pt.get_pt);

        }

        public void set_polygon_edges(HashSet<clipper_polylines_store> other_poly_lines)
        {
            // Set the offsets for the polylines
            // Call after adding all polylines

            HashSet<clipper_polylines_store> connected_to_start_pt_plys = new HashSet<clipper_polylines_store>();
            HashSet<clipper_polylines_store> connected_to_end_pt_plys = new HashSet<clipper_polylines_store>();

            SortedList<double, int> start_connected_poly_id = new SortedList<double, int>();
            SortedList<double, int> end_connected_poly_id = new SortedList<double, int>();

            double current_angle = 0.0;
            // step 1:
            // Find all the polylines attached to the start point
            foreach (clipper_polylines_store s_ply in other_poly_lines)
            {
                if (s_ply.is_attached_to_ptid(this._poly_start_id) == true)
                {
                    // This polys start point is connected to either start or end of other poly
                    PointF s_ply_endpt;
                    if (s_ply.poly_start_id == this._poly_start_id)
                    {
                        s_ply_endpt = s_ply.start_offset_pt.get_pt;
                    }
                    else
                    {
                        s_ply_endpt = s_ply.end_offset_pt.get_pt;
                    }

                    // Add in an ordered way
                    current_angle = get_angle_between(this.start_pt.get_pt, this.start_offset_pt.get_pt, s_ply_endpt);
                    connected_to_start_pt_plys.Add(s_ply);
                    start_connected_poly_id.Add(current_angle, s_ply.poly_id);
                }
            }

            // Find all the polylines attched to the end point
            foreach (clipper_polylines_store e_ply in other_poly_lines)
            {
                if (e_ply.is_attached_to_ptid(this._poly_end_id) == true)
                {
                    // This polys start point is connected to either start or end of other poly

                    PointF e_ply_endpt;
                    if (e_ply.poly_start_id == this._poly_end_id)
                    {
                        e_ply_endpt = e_ply.start_offset_pt.get_pt;
                    }
                    else
                    {
                        e_ply_endpt = e_ply.end_offset_pt.get_pt;
                    }
                    current_angle = get_angle_between(this.end_pt.get_pt, this.end_offset_pt.get_pt, e_ply_endpt);
                    connected_to_end_pt_plys.Add(e_ply);
                    end_connected_poly_id.Add(current_angle, e_ply.poly_id);
                }
            }

            // Step 2: Get the First from the Left and First from the right at both start and end pt
            this._start_w_edge = start_connected_poly_id.First().Value;
            this._start_ccw_edge = start_connected_poly_id.Last().Value;

            this._end_w_edge = end_connected_poly_id.First().Value;
            this._end_ccw_edge = end_connected_poly_id.Last().Value;
        }

        public void set_polygon_loop(HashSet<clipper_polylines_store> other_poly_lines)
        {
            this._loop_ccw_poly_ids = new HashSet<int>();
            this._loop_w_poly_ids = new HashSet<int>();

            // get all the points in counter Clock wise loop
            this._ccw_loop_pts = new List<clipper_polypts_store>();

            int temp_pt_id = poly_end_id;
            int nxt_p_index = end_ccw_edge;
            this._loop_ccw_poly_ids.Add(nxt_p_index);
            this._loop_ccw_endpt_ids.Add(temp_pt_id);

            while (nxt_p_index != this.poly_id)
            {
                // Get the ccw polyline attached to the end point
                clipper_polylines_store attched_ply = other_poly_lines.Last(obj => obj.Equals(nxt_p_index));
                clipper_polypts_store attached_ply_end_pt = attched_ply.get_other_pt(temp_pt_id);

                // Get the other point of the polyline and add to list
                temp_pt_id = attached_ply_end_pt.p_id;
                this._loop_ccw_endpt_ids.Add(temp_pt_id);

                // Index of the next line attached in ccw direction
                if (temp_pt_id == attched_ply.poly_end_id)
                {
                    // attached in reverse direction because the end is attached to end
                    this._ccw_loop_pts.AddRange(get_line_poly_points(attched_ply, true));
                    nxt_p_index = attched_ply.end_ccw_edge;
                    this._loop_ccw_poly_ids.Add(nxt_p_index);
                }
                else if (temp_pt_id == attched_ply.poly_start_id)
                {
                    // Attached in positive direction because the end is attached to the start
                    this._ccw_loop_pts.AddRange(get_line_poly_points(attched_ply, false));
                    nxt_p_index = attched_ply.start_ccw_edge;
                    this._loop_ccw_poly_ids.Add(nxt_p_index);
                }
            }
            // Add the last loop which is the start
            this._ccw_loop_pts.AddRange(get_line_poly_points(this, true));

            // get all the points in counter Clock wise loop
            this._w_loop_pts = new List<clipper_polypts_store>();

            temp_pt_id = poly_end_id;
            nxt_p_index = end_w_edge;
            this._loop_w_poly_ids.Add(nxt_p_index);
            this._loop_w_endpt_ids.Add(temp_pt_id);

            while (nxt_p_index != this.poly_id)
            {
                // Get the ccw polyline attached to the end point
                clipper_polylines_store attched_ply = other_poly_lines.Last(obj => obj.Equals(nxt_p_index));
                clipper_polypts_store attached_ply_end_pt = attched_ply.get_other_pt(temp_pt_id);

                // Get the other point of the polyline and add to list
                temp_pt_id = attached_ply_end_pt.p_id;
                this._loop_w_endpt_ids.Add(temp_pt_id);

                // Index of the next line attached in ccw direction
                if (temp_pt_id == attched_ply.poly_end_id)
                {
                    // attached in reverse direction because the end is attached to end
                    this._w_loop_pts.AddRange(get_line_poly_points(attched_ply, true));
                    nxt_p_index = attched_ply.end_w_edge;
                    this._loop_w_poly_ids.Add(nxt_p_index);
                }
                else if (temp_pt_id == attched_ply.poly_start_id)
                {
                    // Attached in positive direction because the end is attached to the start
                    this._w_loop_pts.AddRange(get_line_poly_points(attched_ply, false));
                    nxt_p_index = attched_ply.start_w_edge;
                    this._loop_w_poly_ids.Add(nxt_p_index);
                }
            }
            this._w_loop_pts.AddRange(get_line_poly_points(this, true));


        }

        private HashSet<clipper_polypts_store> get_line_poly_points(clipper_polylines_store ply, bool positive_dir)
        {
            // Positive direction (start -> End)
            if (positive_dir == true)
            {
                return ply.get_all_poly_pts;
            }
            else
            {
                // HashSet<PointF> t_pt_list =  ply.get_all_Points.Reverse().ToHashSet();
                return ply.get_all_poly_pts.Reverse().ToHashSet();
            }
        }


        private double get_signed_inner_angle(PointF apt, PointF spt, PointF ept)
        {
            // Create vector 1
            double norm;
            double v1_x, v1_y;
            v1_x = spt.X - apt.X;
            v1_y = spt.Y - apt.Y;
            norm = Math.Sqrt((v1_x * v1_x) + (v1_y * v1_y));
            // Normalize the vector
            v1_x = v1_x / norm;
            v1_y = v1_y / norm;

            // Create vector 2
            double v2_x, v2_y;
            v2_x = ept.X - apt.X;
            v2_y = ept.Y - apt.Y;
            norm = Math.Sqrt((v2_x * v2_x) + (v2_y * v2_y));
            // Normalize the vector
            v2_x = v2_x / norm;
            v2_y = v2_y / norm;

            // Find angle
            double t_sin, t_cos;
            t_sin = (v1_x * v2_y) - (v2_x * v1_y);
            t_cos = (v1_x * v2_x) + (v1_y * v2_y);
            double inner_product_deg = Math.Atan2(t_sin, t_cos) * (180 / Math.PI);

            return inner_product_deg;
        }

        private double get_angle_between(PointF orgin_pt, PointF start_pt, PointF end_pt)
        {
            double sweep_angle = (gfunctions.angle_between_2lines(orgin_pt, start_pt, orgin_pt, end_pt, true));
            if (gfunctions.ordered_orientation(start_pt, end_pt, orgin_pt) > 0)
            {
                sweep_angle = 360 - sweep_angle;
            }
            return sweep_angle;
        }

        public void paint_polylines(Graphics gr0)
        {
            foreach (clipper_polypts_store pt in intermediate_pts)
            {
                gr0.DrawEllipse(Pens.Black, (float)(pt.x - 2), (float)(pt.y - 2), 4, 4);
            }

        }

        public bool is_selfintersecting()
        {
            // Checked only for beziers (lines and arcs dont self intersect)
            // temporary list containing all points
            List<clipper_polypts_store> all_ply_line_pts = new List<clipper_polypts_store>(this.get_all_poly_pts);

            int i = 0;
            PointF ln_s_pt, ln_e_pt;
            ln_s_pt = new PointF(all_ply_line_pts[i].get_pt.X, all_ply_line_pts[i].get_pt.Y);

            for (i = 2; i < (all_ply_line_pts.Count); i++)
            {
                // Form the main line
                ln_e_pt = new PointF(all_ply_line_pts[i].get_pt.X, all_ply_line_pts[i].get_pt.Y);

                for (int j = (i + 3); j < all_ply_line_pts.Count; j++)
                {
                    // check with other line
                    PointF ol_p1 = new PointF(all_ply_line_pts[j - 2].get_pt.X, all_ply_line_pts[j - 2].get_pt.Y);
                    PointF ol_p2 = new PointF(all_ply_line_pts[j].get_pt.X, all_ply_line_pts[j].get_pt.Y);

                    bool is_intersect = false;
                    // Check intersection
                    is_intersect = is_lines_intersect(ln_s_pt, ln_e_pt, ol_p1, ol_p2);

                    if (is_intersect == true)
                    {
                        return true;
                    }
                }

                ln_s_pt = new PointF(all_ply_line_pts[i - 1].get_pt.X, all_ply_line_pts[i - 1].get_pt.Y);
            }


            return false;
        }

        public bool is_intersecting(clipper_polylines_store other_poly_lines)
        {
            // Brute force intersection check
            List<clipper_polypts_store> this_ply_line_pts = new List<clipper_polypts_store>(this.get_all_poly_pts);
            List<clipper_polypts_store> other_ply_line_pts = new List<clipper_polypts_store>(other_poly_lines.get_all_poly_pts);

            int i = 0;
            PointF this_pln_s_pt, this_pln_e_pt;

            // Set the first point
            this_pln_s_pt = new PointF(this_ply_line_pts[i].get_pt.X, this_ply_line_pts[i].get_pt.Y);

            for (i = 2; i < (this_ply_line_pts.Count); i++)
            {
                // Form the main line (from this polyline)
                this_pln_e_pt = new PointF(this_ply_line_pts[i].get_pt.X, this_ply_line_pts[i].get_pt.Y);

                int j = 0;
                PointF other_pln_s_pt, other_pln_e_pt;

                // set the other polyline first point
                other_pln_s_pt = new PointF(other_ply_line_pts[j].get_pt.X, other_ply_line_pts[j].get_pt.Y);

                for (j = 2; j < other_ply_line_pts.Count; j++)
                {
                    // check with other line
                    other_pln_e_pt = new PointF(other_ply_line_pts[j].get_pt.X, other_ply_line_pts[j].get_pt.Y);


                    bool is_intersect = false;
                    // Check intersection
                    is_intersect = is_lines_intersect(this_pln_s_pt, this_pln_e_pt, other_pln_s_pt, other_pln_e_pt);

                    if (is_intersect == true)
                    {
                        return true;
                    }
                    other_pln_s_pt = new PointF(other_ply_line_pts[j - 1].get_pt.X, other_ply_line_pts[j - 1].get_pt.Y);
                }
                this_pln_s_pt = new PointF(this_ply_line_pts[i - 1].get_pt.X, this_ply_line_pts[i - 1].get_pt.Y);
            }

            return false;
        }

        private bool is_lines_intersect(PointF p1, PointF q1, PointF p2, PointF q2)
        {
            //// Skip if the lines are connected (Lines)
            if (p1.Equals(q2) == true || q1.Equals(p2) == true || p1.Equals(p2) == true || q1.Equals(q2) == true)
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
                // colinear cases are not considered !!!!
            }
            return false; // Doesn't fall in any of the above cases
        }

        private int ordered_orientation(PointF p, PointF q, PointF r)
        {
            // To find orientation of ordered triplet (p, q, r).
            // The function returns following values
            // 0 --> p, q and r are collinear
            // 1 --> Clockwise
            // -1 --> Counterclockwise

            double val = (((q.Y - p.Y) * (r.X - q.X)) - ((q.X - p.X) * (r.Y - q.Y)));

            if (Math.Round(val, 2) == 0) return 0; // collinear

            return (val > 0) ? 1 : -1; // clock or counterclock wise

        }

        public bool is_attached_to_ptid(int pt_id)
        {
            if (this._poly_start_id == pt_id || this._poly_end_id == pt_id)
            {
                return true;
            }
            return false;
        }

        public clipper_polypts_store get_other_pt(int id)
        {
            // Returns the other end point of the polyline
            if (this.poly_start_id == id)
            {
                return end_pt;
            }
            else if (this.poly_end_id == id)
            {
                return start_pt;
            }
            return null;
        }

        public clipper_polypts_store get_offset_pt(int pt_id)
        {
            if (this._poly_start_id == pt_id)
            {
                return start_offset_pt;
            }
            else if (this._poly_end_id == pt_id)
            {
                return end_offset_pt;
            }

            // Do not call before checking is attached!!!
            return null;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as clipper_polylines_store);
        }

        public bool Equals(clipper_polylines_store other_poly)
        {
            // Check 1 (Poly Ids)
            if (this.Equals(other_poly.poly_id))
            {
                // poly ids are matching
                return true;
            }
            return false;
        }

        public bool Equals(int other_poly_id)
        {
            if (this._poly_id == other_poly_id)
            {
                // Poly ids are matching
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.poly_id, 17);
        }

    }
}
