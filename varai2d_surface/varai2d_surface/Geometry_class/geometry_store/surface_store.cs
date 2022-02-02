using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.global_static;

namespace varai2d_surface.Geometry_class.geometry_store
{
    [Serializable]
    public class surface_store
    {
        private int _surf_id;
        private HashSet<int> _closed_loop_bndry_id = new HashSet<int>();
        private HashSet<int> _closed_loop_pt_id = new HashSet<int>();
        private List<PointF> _polygon_pts = new List<PointF>();
        private HashSet<surface_store> _nested_surface = new HashSet<surface_store>();

        private HashSet<int> _line_ids = new HashSet<int>();
        private HashSet<int> _arc_ids = new HashSet<int>();
        private HashSet<int> _bezier_ids = new HashSet<int>();

        //private bool _is_selected = false;

        private double _poly_area = 0.0;

        private Color surf_clr;
        private PointF pt_show_id_at;

        public int surf_id { get { return this._surf_id; } }

        public HashSet<int> closed_loop_bndry_id { get { return this._closed_loop_bndry_id; } }

        public HashSet<int> closed_loop_pt_id { get { return this._closed_loop_pt_id; } }

        public int nested_surface_count { get { return this._nested_surface.Count; } }

       // public HashSet<surface_store> nested_surface_output { get { return this._nested_surface; } }

        //public HashSet<int> line_ids { get { return this._line_ids; } }

        //public HashSet<int> arc_ids { get { return this._arc_ids; } }

        //public HashSet<int> bezier_ids { get { return this._bezier_ids; } }
        public string str_surf { 
            get 
            {
                string rslt_str = "";
                if(gvariables.Is_paint_surfaceid == true)
                {
                    rslt_str = rslt_str + "Surface " + (this.surf_id+1).ToString();
                }
              
                if(gvariables.Is_paint_memberlength == true)
                {
                    rslt_str = rslt_str + Environment.NewLine + "Area = " + str_poly_area;
                }
                return rslt_str; 
            } 
        }

        public List<PointF> polygon_pts { get { return this._polygon_pts; } }

        public double poly_area { get { return this._poly_area ; } }

        public string str_poly_area { get { return (this._poly_area / (gvariables.scale_factor * gvariables.scale_factor)).ToString(gvariables.ln_length_pres); } }

        [NonSerialized]
        private Region _surf_region = new Region();

        public Region surf_region { get { return this._surf_region; } }

        public surface_store()
        {
            // Empty Constructor
        }

        public surface_store(int t_surf_id, HashSet<int> t_closed_loop_bndry_id,
            HashSet<int> t_closed_loop_pt_id,
            HashSet<int> t_line_ids,
            HashSet<int> t_arc_ids,
            HashSet<int> t_bezier_ids,
            List<PointF> t_ply_pts,
            PointF t_pt_show_id_at,
            Color clr)
        {
            this._surf_id = t_surf_id;

            // Add closed loop boundary
            this._closed_loop_bndry_id = new HashSet<int>(t_closed_loop_bndry_id);
            // Add closed loop point id
            this._closed_loop_pt_id = new HashSet<int>(t_closed_loop_pt_id);
            // Add all the polygon points
            this._polygon_pts = new List<PointF>(t_ply_pts);

            this._poly_area = Math.Abs(polygon_area(t_ply_pts));
            this.surf_clr = clr;

            // Set the line ids, arc ids and bezier ids associated with this surface
            this._line_ids = new HashSet<int>(t_line_ids);
            this._arc_ids = new HashSet<int>(t_arc_ids);
            this._bezier_ids = new HashSet<int>(t_bezier_ids);

            // nested surface will be added once all the surfaces are created
            this._nested_surface = new HashSet<surface_store>();
            this.pt_show_id_at = t_pt_show_id_at;

            // set_nonserialized_surface_region();
        }

        public void set_nested_surface(HashSet<surface_store> this_nested_surfaces)
        {
            // Call after setting up all the surface
            this._nested_surface = new HashSet<surface_store>(this_nested_surfaces);
            set_nonserialized_surface_region();
        }

