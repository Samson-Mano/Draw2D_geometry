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
    public class modify_rotation_control
    {
        private workarea_control wkc_obj;

        // Points
        List<PointF> interim_click_pts;

        int selected_line_count = 0;
        int selected_arc_count = 0;
        int selected_bezier_count = 0;

        List<int> bezier_type;

        public void modify_rotation(workarea_control wkc)
        {
            this.wkc_obj = wkc;
            bool duplicate_transformation = gvariables.Is_duplicate;

            // Get the rotation point and rotation angle
            PointF rotation_pt = this.wkc_obj.interim_obj.click_pts[0].get_point;
            double rot_angle_rad = -1.0 * gfunctions.get_angle_rad(this.wkc_obj.interim_obj.click_pts[0].get_point, this.wkc_obj.interim_obj.click_pts[1].get_point);

            // Points, Length and angle
            this.interim_click_pts = new List<PointF>(); // [0,1], [2,3], [4,5] ...

            this.selected_line_count = 0;
            this.selected_arc_count = 0;
            this.selected_bezier_count = 0;

            // Set Rotate lines
            if (this.wkc_obj.interim_obj.selected_lines.Count > 0)
            {
                set_rotation_of_lines(rotation_pt, rot_angle_rad);
            }

            // Set Rotate arcs
            if (this.wkc_obj.interim_obj.selected_arcs.Count > 0)
            {
                set_rotation_of_arcs(rotation_pt, rot_angle_rad);
            }

            // Set Rotate bezier
            if (this.wkc_obj.interim_obj.selected_beziers.Count > 0)
            {
                set_rotation_of_beziers(rotation_pt, rot_angle_rad);
            }

            // Delete member (if duplicate is not true)
            if (duplicate_transformation == false && (this.wkc_obj.interim_obj.selected_lines.Count > 0 ||
                this.wkc_obj.interim_obj.selected_arcs.Count > 0 ||
                this.wkc_obj.interim_obj.selected_beziers.Count > 0))
            {
                // Remove disassociated points
                (new delete_operation_control()).delete_member(this.wkc_obj);
            }

            // Apply Rotate lines
            if (this.selected_line_count > 0)
            {
                apply_rotation_of_lines();
            }

            // Apply Rotate arcs
            if (this.selected_arc_count > 0)
            {
                apply_rotation_of_arcs();
            }

            // Apply Rotate beziers
            if (this.selected_bezier_count > 0)
            {
                apply_rotation_of_beziers();
            }
        }

        private void set_rotation_of_lines(PointF rotation_pt, double rot_angle_rad)
        {
            // Set the translation to click points
            // Points list order [0,1], [2,3], [4,5] ...
            this.selected_line_count = 0;

            foreach (lines_store selected_line in this.wkc_obj.interim_obj.selected_lines)
            {
                points_store s_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(selected_line.pt_start_id));
                points_store e_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(selected_line.pt_end_id));

                // Apply the rotation
                // Pt1
                double rot_x1 = ((s_pt.x - rotation_pt.X) * Math.Cos(rot_angle_rad) - (s_pt.y - rotation_pt.Y) * Math.Sin(rot_angle_rad)) + rotation_pt.X;
                double rot_y1 = ((s_pt.x - rotation_pt.X) * Math.Sin(rot_angle_rad) + (s_pt.y - rotation_pt.Y) * Math.Cos(rot_angle_rad)) + rotation_pt.Y;

                // Pt2
                double rot_x2 = ((e_pt.x - rotation_pt.X) * Math.Cos(rot_angle_rad) - (e_pt.y - rotation_pt.Y) * Math.Sin(rot_angle_rad)) + rotation_pt.X;
                double rot_y2 = ((e_pt.x - rotation_pt.X) * Math.Sin(rot_angle_rad) + (e_pt.y - rotation_pt.Y) * Math.Cos(rot_angle_rad)) + rotation_pt.Y;

                s_pt = new points_store(selected_line.pt_start_id, rot_x1, rot_y1);
                e_pt = new points_store(selected_line.pt_end_id, rot_x2, rot_y2);

                // Start and end point
                interim_click_pts.Add(s_pt.get_point); // 0, 2, 4, ...
                interim_click_pts.Add(e_pt.get_point); // 1, 3, 5, ...

                this.selected_line_count++;
            }

        }

        private void apply_rotation_of_lines()
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

        private void set_rotation_of_arcs(PointF rotation_pt, double rot_angle_rad)
        {
            // Set the translation to click points
            // Points list order [0,1,2,3], [4,5,6,7], [8,9,10,11] ...
            this.selected_arc_count = 0;

            foreach (arcs_store selected_arc in this.wkc_obj.interim_obj.selected_arcs)
            {
                points_store chord_s_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(selected_arc.pt_chord_start_id));
                points_store chord_e_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(selected_arc.pt_chord_end_id));
               
                // Apply the rotation for chord point
                // Pt1
                double rot_x1 = ((chord_s_pt.x - rotation_pt.X) * Math.Cos(rot_angle_rad) - (chord_s_pt.y - rotation_pt.Y) * Math.Sin(rot_angle_rad)) + rotation_pt.X;
                double rot_y1 = ((chord_s_pt.x - rotation_pt.X) * Math.Sin(rot_angle_rad) + (chord_s_pt.y - rotation_pt.Y) * Math.Cos(rot_angle_rad)) + rotation_pt.Y;

                // Pt2
                double rot_x2 = ((chord_e_pt.x - rotation_pt.X) * Math.Cos(rot_angle_rad) - (chord_e_pt.y - rotation_pt.Y) * Math.Sin(rot_angle_rad)) + rotation_pt.X;
                double rot_y2 = ((chord_e_pt.x - rotation_pt.X) * Math.Sin(rot_angle_rad) + (chord_e_pt.y - rotation_pt.Y) * Math.Cos(rot_angle_rad)) + rotation_pt.Y;

                chord_s_pt = new points_store(selected_arc.pt_chord_start_id, rot_x1, rot_y1);
                chord_e_pt = new points_store(selected_arc.pt_chord_end_id, rot_x2, rot_y2);

                PointF arc_cntrl_pt = new PointF(selected_arc.cntrl_pt_on_arc.X , selected_arc.cntrl_pt_on_arc.Y );
                PointF arc_center_pt = new PointF(selected_arc.arc_center_pt.X, selected_arc.arc_center_pt.Y);
                
                // Apply the rotation for arc control point & arc center point
                // Pt3
                double rot_x3 = ((arc_cntrl_pt.X - rotation_pt.X) * Math.Cos(rot_angle_rad) - (arc_cntrl_pt.Y - rotation_pt.Y) * Math.Sin(rot_angle_rad)) + rotation_pt.X;
                double rot_y3 = ((arc_cntrl_pt.X - rotation_pt.X) * Math.Sin(rot_angle_rad) + (arc_cntrl_pt.Y - rotation_pt.Y) * Math.Cos(rot_angle_rad)) + rotation_pt.Y;

                // Pt4
                double rot_x4 = ((arc_center_pt.X - rotation_pt.X) * Math.Cos(rot_angle_rad) - (arc_center_pt.Y - rotation_pt.Y) * Math.Sin(rot_angle_rad)) + rotation_pt.X;
                double rot_y4 = ((arc_center_pt.X - rotation_pt.X) * Math.Sin(rot_angle_rad) + (arc_center_pt.Y - rotation_pt.Y) * Math.Cos(rot_angle_rad)) + rotation_pt.Y;

                arc_cntrl_pt = new PointF((float)rot_x3, (float)rot_y3);
                arc_center_pt = new PointF((float)rot_x4, (float)rot_y4);

                // Chord start and end point
                interim_click_pts.Add(chord_s_pt.get_point); // 0, 4, 8, ...
                interim_click_pts.Add(chord_e_pt.get_point); // 1, 5, 9, ...
                interim_click_pts.Add(arc_cntrl_pt); // 2, 6, 10, ...
                interim_click_pts.Add(arc_center_pt); // 3, 7, 11, ...

                this.selected_arc_count++;
            }

        }

        private void apply_rotation_of_arcs()
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


        private void set_rotation_of_beziers(PointF rotation_pt, double rot_angle_rad)
        {
            // Set the rotation to click points
            // Points list order [0,1], [2,3], [4,5] ...
            this.selected_bezier_count = 0;
            bezier_type = new List<int>();

            foreach (beziers_store selected_bezier in this.wkc_obj.interim_obj.selected_beziers)
            {
                bezier_type.Add(selected_bezier.poly_count);

                double rot_x1, rot_y1;
                for (int i = 0; i < selected_bezier.bezier_cntrl_pts.Count; i++)
                {

                    PointF e_pt = new PointF((float)(selected_bezier.bezier_cntrl_pts[i].X ),
                       (float)(selected_bezier.bezier_cntrl_pts[i].Y));
                    
                    rot_x1 = ((e_pt.X - rotation_pt.X) * Math.Cos(rot_angle_rad) - (e_pt.Y - rotation_pt.Y) * Math.Sin(rot_angle_rad)) + rotation_pt.X;
                    rot_y1 = ((e_pt.X - rotation_pt.X) * Math.Sin(rot_angle_rad) + (e_pt.Y - rotation_pt.Y) * Math.Cos(rot_angle_rad)) + rotation_pt.Y;

                    e_pt = new PointF((float)rot_x1, (float)rot_y1);

                    // Start and end point
                    interim_click_pts.Add(e_pt); // 1, 2, 3, ...
                }

                this.selected_bezier_count++;
            }
        }

        private void apply_rotation_of_beziers()
        {
            int click_index_shift = (this.selected_line_count * 2) + (this.selected_arc_count * 4);
            int cindex_offset = 0;

            // Rotate the member
            for (int i = 0; i < this.selected_bezier_count; i++)
            {
                int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();
                this.wkc_obj.snap_obj.clear_temp_point(); // clear temp points to get the current node

                points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[click_index_shift + cindex_offset], this.wkc_obj.geom_obj);
                points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(interim_click_pts[click_index_shift + cindex_offset + (bezier_type[i] - 1)], this.wkc_obj.geom_obj);

                List<PointF> cntrl_pts = new List<PointF>();

                for (int j = 0; j < bezier_type[i]; j++)
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
