using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using varai2d_surface.Geometry_class;
using varai2d_surface.Geometry_class.geometry_store;
using varai2d_surface.Geometry_class.geometry_store.surface_helper_class;

namespace varai2d_surface.global_static
{
    public partial class surface_form_api : Form
    {
        private main_form the_parent;
        private workarea_control wkc_obj;
        private bool is_add = true;
        private HashSet<clipper_surface_store> all_closed_bndrys = new HashSet<clipper_surface_store>();
        private Random rand;
        private List<KnownColor> colorList;

        public surface_form_api(main_form m_frm, workarea_control wkc)
        {
            InitializeComponent();

            // Initialize the Parent.
            the_parent = m_frm;

            // Find all the closed boundaries
            this.wkc_obj = wkc;

            // Set the color random
            //Set Random colors for surface
            // this list will include all Colors including OS system colors
            this.colorList = Enum.GetValues(typeof(KnownColor))
               .Cast<KnownColor>()
               .ToList();

            rand = new Random(DateTime.Now.Ticks.GetHashCode());
        }

        public void main_pic_click(PointF click_pt)
        {
            if (is_add == true)
            {
                // Check whether the point is in already created surface
                foreach (surface_store existing_surf in this.wkc_obj.geom_obj.all_surfaces)
                {
                    if (existing_surf.surf_region.IsVisible(click_pt) == true)
                        return;
                }

                for (int i = 0; i < all_closed_bndrys.Count; i++)
                {
                    clipper_surface_store closed_boundary_i = all_closed_bndrys.ElementAt(i);
                    // Is point is inside the closed boundary
                    if (closed_boundary_i.clipper_surf_region.IsVisible(click_pt) == true)
                    {
                        int clicked_boundary_id = closed_boundary_i.surf_id;
                        // Nested closed boundary
                        HashSet<int> line_ids = new HashSet<int>();
                        HashSet<int> arc_ids = new HashSet<int>();
                        HashSet<int> bz_ids = new HashSet<int>();
                        HashSet<surface_store> nested_closed_bndry = new HashSet<surface_store>();
                        surface_store temp_surface;
                        for (int j = i - 1; j >= 0; j--)
                        {
                            clipper_surface_store closed_boundary_j = all_closed_bndrys.ElementAt(j);
                            // Find all the boundaries which are nested to the clicked boundary
                            if (closed_boundary_j.this_nested_to == clicked_boundary_id)
                            {
                                // Get the individual ids from the closed loop bndry
                                fix_member_ids(closed_boundary_j.closed_loop_bndry_id,
                                    ref line_ids,
                                    ref arc_ids,
                                    ref bz_ids);

                                temp_surface = new surface_store(closed_boundary_j.surf_id,
                                    closed_boundary_j.closed_loop_bndry_id,
                                    closed_boundary_j.closed_loop_pt_id,
                                    line_ids,
                                    arc_ids,
                                    bz_ids,
                                    closed_boundary_j.get_polygon_pts, click_pt,
                                    Color.FromKnownColor(getRandomColor()));

                                // Add to the nested boundary
                                nested_closed_bndry.Add(temp_surface);
                            }
                        }
                        // Get the individual ids from the closed loop bndry
                        fix_member_ids(closed_boundary_i.closed_loop_bndry_id,
                            ref line_ids,
                            ref arc_ids,
                            ref bz_ids);

                        // Create the surface
                        temp_surface = new surface_store(get_unique_id(),
                           closed_boundary_i.closed_loop_bndry_id,
                           closed_boundary_i.closed_loop_pt_id,
                                   line_ids,
                                   arc_ids,
                                   bz_ids,
                           closed_boundary_i.get_polygon_pts, click_pt,
                           Color.FromKnownColor(getRandomColor()));

                        // set the nested surface for this
                        temp_surface.set_nested_surface(nested_closed_bndry);

                        // Add to the main object
                        this.wkc_obj.geom_obj.add_surface(temp_surface);

                        // Add to data grid view
                        add_to_datagrid_view(temp_surface);
                        break;
                    }
                }
            }
            else
            {
                // Remove in progress
                foreach (surface_store existing_surf in this.wkc_obj.geom_obj.all_surfaces)
                {
                    if (existing_surf.surf_region.IsVisible(click_pt) == true)
                    {
                        delete_from_datagrid_view(existing_surf);
                        this.wkc_obj.geom_obj.remove_surface(existing_surf);
                        return;
                    }

                }
            }
        }

