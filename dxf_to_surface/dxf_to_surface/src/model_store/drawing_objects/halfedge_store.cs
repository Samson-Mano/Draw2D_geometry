using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.model_store.drawing_objects
{

    public enum PrimitiveKind { Line, Circle, Ellipse, Spline }

    public class halfedge_store
    {
        // --- Identity ---
        public int HalfEdgeId;

        // --- Source primitive (exactly one is non-null, per Kind) ---
        public PrimitiveKind Kind;
        public line_store Line;
        public circle_store Circle;
        public ellipse_store Ellipse;
        public spline_store Spline;

        // --- Parameter range on the primitive, in the primitive's native parameter ---
        // Line:    t in [0, 1] along Line.Start->Line.End
        // Circle:  angle in [0, 2π), CCW, measured from +X in world frame
        // Ellipse: eccentric anomaly u in [0, 2π), CCW, measured in the ellipse's local frame
        // Spline:  knot parameter u in [knots[degree], knots[n+1]]
        public double StartParam;
        public double EndParam;

        // --- Orientation ---
        // false = evaluate the primitive's native parameterization from StartParam to EndParam
        // true  = the same geometric curve, but traversed in reverse (EndParam -> StartParam)
        // The ring traversal always follows the half-edge from StartPoint to EndPoint.
        public bool Reversed;

        // --- Cached for the solver ---
        public double Length;           // arc length of this half-edge
        public Vector2 StartPoint;      // world point at the ring's "start" of this half-edge
        public Vector2 EndPoint;        // world point at the ring's "end"

        // --- Ring linkage (filled when the ring is assembled) ---
        public halfedge_store Next;           // next half-edge CCW around the same region
        public halfedge_store Prev;           // previous half-edge (optional; derivable from Next)

        
        // --- arc-length table for non-uniform primitives ---
        public double[] ArcLengthTableParam;   // null for line/circle (uniform)
        public double[] ArcLengthTableS;


        // --- Back-pointer to the region (one of the two adjacent regions) ---
        // Convention: this half-edge is on the boundary of the region to its LEFT
        // when traversed StartPoint -> EndPoint.
        public region_store LeftRegion;       // may be null for the outer infinite face
        public halfedge_store Twin;           // the opposite-direction half-edge on the shared edge, if any


        // --- Evaluation ---

        public Vector2 PointAt(double s)       // s in [0,1] by arc-length fraction
        {
            double p = ParamFromFraction(s);
            switch (Kind)
            {
                case PrimitiveKind.Line: return Line.GetLinePoint(p);
                case PrimitiveKind.Circle: return Circle.GetCirclePoint(p);
                case PrimitiveKind.Ellipse: return Ellipse.GetEllipsePoint(p);
                case PrimitiveKind.Spline: return Spline.GetSplinePoint(p);
                default: throw new InvalidOperationException();
            }
        }

        public Vector2 TangentAt(double s)
        {
            double p = ParamFromFraction(s);
            Vector2 d;
            switch (Kind)
            {
                case PrimitiveKind.Line: d = Line.GetLineDerivative(); break;
                case PrimitiveKind.Circle: d = Circle.GetCircleDerivative(p); break;
                case PrimitiveKind.Ellipse: d = Ellipse.GetEllipseDerivative(p); break;
                case PrimitiveKind.Spline: d = Spline.GetSplineDerivative(p); break;
                default: throw new InvalidOperationException();
            }
            if (Reversed) d = -d;
            double len = Math.Sqrt(d.X * d.X + d.Y * d.Y);
            return len < 1e-12 ? new Vector2(0, 0) : new Vector2((float)(d.X / len), (float)(d.Y / len));
        }


        public double CurvatureAt(double s)
        {
            double p = ParamFromFraction(s);
            Vector2 d1, d2;
            switch (Kind)
            {
                case PrimitiveKind.Line:
                    return 0.0;
                case PrimitiveKind.Circle:
                    d1 = Circle.GetCircleDerivative(p);
                    d2 = Circle.GetCircleSecondDerivative(p);
                    break;
                case PrimitiveKind.Ellipse:
                    d1 = Ellipse.GetEllipseDerivative(p);
                    d2 = Ellipse.GetEllipseSecondDerivative(p);
                    break;
                case PrimitiveKind.Spline:
                    d1 = Spline.GetSplineDerivative(p);
                    d2 = Spline.GetSplineSecondDerivative(p);
                    break;
                default: throw new InvalidOperationException();
            }
            if (Reversed) d1 = -d1;
            double cross = d1.X * d2.Y - d1.Y * d2.X;
            double speed = Math.Sqrt(d1.X * d1.X + d1.Y * d1.Y);
            return speed < 1e-12 ? 0.0 : cross / (speed * speed * speed);
        }


        private double ParamFromFraction(double s)
        {
            // Reversed half-edge: fraction s along the reversed traversal is
            // fraction (1 - s) along the primitive's native parameterization.
            double sEff = Reversed ? 1.0 - s : s;

            switch (Kind)
            {
                case PrimitiveKind.Line:
                    {
                        return StartParam + sEff * (EndParam - StartParam);
                    }
                case PrimitiveKind.Circle:
                    {
                        double sweep = circle_store.CCWSweep(Circle.startAngle, Circle.endAngle);
                        return Circle.startAngle + sEff * sweep;
                    }
                case PrimitiveKind.Ellipse:
                    {
                        return LookupByArcLength(
                            Ellipse.ArcLengthTableS,
                            Ellipse.ArcLengthTableParam,
                            sEff * Ellipse.ArcLengthTableS[Ellipse.ArcLengthTableS.Length - 1]);
                    }
                case PrimitiveKind.Spline:
                    {
                        return LookupByArcLength(
                            Spline.ArcLengthTableS,
                            Spline.ArcLengthTableParam,
                            sEff * Spline.ArcLengthTableS[Spline.ArcLengthTableS.Length - 1]);
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        // Given cumulative arc-length array S and parameter array P (same length,
        // both ascending), return the parameter at the given target arc length,
        // linearly interpolated between the bracketing samples.
        private static double LookupByArcLength(double[] S, double[] P, double target)
        {
            int i = BinarySearchInterval(S, target);
            int j = Math.Min(i + 1, S.Length - 1);
            double span = S[j] - S[i];
            double frac = span > 1e-15 ? (target - S[i]) / span : 0.0;
            return P[i] + frac * (P[j] - P[i]);
        }


        /// <summary>
        /// Returns the largest index i with arr[i] <= target, clamped to [0, arr.Length - 2].
        /// Assumes arr is sorted ascending with arr.Length >= 2.
        /// For target < arr[0], returns 0.
        /// For target >= arr[arr.Length - 1], returns arr.Length - 2.
        /// </summary>
        private static int BinarySearchInterval(double[] arr, double target)
        {
            int n = arr.Length;
            if (n < 2) return 0;

            if (target <= arr[0]) return 0;
            if (target >= arr[n - 1]) return n - 2;

            int lo = 0, hi = n - 1;
            while (hi - lo > 1)
            {
                int mid = (lo + hi) >> 1;
                if (arr[mid] <= target) lo = mid;
                else hi = mid;
            }
            return lo;   // arr[lo] <= target < arr[lo + 1], so lo <= n - 2
        }






    }
}
