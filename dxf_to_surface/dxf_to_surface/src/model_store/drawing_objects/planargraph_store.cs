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
            public List<int> OutgoingHalfEdgesId = new List<int>();

            public node_point_store(int id, Vector2 p) { pointId = id; Point = p; }
        }



        public Dictionary<int, node_point_store> NodePoints { get; private set; } = new Dictionary<int, node_point_store>();


        private Dictionary<(long, long), List<int>> _buckets = new Dictionary<(long, long), List<int>>();
        private int _nextPointId = 0;

        public Dictionary<int, halfedge_store> HalfEdges { get; set; } = new Dictionary<int, halfedge_store>();
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
            this.Regions = new List<region_store>();

            // Create regions from half-edges for full circles and ellipses (type=0) that have no start/end points
            // and splines that are closed (start/end points coincide)
            AddClosedPrimitiveLoop();


            // Create regions from half-edges for closed loops of half-edges (start/end points coincide)
            AddClosedHalfEdgesLoop();

            // Nesting of regions
            PerformRegionNesting();

        }


        private void AddHalfEdge(halfedge_store he, bool attachNodes = true)
        {
            he.HalfEdgeId = this.HalfEdges.Count;
            he.TwinHalfEdgeId = this.HalfEdges.Count + 1; // will be set later when the twin is created

            halfedge_store twinhe = new halfedge_store
            {
                HalfEdgeId = he.TwinHalfEdgeId,
                TwinHalfEdgeId = he.HalfEdgeId,

                Kind = he.Kind,
                Line = he.Line,
                Circle = he.Circle,
                Ellipse = he.Ellipse,
                Spline = he.Spline,

                startPtId = -1,  // will be set later when the twin is created
                endPtId = -1,    // will be set later when the twin is created

                // Swap endpoints and parameters.
                StartParam = he.EndParam,
                EndParam = he.StartParam,
                Reversed = !he.Reversed,
                Length = he.Length,
                StartPoint = he.EndPoint,
                EndPoint = he.StartPoint
            };

            // Register the half-edge and its twin in the HalfEdges dictionary
            HalfEdges[he.HalfEdgeId] = he;
            HalfEdges[twinhe.HalfEdgeId] = twinhe;


            if (!attachNodes)
            {
                // Exit for full circles and ellipses (type=0) that have no start/end points
                return;
            }


            if (he.StartPoint == null || he.EndPoint == null) return; // safety

            var vStart = GetOrCreateNode(he.StartPoint);
            var vEnd = GetOrCreateNode(he.EndPoint);

            // Set the point ids for the half-edge and its twin
            he.startPtId = vStart.pointId;
            he.endPtId = vEnd.pointId;

            twinhe.startPtId = vEnd.pointId;
            twinhe.endPtId = vStart.pointId;


            // Each node records only its OUTGOING half-edges.
            vStart.OutgoingHalfEdgesId.Add(he.HalfEdgeId);  // outgoing from start
                                       // Do NOT add to vEnd.HalfEdges here: those are "outgoing" lists.
                                       // The twin (added later) will populate vEnd.HalfEdges when created.

            vEnd.OutgoingHalfEdgesId.Add(twinhe.HalfEdgeId);  // outgoing from end

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


            node_point_store node = new node_point_store(_nextPointId++, p);
            NodePoints[node.pointId] = node;

            if (!_buckets.TryGetValue((bx, by), out var b))
            {
                _buckets[(bx, by)] = b = new List<int>();
            }

            b.Add(node.pointId);
            return node;

            //foreach (var node in NodePoints.Values)
            //    if ((node.Point - p).Length() <= PT_TOL) return node;
            //var node = new node_point_store(_nextNodeId++, p);
            //NodePoints[node.pointId] = node;
            //return node;
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


        private void AddClosedPrimitiveLoop()
        {

            foreach (var he in this.HalfEdges.Values)
            {
                // "Isolated closed primitive": no node attachment (sentinel startPtId < 0)
                // and it is a closed curve kind.
                bool isIsolatedClosed = isIsolatedClosedPrimitive(he);

                if (!isIsolatedClosed) continue;

                // Enforce CCW orientation for the outer ring.
                // If the primitive traverses CW in world space, build a Reversed twin and use it.
                halfedge_store ringHe = he;

                if (SignedAreaOfClosedHalfEdge(he) < 0)
                {
                    ringHe = MakeReversed(he);
                }
                    

                var region = new region_store
                {
                    RegionId = Regions.Count
                };

                region.AddIsolatedClosedPrimitive(ringHe);
                Regions.Add(region);
            }
        }


        // Numerically integrate 1/2 ∫ (x dy - y dx) around the closed half-edge.
        // Positive = CCW.
        private static double SignedAreaOfClosedHalfEdge(halfedge_store he, int n = 256)
        {
            double A = 0.0;
            // Sample-based shoelace for curves.
            // Per-half-edge (not cross-half-edge) to avoid accumulating gap error.

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
                NextHalfEdgeId = -1,  // Not applicable for isolated closed primitives
                PrevHalfEdgeId = -1,  // Not applicable for isolated closed primitives
                TwinHalfEdgeId = -1  // Not applicable for isolated closed primitives
            };
        }


        private void AddClosedHalfEdgesLoop()
        {

            // Copy the half-edges to a working list, and mark them as unvisited.
            Dictionary<int, halfedge_store> halfedge_dict = new Dictionary<int, halfedge_store>(this.HalfEdges);


            // Step 1: Clear the half edges which are already part of a region (i.e., have a LeftRegion assigned).
            // Primitives that are part of a region (e.g., isolated closed primitives) will be skipped in the next step.
            List<int> remove_id = new List<int>();

            foreach (var he in halfedge_dict.Values)
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

            // Step 3A: Buld the Next and Prev links for each half-edge.
            // This is done by searching for the next half-edge that shares the same end point as the current half-edge's start point.
            foreach (halfedge_store he in halfedge_dict.Values)
            {
                he.setNextPrevLinks(halfedge_dict);
            }


            // Step 3B: Build the regions from the closed loops of half-edges.
            HashSet<int> faceVisited = new HashSet<int>();

            foreach (halfedge_store he in halfedge_dict.Values)
            {

                if (faceVisited.Contains(he.HalfEdgeId)) continue;

                // Walk the left face, collecting ids in order.
                List<int> ringIds = new List<int>();

                int cur = he.HalfEdgeId;
                bool closed = false;

                while (cur != -1)
                {
                    if (faceVisited.Add(cur) == false)
                    {
                        // If we return to the starting id, the face closed.
                        if (cur == he.HalfEdgeId) 
                        { 
                            closed = true; 
                        }
                        break;
                    }

                    ringIds.Add(cur);
                    cur = halfedge_dict[cur].NextHalfEdgeId;
                }

                if (closed == false || ringIds.Count == 0)
                {
                    // The face did not close properly, or there are no half-edges in the face. Skip this face.
                    continue;
                }
                    

                // Signed area: discard infinite face (negative or near-zero).
                double area = SignedAreaOfRing(halfedge_dict, ringIds);

                if (area <= 1e-12) continue;     // infinite face, or degenerate

                // Build the region (outer ring only for now).
                List<halfedge_store> loopHalfEdges = new List<halfedge_store>(ringIds.Count);
                foreach (int id in ringIds)
                {
                    loopHalfEdges.Add(halfedge_dict[id]);
                }
                
                region_store region = new region_store { RegionId = Regions.Count };
                region.AddOuterRing(loopHalfEdges);  
                Regions.Add(region);
            }


        }



        /// <summary>
        /// Signed area of a closed ring of half-edges, in world units squared.
        /// Positive for CCW (left-face) rings, negative for CW (right-face / infinite face) rings.
        /// Lines are computed exactly; curves are sampled at n segments per half-edge.
        /// </summary>
        private double SignedAreaOfRing(Dictionary<int, halfedge_store> halfedge_dict,
                                        List<int> ringIds,
                                        int n = 256)
        {
            if (ringIds == null || ringIds.Count == 0) return 0.0;
            if (n < 1) n = 1;

            double A = 0.0;

            foreach (int id in ringIds)
            {
                //if (!halfedge_dict.TryGetValue(id, out halfedge_store he))
                //    throw new KeyNotFoundException($"Half-edge id {id} not in halfedge_dict");

                halfedge_store he = halfedge_dict[id];


                if (he.Kind == PrimitiveKind.Line)
                {
                    // Exact shoelace for a straight segment.
                    Vector2 a = he.StartPoint;
                    Vector2 b = he.EndPoint;
                    A += a.X * b.Y - b.X * a.Y;
                    continue;
                }

                // Sample-based shoelace for curves.
                // Per-half-edge (not cross-half-edge) to avoid accumulating gap error.
                Vector2 prev = he.PointAt(0.0);

                //if (double.IsNaN(prev.X) || double.IsNaN(prev.Y))
                //    throw new InvalidOperationException($"NaN at PointAt(0) on half-edge {id}");

                for (int i = 1; i <= n; i++)
                {
                    Vector2 cur = he.PointAt((double)i / n);

                    //if (double.IsNaN(cur.X) || double.IsNaN(cur.Y))
                    //    throw new InvalidOperationException($"NaN at PointAt({i}/{n}) on half-edge {id}");

                    A += prev.X * cur.Y - cur.X * prev.Y;
                    prev = cur;
                }
            }

            return 0.5 * A;
        }



        private class face_store
        {
            public region_store Region;
            public double SignedArea;

            public List<Vector2> InteriorPts = new List<Vector2>()  ;  // a point inside the face, for nesting tests
        }


        private void PerformRegionNesting()
        {
            // 1. Compute area and interior points for each region
            List<face_store> faceList = new List<face_store>();

            foreach (region_store region in Regions)
            {
                double area = SignedAreaOfRing(region.OuterRing);
                face_store face = new face_store
                {
                    Region = region,
                    SignedArea = area
                };
                // Compute an interior point for the face (for nesting tests)
                Vector2 interiorPt = ComputeInteriorPoint(region);
                face.InteriorPts.Add(interiorPt);
                faceList.Add(face);
            }

            // 2. Sort descending by area (largest first).
            faceList.Sort((a, b) => Math.Abs(b.SignedArea).CompareTo(Math.Abs(a.SignedArea)));

            // 3. Find the smallest containing parent for each face.
            for (int i = 0; i < faceList.Count; i++)
            {
                faceList[i].Region.Parent = null;
                for (int j = 0; j < i; j++)
                {
                    if (PointInRing(faceList[i].InteriorPts[0], faceList[j].Region.OuterRing))
                    {
                        faceList[i].Region.Parent = faceList[j].Region;
                        break;
                    }
                }
                faceList[i].Region.Parent?.Children.Add(faceList[i].Region);
            }

            // 4. Assign holes: for each region at odd depth, its twin cycle is a hole of its parent.
            foreach (var r in Regions)
            {
                r.InnerRings = new List<List<halfedge_store>>();
            }

            foreach (var r in Regions)
            {
                if (Depth(r) % 2 == 1 && r.Parent != null)
                {
                    // r is a "hole" face — its twin boundary belongs to r.Parent's InnerRings.
                    var cwHole = r.OuterRing
                        .Select(he => this.HalfEdges[he.TwinHalfEdgeId])
                        .Reverse()
                        .ToList();
                    r.Parent.InnerRings.Add(cwHole);
                }
            }

        }



        private double SignedAreaOfRing(List<halfedge_store> halfedge_list, int n = 256)
        {
            if (halfedge_list == null || halfedge_list.Count == 0) return 0.0;
            if (n < 1) n = 1;

            double A = 0.0;

            foreach (halfedge_store he in halfedge_list)
            {
                //if (!halfedge_dict.TryGetValue(id, out halfedge_store he))
                //    throw new KeyNotFoundException($"Half-edge id {id} not in halfedge_dict");

                if (he.Kind == PrimitiveKind.Line)
                {
                    // Exact shoelace for a straight segment.
                    Vector2 a = he.StartPoint;
                    Vector2 b = he.EndPoint;
                    A += a.X * b.Y - b.X * a.Y;
                    continue;
                }

                // Sample-based shoelace for curves.
                // Per-half-edge (not cross-half-edge) to avoid accumulating gap error.
                Vector2 prev = he.PointAt(0.0);

                //if (double.IsNaN(prev.X) || double.IsNaN(prev.Y))
                //    throw new InvalidOperationException($"NaN at PointAt(0) on half-edge {id}");

                for (int i = 1; i <= n; i++)
                {
                    Vector2 cur = he.PointAt((double)i / n);

                    //if (double.IsNaN(cur.X) || double.IsNaN(cur.Y))
                    //    throw new InvalidOperationException($"NaN at PointAt({i}/{n}) on half-edge {id}");

                    A += prev.X * cur.Y - cur.X * prev.Y;
                    prev = cur;
                }
            }

            return 0.5 * A;
        }



    }


}
