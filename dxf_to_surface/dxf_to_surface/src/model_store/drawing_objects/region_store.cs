using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public class region_store
    {

        public int RegionId;

        // Boundary: ordered CCW outer ring, and zero or more CW inner rings (holes)
        public List<halfedge_store> OuterRing;      // CCW, defines the region
        public List<int> OuterRingIds;
        public List<Vector2> OuterRingVertices;



        public List<List<halfedge_store>> InnerRings; // CW, each is a hole

        // Geometry back-reference: each HalfEdge carries a reference to the source primitive
        // (line_store, circle_store, ellipse_store, spline_store) and a parameter direction.

        // Nesting (derived, not manually set)
        public region_store Parent;                // null for top-level regions
        public List<region_store> Children;        // regions directly contained inside this one

        // Solver-facing properties
        public Dictionary<string, object> Properties; // material, BC, etc.

       


        public void AddIsolatedClosedPrimitive(halfedge_store halfEdge)
        {
            // Add the Isolated Closed Primitive as the Outer Ring of the Region
            // Circle, Ellipse, or Spline primitives are represented as a
            // single HalfEdge in the Outer Ring of the Region

            OuterRing = new List<halfedge_store> { halfEdge };
            OuterRingIds = new List<int> { halfEdge.HalfEdgeId };

            OuterRingVertices = new List<Vector2>(); // Empty list, as the vertices will be derived from the primitive's geometry
        }



        public void AddOuterRing(List<halfedge_store> halfEdges)
        {
            OuterRing = halfEdges;
            OuterRingIds = new List<int>();

            // Set the Outer Ring Vertices based on the start vertices of the half-edges
            OuterRingVertices = new List<Vector2>();

            for(int i = 0; i < halfEdges.Count; i++)
            {
                OuterRingIds.Add(halfEdges[i].HalfEdgeId);
                OuterRingVertices.Add(halfEdges[i].StartPoint);

                halfedge_store he = halfEdges[i];
                he.LeftRegion = this; // Set the LeftRegion of each half-edge to this region
            }

            // Close the polygon by adding the start point of the first half-edge to the end of the list
            OuterRingVertices.Add(halfEdges[0].StartPoint);


        }



        /// <summary>
        /// Signed area of a closed ring of half-edges, in world units squared.
        /// Positive for CCW (left-face) rings, negative for CW (right-face / infinite face) rings.
        /// Lines are computed exactly; curves are sampled at n segments per half-edge.
        /// </summary>
        public double SignedAreaOfRegion(int n = 256)
        {
            if (OuterRingIds == null || OuterRingIds.Count == 0) return 0.0;
            if (n < 1) n = 1;

            double A = 0.0;

            foreach (halfedge_store he in OuterRing)
            {
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
                for (int i = 1; i <= n; i++)
                {
                    Vector2 cur = he.PointAt((double)i / n);
                    A += prev.X * cur.Y - cur.X * prev.Y;
                    prev = cur;
                }
            }

            double outterRingArea = 0.5 * A;

            double innerRingsArea = 0.0;

            foreach (region_store inner_region in Children)
            {
                innerRingsArea += inner_region.SignedAreaOfRegion(n);
            }


            return outterRingArea - innerRingsArea;
        }

    }


}
