using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;


namespace dxf_to_surface.src.model_store.drawing_objects
{
    public class planargraph_store
    {
        public const double PT_TOL = 1e-6;

        public class node_point_store
        {
            public int pointId;
            public Vector2 Point;
            public List<halfedge_store> HalfEdges = new List<halfedge_store>();

            public node_point_store(int id, Vector2 p) { pointId = id; Point = p; }
        }



        public Dictionary<int, node_point_store> NodePoints { get; private set; } = new Dictionary<int, node_point_store>();


        private Dictionary<(long, long), List<int>> _buckets = new Dictionary<(long, long), List<int>>();
        private int _nextPointId = 0;

        public List<halfedge_store> HalfEdges { get; set; } = new List<halfedge_store>();
        public List<region_store> Regions { get; set; } = new List<region_store>();

        public planargraph_store(
            linelist_store line_data,
            circlelist_store circle_data,
            ellipselist_store ellipse_data,
            splinelist_store spline_data)
        {
            // ---- Lines ----
            foreach (var line in line_data.lineMap.Values)
            {
                AddHalfEdge(new halfedge_store
                {
                    Kind = PrimitiveKind.Line,
                    Line = line,
                    StartParam = 0.0,
                    EndParam = 1.0,
                    Reversed = false,
                    StartPoint = line.lineStartPt,
                    EndPoint = line.lineEndPt
                });
            }

            // ---- Circles ----
            foreach (var c in circle_data.circleMap.Values)
            {
                if (c.type == 0)   // full circle: isolated until split; skip node attach
                {
                    AddHalfEdge(new halfedge_store
                    {
                        Kind = PrimitiveKind.Circle,
                        Circle = c,
                        StartParam = 0.0,
                        EndParam = 2 * Math.PI,
                        Reversed = false,
                        StartPoint = c.GetCirclePoint(0.0),
                        EndPoint = c.GetCirclePoint(2 * Math.PI),
                        startPtId = -1,
                        endPtId = -1
                    }, attachNodes: false);
                }
                else               // arc
                {
                    AddHalfEdge(new halfedge_store
                    {
                        Kind = PrimitiveKind.Circle,
                        Circle = c,
                        StartParam = c.startAngle,
                        EndParam = c.endAngle,
                        Reversed = false,
                        StartPoint = c.arcStartPt,
                        EndPoint = c.arcEndPt
                    });
                }
            }

            // ---- Ellipses ----
            foreach (var e in ellipse_data.ellipseMap.Values)
            {
                if (e.type == 0)
                {
                    AddHalfEdge(new halfedge_store
                    {
                        Kind = PrimitiveKind.Ellipse,
                        Ellipse = e,
                        StartParam = 0.0,
                        EndParam = 2 * Math.PI,
                        Reversed = false,
                        StartPoint = e.GetEllipsePoint(0.0),
                        EndPoint = e.GetEllipsePoint(2 * Math.PI),
                        startPtId = -1,
                        endPtId = -1
                    }, attachNodes: false);
                }
                else
                {
                    AddHalfEdge(new halfedge_store
                    {
                        Kind = PrimitiveKind.Ellipse,
                        Ellipse = e,
                        StartParam = e.startParameter,
                        EndParam = e.endParameter,
                        Reversed = false,
                        StartPoint = e.arcStartPt,
                        EndPoint = e.arcEndPt
                    });
                }
            }

            // ---- Splines ----
            foreach (var s in spline_data.splineMap.Values)
            {
                AddHalfEdge(new halfedge_store
                {
                    Kind = PrimitiveKind.Spline,
                    Spline = s,
                    StartParam = s.knots[s.degree],
                    EndParam = s.knots[s.controlPoints.Count],
                    Reversed = false,
                    StartPoint = s.GetSplinePoint(s.knots[s.degree]),
                    EndPoint = s.GetSplinePoint(s.knots[s.controlPoints.Count])
                });
            }


            // Create regions from half-edges
            Regions = new List<region_store>();

            // Create regions from half-edges for full circles and ellipses (type=0) that have no start/end points
            // and splines that are closed (start/end points coincide)
            AddClosedPrimitiveLoop(HalfEdges);


            // Create regions from half-edges for closed loops of half-edges (start/end points coincide)
            AddClosedHalfEdgesLoop(HalfEdges);



        }


        private void AddHalfEdge(halfedge_store he, bool attachNodes = true)
        {
            he.HalfEdgeId = HalfEdges.Count;
            HalfEdges.Add(he);

            if (!attachNodes)
            {
                // Exit for full circles and ellipses (type=0) that have no start/end points
                return;
            }


            if (he.StartPoint == null || he.EndPoint == null) return; // safety

            var vStart = GetOrCreateNode(he.StartPoint);
            var vEnd = GetOrCreateNode(he.EndPoint);

            he.startPtId = vStart.pointId;
            he.endPtId = vEnd.pointId;

            vStart.HalfEdges.Add(he);  // outgoing from start
                                       // Do NOT add to vEnd.HalfEdges here: those are "outgoing" lists.
                                       // The twin (added later) will populate vEnd.HalfEdges when created.
        }

        private node_point_store GetOrCreateNode(Vector2 p)
        {
            long bx = (long)Math.Round(p.X / PT_TOL);
            long by = (long)Math.Round(p.Y / PT_TOL);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var key = (bx + dx, by + dy);
                    if (!_buckets.TryGetValue(key, out var list)) continue;
                    foreach (var id in list)
                    {
                        var n = NodePoints[id];
                        if ((n.Point - p).Length <= PT_TOL) return n;
                    }
                }
            }


