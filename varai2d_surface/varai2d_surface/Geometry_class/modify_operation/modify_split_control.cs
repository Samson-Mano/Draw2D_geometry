using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.Geometry_class.geometry_store;
using varai2d_surface.global_static;
using varai2d_surface.Drawing_area;
using System.Windows.Forms;

namespace varai2d_surface.Geometry_class.modify_operation
{
    public class modify_split_control
    {
        private workarea_control wkc_obj;

        // Points
        List<PointF> interim_click_pts;

        int selected_line_count = 0;
        int selected_arc_count = 0;
        int selected_bezier_count = 0;

        public void split_member(workarea_control wkc)
        {
            this.wkc_obj = wkc;

            //// Split member
            //// Displays the MessageBox. if more than 1 lines are splitted
            //if (this.histU_obj.interim_obj.selected_lines.Count > 1)
            //{
            //    DialogResult result = MessageBox.Show("Split all segments?", "Split", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            //    // Check the user result
            //    if (result == DialogResult.No)
            //    {
            //        this.histU_obj.cancel_operation();
            //        return;
            //    }
            //}

            // Get the split parameter at t
            double param_t = 0.5;
            if (gfunctions.InputBox("Paramterization split", "Split the line at t [0,1] = ", ref param_t) == DialogResult.Cancel)
            {
                this.wkc_obj.cancel_operation();
                return;
            }


            if (param_t <= 0.05 || param_t >= 0.95)
            {
                // invalid split location
                this.wkc_obj.cancel_operation();
                return;
            }

            // Split the lines
            // Get all the lines and make a tuple
            // Points
            this.interim_click_pts = new List<PointF>(); // [0,1], [2,3], [4,5] ...

            this.selected_line_count = 0;
            this.selected_arc_count = 0;
            this.selected_bezier_count = 0;

            // Set split lines
            if (this.wkc_obj.interim_obj.selected_lines.Count > 0)
            {
                set_split_lines(param_t);
            }

            // Set split arcs
            if (this.wkc_obj.interim_obj.selected_arcs.Count > 0)
            {
                set_split_arcs(param_t);
            }

            // Set split beziers
            if (this.wkc_obj.interim_obj.selected_beziers.Count > 0)
            {
                set_split_beziers(param_t);
            }

            // Delete the member
            (new delete_operation_control()).delete_member(this.wkc_obj);

            // Apply split lines
            if (this.selected_line_count > 0)
            {
                apply_split_of_lines();
            }

            // Apply split arcs
            if (this.selected_arc_count > 0)
            {
                apply_split_of_arcs();
            }

            // Apply split beziers
            if (this.selected_bezier_count > 0)
            {
                apply_split_of_beziers();
            }

            // complete operation
            // this.histU_obj.cancel_operation();
        }

        private void set_split_lines(double param_t)
        {
            List<Tuple<PointF, PointF, PointF>> temp_store_selected_lines = new List<Tuple<PointF, PointF, PointF>>();

            // Add the selected lines to tuple
            foreach (lines_store ln in this.wkc_obj.interim_obj.selected_lines)
            {
                PointF ln_p = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(ln.pt_start_id)).get_point;
                PointF ln_q = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(ln.pt_end_id)).get_point;

                // New point at parameter t
                PointF pt_at_paramt = ln.get_point_at_t(param_t);

                // Add to the tuple
                temp_store_selected_lines.Add(new Tuple<PointF, PointF, PointF>(ln_p, pt_at_paramt, ln_q));
            }

