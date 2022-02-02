using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using varai2d_surface.Geometry_class.geometry_store;

namespace varai2d_surface.Geometry_class.modify_operation
{
   public class delete_operation_control
    {
        private workarea_control wkc_obj;

        public void delete_member(workarea_control wkc_j)
        {
            this.wkc_obj = wkc_j;

            // Delete member
            // Delete Lines
            delete_lines(this.wkc_obj.interim_obj.selected_lines);
            // Delter Arcs
            delete_arcs(this.wkc_obj.interim_obj.selected_arcs);
            // Delete Beziers
            delete_beziers(this.wkc_obj.interim_obj.selected_beziers);

            // Remove disassociated points (a.ka. free points)
            delete_points();

            // complete operation
            this.wkc_obj.cancel_operation();
        }

        public void delete_lines(HashSet<lines_store> r_lines)
        {
            this.wkc_obj.geom_obj.remove_lines(r_lines);
        }

        public void delete_arcs(HashSet<arcs_store> r_arcs)
        {
            this.wkc_obj.geom_obj.remove_arcs(r_arcs);
        }

        public void delete_beziers(HashSet<beziers_store> r_beziers)
        {
            this.wkc_obj.geom_obj.remove_beziers(r_beziers);
        }

        private void delete_points()
        {
            HashSet<points_store> all_points_associated_with_member = new HashSet<points_store>();

            // All points associated with lines
            foreach (lines_store lines in this.wkc_obj.geom_obj.all_lines)
            {
                all_points_associated_with_member.Add(this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(lines.pt_end_id)));
                all_points_associated_with_member.Add(this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(lines.pt_start_id)));
            }

            // All points associated with arcs
            foreach (arcs_store arcs in this.wkc_obj.geom_obj.all_arcs)
            {
                all_points_associated_with_member.Add(this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(arcs.pt_chord_end_id)));
                all_points_associated_with_member.Add(this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(arcs.pt_chord_start_id)));
            }

            // All points associated with beziers
            foreach (beziers_store bz in this.wkc_obj.geom_obj.all_beziers)
            {
                all_points_associated_with_member.Add(this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(bz.pt_bz_start_id)));
                all_points_associated_with_member.Add(this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(bz.pt_bz_end_id)));
            }

            IEnumerable<points_store> free_points_e = this.wkc_obj.interim_obj.selected_points.Except(all_points_associated_with_member);
            // Delete points
            this.wkc_obj.geom_obj.remove_points(free_points_e.ToHashSet());
        }
    }
}
