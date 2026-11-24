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
        public List<List<halfedge_store>> InnerRings; // CW, each is a hole

        // Geometry back-reference: each HalfEdge carries a reference to the source primitive
        // (line_store, circle_store, ellipse_store, spline_store) and a parameter direction.

        // Nesting (derived, not manually set)
        public region_store Parent;                // null for top-level regions
        public List<region_store> Children;        // regions directly contained inside this one

        // Solver-facing properties
        public Dictionary<string, object> Properties; // material, BC, etc.

       

        public void AddOuterRing(List<halfedge_store> halfEdges)
        {
            OuterRing = halfEdges;

            //foreach (var he in halfEdges)
            //{
            //    he.Next = halfEdges[(halfEdges.IndexOf(he) + 1) % halfEdges.Count];
            //    he.Prev = halfEdges[(halfEdges.IndexOf(he) - 1 + halfEdges.Count) % halfEdges.Count];
            //}
        }


    }


}
