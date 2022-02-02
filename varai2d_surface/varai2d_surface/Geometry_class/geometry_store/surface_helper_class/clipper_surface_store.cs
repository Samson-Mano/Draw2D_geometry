using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.Drawing_area;
using varai2d_surface.global_static;

namespace varai2d_surface.Geometry_class.geometry_store.surface_helper_class
{
    public class clipper_surface_store
    {
        private int _surf_id;
        private HashSet<int> _closed_loop_bndry_id = new HashSet<int>();
        private HashSet<int> _closed_loop_pt_id = new HashSet<int>();
        private List<clipper_polypts_store> _polygon_loop_pts = new List<clipper_polypts_store>();
        private Region _clipper_surf_region = new Region();
        //private HashSet<int> _nested_surf_id = new HashSet<int>();
        //private HashSet<clipper_surface_store> _the_nested_surfaces = new HashSet<clipper_surface_store>();
        // private GraphicsPath _closed_surf_boundary_path = new GraphicsPath();

        private double _this_poly_area;
       // private double _nested_poly_area;

        private int _this_nested_to = -1;

        public int surf_id { get { return this._surf_id; } }

        public HashSet<int> closed_loop_bndry_id { get { return this._closed_loop_bndry_id; } }

        public HashSet<int> closed_loop_pt_id { get { return this._closed_loop_pt_id; } }

        public List<clipper_polypts_store> polygon_loop_pts { get { return this._polygon_loop_pts; } }

        public Region clipper_surf_region { get { return this._clipper_surf_region; } }

       // public HashSet<int> nested_surf_id { get { return null; } }// Not used

        public List<PointF> get_polygon_pts
        {
            get
            {
                List<PointF> pts = new List<PointF>();
                foreach (clipper_polypts_store temp_p in this._polygon_loop_pts)
                {
                    pts.Add(temp_p.get_pt);
                }
                return pts;
            }
        }

        public int this_nested_to { get { return this._this_nested_to; } }

        public double poly_area { get { return (this._this_poly_area); } }

       // public double poly_nested_area { get { return (this._nested_poly_area); } }

        public clipper_surface_store(int t_surf_id, HashSet<int> t_closed_loop_bndry_id, HashSet<int> t_closed_loop_pt_id, List<clipper_polypts_store> t_ply_pts, bool is_oriented)
        {
            this._surf_id = t_surf_id;
            if (is_oriented == true)
            {
                // the points are already oriented
                // Add closed loop boundary
                this._closed_loop_bndry_id = new HashSet<int>(t_closed_loop_bndry_id);
                // Add closed loop point id
                this._closed_loop_pt_id = new HashSet<int>(t_closed_loop_pt_id);
                // Add all the polygon points
                this._polygon_loop_pts = new List<clipper_polypts_store>(t_ply_pts);
            }
            else
            {
                if (polygon_area(t_ply_pts) > 0)
                {
                    // Add closed loop boundary
                    this._closed_loop_bndry_id = new HashSet<int>(t_closed_loop_bndry_id);
                    // Add closed loop point id
                    this._closed_loop_pt_id = new HashSet<int>(t_closed_loop_pt_id);
                    // Add all the polygon points
                    this._polygon_loop_pts = new List<clipper_polypts_store>(t_ply_pts);
                }
                else
                {
                    // Add closed loop boundary
                    this._closed_loop_bndry_id = new HashSet<int>(t_closed_loop_bndry_id.Reverse());
                    // Add closed loop point id
                    this._closed_loop_pt_id = new HashSet<int>(t_closed_loop_pt_id.Reverse());
                    // Add all the polygon points
                    this._polygon_loop_pts = new List<clipper_polypts_store>(Enumerable.Reverse(t_ply_pts));
                }
            }

            this._this_poly_area = Math.Abs(polygon_area(t_ply_pts));

            GraphicsPath temp_gpath = new GraphicsPath();
            PointF[] temp_all_pts = this.get_polygon_pts.ToArray();
            temp_gpath.AddLines(temp_all_pts);
            temp_gpath.AddLine(temp_all_pts[temp_all_pts.Length - 1], temp_all_pts[0]);
            // Set the region
            this._clipper_surf_region = new Region(temp_gpath);

            this._this_nested_to = -1;
        }

