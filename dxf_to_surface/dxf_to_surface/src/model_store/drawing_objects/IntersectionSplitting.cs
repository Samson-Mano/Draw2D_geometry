using dxf_to_surface.src.global_variables;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public static class IntersectionSplitting
    {


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
                line_store originalLine = line_data.lineMap[lineId];

                List<Vector2> intersectionPoints = new List<Vector2>();

                // Find the intersection points with the intersecting lines
                foreach (int intersectingLineId in intersectingLineIds)
                {
                    line_store intersectingLine = line_data.lineMap[intersectingLineId];

                    Vector2 intersectionPoint = gvariables_static.GetLineSegmentIntersectionPoint(originalLine.lineStartPt,
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


        }





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


    }
}