            // Create interim click points, Length & angle
            this.selected_line_count = 0;
            foreach (Tuple<PointF, PointF, PointF> split_lines in temp_store_selected_lines)
            {
                // Line 1 
                interim_click_pts.Add(split_lines.Item1);
                interim_click_pts.Add(split_lines.Item2);

                this.selected_line_count++;

                // Line 2
                interim_click_pts.Add(split_lines.Item2);
                interim_click_pts.Add(split_lines.Item3);

                this.selected_line_count++;
            }
        }

        private void apply_split_of_lines()
        {
            // Split the lines
            for (int i = 0; i < this.selected_line_count; i++)
            {
                int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();
                this.wkc_obj.snap_obj.clear_temp_point(); // clear temp points to get the current node

                points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[i * 2], this.wkc_obj.geom_obj);
                points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[(i * 2) + 1], this.wkc_obj.geom_obj);

                // Add line
                this.wkc_obj.geom_obj.add_line(member_id, s_pt, e_pt);
            }
        }

        private void set_split_arcs(double param_t)
        {
            List<Tuple<PointF, PointF, PointF, PointF, PointF, PointF>> temp_store_selected_arcs = new List<Tuple<PointF, PointF, PointF, PointF, PointF, PointF>>();

            // Add the selected arc to tuple
            foreach (arcs_store arc in this.wkc_obj.interim_obj.selected_arcs)
            {
                // Chord end points
                PointF chord_p = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(arc.pt_chord_start_id)).get_point;
                PointF chord_q = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(arc.pt_chord_end_id)).get_point;
                // new chord point at parameter t
                PointF new_chord_pt = arc.get_point_at_t(param_t);
                // crown point 1 and crown point 2
                PointF crown_pt_p = arc.get_point_at_t((param_t * 0.5));
                PointF crown_pt_q = arc.get_point_at_t(param_t + ((1 - param_t) * 0.5));
                // center point
                PointF center_pt = arc.arc_center_pt;

                // Add to the tuple
                temp_store_selected_arcs.Add(new Tuple<PointF, PointF, PointF, PointF, PointF, PointF>(chord_p, new_chord_pt, chord_q, crown_pt_p, crown_pt_q, center_pt));
            }

            // Create interim click points, Length & angle
            this.selected_arc_count = 0;
            foreach (Tuple<PointF, PointF, PointF, PointF, PointF, PointF> split_arcs in temp_store_selected_arcs)
            {
                // Arc 1 
                // Chord points
                interim_click_pts.Add(split_arcs.Item1);
                interim_click_pts.Add(split_arcs.Item2);

                // Control point and Center point
                interim_click_pts.Add(split_arcs.Item4);
                interim_click_pts.Add(split_arcs.Item6);

                this.selected_arc_count++;

                // Arc 2
                // Chord points
                interim_click_pts.Add(split_arcs.Item2);
                interim_click_pts.Add(split_arcs.Item3);

                // Control point and Center point
                interim_click_pts.Add(split_arcs.Item5);
                interim_click_pts.Add(split_arcs.Item6);

                this.selected_arc_count++;
            }

        }

        private void apply_split_of_arcs()
        {
            int click_index_shift = this.selected_line_count * 2;

            // Split the arcs
            for (int i = 0; i < this.selected_arc_count; i++)
            {
                int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();
                this.wkc_obj.snap_obj.clear_temp_point(); // clear temp points to get the current node

                points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[click_index_shift + (i * 4)], this.wkc_obj.geom_obj);
                points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[click_index_shift + ((i * 4) + 1)], this.wkc_obj.geom_obj);

                // Add arc
                this.wkc_obj.geom_obj.add_arc(member_id, s_pt, e_pt,
                    interim_click_pts[click_index_shift + ((i * 4) + 2)], interim_click_pts[click_index_shift + ((i * 4) + 3)]);
            }
        }

        private void set_split_beziers(double param_t)
        {
            List<PointF> bezier_pts = new List<PointF>();
            // Add the bezier points to list
            foreach (PointF cpts in this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).bezier_cntrl_pts)
            {
                bezier_pts.Add(cpts);
            }

            List<PointF> bezier_split_pts_f = new List<PointF>();
            List<PointF> bezier_split_pts_s = new List<PointF>();
            PointF e_pt;
            e_pt = getCasteljauPoint_allpoints(bezier_pts, ref bezier_split_pts_f, bezier_pts.Count - 1, 0, param_t);
            bezier_split_pts_f.Add(e_pt);

            bezier_pts.Reverse();
            e_pt = getCasteljauPoint_allpoints(bezier_pts, ref bezier_split_pts_s, bezier_pts.Count - 1, 0, (1 - param_t));
            bezier_split_pts_s.Add(e_pt);
            bezier_split_pts_s.Reverse();

            // Create interim click points, Length & angle
            this.selected_bezier_count = 0;
            int i = 0;

            // First segment control points
            for (i = 0; i < bezier_split_pts_f.Count ; i++)
            {
                // Click points
                interim_click_pts.Add(bezier_split_pts_f[i]);
            }
            this.selected_bezier_count++;

            // Second segment control points
            for (i = 0; i < bezier_split_pts_s.Count; i++)
            {
                // Click points
                interim_click_pts.Add(bezier_split_pts_s[i]);
            }
            this.selected_bezier_count++;
        }

        private PointF getCasteljauPoint_allpoints(List<PointF> cntrl_pts, ref List<PointF> split_pts, int r, int i, double t)
        {
            if (r == 0) return cntrl_pts[i];

            PointF p1 = getCasteljauPoint_allpoints(cntrl_pts, ref split_pts, r - 1, i, t);
            PointF p2 = getCasteljauPoint_allpoints(cntrl_pts, ref split_pts, r - 1, i + 1, t);

            if (i == 0)
            {
                // CasteljauPoint Algorithm to get all the intermediate control points
                split_pts.Add(p1);
            }
            return new PointF((float)((1 - t) * p1.X + t * p2.X), (float)((1 - t) * p1.Y + t * p2.Y));
        }


        private void apply_split_of_beziers()
        {
            // Multiple split is not enabled for beziers (need to work on it) 
            int bezier_index = (int)((float)interim_click_pts.Count / 2.0f);

            // Split the beziers
            for (int i = 0; i < this.selected_bezier_count; i++)
            {
                // Add control points to the list
                List<PointF> cntrl_pts = new List<PointF>();
                for (int j = (i*bezier_index); j < ((i+1)*bezier_index); j++)
                {
                    cntrl_pts.Add(interim_click_pts[j]);
                }

                int poly_count = bezier_index;

                int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();
                // Need special consideration to add the end points (to get the point ids to allign with order !!)
                this.wkc_obj.snap_obj.clear_temp_point(); // clear temp points to get the current node
                points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(cntrl_pts[0], this.wkc_obj.geom_obj);
                points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(cntrl_pts[poly_count - 1], this.wkc_obj.geom_obj);

                // Add Bezier
                this.wkc_obj.geom_obj.add_bezier(member_id, poly_count,
                    s_pt,
                    e_pt, cntrl_pts);
            }

        }
    }
}
