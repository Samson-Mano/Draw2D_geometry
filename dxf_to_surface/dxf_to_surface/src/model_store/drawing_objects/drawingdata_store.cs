using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public class drawingdata_store
    {

        public linelist_store line_data; // { get; }
        public circlelist_store circle_data; // { get; }
        public ellipselist_store ellipse_data; // { get; }
        public splinelist_store spline_data; // { get; }


        public drawingdata_store()
        {
            line_data = new linelist_store();
            circle_data = new circlelist_store();
            ellipse_data = new ellipselist_store();
            spline_data = new splinelist_store();

        }

        public void paint_drawing_data()
        {
            line_data.paint_lines();
            circle_data.paint_circles();
            ellipse_data.paint_ellipses();
            spline_data.paint_splines();

        }


        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            // Update the drawing object uniforms
            line_data.update_openTK_uniforms(graphic_events_control);
            circle_data.update_openTK_uniforms(graphic_events_control);
            ellipse_data.update_openTK_uniforms(graphic_events_control);
            spline_data.update_openTK_uniforms(graphic_events_control);

        }

        public void clear_drawing_data()
        {
            line_data.clear_lines();
            circle_data.clear_circles();
            ellipse_data.clear_ellipses();
            spline_data.clear_splines();
        }



        public void perform_intersection_and_splitting()
        {

            // Perform intersection and splitting for lines, circles, and ellipses
            IntersectionSplitting.Intersection_and_Splitting(ref line_data, ref circle_data, ref ellipse_data);


        }



    }
}