        private double polygon_area(List<clipper_polypts_store> t_ply_pts)
        {
            double area2 = 0.0;
            for (int i = 0; i < (t_ply_pts.Count - 1); i++)
            {
                area2 = area2 + ((t_ply_pts[i].x * t_ply_pts[i + 1].y) - (t_ply_pts[i + 1].x * t_ply_pts[i].y));
            }
            area2 = area2 + ((t_ply_pts[t_ply_pts.Count - 1].x * t_ply_pts[0].y) - (t_ply_pts[0].x * t_ply_pts[t_ply_pts.Count - 1].y));

            return (area2 * 0.5f);
        }

        public void set_nest_this_surface(HashSet<clipper_surface_store> other_surfaces)
        {
            if (this._this_nested_to != -1)
                return;

            foreach (clipper_surface_store surf in other_surfaces)
            {
                // Create a region with this surface
                GraphicsPath temp_gpath = new GraphicsPath();
                temp_gpath.AddLines(surf.get_polygon_pts.ToArray());
                temp_gpath.AddLine(surf.get_polygon_pts[surf.get_polygon_pts.Count - 1], surf.get_polygon_pts[0]);
                temp_gpath.FillMode = FillMode.Winding;

                Region temp_reg = new Region(temp_gpath);
                bool is_inside = true;

                foreach (PointF pt in this.get_polygon_pts)
                {
                    // test if the point is inside the region
                    if (temp_reg.IsVisible(pt) == false)
                    {
                        // Check whether the point is in the boundary
                        if (temp_gpath.IsOutlineVisible(pt, new Pen(Brushes.Black, gvariables.linewidth_curves + 4)) == false)
                        {
                            is_inside = false;
                            break;
                        }
                    }
                }

                if (is_inside == true)
                {
                    // all the points of this surface is inside th region
                    this._this_nested_to = surf.surf_id;
                    return;
                }
            }
        }

        //public bool is_point_inside_outter_boundary(PointF pt)
        //{
        //    if (this._clipper_surf_region.IsVisible(pt) ==)

        //}


        public void set_nested_polygon(HashSet<clipper_surface_store> other_surf_list)
        {
            //// Check whether polygon 
            //List<PointF> this_poly_pts = new List<PointF>(this.get_polygon_pts);
            //this._nested_surf_id.Clear();
            //this._the_nested_surfaces.Clear();
            //double nested_surf_area = 0.0;

            //foreach (clipper_surface_store other_surf in other_surf_list)
            //{
            //    if (other_surf.this_nested_to == this.surf_id)
            //    {
            //        this.nested_surf_id.Add(other_surf.surf_id);
            //        this._the_nested_surfaces.Add(other_surf);

            //        // Must include nested surfaces's nested area as well 
            //        nested_surf_area = (other_surf.poly_area + other_surf.poly_nested_area);
            //    }
            //}

            //// Return the nested area
            //this._nested_poly_area = nested_surf_area;
        }


        private bool winding_number_algorithm(PointF pt, List<PointF> loop_pts)
        {
            int wn = 0;    // the  winding number counter

            // loop through all edges of the polygon
            for (int i = 0; i < (loop_pts.Count - 1); i++)
            {   // edge from V[i] to  V[i+1]
                if (loop_pts[i].Y <= pt.Y)
                {          // start y <= P.y
                    if (loop_pts[i + 1].Y > pt.Y)      // an upward crossing
                    {
                        if (isLeft(loop_pts[i], loop_pts[i + 1], pt) > 0)  // P left of  edge
                        {
                            wn++;            // have  a valid up intersect
                        }
                    }
                }
                else
                {                        // start y > P.y (no test needed)
                    if (loop_pts[i + 1].Y <= pt.Y)     // a downward crossing
                    {
                        if (isLeft(loop_pts[i], loop_pts[i + 1], pt) < 0)  // P right of  edge
                        {
                            wn--;            // have  a valid down intersect
                        }
                    }
                }
            }

            if (wn != 0)
            {
                return true;
            }
            return false;
        }

        private int isLeft(PointF p0, PointF p1, PointF pt)
        {
            return (int)((p1.X - p0.X) * (pt.Y - p0.Y) -
                    (pt.X - p0.X) * (p1.Y - p0.Y));
        }


    }


    class clipper_surface_Comparer : IComparer<clipper_surface_store>
    {
        public int Compare(clipper_surface_store first_surface, clipper_surface_store second_surface)
        {
            // Compare Areas and sort by area
            if (first_surface.poly_area < second_surface.poly_area)
            {
                return -1;
            }
            else if (first_surface.poly_area > second_surface.poly_area)
            {
                return 1;
            }
            return 0;
        }
    }
}
