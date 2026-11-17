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



        public static void import_DXF_file(string filePath, ref drawingdata_store drawingData, ref List<Vector2> nodePtsList, ref bool IsModelSet)
        {

            // Read the entire file into a string
            string fileContents = File.ReadAllText(filePath);

            // Split the string into lines
            var lines = new List<string>(fileContents.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

            var allDxfPoints = new List<DxfPoint>();
            var allDxfLines = new List<DxfLine>();

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
                    int startPtId = DxfPoint.IsExist(allDxfPoints, lnStartPt);
                    if (startPtId == -1)
                    {
                        startPtId = allDxfPoints.Count;
                        lnStartPt.PtId = startPtId;
                        allDxfPoints.Add(lnStartPt);
                    }

                    // Add end point
                    int endPtId = DxfPoint.IsExist(allDxfPoints, lnEndPt);
                    if (endPtId == -1)
                    {
                        endPtId = allDxfPoints.Count;
                        lnEndPt.PtId = endPtId;
                        allDxfPoints.Add(lnEndPt);
                    }

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
                if (j < lines.Count && lines[j] == "POLYLINE")
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
                        int startPtId = DxfPoint.IsExist(allDxfPoints, lnStartPt);
                        if (startPtId == -1)
                        {
                            startPtId = allDxfPoints.Count;
                            lnStartPt.PtId = startPtId;
                            allDxfPoints.Add(lnStartPt);
                        }

                        // Add end point
                        int endPtId = DxfPoint.IsExist(allDxfPoints, lnEndPt);
                        if (endPtId == -1)
                        {
                            endPtId = allDxfPoints.Count;
                            lnEndPt.PtId = endPtId;
                            allDxfPoints.Add(lnEndPt);
                        }

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

                j++;
            }

            //// Build output string
            //var dxfInput = new StringBuilder();

            //foreach (var pt in allDxfPoints)
            //{
            //    dxfInput.AppendLine(string.Format(CultureInfo.InvariantCulture,
            //        "node, {0}, {1:F9}, {2:F9}", pt.PtId, pt.PtX, pt.PtY));
            //}

            //int lineId = 0;
            //foreach (var ln in allDxfLines)
            //{
            //    dxfInput.AppendLine(string.Format(CultureInfo.InvariantCulture,
            //        "line, {0}, {1}, {2}", lineId, ln.StartPtId, ln.EndPtId));
            //    lineId++;
            //}


            drawingData.line_data.clear_lines();

            int lineID = 0;

            // Add the lines to the lineMap
            foreach(DxfLine line in allDxfLines)
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




            IsModelSet = true;

        }








        public static void export_FDM_surface(string filePath)
        {


        }




//_____



    }
}
