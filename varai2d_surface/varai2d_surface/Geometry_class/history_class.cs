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
    public class history_class
    {
        private HashSet<lines_store> _U_lines = new HashSet<lines_store>();
        private HashSet<arcs_store> _U_arcs = new HashSet<arcs_store>();
        private HashSet<beziers_store> _U_beziers = new HashSet<beziers_store>();
        private HashSet<points_store> _U_end_pts = new HashSet<points_store>();
        private HashSet<surface_store> _U_surfaces = new HashSet<surface_store>();
        private member_id_control _U_id_control = new member_id_control();

        private geom_class _geom_obj;
        private double _zoom_value;
        private double _scale_value;
        private PointF _transl_center;

        public geom_class geom_obj { get { return this._geom_obj; } }

        public double zoom_value { get { return this._zoom_value; } }

        public double scale_value { get { return this._scale_value; } }

        public PointF transl_center { get { return this._transl_center; } }


        public history_class(geom_class t_geom_obj, double t_zoom_value, double t_scale_value, PointF t_transl_center)
        {
            // Main constructor to store the history of operations
            // Add lines, arcs, beziers, points and id control data to the lists
            // Lines
            this._U_lines = new HashSet<lines_store>();
            this._U_lines.UnionWith(t_geom_obj.all_lines);

            // Arcs
            this._U_arcs = new HashSet<arcs_store>();
            this._U_arcs.UnionWith(t_geom_obj.all_arcs);

            // Beziers
            this._U_beziers = new HashSet<beziers_store>();
            this._U_beziers.UnionWith(t_geom_obj.all_beziers);

            // End points
            this._U_end_pts = new HashSet<points_store>();
            this._U_end_pts.UnionWith(t_geom_obj.all_end_pts);

            // ID control
            this._U_id_control = new member_id_control(t_geom_obj.id_control.point_id,
                t_geom_obj.id_control.line_id,
                t_geom_obj.id_control.arc_id,
                t_geom_obj.id_control.bezier_id,
                t_geom_obj.id_control.member_id0);

            this._geom_obj = new geom_class(this._U_lines,
                this._U_arcs,
                this._U_beziers,
                this._U_end_pts,
                this._U_surfaces,
                this._U_id_control);

            // Save the current graphics state
            this._zoom_value = t_zoom_value;
            this._scale_value = t_scale_value;
            this._transl_center =t_transl_center;
        }

    }
}
