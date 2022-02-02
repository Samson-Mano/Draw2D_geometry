using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.global_static;
using varai2d_surface.Drawing_area;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using varai2d_surface.Geometry_class.geometry_store;

namespace varai2d_surface.Geometry_class.modify_operation
{
    public class modify_intersect_control
    {
        private workarea_control wkc_obj;

        // Points of individual segments
        List<PointF> interim_click_pts_segment_A1;
        List<PointF> interim_click_pts_segment_A2;
        List<PointF> interim_click_pts_segment_B1;
        List<PointF> interim_click_pts_segment_B2;

        public void intersect_member(workarea_control wkc)
        {
            this.wkc_obj = wkc;

            if (this.wkc_obj.interim_obj.selected_lines.Count == 2)
            {
                // Line - Line intersection
                line_line_intersection();
                return;
            }

            if (this.wkc_obj.interim_obj.selected_lines.Count == 1 && this.wkc_obj.interim_obj.selected_arcs.Count == 1)
            {
                // Line - Arc intersection
                arc_line_intersection();
                return;
            }

            if (this.wkc_obj.interim_obj.selected_arcs.Count == 2)
            {
                // Arc - Arc intersection
                arc_arc_intersection();
                return;
            }

            if (this.wkc_obj.interim_obj.selected_beziers.Count == 2)
            {
                // Bezier - Bezier intersection
                bezier_bezier_intersection();
            }

            if (this.wkc_obj.interim_obj.selected_beziers.Count == 1 && this.wkc_obj.interim_obj.selected_arcs.Count == 1)
            {
                // Bezier - Arc intersection
                bezier_arc_intersection();
            }

            if (this.wkc_obj.interim_obj.selected_beziers.Count == 1 && this.wkc_obj.interim_obj.selected_lines.Count == 1)
            {
                // Bezier - Line intersection
                bezier_line_intersection();
            }
        }

        private void line_line_intersection()
        {
            // Intersect member
            // Line - Line intersection
            // Get all the lines and make a tuple
            List<Tuple<int, PointF, PointF>> temp_store_selected_lines = new List<Tuple<int, PointF, PointF>>();
            HashSet<lines_store> lines_broken = new HashSet<lines_store>();

            foreach (lines_store ln in this.wkc_obj.interim_obj.selected_lines)
            {
                PointF ln_p = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(ln.pt_start_id)).get_point;
                PointF ln_q = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(ln.pt_end_id)).get_point;

                // Add to the tuple
                temp_store_selected_lines.Add(new Tuple<int, PointF, PointF>(ln.line_id, ln_p, ln_q));
            }

            bool is_intersection = false;

            PointF ln1_p = new PointF(0, 0), ln1_q = new PointF(0, 0);
            PointF ln2_p = new PointF(0, 0), ln2_q = new PointF(0, 0);

            // Get the points of lines
            ln1_p = temp_store_selected_lines[0].Item2;
            ln1_q = temp_store_selected_lines[0].Item3;

            ln2_p = temp_store_selected_lines[1].Item2;
            ln2_q = temp_store_selected_lines[1].Item3;
            // Check intesection
            is_intersection = gfunctions.Is_lines_intersect(ln1_p, ln1_q, ln2_p, ln2_q);


