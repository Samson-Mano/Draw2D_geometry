using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store.geom_objects;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace dxf_to_surface.src.model_store.drawing_objects
{

    public class spline_store
    {
        public int spline_id;
        public int degree;
        public List<Vector2> controlPoints;
        public List<double> weights;     // 1.0 for non-rational
        public List<double> knots;       // length == controlPoints.Count + degree + 1
        public bool isClosed;
        public bool isPeriodic;
        public bool isRational;

        // fit-point splines (LibreCAD "spline through points"):
        // set controlPoints/weights/knots by *interpolation* at import, OR
        // keep fitPoints and interpolate lazily. Prefer the former.
        public List<Vector2> fitPoints;  // informational / fallback


        public double[] ArcLengthTableParam;   // null until built
        public double[] ArcLengthTableS;       // null until built




        // Evaluate this spline's homogeneous coords at u (clamps/wraps u first).
        private void EvalHomogeneous(double u, out double xw, out double yw, out double w)
        {
            int p = degree;
            int n = controlPoints.Count - 1;

            double uc = ClampParam(u);

            // Pack control points + weights into homogeneous arrays.
            int count = controlPoints.Count;
            var dx = new double[count];
            var dy = new double[count];
            var dw = new double[count];
            for (int i = 0; i < count; i++)
            {
                double wi = isRational ? weights[i] : 1.0;
                dx[i] = controlPoints[i].X * wi;
                dy[i] = controlPoints[i].Y * wi;
                dw[i] = wi;
            }

            DeBoorCore(p, dx, dy, dw, knots.ToArray(), n, uc, out xw, out yw, out w);
        }


        // Evaluate an arbitrary homogeneous NURBS at u. `u` must already be in
        // the domain of `knots` — callers are responsible for clamping/wrapping.
        private void EvalHomogeneousArbitrary(
            int p, double[] dx, double[] dy, double[] dw,
            double[] knots, int n, double u,
            out double xw, out double yw, out double w)
        {
            DeBoorCore(p, dx, dy, dw, knots, n, u, out xw, out yw, out w);
        }



        // Pure De Boor recursion on the arrays. No `this` access.
        private static void DeBoorCore(
            int p, double[] dx, double[] dy, double[] dw,
            double[] knots, int n, double u,
            out double xw, out double yw, out double w)
        {
            int k;
            if (u >= knots[n + 1]) k = n;
            else
            {
                int lo = p, hi = n + 1;
                while (hi - lo > 1)
                {
                    int mid = (lo + hi) >> 1;
                    if (u < knots[mid]) hi = mid; else lo = mid;
                }
                k = lo;
            }

            int m = p + 1;
            var bx = new double[m];
            var by = new double[m];
            var bw = new double[m];
            for (int j = 0; j < m; j++)
            {
                int idx = k - p + j;
                bx[j] = dx[idx];
                by[j] = dy[idx];
                bw[j] = dw[idx];
            }

            for (int r = 1; r <= p; r++)
            {
                for (int j = p; j >= r; j--)
                {
                    int i = j + k - p;
                    double denom = knots[i + p - r + 1] - knots[i];
                    double alpha = denom == 0.0 ? 0.0 : (u - knots[i]) / denom;
                    bx[j] = (1.0 - alpha) * bx[j - 1] + alpha * bx[j];
                    by[j] = (1.0 - alpha) * by[j - 1] + alpha * by[j];
                    bw[j] = (1.0 - alpha) * bw[j - 1] + alpha * bw[j];
                }
            }
            xw = bx[p]; yw = by[p]; w = bw[p];
        }



        // First derivative of the NURBS at u, in double.
        private void GetSplineDerivativeD(double u, out double dx, out double dy)
        {
            int p = degree;
            if (p == 0) { dx = dy = 0; return; }

            // Point and its homogeneous evaluation at u (needed for the quotient rule).
            EvalHomogeneous(u, out double xw, out double yw, out double w);

            // Build the derivative NURBS in homogeneous coords, degree p-1.
            int n = controlPoints.Count - 1;
            var knots = this.knots;

            // Derivative control points: Q_i = p * (Pw_{i+1} - Pw_i) / (U_{i+p+1} - U_{i+1})
            // where Pw_i = w_i * P_i (homogeneous).
            int dCount = n;   // one fewer control point
            var qx = new double[dCount];
            var qy = new double[dCount];
            var qw = new double[dCount];
            for (int i = 0; i < dCount; i++)
            {
                double wi = isRational ? weights[i] : 1.0;
                double wi1 = isRational ? weights[i + 1] : 1.0;
                double denom = knots[i + p + 1] - knots[i + 1];
                if (Math.Abs(denom) < 1e-12) { qx[i] = qy[i] = 0; qw[i] = 0; continue; }
                qx[i] = p * (controlPoints[i + 1].X * wi1 - controlPoints[i].X * wi) / denom;
                qy[i] = p * (controlPoints[i + 1].Y * wi1 - controlPoints[i].Y * wi) / denom;
                qw[i] = p * (wi1 - wi) / denom;
            }

            // Derivative knot vector: drop first and last.
            var dKnots = new double[knots.Count - 2];
            for (int i = 0; i < dKnots.Length; i++) dKnots[i] = knots[i + 1];

            // Evaluate the derivative NURBS at u. This is a De Boor call with:
            //   degree = p-1, controlPoints = (qx, qy, qw), knots = dKnots.
            double uClamped = ClampParam(u);
            EvalHomogeneousArbitrary(p - 1, qx, qy, qw, dKnots, qx.Length - 1, uClamped,
                      out double xwPrime, out double ywPrime, out double wPrime);

            // Rational quotient rule:
            //   d/du (xw/w) = (xw' * w - xw * w') / w^2
            //   d/du (yw/w) = (yw' * w - yw * w') / w^2
            if (isRational && Math.Abs(w) > 1e-12)
            {
                double w2 = w * w;
                dx = (xwPrime * w - xw * wPrime) / w2;
                dy = (ywPrime * w - yw * wPrime) / w2;
            }
            else
            {
                dx = xwPrime;
                dy = ywPrime;
            }
        }


        /// <summary>
        /// Clamps or wraps u into the valid parameter domain of this spline.
        /// For a clamped (non-periodic) spline, the domain is [knots[p], knots[n+1]];
        /// For a periodic spline, u is wrapped into that same interval by modular
        /// arithmetic, so the returned value is always in [knots[p], knots[n+1]).
        /// </summary>
        private double ClampParam(double u)
        {
            int p = degree;
            int n = controlPoints.Count - 1;
            double uMin = knots[p];
            double uMax = knots[n + 1];

            if (isPeriodic)
            {
                double span = uMax - uMin;
                if (span <= 0.0)
                    return uMin;                       // degenerate periodic: collapse to uMin
                                                       // Wrap into [uMin, uMin + span). The double-mod handles negative u.
                return uMin + ((u - uMin) % span + span) % span;
            }

            if (u < uMin) return uMin;
            if (u > uMax) return uMax;
            return u;
        }

        private void GetSplinePointD(double u, out double x, out double y)
        {
            EvalHomogeneous(u, out double xw, out double yw, out double w);
            if (isRational && Math.Abs(w) > 1e-12) { x = xw / w; y = yw / w; }
            else { x = xw; y = yw; }
        }


        public void GetSplineSecondDerivativeD(double u, out double ddx, out double ddy)
        {
            double h = 1e-4 * (knots[controlPoints.Count - 1] - knots[degree]);
            if (h < 1e-10) h = 1e-10;

            GetSplineDerivativeD(u + h, out double dxp, out double dyp);
            GetSplineDerivativeD(u - h, out double dxm, out double dym);
            ddx = (dxp - dxm) / (2 * h);
            ddy = (dyp - dym) / (2 * h);
        }


        public Vector2 GetSplinePoint(double u)
        {
            GetSplinePointD(u, out double x, out double y);
            return new Vector2((float)x, (float)y);
        }


                        

        public Vector2 GetSplineDerivative(double u)
        {
            // Evaluate the first derivative of the spline at parameter u
            // using De Boor's algorithm for derivatives
            GetSplineDerivativeD(u, out double dx, out double dy);
            return new Vector2((float)dx, (float)dy);
        }


        public Vector2 GetSplineSecondDerivative(double u)
        {
            // Evaluate the second derivative of the spline at parameter u
            // using De Boor's algorithm for second derivatives
            GetSplineSecondDerivativeD(u, out double ddx, out double ddy);
            return new Vector2((float)ddx, (float)ddy); 
        }



        public (double[] Params, double[] S) BuildArcLengthTable(int samples = 64)
        {
            double uMin = knots[degree];
            double uMax = knots[controlPoints.Count - 1 + 1];   // knots[n+1]
            if (isPeriodic)
            {
                // For a periodic spline, the domain is one period; the table covers it.
                uMax = knots[controlPoints.Count];
            }

            if (uMax <= uMin) { ArcLengthTableParam = new[] { uMin }; ArcLengthTableS = new[] { 0.0 }; return (ArcLengthTableParam, ArcLengthTableS); }

            var ps = new double[samples + 1];
            var ss = new double[samples + 1];
            ps[0] = uMin; ss[0] = 0.0;
            double du = (uMax - uMin) / samples;

            for (int i = 1; i <= samples; i++)
            {
                double u0 = uMin + (i - 1) * du;
                double u1 = uMin + i * du;
                ps[i] = u1;

                // 4-point Gauss-Legendre on [u0, u1] for ∫|dP/du| du.
                double half = 0.5 * (u1 - u0), mid = 0.5 * (u1 + u0);
                double[] gx = { -0.861136311594053, -0.339981043584856, 0.339981043584856, 0.861136311594053 };
                double[] gw = { 0.347854845137454, 0.652145154862546, 0.652145154862546, 0.347854845137454 };
                double sum = 0.0;
                for (int k = 0; k < 4; k++)
                {
                    double u = mid + half * gx[k];
                    GetSplineDerivativeD(u, out double dx, out double dy);
                    sum += gw[k] * Math.Sqrt(dx * dx + dy * dy);
                }
                ss[i] = ss[i - 1] + half * sum;
            }
            ArcLengthTableParam = ps;
            ArcLengthTableS = ss;
            return (ps, ss);
        }

        public double GetArcLength()
        {
            if (ArcLengthTableS == null || ArcLengthTableS.Length == 0) BuildArcLengthTable();
            return ArcLengthTableS[ArcLengthTableS.Length - 1];
        }


    }


    public class splinelist_store
    {

        public Dictionary<int, spline_store> splineMap { get; } = new Dictionary<int, spline_store>();
        private int splineCount = 0;

        private const int SPLINE_VERTEX_COUNT = 200; // Number of vertices to approximate a spline

        // Rendering resources
        private VertexArray _splineVAO;
        private VertexBuffer _splineVBO;
        private IndexBuffer _splineIBO;

        private Shader _splineShader;


        public splinelist_store()
        {
            splineMap = new Dictionary<int, spline_store>();
            splineCount = 0;

            InitializeShader();
            InitializeBuffers();

        }


        public void add_spline(int splineID, int degree, List<Vector2> controlPoints, List<double> weights,
            List<double> knots, bool isClosed, bool isPeriodic, bool isRational, List<Vector2> fitPoints)
        {
            var spline = new spline_store
            {
                spline_id = splineID,
                degree = degree,
                controlPoints = controlPoints,
                weights = weights,
                knots = knots,
                isClosed = isClosed,
                isPeriodic = isPeriodic,
                isRational = isRational,
                fitPoints = fitPoints
            };

            splineMap.Add(splineID, spline);
        }



        public void remove_spline(int splineID)
        {
            if (splineMap.ContainsKey(splineID))
            {
                splineMap.Remove(splineID);
            }
        }



        private void InitializeShader()
        {
            // Create Shader
            _splineShader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.LineShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.LineShader));
        }



        private void InitializeBuffers()
        {
            // Label buffer
            _splineVAO = new VertexArray();
            _splineVBO = new VertexBuffer(10);
            _splineIBO = new IndexBuffer(10);

            var splineLayout = new VertexBufferLayout();
            splineLayout.AddFloat(2);  // Spline vertex position (x, y)

            _splineVAO.Add_vertexBuffer(_splineVBO, splineLayout);
        }


        public void UpdateBuffers()
        {
            // Set the buffer for index (2 indices per line)
            List<int> splinelinesegment_indices = new List<int>();

            // Set the line index buffers
            int t_id = 0;
            foreach (var spline in splineMap)
            {
                get_spline_index_buffer(spline.Value, ref splinelinesegment_indices, ref t_id);
            }

            // Update the index buffer
            _splineIBO.ClearIndexBuffer();
            _splineIBO.AppendIndexBuffer(splinelinesegment_indices.ToArray());


            // Define the vertex buffer size for a character
            List<float> splinelinesegment_vertices = new List<float>();

            // Set the line vertex buffers
            foreach (var spline in splineMap)
            {
                // Add vertex buffers
                get_spline_vertex_buffer(spline.Value, ref splinelinesegment_vertices);
            }


            // Update the vertex buffer
            _splineVBO.ClearVertexBuffer();
            _splineVBO.AppendVertexBuffer(splinelinesegment_vertices.ToArray());

        }


        public void clear_splines()
        {
            // clear the splineMap and reset the count
            splineMap.Clear();
            splineCount = 0;

            // clear the buffers
            _splineVBO.ClearVertexBuffer();
            _splineIBO.ClearIndexBuffer();

        }

        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                            graphic_events_control.viewMatrix *
                            graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;


            // Update the openGl uniform matrices
            _splineShader.SetMatrix4("uMVP", uMVP);

            Vector3 splineColor = gvariables_static.ColorUtils.get_SplineColor();
            Vector4 splineVertexColor = new Vector4(splineColor.X, splineColor.Y, splineColor.Z, gvariables_static.geom_transparency);

            _splineShader.SetVector4("lineVertexColor", splineVertexColor);

        }

        public void paint_splines()
        {
            // Bind the shader
            _splineShader.Bind();
            _splineVAO.Bind();
            _splineIBO.Bind();

            GL.LineWidth(gvariables_static.LineWidth); // Set the line width

            // Draw the lines
            GL.DrawElements(PrimitiveType.Lines, _splineIBO.BufferCount, DrawElementsType.UnsignedInt, 0);

            _splineShader.UnBind();
            _splineVAO.UnBind();
            _splineIBO.UnBind();

        }


        //// Evaluate a NURBS curve at parameter u using De Boor's algorithm.
        //// Returns the 2D point. If rational (weights != null and isRational),
        //// the rational form is used; otherwise homogeneous w=1 is used.
        //private static Vector2 eval_spline(spline_store s, double u)
        //{
        //    int p = s.degree;
        //    int n = s.controlPoints.Count - 1;   // highest control point index
        //    var knots = s.knots;

        //    // Clamp u into the valid domain. For a clamped spline the valid domain is
        //    // [knots[p], knots[n+1]]. For periodic splines we wrap into the same range.
        //    double uMin = knots[p];
        //    double uMax = knots[n + 1];

        //    if (s.isPeriodic)
        //    {
        //        double span = uMax - uMin;
        //        if (span <= 0) return s.controlPoints[0];
        //        u = uMin + ((u - uMin) % span + span) % span;
        //    }
        //    else
        //    {
        //        if (u < uMin) u = uMin;
        //        if (u > uMax) u = uMax;
        //    }

        //    // Find knot span k such that knots[k] <= u < knots[k+1]
        //    // (with the standard special case at the right end: k = n).
        //    int k = p;
        //    if (u >= uMax) k = n;
        //    else
        //    {
        //        // binary search for speed; linear is fine for small control counts
        //        int lo = p, hi = n + 1;
        //        while (hi - lo > 1)
        //        {
        //            int mid = (lo + hi) >> 1;
        //            if (u < knots[mid]) hi = mid; else lo = mid;
        //        }
        //        k = lo;
        //    }

        //    // Copy the p+1 control points (homogeneous) into d[]
        //    int m = p + 1;
        //    var dx = new double[m];
        //    var dy = new double[m];
        //    var dw = new double[m];

        //    for (int j = 0; j < m; j++)
        //    {
        //        int idx = k - p + j;
        //        double w = s.isRational ? s.weights[idx] : 1.0;
        //        dx[j] = s.controlPoints[idx].X * w;
        //        dy[j] = s.controlPoints[idx].Y * w;
        //        dw[j] = w;
        //    }

        //    // De Boor recursion
        //    for (int r = 1; r <= p; r++)
        //    {
        //        for (int j = p; j >= r; j--)
        //        {
        //            int i = j + k - p;
        //            double denom = knots[i + p - r + 1] - knots[i];
        //            double alpha = denom == 0.0 ? 0.0 : (u - knots[i]) / denom;

        //            dx[j] = (1.0 - alpha) * dx[j - 1] + alpha * dx[j];
        //            dy[j] = (1.0 - alpha) * dy[j - 1] + alpha * dy[j];
        //            dw[j] = (1.0 - alpha) * dw[j - 1] + alpha * dw[j];
        //        }
        //    }

        //    double wf = dw[p];
        //    if (s.isRational && Math.Abs(wf) > 1e-12)
        //        return new Vector2((float)(dx[p] / wf), (float)(dy[p] / wf));
        //    else
        //        return new Vector2((float)dx[p], (float)dy[p]);
        //}



        private void get_spline_vertex_buffer(spline_store spline, ref List<float> spline_vertices)
        {

            if (spline == null || spline.controlPoints == null || spline.controlPoints.Count == 0)
                return;
            if (spline.knots == null || spline.knots.Count != spline.controlPoints.Count + spline.degree + 1)
            {
                // Malformed spline — skip rather than emit garbage.
                return;
            }

            int p = spline.degree;
            int n = spline.controlPoints.Count - 1;
            double uMin = spline.knots[p];
            double uMax = spline.knots[n + 1];
            double span = uMax - uMin;

            if (span <= 0.0)
            {
                // Degenerate: emit a single point repeated (or just the point once).
                var pt = spline.controlPoints[0];
                spline_vertices.Add(pt.X);
                spline_vertices.Add(pt.Y);
                return;
            }

            // For a closed *non-periodic* spline (isClosed && !isPeriodic) the curve
            // still stops at uMax; the caller is responsible for closing the loop by
            // connecting the last vertex to the first. For periodic splines the seam
            // is already continuous, so we sample [uMin, uMax) to avoid duplicating.
            bool sampleHalfOpen = spline.isPeriodic;
            int count = SPLINE_VERTEX_COUNT;

            int endExclusive = sampleHalfOpen ? count : count; // same count either way
            for (int i = 0; i < endExclusive; i++)
            {
                double t = (double)i / (count - 1);     // 0..1 inclusive
                if (sampleHalfOpen && i == count - 1) t = 1.0 - 1e-9; // avoid exact seam dup
                double u = uMin + t * span;

                Vector2 pt = spline.GetSplinePoint(u);
                spline_vertices.Add(pt.X);
                spline_vertices.Add(pt.Y);
            }

        }


        private void get_spline_index_buffer(spline_store spline, ref List<int> spline_indices, ref int t_id)
        {
            for (int i = 0; i < SPLINE_VERTEX_COUNT - 1; i++)
            {
                spline_indices.Add(t_id + i);
                spline_indices.Add(t_id + (i + 1));

            }

            t_id += SPLINE_VERTEX_COUNT;

        }


        public void getModelScalingPoints(ref List<Vector2> scalingPoints)
        {
            List<float> spline_vertices = new List<float>();

            foreach (var spline in splineMap.Values)
            {
                get_spline_vertex_buffer(spline, ref spline_vertices);
            }

            for(int i = 0; i < spline_vertices.Count; i += 2)
            {
                scalingPoints.Add(new Vector2(spline_vertices[i], spline_vertices[i + 1]));
            }

        }


    }
}
