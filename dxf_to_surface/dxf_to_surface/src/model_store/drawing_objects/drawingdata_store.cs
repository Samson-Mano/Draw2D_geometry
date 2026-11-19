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


        private void Line_Line_Intersection_and_Splitting()
        {
            // Perform line-line intersection and splitting
            // Line - Line Intersection and Splitting

            // Dictionary to store line id and their corresponding
            Dictionary<int, List<int>> line_intersections = new Dictionary<int, List<int>>();

            // Create a list of lines to process
            foreach (var line in line_data.lineMap.Values)
            {
                line_intersections[line.line_id] = new List<int>();
            }


            for (int i = 0; i < line_data.lineMap.Count; i++)
            {
                var line1 = line_data.lineMap.ElementAt(i).Value;

                for (int j = i + 1; j < line_data.lineMap.Count; j++)
                {
                    var line2 = line_data.lineMap.ElementAt(j).Value;
                    if (line1.line_id != line2.line_id)
                    {
                        if (gvariables_static.DoLinesIntersect(line1.lineStartPt, line1.lineEndPt, line2.lineStartPt, line2.lineEndPt))
                        {
                            // Store the intersection information
                            line_intersections[line1.line_id].Add(line2.line_id);
                            line_intersections[line2.line_id].Add(line1.line_id);
                        }
                    }
                }
            }


            // Create a new list to hold the split lines
            List<line_store> new_lines = new List<line_store>();


            foreach (var kvp in line_intersections)
            {
                int lineId = kvp.Key;
                List<int> intersectingLineIds = kvp.Value;
                // Get the original line
                var originalLine = line_data.lineMap[lineId];

                List<Vector2> intersectionPoints = new List<Vector2>();

                // Find the intersection points with the intersecting lines
                foreach (int intersectingLineId in intersectingLineIds)
                {
                    var intersectingLine = line_data.lineMap[intersectingLineId];

                    Vector2 intersectionPoint = gvariables_static.GetLineSegmentIntersectionPoint(originalLine.lineStartPt, originalLine.lineEndPt, intersectingLine.lineStartPt, intersectingLine.lineEndPt);

                    // Add the intersection point to the original line's intersection points
                    intersectionPoints.Add(intersectionPoint);

                }

                // Order the intersection points along the original line
                List<Vector2> orderedPoints = new List<Vector2>();

                orderedPoints.Add(originalLine.lineStartPt);
                orderedPoints.AddRange(intersectionPoints.OrderBy(pt => (pt - originalLine.lineStartPt).LengthSquared));
                orderedPoints.Add(originalLine.lineEndPt);


                // Create new lines from the ordered points
                for (int i = 0; i < orderedPoints.Count - 1; i++)
                {
                    Vector2 startPt = orderedPoints[i];
                    Vector2 endPt = orderedPoints[i + 1];
                    // Create a new line segment
                    line_store newLine = new line_store
                    {
                        line_id = 0,
                        lineStartPt = startPt,
                        lineEndPt = endPt
                    };

                    new_lines.Add(newLine);
                }

            }

            // Clear the original lines and add the new split lines
            line_data.clear_lines();

            int LineId = 0;

            foreach (var newLine in new_lines)
            {
                line_data.add_line(LineId, newLine.lineStartPt, newLine.lineEndPt);
                LineId++;
            }



        }



        public void perform_intersection_and_splitting()
        {

           




        }



    }
}
