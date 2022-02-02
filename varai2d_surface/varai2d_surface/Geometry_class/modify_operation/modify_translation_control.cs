using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.global_static;
using varai2d_surface.Geometry_class.geometry_store;
using varai2d_surface.Drawing_area;
using System.Windows.Forms;

namespace varai2d_surface.Geometry_class.modify_operation
{
    public class modify_translation_control
    {
        private workarea_control wkc_obj;

        // Points
        List<PointF> interim_click_pts;

        int selected_line_count = 0;
        int selected_arc_count = 0;
        int selected_bezier_count = 0;

        List<int> bezier_type;

        public void modify_translation(workarea_control wkc_j)
        {
            this.wkc_obj = wkc_j;
            bool duplicate_transformation = gvariables.Is_duplicate;

            // Translation distance
            double tx = this.wkc_obj.interim_obj.click_pts[1].x - this.wkc_obj.interim_obj.click_pts[0].x;
            double ty = this.wkc_obj.interim_obj.click_pts[1].y - this.wkc_obj.interim_obj.click_pts[0].y;

            // Points, Length and angle
            this.interim_click_pts = new List<PointF>(); // [0,1], [2,3], [4,5] ...
           
            this.selected_line_count = 0;
            this.selected_arc_count = 0;
            this.selected_bezier_count = 0;

            // Set Translate lines
            if (this.wkc_obj.interim_obj.selected_lines.Count > 0)
            {
                set_translation_of_lines(tx, ty);
            }

            // Set Translate arcs
            if (this.wkc_obj.interim_obj.selected_arcs.Count > 0)
            {
                set_translation_of_arcs(tx, ty);
            }

            // Set Translate bezier
            if (this.wkc_obj.interim_obj.selected_beziers.Count > 0)
            {
                set_translation_of_beziers(tx, ty);
            }

            // Delete member (if duplicate is not true)
            if (duplicate_transformation == false && (this.wkc_obj.interim_obj.selected_lines.Count > 0 ||
                 this.wkc_obj.interim_obj.selected_arcs.Count > 0 ||
                 this.wkc_obj.interim_obj.selected_beziers.Count > 0))
            {
                // Remove disassociated points
                (new delete_operation_control()).delete_member(this.wkc_obj);
            }

            // Apply Translate lines
            if (this.selected_line_count > 0)
            {
                apply_translation_of_lines();
            }

            // Apply Translate arcs
            if (this.selected_arc_count > 0)
            {
                apply_translation_of_arcs();
            }

            // Apply Translate beziers
            if (this.selected_bezier_count > 0)
            {
                apply_translation_of_beziers();
            }
        }

        private void set_translation_of_lines(double tx, double ty)
        {
            // Set the translation to click points
            // Points list order [0,1], [2,3], [4,5] ...
            this.selected_line_count = 0;

            foreach (lines_store selected_line in this.wkc_obj.interim_obj.selected_lines)
            {
                points_store s_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(selected_line.pt_start_id));
                points_store e_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(selected_line.pt_end_id));

                // Apply the translation
                s_pt = new points_store(selected_line.pt_start_id, (s_pt.x + tx), (s_pt.y + ty));
                e_pt = new points_store(selected_line.pt_end_id, (e_pt.x + tx), (e_pt.y + ty));

                // Start and end point
                interim_click_pts.Add(s_pt.get_point); // 0, 2, 4, ...
                interim_click_pts.Add(e_pt.get_point); // 1, 3, 5, ...

