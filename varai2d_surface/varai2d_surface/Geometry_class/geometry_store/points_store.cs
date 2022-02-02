using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.Drawing_area;
using varai2d_surface.global_static;

namespace varai2d_surface.Geometry_class.geometry_store
{
    [Serializable]
    public class points_store : IEquatable<points_store>
    {
        private int _nd_id;
        private double _x;
        private double _y;

        public int nd_id { get { return this._nd_id; } }

        public double x { get { return this._x; } }

        public double y { get { return this._y; } }

        public PointF get_point
        {
            get
            {
                return new PointF((float)this._x,
                                  (float)this._y);
            }
        }

        public string str_node_coord { get { return ("(" + (x/gvariables.scale_factor).ToString(gvariables.pt_coord_pres) + ", " + (-1.0f * (y / gvariables.scale_factor)).ToString(gvariables.pt_coord_pres) + ")"); } }

        public string str_node_id { get { return (this._nd_id.ToString()); } }

        public string str_node_txt
        {
            get
            {
                // Send the node details as string to paint
                string str_rslt = "";
                if (gvariables.Is_paint_ptid == true)
                {
                    str_rslt = str_node_id;
                }

                if (gvariables.Is_paint_ptcoord == true)
                {
                    str_rslt = str_rslt + str_node_coord;
                }
                return str_rslt;
            }
        }

        public points_store(int id, double t_x, double t_y)
        {
            // Constructor to add point
            this._nd_id = id;
            this._x = t_x;
            this._y = t_y;
        }

        public void Scale_points(double scale, PointF transL)
        {
            // Translate 
            this._x = this._x + transL.X;
            this._y = this._y + transL.Y;

            // Scale
            this._x = this._x * scale;
            this._y = this._y * scale;
        }


        public void paint_selected_point(Graphics gr0)
        {
            // Paint the selected ellipse on points
            gr0.FillEllipse(gvariables.pen_points.Brush, gfunctions.get_ellipse_rectangle(get_point, gvariables.radius_points + 1));
        }

        public void paint_point(Graphics gr0)
        {
            // Paint the ellipse on points
            gr0.FillEllipse(gvariables.pen_points.Brush, gfunctions.get_ellipse_rectangle(get_point, gvariables.radius_points));
            // Paint point id & co-ordinate
            string_drawing_control.paint_string(gr0, str_node_txt, new PointF(1, 0), get_point);
        }

        public void paint_modify_base_pt(Graphics gr0)
        {
            // Paint the ellipse on the first click after modification (translate, rotate, mirror) starts
            gr0.FillEllipse(gvariables.pen_snapline.Brush, gfunctions.get_ellipse_rectangle(get_point, gvariables.radius_points));
        }

        public void paint_temp_point_during_translation(Graphics gr0, double transl_x, double transl_y)
        {
            // Paint the points while being translated (to give user the idea how the points will be after translating)
            double temp_x = x + transl_x;
            double temp_y = y + transl_y;

            gr0.FillEllipse(gvariables.pen_points.Brush, gfunctions.get_ellipse_rectangle(new PointF((float)temp_x, (float)temp_y), gvariables.radius_points));
        }

        public void paint_temp_point_during_rotation(Graphics gr0, PointF rotation_pt, double rot_angle_rad)
        {
            // Paint the points while being rotated (to give user the idea how the points will be after rotation)
            double temp_x = x - rotation_pt.X;
            double temp_y = y - rotation_pt.Y;
            double rot_x = (temp_x * Math.Cos(rot_angle_rad) - temp_y * Math.Sin(rot_angle_rad)) + rotation_pt.X;
            double rot_y = (temp_x * Math.Sin(rot_angle_rad) + temp_y * Math.Cos(rot_angle_rad)) + rotation_pt.Y;

            gr0.FillEllipse(gvariables.pen_points.Brush, gfunctions.get_ellipse_rectangle(new PointF((float)rot_x, (float)rot_y), gvariables.radius_points));
        }

        public void paint_temp_point_during_mirror(Graphics gr0, PointF rotation_pt, double rot_angle_rad)
        {
            // Paint the points while being mirrored (to give user the idea how the points will be after mirroring)
            double temp_x = x - rotation_pt.X;
            double temp_y = y - rotation_pt.Y;
            double rot_x = (temp_x * Math.Cos(2 * rot_angle_rad) + temp_y * Math.Sin(2 * rot_angle_rad)) + rotation_pt.X;
            double rot_y = (temp_x * Math.Sin(2 * rot_angle_rad) - temp_y * Math.Cos(2 * rot_angle_rad)) + rotation_pt.Y;

            gr0.FillEllipse(gvariables.pen_points.Brush, gfunctions.get_ellipse_rectangle(new PointF((float)rot_x, (float)rot_y), gvariables.radius_points));
        }

        public bool Is_point_snap(points_store the_pt)
        {
            if (gvariables.Is_xysnap == false)
                return false;

            // Check whether the point is snaping to xy point
            // node lies on x
            if (((the_pt.x + gvariables.xysnap_intensity) > this._x && (the_pt.x - gvariables.xysnap_intensity) < this._x) == false)
            {
                return false;
            }

            // node lies on y
            if (((the_pt.y + gvariables.xysnap_intensity) > this._y && (the_pt.y - gvariables.xysnap_intensity) < this._y) == false)
            {
                return false;
            }

            return true;
        }

        public bool Is_point_v_snap(points_store the_pt)
        {
            if (gvariables.Is_hvsnap == false)
                return false;

            // check whether the point aligns in vertical direction
            if (((the_pt.x + gvariables.xysnap_intensity) > this._x && (the_pt.x - gvariables.xysnap_intensity) < this._x) == false)
            {
                return false;
            }
            return true;
        }

        public bool Is_point_h_snap(points_store the_pt)
        {
            if (gvariables.Is_hvsnap == false)
                return false;

            // check whether the point aligns in horizontal direction
            if (((the_pt.y + gvariables.hvsnap_intensity) > this._y && (the_pt.y - gvariables.hvsnap_intensity) < this._y) == false)
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as points_store);
        }

        public bool Equals(points_store other_pt)
        {
            if (this._nd_id == other_pt.nd_id)
            {
                return true;
            }
            return false;
        }

        public bool Equals(int other_pt_id)
        {
            // check whether a point with same id exists (overload 1)
            if (this._nd_id == other_pt_id)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(nd_id);
        }
    }


    // Custom comparer for the lines_store class
    class PointsComparer : IEqualityComparer<points_store>
    {
        // lines are equal if their line_ids are equal or both lines have same point id (either direction).
        public bool Equals(points_store first_point, points_store second_point)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(first_point, second_point)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(first_point, null) || Object.ReferenceEquals(second_point, null))
                return false;

            //Check whether the lines are equal.
            return first_point.Equals(second_point);
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(points_store other_point)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(other_point, null)) return 0;

            //Calculate the hash code for the product.
            return other_point.GetHashCode();
        }
    }



}
