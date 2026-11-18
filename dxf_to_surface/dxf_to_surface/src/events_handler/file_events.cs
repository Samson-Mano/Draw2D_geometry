// OpenTK library
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store.drawing_objects;
using dxf_to_surface.src.model_store.geom_objects;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace dxf_to_surface.src.events_handler
{
    public static class file_events
    {

        private static double RoundToSixDigits(double value) => Math.Round(value, 6);



        private class DxfPoint
        {
            public int PtId = 0;
            public double PtX = 0.0;
            public double PtY = 0.0;

            public static int IsExist(List<DxfPoint> allPts, DxfPoint thisPt)
            {
                foreach (var pt in allPts)
                {
                    if (Math.Abs(pt.PtX - thisPt.PtX) < 1e-6 &&
                        Math.Abs(pt.PtY - thisPt.PtY) < 1e-6)
                    {
                        return pt.PtId;
                    }
                }
                return -1;
            }
        }

        // Equivalent of dxf_lines struct
        private class DxfLine
        {
            public int StartPtId = 0;
            public int EndPtId = 0;

            public static bool IsExist(List<DxfLine> allLines, DxfLine thisLn)
            {
                foreach (var ln in allLines)
                {
                    if (ln.StartPtId == thisLn.StartPtId && ln.EndPtId == thisLn.EndPtId)
                        return true;

                    if (ln.StartPtId == thisLn.EndPtId && ln.EndPtId == thisLn.StartPtId)
                        return true;
                }
                return false;
            }
        }


        private class DxfCircle
        {
            public int CenterPtId = 0;
            public double Radius = 0.0;
            public double startAngle = 0.0;
            public double endAngle = 0.0;
            public int type = -1; // 0 for circle, 1 for arc


            public static bool IsExist(List<DxfCircle> allCircles, DxfCircle thisCircle)
            {
                foreach (var circle in allCircles)
                {
                    if (circle.CenterPtId == thisCircle.CenterPtId &&
                        circle.type == thisCircle.type &&
                        Math.Abs(circle.Radius - thisCircle.Radius) < 1e-6 &&
                        Math.Abs(circle.startAngle - thisCircle.startAngle) < 1e-6 &&
                        Math.Abs(circle.endAngle - thisCircle.endAngle) < 1e-6)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        private class DxfEllipse
        {
            public int CenterPtId = 0;
            public double ellipseMajorAxis = 0.0;
            public double ellipseMinorAxis = 0.0;
            public double ellipseRotationAngle = 0.0; // in radians
            public double startParameter = 0.0; // in degrees
            public double endParameter = 0.0; // in degrees
            public int type = -1; // 0 for circle, 1 for arc
            public static bool IsExist(List<DxfEllipse> allEllipses, DxfEllipse thisEllipse)
            {
                foreach (var ellipse in allEllipses)
                {
                    if (ellipse.CenterPtId == thisEllipse.CenterPtId &&
                        ellipse.type == thisEllipse.type &&
                        Math.Abs(ellipse.ellipseMajorAxis - thisEllipse.ellipseMajorAxis) < 1e-6 &&
                        Math.Abs(ellipse.ellipseMinorAxis - thisEllipse.ellipseMinorAxis) < 1e-6 &&
                        Math.Abs(ellipse.ellipseRotationAngle - thisEllipse.ellipseRotationAngle) < 1e-6 &&
                        Math.Abs(ellipse.startParameter - thisEllipse.startParameter) < 1e-6 &&
                        Math.Abs(ellipse.endParameter - thisEllipse.endParameter) < 1e-6)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        private class DxfSpline
        {
            public int degree = 0;
            public int flags = 0; // Flags (bit-coded): 1 = closed, 2 = periodic, 4 = rational, 8 = planar, 16 = linear
            public bool isClosed = false;
            public bool isPeriodic = false;
            public bool isRational = false;

            public List<int> controlPointIds = new List<int>();
            public List<double> weights = new List<double>();      // same count as controlPointIds (1.0 if not rational)
            public List<double> knotVector = new List<double>();
            public List<int> fitPointIds = new List<int>();        // used if there are no control points

            public static bool IsExist(List<DxfSpline> all, DxfSpline s)
            {
                foreach (var e in all)
                {
                    if (e.degree != s.degree ||
                        e.isClosed != s.isClosed ||
                        e.controlPointIds.Count != s.controlPointIds.Count ||
                        e.fitPointIds.Count != s.fitPointIds.Count ||
                        e.knotVector.Count != s.knotVector.Count)
                        continue;

                    bool same = true;
                    for (int i = 0; i < e.controlPointIds.Count && same; i++)
                        same = e.controlPointIds[i] == s.controlPointIds[i] &&
                               Math.Abs(e.weights[i] - s.weights[i]) < 1e-6;
                    for (int i = 0; i < e.fitPointIds.Count && same; i++)
                        same = e.fitPointIds[i] == s.fitPointIds[i];
                    for (int i = 0; i < e.knotVector.Count && same; i++)
                        same = Math.Abs(e.knotVector[i] - s.knotVector[i]) < 1e-6;

                    if (same) return true;
                }
                return false;
            }
        }

        private static int GetOrAddPoint(List<DxfPoint> allPts, DxfPoint pt)
        {
            int id = DxfPoint.IsExist(allPts, pt);
            if (id == -1)
            {
                id = allPts.Count;
                pt.PtId = id;
                allPts.Add(pt);
            }
            return id;
        }

        public static void import_DXF_file(string filePath, ref drawingdata_store drawingData, ref List<Vector2> nodePtsList, ref bool IsModelSet)
        {

            // Read the entire file into a string
            string fileContents = File.ReadAllText(filePath);

            // Split the string into lines
            var lines = new List<string>(fileContents.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

            var allDxfPoints = new List<DxfPoint>();
            var allDxfLines = new List<DxfLine>();
            var allDxfCircles = new List<DxfCircle>();
            var allDxfEllipses = new List<DxfEllipse>();
            var allDxfSplines = new List<DxfSpline>();

            int j = 0;

            // Process the lines
            while (j < lines.Count)
            {
                // Read the LINE Data
                if (lines[j] == "LINE")
                {
                    var lnStartPt = new DxfPoint();
                    var lnEndPt = new DxfPoint();

                    j = j + 5; // Move to the next five lines
                    while (j < lines.Count && lines[j] != "  0")
                    {
                        if (lines[j] == " 10")
                        {
                            lnStartPt.PtX = double.Parse(lines[j + 1], CultureInfo.InvariantCulture);
                        }
                        else if (lines[j] == " 20")
                        {
                            lnStartPt.PtY = double.Parse(lines[j + 1], CultureInfo.InvariantCulture);
                        }
                        else if (lines[j] == " 11")
                        {
                            lnEndPt.PtX = double.Parse(lines[j + 1], CultureInfo.InvariantCulture);
                        }
                        else if (lines[j] == " 21")
                        {
                            lnEndPt.PtY = double.Parse(lines[j + 1], CultureInfo.InvariantCulture);
                        }

                        j++;
                    }

                    // Add start point
                    int startPtId = GetOrAddPoint(allDxfPoints, lnStartPt);

                    // Add end point
                    int endPtId = GetOrAddPoint(allDxfPoints, lnEndPt);

                    // Add line
                    if (startPtId != endPtId)
                    {
                        var lineSegment = new DxfLine
                        {
                            StartPtId = startPtId,
                            EndPtId = endPtId
                        };

                        if (!DxfLine.IsExist(allDxfLines, lineSegment))
                        {
                            allDxfLines.Add(lineSegment);
                        }
                    }
                }

                // Read the POLYLINE Data
                if (j < lines.Count && (lines[j] == "POLYLINE" || lines[j] == "LWPOLYLINE"))
                {
                    var polylinePoints = new List<DxfPoint>();
                    var plnPt = new DxfPoint();
                    bool firstPt = true;

                    j++; // Move to the next line
                    while (j < lines.Count && lines[j] != "SEQEND")
                    {
                        if (lines[j] == " 10")
                        {
                            plnPt.PtX = double.Parse(lines[j + 1], CultureInfo.InvariantCulture);
                        }
                        else if (lines[j] == " 20")
                        {
                            plnPt.PtY = double.Parse(lines[j + 1], CultureInfo.InvariantCulture);

                            if (!firstPt)
                            {
                                if (DxfPoint.IsExist(polylinePoints, plnPt) == -1)
                                {
                                    polylinePoints.Add(plnPt);
                                }
                            }
                            else
                            {
                                firstPt = false;
                            }
                        }

                        j++;
                    }

                    // Iterate through the polyline points and add segments
                    for (int i = 0; i < polylinePoints.Count - 1; i++)
                    {
                        var lnStartPt = polylinePoints[i];
                        var lnEndPt = polylinePoints[i + 1];

                        // Add start point
                        int startPtId = GetOrAddPoint(allDxfPoints, lnStartPt);

                        // Add end point
                        int endPtId = GetOrAddPoint(allDxfPoints, lnEndPt);

                        // Add line
                        if (startPtId != endPtId)
                        {
                            var lineSegment = new DxfLine
                            {
                                StartPtId = startPtId,
                                EndPtId = endPtId
                            };

                            if (!DxfLine.IsExist(allDxfLines, lineSegment))
                            {
                                allDxfLines.Add(lineSegment);
                            }
                        }
                    }
                }


                // Read the CIRCLE Data
                if (j < lines.Count && lines[j].Trim() == "CIRCLE")
                {
                    var centerPt = new DxfPoint();
                    double radius = 0.0;

                    j++; // move to the first group code line
                         // Read (code, value) pairs until the next entity starts ("0")
                    while (j + 1 < lines.Count && lines[j].Trim() != "0")
                    {
                        string code = lines[j].Trim();
                        string value = lines[j + 1].Trim();

                        if (code == "10") centerPt.PtX = double.Parse(value, CultureInfo.InvariantCulture);
                        else if (code == "20") centerPt.PtY = double.Parse(value, CultureInfo.InvariantCulture);
                        else if (code == "40") radius = double.Parse(value, CultureInfo.InvariantCulture);

                        j += 2;
                    }

                    int centerPtId = GetOrAddPoint(allDxfPoints, centerPt);

                    var circle = new DxfCircle
                    {
                        CenterPtId = centerPtId,
                        Radius = radius,
                        startAngle = 0.0,
                        endAngle = 360.0,
                        type = 0 // Mark as circle
                    };

                    if (!DxfCircle.IsExist(allDxfCircles, circle))
                        allDxfCircles.Add(circle);
                }

                // Read the ARC Data
                if (j < lines.Count && lines[j].Trim() == "ARC")
                {
                    var centerPt = new DxfPoint();
                    double radius = 0.0, startAng = 0.0, endAng = 0.0;

                    j++;
                    while (j + 1 < lines.Count && lines[j].Trim() != "0")
                    {
                        string code = lines[j].Trim();
                        string value = lines[j + 1].Trim();

                        if (code == "10") centerPt.PtX = double.Parse(value, CultureInfo.InvariantCulture);
                        else if (code == "20") centerPt.PtY = double.Parse(value, CultureInfo.InvariantCulture);
                        else if (code == "40") radius = double.Parse(value, CultureInfo.InvariantCulture);
                        else if (code == "50") startAng = double.Parse(value, CultureInfo.InvariantCulture);
                        else if (code == "51") endAng = double.Parse(value, CultureInfo.InvariantCulture);

                        j += 2;
                    }

                    int centerPtId = GetOrAddPoint(allDxfPoints, centerPt);

                    var arc = new DxfCircle
                    {
                        CenterPtId = centerPtId,
                        Radius = radius,
                        startAngle = startAng,
                        endAngle = endAng,
                        type = 1 // Mark as arc
                    };

                    if (!DxfCircle.IsExist(allDxfCircles, arc))
                        allDxfCircles.Add(arc);
                }


                // Read the ELLIPSE data (covers both full ellipses and elliptical arcs)
                if (j < lines.Count && lines[j].Trim() == "ELLIPSE")
                {
                    var centerPt = new DxfPoint();
                    double majorX = 0.0, majorY = 0.0;
                    double ratio = 1.0;
                    double startParam = 0.0;
                    double endParam = 2.0 * Math.PI; // defaults describe a full ellipse

                    j++; // move to the first group code line
                    while (j + 1 < lines.Count && lines[j].Trim() != "0")
                    {
                        string code = lines[j].Trim();
                        string value = lines[j + 1].Trim();

                        switch (code)
                        {
                            case "10": centerPt.PtX = double.Parse(value, CultureInfo.InvariantCulture); break;
                            case "20": centerPt.PtY = double.Parse(value, CultureInfo.InvariantCulture); break;
                            case "11": majorX = double.Parse(value, CultureInfo.InvariantCulture); break;
                            case "21": majorY = double.Parse(value, CultureInfo.InvariantCulture); break;
                            case "40": ratio = double.Parse(value, CultureInfo.InvariantCulture); break;
                            case "41": startParam = double.Parse(value, CultureInfo.InvariantCulture); break;
                            case "42": endParam = double.Parse(value, CultureInfo.InvariantCulture); break;
                        }

                        j += 2;
                    }

                    double majorLen = Math.Sqrt(majorX * majorX + majorY * majorY);
                    double minorLen = majorLen * ratio;
                    double rotation = Math.Atan2(majorY, majorX); // radians

                    // Full ellipse if the sweep is 0..2π (or any start == end modulo 2π)
                    double sweep = endParam - startParam;
                    bool isFull = Math.Abs(sweep) < 1e-9
                               || Math.Abs(Math.Abs(sweep) - 2.0 * Math.PI) < 1e-6;

                    int centerPtId = GetOrAddPoint(allDxfPoints, centerPt);

                    var ellipse = new DxfEllipse
                    {
                        CenterPtId = centerPtId,
                        ellipseMajorAxis = majorLen,
                        ellipseMinorAxis = minorLen,
                        ellipseRotationAngle = rotation,
                        startParameter = isFull ? 0.0 : startParam,
                        endParameter = isFull ? 2.0 * Math.PI : endParam,
                        type = isFull ? 0 : 1 // 0 = full ellipse, 1 = elliptical arc
                    };

                    if (!DxfEllipse.IsExist(allDxfEllipses, ellipse))
                        allDxfEllipses.Add(ellipse);
                }


                // Read the SPLINE data
                if (j < lines.Count && lines[j].Trim() == "SPLINE")
                {
                    int flags = 0, degree = 0;
                    var knots = new List<double>();
                    var weights = new List<double>();
                    var ctrlX = new List<double>();
                    var ctrlY = new List<double>();
                    var fitX = new List<double>();
                    var fitY = new List<double>();

                    j++; // move to the first group code line
                    while (j + 1 < lines.Count && lines[j].Trim() != "0")
                    {
                        string code = lines[j].Trim();
                        string value = lines[j + 1].Trim();

                        switch (code)
                        {
                            case "70": flags = int.Parse(value, CultureInfo.InvariantCulture); break;
                            case "71": degree = int.Parse(value, CultureInfo.InvariantCulture); break;

                            case "40": knots.Add(double.Parse(value, CultureInfo.InvariantCulture)); break;
                            case "41": weights.Add(double.Parse(value, CultureInfo.InvariantCulture)); break;

                            // 10 starts a new control point, 20 fills in its Y
                            case "10": ctrlX.Add(double.Parse(value, CultureInfo.InvariantCulture)); ctrlY.Add(0.0); break;
                            case "20": if (ctrlY.Count > 0) ctrlY[ctrlY.Count - 1] = double.Parse(value, CultureInfo.InvariantCulture); break;

                            // 11 starts a new fit point, 21 fills in its Y
                            case "11": fitX.Add(double.Parse(value, CultureInfo.InvariantCulture)); fitY.Add(0.0); break;
                            case "21": if (fitY.Count > 0) fitY[fitY.Count - 1] = double.Parse(value, CultureInfo.InvariantCulture); break;

                                // 72/73/74 (counts), 42-44 (tolerances), 12/13 (tangents), 30/31/32... are ignored
                        }

                        j += 2;
                    }

                    var spline = new DxfSpline
                    {
                        degree = degree,
                        flags = flags,
                        isClosed = (flags & 1) != 0,
                        isPeriodic = (flags & 2) != 0,
                        isRational = (flags & 4) != 0,
                        knotVector = knots
                    };

                    for (int i = 0; i < ctrlX.Count; i++)
                    {
                        int id = GetOrAddPoint(allDxfPoints, new DxfPoint { PtX = ctrlX[i], PtY = ctrlY[i] });
                        spline.controlPointIds.Add(id);
                        spline.weights.Add(i < weights.Count ? weights[i] : 1.0);
                    }

                    for (int i = 0; i < fitX.Count; i++)
                    {
                        int id = GetOrAddPoint(allDxfPoints, new DxfPoint { PtX = fitX[i], PtY = fitY[i] });
                        spline.fitPointIds.Add(id);
                    }

                    if (!DxfSpline.IsExist(allDxfSplines, spline))
                        allDxfSplines.Add(spline);
                }


                j++;
            }


            //___________________________________________________________________________________

            drawingData.line_data.clear_lines();

            int lineID = 0;

            // Add the lines to the lineMap
            foreach (DxfLine line in allDxfLines)
            {
                DxfPoint startPt = allDxfPoints[line.StartPtId];
                DxfPoint endPt = allDxfPoints[line.EndPtId];

                Vector2 StartPoint = new Vector2((float)startPt.PtX, (float)startPt.PtY);
                Vector2 EndPoint = new Vector2((float)endPt.PtX, (float)endPt.PtY);

                // Add to the NodePtsList for scaling the model
                nodePtsList.Add(StartPoint);
                nodePtsList.Add(EndPoint);


                drawingData.line_data.add_line(lineID, StartPoint, EndPoint);
                lineID++;
            }

            drawingData.line_data.UpdateBuffers();

            //___________________________________________________________________________________

            drawingData.circle_data.clear_circles();

            int circleID = 0;

            // Add the circles and arcs to the circleMap
            foreach (DxfCircle circle in allDxfCircles)
            {
                DxfPoint centerPt = allDxfPoints[circle.CenterPtId];
                Vector2 CenterPoint = new Vector2((float)centerPt.PtX, (float)centerPt.PtY);

                // Add to the NodePtsList for scaling the model
                nodePtsList.Add(CenterPoint);
                // Add four end point on the circumference for scaling
                nodePtsList.Add(new Vector2((float)centerPt.PtX + (float)circle.Radius, (float)centerPt.PtY)); 
                nodePtsList.Add(new Vector2((float)centerPt.PtX - (float)circle.Radius, (float)centerPt.PtY)); 
                nodePtsList.Add(new Vector2((float)centerPt.PtX, (float)centerPt.PtY + (float)circle.Radius)); 
                nodePtsList.Add(new Vector2((float)centerPt.PtX, (float)centerPt.PtY - (float)circle.Radius)); 

                if (circle.type == 0) // Circle
                {
                    // add_circle(int circleID, Vector2 centerPt, double radius, double startAngle, double endAngle, int type)
                    drawingData.circle_data.add_circle(circleID, CenterPoint, circle.Radius, circle.startAngle, circle.endAngle, circle.type);
                }
                else if (circle.type == 1) // Arc
                {
                    drawingData.circle_data.add_circle(circleID, CenterPoint,circle.Radius,circle.startAngle, circle.endAngle, circle.type);
                }
                circleID++;
            }

            drawingData.circle_data.UpdateBuffers();

            //___________________________________________________________________________________

            drawingData.ellipse_data.clear_ellipses();

            int ellipseID = 0;

            // Add the ellipses and elliptical arcs to the ellipseMap
            foreach (DxfEllipse ellipse in allDxfEllipses)
            {
                DxfPoint centerPt = allDxfPoints[ellipse.CenterPtId];
                Vector2 CenterPoint = new Vector2((float)centerPt.PtX, (float)centerPt.PtY);

                // Add to the NodePtsList for scaling the model
                nodePtsList.Add(CenterPoint);
                // Add four end point on the circumference for scaling
                nodePtsList.Add(new Vector2((float)centerPt.PtX + (float)ellipse.ellipseMajorAxis, (float)centerPt.PtY));
                nodePtsList.Add(new Vector2((float)centerPt.PtX - (float)ellipse.ellipseMajorAxis, (float)centerPt.PtY));
                nodePtsList.Add(new Vector2((float)centerPt.PtX, (float)centerPt.PtY + (float)ellipse.ellipseMajorAxis));
                nodePtsList.Add(new Vector2((float)centerPt.PtX, (float)centerPt.PtY - (float)ellipse.ellipseMajorAxis));

                drawingData.ellipse_data.add_ellipse(ellipseID, CenterPoint, ellipse.ellipseMajorAxis, 
                    ellipse.ellipseMinorAxis, ellipse.ellipseRotationAngle, ellipse.type,
                    ellipse.startParameter, ellipse.endParameter);

                ellipseID++;
            }


            drawingData.ellipse_data.UpdateBuffers();

            //___________________________________________________________________________________

            drawingData.spline_data.clear_splines();

            int splineID = 0;

            // Add the splines to the splineMap
            foreach (DxfSpline spline in allDxfSplines)
            {


                splineID++;
            }

            drawingData.spline_data.UpdateBuffers();


            //___________________________________________________________________________________

            IsModelSet = true;

        }








        public static void export_FDM_surface(string filePath)
        {


        }




        //_____



    }
}
