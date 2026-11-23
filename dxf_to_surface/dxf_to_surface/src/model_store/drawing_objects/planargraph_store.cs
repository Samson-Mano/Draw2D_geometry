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





    }


}