        public void set_nonserialized_surface_region()
        {
            // Create the region
            GraphicsPath temp = new GraphicsPath();
            temp.AddLines(this._polygon_pts.ToArray());
            temp.AddLine(this._polygon_pts[this._polygon_pts.Count - 1], this._polygon_pts[0]);
            temp.FillMode = FillMode.Winding;

            this._surf_region = new Region(temp);

            // Set the surface region
            double nested_poly_area = 0.0;
            foreach (surface_store surf in this._nested_surface)
            {
                // Create a temporary region for the nested surface
                temp = new GraphicsPath();
                temp.AddLines(surf.polygon_pts.ToArray());
                temp.AddLine(surf.polygon_pts[surf.polygon_pts.Count - 1], surf.polygon_pts[0]);
                temp.FillMode = FillMode.Winding;

                Region temp_rg = new Region(temp);
                // Exclude that region from the main surface
                nested_poly_area = nested_poly_area + surf.poly_area;
                this._surf_region.Exclude(temp_rg);
            }
            // Reduce the nested poly area from this surface
            this._poly_area = this._poly_area - nested_poly_area;
        }


        public HashSet<int> get_nested_surfaces_boundary_member_ids()
        {
            // Get all the ids of nested boundary member (used in association of nested members to this surface)
            HashSet<int> mem_ids = new HashSet<int>();
            foreach(surface_store surf in this._nested_surface)
            {
                mem_ids.UnionWith(surf.closed_loop_bndry_id);
            }
            return mem_ids;
        }

        public void Scale_surfaces(double scale, PointF transL, HashSet<points_store> all_perm_pts)
        {

            for (int i = 0; i < this._polygon_pts.Count; i++)
            {
                // Translate and Scale
                PointF temp_pt = new PointF((float)((this._polygon_pts[i].X + transL.X) * scale),
                    (float)((this._polygon_pts[i].Y + transL.Y) * scale));

                this._polygon_pts[i] =  temp_pt;
            }

            // Surface id showed at
            this.pt_show_id_at = new PointF((float)((pt_show_id_at.X + transL.X) * scale),
                    (float)((pt_show_id_at.Y + transL.Y) * scale));

            GraphicsPath temp = new GraphicsPath();
            temp.AddLines(this._polygon_pts.ToArray());
            temp.AddLine(this._polygon_pts[this._polygon_pts.Count - 1], this._polygon_pts[0]);
            // temp.FillMode = FillMode.Winding;

            this._surf_region = new Region(temp);

            // Polygon Area
            this._poly_area = Math.Abs(polygon_area(this._polygon_pts));

            // Exclude the nested region
            double nested_poly_area = 0.0;
            foreach (surface_store surf in this._nested_surface)
            {
                surf.Scale_surfaces(scale, transL, all_perm_pts);
                
                // Create a temporary region for the nested surface
                temp = new GraphicsPath();
                temp.AddLines(surf.polygon_pts.ToArray());
                temp.AddLine(surf.polygon_pts[surf.polygon_pts.Count - 1], surf.polygon_pts[0]);
               // temp.FillMode = FillMode.Winding;

                Region temp_rg = new Region(temp);

                nested_poly_area = nested_poly_area + surf.poly_area;
                // Exclude that region from the main surface
                this._surf_region.Exclude(temp_rg);
            }

            // Reduce the nested poly area from this surface
            this._poly_area = this._poly_area - nested_poly_area;
        }


        private double polygon_area(List<PointF> t_ply_pts)
        {
            double area2 = 0.0;
            for (int i = 0; i < (t_ply_pts.Count - 1); i++)
            {
                area2 = area2 + ((t_ply_pts[i].X * t_ply_pts[i + 1].Y) - (t_ply_pts[i + 1].X * t_ply_pts[i].Y));
            }
            return (area2 * 0.5f);
        }

        private PointF polygon_centroid(HashSet<PointF> t_ply_pts)
        {
            double c_x = 0.0;
            double c_y = 0.0;

            double signedArea = 0.0;
            double x0 = 0.0; // Current vertex X
            double y0 = 0.0; // Current vertex Y
            double x1 = 0.0; // Next vertex X
            double y1 = 0.0; // Next vertex Y
            double a = 0.0;  // Partial signed area

            // For all vertices except last
            int i = 0;
            for (i = 0; i < (t_ply_pts.Count - 1); ++i)
            {
                x0 = t_ply_pts.ElementAt(i).X;
                y0 = t_ply_pts.ElementAt(i).Y;
                x1 = t_ply_pts.ElementAt(i + 1).X;
                y1 = t_ply_pts.ElementAt(i + 1).Y;
                a = x0 * y1 - x1 * y0;
                signedArea = signedArea + a;
                c_x = c_x + (x0 + x1) * a;
                c_y = c_y + (y0 + y1) * a;
            }

            // Do last vertex separately to avoid performing an expensive
            // modulus operation in each iteration.
            x0 = t_ply_pts.ElementAt(i).X;
            y0 = t_ply_pts.ElementAt(i).Y;
            x1 = t_ply_pts.ElementAt(0).X;
            y1 = t_ply_pts.ElementAt(0).X;
            a = x0 * y1 - x1 * y0;
            signedArea = signedArea + a;
            c_x = c_x + (x0 + x1) * a;
            c_y = c_y + (y0 + y1) * a;

            signedArea = signedArea * 0.5;
            c_x = c_x / (6.0 * signedArea);
            c_y = c_y / (6.0 * signedArea);

            return new PointF((float)c_x, (float)c_y);

        }

