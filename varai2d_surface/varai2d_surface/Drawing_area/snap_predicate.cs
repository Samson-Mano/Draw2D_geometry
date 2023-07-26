using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.global_static;
using varai2d_surface.Geometry_class;
using varai2d_surface.Geometry_class.geometry_store;

namespace varai2d_surface.Drawing_area
{
    [Serializable]
    public class snap_predicate
    {
        // temporary points are added during the addition process
        List<points_store> temp_points = new List<points_store>();
        List<int> temp_pt_id = new List<int>();

        private bool _Is_snapped = false;
        private bool Is_point_snap = false;
        private bool Is_point_v_snap = false;
        private bool Is_point_h_snap = false;

        private PointF snapped_pt = new PointF(0, 0);
        public bool Is_snapped { get { return this._Is_snapped; } set { this._Is_snapped = value; } }

        public void paint_snap_lines(Graphics gr0)
        {

            // Remove below !!!!!!!!!!!!!!!!!!!!!!!
            foreach (points_store pt in temp_points)
                pt.paint_point(gr0);
            ////////////////////////////////////////////// !!!!!!!!!!!!!!!!!!!!!!!!!!

            if (_Is_snapped == false)
                return;

            if (Is_point_snap == true)
            {
                // Draw ellipse at the snapped point
                gr0.DrawEllipse(gvariables.pen_snapline, gfunctions.get_ellipse_rectangle(snapped_pt, 6));
                return;
            }

            if (Is_point_v_snap == true)
            {
                // Draw vertical line (twice the length of canvas height)
                
                gr0.DrawLine(gvariables.pen_snapline, snapped_pt.X, snapped_pt.Y+(float)((-1.0f*gvariables.mainpic_size.Height) / gvariables.zoom_factor), snapped_pt.X, snapped_pt.Y + (float)(gvariables.mainpic_size.Height/gvariables.zoom_factor));
            }

            if (Is_point_h_snap == true)
            {
                // Draw horizontal line (twice the length of canvas width)
               // gvariables.pen_snapline.DashStyle = DashStyle.DashDotDot;
                gr0.DrawLine(gvariables.pen_snapline, snapped_pt.X+(float)((-1.0f * gvariables.mainpic_size.Width) / gvariables.zoom_factor), snapped_pt.Y, snapped_pt.X+(float)(gvariables.mainpic_size.Width / gvariables.zoom_factor), snapped_pt.Y);
            }
        }

        public snap_predicate()
        {
          gvariables.pen_snapline.DashStyle = DashStyle.DashDotDot;
        }

        public void clear_temp_point()
        {
            _Is_snapped = false;
            Is_point_snap = false;
            Is_point_v_snap = false;
            Is_point_h_snap = false;

            temp_pt_id.Clear();
            temp_points.Clear();
        }

        public bool Check_snap(PointF c_pt, HashSet<points_store> all_perm_points)
        {
            // Make a list with permanent and temporary points
            List<points_store> perm_and_temp_pts = new List<points_store>();

            perm_and_temp_pts.AddRange(all_perm_points);
            perm_and_temp_pts.AddRange(temp_points);

            // Snapped
            Is_point_snap = false;
            Is_point_h_snap = false;
            Is_point_v_snap = false;

            // create a point just an adhoc to check whether the point snaps to an existing point
            points_store adhoc_pt = new points_store(-100, c_pt.X, c_pt.Y);

            int index;
            // Check whether the cursor pt lies on an existing point
            index = perm_and_temp_pts.FindIndex(obj => obj.is_pt_snap(adhoc_pt));

            if (index != -1)
            {
                // an existing point with same x,y co-ordinate is found
                this.snapped_pt = perm_and_temp_pts[index].get_point;
                this._Is_snapped = true;
                Is_point_snap = true;
                return true;
            }

            // X,Y point;
            double x_cpt = adhoc_pt.x;
            double y_cpt = adhoc_pt.y;

            // Check whether the cursor pt lies on a vertical line of any existing point
            index = perm_and_temp_pts.FindIndex(obj => obj.is_pt_v_snap(adhoc_pt));

            if (index != -1)
            {
                // an existing point with same x co-ordinate is found
                x_cpt = perm_and_temp_pts[index].x;
                this._Is_snapped = true;
                Is_point_v_snap = true;
            }

            // Check whehter the cursor pt lies on a horizontal line of any existing point
            index = perm_and_temp_pts.FindIndex(obj => obj.is_pt_h_snap(adhoc_pt));

            if (index != -1)
            {
                // an existing point with same x co-ordinate is found
                y_cpt =perm_and_temp_pts[index].y;
                this._Is_snapped = true;
                Is_point_h_snap = true;
            }

            if (Is_point_h_snap == true || Is_point_v_snap == true)
            {
                this.snapped_pt = new PointF((float)x_cpt, (float)y_cpt);
                return true;
            }

            Is_point_snap = false;
            Is_point_h_snap = false;
            Is_point_v_snap = false;
            return false;
        }

        public points_store get_snap_point(PointF c_pt, geom_class geom)
        {
            // Make a list with permanent and temporary points
            List<points_store> perm_and_temp_pts = new List<points_store>();

            perm_and_temp_pts.AddRange(geom.all_end_pts);
            perm_and_temp_pts.AddRange(temp_points);

            int unique_node_id = geom.id_control.get_point_id(temp_pt_id);

            // create a point just an adhoc to check whether the point snaps to an existing point
            points_store adhoc_pt = new points_store(unique_node_id, c_pt.X, c_pt.Y);

            int index;
            // Check whether the cursor pt lies on an existing point
            index = perm_and_temp_pts.FindIndex(obj => obj.is_pt_snap(adhoc_pt));

            if (index != -1)
            {
                // an existing point with same x,y co-ordinate is found
                return perm_and_temp_pts[index];
            }

            // X,Y point;
            double x_cpt = adhoc_pt.x;
            double y_cpt = adhoc_pt.y;

            // Check whether the cursor pt lies on a vertical line of any existing point
            index = perm_and_temp_pts.FindIndex(obj => obj.is_pt_v_snap(adhoc_pt));

            if (index != -1)
            {
                // an existing point with same x co-ordinate is found
                x_cpt = perm_and_temp_pts[index].x;
            }

            // Check whehter the cursor pt lies on a horizontal line of any existing point
            index = perm_and_temp_pts.FindIndex(obj => obj.is_pt_h_snap(adhoc_pt));

            if (index != -1)
            {
                // an existing point with same x co-ordinate is found
                y_cpt = perm_and_temp_pts[index].y;
            }

            // snapped
            adhoc_pt = new points_store(adhoc_pt.nd_id, x_cpt, y_cpt);
            temp_pt_id.Add(adhoc_pt.nd_id);
            temp_points.Add(adhoc_pt);
            return adhoc_pt;
        }

    }
}
