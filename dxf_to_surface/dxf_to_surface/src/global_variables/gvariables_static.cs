using OpenTK;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace dxf_to_surface.src.global_variables
{
    public static class gvariables_static
    {

        public static class ColorUtils
        {
            private static readonly List<Color> StandardColors = new List<Color>
            {
        Color.Blue, Color.BlueViolet, Color.Brown, Color.BurlyWood, Color.CadetBlue, Color.Chocolate,
        Color.Coral, Color.CornflowerBlue, Color.Crimson, Color.DarkBlue, Color.DarkCyan, Color.DarkGoldenrod,
        Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta, Color.DarkOliveGreen, Color.DarkOrange,
        Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen, Color.DarkSlateBlue,
        Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue,
        Color.DodgerBlue, Color.Firebrick, Color.ForestGreen, Color.Fuchsia, Color.Goldenrod, Color.Green,
        Color.HotPink, Color.IndianRed, Color.Indigo, Color.Khaki, Color.LightCoral, Color.LightSalmon,
        Color.LightSeaGreen, Color.LightSkyBlue, Color.LightSteelBlue, Color.LimeGreen, Color.Magenta,
        Color.Maroon, Color.MediumAquamarine, Color.MediumBlue, Color.MediumOrchid, Color.MediumPurple,
        Color.MediumSeaGreen, Color.MediumSlateBlue, Color.MediumTurquoise, Color.MediumVioletRed,
        Color.MidnightBlue, Color.Navy, Color.Olive, Color.OliveDrab, Color.Orange, Color.OrangeRed,
        Color.Orchid, Color.PaleVioletRed, Color.Peru, Color.Purple, Color.Red, Color.RosyBrown,
        Color.RoyalBlue, Color.SaddleBrown, Color.Salmon, Color.SandyBrown, Color.SeaGreen, Color.Sienna,
        Color.SkyBlue, Color.SlateBlue, Color.SlateGray, Color.SteelBlue, Color.Tan, Color.Teal,
        Color.Thistle, Color.Tomato, Color.Turquoise, Color.Violet, Color.Wheat, Color.Yellow,
        Color.YellowGreen
    };

            private static readonly List<Color> ShuffledColors;

            static ColorUtils()
            {
                // Shuffle with fixed seed for deterministic results
                Random rng = new Random(42);
                ShuffledColors = new List<Color>(StandardColors);

                for (int i = ShuffledColors.Count - 1; i > 0; i--)
                {
                    int swapIndex = rng.Next(i + 1);
                    (ShuffledColors[i], ShuffledColors[swapIndex]) = (ShuffledColors[swapIndex], ShuffledColors[i]);
                }
            }

            /// <summary>
            /// Returns a deterministic pseudo-random color based on the given ID.
            /// </summary>
            public static Color GetRandomColor(int colorId)
            {
                return ShuffledColors[Math.Abs(colorId) % ShuffledColors.Count];
            }


            public static Vector3 get_PtColor()
            {
                Color color = Color.Brown;

                return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            }

            public static Vector3 get_LineColor()
            {
                Color color = Color.SkyBlue;

                return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            }


            public static Vector3 get_SelectionPtColor()
            {
                Color color = Color.Red;

                return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            }

            public static Vector3 get_ConstraintColor()
            {
                Color color = Color.Magenta;

                return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            }

            public static Vector3 get_LoadColor()
            {
                Color color = Color.Green;

                return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            }


            public static Vector3 get_WireframeColor()
            {
                Color color = Color.Gray;

                return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            }


            /// <summary>
            /// Returns a Vector3 representation of a color for mesh rendering.
            /// </summary>
            public static Vector3 MeshGetRandomColor(int colorId)
            {

                Color color = GetRandomColor(colorId);

                return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);

            }


        }



        public static Color glcontrol_background_color = Color.White;

        // Garphics Control variables
        public static bool is_paint_meshpoints = false;
        public static bool is_paint_mesh_boundaries = true;
        public static bool is_paint_mesh = true;
        public static bool is_paint_constraints = true;
        public static bool is_paint_loads = true;

        public static bool is_paint_loads_label = true;

        public static bool is_paint_shrunk_triangle = false;


        public static bool is_show_transparent_model_mesh = true;
        public static bool is_paint_resultmeshpoints = false;
        public static bool is_paint_resultmesh_boundaries = false;
        public static bool is_paint_resultmesh = true;
        public static bool is_paint_result_shrunk_triangle = false;

        public static bool is_paint_result_contourlines = true;
       
        public static float contourLevel_rangeMax = 1.0f;
        public static float contourLevel_rangeMin = 0.0f;

        public static int result_option = 0; 
        // 1 = displacement, 2 = stressX, 3 = stressY, 4 = tauXY, 5 = vonMises,
        // 6 = principalStress1, 7 = principalStress2, 8 = maxShearStress, 9 = PSL

        public static bool is_paint_result_displacement = false; // option 1
        public static bool is_paint_result_stressX = false; // option 2
        public static bool is_paint_result_stressY = false; // option 3
        public static bool is_paint_result_tauXY = false; // option 4
        public static bool is_paint_result_vonMises = false; // option 5
        public static bool is_paint_result_principalStress1 = false; // option 6
        public static bool is_paint_result_principalStress2 = false; // option 7
        public static bool is_paint_result_maxShearStress = false; // option 8
        public static bool is_paint_result_PSL = false; // option 9

        public static float mesh_shrink_factor = 0.8f;
        public static float selectedmesh_shrink_factor = 0.8f;
        public static bool is_RectangleSelection = true; // true = Rectangle selection, false = Circle Selection

        public static float geom_size = 0.0f;


        public static float PointSize = 1.0f;
        public static float LineWidth = 2.0f;

        public static GDIFontAtlas main_font = new GDIFontAtlas();


        public static float geom_transparency = 1.0f;
        public static float rslt_transparency = 0.8f;


        // Control Modal Animation
        public static bool animate_play = false;
        public static bool animate_pause = false;
        public static bool animate_stop = true;



        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool PeekMessage(out Message msg, IntPtr hWnd,
                            uint messageFilterMin, uint messageFilterMax, uint flags);



        public static bool is_paint_result()
        {
          return gvariables_static.is_paint_result_displacement == true ||
                    gvariables_static.is_paint_result_stressX == true ||
                    gvariables_static.is_paint_result_stressY == true ||
                    gvariables_static.is_paint_result_tauXY == true ||
                    gvariables_static.is_paint_result_vonMises == true ||
                    gvariables_static.is_paint_result_principalStress1 == true ||
                    gvariables_static.is_paint_result_principalStress2 == true ||
                    gvariables_static.is_paint_result_maxShearStress == true ||
                    gvariables_static.is_paint_result_PSL == true;
        }


        public static int RoundOff(this int i)
        {
            // Roundoff to nearest 10 (used to display zoom value)
            return ((int)Math.Round(i / 10.0)) * 10;
        }


        public static float get_font_scale(float font_size)
        {
            return gvariables_static.geom_size * 0.01f * (font_size / 12.0f);

        }


        //public static float get_text_height(float font_size)
        //{
        //    char ch = 'A';
        //    Character ch_data = gvariables_static.main_font.Glyphs[ch];

        //    return ch_data.Size.Y * get_font_scale(font_size);

        //}


        public static int error_tracker = 0;

        public static void Show_error_Dialog(string title, string text)
        {
            var form = new Form()
            {
                Text = title,
                Size = new Size(800, 600)
            };

            form.Controls.Add(new TextBox()
            {
                Font = new Font("Segoe UI", 12),
                Text = text,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill
            });

            form.ShowDialog();
            form.Controls.OfType<TextBox>().First().Dispose();
            form.Dispose();
        }


        public static int get_unique_id(List<int> all_ids)
        {
            // Return the unique id
            if (all_ids != null && all_ids.Count != 0)
            {
                // Sort the list ascendingly
                all_ids.Sort();

                // Find if any of the nodes are missing in an ordered int
                for (int i = 0; i < all_ids.Count; i++)
                {
                    if (all_ids[i] != i)
                    {
                        return i;
                    }
                }

                // no node id is missing in an ordered list so add to the end
                return all_ids.Count;
            }

            return 0;
        }

        public static Vector2 RotatePoint(Vector2 rotateAbout, Vector2 pt, double rotationAngle)
        {
            // Translate the point relative to the rotation center
            Vector2 translatedPt = pt - rotateAbout;

            double rotationRadian = (Math.PI / 180.0) * rotationAngle;

            // Apply rotation
            double cosTheta = Math.Cos(rotationRadian);
            double sinTheta = Math.Sin(rotationRadian);

            // Rotate the point
            Vector2 rotatedPt = new Vector2(
                (float)((translatedPt.X * cosTheta) - (translatedPt.Y * sinTheta)),
                (float)((translatedPt.X * sinTheta) + (translatedPt.Y * cosTheta))
            );

            // Translate back and return
            return rotatedPt + rotateAbout;
        }


        public static Vector3 FindGeometricCenter(List<Vector3> allPts)
        {
            // Function returns the geometric center of the nodes
            // Initialize the sum with zero
            Vector3 sum = Vector3.Zero;

            // Sum the points
            foreach (var pt in allPts)
            {
                sum += pt;
            }

            return allPts.Count > 0 ? sum / allPts.Count : Vector3.Zero;
        }


        public static Vector2 FindGeometricCenter(List<Vector2> allPts)
        {
            // Function returns the geometric center of the nodes
            // Initialize the sum with zero
            Vector2 sum = Vector2.Zero;

            // Sum the points
            foreach (var pt in allPts)
            {
                sum += pt;
            }

            return allPts.Count > 0 ? sum / allPts.Count : Vector2.Zero;
        }



        public static Tuple<Vector3, Vector3> FindMinMaxXY(List<Vector3> allPts)
        {
            if (allPts == null || allPts.Count < 1)
            {
                // Null input
                return Tuple.Create(Vector3.Zero, Vector3.Zero);
            }

            // Initialize min and max values to first node in map
            Vector3 minXY = allPts[0];
            Vector3 maxXY = allPts[0];

            // Loop through all nodes in map and update min and max values
            foreach (var pt in allPts)
            {
                // Minimum
                minXY.X = Math.Min(minXY.X, pt.X);
                minXY.Y = Math.Min(minXY.Y, pt.Y);
                minXY.Z = Math.Min(minXY.Z, pt.Z);

                // Maximum
                maxXY.X = Math.Max(maxXY.X, pt.X);
                maxXY.Y = Math.Max(maxXY.Y, pt.Y);
                maxXY.Z = Math.Max(maxXY.Z, pt.Z);
            }

            // Return pair of min and max values
            return Tuple.Create(minXY, maxXY);
        }


        public static Tuple<Vector2, Vector2> FindMinMaxXY(List<Vector2> allPts)
        {
            if (allPts == null || allPts.Count < 1)
            {
                // Null input
                return Tuple.Create(Vector2.Zero, Vector2.Zero);
            }

            // Initialize min and max values to first node in map
            Vector2 minXY = allPts[0];
            Vector2 maxXY = allPts[0];

            // Loop through all nodes in map and update min and max values
            foreach (var pt in allPts)
            {
                // Minimum
                minXY.X = Math.Min(minXY.X, pt.X);
                minXY.Y = Math.Min(minXY.Y, pt.Y);

                // Maximum
                maxXY.X = Math.Max(maxXY.X, pt.X);
                maxXY.Y = Math.Max(maxXY.Y, pt.Y);
            }

            // Return pair of min and max values
            return Tuple.Create(minXY, maxXY);
        }



        public static double Clamp(double value, double min, double max)
        {
            // Perform .Net Core 2.0s Math.Clamp
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // Manual clamp for .NET Framework 4.8
        private static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        // Original jet colormap with clamping (for values in [0,1])
        public static Vector3 GetJetColorClamped(float t)
        {
            float r = Clamp(1.5f - Math.Abs(4.0f * t - 3.0f), 0f, 1f);
            float g = Clamp(1.5f - Math.Abs(4.0f * t - 2.0f), 0f, 1f);
            float b = Clamp(1.5f - Math.Abs(4.0f * t - 1.0f), 0f, 1f);

            return new Vector3(r, g, b);
        }



        public static double UpdateZoom(double zoomVal, int eDelta, double zoomStep = 1.1f,
            double minZoom = 1e-3d, double maxZoom = 1e6d)
        {
            if (eDelta == 0)
                return zoomVal;

            // Normalize: Windows wheel delta is usually multiples of 120
            float steps = eDelta / 120.0f;

            // Exponential zoom scaling
            zoomVal *= Math.Pow(zoomStep, steps);

            // Clamp to reasonable soft limits
            zoomVal = Clamp(zoomVal, minZoom, maxZoom);

            return zoomVal;
        }



        public static Vector3 linear_interpolation3d(Vector3 pt1, Vector3 pt2, double param_t)
        {
            return new Vector3((float)(pt1.X * (1.0 - param_t) + (pt2.X * param_t)),
                             (float)(pt1.Y * (1.0 - param_t) + (pt2.Y * param_t)),
                             (float)(pt1.Z * (1.0 - param_t) + (pt2.Z * param_t)));

        }


        public static bool isPointSelected(Vector2 rectCpt1, Vector2 rectCpt2, Vector2 pt)
        {
            if (is_RectangleSelection == true)
            {
                return isPointInsideRectangle(rectCpt1, rectCpt2, pt);
            }
            else
            {
                return isPointInsideCircle(rectCpt1, rectCpt2, pt);
            }

        }


        private static bool isPointInsideRectangle(Vector2 rectCpt1, Vector2 rectCpt2, Vector2 pt)
        {
            return pt.X >= Math.Min(rectCpt1.X, rectCpt2.X) &&
                   pt.X <= Math.Max(rectCpt1.X, rectCpt2.X) &&
                   pt.Y >= Math.Min(rectCpt1.Y, rectCpt2.Y) &&
                   pt.Y <= Math.Max(rectCpt1.Y, rectCpt2.Y);
        }


        private static bool isPointInsideCircle(Vector2 rectCpt1, Vector2 rectCpt2, Vector2 pt)
        {
            // Calculate center and radius
            Vector2 center = new Vector2((rectCpt1.X + rectCpt2.X) * 0.5f, (rectCpt1.Y + rectCpt2.Y) * 0.5f);
            float radius = Vector2.Distance(rectCpt1, rectCpt2) * 0.5f;

            // Check if point is within radius
            return Vector2.Distance(pt, center) < radius;

        }


        public static bool isEdgeSelected(Vector2 rectCpt1, Vector2 rectCpt2, 
            Vector2 EdgeStartpt, Vector2 EdgeEndPt)
        {

            if (is_RectangleSelection == true)
            {
                return isEdgeIntersectRectangle(rectCpt1, rectCpt2, EdgeStartpt, EdgeEndPt);
            }
            else
            {
                return isEdgeIntersectCircle(rectCpt1, rectCpt2, EdgeStartpt, EdgeEndPt);
            }

        }


        public static bool isEdgeIntersectRectangle(Vector2 rectCpt1, Vector2 rectCpt2,
            Vector2 EdgeStartpt, Vector2 EdgeEndPt)
        {
            // Get rectangle bounds
            float minX = Math.Min(rectCpt1.X, rectCpt2.X);
            float maxX = Math.Max(rectCpt1.X, rectCpt2.X);
            float minY = Math.Min(rectCpt1.Y, rectCpt2.Y);
            float maxY = Math.Max(rectCpt1.Y, rectCpt2.Y);

            // Check if either endpoint is inside the rectangle
            if (isPointInsideRectangle(rectCpt1, rectCpt2, EdgeStartpt) ||
                isPointInsideRectangle(rectCpt1, rectCpt2, EdgeEndPt))
            {
                return true;
            }

            // Check if the line segment intersects any of the rectangle's edges
            // Rectangle edges: left, right, bottom, top
            Vector2 rectEdgesStart = new Vector2(minX, minY);
            Vector2 rectEdgesEnd = new Vector2(maxX, minY);   // Bottom edge
            if (DoLinesIntersect(EdgeStartpt, EdgeEndPt, rectEdgesStart, rectEdgesEnd))
                return true;

            rectEdgesStart = new Vector2(maxX, minY);
            rectEdgesEnd = new Vector2(maxX, maxY);           // Right edge
            if (DoLinesIntersect(EdgeStartpt, EdgeEndPt, rectEdgesStart, rectEdgesEnd))
                return true;

            rectEdgesStart = new Vector2(maxX, maxY);
            rectEdgesEnd = new Vector2(minX, maxY);           // Top edge
            if (DoLinesIntersect(EdgeStartpt, EdgeEndPt, rectEdgesStart, rectEdgesEnd))
                return true;

            rectEdgesStart = new Vector2(minX, maxY);
            rectEdgesEnd = new Vector2(minX, minY);           // Left edge
            if (DoLinesIntersect(EdgeStartpt, EdgeEndPt, rectEdgesStart, rectEdgesEnd))
                return true;

            return false;

        }


        public static bool isEdgeIntersectCircle(Vector2 rectCpt1, Vector2 rectCpt2,
            Vector2 EdgeStartpt, Vector2 EdgeEndPt)
        {
            // Calculate center and radius
            Vector2 center = new Vector2((rectCpt1.X + rectCpt2.X) * 0.5f, (rectCpt1.Y + rectCpt2.Y) * 0.5f);
            float radius = Vector2.Distance(rectCpt1, rectCpt2) * 0.5f;

            // Check if either endpoint is inside the circle
            if (isPointInsideCircle(rectCpt1, rectCpt2, EdgeStartpt) ||
                isPointInsideCircle(rectCpt1, rectCpt2, EdgeEndPt))
            {
                return true;
            }

            // Check if the line segment intersects the circle
            return DoLineSegmentIntersectCircle(EdgeStartpt, EdgeEndPt, center, radius);

        }


        // Helper method to check if a line segment intersects a circle
        private static bool DoLineSegmentIntersectCircle(Vector2 start, Vector2 end, Vector2 center, float radius)
        {
            // Vector from start to end
            Vector2 d = end - start;

            // Vector from start to circle center
            Vector2 f = start - center;

            // Quadratic equation coefficients: a*t^2 + b*t + c = 0
            float a = Vector2.Dot(d, d);
            float b = 2 * Vector2.Dot(f, d);
            float c = Vector2.Dot(f, f) - radius * radius;

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                // No intersection with the infinite line
                return false;
            }

            // Find the closest point on the line segment to the circle center
            float t = -b / (2 * a);

            if (t < 0)
            {
                // Closest point is before the start of the segment
                return Vector2.Distance(start, center) <= radius;
            }
            else if (t > 1)
            {
                // Closest point is after the end of the segment
                return Vector2.Distance(end, center) <= radius;
            }
            else
            {
                // Closest point is on the segment
                Vector2 closestPoint = start + t * d;
                return Vector2.Distance(closestPoint, center) <= radius;
            }
        }


        // Helper method to check if two line segments intersect
        private static bool DoLinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            // Calculate the direction vectors
            Vector2 d1 = p2 - p1;
            Vector2 d2 = p4 - p3;

            // Calculate the cross products
            float cross1 = d1.X * d2.Y - d1.Y * d2.X;
            float cross2 = (p3 - p1).X * d2.Y - (p3 - p1).Y * d2.X;

            if (Math.Abs(cross1) < float.Epsilon)
            {
                // Lines are parallel - check if they're collinear and overlapping
                if (Math.Abs(cross2) < float.Epsilon)
                {
                    // Check if the segments overlap
                    float t1 = 0, t2 = 0;
                    if (Math.Abs(d1.X) > float.Epsilon)
                    {
                        t1 = (p3.X - p1.X) / d1.X;
                        t2 = (p4.X - p1.X) / d1.X;
                    }
                    else if (Math.Abs(d1.Y) > float.Epsilon)
                    {
                        t1 = (p3.Y - p1.Y) / d1.Y;
                        t2 = (p4.Y - p1.Y) / d1.Y;
                    }
                    else
                    {
                        return false; // Degenerate line
                    }

                    // Check if segments overlap
                    float minT = Math.Min(t1, t2);
                    float maxT = Math.Max(t1, t2);
                    return minT <= 1 && maxT >= 0;
                }
                return false;
            }

            // Check if the intersection point is within both segments
            float t = cross2 / cross1;
            float u = ((p3 - p1).X * d1.Y - (p3 - p1).Y * d1.X) / cross1;

            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }




        public static double EaseInOut(double t)
        {
            // Ease In-Out (Smooth start and stop)
            // Starts slow => speeds up => slows down near the end.

            return t * t * (3 - 2 * t); // smoothstep
        }


        public static double get_angle_ABX(PointF A_pt, PointF B_pt, bool is_deg = false)
        {
            // Angle made with X -axis (assuming pt B lies on x_axis)
            double delta_X, delta_Y;
            delta_X = Math.Abs(B_pt.X - A_pt.X);
            delta_Y = Math.Abs(B_pt.Y - A_pt.Y);

            // Angle with x-line
            double angle_with_xaxis;
            angle_with_xaxis = Math.Atan2(delta_Y, delta_X);

            if (B_pt.Y < A_pt.Y) // Second point is lower than first, angle goes down (180-360)
            {
                if (B_pt.X < A_pt.X) // Second point is to the left of first (180-270)
                {
                    angle_with_xaxis = angle_with_xaxis + Math.PI; // 180 degree to 270 degree
                }
                else // Angle range (270 to 360)
                {
                    angle_with_xaxis = (2 * Math.PI) - angle_with_xaxis;  // 270 degree to 360 degree
                }
            }
            else if (B_pt.X < A_pt.X) //Second point is top left of first (90-180)
            {
                angle_with_xaxis = Math.PI - angle_with_xaxis; // 90 degree to 180 degree
            }

            if (is_deg == true)
                angle_with_xaxis = angle_with_xaxis * (180 / Math.PI);

            return angle_with_xaxis;
        }

        public static int ordered_orientation(PointF p, PointF q, PointF r)
        {
            // To find orientation of ordered triplet (p, q, r).
            // The function returns following values
            // 0 --> p, q and r are collinear
            // 1 -->  Clockwise
            // -1 --> Counter clockwise

            double val = (((q.Y - p.Y) * (r.X - q.X)) - ((q.X - p.X) * (r.Y - q.Y)));

            if (Math.Round(val, 3) == 0) return 0; // collinear

            return (val > 0) ? 1 : -1; // clock or counterclock wise
        }

        public static double angle_between_2lines(PointF line1_pt1, PointF line1_pt2, PointF line2_pt1, PointF line2_pt2, bool to_deg = false)
        {
            double dx1, dy1;
            double norm;
            dx1 = line1_pt2.X - line1_pt1.X;
            dy1 = line1_pt2.Y - line1_pt1.Y;
            norm = Math.Sqrt((dx1 * dx1) + (dy1 * dy1));
            // vector 1
            dx1 = dx1 / norm;
            dy1 = dy1 / norm;

            double dx2, dy2;
            dx2 = line2_pt2.X - line2_pt1.X;
            dy2 = line2_pt2.Y - line2_pt1.Y;
            norm = Math.Sqrt((dx2 * dx2) + (dy2 * dy2));
            // vector 2
            dx2 = dx2 / norm;
            dy2 = dy2 / norm;

            // Dot product
            double angle_in_rad = Math.Acos((dx1 * dx2) + (dy1 * dy2));

            if (to_deg == true)
                angle_in_rad = angle_in_rad * (180 / Math.PI);

            return angle_in_rad;
        }

        public static Tuple<double, double> get_arc_angles(PointF chord_start_pt, PointF chord_end_pt, PointF pt_on_arc, PointF arc_center_pt)
        {
            // Start and sweep angle
            double start_angle, sweep_angle;
            if (ordered_orientation(chord_start_pt, chord_end_pt, pt_on_arc) > 0)
            {
                // Counter clockwise in screen co-ordinates
                start_angle = -1.0 * (360 - get_angle_ABX(arc_center_pt, chord_start_pt, true));
                sweep_angle = (angle_between_2lines(arc_center_pt, chord_start_pt, arc_center_pt, chord_end_pt, true));

                if (ordered_orientation(chord_start_pt, chord_end_pt, arc_center_pt) > 0)
                {
                    sweep_angle = 360 - sweep_angle;
                }
            }
            else
            {
                // Clockwise in screen co-ordinates
                start_angle = -1.0 * (360 - get_angle_ABX(arc_center_pt, chord_end_pt, true));
                sweep_angle = (angle_between_2lines(arc_center_pt, chord_end_pt, arc_center_pt, chord_start_pt, true));

                if (ordered_orientation(chord_end_pt, chord_start_pt, arc_center_pt) > 0)
                {
                    sweep_angle = 360 - sweep_angle;
                }
            }

            return new Tuple<double, double>(start_angle, sweep_angle);
        }
    }
}