        private void add_to_datagrid_view(surface_store surf_added)
        {
            // Add to data grid view
            dataGridView_surfacedata.Rows.Add(surf_added.get_data_grid_view_string());
        }

        private void delete_from_datagrid_view(surface_store surf_deleted)
        {
            // Delete surface from data grid view
               foreach(DataGridViewRow rw in dataGridView_surfacedata.Rows)
            {
                int selected_poly_id_value;
                int.TryParse(rw.Cells[0].Value.ToString(), out selected_poly_id_value);
                selected_poly_id_value--;

                if(selected_poly_id_value == surf_deleted.surf_id)
                {
                    // Delete and exit
                    dataGridView_surfacedata.Rows.Remove(rw);
                    return;
                }
            }
        }

        private void fill_datagridview()
        {
            // Fill the data grid view at the opening of the form
            foreach(surface_store surf  in this.wkc_obj.geom_obj.all_surfaces)
            {
                add_to_datagrid_view(surf);
            }
        }

        private int get_unique_id()
        {
            HashSet<int> existing_ids = new HashSet<int>();
            // Get all the id from the already added surfaces
            foreach(surface_store surf in this.wkc_obj.geom_obj.all_surfaces)
            {
                existing_ids.Add(surf.surf_id);
            }

            existing_ids = existing_ids.OrderBy(obj => obj).ToHashSet();

            // Check all the id 
            for (int i = 0; i < existing_ids.Count; i++)
            {
                // If any id sequence is missing return
                if (i != existing_ids.ElementAt(i))
                {
                    return i;
                }
            }
            return existing_ids.Count;
        }

        private void fix_member_ids(HashSet<int> member_ids, ref HashSet<int> line_ids, ref HashSet<int> arc_ids, ref HashSet<int> bz_ids)
        {
            line_ids = new HashSet<int>();
            arc_ids = new HashSet<int>();
            bz_ids = new HashSet<int>();

            // Loop through member ids and add to appropriate type
            foreach (int m_ids in member_ids)
            {
                // Line id
                foreach (lines_store ln in this.wkc_obj.geom_obj.all_lines)
                {
                    if (m_ids == ln.line_id)
                    {
                        line_ids.Add(m_ids);
                        break;
                    }
                }

                // Arc id
                foreach (arcs_store arc in this.wkc_obj.geom_obj.all_arcs)
                {
                    if (m_ids == arc.arc_id)
                    {
                        arc_ids.Add(m_ids);
                        break;
                    }
                }

                // Bezier id
                foreach (beziers_store bz in this.wkc_obj.geom_obj.all_beziers)
                {
                    if (m_ids == bz.bezier_id)
                    {
                        bz_ids.Add(m_ids);
                        break;
                    }
                }
            }

        }

        private void surface_form_api_Load(object sender, EventArgs e)
        {
            // Set surface form open
            this.Location = new Point(the_parent.Location.X + 10, the_parent.Location.Y + 150);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.ShowIcon = false;
            this.Opacity = 0.9;
            this.BringToFront();
            this.TopMost = true;

            this.all_closed_bndrys = new HashSet<clipper_surface_store>();
            this.all_closed_bndrys = get_closed_boundaries();
            fill_datagridview();

            toolStripMenuItem_add_ItemClicked(sender, e);
            gvariables.Is_surface_frm_open = true;
            the_parent.mt_pic.Refresh();
        }

        #region"Closed boundary creating methods"

