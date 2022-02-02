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

    public class scale_transformation
    {
        private workarea_control wkc_obj;
        private double scale_value;
        private PointF transl_value;

        public scale_transformation(int main_pic_width, int main_pic_height,workarea_control wkc)
        {
            this.wkc_obj = wkc;

            if (this.wkc_obj.geom_obj.all_end_pts.Count < 2)
                return;

            // Scale the entire geometry to fit to the size of the painting area
            // Get all the co-ordinates of geometry
            double min_x = Double.MaxValue;
            double min_y = Double.MaxValue;
            double max_x = Double.MinValue;
            double max_y = Double.MinValue;

            // Arc crown and center point
            foreach (arcs_store g_arc in this.wkc_obj.geom_obj.all_arcs)
            {
                double[] param_t_list = new double[11] { 0.01, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.99 };
               
                for (int i = 0; i < param_t_list.Length; i++)
                {
                    //Get the point at parameter t
                    PointF get_param_pt = g_arc.get_point_at_t(param_t_list[i]);

                    min_x = Math.Min(min_x, get_param_pt.X);
                    max_x = Math.Max(max_x, get_param_pt.X);

                    min_y = Math.Min(min_y, get_param_pt.Y);
                    max_y = Math.Max(max_y, get_param_pt.Y);
                }
            }

            // Bezier all control points
            foreach (beziers_store g_bezier in this.wkc_obj.geom_obj.all_beziers)
            {
                double[] param_t_list = new double[11] { 0.01, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.99 };

                for (int i = 0; i < param_t_list.Length; i++)
                {
                    //Get the point at parameter t
                    PointF get_param_pt = g_bezier.get_point_at_t(param_t_list[i]);

                    min_x = Math.Min(min_x, get_param_pt.X);
                    max_x = Math.Max(max_x, get_param_pt.X);

                    min_y = Math.Min(min_y, get_param_pt.Y);
                    max_y = Math.Max(max_y, get_param_pt.Y);
                }
            }

            // All end points
            foreach (points_store g_points in this.wkc_obj.geom_obj.all_end_pts)
            {
                min_x = Math.Min(min_x, g_points.x);
                max_x = Math.Max(max_x, g_points.x);

                min_y = Math.Min(min_y, g_points.y);
                max_y = Math.Max(max_y, g_points.y);
            }

            double bound_width = max_x - min_x;
            double bound_height = max_y - min_y;

            // Translate co-ordinates to zero
            this.transl_value = new PointF(-(float)(min_x + (bound_width * 0.5f)), -(float)(min_y + (bound_height * 0.5f)));
            // Scale co-ordinates to fit the size
           this.scale_value = Math.Min((((double)main_pic_height) / bound_height), (((double)main_pic_width) / bound_width)) * gvariables.scale_margin;
            gvariables.scale_factor = gvariables.scale_factor * this.scale_value;
            // Scale all the points
            scale_fit_points();

            // Scale all the lines
            scale_fit_lines();

            // Scale all the Arcs
            scale_fit_arcs();

            // Scale all the Beziers
            scale_fit_beziers();

            // Scale all the Surfaces
            scale_fit_surfaces();
        }

        private void scale_fit_points()
        {
            foreach (points_store pts in this.wkc_obj.geom_obj.all_end_pts)
            {
                pts.Scale_points(this.scale_value, this.transl_value);
            }

        }

        private void scale_fit_lines()
        {
            foreach (lines_store lns in this.wkc_obj.geom_obj.all_lines)
            {
                lns.Scale_lines(this.scale_value, this.transl_value, this.wkc_obj.geom_obj.all_end_pts);
            }
        }

        private void scale_fit_arcs()
        {
            foreach (arcs_store arcs in this.wkc_obj.geom_obj.all_arcs)
            {
                arcs.Scale_arcs(this.scale_value, this.transl_value, this.wkc_obj.geom_obj.all_end_pts);
            }
        }

        private void scale_fit_beziers()
        {
            foreach (beziers_store bzs in this.wkc_obj.geom_obj.all_beziers)
            {
                bzs.Scale_beziers(this.scale_value, this.transl_value, this.wkc_obj.geom_obj.all_end_pts);
            }
        }

        private void scale_fit_surfaces()
        {
            foreach (surface_store surf in this.wkc_obj.geom_obj.all_surfaces)
            {
                surf.Scale_surfaces(this.scale_value, this.transl_value, this.wkc_obj.geom_obj.all_end_pts);
            }
        }
    }
}
