using dxf_to_surface.src.global_variables;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public static class IntersectionSplitting
    {


        public static void Intersection_and_Splitting(ref linelist_store line_data,
            ref circlelist_store circle_data, ref ellipselist_store ellipse_data,
            ref splinelist_store spline_data)
        {
            // Check if the splines self intersect and remove them from the spline data if they do
            // Self-intersecting splines are not supported
            Remove_selfintersecting_splines(ref spline_data);

            // Perform intersection and splitting for lines, circles, and ellipses
            // Hierarchy 1: Line-Line, Line-Circle, Line-Ellipse
            Line_Line_Intersection_and_Splitting(ref line_data);
            Line_Circle_Intersection_and_Splitting(ref line_data, ref circle_data);
            Line_Ellipse_Intersection_and_Splitting(ref line_data, ref ellipse_data);
            Line_Spline_Intersection_and_Splitting(ref line_data, ref spline_data);

            // Hierarchy 2: Circle-Circle, Circle-Ellipse
            Circle_Circle_Intersection_and_Splitting(ref circle_data);
            Circle_Ellipse_Intersection_and_Splitting(ref circle_data, ref ellipse_data);

            // Hierarchy 3: Ellipse-Ellipse
            Ellipse_Ellipse_Intersection_and_Splitting(ref ellipse_data);

        }


        public static void Remove_selfintersecting_splines(ref splinelist_store spline_data)
        {
            // Check if the splines self intersect
            List<int> selfIntersectingSplineIds = new List<int>();

            foreach (spline_store spline in spline_data.splineMap.Values)
            {
                if (DoesSplineSelfIntersect(spline.degree, spline.isPeriodic, 
                    spline.isRational, spline.weights, spline.knots, spline.controlPoints))
                {
                    selfIntersectingSplineIds.Add(spline.spline_id);
                }
            }

            // Remove self-intersecting splines from the spline data
            foreach (int splineId in selfIntersectingSplineIds)
            {
                spline_data.remove_spline(splineId);
            }
        }





        public static void Line_Line_Intersection_and_Splitting(ref linelist_store line_data)
        {
            // Perform line-line intersection and splitting
            // Line - Line Intersection and Splitting

            // Dictionary to store line id and their corresponding intersecting line ids
            Dictionary<int, List<int>> line_intersections = new Dictionary<int, List<int>>();

            // Create a list of lines to process
            foreach (line_store line in line_data.lineMap.Values)
            {
                line_intersections[line.line_id] = new List<int>();
            }


            for (int i = 0; i < line_data.lineMap.Count; i++)
            {
                line_store line1 = line_data.lineMap.ElementAt(i).Value;

                for (int j = i + 1; j < line_data.lineMap.Count; j++)
                {
                    line_store line2 = line_data.lineMap.ElementAt(j).Value;

                    // Gaurd to prevent line self intersection
                    if (line1.line_id != line2.line_id)
                    {
                        if (DoLineSegmentsIntersect(line1.lineStartPt, line1.lineEndPt, line2.lineStartPt, line2.lineEndPt))
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
                line_store originalLine = line_data.lineMap[lineId];

                List<Vector2> intersectionPoints = new List<Vector2>();

                // Find the intersection points with the intersecting lines
                foreach (int intersectingLineId in intersectingLineIds)
                {
                    line_store intersectingLine = line_data.lineMap[intersectingLineId];

                    Vector2 intersectionPoint = GetLineSegmentIntersectionPoint(originalLine.lineStartPt,
                        originalLine.lineEndPt, intersectingLine.lineStartPt, intersectingLine.lineEndPt);

                    // Add the intersection point to the original line's intersection points
                    intersectionPoints.Add(intersectionPoint);

                }

                // Order the intersection points along the original line
                List<Vector2> orderedPoints = new List<Vector2> { originalLine.lineStartPt };

                foreach (var p in intersectionPoints.OrderBy(pt => (pt - originalLine.lineStartPt).LengthSquared))
                {
                    if ((p - orderedPoints[orderedPoints.Count - 1]).LengthSquared > 1e-12)
                        orderedPoints.Add(p);
                }

                if ((originalLine.lineEndPt - orderedPoints[orderedPoints.Count - 1]).LengthSquared > 1e-12)
                {
                    // Final intersection point is not too close to the last point
                    orderedPoints.Add(originalLine.lineEndPt);
                }
                else
                {
                    orderedPoints[orderedPoints.Count - 1] = originalLine.lineEndPt;  // snap last
                }



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


        public static void Line_Circle_Intersection_and_Splitting(ref linelist_store line_data, ref circlelist_store circle_data)
        {
            // Perform line-circle intersection and splitting
            // Line - Circle Intersection and Splitting

            // Dictionary to store line id and their corresponding intersecting circle ids
            Dictionary<int, List<int>> linecircle_intersections = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> circleline_intersections = new Dictionary<int, List<int>>();


            // Create a list of lines to process
            foreach (line_store line in line_data.lineMap.Values)
            {
                linecircle_intersections[line.line_id] = new List<int>();
            }


            // Create a list of circles to process
            foreach (circle_store circle in circle_data.circleMap.Values)
            {
                circleline_intersections[circle.circle_id] = new List<int>();
            }



            for (int i = 0; i < line_data.lineMap.Count; i++)
            {
                line_store line1 = line_data.lineMap.ElementAt(i).Value;

                for (int j = 0; j < circle_data.circleMap.Count; j++)
                {
                    circle_store circle = circle_data.circleMap.ElementAt(j).Value;

                    if (circle.type == 0)
                    {
                        if (DoLineSegmentIntersectCircle(line1.lineStartPt, line1.lineEndPt,
                            circle.circleCenterPt, (float)circle.circleRadius))
                        {
                            // Store the intersection information
                            linecircle_intersections[line1.line_id].Add(circle.circle_id);
                            circleline_intersections[circle.circle_id].Add(line1.line_id);
                        }

                    }
                    else if (circle.type == 1)
                    {
                        if (DoLineSegmentIntersectCircularArc(line1.lineStartPt, line1.lineEndPt,
                            circle.circleCenterPt, (float)circle.circleRadius,
                            (float)circle.startAngle, (float)circle.endAngle))
                        {
                            // Store the intersection information
                            linecircle_intersections[line1.line_id].Add(circle.circle_id);
                            circleline_intersections[circle.circle_id].Add(line1.line_id);
                        }
                    }

                }
            }


            // Create a new list to hold the split lines
            List<line_store> new_lines = new List<line_store>();


            foreach (var kvp in linecircle_intersections)
            {
                int lineId = kvp.Key;
                List<int> intersectionCircleIds = kvp.Value;
                // Get the original line
                line_store originalLine = line_data.lineMap[lineId];

                List<Vector2> intersectionPoints = new List<Vector2>();

                // Find the intersection points with the intersecting circles
                foreach (int intersectingCircleId in intersectionCircleIds)
                {
                    circle_store intersectingCircle = circle_data.circleMap[intersectingCircleId];

                    if (intersectingCircle.type == 0)
                    {
                        // Circle intersection
                        GetLineCircleIntersectionPoints(originalLine.lineStartPt, originalLine.lineEndPt, intersectingCircle.circleCenterPt, (float)intersectingCircle.circleRadius,
                            out Vector2 p0, out Vector2 p1, out bool hasSecond);
                        intersectionPoints.Add(p0);
                        if (hasSecond)
                            intersectionPoints.Add(p1);
                    }
                    else if (intersectingCircle.type == 1)
                    {
                        // Circular arc intersection
                        GetLineArcIntersectionPoints(originalLine.lineStartPt, originalLine.lineEndPt, intersectingCircle.circleCenterPt, (float)intersectingCircle.circleRadius,
                            (float)intersectingCircle.startAngle, (float)intersectingCircle.endAngle,
                            out Vector2 p0, out Vector2 p1, out bool hasSecond);
                        intersectionPoints.Add(p0);
                        if (hasSecond)
                            intersectionPoints.Add(p1);
                    }
                }

                // Order the intersection points along the original line
                List<Vector2> orderedPoints = new List<Vector2> { originalLine.lineStartPt };

                foreach (var p in intersectionPoints.OrderBy(pt => (pt - originalLine.lineStartPt).LengthSquared))
                {
                    if ((p - orderedPoints[orderedPoints.Count - 1]).LengthSquared > 1e-12)
                        orderedPoints.Add(p);
                }

                if ((originalLine.lineEndPt - orderedPoints[orderedPoints.Count - 1]).LengthSquared > 1e-12)
                {
                    // Final intersection point is not too close to the last point
                    orderedPoints.Add(originalLine.lineEndPt);
                }
                else
                {
                    orderedPoints[orderedPoints.Count - 1] = originalLine.lineEndPt;  // snap last
                }


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


            // Create a new list to hold the split circles
            List<circle_store> new_circles = new List<circle_store>();

            foreach (var kvp in circleline_intersections)
            {
                int circleId = kvp.Key;
                List<int> intersectionLineIds = kvp.Value;
                circle_store originalCircle = circle_data.circleMap[circleId];

                // Collect angles (not points) — the split is in angle space, so
                // deriving angles here and deduping on the angle is the right unit.
                var hitAngles = new List<double>();
                foreach (int intersectingLineId in intersectionLineIds)
                {
                    line_store intersectingLine = line_data.lineMap[intersectingLineId];

                    Vector2 p0, p1;
                    bool hasSecond;
                    if (originalCircle.type == 0)
                    {
                        GetLineCircleIntersectionPoints(
                            intersectingLine.lineStartPt, intersectingLine.lineEndPt,
                            originalCircle.circleCenterPt, (float)originalCircle.circleRadius,
                            out p0, out p1, out hasSecond);
                    }
                    else
                    {
                        GetLineArcIntersectionPoints(
                            intersectingLine.lineStartPt, intersectingLine.lineEndPt,
                            originalCircle.circleCenterPt, (float)originalCircle.circleRadius,
                            (float)originalCircle.startAngle, (float)originalCircle.endAngle,
                            out p0, out p1, out hasSecond);
                    }

                    hitAngles.Add(PointAngle(p0, originalCircle.circleCenterPt));
                    if (hasSecond)
                        hitAngles.Add(PointAngle(p1, originalCircle.circleCenterPt));
                }

                if (originalCircle.type == 0)
                {
                    // Full circle: need >= 2 distinct hit angles to split.
                    // Normalize, sort, dedup.
                    var angles = hitAngles
                        .Select(Normalize)
                        .OrderBy(a => a)
                        .ToList();

                    // Dedup within eps.
                    var distinct = new List<double>();
                    foreach (double a in angles)
                    {
                        if (distinct.Count == 0 || Math.Abs(a - distinct[distinct.Count - 1]) > 1e-9)
                            distinct.Add(a);
                    }

                    if (distinct.Count < 2)
                    {
                        // 0 or 1 crossing: circle stays whole (no topological change).
                        new_circles.Add(originalCircle);
                    }
                    else
                    {
                        // Emit one arc per gap, including the wrap-around gap.
                        for (int i = 0; i < distinct.Count; i++)
                        {
                            double a0 = distinct[i];
                            double a1 = distinct[(i + 1) % distinct.Count];
                            new_circles.Add(new circle_store
                            {
                                circle_id = 0,
                                circleCenterPt = originalCircle.circleCenterPt,
                                circleRadius = originalCircle.circleRadius,
                                type = 1,
                                startAngle = (float)a0,
                                endAngle = (float)a1
                            });
                        }
                    }
                }
                else if (originalCircle.type == 1)
                {
                    // Arc: the arc's own endpoints are already nodes, so the split
                    // angles are {startA, hit..., endAngle}, filtered to the sweep,
                    // deduped, and snapped to the endpoints.
                    double startA = Normalize(originalCircle.startAngle);
                    double endA = Normalize(originalCircle.endAngle);
                    double sweep = CCWSweep(startA, endA);   // in (0, 2π)

                    // Build the ordered list of split angles.
                    // rel = CCW distance from startA, in [0, sweep].
                    var rel = new List<double> { 0.0 };       // startA is rel 0
                    foreach (double a in hitAngles)
                    {
                        double r = CCWSweep(startA, Normalize(a));
                        if (r > 1e-9 && r < sweep - 1e-9)     // strictly inside
                            rel.Add(r);
                    }
                    rel.Add(sweep);                            // endA is rel sweep

                    rel.Sort();

                    // Dedup within eps on the rel axis.
                    var relDistinct = new List<double>();
                    foreach (double r in rel)
                    {
                        if (relDistinct.Count == 0 || Math.Abs(r - relDistinct[relDistinct.Count - 1]) > 1e-9)
                            relDistinct.Add(r);
                    }

                    // Convert back to absolute angles. Snap first/last to the arc's
                    // own endpoints so a T-junction at the endpoint does not create
                    // a ~1e-9 sub-arc.
                    var splitAngles = new List<double>(relDistinct.Count);
                    for (int i = 0; i < relDistinct.Count; i++)
                    {
                        if (i == 0) splitAngles.Add(startA);
                        else if (i == relDistinct.Count - 1) splitAngles.Add(endA);
                        else splitAngles.Add(Normalize(startA + relDistinct[i]));
                    }

                    // If no interior hits, keep the arc unchanged.
                    if (splitAngles.Count <= 2)
                    {
                        new_circles.Add(originalCircle);
                    }
                    else
                    {
                        for (int i = 0; i < splitAngles.Count - 1; i++)
                        {
                            new_circles.Add(new circle_store
                            {
                                circle_id = 0,
                                circleCenterPt = originalCircle.circleCenterPt,
                                circleRadius = originalCircle.circleRadius,
                                type = 1,
                                startAngle = (float)splitAngles[i],
                                endAngle = (float)splitAngles[i + 1]
                            });
                        }
                    }
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

            // Clear the original circles and add the new split circles
            circle_data.clear_circles();

            int CircleId = 0;

            foreach (var newCircle in new_circles)
            {
                circle_data.add_circle(CircleId, newCircle.circleCenterPt, newCircle.circleRadius,
                    newCircle.startAngle, newCircle.endAngle, newCircle.type);
                CircleId++;
            }

        }




        public static void Line_Ellipse_Intersection_and_Splitting(ref linelist_store line_data, ref ellipselist_store ellipse_data)
        {
            // Perform line-ellipse intersection and splitting
            // Line - Ellipse Intersection and Splitting

            // Dictionary to store line id and their corresponding intersecting ellipse ids
            Dictionary<int, List<int>> lineellipse_intersections = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> ellipseline_intersections = new Dictionary<int, List<int>>();


            // Create a list of lines to process
            foreach (line_store line in line_data.lineMap.Values)
            {
                lineellipse_intersections[line.line_id] = new List<int>();
            }


            // Create a list of ellipses to process
            foreach (ellipse_store ellipse in ellipse_data.ellipseMap.Values)
            {
                ellipseline_intersections[ellipse.ellipse_id] = new List<int>();
            }



            for (int i = 0; i < line_data.lineMap.Count; i++)
            {
                line_store line1 = line_data.lineMap.ElementAt(i).Value;

                for (int j = 0; j < ellipse_data.ellipseMap.Count; j++)
                {
                    ellipse_store ellipse = ellipse_data.ellipseMap.ElementAt(j).Value;

                    if (ellipse.type == 0)
                    {
                        if (DoLineSegmentIntersectEllipse(line1.lineStartPt, line1.lineEndPt, ellipse.ellipseCenterPt,
                            (float)ellipse.ellipseMajorAxis, (float)ellipse.ellipseMinorAxis, (float)ellipse.ellipseRotationAngle))
                        {
                            // Store the intersection information
                            lineellipse_intersections[line1.line_id].Add(ellipse.ellipse_id);
                            ellipseline_intersections[ellipse.ellipse_id].Add(line1.line_id);
                        }

                    }
                    else if (ellipse.type == 1)
                    {
                        if (DoLineSegmentIntersectEllipticalArc(line1.lineStartPt, line1.lineEndPt, ellipse.ellipseCenterPt,
                            (float)ellipse.ellipseMajorAxis, (float)ellipse.ellipseMinorAxis, (float)ellipse.ellipseRotationAngle,
                            (float)ellipse.startParameter, (float)ellipse.endParameter))
                        {
                            // Store the intersection information
                            lineellipse_intersections[line1.line_id].Add(ellipse.ellipse_id);
                            ellipseline_intersections[ellipse.ellipse_id].Add(line1.line_id);
                        }
                    }

                }
            }


            // Create a new list to hold the split lines
            List<line_store> new_lines = new List<line_store>();


            foreach (var kvp in lineellipse_intersections)
            {
                int lineId = kvp.Key;
                List<int> intersectionEllipseIds = kvp.Value;
                // Get the original line
                line_store originalLine = line_data.lineMap[lineId];

                List<Vector2> intersectionPoints = new List<Vector2>();

                // Find the intersection points with the intersecting ellipses
                foreach (int intersectingEllipseId in intersectionEllipseIds)
                {
                    ellipse_store intersectingEllipse = ellipse_data.ellipseMap[intersectingEllipseId];

                    if (intersectingEllipse.type == 0)
                    {
                        // Ellipse intersection
                        GetLineEllipseIntersectionPoints(originalLine.lineStartPt, originalLine.lineEndPt, intersectingEllipse.ellipseCenterPt,
                            (float)intersectingEllipse.ellipseMajorAxis, (float)intersectingEllipse.ellipseMinorAxis,
                            (float)intersectingEllipse.ellipseRotationAngle,
                            out Vector2 p0, out Vector2 p1, out bool hasSecond);

                        intersectionPoints.Add(p0);
                        if (hasSecond)
                            intersectionPoints.Add(p1);
                    }
                    else if (intersectingEllipse.type == 1)
                    {
                        // Elliptical arc intersection
                        GetLineEllipticalArcIntersectionPoints(originalLine.lineStartPt, originalLine.lineEndPt, intersectingEllipse.ellipseCenterPt,
                        (float)intersectingEllipse.ellipseMajorAxis, (float)intersectingEllipse.ellipseMinorAxis,
                        (float)intersectingEllipse.ellipseRotationAngle,
                        (float)intersectingEllipse.startParameter, (float)intersectingEllipse.endParameter,
                        out Vector2 p0, out Vector2 p1, out bool hasSecond);

                        intersectionPoints.Add(p0);
                        if (hasSecond)
                            intersectionPoints.Add(p1);
                    }
                }

                // Order the intersection points along the original line
                List<Vector2> orderedPoints = new List<Vector2> { originalLine.lineStartPt };

                foreach (var p in intersectionPoints.OrderBy(pt => (pt - originalLine.lineStartPt).LengthSquared))
                {
                    if ((p - orderedPoints[orderedPoints.Count - 1]).LengthSquared > 1e-12)
                        orderedPoints.Add(p);
                }

                if ((originalLine.lineEndPt - orderedPoints[orderedPoints.Count - 1]).LengthSquared > 1e-12)
                {
                    // Final intersection point is not too close to the last point
                    orderedPoints.Add(originalLine.lineEndPt);
                }
                else
                {
                    orderedPoints[orderedPoints.Count - 1] = originalLine.lineEndPt;  // snap last
                }


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


            // Create a new list to hold the split ellipses
            List<ellipse_store> new_ellipses = new List<ellipse_store>();

            foreach (var kvp in ellipseline_intersections)
            {
                int ellipseId = kvp.Key;
                List<int> intersectionLineIds = kvp.Value;
                ellipse_store originalEllipse = ellipse_data.ellipseMap[ellipseId];

                // Collect angles (not points) — the split is in angle space, so
                // deriving angles here and deduping on the angle is the right unit.
                var hitAngles = new List<double>();
                foreach (int intersectingLineId in intersectionLineIds)
                {
                    line_store intersectingLine = line_data.lineMap[intersectingLineId];

                    Vector2 p0, p1;
                    bool hasSecond;
                    if (originalEllipse.type == 0)
                    {
                        GetLineEllipseIntersectionPoints(intersectingLine.lineStartPt, intersectingLine.lineEndPt,
                            originalEllipse.ellipseCenterPt, (float)originalEllipse.ellipseMajorAxis, (float)originalEllipse.ellipseMinorAxis,
                            (float)originalEllipse.ellipseRotationAngle,
                            out p0, out p1, out hasSecond);
                    }
                    else
                    {
                        GetLineEllipticalArcIntersectionPoints(intersectingLine.lineStartPt, intersectingLine.lineEndPt,
                            originalEllipse.ellipseCenterPt, (float)originalEllipse.ellipseMajorAxis, (float)originalEllipse.ellipseMinorAxis,
                            (float)originalEllipse.ellipseRotationAngle,
                            (float)originalEllipse.startParameter, (float)originalEllipse.endParameter,
                            out p0, out p1, out hasSecond);
                    }

                    hitAngles.Add(EllipsePointAngle(p0, originalEllipse.ellipseCenterPt, originalEllipse.ellipseMajorAxis,
                        originalEllipse.ellipseMinorAxis, originalEllipse.ellipseRotationAngle));

                    if (hasSecond)
                        hitAngles.Add(EllipsePointAngle(p1, originalEllipse.ellipseCenterPt, originalEllipse.ellipseMajorAxis,
                            originalEllipse.ellipseMinorAxis, originalEllipse.ellipseRotationAngle));
                }

                if (originalEllipse.type == 0)
                {
                    // Full circle: need >= 2 distinct hit angles to split.
                    // Normalize, sort, dedup.
                    var angles = hitAngles
                        .Select(Normalize)
                        .OrderBy(a => a)
                        .ToList();

                    // Dedup within eps.
                    var distinct = new List<double>();
                    foreach (double a in angles)
                    {
                        if (distinct.Count == 0 || Math.Abs(a - distinct[distinct.Count - 1]) > 1e-9)
                            distinct.Add(a);
                    }

                    if (distinct.Count < 2)
                    {
                        // 0 or 1 crossing: ellipse stays whole (no topological change).
                        new_ellipses.Add(originalEllipse);
                    }
                    else
                    {
                        // Emit one arc per gap, including the wrap-around gap.
                        for (int i = 0; i < distinct.Count; i++)
                        {
                            double a0 = distinct[i];
                            double a1 = distinct[(i + 1) % distinct.Count];
                            new_ellipses.Add(new ellipse_store
                            {
                                ellipse_id = 0,
                                ellipseCenterPt = originalEllipse.ellipseCenterPt,
                                ellipseMajorAxis = originalEllipse.ellipseMajorAxis,
                                ellipseMinorAxis = originalEllipse.ellipseMinorAxis,
                                ellipseRotationAngle = originalEllipse.ellipseRotationAngle,
                                type = 1,
                                startParameter = (float)a0,
                                endParameter = (float)a1
                            });
                        }
                    }
                }
                else if (originalEllipse.type == 1)
                {
                    // Arc: the arc's own endpoints are already nodes, so the split
                    // angles are {startA, hit..., endAngle}, filtered to the sweep,
                    // deduped, and snapped to the endpoints.
                    double startA = Normalize(originalEllipse.startParameter);
                    double endA = Normalize(originalEllipse.endParameter);
                    double sweep = CCWSweep(startA, endA);   // in (0, 2π)

                    // Build the ordered list of split angles.
                    // rel = CCW distance from startA, in [0, sweep].
                    var rel = new List<double> { 0.0 };       // startA is rel 0
                    foreach (double a in hitAngles)
                    {
                        double r = CCWSweep(startA, Normalize(a));
                        if (r > 1e-9 && r < sweep - 1e-9)     // strictly inside
                            rel.Add(r);
                    }
                    rel.Add(sweep);                            // endA is rel sweep

                    rel.Sort();

                    // Dedup within eps on the rel axis.
                    var relDistinct = new List<double>();
                    foreach (double r in rel)
                    {
                        if (relDistinct.Count == 0 || Math.Abs(r - relDistinct[relDistinct.Count - 1]) > 1e-9)
                            relDistinct.Add(r);
                    }

                    // Convert back to absolute angles. Snap first/last to the arc's
                    // own endpoints so a T-junction at the endpoint does not create
                    // a ~1e-9 sub-arc.
                    var splitAngles = new List<double>(relDistinct.Count);
                    for (int i = 0; i < relDistinct.Count; i++)
                    {
                        if (i == 0) splitAngles.Add(startA);
                        else if (i == relDistinct.Count - 1) splitAngles.Add(endA);
                        else splitAngles.Add(Normalize(startA + relDistinct[i]));
                    }

                    // If no interior hits, keep the arc unchanged.
                    if (splitAngles.Count <= 2)
                    {
                        new_ellipses.Add(originalEllipse);
                    }
                    else
                    {
                        for (int i = 0; i < splitAngles.Count - 1; i++)
                        {
                            new_ellipses.Add(new ellipse_store
                            {
                                ellipse_id = 0,
                                ellipseCenterPt = originalEllipse.ellipseCenterPt,
                                ellipseMajorAxis = originalEllipse.ellipseMajorAxis,
                                ellipseMinorAxis = originalEllipse.ellipseMinorAxis,
                                ellipseRotationAngle = originalEllipse.ellipseRotationAngle,
                                type = 1,
                                startParameter = (float)splitAngles[i],
                                endParameter = (float)splitAngles[i + 1]
                            });
                        }
                    }
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

            // Clear the original ellipses and add the new split ellipses
            ellipse_data.clear_ellipses();

            int EllipseId = 0;

            foreach (var newEllipse in new_ellipses)
            {
                ellipse_data.add_ellipse(EllipseId, newEllipse.ellipseCenterPt, newEllipse.ellipseMajorAxis, newEllipse.ellipseMinorAxis,
                    newEllipse.ellipseRotationAngle, newEllipse.type, newEllipse.startParameter, newEllipse.endParameter);

                EllipseId++;
            }

        }



        public static void Line_Spline_Intersection_and_Splitting(ref linelist_store line_data, ref splinelist_store spline_data)
        {
            // Perform line-spline intersection and splitting
            // Line - Spline Intersection and Splitting
            // Dictionary to store line id, spline id and their corresponding intersecting param t values
            Dictionary<int, List<(double tLine, double tSpline, Vector2 point)>> linespline_intersections = 
                new Dictionary<int, List<(double tLine, double tSpline, Vector2 point)>>();

            Dictionary<int, List<(double tLine, double tSpline, Vector2 point)>> splineline_intersections = 
                new Dictionary<int, List<(double tLine, double tSpline, Vector2 point)>>();


            // Create a list of lines to process
            foreach (line_store line in line_data.lineMap.Values)
            {
                linespline_intersections[line.line_id] = new List<(double tLine, double tSpline, Vector2 point)>();
            }
            // Create a list of splines to process
            foreach (spline_store spline in spline_data.splineMap.Values)
            {
                splineline_intersections[spline.spline_id] = new List<(double tLine, double tSpline, Vector2 point)>();
            }



            // Get all the intersection parameter t for line and splines
            for(int i = 0; i < line_data.lineMap.Count; i++)
            {
                line_store line = line_data.lineMap.ElementAt(i).Value;

                for (int j = 0; j < spline_data.splineMap.Count; j++)
                {
                    spline_store spline = spline_data.splineMap.ElementAt(j).Value;

                    List<double> line_intersection_param_t = new List<double>();
                    List<double> spline_intersection_param_t = new List<double>();
                    List<Vector2> intersection_points = new List<Vector2>();

                    // int degree, bool isPeriodic, bool isRational,
                    // List<double> weights, List< double > knots,
                    // List<Vector2> controlPoints

                    GetLineSplineIntersectionParameters(line.lineStartPt, line.lineEndPt,
                                spline.degree, spline.isPeriodic,
                            spline.isRational, spline.weights, spline.knots, spline.controlPoints,
                                out line_intersection_param_t, out spline_intersection_param_t, out intersection_points);

                    for (int k = 0; k < line_intersection_param_t.Count; k++)
                    {
                        var hit = (line_intersection_param_t[k], spline_intersection_param_t[k], intersection_points[k]);
                        linespline_intersections[line.line_id].Add(hit);
                        splineline_intersections[spline.spline_id].Add(hit);
                    }

                }
            }


            // Create a new list to hold the split lines
            List<line_store> new_lines = new List<line_store>();

            foreach (var kvp in linespline_intersections)
            {
                int lineId = kvp.Key;
                List<double> intersectionparam_t = kvp.Value;

                // Get the original line
                line_store originalLine = line_data.lineMap[lineId];

                List<Vector2> intersectionPoints = linespline_intersection_points[lineId];

                Vector2 startPt = originalLine.lineStartPt;

                foreach (Vector2 pt in intersectionPoints)
                {
                    // Create a new line segment from startPt to pt
                    line_store newLine = new line_store
                    {
                        line_id = 0,
                        lineStartPt = startPt,
                        lineEndPt = pt
                    };
                    new_lines.Add(newLine);

                    // Update startPt for the next segment
                    startPt = pt;
                }

                // Add the last line segment from the last intersection point to the original line's end point
                line_store lastLine = new line_store
                {
                    line_id = 0,
                    lineStartPt = startPt,
                    lineEndPt = originalLine.lineEndPt
                };
                new_lines.Add(lastLine);
            }

            // Create a new list to hold the split splines
            List<spline_store> new_splines = new List<spline_store>();

            foreach(var kvp in splineline_intersections)
            {
                int splineId = kvp.Key;
                List<double> intersectionparam_t = kvp.Value;

                // Get the original spline
                spline_store originalSpline = spline_data.splineMap[splineId];

                //double uMin = originalSpline.knots[originalSpline.degree];
                //double uMax = originalSpline.knots[originalSpline.controlPoints.Count];

                List<Vector2> intersectionPoints = splineline_intersection_points[splineId];

                Vector2 startPt = originalSpline.GetSplinePoint(0);

                int k = 0;
                foreach (double param_t in intersectionparam_t)
                {
                    Vector2 pt = intersectionPoints[k];

                    // Using the param_t, split the spline into two segments



                    // Update startPt for the next segment
                    startPt = pt;
                    k++;

                }


            }





        }




        public static void Circle_Circle_Intersection_and_Splitting(ref circlelist_store circle_data)
        {
            // Perform circle-circle intersection and splitting
            // Circle - Circle Intersection and Splitting

            // Dictionary to store circle id and their corresponding
            Dictionary<int, List<int>> circle_intersections = new Dictionary<int, List<int>>();

            // Create a list of circles to process
            foreach (var circle in circle_data.circleMap.Values)
            {
                circle_intersections[circle.circle_id] = new List<int>();
            }


            for (int i = 0; i < circle_data.circleMap.Count; i++)
            {
                circle_store circle1 = circle_data.circleMap.ElementAt(i).Value;

                for (int j = i + 1; j < circle_data.circleMap.Count; j++)
                {
                    circle_store circle2 = circle_data.circleMap.ElementAt(j).Value;

                    // Gaurd to prevent circle self intersection
                    if (circle1.circle_id != circle2.circle_id)
                    {
                        if (DoArcsIntersect(circle1.circleCenterPt, (float)circle1.circleRadius, (float)circle1.startAngle, (float)circle1.endAngle,
                                             circle2.circleCenterPt, (float)circle2.circleRadius, (float)circle2.startAngle, (float)circle2.endAngle))
                        {
                            // Store the intersection information
                            circle_intersections[circle1.circle_id].Add(circle2.circle_id);
                            circle_intersections[circle2.circle_id].Add(circle1.circle_id);
                        }
                    }
                }
            }


            // Create a new list to hold the split circles
            List<circle_store> new_circles = new List<circle_store>();


            foreach (var kvp in circle_intersections)
            {
                circle_store originalCircle = circle_data.circleMap[kvp.Key];
                List<int> intersectingIds = kvp.Value;

                // Collect hit angles (not points).
                var hitAngles = new List<double>();
                foreach (int id in intersectingIds)
                {
                    var other = circle_data.circleMap[id];
                    GetArcArcIntersectionPoints(
                        originalCircle.circleCenterPt, (float)originalCircle.circleRadius,
                        (float)originalCircle.startAngle, (float)originalCircle.endAngle,
                        other.circleCenterPt, (float)other.circleRadius,
                        (float)other.startAngle, (float)other.endAngle,
                        out var p0, out var p1, out bool hasSecond);
                    if (p0 != default) hitAngles.Add(PointAngle(p0, originalCircle.circleCenterPt));
                    if (hasSecond) hitAngles.Add(PointAngle(p1, originalCircle.circleCenterPt));
                }

                if (originalCircle.type == 0)
                {
                    var distinct = DedupSorted(hitAngles.Select(Normalize).OrderBy(a => a).ToList(), 1e-9);
                    if (distinct.Count < 2) { new_circles.Add(originalCircle); continue; }
                    for (int i = 0; i < distinct.Count; i++)
                    {
                        double a0 = distinct[i], a1 = distinct[(i + 1) % distinct.Count];
                        new_circles.Add(MakeArc(originalCircle, a0, a1));
                    }
                }
                else
                {
                    double startA = Normalize(originalCircle.startAngle);
                    double endA = Normalize(originalCircle.endAngle);
                    double sweep = CCWSweep(startA, endA);
                    var rel = new List<double> { 0.0 };
                    foreach (double a in hitAngles)
                    {
                        double r = CCWSweep(startA, Normalize(a));
                        if (r > 1e-9 && r < sweep - 1e-9) rel.Add(r);
                    }
                    rel.Add(sweep);
                    rel.Sort();
                    var relDistinct = DedupSorted(rel, 1e-9);
                    if (relDistinct.Count <= 2) { new_circles.Add(originalCircle); continue; }
                    var splitAngles = new List<double>();
                    for (int i = 0; i < relDistinct.Count; i++)
                    {
                        if (i == 0) splitAngles.Add(startA);
                        else if (i == relDistinct.Count - 1) splitAngles.Add(endA);
                        else splitAngles.Add(Normalize(startA + relDistinct[i]));
                    }
                    for (int i = 0; i < splitAngles.Count - 1; i++)
                        new_circles.Add(MakeArc(originalCircle, splitAngles[i], splitAngles[i + 1]));
                }
            }



            // Clear the original circles and add the new split circles
            circle_data.clear_circles();

            int CircleId = 0;

            foreach (var newCircle in new_circles)
            {
                circle_data.add_circle(CircleId, newCircle.circleCenterPt, newCircle.circleRadius,
                    newCircle.startAngle, newCircle.endAngle, newCircle.type);
                CircleId++;
            }

        }



        public static void Circle_Ellipse_Intersection_and_Splitting(ref circlelist_store circle_data, ref ellipselist_store ellipse_data)
        {

            // Perform circle-ellipse intersection and splitting
            // Circle - Ellipse Intersection and Splitting

            // Dictionary to store circle id and their corresponding
            Dictionary<int, List<int>> circleellipse_intersections = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> ellipsecircle_intersections = new Dictionary<int, List<int>>();

            // Create a list of circles to process
            foreach (var circle in circle_data.circleMap.Values)
            {
                circleellipse_intersections[circle.circle_id] = new List<int>();
            }

            // Create a list of ellipses to process
            foreach (var ellipse in ellipse_data.ellipseMap.Values)
            {
                ellipsecircle_intersections[ellipse.ellipse_id] = new List<int>();
            }



            for (int i = 0; i < circle_data.circleMap.Count; i++)
            {
                circle_store circle1 = circle_data.circleMap.ElementAt(i).Value;

                for (int j = 0; j < ellipse_data.ellipseMap.Count; j++)
                {
                    ellipse_store ellipse = ellipse_data.ellipseMap.ElementAt(j).Value;

                    if (DoCircleEllipseIntersect(circle1.circleCenterPt, (float)circle1.circleRadius,
                        (float)circle1.startAngle, (float)circle1.endAngle,
                        ellipse.ellipseCenterPt, (float)ellipse.ellipseMajorAxis,
                        (float)ellipse.ellipseMinorAxis, (float)ellipse.ellipseRotationAngle,
                        (float)ellipse.startParameter, (float)ellipse.endParameter))
                    {
                        // Store the intersection information
                        circleellipse_intersections[circle1.circle_id].Add(ellipse.ellipse_id);
                        ellipsecircle_intersections[ellipse.ellipse_id].Add(circle1.circle_id);
                    }
                }
            }


            // --- Split circles ---
            var new_circles = new List<circle_store>();
            foreach (var kvp in circleellipse_intersections)
            {
                circle_store originalCircle = circle_data.circleMap[kvp.Key];

                var hitAngles = new List<double>();
                foreach (int ellipseId in kvp.Value)
                {
                    ellipse_store ellipse = ellipse_data.ellipseMap[ellipseId];
                    GetCircleEllipseIntersectionPoints(
                        originalCircle.circleCenterPt, (float)originalCircle.circleRadius,
                        (float)originalCircle.startAngle, (float)originalCircle.endAngle,
                        ellipse.ellipseCenterPt,
                        (float)ellipse.ellipseMajorAxis, (float)ellipse.ellipseMinorAxis,
                        (float)ellipse.ellipseRotationAngle,
                        (float)ellipse.startParameter, (float)ellipse.endParameter,
                        out var pts);
                    foreach (var p in pts)
                        hitAngles.Add(PointAngle(p, originalCircle.circleCenterPt));
                }
                SplitCircleAtAngles(originalCircle, hitAngles, new_circles);
            }



            // --- Split ellipses ---
            var new_ellipses = new List<ellipse_store>();
            foreach (var kvp in ellipsecircle_intersections)
            {
                ellipse_store originalEllipse = ellipse_data.ellipseMap[kvp.Key];

                var hitParams = new List<double>();   // eccentric anomalies
                foreach (int circleId in kvp.Value)
                {
                    circle_store circle = circle_data.circleMap[circleId];
                    GetCircleEllipseIntersectionPoints(
                        circle.circleCenterPt, (float)circle.circleRadius,
                        (float)circle.startAngle, (float)circle.endAngle,
                        originalEllipse.ellipseCenterPt,
                        (float)originalEllipse.ellipseMajorAxis, (float)originalEllipse.ellipseMinorAxis,
                        (float)originalEllipse.ellipseRotationAngle,
                        (float)originalEllipse.startParameter, (float)originalEllipse.endParameter,
                        out var pts);
                    foreach (var p in pts)
                        hitParams.Add(EllipsePointAngle(
                            p, originalEllipse.ellipseCenterPt,
                            originalEllipse.ellipseMajorAxis, originalEllipse.ellipseMinorAxis,
                            originalEllipse.ellipseRotationAngle));
                }
                SplitEllipseAtParameters(originalEllipse, hitParams, new_ellipses);
            }


            // Clear the original circles and add the new split circles
            circle_data.clear_circles();

            int CircleId = 0;

            foreach (var newCircle in new_circles)
            {
                circle_data.add_circle(CircleId, newCircle.circleCenterPt, newCircle.circleRadius,
                    newCircle.startAngle, newCircle.endAngle, newCircle.type);
                CircleId++;
            }



            // Clear the original ellipses and add the new split ellipses
            ellipse_data.clear_ellipses();

            int EllipseId = 0;

            foreach (var newEllipse in new_ellipses)
            {
                ellipse_data.add_ellipse(EllipseId, newEllipse.ellipseCenterPt, newEllipse.ellipseMajorAxis, newEllipse.ellipseMinorAxis,
                    newEllipse.ellipseRotationAngle, newEllipse.type, newEllipse.startParameter, newEllipse.endParameter);

                EllipseId++;
            }

        }


        public static void Ellipse_Ellipse_Intersection_and_Splitting(ref ellipselist_store ellipse_data)
        {
            // Perform ellipse-ellipse intersection and splitting
            // Ellipse - Ellipse Intersection and Splitting
            // Dictionary to store ellipse id and their corresponding
            Dictionary<int, List<int>> ellipse_intersections = new Dictionary<int, List<int>>();
            // Create a list of ellipses to process
            foreach (var ellipse in ellipse_data.ellipseMap.Values)
            {
                ellipse_intersections[ellipse.ellipse_id] = new List<int>();
            }


            for (int i = 0; i < ellipse_data.ellipseMap.Count; i++)
            {
                ellipse_store e1 = ellipse_data.ellipseMap.ElementAt(i).Value;
                for (int j = i + 1; j < ellipse_data.ellipseMap.Count; j++)
                {
                    ellipse_store e2 = ellipse_data.ellipseMap.ElementAt(j).Value;

                    if (DoEllipsesIntersect(
                            e1.ellipseCenterPt,
                            (float)e1.ellipseMajorAxis, (float)e1.ellipseMinorAxis,
                            (float)e1.ellipseRotationAngle,
                            (float)e1.startParameter, (float)e1.endParameter,
                            e2.ellipseCenterPt,
                            (float)e2.ellipseMajorAxis, (float)e2.ellipseMinorAxis,
                            (float)e2.ellipseRotationAngle,
                            (float)e2.startParameter, (float)e2.endParameter))
                    {
                        ellipse_intersections[e1.ellipse_id].Add(e2.ellipse_id);
                        ellipse_intersections[e2.ellipse_id].Add(e1.ellipse_id);
                    }
                }
            }

            // --- Split phase ---
            var new_ellipses = new List<ellipse_store>();

            foreach (var kvp in ellipse_intersections)
            {
                ellipse_store original = ellipse_data.ellipseMap[kvp.Key];

                // Eccentric anomalies (on `original`) of every hit with every partner.
                var hitParams = new List<double>();

                foreach (int otherId in kvp.Value)
                {
                    ellipse_store other = ellipse_data.ellipseMap[otherId];

                    GetEllipseEllipseIntersectionPoints(
                        original.ellipseCenterPt,
                        (float)original.ellipseMajorAxis, (float)original.ellipseMinorAxis,
                        (float)original.ellipseRotationAngle,
                        (float)original.startParameter, (float)original.endParameter,
                        other.ellipseCenterPt,
                        (float)other.ellipseMajorAxis, (float)other.ellipseMinorAxis,
                        (float)other.ellipseRotationAngle,
                        (float)other.startParameter, (float)other.endParameter,
                        out var pts);

                    foreach (var p in pts)
                    {
                        // Convert each hit point to eccentric anomaly on `original`.
                        hitParams.Add(EllipsePointAngle(
                            p,
                            original.ellipseCenterPt,
                            original.ellipseMajorAxis,
                            original.ellipseMinorAxis,
                            original.ellipseRotationAngle));
                    }
                }

                SplitEllipseAtParameters(original, hitParams, new_ellipses);
            }


            // Clear the original ellipses and add the new split ellipses
            ellipse_data.clear_ellipses();

            int EllipseId = 0;

            foreach (var newEllipse in new_ellipses)
            {
                ellipse_data.add_ellipse(EllipseId, newEllipse.ellipseCenterPt, newEllipse.ellipseMajorAxis, newEllipse.ellipseMinorAxis,
                    newEllipse.ellipseRotationAngle, newEllipse.type, newEllipse.startParameter, newEllipse.endParameter);

                EllipseId++;
            }

        }


        #region "Line - Line Helper Functions"

        /// <summary>
        /// Returns true iff segments (p1,p2) and (p3,p4) cross transversally
        /// at a point strictly inside both segments' spans (within eps slack).
        /// Collinear overlaps return false — use TryGetCollinearOverlap for those.
        /// </summary>
        public static bool DoLineSegmentsIntersect(
            Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, double eps = 1e-9)
        {
            double d1x = p2.X - p1.X, d1y = p2.Y - p1.Y;
            double d2x = p4.X - p3.X, d2y = p4.Y - p3.Y;

            double len1sq = d1x * d1x + d1y * d1y;
            double len2sq = d2x * d2x + d2y * d2y;
            if (len1sq < eps * eps || len2sq < eps * eps)
                return false;                       // degenerate segment

            double cross = d1x * d2y - d1y * d2x;   // d1 x d2
            double scale = Math.Sqrt(len1sq * len2sq);
            if (Math.Abs(cross) < eps * Math.Max(scale, 1.0))
                return false;                       // parallel (incl. collinear)

            double rx = p3.X - p1.X, ry = p3.Y - p1.Y;
            double t = (rx * d2y - ry * d2x) / cross;
            double s = (rx * d1y - ry * d1x) / cross;

            return t >= -eps && t <= 1.0 + eps
                && s >= -eps && s <= 1.0 + eps;
        }

        /// <summary>
        /// Computes the transversal intersection point of two segments.
        /// Assumes DoLinesIntersect returned true for the same inputs
        /// (same eps). Returns (0,0) if the inputs are not a proper
        /// intersection — callers MUST gate on DoLinesIntersect.
        /// </summary>
        public static Vector2 GetLineSegmentIntersectionPoint(
            Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, double eps = 1e-9)
        {
            double d1x = p2.X - p1.X, d1y = p2.Y - p1.Y;
            double d2x = p4.X - p3.X, d2y = p4.Y - p3.Y;

            double cross = d1x * d2y - d1y * d2x;
            double scale = Math.Sqrt((d1x * d1x + d1y * d1y) * (d2x * d2x + d2y * d2y));
            if (Math.Abs(cross) < eps * Math.Max(scale, 1.0))
                return new Vector2(0, 0);           // parallel

            double rx = p3.X - p1.X, ry = p3.Y - p1.Y;
            double t = (rx * d2y - ry * d2x) / cross;

            // Snap to the endpoints when the intersection is within eps of a vertex.
            // This prevents 1e-12-length stubs that become degree-1 nodes and get
            // pruned, disconnecting the graph.
            if (t < eps) t = 0.0;
            else if (t > 1 - eps) t = 1.0;

            return new Vector2(
                (float)(p1.X + t * d1x),
                (float)(p1.Y + t * d1y));
        }



        #endregion


        #region "Line - Circle Helper Functions"

        // Solves |start + t*d - center|^2 = r^2 for t in [0,1].
        // Returns the roots (0, 1, or 2) in ascending order.
        // Collapses a tangent to a single root. Returns 0 for a miss or degenerate segment.
        private static int SolveLineCircle(Vector2 start, Vector2 end, Vector2 center, float radius,
            out double t0, out double t1, double eps = 1e-9)
        {
            t0 = t1 = 0.0;

            double dx = end.X - start.X, dy = end.Y - start.Y;
            double fx = start.X - center.X, fy = start.Y - center.Y;

            double a = dx * dx + dy * dy;
            if (a < eps * eps) return 0;                       // zero-length segment

            double b = 2.0 * (fx * dx + fy * dy);
            double c = fx * fx + fy * fy - (double)radius * radius;

            double disc = b * b - 4.0 * a * c;
            if (disc < -eps) return 0;                       // infinite line misses circle
            if (disc < 0) disc = 0;                          // snap near-tangent

            double sqrtDisc = Math.Sqrt(disc);
            double inv2a = 1.0 / (2.0 * a);
            double r0 = (-b - sqrtDisc) * inv2a;
            double r1 = (-b + sqrtDisc) * inv2a;
            if (r0 > r1) { double tmp = r0; r0 = r1; r1 = tmp; }

            // Keep roots that lie on the segment (with eps slack), snap to endpoints.
            int n = 0;
            double[] roots = { r0, r1 };
            for (int i = 0; i < 2; i++)
            {
                double t = roots[i];
                if (t < -eps || t > 1.0 + eps) continue;
                if (t < eps) t = 0.0;
                else if (t > 1.0 - eps) t = 1.0;

                if (n == 0) t0 = t;
                else if (Math.Abs(t - t0) > eps) t1 = t;     // collapse double root
                if (n > 0 && Math.Abs(t - t0) <= eps) continue;
                n++;
            }
            return n;
        }


        // Returns true iff the segment crosses the circle's boundary at least once
        // within its span. A segment lying entirely inside or entirely outside the
        // disk returns false (nothing to split at).
        public static bool DoLineSegmentIntersectCircle(Vector2 start, Vector2 end, Vector2 center, float radius)
        {
            int n = SolveLineCircle(start, end, center, radius, out _, out _);
            return n > 0;
        }


        // Returns the boundary crossing points. Valid only if
        // DoLineSegmentIntersectCircle returned true for the same inputs.
        // On a tangent, returns one point (the second is the same, deduped by the caller).
        public static void GetLineCircleIntersectionPoints(Vector2 start, Vector2 end, Vector2 center, float radius,
            out Vector2 p0, out Vector2 p1, out bool hasSecond)
        {
            p0 = p1 = default;
            hasSecond = false;

            int n = SolveLineCircle(start, end, center, radius, out double t0, out double t1);
            if (n == 0) return;

            Vector2 d = end - start;
            p0 = new Vector2((float)(start.X + t0 * d.X), (float)(start.Y + t0 * d.Y));
            if (n == 2)
            {
                p1 = new Vector2((float)(start.X + t1 * d.X), (float)(start.Y + t1 * d.Y));
                hasSecond = true;
            }
        }



        public static bool DoLineSegmentIntersectCircularArc(Vector2 start, Vector2 end, Vector2 center, float radius,
    float startAngle, float endAngle)
        {
            int n = SolveLineCircle(start, end, center, radius, out double t0, out double t1);
            if (n == 0) return false;

            Vector2 d = end - start;

            if (AngleInArc(PointAngle(start, d, t0, center), startAngle, endAngle)) return true;
            if (n == 2 && AngleInArc(PointAngle(start, d, t1, center), startAngle, endAngle)) return true;
            return false;
        }



        public static void GetLineArcIntersectionPoints(Vector2 start, Vector2 end, Vector2 center, float radius,
            float startAngle, float endAngle,
            out Vector2 p0, out Vector2 p1, out bool hasSecond)
        {
            p0 = p1 = default; hasSecond = false;

            int n = SolveLineCircle(start, end, center, radius, out double t0, out double t1);
            if (n == 0) return;

            Vector2 d = end - start;

            bool got0 = AngleInArc(PointAngle(start, d, t0, center), startAngle, endAngle);
            bool got1 = n == 2 && AngleInArc(PointAngle(start, d, t1, center), startAngle, endAngle);

            if (got0)
                p0 = new Vector2((float)(start.X + t0 * d.X), (float)(start.Y + t0 * d.Y));
            if (got1)
            {
                var q = new Vector2((float)(start.X + t1 * d.X), (float)(start.Y + t1 * d.Y));
                if (got0) { p1 = q; hasSecond = true; }
                else { p0 = q; }
            }
        }



        private static double PointAngle(Vector2 start, Vector2 d, double t, Vector2 center)
        {
            double px = start.X + t * d.X;
            double py = start.Y + t * d.Y;
            double ang = Math.Atan2(py - center.Y, px - center.X);
            return ang < 0 ? ang + 2.0 * Math.PI : ang;
        }



        // Arc convention: CCW sweep from startAngle to endAngle, radians, [0, 2π).
        private static bool AngleInArc(double angle, double startAngle, double endAngle, double eps = 1e-9)
        {
            double twoPi = 2.0 * Math.PI;
            angle = ((angle % twoPi) + twoPi) % twoPi;
            startAngle = ((startAngle % twoPi) + twoPi) % twoPi;
            endAngle = ((endAngle % twoPi) + twoPi) % twoPi;

            double sweep = endAngle - startAngle;
            if (sweep < 0) sweep += twoPi;
            if (sweep < eps) return false;              // zero-sweep arc (use circle routine)

            double rel = angle - startAngle;
            if (rel < 0) rel += twoPi;
            return rel <= sweep + eps;
        }


        private static double Normalize(double angle)
        {
            double twoPi = 2.0 * Math.PI;
            return ((angle % twoPi) + twoPi) % twoPi;
        }

        // CCW sweep from `from` to `to`, in (0, 2π].
        // Returns 0.0 if from == to (mod 2π), which the caller treats as degenerate.
        private static double CCWSweep(double from, double to)
        {
            double twoPi = 2.0 * Math.PI;
            double d = Normalize(to) - Normalize(from);
            if (d < 0) d += twoPi;
            return d;
        }

        private static double PointAngle(Vector2 p, Vector2 center)
        {
            return Normalize(Math.Atan2(p.Y - center.Y, p.X - center.X));
        }

        #endregion


        #region "Line - Ellipse Helper Functions"

        // Solves for t in [0,1] where segment (start + t*d) crosses the ellipse
        // centered at `center`, rotated by `ellipseRotationAngle` (radians, CCW from +X),
        // with semi-axes MajorAxis (a) and MinorAxis (b).
        // Returns 0, 1, or 2 roots in ascending order. Tangent collapses to one root.
        private static int SolveLineEllipse(
            Vector2 start, Vector2 end, Vector2 center,
            float MajorAxis, float MinorAxis, float ellipseRotationAngle,
            out double t0, out double t1, double eps = 1e-9)
        {
            t0 = t1 = 0.0;

            double a = MajorAxis;                 // semi-major
            double b = MinorAxis;                 // semi-minor
            if (a < eps || b < eps) return 0;     // degenerate ellipse

            double cosT = Math.Cos(ellipseRotationAngle);
            double sinT = Math.Sin(ellipseRotationAngle);

            // Local frame axes: u = major-axis direction, v = minor-axis direction
            double ux = cosT, uy = sinT;
            double vx = -sinT, vy = cosT;

            // Segment direction and start offset from center, in world
            double dx = end.X - start.X, dy = end.Y - start.Y;
            double fx = start.X - center.X, fy = start.Y - center.Y;

            // Project into the ellipse's local frame
            double du = dx * ux + dy * uy;
            double dv = dx * vx + dy * vy;
            double fu = fx * ux + fy * uy;
            double fv = fx * vx + fy * vy;

            double a2 = a * a, b2 = b * b;

            // Quadratic A t^2 + B t + C = 0 from
            // (fu + t du)^2 / a^2 + (fv + t dv)^2 / b^2 = 1
            double A = b2 * du * du + a2 * dv * dv;
            if (A < eps * eps) return 0;          // zero-length segment

            double B = 2.0 * (b2 * fu * du + a2 * fv * dv);
            double C = b2 * fu * fu + a2 * fv * fv - a2 * b2;

            double disc = B * B - 4.0 * A * C;
            if (disc < -eps) return 0;            // infinite line misses the ellipse
            if (disc < 0) disc = 0;               // snap near-tangent

            double sqrtDisc = Math.Sqrt(disc);
            double inv2A = 1.0 / (2.0 * A);
            double r0 = (-B - sqrtDisc) * inv2A;
            double r1 = (-B + sqrtDisc) * inv2A;
            if (r0 > r1) { double tmp = r0; r0 = r1; r1 = tmp; }

            int n = 0;
            double[] roots = { r0, r1 };
            for (int i = 0; i < 2; i++)
            {
                double t = roots[i];
                if (t < -eps || t > 1.0 + eps) continue;
                if (t < eps) t = 0.0;
                else if (t > 1.0 - eps) t = 1.0;

                if (n == 0) t0 = t;
                else if (Math.Abs(t - t0) > eps) t1 = t;
                if (n > 0 && Math.Abs(t - t0) <= eps) continue;
                n++;
            }
            return n;
        }


        public static bool DoLineSegmentIntersectEllipse(Vector2 start, Vector2 end, Vector2 center,
       float MajorAxis, float MinorAxis, float ellipseRotationAngle)
        {
            int n = SolveLineEllipse(start, end, center, MajorAxis, MinorAxis,
                                     ellipseRotationAngle, out _, out _);
            return n > 0;
        }


        public static void GetLineEllipseIntersectionPoints(Vector2 start, Vector2 end, Vector2 center,
    float MajorAxis, float MinorAxis, float ellipseRotationAngle,
    out Vector2 p0, out Vector2 p1, out bool hasSecond)
        {
            p0 = p1 = default;
            hasSecond = false;

            int n = SolveLineEllipse(start, end, center, MajorAxis, MinorAxis,
                                     ellipseRotationAngle, out double t0, out double t1);
            if (n == 0) return;

            p0 = LinePoint(start, end, t0);
            if (n == 2)
            {
                p1 = LinePoint(start, end, t1);
                hasSecond = true;
            }
        }


        public static bool DoLineSegmentIntersectEllipticalArc(
            Vector2 start, Vector2 end, Vector2 center,
            float MajorAxis, float MinorAxis, float ellipseRotationAngle,
            float startAngle, float endAngle)
        {
            int n = SolveLineEllipse(start, end, center, MajorAxis, MinorAxis,
                                     ellipseRotationAngle, out double t0, out double t1);
            if (n == 0) return false;

            Vector2 p0 = LinePoint(start, end, t0);
            if (EllipseAngleInArc(
                    EllipsePointAngle(p0, center, MajorAxis, MinorAxis, ellipseRotationAngle),
                    startAngle, endAngle))
                return true;

            if (n == 2)
            {
                Vector2 p1 = LinePoint(start, end, t1);
                if (EllipseAngleInArc(
                        EllipsePointAngle(p1, center, MajorAxis, MinorAxis, ellipseRotationAngle),
                        startAngle, endAngle))
                    return true;
            }
            return false;
        }



        public static void GetLineEllipticalArcIntersectionPoints(
            Vector2 start, Vector2 end, Vector2 center,
            float MajorAxis, float MinorAxis, float ellipseRotationAngle,
            float startAngle, float endAngle,
            out Vector2 p0, out Vector2 p1, out bool hasSecond)
        {
            p0 = p1 = default;
            hasSecond = false;

            int n = SolveLineEllipse(start, end, center, MajorAxis, MinorAxis,
                                     ellipseRotationAngle, out double t0, out double t1);
            if (n == 0) return;

            Vector2 d = end - start;

            // Evaluate the world points first, then test each in eccentric-anomaly space.
            Vector2 q0 = new Vector2((float)(start.X + t0 * d.X), (float)(start.Y + t0 * d.Y));
            bool got0 = AngleInArc(
                EllipsePointAngle(q0, center, MajorAxis, MinorAxis, ellipseRotationAngle),
                startAngle, endAngle);

            Vector2 q1 = default;
            bool got1 = false;
            if (n == 2)
            {
                q1 = new Vector2((float)(start.X + t1 * d.X), (float)(start.Y + t1 * d.Y));
                got1 = AngleInArc(
                    EllipsePointAngle(q1, center, MajorAxis, MinorAxis, ellipseRotationAngle),
                    startAngle, endAngle);
            }

            if (got0)
            {
                p0 = q0;
                if (got1) { p1 = q1; hasSecond = true; }
            }
            else if (got1)
            {
                p0 = q1;
            }
            // If neither got0 nor got1: p0 stays default, hasSecond = false.
        }





        // World-space point on the segment at parameter t.
        private static Vector2 LinePoint(Vector2 start, Vector2 end, double t)
        {
            double dx = end.X - start.X, dy = end.Y - start.Y;
            return new Vector2((float)(start.X + t * dx), (float)(start.Y + t * dy));
        }



        // Parameter (eccentric anomaly) of the world point P on the given ellipse, in [0, 2π).
        private static double EllipsePointAngle(Vector2 P, Vector2 center, double a, double b, double rotation)
        {
            double cosT = Math.Cos(rotation), sinT = Math.Sin(rotation);
            double ux = cosT, uy = sinT;
            double vx = -sinT, vy = cosT;

            double px = P.X - center.X, py = P.Y - center.Y;
            double pu = px * ux + py * uy;
            double pv = px * vx + py * vy;

            double ang = Math.Atan2(pv / b, pu / a);
            if (ang < 0) ang += 2.0 * Math.PI;
            return ang;
        }


        // Same CCW test as AngleInArc, but named for clarity at the call site.
        private static bool EllipseAngleInArc(double angle, double startAngle, double endAngle, double eps = 1e-9)
        {
            return AngleInArc(angle, startAngle, endAngle, eps);
        }


        #endregion


        #region "Circle - Circle Helper Functions"


        /// <summary>
        /// Solves the intersection of two circles.
        /// Returns false if they do not intersect (separate, or one inside the other,
        /// or concentric with different radii). Returns true with one point for a
        /// tangency (external or internal), two points for a proper crossing.
        /// Does NOT handle the coincident case (d == 0, r1 == r2) — caller must check.
        /// </summary>
        private static bool TryIntersectTwoCircles(
            Vector2 c1, double r1, Vector2 c2, double r2,
            out Vector2 p0, out Vector2 p1, out bool hasSecond,
            double eps = 1e-9)
        {
            p0 = p1 = default;
            hasSecond = false;

            double dx = c2.X - c1.X, dy = c2.Y - c1.Y;
            double dSq = dx * dx + dy * dy;
            double d = Math.Sqrt(dSq);

            // Concentric: no intersection unless coincident (caller handles coincident).
            if (d < eps)
                return false;

            // No intersection if separate or one inside the other.
            if (d > r1 + r2 + eps) return false;
            if (d < Math.Abs(r1 - r2) - eps) return false;

            // Distance from c1 to the radical line along the c1->c2 axis.
            double a = (r1 * r1 - r2 * r2 + dSq) / (2.0 * d);

            // Half-chord length. Clamp to 0 to absorb rounding at tangency.
            double hSq = r1 * r1 - a * a;
            if (hSq < 0) hSq = 0;
            double h = Math.Sqrt(hSq);

            // Midpoint on the radical line.
            double mx = c1.X + a * dx / d;
            double my = c1.Y + a * dy / d;

            // Perpendicular unit direction.
            double px = -dy / d, py = dx / d;

            p0 = new Vector2((float)(mx + h * px), (float)(my + h * py));

            if (h > eps)
            {
                p1 = new Vector2((float)(mx - h * px), (float)(my - h * py));
                hasSecond = true;
            }
            return true;
        }



        /// <summary>
        /// Returns true iff the two arcs (or circles, if startAngle == endAngle meaning
        /// full circle — see note) share at least one intersection point.
        /// Angles are radians, CCW, arc convention [startAngle, endAngle] with wrap.
        /// </summary>
        private static bool DoArcsIntersect(
            Vector2 center1, float radius1, float startAngle1, float endAngle1,
            Vector2 center2, float radius2, float startAngle2, float endAngle2,
            double eps = 1e-9)
        {
            // --- Step 1: solve the two-circle intersection ---
            if (!TryIntersectTwoCircles(center1, radius1, center2, radius2,
                                        out Vector2 p0, out Vector2 p1,
                                        out bool hasSecond, eps))
                return false;

            // --- Step 2: filter by arc angular spans ---
            if (AngleInArc(PointAngle(p0, center1), startAngle1, endAngle1) &&
                AngleInArc(PointAngle(p0, center2), startAngle2, endAngle2))
                return true;

            if (hasSecond)
            {
                if (AngleInArc(PointAngle(p1, center1), startAngle1, endAngle1) &&
                    AngleInArc(PointAngle(p1, center2), startAngle2, endAngle2))
                    return true;
            }
            return false;
        }

        public static void GetArcArcIntersectionPoints(
            Vector2 center1, float radius1, float startAngle1, float endAngle1,
            Vector2 center2, float radius2, float startAngle2, float endAngle2,
            out Vector2 p0, out Vector2 p1, out bool hasSecond)
        {
            p0 = p1 = default; hasSecond = false;

            if (!TryIntersectTwoCircles(center1, radius1, center2, radius2,
                                        out Vector2 q0, out Vector2 q1, out bool two, 1e-9))
                return;

            bool got0 = AngleInArc(PointAngle(q0, center1), startAngle1, endAngle1)
                     && AngleInArc(PointAngle(q0, center2), startAngle2, endAngle2);
            bool got1 = two
                     && AngleInArc(PointAngle(q1, center1), startAngle1, endAngle1)
                     && AngleInArc(PointAngle(q1, center2), startAngle2, endAngle2);

            if (got0) { p0 = q0; if (got1) { p1 = q1; hasSecond = true; } }
            else if (got1) { p0 = q1; }
        }


        // Removes consecutive duplicates within `eps` from a sorted list.
        // Returns a new list; the input is not modified.
        private static List<double> DedupSorted(List<double> sorted, double eps = 1e-9)
        {
            var result = new List<double>(sorted.Count);
            foreach (double v in sorted)
            {
                if (result.Count == 0 || Math.Abs(v - result[result.Count - 1]) > eps)
                    result.Add(v);
            }
            return result;
        }


        // Builds an arc segment of `original` between the two angles (radians, CCW).
        // arcStartPt/arcEndPt are derived from the angles, not passed in, so the
        // stored points and angles are always consistent.
        private static circle_store MakeArc(circle_store original, double startAngle, double endAngle)
        {
            float cx = original.circleCenterPt.X;
            float cy = original.circleCenterPt.Y;
            float r = (float)original.circleRadius;

            return new circle_store
            {
                circle_id = 0,                    // reassigned by caller
                circleCenterPt = original.circleCenterPt,
                circleRadius = original.circleRadius,
                type = 1,                    // arc
                startAngle = (float)startAngle,
                endAngle = (float)endAngle,
                arcStartPt = new Vector2(
                                         cx + r * (float)Math.Cos(startAngle),
                                         cy + r * (float)Math.Sin(startAngle)),
                arcEndPt = new Vector2(
                                         cx + r * (float)Math.Cos(endAngle),
                                         cy + r * (float)Math.Sin(endAngle))
            };
        }


        #endregion


        #region "Circle - Ellipse Helper Functions"

        /// <summary>
        /// Finds circle-ellipse intersections by sampling g(θ) on the circle and
        /// refining sign changes. Returns θ values on the circle, sorted ascending.
        /// Tangencies are included as roots (single θ, not a sign change).
        /// </summary>
        private static List<double> SolveCircleEllipse(
            Vector2 circleCenter, double circleRadius,
            Vector2 ellipseCenter, double a, double b, double rotation,
            double eps = 1e-9)
        {
            var hits = new List<double>();
            if (circleRadius < eps || a < eps || b < eps) return hits;

            double cosT = Math.Cos(rotation), sinT = Math.Sin(rotation);
            double ux = cosT, uy = sinT;
            double vx = -sinT, vy = cosT;

            double a2 = a * a, b2 = b * b;

            // F(P) for a point P, in the ellipse's local frame.
            double F(double px, double py)
            {
                double ex = px - ellipseCenter.X, ey = py - ellipseCenter.Y;
                double lu = ex * ux + ey * uy;
                double lv = ex * vx + ey * vy;
                return (lu * lu) / a2 + (lv * lv) / b2 - 1.0;
            }

            // g(θ) = F(circle point at θ)
            double G(double theta)
            {
                double px = circleCenter.X + circleRadius * Math.Cos(theta);
                double py = circleCenter.Y + circleRadius * Math.Sin(theta);
                return F(px, py);
            }

            const int N = 128;
            double step = 2.0 * Math.PI / N;
            double prevTheta = 0.0;
            double prevG = G(prevTheta);

            for (int i = 1; i <= N; i++)
            {
                double theta = i * step;
                double g = G(theta);

                // Sign change: bisection refine.
                if (prevG * g < 0)
                {
                    double lo = prevTheta, hi = theta;
                    double glo = prevG;
                    for (int k = 0; k < 60; k++)
                    {
                        double mid = 0.5 * (lo + hi);
                        double gmid = G(mid);
                        if (gmid == 0) { lo = hi = mid; break; }
                        if (glo * gmid < 0) { hi = mid; }
                        else { lo = mid; glo = gmid; }
                    }
                    hits.Add(0.5 * (lo + hi));
                }
                else if (Math.Abs(g) < eps && Math.Abs(prevG) < eps)
                {
                    // Both samples near zero: tangent root, or a flat zero region.
                    // Record the midpoint; the caller dedups.
                    hits.Add(0.5 * (prevTheta + theta));
                }

                prevTheta = theta;
                prevG = g;
            }

            // Dedup within eps and sort (the wrap-around can put the last root
            // before the first; normalize and sort).
            hits.Sort();
            var deduped = new List<double>();
            foreach (double h in hits)
            {
                if (deduped.Count == 0 || Math.Abs(h - deduped[deduped.Count - 1]) > 1e-6)
                    deduped.Add(h);
            }
            return deduped;
        }



        /// <summary>
        /// Returns true iff the circle crosses the ellipse's boundary at least once,
        /// AND at least one such crossing lies within both angular/parametric spans.
        /// Pass startAngle = 0, endAngle = 2π for a full circle; similarly
        /// startParameter = 0, endParameter = 2π for a full ellipse.
        /// </summary>
        public static bool DoCircleEllipseIntersect(
            Vector2 circleCenter, float circleRadius, float circleStartAngle, float circleEndAngle,
            Vector2 ellipseCenter, float ellipseMajorAxis, float ellipseMinorAxis, float ellipseRotationAngle,
            float ellipseStartParameter, float ellipseEndParameter)
        {
            var thetas = SolveCircleEllipse(
                circleCenter, circleRadius,
                ellipseCenter, ellipseMajorAxis, ellipseMinorAxis, ellipseRotationAngle);
            if (thetas.Count == 0) return false;

            foreach (double theta in thetas)
            {
                // Circle-side filter
                if (!AngleInArc(theta, circleStartAngle, circleEndAngle)) continue;

                // Ellipse-side filter: convert the hit point to eccentric anomaly
                double px = circleCenter.X + circleRadius * Math.Cos(theta);
                double py = circleCenter.Y + circleRadius * Math.Sin(theta);
                var p = new Vector2((float)px, (float)py);
                double u = EllipsePointAngle(p, ellipseCenter, ellipseMajorAxis, ellipseMinorAxis, ellipseRotationAngle);
                if (AngleInArc(u, ellipseStartParameter, ellipseEndParameter)) return true;
            }
            return false;
        }



        public static void GetCircleEllipseIntersectionPoints(
    Vector2 circleCenter, float circleRadius, float circleStartAngle, float circleEndAngle,
    Vector2 ellipseCenter, float ellipseMajorAxis, float ellipseMinorAxis, float ellipseRotationAngle,
    float ellipseStartParameter, float ellipseEndParameter,
    out List<Vector2> points)
        {
            points = new List<Vector2>();

            var thetas = SolveCircleEllipse(
                circleCenter, circleRadius,
                ellipseCenter, ellipseMajorAxis, ellipseMinorAxis, ellipseRotationAngle);

            foreach (double theta in thetas)
            {
                if (!AngleInArc(theta, circleStartAngle, circleEndAngle)) continue;

                double px = circleCenter.X + circleRadius * Math.Cos(theta);
                double py = circleCenter.Y + circleRadius * Math.Sin(theta);
                var p = new Vector2((float)px, (float)py);

                double u = EllipsePointAngle(p, ellipseCenter, ellipseMajorAxis, ellipseMinorAxis, ellipseRotationAngle);
                if (AngleInArc(u, ellipseStartParameter, ellipseEndParameter))
                    points.Add(p);
            }
        }


        /// <summary>
        /// Splits a circle (type 0, wrap-around) or arc (type 1, bounded) at the given
        /// angles. Unchanged if fewer than 2 distinct angles (circle) or no interior
        /// angles (arc). Appends results to `output`.
        /// </summary>
        private static void SplitCircleAtAngles(
            circle_store original, List<double> hitAngles, List<circle_store> output)
        {
            if (hitAngles.Count == 0) { output.Add(original); return; }

            if (original.type == 0)
            {
                var distinct = DedupSorted(
                    hitAngles.Select(Normalize).OrderBy(a => a).ToList(), 1e-6);
                if (distinct.Count < 2) { output.Add(original); return; }
                for (int i = 0; i < distinct.Count; i++)
                    output.Add(MakeArc(original,
                                       distinct[i],
                                       distinct[(i + 1) % distinct.Count]));
            }
            else // type == 1, arc
            {
                double startA = Normalize(original.startAngle);
                double endA = Normalize(original.endAngle);
                double sweep = CCWSweep(startA, endA);

                var rel = new List<double> { 0.0 };
                foreach (double a in hitAngles)
                {
                    double r = CCWSweep(startA, Normalize(a));
                    if (r > 1e-9 && r < sweep - 1e-9) rel.Add(r);
                }
                rel.Add(sweep);
                rel.Sort();

                var relDistinct = DedupSorted(rel, 1e-6);
                if (relDistinct.Count <= 2) { output.Add(original); return; }

                var splitAngles = new List<double>(relDistinct.Count);
                for (int i = 0; i < relDistinct.Count; i++)
                {
                    if (i == 0) splitAngles.Add(startA);
                    else if (i == relDistinct.Count - 1) splitAngles.Add(endA);
                    else splitAngles.Add(Normalize(startA + relDistinct[i]));
                }
                for (int i = 0; i < splitAngles.Count - 1; i++)
                    output.Add(MakeArc(original, splitAngles[i], splitAngles[i + 1]));
            }
        }

        /// <summary>
        /// Splits an ellipse (type 0, wrap-around) or elliptical arc (type 1, bounded)
        /// at the given eccentric anomalies. Same structure as SplitCircleAtAngles.
        /// </summary>
        private static void SplitEllipseAtParameters(
            ellipse_store original, List<double> hitParams, List<ellipse_store> output)
        {
            if (hitParams.Count == 0) { output.Add(original); return; }

            if (original.type == 0)
            {
                var distinct = DedupSorted(
                    hitParams.Select(Normalize).OrderBy(a => a).ToList(), 1e-6);
                if (distinct.Count < 2) { output.Add(original); return; }
                for (int i = 0; i < distinct.Count; i++)
                    output.Add(MakeEllipseArc(original,
                                              distinct[i],
                                              distinct[(i + 1) % distinct.Count]));
            }
            else // type == 1, elliptical arc
            {
                double startA = Normalize(original.startParameter);
                double endA = Normalize(original.endParameter);
                double sweep = CCWSweep(startA, endA);

                var rel = new List<double> { 0.0 };
                foreach (double a in hitParams)
                {
                    double r = CCWSweep(startA, Normalize(a));
                    if (r > 1e-9 && r < sweep - 1e-9) rel.Add(r);
                }
                rel.Add(sweep);
                rel.Sort();

                var relDistinct = DedupSorted(rel, 1e-6);
                if (relDistinct.Count <= 2) { output.Add(original); return; }

                var splitParams = new List<double>(relDistinct.Count);
                for (int i = 0; i < relDistinct.Count; i++)
                {
                    if (i == 0) splitParams.Add(startA);
                    else if (i == relDistinct.Count - 1) splitParams.Add(endA);
                    else splitParams.Add(Normalize(startA + relDistinct[i]));
                }
                for (int i = 0; i < splitParams.Count - 1; i++)
                    output.Add(MakeEllipseArc(original, splitParams[i], splitParams[i + 1]));
            }
        }

        private static ellipse_store MakeEllipseArc(
            ellipse_store original, double startParam, double endParam)
        {
            return new ellipse_store
            {
                ellipse_id = 0,
                ellipseCenterPt = original.ellipseCenterPt,
                ellipseMajorAxis = original.ellipseMajorAxis,
                ellipseMinorAxis = original.ellipseMinorAxis,
                ellipseRotationAngle = original.ellipseRotationAngle,
                type = 1,
                startParameter = (float)startParam,
                endParameter = (float)endParam
            };
        }


        #endregion


        #region "Ellipse - Ellipse Helper Functions"

        /// <summary>
        /// Finds intersections of two ellipses by parameterizing the first ellipse
        /// by its eccentric anomaly u and finding zeros of the second ellipse's
        /// implicit function along that curve.
        /// Returns eccentric anomalies u on ellipse 1, sorted ascending, deduped.
        /// </summary>
        private static List<double> SolveEllipseEllipse(
            Vector2 c1, double a1, double b1, double rot1,
            Vector2 c2, double a2, double b2, double rot2,
            double eps = 1e-9)
        {
            var hits = new List<double>();
            if (a1 < eps || b1 < eps || a2 < eps || b2 < eps) return hits;

            // Ellipse 1 frame
            double cos1 = Math.Cos(rot1), sin1 = Math.Sin(rot1);
            double u1x = cos1, u1y = sin1;
            double v1x = -sin1, v1y = cos1;

            // Ellipse 2 frame
            double cos2 = Math.Cos(rot2), sin2 = Math.Sin(rot2);
            double u2x = cos2, u2y = sin2;
            double v2x = -sin2, v2y = cos2;

            double a2sq = a2 * a2, b2sq = b2 * b2;

            // Point on ellipse 1 at eccentric anomaly u
            void E1(double u, out double px, out double py)
            {
                px = c1.X + a1 * Math.Cos(u) * u1x + b1 * Math.Sin(u) * v1x;
                py = c1.Y + a1 * Math.Cos(u) * u1y + b1 * Math.Sin(u) * v1y;
            }

            // Implicit function of ellipse 2 evaluated at a world point.
            double F2(double px, double py)
            {
                double ex = px - c2.X, ey = py - c2.Y;
                double lu = ex * u2x + ey * u2y;
                double lv = ex * v2x + ey * v2y;
                return (lu * lu) / a2sq + (lv * lv) / b2sq - 1.0;
            }

            // g(u) = F2(E1(u))
            double G(double u)
            {
                E1(u, out double px, out double py);
                return F2(px, py);
            }

            const int N = 256;   // two ellipses can have 4 roots; use denser sampling than circle-ellipse
            double step = 2.0 * Math.PI / N;
            double prevU = 0.0;
            double prevG = G(prevU);

            for (int i = 1; i <= N; i++)
            {
                double u = i * step;
                double g = G(u);

                if (prevG * g < 0)
                {
                    double lo = prevU, hi = u, glo = prevG;
                    for (int k = 0; k < 60; k++)
                    {
                        double mid = 0.5 * (lo + hi);
                        double gmid = G(mid);
                        if (gmid == 0) { lo = hi = mid; break; }
                        if (glo * gmid < 0) hi = mid;
                        else { lo = mid; glo = gmid; }
                    }
                    hits.Add(0.5 * (lo + hi));
                }
                else if (Math.Abs(g) < eps && Math.Abs(prevG) < eps)
                {
                    hits.Add(0.5 * (prevU + u));   // tangent root
                }

                prevU = u;
                prevG = g;
            }

            hits.Sort();
            var deduped = new List<double>();
            foreach (double h in hits)
                if (deduped.Count == 0 || Math.Abs(h - deduped[deduped.Count - 1]) > 1e-6)
                    deduped.Add(h);
            return deduped;
        }


        /// <summary>
        /// Returns true iff ellipses (c1,a1,b1,rot1) and (c2,a2,b2,rot2) cross,
        /// AND at least one crossing lies in both parametric spans.
        /// </summary>
        public static bool DoEllipsesIntersect(
            Vector2 center1, float a1, float b1, float rotation1,
            float startParam1, float endParam1,
            Vector2 center2, float a2, float b2, float rotation2,
            float startParam2, float endParam2)
        {
            var us = SolveEllipseEllipse(
                center1, a1, b1, rotation1,
                center2, a2, b2, rotation2);
            if (us.Count == 0) return false;

            // Ellipse 1 frame (for evaluating points and converting to ellipse-2 param)
            double cos1 = Math.Cos(rotation1), sin1 = Math.Sin(rotation1);
            double u1x = cos1, u1y = sin1, v1x = -sin1, v1y = cos1;

            foreach (double u in us)
            {
                if (!AngleInArc(u, startParam1, endParam1)) continue;

                double px = center1.X + a1 * Math.Cos(u) * u1x + b1 * Math.Sin(u) * v1x;
                double py = center1.Y + a1 * Math.Cos(u) * u1y + b1 * Math.Sin(u) * v1y;
                var p = new Vector2((float)px, (float)py);

                double v = EllipsePointAngle(p, center2, a2, b2, rotation2);
                if (AngleInArc(v, startParam2, endParam2)) return true;
            }
            return false;
        }

        /// <summary>
        /// Two-point getter. DROPS the 3rd and 4th intersections if they exist.
        /// Use GetEllipseEllipseIntersectionPoints (List) for the split pass.
        /// </summary>
        public static void GetEllipticalArcArcIntersectionPoints(
            Vector2 center1, float a1, float b1, float rotation1,
            float startParam1, float endParam1,
            Vector2 center2, float a2, float b2, float rotation2,
            float startParam2, float endParam2,
            out Vector2 p0, out Vector2 p1, out bool hasSecond)
        {
            p0 = p1 = default;
            hasSecond = false;

            GetEllipseEllipseIntersectionPoints(
                center1, a1, b1, rotation1, startParam1, endParam1,
                center2, a2, b2, rotation2, startParam2, endParam2,
                out var pts);

            if (pts.Count >= 1) p0 = pts[0];
            if (pts.Count >= 2) { p1 = pts[1]; hasSecond = true; }
        }

        /// <summary>
        /// Full getter: returns all intersections within both parametric spans.
        /// </summary>
        public static void GetEllipseEllipseIntersectionPoints(
            Vector2 center1, float a1, float b1, float rotation1,
            float startParam1, float endParam1,
            Vector2 center2, float a2, float b2, float rotation2,
            float startParam2, float endParam2,
            out List<Vector2> points)
        {
            points = new List<Vector2>();

            var us = SolveEllipseEllipse(
                center1, a1, b1, rotation1,
                center2, a2, b2, rotation2);
            if (us.Count == 0) return;

            double cos1 = Math.Cos(rotation1), sin1 = Math.Sin(rotation1);
            double u1x = cos1, u1y = sin1, v1x = -sin1, v1y = cos1;

            foreach (double u in us)
            {
                if (!AngleInArc(u, startParam1, endParam1)) continue;

                double px = center1.X + a1 * Math.Cos(u) * u1x + b1 * Math.Sin(u) * v1x;
                double py = center1.Y + a1 * Math.Cos(u) * u1y + b1 * Math.Sin(u) * v1y;
                var p = new Vector2((float)px, (float)py);

                double v = EllipsePointAngle(p, center2, a2, b2, rotation2);
                if (AngleInArc(v, startParam2, endParam2))
                    points.Add(p);
            }
        }




        #endregion


        #region "Spline - Spline Helper Functions"

        // Proper crossing: interiors of both segments intersect at a point.
        // Returns false if they share an endpoint or are collinear-overlapping.
        private static bool ProperSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, double tol = 1e-9)
        {
            // Standard orientation test.
            double d1 = Cross(q2 - q1, p1 - q1);
            double d2 = Cross(q2 - q1, p2 - q1);
            double d3 = Cross(p2 - p1, q1 - p1);
            double d4 = Cross(p2 - p1, q2 - p1);

            // Endpoint contact: reject.
            if (Math.Abs(d1) < tol || Math.Abs(d2) < tol || Math.Abs(d3) < tol || Math.Abs(d4) < tol)
                return false;

            // Proper crossing: d1, d2 have opposite signs; d3, d4 have opposite signs.
            return (d1 * d2 < 0) && (d3 * d4 < 0);
        }

        private static double Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;


        private static Vector2 eval_spline(int degree, bool isPeriodic, bool isRational,
            List<double> weights, List<double> knots,
            List<Vector2> controlPoints, double u)
        {
            int p = degree;
            int n = controlPoints.Count - 1;   // highest control point index
            // var knots = s.knots;

            // Clamp u into the valid domain. For a clamped spline the valid domain is
            // [knots[p], knots[n+1]]. For periodic splines we wrap into the same range.
            double uMin = knots[p];
            double uMax = knots[n + 1];

            if (isPeriodic)
            {
                double span = uMax - uMin;
                if (span <= 0) return controlPoints[0];
                u = uMin + ((u - uMin) % span + span) % span;
            }
            else
            {
                if (u < uMin) u = uMin;
                if (u > uMax) u = uMax;
            }

            // Find knot span k such that knots[k] <= u < knots[k+1]
            // (with the standard special case at the right end: k = n).
            int k = p;
            if (u >= uMax) k = n;
            else
            {
                // binary search for speed; linear is fine for small control counts
                int lo = p, hi = n + 1;
                while (hi - lo > 1)
                {
                    int mid = (lo + hi) >> 1;
                    if (u < knots[mid]) hi = mid; else lo = mid;
                }
                k = lo;
            }

            // Copy the p+1 control points (homogeneous) into d[]
            int m = p + 1;
            var dx = new double[m];
            var dy = new double[m];
            var dw = new double[m];

            for (int j = 0; j < m; j++)
            {
                int idx = k - p + j;
                double w = isRational ? weights[idx] : 1.0;
                dx[j] = controlPoints[idx].X * w;
                dy[j] = controlPoints[idx].Y * w;
                dw[j] = w;
            }

            // De Boor recursion
            for (int r = 1; r <= p; r++)
            {
                for (int j = p; j >= r; j--)
                {
                    int i = j + k - p;
                    double denom = knots[i + p - r + 1] - knots[i];
                    double alpha = denom == 0.0 ? 0.0 : (u - knots[i]) / denom;

                    dx[j] = (1.0 - alpha) * dx[j - 1] + alpha * dx[j];
                    dy[j] = (1.0 - alpha) * dy[j - 1] + alpha * dy[j];
                    dw[j] = (1.0 - alpha) * dw[j - 1] + alpha * dw[j];
                }
            }

            double wf = dw[p];
            if (isRational && Math.Abs(wf) > 1e-12)
                return new Vector2((float)(dx[p] / wf), (float)(dy[p] / wf));
            else
                return new Vector2((float)dx[p], (float)dy[p]);
        }


        private static bool DoesSplineSelfIntersect(int degree, bool isPeriodic, bool isRational,
            List<double> weights, List<double> knots,
            List<Vector2> controlPoints)
        {
            // ---- Fast reject: self-intersecting control polygon implies self-intersecting spline ----
            int cpCount = controlPoints.Count;
            for (int i = 0; i < cpCount - 1; i++)
            {
                for (int j = i + 2; j < cpCount - 1; j++)
                {
                    if (isPeriodic && i == 0 && j == cpCount - 2) continue;   // wrap-around

                    if (ProperSegmentsIntersect( controlPoints[i], controlPoints[i + 1],
                        controlPoints[j], controlPoints[j + 1]))
                        return true;
                }
            }

            // ---- Main check: sample the spline densely ----
            int n = Math.Max(256, 16 * cpCount);
            var splinePoints = new List<Vector2>(n);

            int p = degree;
            int nCp = controlPoints.Count - 1;
            double uMin = knots[p];
            double uMax = knots[nCp + 1];
            double span = uMax - uMin;

            for (int i = 0; i < n; i++)
            {
                double u = uMin + span * ((double)i / (n - 1.0));
                splinePoints.Add(eval_spline(degree, isPeriodic, isRational, weights, knots, controlPoints, u));
            }

            int maxSegments = n - 1;
            for (int i = 0; i < maxSegments; i++)
            {
                Vector2 p1 = splinePoints[i];
                Vector2 p2 = splinePoints[i + 1];
                for (int j = i + 2; j < maxSegments; j++)
                {
                    if (isPeriodic && i == 0 && j == maxSegments - 1) continue;   // wrap-around

                    Vector2 q1 = splinePoints[j];
                    Vector2 q2 = splinePoints[j + 1];

                    if (ProperSegmentsIntersect(p1, p2, q1, q2))
                    {
                        return true;
                    }
                        //__________________
                }
            }
            return false;
        }



        private static void DedupeIntersectionParams(List<double> tLine, List<double> tSpline,
            List<Vector2> points, double tol = 1e-9)
        {
            // Dedupe by spline parameter (same u means same physical point).
            // More robust than deduping by tLine, because two near-equal tLine values
            // can come from distinct nearby intersections on a wiggly spline.
            var items = new List<(double tL, double tS, Vector2 p)>();

            for (int i = 0; i < tLine.Count; i++)
            {
                items.Add((tLine[i], tSpline[i], points[i]));
            }


            items.Sort((a, b) =>
            {
                int c = a.tL.CompareTo(b.tL);
                return c != 0 ? c : a.tS.CompareTo(b.tS);
            });

            var deduped = new List<(double tL, double tS, Vector2 p)>();
            foreach (var it in items)
            {
                if (deduped.Count > 0 && Math.Abs(it.tS - deduped[deduped.Count - 1].tS) <= tol)
                    continue;  // near-duplicate in spline parameter; keep first
                deduped.Add(it);
            }


            tLine.Clear(); tSpline.Clear(); points.Clear();
            foreach (var it in deduped) 
            { 
                tLine.Add(it.tL); tSpline.Add(it.tS); points.Add(it.p); 
            }

        }


        private static void SortByLineParam(List<double> tLine, List<double> tSpline,
             List<Vector2> points)
        {
            var items = new List<(double tL, double tS, Vector2 p)>();
            for (int i = 0; i < tLine.Count; i++)
            {
                items.Add((tLine[i], tSpline[i], points[i]));
            }


            items.Sort((a, b) =>
            {
                int c = a.tL.CompareTo(b.tL);
                return c != 0 ? c : a.tS.CompareTo(b.tS);
            });


            tLine.Clear(); tSpline.Clear(); points.Clear();

            foreach (var it in items) 
            { 
                tLine.Add(it.tL); tSpline.Add(it.tS); points.Add(it.p); 
            }

        }



        private static void GetLineSplineIntersectionParameters(
            Vector2 lineStartPt, Vector2 lineEndPt,
            int degree, bool isPeriodic, bool isRational,
            List<double> weights, List<double> knots,
            List<Vector2> controlPoints,
            out List<double> line_intersection_param_t,
            out List<double> spline_intersection_param_t,
             out List<Vector2> intersection_points)
        {
            line_intersection_param_t = new List<double>();
            spline_intersection_param_t = new List<double>();
            intersection_points = new List<Vector2>();

            int cpCount = controlPoints.Count;
            int n = Math.Max(256, 16 * cpCount);

            int p = degree;
            int nCp = controlPoints.Count - 1;
            double uMin = knots[p];
            double uMax = knots[nCp + 1];
            double span = uMax - uMin;

            // ---- 1. Sample the spline into a polyline, keeping parameters ----
            var splinePoints = new List<Vector2>(n);
            var splineParams = new List<double>(n);
            for (int i = 0; i < n; i++)
            {
                double u = uMin + span * ((double)i / (n - 1.0));
                splineParams.Add(u);
                splinePoints.Add(eval_spline(degree, isPeriodic, isRational, weights, knots, controlPoints, u));
            }

            // ---- 2. Intersect each polyline segment with the line segment ----
            Vector2 lineDir = lineEndPt - lineStartPt;
            double lineLen2 = lineDir.X * lineDir.X + lineDir.Y * lineDir.Y;
            if (lineLen2 < 1e-24)
            {
                // Degenerate line; no intersections.
                return;
            }

            for (int i = 0; i < n - 1; i++)
            {
                Vector2 P = splinePoints[i];
                Vector2 Q = splinePoints[i + 1];
                Vector2 PQ = Q - P;

                // Solve: lineStart + tLine * lineDir = P + alpha * PQ
                // Two equations, two unknowns (tLine, alpha).
                double denom = lineDir.X * PQ.Y - lineDir.Y * PQ.X;   // = cross(lineDir, PQ)
                if (Math.Abs(denom) < 1e-15)
                    continue;   // parallel or degenerate segment

                Vector2 AP = P - lineStartPt;
                double tLine = (AP.X * PQ.Y - AP.Y * PQ.X) / denom;
                double alpha = (AP.X * lineDir.Y - AP.Y * lineDir.X) / denom;

                // Reject if outside either segment.
                if (tLine < -1e-12 || tLine > 1.0 + 1e-12) continue;
                if (alpha < -1e-12 || alpha > 1.0 + 1e-12) continue;

                // Clamp small numerical overshoots.
                if (tLine < 0.0) tLine = 0.0;
                if (tLine > 1.0) tLine = 1.0;
                if (alpha < 0.0) alpha = 0.0;
                if (alpha > 1.0) alpha = 1.0;

                // Shared point: midpoint of line-side and polyline-side evaluations.
                // This is the point BOTH sides will use as their endpoint after the split.
                Vector2 P_line = lineStartPt + (float)tLine * lineDir;
                Vector2 P_spline = P + (float)alpha * PQ;
                Vector2 P_shared = 0.5f * (P_line + P_spline);


                // Map alpha ∈ [0, 1] on segment i to spline parameter.
                double uHit = splineParams[i] + alpha * (splineParams[i + 1] - splineParams[i]);
                double tSplineNorm = (uHit - uMin) / span;   // ∈ [0, 1]

                line_intersection_param_t.Add(tLine);
                spline_intersection_param_t.Add(tSplineNorm);
                intersection_points.Add(P_shared);
            }

            // ---- 3. Dedupe near-identical intersections ----
            // Two consecutive polyline segments meeting at a vertex can both report the
            // same hit (when the intersection is exactly at a sample point). Also, the
            // end of segment i and start of segment i+1 can both register alpha ≈ 1 and
            // alpha ≈ 0 for the same vertex.
            DedupeIntersectionParams(line_intersection_param_t, spline_intersection_param_t, intersection_points);

            // ---- 4. Sort by line parameter (stable order for downstream) ----
            SortByLineParam(line_intersection_param_t, spline_intersection_param_t, intersection_points);
        }


        #endregion

    }
}
