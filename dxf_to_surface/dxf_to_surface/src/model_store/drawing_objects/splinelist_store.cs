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


        // Evaluate a NURBS curve at parameter u using De Boor's algorithm.
        // Returns the 2D point. If rational (weights != null and isRational),
        // the rational form is used; otherwise homogeneous w=1 is used.
        private static Vector2 eval_spline(spline_store s, double u)
        {
            int p = s.degree;
            int n = s.controlPoints.Count - 1;   // highest control point index
            var knots = s.knots;

            // Clamp u into the valid domain. For a clamped spline the valid domain is
            // [knots[p], knots[n+1]]. For periodic splines we wrap into the same range.
            double uMin = knots[p];
            double uMax = knots[n + 1];

            if (s.isPeriodic)
            {
                double span = uMax - uMin;
                if (span <= 0) return s.controlPoints[0];
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
                double w = s.isRational ? s.weights[idx] : 1.0;
                dx[j] = s.controlPoints[idx].X * w;
                dy[j] = s.controlPoints[idx].Y * w;
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
            if (s.isRational && Math.Abs(wf) > 1e-12)
                return new Vector2((float)(dx[p] / wf), (float)(dy[p] / wf));
            else
                return new Vector2((float)dx[p], (float)dy[p]);
        }



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

                Vector2 pt = eval_spline(spline, u);
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
