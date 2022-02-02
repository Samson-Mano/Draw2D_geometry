using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.global_static;
using varai2d_surface.Drawing_area;
using varai2d_surface.Geometry_class.modify_operation;
using varai2d_surface.Geometry_class.add_operation;
using varai2d_surface.Geometry_class.geometry_store;
using System.Windows.Forms;

namespace varai2d_surface.Geometry_class
{
    [Serializable]
    public class workarea_control
    {
        private List<history_class> _histU = new List<history_class>();
        private List<history_class> _histR = new List<history_class>();
        private geom_class _geom_obj;
        private snap_predicate _snap_obj;
        private interim_geom_class _interim_obj;
        private modify_member_control _modify_obj;
        private add_member_control _add_obj;

        private int _actions_count;

        public int actions_count { get { return this._actions_count; } }

        public geom_class geom_obj { get { return this._geom_obj; } set { this._geom_obj = value; } }

        public snap_predicate snap_obj { get { return this._snap_obj; } set { this._snap_obj = value; } }

        public interim_geom_class interim_obj { get { return this._interim_obj; } set { this._interim_obj = value; } }

        public int undo_operation_count { get { return this._histU.Count; } }

        public int redo_operation_count { get { return this._histR.Count; } }

        public workarea_control()
        {
            // Main constructor
            this._actions_count = 0;

            // Initialize all items
            this._geom_obj = new geom_class();
            this._snap_obj = new snap_predicate();
            this._interim_obj = new interim_geom_class();
            this._modify_obj = new modify_member_control();
            this._add_obj = new add_member_control();
        }

        public void cntrl_Z()
        {
            // Undo operation cntrl + Z
            if (this._histU.Count != 0)
            {
                this._histR.Add(new history_class(this._geom_obj, gvariables.zoom_factor, gvariables.scale_factor, gvariables.mainpic_center));

                this._geom_obj = new geom_class(this._histU[this._histU.Count - 1].geom_obj.all_lines,
                    this._histU[this._histU.Count - 1].geom_obj.all_arcs,
                    this._histU[this._histU.Count - 1].geom_obj.all_beziers,
                    this._histU[this._histU.Count - 1].geom_obj.all_end_pts,
                    this._histU[this._histU.Count -1].geom_obj.all_surfaces,
                     this._histU[this._histU.Count - 1].geom_obj.id_control);

                // Retrive the previous state
                //this._geom_obj = this._histU[this._histU.Count - 1].geom_obj;
                gvariables.zoom_factor = this._histU[this._histU.Count - 1].zoom_value;
                gvariables.scale_factor = this._histU[this._histU.Count - 1].scale_value;
                gvariables.mainpic_center = this._histU[this._histU.Count - 1].transl_center;

                // Remove the Last
                this._histU.RemoveAt(this._histU.Count - 1);
            }
        }

        public void cntrl_R()
        {
            // Redo operation cntrl + R
            if (this._histR.Count != 0)
            {
                this._histU.Add(new history_class(this._geom_obj, gvariables.zoom_factor, gvariables.scale_factor, gvariables.mainpic_center));
                this._geom_obj = new geom_class(this._histR[this._histR.Count - 1].geom_obj.all_lines,
                    this._histR[this._histR.Count - 1].geom_obj.all_arcs,
                    this._histR[this._histR.Count - 1].geom_obj.all_beziers,
                    this._histR[this._histR.Count - 1].geom_obj.all_end_pts,
                    this._histR[this._histR.Count - 1].geom_obj.all_surfaces,
                     this._histR[this._histR.Count - 1].geom_obj.id_control);

                // Retrive the previous state
                //this._geom_obj = this._histR[this._histR.Count - 1].geom_obj;
                gvariables.zoom_factor = this._histR[this._histR.Count - 1].zoom_value;
                gvariables.scale_factor = this._histR[this._histR.Count - 1].scale_value;
                gvariables.mainpic_center = this._histR[this._histR.Count - 1].transl_center;

                // Remove the Last
                this._histR.RemoveAt(this._histR.Count - 1);
            }
        }

        public void save_state()
        {
            // Save the current state of the geometry before performing any add, modify or scale operation
            this._histU.Add(new history_class(geom_obj, gvariables.zoom_factor, gvariables.scale_factor, gvariables.mainpic_center));
           
            if(_histU.Count>10)
            {
                // Only 10 instances are saved
                this._histU.RemoveAt(0);
            }
        }

        public bool mouse_click(bool operation_cancel, PointF pt_location)
        {
            if (operation_cancel == true)
            {
                // cancel the operation
                cancel_operation();
                return false;
            }

            // Left Mouse click 
            Tuple<bool, int> rslt = new Tuple<bool, int>(false, 0);
            points_store s_pt = snap_obj.get_snap_point(pt_location, geom_obj);
            rslt = this._interim_obj.update_click_pts(s_pt);

            if (rslt.Item1 != false)
            {
                if (rslt.Item2 > 0 && rslt.Item2 < 6)
                {
                    // Process Addition
                    this._add_obj.add_member(rslt.Item2, this);
                    cancel_operation();
                    return false;
                }
                else if (rslt.Item2 > 5 && rslt.Item2 < 9)
                {
                    // Process Modification
                    this._modify_obj.modify_member(rslt.Item2, this);
                    cancel_operation();
                    return false;
                }
                else
                {
                    // Selection in progress clear the click points
                    this._snap_obj.clear_temp_point();
                    // continue selection process
                    return true;
                }
            }

            // continue operation
            return true;
        }

        public void button_click(int c_state)
        {
            if (c_state == 9 || c_state == 10 || c_state == 11)
            {
                // Delete or Intersect or Split operation
                this._modify_obj.modify_member(c_state, this);
            }
            cancel_operation();
        }

        public void cancel_operation()
        {
            // Cancel all the operation in progress
            this._snap_obj.clear_temp_point();
            this._interim_obj.clear_interim();
        }

        public void scale_to_fit(int main_pic_width, int main_pic_height)
        {
            // Save the state before scale transform
            save_state();
            // Scale transform the geometry to fit the view at zoom factor 1.0f
            new scale_transformation(main_pic_width, main_pic_height, this);

            // Pan and zoom to origin
            gvariables.mainpic_center = new PointF((main_pic_width * 0.5f), (main_pic_height * 0.5f));
            gvariables.zoom_factor = 1.01f;

        }

        //public void selection_operation_inprogress(bool Is_selection)
        //{
        //    // Is selection operation in progress
        //    // this._interim_obj.Is_selection = Is_selection;
        //}

        public void select_objects(RectangleF selection_rect, bool Is_shiftselect)
        {
            // Select completes
            this._interim_obj.update_selection_list(selection_rect, geom_obj, Is_shiftselect);

        }


    }
}
