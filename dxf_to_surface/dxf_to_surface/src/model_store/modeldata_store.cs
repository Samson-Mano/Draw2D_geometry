// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store.drawing_objects;
using dxf_to_surface.src.model_store.geom_objects;
// using dxf_to_surface.src.model_store.rslt_objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.model_store
{
    public class modeldata_store
    {

        // Main Drawing data store
        public drawingdata_store drawing_data;
        //public fedata_store fe_data;

        //// Result data store
        //public rsltdata_store rslt_data;


        //// Contour bar data for results visualization
        //private contourlevelbar_store contour_bar_data;


        // Drawing bound data
        public Vector2 min_bounds = new Vector2(-1);
        public Vector2 max_bounds = new Vector2(1);
        public Vector2 geom_bounds = new Vector2(2);


        public selectrectangle_store selection_rectangle; // { get; }
        public selectcircle_store selection_circle; // { get; }

        // To control the drawing events
        public drawing_events graphic_events_control { get; private set; }

        // Update of mesh properties
        public bool isConstraintUpdateInProgress = false;
        public bool isLoadUpdateInProgress = false;
        public bool isMaterialUpdateInProgress = false;
        public bool isAnnotateResultInProgress = false;

        public bool IsModelSet = false;


        //// Animation control data
        //public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();




        public modeldata_store()
        {

            // To control the drawing graphics
            graphic_events_control = new drawing_events(this);

            // Set a default geometry bounds
            min_bounds = new Vector2(-1);
            max_bounds = new Vector2(1);
            geom_bounds = new Vector2(2);


            IsModelSet = false;
        }


        public void InitializeModelGeom()
        {
            // Set the selection rectangle  & selection circle
            selection_rectangle = new selectrectangle_store();
            selection_circle = new selectcircle_store();

            drawing_data = new drawingdata_store();
            //fe_data = new fedata_store();
            //rslt_data = new rsltdata_store();


        }


        public void importFile(string filePath)
        {
            List<Vector2> nodePtsList = new List<Vector2>();
            IsModelSet = false;

            drawing_data = new drawingdata_store();
            //fe_data = new fedata_store();
            //rslt_data = new rsltdata_store();


            // Import type is DXF file DXF 2007
            file_events.import_DXF_file(filePath, ref drawing_data, ref nodePtsList, ref IsModelSet);



            //if (type == 0)
            //{
            //    // Import type is TXT file
            //    string fileContent = System.IO.File.ReadAllText(filePath);
            //    file_events.import_txt_mesh(fileContent, ref fe_data, ref nodePtsList, ref IsModelSet);
            //}
            //else if (type == 1)
            //{
            //    // Import type is BIN file
            //    file_events.import_binary_mesh(filePath, ref fe_data, ref nodePtsList, ref IsModelSet);
            //}


            if (IsModelSet == false)
                return;

            // Set the mesh boundaries
            Vector2 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector2, Vector2> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            this.min_bounds = geom_extremes.Item1; // Minimum bound
            this.max_bounds = geom_extremes.Item2; // Maximum bound

            this.geom_bounds = max_bounds - min_bounds;

            // update the global static value
            gvariables_static.geom_size = this.geom_bounds.Length;


            //fe_data.set_meshdrawing_data();

            //// Initialize contour bar data
            //contour_bar_data.InitializeContourLevelBarData(graphic_events_control.window_width, graphic_events_control.window_height);

            update_openTK_uniforms();

        }


        public void exportBINFile(string filePath)
        {
            if (!IsModelSet)
                return;

            // // Export the bindary mesh
            // file_events.export_binary_mesh(filePath, fe_data);

        }


        public void paint_model()
        {

            if (!IsModelSet)
                return;


            drawing_data.paint_drawing_data();

            //bool hasResults = IsResultSet && gvariables_static.is_paint_result();
            //bool showTransparentMesh = gvariables_static.is_show_transparent_model_mesh;

            //// 1. Paint the model mesh FIRST (as background/underlay)
            //// Always paint model if we want transparent mesh OR if no results are shown
            //if (!hasResults || showTransparentMesh)
            //{
            //    fe_data.paint_model();

            //    if (isMaterialUpdateInProgress == true || isLoadUpdateInProgress == true || isConstraintUpdateInProgress == true)
            //    {
            //        if (gvariables_static.is_RectangleSelection == true)
            //        {
            //            // Paint the selection rectangle
            //            selection_rectangle.draw_selection_rectangle();
            //        }
            //        else
            //        {
            //            // Paint the selection circle
            //            selection_circle.draw_selection_circle();
            //        }
            //    }
            //}

            //// 2. Paint results ON TOP of the mesh
            //if (hasResults)
            //{
            //    rslt_data.paint_results();

            //    contour_bar_data.draw_contour_bar();

            //    if (isAnnotateResultInProgress == true)
            //    {
            //        if (gvariables_static.is_RectangleSelection == true)
            //        {
            //            // Paint the selection rectangle
            //            selection_rectangle.draw_selection_rectangle();
            //        }
            //        else
            //        {
            //            // Paint the selection circle
            //            selection_circle.draw_selection_circle();
            //        }
            //    }

            //}

        }



        public void update_openTK_uniforms()
        {

            if (!IsModelSet)
                return;

            drawing_data.update_openTK_uniforms(graphic_events_control);

            //fe_data.update_openTK_uniforms(graphic_events_control);
            //rslt_data.update_openTK_uniforms(graphic_events_control);


        }


        public void select_model_objects(Vector2 o_pt, Vector2 c_pt, bool isRightButton)
        {
            if (!IsModelSet) return;

            // Perform the select option
            //if (isMaterialUpdateInProgress == true)
            //{
            //    fe_data.select_mesh(o_pt, c_pt, isRightButton, graphic_events_control);

            //}

            //if (isLoadUpdateInProgress == true)
            //{
            //    // Select the points for load update
            //    fe_data.select_nodes(o_pt, c_pt, isRightButton, graphic_events_control);

            //}

            //if (isConstraintUpdateInProgress == true)
            //{
            //    // Select the points for constraint update
            //    fe_data.select_nodes(o_pt, c_pt, isRightButton, graphic_events_control);

            //}

            //if(isAnnotateResultInProgress == true)
            //{
            //    // Select the points for result annotation
            //    rslt_data.select_result_nodes(o_pt, c_pt, isRightButton, graphic_events_control);
            //}

        }



    }
}
