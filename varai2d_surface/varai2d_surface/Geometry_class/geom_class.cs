using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.global_static;
using varai2d_surface.Geometry_class.geometry_store;
using varai2d_surface.Geometry_class.add_operation;

namespace varai2d_surface.Geometry_class
{
    [Serializable]
    public class geom_class
    {
        HashSet<lines_store> _all_lines = new HashSet<lines_store>();
        HashSet<arcs_store> _all_arcs = new HashSet<arcs_store>();
        HashSet<beziers_store> _all_beziers = new HashSet<beziers_store>();
        HashSet<points_store> _all_end_pts = new HashSet<points_store>();
        HashSet<surface_store> _all_surfaces = new HashSet<surface_store>();

        member_id_control _id_control = new member_id_control();

        public HashSet<points_store> all_end_pts { get { return this._all_end_pts; } }

        public HashSet<lines_store> all_lines { get { return this._all_lines; } }

        public HashSet<arcs_store> all_arcs { get { return this._all_arcs; } }

        public HashSet<beziers_store> all_beziers { get { return this._all_beziers; } }

        public HashSet<surface_store> all_surfaces { get { return this._all_surfaces; } }

        public member_id_control id_control { get { return this._id_control; } }

        public geom_class()
        { // Empty constructor
        }

        public geom_class(HashSet<lines_store> t_all_lines,
             HashSet<arcs_store> t_all_arcs,
             HashSet<beziers_store> t_all_beziers,
             HashSet<points_store> t_all_end_pts,
             HashSet<surface_store> t_all_surfaces,
             member_id_control t_id_tcontrol)
        {
            // Constructor to renew the entire data
            // Lines
            this._all_lines = new HashSet<lines_store>();
            this._all_lines.UnionWith(t_all_lines);

            // Arcs
            this._all_arcs = new HashSet<arcs_store>();
            this._all_arcs.UnionWith(t_all_arcs);

            // Beziers
            this._all_beziers = new HashSet<beziers_store>();
            this._all_beziers.UnionWith(t_all_beziers);

            // Points
            this._all_end_pts = new HashSet<points_store>();
            this._all_end_pts.UnionWith(t_all_end_pts);

            // Surfaces
            this._all_surfaces = new HashSet<surface_store>();
            this.all_surfaces.UnionWith(t_all_surfaces);

            // Ids
            this._id_control = new member_id_control(t_id_tcontrol.point_id,
                t_id_tcontrol.line_id,
                t_id_tcontrol.arc_id,
                t_id_tcontrol.bezier_id,
                t_id_tcontrol.member_id0);
        }

        public void remove_points(HashSet<points_store> points_to_be_removed)
        {
            foreach (points_store r_pts in points_to_be_removed)
            {
                this._id_control.delete_point_id(r_pts.nd_id);
            }

            // Remove point
            IEnumerable<points_store> rslt_set = this._all_end_pts.Except(points_to_be_removed);
            this._all_end_pts = rslt_set.ToHashSet();
        }

        public void remove_lines(HashSet<lines_store> lines_to_be_removed)
        {
            foreach (lines_store r_ln in lines_to_be_removed)
            {
                this._id_control.delete_member_id(r_ln.line_id, 1);
            }

            // Remove lines 
            IEnumerable<lines_store> rslt_set = this._all_lines.Except(lines_to_be_removed);
            this._all_lines = rslt_set.ToHashSet();
        }

        public void remove_arcs(HashSet<arcs_store> arcs_to_be_removed)
        {
            foreach (arcs_store r_ar in arcs_to_be_removed)
            {
                this._id_control.delete_member_id(r_ar.arc_id, 2);
            }

            // Remove arcs
            IEnumerable<arcs_store> rslt_set = this._all_arcs.Except(arcs_to_be_removed);
            this._all_arcs = rslt_set.ToHashSet();
        }