        public void paint_surface(Graphics gr0)
        {
            // Paint the surface
            Region region = new Region(this.surf_region.GetRegionData());

            // Fill the region
            using (Pen poly_f_pen = new Pen(Color.FromArgb(100, surf_clr), 4))
            {
                gr0.FillRegion(poly_f_pen.Brush, region);
            }

            gr0.DrawString(str_surf, new Font("Cambria Math",gvariables.str_font_size), new Pen(Color.FromArgb(200, surf_clr), 4).Brush, pt_show_id_at);
        }

        public string[] get_data_grid_view_string()
        {
            // Add to data grid view
            string[] str_datGrid_row = new string[7];
            // 1) Surface id
            str_datGrid_row[0] = (this._surf_id + 1).ToString();

            // 2) Add End point ids
            str_datGrid_row[1] = "";
            foreach (int ept_id in this._closed_loop_pt_id)
            {
                str_datGrid_row[1] = str_datGrid_row[1] + ept_id.ToString() + ", ";
            }
            str_datGrid_row[1] = str_datGrid_row[1].Remove(str_datGrid_row[1].Length - 2, 2);

            // 3) Line ids
            str_datGrid_row[2] = "";
            foreach (int ln_id in this._line_ids)
            {
                str_datGrid_row[2] = str_datGrid_row[2] + ln_id.ToString() + ", ";
            }
            if (this._line_ids.Count > 0)
            {
                str_datGrid_row[2] = str_datGrid_row[2].Remove(str_datGrid_row[2].Length - 2, 2);
            }

            // 4) Arc ids
            str_datGrid_row[3] = "";
            foreach (int arc_id in this._arc_ids)
            {
                str_datGrid_row[3] = str_datGrid_row[3] + arc_id.ToString() + ", ";
            }
            if (this._arc_ids.Count > 0)
            {
                str_datGrid_row[3] = str_datGrid_row[3].Remove(str_datGrid_row[3].Length - 2, 2);
            }

            // 5) Bezier ids
            str_datGrid_row[4] = "";
            foreach (int bz_id in this._bezier_ids)
            {
                str_datGrid_row[4] = str_datGrid_row[4] + bz_id.ToString() + ", ";
            }
            if (this._bezier_ids.Count > 0)
            {
                str_datGrid_row[4] = str_datGrid_row[4].Remove(str_datGrid_row[4].Length - 2, 2);
            }

            // 6) Nested member ids
            str_datGrid_row[5] = get_nested_surface_closed_boundary_member_ids();
 
            // 7) Surface area
            str_datGrid_row[6] = str_poly_area;

            return str_datGrid_row;
        }

        public string get_nested_surface_closed_boundary_member_ids()
        {
            string rslt_str=null;
            foreach (surface_store nested_surf in this._nested_surface)
            {
                rslt_str = rslt_str + "[";
                foreach (int mem_id in nested_surf.closed_loop_bndry_id)
                {
                    rslt_str = rslt_str + mem_id.ToString() + ", ";
                }
                if (this.closed_loop_bndry_id.Count > 0)
                {
                    rslt_str = rslt_str.Remove(rslt_str.Length - 2, 2);
                }
                rslt_str = rslt_str + "] " + Environment.NewLine;
            }
            if(rslt_str != null)
            {
                rslt_str = rslt_str.Remove(rslt_str.LastIndexOf(Environment.NewLine));
            }
            return rslt_str;
        }


        public void update_selection(bool t_is_selected)
        {
            //this._is_selected = t_is_selected;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as surface_store);
        }

        public bool Equals(surface_store other_surf)
        {
            // Check 1 (Poly Ids)
            if (this.Equals(other_surf.surf_id))
            {
                // poly ids are matching
                return true;
            }
            return false;
        }

        public bool Equals(int other_surf_id)
        {
            if (this._surf_id == other_surf_id)
            {
                // Poly ids are matching
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.surf_id, 17);
        }
    }

    class SurfaceComparer : IComparer<surface_store>
    {
        public int Compare(surface_store first_surface, surface_store second_surface)
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