        private HashSet<clipper_surface_store> get_closed_boundaries()
        {
            // Main code to set all the simple closed boundaries
            HashSet<clipper_polylines_store> poly_lines = new HashSet<clipper_polylines_store>();
            clipper_polylines_store temp_poly;

            // Extract all polylines from drawn objects
            // Convert line to poly line
            foreach (lines_store ln in this.wkc_obj.geom_obj.all_lines)
            {

                // Get the start and end points from pt strore
                points_store t_start_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(ln.pt_start_id));
                points_store t_end_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(ln.pt_end_id));
                List<PointF> inter_pts = ln.discretized_pts;


                temp_poly = new clipper_polylines_store(ln.line_id, ln.pt_start_id, ln.pt_end_id, 1,
                    t_start_pt.x, t_start_pt.y, inter_pts, t_end_pt.x, t_end_pt.y);

                // Add to the list of poly lines
                poly_lines.Add(temp_poly);
            }



            // Convert arc to poly line
            foreach (arcs_store arc in this.wkc_obj.geom_obj.all_arcs)
            {
                // Get the start and end points from pt strore
                points_store t_start_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(arc.pt_chord_start_id));
                points_store t_end_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(arc.pt_chord_end_id));
                List<PointF> inter_pts = arc.discretized_pts;

                temp_poly = new clipper_polylines_store(arc.arc_id, arc.pt_chord_start_id, arc.pt_chord_end_id, 2,
                    t_start_pt.x, t_start_pt.y, inter_pts, t_end_pt.x, t_end_pt.y);

                // Add to the list of poly lines
                poly_lines.Add(temp_poly);
            }

            // Convert bezier to poly line
            foreach (beziers_store bz in this.wkc_obj.geom_obj.all_beziers)
            {

                // Get the start and end points from pt strore
                points_store t_start_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(bz.pt_bz_start_id));
                points_store t_end_pt = this.wkc_obj.geom_obj.all_end_pts.Last(obj => obj.Equals(bz.pt_bz_end_id));
                List<PointF> inter_pts = bz.discretized_pts;

                // remove the first and last point for bezier
                inter_pts.RemoveAt(inter_pts.Count - 1);
                inter_pts.RemoveAt(0);

                temp_poly = new clipper_polylines_store(bz.bezier_id, bz.pt_bz_start_id, bz.pt_bz_end_id, 3,
                    t_start_pt.x, t_start_pt.y, inter_pts, t_end_pt.x, t_end_pt.y);

                // Only check self intersection for bezier curves
                if (temp_poly.is_selfintersecting() == false)
                {
                    // Add to the list of poly lines
                    poly_lines.Add(temp_poly);
                }
            }

            bool is_simple_closed_curves;
            do
            {
                // Remove polylines which are NOT part of the closed boundary
                remove_open_poly_lines(ref poly_lines);
                // Remove polylines which are intersecting
                is_simple_closed_curves = remove_intersecting_poly_lines(ref poly_lines);

            } while (is_simple_closed_curves == false);

            // Set the ccw and cw edges of all the polygons
            foreach (clipper_polylines_store p_line in poly_lines)
            {
                // get a subset other than the current polyline
                HashSet<clipper_polylines_store> plines_subset = new HashSet<clipper_polylines_store>(poly_lines);
                HashSet<clipper_polylines_store> temp_subset = new HashSet<clipper_polylines_store>();
                temp_subset.Add(p_line);
                plines_subset.ExceptWith(temp_subset);

                p_line.set_polygon_edges(plines_subset);
            }

            // Set whether the boundary loop of all the polylines
            foreach (clipper_polylines_store p_line in poly_lines)
            {
                // get a subset other than the current polyline
                HashSet<clipper_polylines_store> plines_subset = new HashSet<clipper_polylines_store>(poly_lines);
                HashSet<clipper_polylines_store> temp_subset = new HashSet<clipper_polylines_store>();
                temp_subset.Add(p_line);
                plines_subset.ExceptWith(temp_subset);

                p_line.set_polygon_loop(plines_subset);
            }

            // Set the closed boundaries
            HashSet<clipper_surface_store> closed_bndries = get_clipper_surface_from_polylines(ref poly_lines);
            return closed_bndries;
        }

        private void remove_open_poly_lines(ref HashSet<clipper_polylines_store> poly_lines)
        {
            // Poly lines which doesnot belong to any closed boundary will be removed
            bool is_open_poly_found = false;
            int i = 0;

            HashSet<clipper_polylines_store> all_poly_lines = new HashSet<clipper_polylines_store>(poly_lines);

            // remove the poly lines whose start and end points are the same (Its a special case only for bezier)
            HashSet<clipper_polylines_store> bezier_loop = new HashSet<clipper_polylines_store>();
            foreach (clipper_polylines_store pl in poly_lines)
            {
                if (pl.poly_type == 3)
                {
                    if (pl.poly_start_id == pl.poly_end_id)
                    {
                        all_poly_lines.Remove(pl);
                        continue;
                    }
                }
            }


            do
            {
                is_open_poly_found = false;
                i = 0;
                foreach (clipper_polylines_store p_line in all_poly_lines)
                {
                    // get a subset other than the current polyline
                    HashSet<clipper_polylines_store> plines_subset = new HashSet<clipper_polylines_store>(all_poly_lines);
                    HashSet<clipper_polylines_store> temp_subset = new HashSet<clipper_polylines_store>();
                    temp_subset.Add(p_line);
                    plines_subset.ExceptWith(temp_subset);

                    // Find all the polylines connected to the current polyline at start point
                    //List<int> pl_id_store = new List<int>();
                    int s_id_count = 0;
                    foreach (clipper_polylines_store pl_startchk in plines_subset)
                    {
                        if (pl_startchk.is_attached_to_ptid(p_line.poly_start_id) == true)
                        {
                            s_id_count++;
                        }
                    }

                    // Find all the polylines connected to the current polyline at end point 
                    int e_id_count = 0;
                    foreach (clipper_polylines_store pl_endchk in plines_subset)
                    {
                        if (pl_endchk.is_attached_to_ptid(p_line.poly_end_id) == true)
                        {
                            e_id_count++;
                        }
                    }

                    if (s_id_count == 0 || e_id_count == 0)
                    {
                        is_open_poly_found = true;
                        break;
                    }
                    i++;
                }

                // A poly line which is not connected to other poly line is found so remove
                if (is_open_poly_found == true)
                {
                    poly_lines.Remove(all_poly_lines.ElementAt(i));
                    all_poly_lines.Remove(all_poly_lines.ElementAt(i));
                }
            } while (is_open_poly_found == true);
        }

        private bool remove_intersecting_poly_lines(ref HashSet<clipper_polylines_store> poly_lines)
        {
            // Only simple closed curves are recogonized (Not simple closed curves are removed)
            // Intersecting polylines are removed
            bool is_simple_closed_curve = true;

            foreach (clipper_polylines_store p_line in poly_lines)
            {
                // get a subset other than the current polyline
                HashSet<clipper_polylines_store> plines_subset = new HashSet<clipper_polylines_store>(poly_lines);
                HashSet<clipper_polylines_store> temp_subset = new HashSet<clipper_polylines_store>();
                temp_subset.Add(p_line);
                plines_subset.ExceptWith(temp_subset);

                // search through all the other polylines whether intersection is happening
                foreach (clipper_polylines_store other_pl in plines_subset)
                {
                    if (p_line.is_intersecting(other_pl) == true)
                    {
                        // Remove because intersection is found
                        is_simple_closed_curve = false;
                        poly_lines.Remove(other_pl);
                        break;
                    }
                }
                if (is_simple_closed_curve == false)
                {
                    // remove because intersection is found
                    poly_lines.Remove(p_line);
                    break;
                }
            }
            return is_simple_closed_curve;
        }

        private HashSet<clipper_surface_store> get_clipper_surface_from_polylines(ref HashSet<clipper_polylines_store> poly_lines)
        {
            List<clipper_surface_store> temp_clip_surf_list = new List<clipper_surface_store>();
            clipper_surface_store temp_clip_surf;
            int s_id = 0;

            foreach (clipper_polylines_store p_line in poly_lines)
            {
                // Surface 1
                bool is_add = true;
                temp_clip_surf = new clipper_surface_store(s_id, p_line.loop_ccw_poly_ids, p_line.loop_ccw_endpt_ids, p_line.ccw_loop_pts, false);
                // Check whether loop is already in the list
                foreach (clipper_surface_store surf in temp_clip_surf_list)
                {
                    if (surf.closed_loop_bndry_id.Except(temp_clip_surf.closed_loop_bndry_id).Count() == 0)
                    {
                        is_add = false;
                        break;
                    }
                }

                if (is_add == true)
                {
                    temp_clip_surf_list.Add(temp_clip_surf);
                    s_id++;
                }

                // Surface 2
                is_add = true;
                temp_clip_surf = new clipper_surface_store(s_id, p_line.loop_w_poly_ids, p_line.loop_w_endpt_ids, p_line.w_loop_pts, false);
                // Check whether loop is already in the list
                foreach (clipper_surface_store surf in temp_clip_surf_list)
                {
                    if (surf.closed_loop_bndry_id.Except(temp_clip_surf.closed_loop_bndry_id).Count() == 0)
                    {
                        is_add = false;
                        break;
                    }
                }

                if (is_add == true)
                {
                    temp_clip_surf_list.Add(temp_clip_surf);
                    s_id++;
                }
            }

            // Sort with ascending poly area
            temp_clip_surf_list.Sort(new clipper_surface_Comparer());

            // Create surface for output
            HashSet<clipper_surface_store> rslt_clip_surf = new HashSet<clipper_surface_store>();
            s_id = 0;
            foreach (clipper_surface_store surf in temp_clip_surf_list)
            {
                temp_clip_surf = new clipper_surface_store(s_id,
                    surf.closed_loop_bndry_id,
                    surf.closed_loop_pt_id,
                    surf.polygon_loop_pts, true);
                // Add to the new output list
                rslt_clip_surf.Add(temp_clip_surf);
                s_id++;
            }

            // nest this surface inside others
            for (int i = 0; i < (rslt_clip_surf.Count); i++)
            {
                HashSet<clipper_surface_store> nested_poly_chk = new HashSet<clipper_surface_store>();
                for (int j = i + 1; j < rslt_clip_surf.Count; j++)
                {
                    // Add to the nested poly list check to check against this poly
                    nested_poly_chk.Add(rslt_clip_surf.ElementAt(j));
                }
                // Complete the nesting
                rslt_clip_surf.ElementAt(i).set_nest_this_surface(nested_poly_chk);
            }

            return rslt_clip_surf;
        }

        #endregion

        private void surface_form_api_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Set surface form close
            wkc_obj.cancel_operation();
            the_parent.surface_frm_closed();
            gvariables.Is_surface_frm_open = false;
            the_parent.mt_pic.Refresh();
        }

        private void toolStripMenuItem_add_ItemClicked(object sender, EventArgs e)
        {
            // Add surface bar check
            if (toolStripMenuItem_add.Checked == false)
            {
                this.is_add = true;
                toolStripMenuItem_add.Checked = true;
                toolStripMenuItem_add.BackColor = Color.FromArgb(ProfessionalColors.ButtonCheckedHighlight.ToArgb()); // Checked
                toolStripMenuItem_delete.Checked = false;
                toolStripMenuItem_delete.BackColor = Control.DefaultBackColor; // unchecked
            }
        }

        private void toolStripMenuItem_delete_ItemClicked(object sender, EventArgs e)
        {
            // Delete surface toolbar check
            if (toolStripMenuItem_delete.Checked == false)
            {
                this.is_add = false;
                toolStripMenuItem_add.Checked = false;
                toolStripMenuItem_add.BackColor = Control.DefaultBackColor; // unchecked
                toolStripMenuItem_delete.Checked = true;
                toolStripMenuItem_delete.BackColor = Color.FromArgb(ProfessionalColors.ButtonCheckedHighlight.ToArgb()); // Checked
            }
        }

        private KnownColor getRandomColor()
        {
            // colorList[rand.Next(0, maxColorIndex)];
            return colorList[rand.Next(48, 70)];
        }

    }
}