        public void remove_beziers(HashSet<beziers_store> beziers_to_be_removed)
        {
            foreach (beziers_store r_bz in beziers_to_be_removed)
            {
                this._id_control.delete_member_id(r_bz.bezier_id, 3);
            }

            // Remove arcs
            IEnumerable<beziers_store> rslt_set = this._all_beziers.Except(beziers_to_be_removed);
            this._all_beziers = rslt_set.ToHashSet();
        }

        public void add_line(int line_id, points_store ts_pt, points_store te_pt)
        {
            if (ts_pt.check_point_snap(te_pt) == true)
            {
                // both are same points (Exit)
                return;
            }

            // First point
            // Check whether the point already exists
            if (this._all_end_pts.Contains(this._all_end_pts.LastOrDefault(obj => obj.Equals(ts_pt)), new PointsComparer()) == false)
            {
                // This is a new point
                this._id_control.add_point_id(ts_pt.nd_id);
                this._all_end_pts.Add(ts_pt);
            }

            // Second point
            // Check whether the point already exists
            if (this._all_end_pts.Contains(this._all_end_pts.LastOrDefault(obj => obj.Equals(te_pt)), new PointsComparer()) == false)
            {
                // This is a new point
                this._id_control.add_point_id(te_pt.nd_id);
                this._all_end_pts.Add(te_pt);
            }

            // Add line
            lines_store temp_ln = new lines_store(line_id, ts_pt.nd_id, te_pt.nd_id, all_end_pts);
            // Check whether the line already existis ?? 
            if (all_lines.Contains(temp_ln, new LinesComparer()) == true)
            {
                return;
            }

            this._id_control.add_member_id(temp_ln.line_id, 1);
            this._all_lines.Add(temp_ln);
        }

        public void add_arc(int arc_id, points_store t_chord_s_pt, points_store t_chord_e_pt, PointF cntrl_pt, PointF center_pt)
        {
            if (t_chord_s_pt.check_point_snap(t_chord_e_pt) == true)
            {
                // Chord points coincide (Exit)
                return;
            }

            // First chord point check
            // Check whether the point already exists
            if (this._all_end_pts.Contains(this._all_end_pts.LastOrDefault(obj => obj.Equals(t_chord_s_pt)), new PointsComparer()) == false)
            {
                // This is a new point
                this._id_control.add_point_id(t_chord_s_pt.nd_id);
                this._all_end_pts.Add(t_chord_s_pt);
            }

            // Second chord point check
            // Check whether the point already exists
            if (this._all_end_pts.Contains(this._all_end_pts.LastOrDefault(obj => obj.Equals(t_chord_e_pt)), new PointsComparer()) == false)
            {
                // This is a new point
                this._id_control.add_point_id(t_chord_e_pt.nd_id);
                this._all_end_pts.Add(t_chord_e_pt);
            }

            // Add Arc
            arcs_store temp_arc = new arcs_store(arc_id, t_chord_s_pt.nd_id, t_chord_e_pt.nd_id, cntrl_pt, center_pt, all_end_pts);
            // Check whether the line already existis ?? 
            if (all_arcs.Contains(temp_arc, new ArcsComparer()) == true)
            {
                return;
            }

            this._id_control.add_member_id(temp_arc.arc_id, 2);
            this._all_arcs.Add(temp_arc);
        }

        public void add_bezier(int bezier_id, int poly_count, points_store ts_pt, points_store te_pt, List<PointF> cntrl_pts)
        {
            // First point
            // Check whether the point already exists
            if (this._all_end_pts.Contains(this._all_end_pts.LastOrDefault(obj => obj.Equals(ts_pt)), new PointsComparer()) == false)
            {
                // This is a new point
                this._id_control.add_point_id(ts_pt.nd_id);
                this._all_end_pts.Add(ts_pt);
            }

            // Second point
            // Check whether the point already exists
            if (this._all_end_pts.Contains(this._all_end_pts.LastOrDefault(obj => obj.Equals(te_pt)), new PointsComparer()) == false)
            {
                // This is a new point
                this._id_control.add_point_id(te_pt.nd_id);
                this._all_end_pts.Add(te_pt);
            }

            // Add Bezier
            beziers_store temp_bz = new beziers_store(bezier_id, poly_count, ts_pt.nd_id, te_pt.nd_id, cntrl_pts, all_end_pts);
            // Check whether the Bezier curve already existis ?? 
            //if (all_lines.Contains(temp_ln, new LinesComparer()) == true)
            //{
            //    return;
            //}

            this._id_control.add_member_id(temp_bz.bezier_id, 3);
            this._all_beziers.Add(temp_bz);

        }