            // Line i & j intersects
            if (is_intersection == true)
            {
                // Get the intersection point
                PointF intersection_pt = gfunctions.intersection_point(ln1_p, ln1_q, ln2_p, ln2_q);


                if (gfunctions.is_intersection_line_valid(ln1_p, intersection_pt) == false ||
                    gfunctions.is_intersection_line_valid(ln1_q, intersection_pt) == false ||
                        gfunctions.is_intersection_line_valid(ln2_p, intersection_pt) == false ||
                    gfunctions.is_intersection_line_valid(ln2_q, intersection_pt) == false)
                {
                    // something went wrong !! usually the intersection lines are too small
                    is_intersection = false;
                    return;
                }
                double ln_t0 = gfunctions.get_param_t_for_pt(ln1_p, ln1_q, intersection_pt);
                double ln_t1 = gfunctions.get_param_t_for_pt(ln2_p, ln2_q, intersection_pt);

                set_split_lines(0, 0, ln_t0,ref interim_click_pts_segment_A1);
                set_split_lines(0, ln_t0, 1, ref interim_click_pts_segment_A2);
                set_split_lines(1, 0, ln_t1, ref interim_click_pts_segment_B1);
                set_split_lines(1, ln_t1, 1, ref interim_click_pts_segment_B2);
                // Delete the lines which are interseted ak.a broken into two pieces
                (new delete_operation_control()).delete_member(this.wkc_obj);

                apply_split_of_lines(interim_click_pts_segment_A1);
                apply_split_of_lines(interim_click_pts_segment_A2);
                apply_split_of_lines(interim_click_pts_segment_B1);
                apply_split_of_lines(interim_click_pts_segment_B2);
            }
        }

        private void arc_line_intersection()
        {
            // Intersect member
            // Arc - Line intersection
            // Get all the single line and make a tuple
            PointF ln_p = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(this.wkc_obj.interim_obj.selected_lines.ElementAt(0).pt_start_id)).get_point;
            PointF ln_q = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(this.wkc_obj.interim_obj.selected_lines.ElementAt(0).pt_end_id)).get_point;
            bool is_intersection = false;

            // Get the points of lines
            double chrd_s_t = 0.0;
            double chrd_range0 = 0.0, chrd_range1 = 0.0;
            PointF tngt_pt = new PointF(0, 0);

            for (int i = 0; i < 10; i++)
            {
                PointF chrd_p, chrd_q, tngt_pt_p, tngt_pt_q;
                double chrd_length, chrd_slope;

                // chord segment
                chrd_p = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t);
                chrd_q = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t + 0.1);
                chrd_length = gfunctions.get_length(chrd_p, chrd_q);
                chrd_slope = (chrd_q.Y - chrd_p.Y) / (chrd_q.X - chrd_p.X);

                // Chord segment intersection
                if (gfunctions.Is_lines_intersect(ln_p, ln_q, chrd_p, chrd_q) == true)
                {
                    chrd_range0 = chrd_s_t;
                    chrd_range1 = chrd_s_t + 0.1;
                    is_intersection = true;
                    break;
                }

                // tangent segment
                tngt_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t + 0.05);
                tngt_pt_p = new PointF((float)(tngt_pt.X - ((chrd_q.X - chrd_p.X) * 0.5)), (float)(tngt_pt.Y - ((chrd_q.Y - chrd_p.Y) * 0.5)));
                tngt_pt_q = new PointF((float)(tngt_pt.X + ((chrd_q.X - chrd_p.X) * 0.5)), (float)(tngt_pt.Y + ((chrd_q.Y - chrd_p.Y) * 0.5)));

                // Tangent segment intersection
                if (gfunctions.Is_lines_intersect(ln_p, ln_q, tngt_pt_p, tngt_pt_q) == true)
                {
                    chrd_range0 = chrd_s_t;
                    chrd_range1 = chrd_s_t + 0.1;
                    is_intersection = true;
                    break;
                }

                // chord segment
                chrd_p = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t + 0.05);
                chrd_q = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t + 0.15);
                chrd_length = gfunctions.get_length(chrd_p, chrd_q);
                chrd_slope = (chrd_q.Y - chrd_p.Y) / (chrd_q.X - chrd_p.X);

                // Chord segment intersection
                if (gfunctions.Is_lines_intersect(ln_p, ln_q, chrd_p, chrd_q) == true)
                {
                    chrd_range0 = chrd_s_t;
                    chrd_range1 = chrd_s_t + 0.1;
                    is_intersection = true;
                    break;
                }

                // tangent segment
                tngt_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t + 0.1);
                tngt_pt_p = new PointF((float)(tngt_pt.X - ((chrd_q.X - chrd_p.X) * 0.5)), (float)(tngt_pt.Y - ((chrd_q.Y - chrd_p.Y) * 0.5)));
                tngt_pt_q = new PointF((float)(tngt_pt.X + ((chrd_q.X - chrd_p.X) * 0.5)), (float)(tngt_pt.Y + ((chrd_q.Y - chrd_p.Y) * 0.5)));

                // Tangent segment intersection
                if (gfunctions.Is_lines_intersect(ln_p, ln_q, tngt_pt_p, tngt_pt_q) == true)
                {
                    chrd_range0 = chrd_s_t + 0.05;
                    chrd_range1 = chrd_s_t + 0.15;
                    is_intersection = true;
                    break;
                }

                chrd_s_t = chrd_s_t + 0.1;
            }

            // Arc intersection
            if (is_intersection == true)
            {
                // Ordered orientation
                chrd_s_t = chrd_range0;
                tngt_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t);
                int initial_orientation = gfunctions.ordered_orientation(ln_p, ln_q, tngt_pt);
                int current_orientation = 2;

                double iteration_t = Math.Sign(chrd_range1 - chrd_range0) * 0.001;

                while (current_orientation != 0 && Math.Abs(iteration_t) > (gvariables.epsilon_g / 100))
                {
                    chrd_s_t = chrd_s_t + iteration_t;
                    tngt_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t);
                    current_orientation = gfunctions.ordered_orientation(ln_p, ln_q, tngt_pt);

                    if (current_orientation != initial_orientation)
                    {
                        // change direction
                        initial_orientation = current_orientation;
                        iteration_t = (-iteration_t) / 2;
                    }
                }

                // new chord point at parameter t
                PointF new_chord_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(chrd_s_t);
                double ln_t = gfunctions.get_param_t_for_pt(ln_p, ln_q, new_chord_pt);

                // Intersected Line
                set_split_lines(0, 0, ln_t,ref interim_click_pts_segment_A1);
                set_split_lines(0, ln_t, 1,ref interim_click_pts_segment_A2);

                // Intersected Arc
                set_split_arcs(0, 0, chrd_s_t,ref interim_click_pts_segment_B1);
                set_split_arcs(0, chrd_s_t, 1,ref interim_click_pts_segment_B2);

                // Delete the lines which are intersected a.k.a broken into two pieces
                (new delete_operation_control()).delete_member(this.wkc_obj);

                apply_split_of_lines(interim_click_pts_segment_A1);
                apply_split_of_lines(interim_click_pts_segment_A2);
                apply_split_of_arcs(interim_click_pts_segment_B1);
                apply_split_of_arcs(interim_click_pts_segment_B2);

            }
        }

        private void arc_arc_intersection()
        {
            List<Tuple<PointF, double>> temp_store_selected_circles = new List<Tuple<PointF, double>>();

            foreach (arcs_store arc in this.wkc_obj.interim_obj.selected_arcs)
            {
                PointF arc_center_pt = arc.arc_center_pt;
                PointF arc_crown_pt = arc.arc_crown_pt;
                double arc_radius = gfunctions.get_length(arc_center_pt, arc_crown_pt);

                // Add to the tuple
                temp_store_selected_circles.Add(new Tuple<PointF, double>(arc_center_pt, arc_radius));
            }

            Tuple<bool, PointF, PointF> intersection_rslt = gfunctions.two_circle_intersection_pts(temp_store_selected_circles[0].Item1,
                temp_store_selected_circles[0].Item2,
                temp_store_selected_circles[1].Item1,
                temp_store_selected_circles[1].Item2);

            bool is_intersection = false;
            PointF intersection_pt1, intersection_pt2;

            is_intersection = intersection_rslt.Item1;
            intersection_pt1 = intersection_rslt.Item2;
            intersection_pt2 = intersection_rslt.Item3;

            // Arc 0 & 1 intersects
            if (is_intersection == true)
            {
                bool pt_in_arc = false;
                double p_t1 = 0, p_t2 = 0;

                // Check whether the intersection point 1 in arc1 & arc2
                if (is_orientation_match(0, intersection_pt1) == true)
                {
                    p_t1 = param_t_angle(0, intersection_pt1);
                    if (is_orientation_match(1, intersection_pt1) == true)
                    {
                        p_t2 = param_t_angle(1, intersection_pt1);
                        pt_in_arc = true;
                    }
                }

                if (pt_in_arc == false)
                {
                    // Check whether the intersection point 2 in arc1 & arc2
                    if (is_orientation_match(0, intersection_pt2) == true)
                    {
                        p_t1 = param_t_angle(0, intersection_pt2);
                        if (is_orientation_match(1, intersection_pt2) == true)
                        {
                            p_t2 = param_t_angle(1, intersection_pt2);
                            pt_in_arc = true;
                        }
                    }
                }

                // Return because the arc doesnt intersect
                if (pt_in_arc == false)
                {
                    return;
                }

                // Convert final intersection to chord parameter
             //   this.selected_arc_count = 0;
                set_split_arcs(0, 0, p_t1,ref interim_click_pts_segment_A1);
                set_split_arcs(0, p_t1, 1,ref interim_click_pts_segment_A2);
                set_split_arcs(1, 0, p_t2,ref interim_click_pts_segment_B1);
                set_split_arcs(1, p_t2, 1,ref interim_click_pts_segment_B2);

                // Delete the lines which are intersected a.k.a broken into two pieces
                (new delete_operation_control()).delete_member(this.wkc_obj);

                apply_split_of_arcs(interim_click_pts_segment_A1);
                apply_split_of_arcs(interim_click_pts_segment_A2);
                apply_split_of_arcs(interim_click_pts_segment_B1);
                apply_split_of_arcs(interim_click_pts_segment_B2);
            }

        }

        private void bezier_line_intersection()
        {
            // Bezier as polyline segment
            List<PointF> bezier_pts = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).discretized_pts;
            double bz_param_t = 1.0d / (double)(bezier_pts.Count()-1);
            double bz_int_param_t = 0.0;

            // Line segment
            List<PointF> line_pts = this.wkc_obj.interim_obj.selected_lines.ElementAt(0).discretized_pts;
            line_pts.Insert(0, this.wkc_obj.interim_obj.selected_lines.ElementAt(0).get_point_at_t(0));
            line_pts.Add(this.wkc_obj.interim_obj.selected_lines.ElementAt(0).get_point_at_t(1));
            double ln_param_t = 1.0d / (double)(line_pts.Count()-1);
            double ln_int_param_t = 0.0;

            // Intersection point
            PointF intersection_pt;

            bool is_intersecting = cradle_algorithm(bezier_pts,line_pts,ref bz_int_param_t,ref ln_int_param_t);

            // Exit if no intersection is found
            if (is_intersecting == false)
            {
                return;
            }

            // Convert the parameter t to bezier lines
            double f_ratio = 4.0d;

            PointF bz_pt1 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).get_point_at_t(bz_int_param_t - (bz_param_t / f_ratio));
            PointF bz_pt2 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).get_point_at_t(bz_int_param_t + (bz_param_t / f_ratio));

            // Convert the parameter t to lines
            PointF ln_pt1 = this.wkc_obj.interim_obj.selected_lines.ElementAt(0).get_point_at_t(ln_int_param_t - (ln_param_t / f_ratio));
            PointF ln_pt2 = this.wkc_obj.interim_obj.selected_lines.ElementAt(0).get_point_at_t(ln_int_param_t + (ln_param_t / f_ratio));

            // Find the intersection point
            intersection_pt = gfunctions.intersection_point(bz_pt1, bz_pt2, ln_pt1, ln_pt2);

            // Get the bezier intersection pt parameter
            bz_int_param_t = (bz_int_param_t - (bz_param_t / f_ratio)) + (gfunctions.get_param_t_for_pt(bz_pt1, bz_pt2, intersection_pt) * (bz_param_t / (f_ratio * 0.5d)));

            // Get the line intersection pt parameter
            ln_int_param_t = (ln_int_param_t - (ln_param_t / f_ratio)) + (gfunctions.get_param_t_for_pt(ln_pt1, ln_pt2, intersection_pt) * (ln_param_t / (f_ratio * 0.5d)));


            // Intersected Line
            set_split_lines(0, 0, ln_int_param_t,ref interim_click_pts_segment_A1);
            set_split_lines(0, ln_int_param_t, 1,ref interim_click_pts_segment_A2);

            // Intersected Bezier
            set_split_beziers(0, bz_int_param_t,ref interim_click_pts_segment_B1,ref interim_click_pts_segment_B2);

            // Delete the lines which are intersected a.k.a broken into two pieces
            (new delete_operation_control()).delete_member(this.wkc_obj);

            apply_split_of_lines(interim_click_pts_segment_A1);
            apply_split_of_lines(interim_click_pts_segment_A2);
            apply_split_of_beziers(interim_click_pts_segment_B1);
            apply_split_of_beziers(interim_click_pts_segment_B2);
        }

        private void bezier_arc_intersection()
        {
            // Bezier as polyline segment
            List<PointF> bezier_pts = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).discretized_pts;
            double bz_param_t = 1.0 / (double)(bezier_pts.Count()-1);
            double bz_int_param_t = 0.0;

            // Line segment
            List<PointF> arc_pts = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).discretized_pts;
            arc_pts.Insert(0, this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(0));
            arc_pts.Add(this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(1));
            double arc_param_t = 1.0 / (double)(arc_pts.Count()-1);
            double arc_int_param_t = 0.0;

            // Intersection point
            PointF intersection_pt;

            bool is_intersecting = cradle_algorithm(bezier_pts, arc_pts, ref bz_int_param_t, ref arc_int_param_t);


            // Exit if no intersection is found
            if (is_intersecting == false)
            {
                return;
            }

            // Convert the parameter t to bezier lines
            double f_ratio = 4.0d;

            PointF bz_pt1 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).get_point_at_t(bz_int_param_t - (bz_param_t / f_ratio));
            PointF bz_pt2 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).get_point_at_t(bz_int_param_t + (bz_param_t / f_ratio));

            // Convert the parameter t to lines
            PointF ln_pt1 = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(arc_int_param_t - (arc_param_t / f_ratio));
            PointF ln_pt2 = this.wkc_obj.interim_obj.selected_arcs.ElementAt(0).get_point_at_t(arc_int_param_t + (arc_param_t / f_ratio));

            // Find the intersection point
            intersection_pt = gfunctions.intersection_point(bz_pt1, bz_pt2, ln_pt1, ln_pt2);

            // Get the bezier intersection pt parameter
            bz_int_param_t = (bz_int_param_t - (bz_param_t / f_ratio)) + (gfunctions.get_param_t_for_pt(bz_pt1, bz_pt2, intersection_pt) * (bz_param_t / (f_ratio*0.5d)));

            // Get the line intersection pt parameter
            arc_int_param_t = (arc_int_param_t - (arc_param_t / f_ratio)) + (gfunctions.get_param_t_for_pt(ln_pt1, ln_pt2, intersection_pt) * (arc_param_t / (f_ratio*0.5d)));

            // Intersected Arc
            set_split_arcs(0, 0, arc_int_param_t,ref interim_click_pts_segment_A1);
            set_split_arcs(0, arc_int_param_t, 1,ref interim_click_pts_segment_A2);

            // Intersected Bezier
            set_split_beziers(0, bz_int_param_t,ref interim_click_pts_segment_B1,ref interim_click_pts_segment_B2);

            // Delete the lines which are intersected a.k.a broken into two pieces
            (new delete_operation_control()).delete_member(this.wkc_obj);

            apply_split_of_arcs(interim_click_pts_segment_A1);
            apply_split_of_arcs(interim_click_pts_segment_A2);
            apply_split_of_beziers(interim_click_pts_segment_B1);
            apply_split_of_beziers(interim_click_pts_segment_B2);
        }

        private void bezier_bezier_intersection()
        {
            // Bezier as polyline segment
            List<PointF> bezier_pts1 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).discretized_pts;
            double bz_param_t1 = 1.0 / (double)(bezier_pts1.Count()-1);
            double bz_int_param_t1 = 0.0;

            // Line segment
            List<PointF> bezier_pts2 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(1).discretized_pts;
            double bz_param_t2 = 1.0 / (double)(bezier_pts2.Count()-1);
            double bz_int_param_t2 = 0.0;

            // Intersection point
            PointF intersection_pt;

            bool is_intersecting = cradle_algorithm(bezier_pts1, bezier_pts2, ref bz_int_param_t1, ref bz_int_param_t2);

            // Exit if no intersection is found
            if (is_intersecting == false)
            {
                return;
            }

            // Convert the parameter t to first bezier lines
            double f_ratio = 4.0d;

            PointF bz_pt1 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).get_point_at_t(bz_int_param_t1 - (bz_param_t1 / f_ratio));
            PointF bz_pt2 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(0).get_point_at_t(bz_int_param_t1 + (bz_param_t1 / f_ratio));

            // Convert the parameter t to second bezier lines
            PointF ln_pt1 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(1).get_point_at_t(bz_int_param_t2 - (bz_param_t2 / f_ratio));
            PointF ln_pt2 = this.wkc_obj.interim_obj.selected_beziers.ElementAt(1).get_point_at_t(bz_int_param_t2 + (bz_param_t2 / f_ratio));

            // Find the intersection point
            intersection_pt = gfunctions.intersection_point(bz_pt1, bz_pt2, ln_pt1, ln_pt2);

            // Get the bezier intersection pt parameter
            bz_int_param_t1 = (bz_int_param_t1 - (bz_param_t1 / f_ratio)) + (gfunctions.get_param_t_for_pt(bz_pt1, bz_pt2, intersection_pt) * (bz_param_t1 / (f_ratio * 0.5d)));

            // Get the bezier intersection pt parameter
            bz_int_param_t2 = (bz_int_param_t2 - (bz_param_t2 / f_ratio)) + (gfunctions.get_param_t_for_pt(ln_pt1, ln_pt2, intersection_pt) * (bz_param_t2 / (f_ratio * 0.5d)));

            // Intersected Beziers
            set_split_beziers(0, bz_int_param_t1,ref interim_click_pts_segment_A1,ref interim_click_pts_segment_A2);
            set_split_beziers(1, bz_int_param_t2,ref interim_click_pts_segment_B1,ref interim_click_pts_segment_B2);

            // Delete the lines which are intersected a.k.a broken into two pieces
            (new delete_operation_control()).delete_member(this.wkc_obj);

            apply_split_of_beziers(interim_click_pts_segment_A1);
            apply_split_of_beziers(interim_click_pts_segment_A2);
            apply_split_of_beziers(interim_click_pts_segment_B1);
            apply_split_of_beziers(interim_click_pts_segment_B2);
        }

        private bool cradle_algorithm(List<PointF> poly_line1 , List<PointF> poly_line2,ref double intersection_param1, ref double intersection_param2 )
        {
            // Cradle the polylines to get to the exact intersection

            PointF intersection_pt;
            bool is_intersecting = false;
            double intersection_wd1 = 1.0d / (double)(poly_line1.Count-1);
            double intersection_wd2 = 1.0d / (double)(poly_line2.Count-1);

            for (int i = 1; i < (poly_line1.Count-1); i++)
            {
                // Loop thro first line (i-1 -> i)
                for (int j = 1; j < (poly_line2.Count-1); j++)
                {
                    // Loop thro second line (j-1 -> j)
                    if (gfunctions.Is_lines_intersect(poly_line1[i - 1], poly_line1[i+1], poly_line2[j - 1], poly_line2[j+1]) == true)
                    {
                        // Intersection found
                        intersection_pt = gfunctions.intersection_point(poly_line1[i - 1], poly_line1[i+1], poly_line2[j - 1], poly_line2[j+1]);

                        // Convert intersection point to bezier parameter t
                        intersection_param1 = ((i - 1) * intersection_wd1) + (gfunctions.get_param_t_for_pt(poly_line1[i - 1], poly_line1[i+1], intersection_pt) *2.0* intersection_wd1);

                        // Convert intersection point to line parameter t
                        intersection_param2 = ((j - 1) * intersection_wd2) + (gfunctions.get_param_t_for_pt(poly_line2[j - 1], poly_line2[j+1], intersection_pt) * 2.0* intersection_wd2);

                        is_intersecting = true;
                        break;
                    }
                }
                if (is_intersecting == true)
                {
                    break;
                }
            }
            return is_intersecting;
        }


        private bool is_orientation_match(int arc_index, PointF int_pt)
        {
            PointF arc_end_pt1 = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).get_point_at_t(0);
            PointF arc_crown_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).get_point_at_t(0.5);
            PointF arc_end_pt2 = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).get_point_at_t(1);

            int arc_orientation = gfunctions.ordered_orientation(arc_end_pt1, arc_end_pt2, arc_crown_pt);
            int newpt_orientation = gfunctions.ordered_orientation(arc_end_pt1, arc_end_pt2, int_pt);

            if (arc_orientation == newpt_orientation)
            {
                return true;
            }

            return false;
        }

        private double param_t_angle(int arc_index, PointF int_pt)
        {
            PointF radius_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).arc_center_pt;
            PointF arc_end_pt1 = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).get_point_at_t(0);
            PointF arc_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).get_point_at_t(0.01);

            Tuple<double, double> temp_tuple = gfunctions.get_arc_angles(arc_end_pt1, int_pt, arc_pt, radius_pt);

            return (temp_tuple.Item2 / this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).arc_sweep_angle);
        }

        private void set_split_lines(int int_index, double start_param_t, double end_param_t, ref List<PointF> segment_pts)
        {
            // Intersected line
            PointF l_pt1 = this.wkc_obj.interim_obj.selected_lines.ElementAt(int_index).get_point_at_t(start_param_t);
            PointF l_pt2 = this.wkc_obj.interim_obj.selected_lines.ElementAt(int_index).get_point_at_t(end_param_t);

            segment_pts = new List<PointF>();
            segment_pts.Add(l_pt1);
            segment_pts.Add(l_pt2);
        }

        private void set_split_arcs(int arc_index, double start_param_t, double end_param_t, ref List<PointF> segment_pts)
        {
            double crown_param_t = (start_param_t + end_param_t) * 0.5;
            PointF chord_p, chord_q, crown_pt, center_pt;

            // set the chord point
            chord_p = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).get_point_at_t(start_param_t);
            chord_q = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).get_point_at_t(end_param_t);

            // set the radius & center point
            crown_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).get_point_at_t(crown_param_t);
            center_pt = this.wkc_obj.interim_obj.selected_arcs.ElementAt(arc_index).arc_center_pt;

            segment_pts = new List<PointF>();
            // Chord points
            segment_pts.Add(chord_p);
            segment_pts.Add(chord_q);

            // Control point and Center point
            segment_pts.Add(crown_pt);
            segment_pts.Add(center_pt);
        }

        private void set_split_beziers(int bz_index, double param_t, ref List<PointF> first_segment, ref List<PointF> second_segment)
        {
            List<PointF> bezier_pts = new List<PointF>();
            // Add the bezier points to list
            foreach (PointF cpts in this.wkc_obj.interim_obj.selected_beziers.ElementAt(bz_index).bezier_cntrl_pts)
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
            int i = 0;

            // First segment control points
            first_segment = new List<PointF>();
            for (i = 0; i < bezier_split_pts_f.Count; i++)
            {
                // Click points
                first_segment.Add(bezier_split_pts_f[i]);
            }

            // Second segment control points
            second_segment = new List<PointF>();
            for (i = 0; i < bezier_split_pts_s.Count; i++)
            {
                // Click points
                second_segment.Add(bezier_split_pts_s[i]);
            }
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

        private void apply_split_of_lines(List<PointF> segment_pts)
        {
            // Create the Split lines

                int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();
                this.wkc_obj.snap_obj.clear_temp_point(); // clear temp points to get the current node

                points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(segment_pts[0], this.wkc_obj.geom_obj);
                points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(segment_pts[1], this.wkc_obj.geom_obj);

                // Add line
                this.wkc_obj.geom_obj.add_line(member_id, s_pt, e_pt);

        }

        private void apply_split_of_arcs(List<PointF> segment_pts)
        {

            // Create the Split arcs
            int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();
            this.wkc_obj.snap_obj.clear_temp_point(); // clear temp points to get the current node

            points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(segment_pts[0], this.wkc_obj.geom_obj);
            points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(segment_pts[1], this.wkc_obj.geom_obj);

            // Add arc
            this.wkc_obj.geom_obj.add_arc(member_id, s_pt, e_pt,
                segment_pts[2], segment_pts[3]);

        }

        private void apply_split_of_beziers(List<PointF> segment_pts)
        {
            // Create the Split beziers
            // Add control points to the list
            List<PointF> cntrl_pts = new List<PointF>(segment_pts);

            int poly_count = segment_pts.Count;

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