                this.selected_line_count++;
            }

        }

        private void apply_translation_of_lines()
        {
            // Translate the member
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

        private void set_translation_of_arcs(double tx, double ty)
        {
            // Set the translation to click points
            // Points list order [0,1,2,3], [4,5,6,7], [8,9,10,11] ...
            this.selected_arc_count = 0;

            foreach (arcs_store selected_arc in this.wkc_obj.interim_obj.selected_arcs)
            {
                points_store chord_s_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(selected_arc.pt_chord_start_id));
                points_store chord_e_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(selected_arc.pt_chord_end_id));

                // Apply the translation
                chord_s_pt = new points_store(selected_arc.pt_chord_start_id, (chord_s_pt.x + tx), (chord_s_pt.y + ty));
                chord_e_pt = new points_store(selected_arc.pt_chord_end_id, (chord_e_pt.x + tx), (chord_e_pt.y + ty));

                PointF arc_cntrl_pt = new PointF((float)(selected_arc.cntrl_pt_on_arc.X + tx), (float)(selected_arc.cntrl_pt_on_arc.Y + ty));
                PointF arc_center_pt = new PointF((float)(selected_arc.arc_center_pt.X + tx), (float)(selected_arc.arc_center_pt.Y + ty));

                // Chord start and end point
                interim_click_pts.Add(chord_s_pt.get_point); // 0, 4, 8, ...
                interim_click_pts.Add(chord_e_pt.get_point); // 1, 5, 9, ...
                interim_click_pts.Add(arc_cntrl_pt); // 2, 6, 10, ...
                interim_click_pts.Add(arc_center_pt); // 3, 7, 11, ...

                this.selected_arc_count++;
            }

        }

        private void apply_translation_of_arcs()
        {
            int click_index_shift = this.selected_line_count * 2;

            // Translate the member
            for (int i = 0; i < this.selected_arc_count; i++)
            {
                int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();
                this.wkc_obj.snap_obj.clear_temp_point(); // clear temp points to get the current node

                points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[click_index_shift + (i * 4)], this.wkc_obj.geom_obj);
                points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[click_index_shift + ((i * 4) + 1)], this.wkc_obj.geom_obj);

                // Add line
                this.wkc_obj.geom_obj.add_arc(member_id, s_pt, e_pt,
                    interim_click_pts[click_index_shift + ((i * 4) + 2)], interim_click_pts[click_index_shift + ((i * 4) + 3)]);
            }
        }

        private void set_translation_of_beziers(double tx, double ty)
        {
            // Set the translation to click points
            // Points list order [0,1], [2,3], [4,5] ...
            this.selected_bezier_count = 0;
            bezier_type = new List<int>();

            foreach (beziers_store selected_bezier in this.wkc_obj.interim_obj.selected_beziers)
            {
                bezier_type.Add(selected_bezier.poly_count);
                for (int i = 0; i < selected_bezier.bezier_cntrl_pts.Count; i++)
                {
                    PointF e_pt = new PointF((float)(selected_bezier.bezier_cntrl_pts[i].X + tx),
                       (float)(selected_bezier.bezier_cntrl_pts[i].Y + ty));

                    // Start and end point
                    interim_click_pts.Add(e_pt); // 1, 2, 3, ...
                }

                this.selected_bezier_count++;
            }
        }

        private void apply_translation_of_beziers()
        {
            int click_index_shift = (this.selected_line_count * 2) + (this.selected_arc_count * 4);
            int cindex_offset = 0;

            // Translate the member
            for (int i = 0; i < this.selected_bezier_count; i++)
            {
                int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();
                this.wkc_obj.snap_obj.clear_temp_point(); // clear temp points to get the current node

                points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[click_index_shift + cindex_offset], this.wkc_obj.geom_obj);
                points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[click_index_shift + cindex_offset + (bezier_type[i] - 1)], this.wkc_obj.geom_obj);

                List<PointF> cntrl_pts = new List<PointF>();
                for (int j =0; j < bezier_type[i]; j++)
                {
                    cntrl_pts.Add(interim_click_pts[click_index_shift + cindex_offset + j]);
                }
                cindex_offset = cindex_offset + bezier_type[i];

                // Add bezier
                this.wkc_obj.geom_obj.add_bezier(member_id, bezier_type[i], s_pt, e_pt, cntrl_pts);
            }
        }
    }
}
