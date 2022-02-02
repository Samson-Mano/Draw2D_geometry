using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using varai2d_surface.Geometry_class.geometry_store;

namespace varai2d_surface.Geometry_class.add_operation
{
    public class add_bezier_control
    {
        private workarea_control wkc_obj;

        public void add_bezier(workarea_control wkc)
        {
            this.wkc_obj = wkc;
            int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();

            List<PointF> cntrl_pts = new List<PointF>();

            for(int i= 0; i< this.wkc_obj.interim_obj.click_pts.Count;i++)
            {
                cntrl_pts.Add(this.wkc_obj.interim_obj.click_pts[i].get_point);
            }
            int poly_count = this.wkc_obj.interim_obj.click_pts.Count;

            // Need special consideration to add the end points (to get the point ids to allign with order !!)
            this.wkc_obj.snap_obj.clear_temp_point();
            points_store s_pt = this.wkc_obj.snap_obj.get_snap_point(this.wkc_obj.interim_obj.click_pts[0].get_point, this.wkc_obj.geom_obj);
            points_store e_pt = this.wkc_obj.snap_obj.get_snap_point(this.wkc_obj.interim_obj.click_pts[poly_count - 1].get_point, this.wkc_obj.geom_obj);

            // Add Bezier
            this.wkc_obj.geom_obj.add_bezier(member_id, poly_count,
                s_pt,
                e_pt, cntrl_pts);
        }


    }
}
