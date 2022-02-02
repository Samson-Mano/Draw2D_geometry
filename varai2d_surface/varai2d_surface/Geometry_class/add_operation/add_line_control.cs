using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using varai2d_surface.Geometry_class.geometry_store;

namespace varai2d_surface.Geometry_class.add_operation
{
    public class add_line_control
    {
        private workarea_control wkc_obj;

        public void add_line(workarea_control wkc)
        {
            this.wkc_obj = wkc;
            int member_id = this.wkc_obj.geom_obj.id_control.get_member_id();

            // Add line
            this.wkc_obj.geom_obj.add_line(member_id, this.wkc_obj.interim_obj.click_pts[0], this.wkc_obj.interim_obj.click_pts[1]);
        }

    }
}