            var node = new node_point_store(_nextPointId++, p);
            NodePoints[node.pointId] = node;

            if (!_buckets.TryGetValue((bx, by), out var b))
            {
                _buckets[(bx, by)] = b = new List<int>();
            }

            b.Add(node.pointId);
            return node;
        }


        private bool isIsolatedClosedPrimitive(halfedge_store he)
        {
            bool isFullCircle = he.Kind == PrimitiveKind.Circle
                                && he.Circle != null && he.Circle.type == 0;

            bool isFullEllipse = he.Kind == PrimitiveKind.Ellipse
                              && he.Ellipse != null && he.Ellipse.type == 0;

            // Spline: snapped to a single node (startPtId == endPtId, both valid) AND
            // geometrically closed within tolerance. The startPtId check is redundant if
            // startPtId == endPtId, but keep it for clarity.
            bool isClosedSpline = he.Kind == PrimitiveKind.Spline
                               && he.startPtId >= 0
                               && he.startPtId == he.endPtId
                               && (he.StartPoint - he.EndPoint).Length <= PT_TOL;

            if (!(isFullCircle || isFullEllipse || isClosedSpline))
                return false;

            return true;
        }


        private void AddClosedPrimitiveLoop(List<halfedge_store> halfEdges)
        {

            foreach (var he in halfEdges)
            {
                // "Isolated closed primitive": no node attachment (sentinel startPtId < 0)
                // and it is a closed curve kind.
                bool isIsolatedClosed = isIsolatedClosedPrimitive(he);

                if (!isIsolatedClosed) continue;

                // Enforce CCW orientation for the outer ring.
                // If the primitive traverses CW in world space, build a Reversed twin and use it.
                halfedge_store ringHe = he;
                if (SignedAreaOfClosedHalfEdge(he) < 0)
                    ringHe = MakeReversed(he);

                var region = new region_store
                {
                    RegionId = Regions.Count
                };
                region.AddOuterRing(new List<halfedge_store> { ringHe });
                Regions.Add(region);
            }
        }


        // Numerically integrate 1/2 ∫ (x dy - y dx) around the closed half-edge.
        // Positive = CCW.
        private static double SignedAreaOfClosedHalfEdge(halfedge_store he, int n = 64)
        {
            double A = 0.0;
            Vector2 prev = he.PointAt(0.0);
            for (int i = 1; i <= n; i++)
            {
                Vector2 cur = he.PointAt((double)i / n);
                A += prev.X * cur.Y - cur.X * prev.Y;
                prev = cur;
            }
            return 0.5 * A;
        }

        // Build a Reversed twin of a closed half-edge for outer-ring orientation fixes.
        private static halfedge_store MakeReversed(halfedge_store he)
        {
            return new halfedge_store
            {
                HalfEdgeId = he.HalfEdgeId,  // will be assigned when added to the graph
                Kind = he.Kind,
                Line = he.Line,
                Circle = he.Circle,
                Ellipse = he.Ellipse,
                Spline = he.Spline,
                startPtId = he.endPtId,
                endPtId = he.startPtId,
                StartParam = he.EndParam,
                EndParam = he.StartParam,
                Reversed = !he.Reversed,
                Length = he.Length,
                StartPoint = he.EndPoint,
                EndPoint = he.StartPoint,
                // Twin = null       // isolated, no twin
            };
        }


        private void AddClosedHalfEdgesLoop(List<halfedge_store> halfEdges)
        {

            // Copy the half-edges to a working list, and mark them as unvisited.
            Dictionary<int, halfedge_store> halfedge_dict = halfEdges.ToDictionary(he => he.HalfEdgeId, he => he);


            // Step 1: Clear the half edges which are already part of a region (i.e., have a LeftRegion assigned).
            // Primitives that are part of a region (e.g., isolated closed primitives) will be skipped in the next step.
            List<int> remove_id = new List<int>();

            foreach(var he in halfedge_dict.Values)
            {
                bool isIsolatedClosed = isIsolatedClosedPrimitive(he);

                if (isIsolatedClosed)
                {
                    // Already part of a region, skip it.
                    remove_id.Add(he.HalfEdgeId);
                }
            }

            foreach (int r_id in remove_id)
            {
                halfedge_dict.Remove(r_id);
            }

            // Step 2: Remove the open half-edges from the working list.
            bool isOpenHEFound = false;
       
            do
            {
                isOpenHEFound = false;
                remove_id.Clear();

                foreach (halfedge_store he in halfedge_dict.Values)
                {
                    // Create a subset of half-edges other than the current half-edge to search.
                    Dictionary<int, halfedge_store> otherHalfEdges = new Dictionary<int, halfedge_store>(halfedge_dict);
                    otherHalfEdges.Remove(he.HalfEdgeId);

                    int s_id_count = 0, e_id_count = 0;

                    foreach (halfedge_store other_he in otherHalfEdges.Values)
                    {
                        if (he.startPtId == other_he.startPtId || he.startPtId == other_he.endPtId)
                        {
                            s_id_count++;
                        }

                        if (he.endPtId == other_he.endPtId || he.endPtId == other_he.startPtId)
                        {
                            e_id_count++;
                        }
                    }

                    if (s_id_count == 0 || e_id_count == 0)
                    {
                        isOpenHEFound = true;
                        remove_id.Add(he.HalfEdgeId);
                    }
                }


                foreach (int r_id in remove_id)
                {
                    halfedge_dict.Remove(r_id);
                }


            } while (isOpenHEFound == true);


            // Step 3: Build regions from the remaining closed half-edges.








        }






    }


}