        public void add_surface(surface_store surf)
        {
            // add surface
            this._all_surfaces.Add(surf);
            associate_to_surface(true, surf);

        }

        public void remove_surface(surface_store surf)
        {
            // remove surface
            associate_to_surface(false, surf);
            this._all_surfaces.Remove(surf);
        }

        private void associate_to_surface(bool is_associate, surface_store surf)
        {
            // Get all the member IDs associated with this surface (both the outter member and nested members)
            HashSet<int> surf_member_ids = new HashSet<int>(surf.closed_loop_bndry_id);
            surf_member_ids.UnionWith(surf.get_nested_surfaces_boundary_member_ids());

            // Add association
            // Find all line ids associated with this surface
            HashSet<int> temp_mem_ids = new HashSet<int>();
            foreach (lines_store ln in this.all_lines)
            {
                temp_mem_ids.Add(ln.line_id);
            }
            HashSet<int> tln_ids = temp_mem_ids.Intersect(surf_member_ids).ToHashSet();
            // Associate or Disassociate the lines
            foreach (int ln_id in tln_ids)
            {
                this.all_lines.Last(obj => obj.Equals(ln_id)).set_association_to_surface(is_associate, surf.surf_id);
            }


            // Find all arc ids associated with this surface
            temp_mem_ids.Clear();
            foreach (arcs_store arc in this.all_arcs)
            {
                temp_mem_ids.Add(arc.arc_id);
            }
            HashSet<int> tarc_ids = temp_mem_ids.Intersect(surf_member_ids).ToHashSet();
            // Associate or Disassociate the Arcs
            foreach (int arc_id in tarc_ids)
            {
                this.all_arcs.Last(obj => obj.Equals(arc_id)).set_association_to_surface(is_associate, surf.surf_id);
            }

            // Find all bezier ids associated with this surface
            temp_mem_ids.Clear();
            foreach (beziers_store bz in this.all_beziers)
            {
                temp_mem_ids.Add(bz.bezier_id);
            }
            HashSet<int> tbz_ids = temp_mem_ids.Intersect(surf_member_ids).ToHashSet();
            // Associate or Disassociate the Beziers
            foreach (int bz_id in tbz_ids)
            {
                this.all_beziers.Last(obj => obj.Equals(bz_id)).set_association_to_surface(is_associate, surf.surf_id);
            }

        }


        public void paint_geometry(Graphics gr0, bool paint_transparent)
        {
            // Update the line color
            if (paint_transparent == true)
            {
                gvariables.Trans_PV = 150;
                gfunctions.update_pen();
            }

            // Paint surfaces
            foreach (surface_store g_surf in all_surfaces)
            {
                g_surf.paint_surface(gr0);
            }

            // Paint lines
            foreach (lines_store g_line in all_lines)
            {
                g_line.paint_lines(gr0);
            }

            // Paint arcs
            foreach (arcs_store g_arc in all_arcs)
            {
                g_arc.paint_arcs(gr0);
            }

            // Paint arcs
            foreach (beziers_store g_bezier in _all_beziers)
            {
                g_bezier.paint_beziers(gr0);
            }

            // Paint points
            if (gvariables.Is_paint_pt == true)
            {
                foreach (points_store g_points in all_end_pts)
                {
                    g_points.paint_point(gr0);
                }
            }

            // restore the pen to original
            if (paint_transparent == true)
            {
                gvariables.Trans_PV = 0;
                gfunctions.update_pen();
            }
        }
    }
}
