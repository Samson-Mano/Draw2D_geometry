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
using varai2d_surface.Geometry_class;

namespace varai2d_surface.Drawing_area
{
   public class surface_assoc_control
    {
        private workarea_control wkc_obj;

        public surface_assoc_control(workarea_control wkc)
        {
            // Set the workarea
            this.wkc_obj = wkc;
        }

        public void add_surface_association(surface_store surf_list)
        {
            // add surface

        }

        public void delete_surface_dissociation(surface_store surf_list)
        {
            // Delete surface

        }

        private void associate_lines(bool is_associate)
        {

        }

        private void associate_arcs(bool is_associate)
        {

        }

        private void associate_bezier(bool is_associate)
        {

        }
    }
}
