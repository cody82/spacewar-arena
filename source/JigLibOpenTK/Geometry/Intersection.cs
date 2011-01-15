#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using JigLibX.Math;
using JigLibX.Geometry;
using JigLibOpenTK;
#endregion

namespace JigLibX.Geometry
{
    /// <summary>
    /// Class Intersection
    /// </summary>
    public sealed class Intersection
    {
        /// <summary>
        /// enum EdgesToTest
        /// </summary>
        public enum EdgesToTest
        {
            /// <summary>
            /// None
            /// </summary>
            None = 0,
            /// <summary>
            /// edge0
            /// </summary>
            Edge0 = 1 << 0,
            /// <summary>
            /// edge1
            /// </summary>
            Edge1 = 1 << 1,
            /// <summary>
            /// edge2
            /// </summary>
            Edge2 = 1 << 2,
            /// <summary>
            /// edgeAll
            /// </summary>
            EdgeAll = Edge0 | Edge1 | Edge2
        }

        /// <summary>
        /// enum CornersToTest
        /// </summary>
        public enum CornersToTest
        {
            /// <summary>
            /// None
            /// </summary>
            None = 0,
            /// <summary>
            /// Corner0
            /// </summary>
            Corner0 = 1 << 0,
            /// <summary>
            /// Corner1
            /// </summary>
            Corner1 = 1 << 1,
            /// <summary>
            /// Corner2
            /// </summary>
            Corner2 = 1 << 2,
            /// <summary>
            /// CornerAll
            /// </summary>
            CornerAll = Corner0 | Corner1 | Corner2
        }

        private Intersection() { }

        #region LinePlaneIntersection
        /// <summary>
        /// LinePlaneIntersection
        /// </summary>
        /// <param name="t"></param>
        /// <param name="line"></param>
        /// <param name="plane"></param>
        /// <returns>bool</returns>
        public static bool LinePlaneIntersection(out float t, Line line, Plane plane)
        {
            float dot = Vector3.Dot(line.Dir, plane.Normal);

            if (System.Math.Abs(dot) < JiggleMath.Epsilon)
            {
                t = 0.0f;
                return false;
            }

            float dist = Distance.PointPlaneDistance(line.Origin,plane);
            t = -dist / dot;
            return true;
        }
        #endregion

        #region RayPlaneIntersection
        /// <summary>
        /// RayPlaneIntersection
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ray"></param>
        /// <param name="plane"></param>
        /// <returns>bool</returns>
        public static bool RayPlaneIntersection(out float t, Ray ray, Plane plane)
        {
            float dot = Vector3.Dot(ray.Dir, plane.Normal);
            if (System.Math.Abs(dot) < JiggleMath.Epsilon)
            {
                t = 0.0f;
                return false;
            }

            float dist = Distance.PointPlaneDistance(ray.Origin, plane);
            t = -dist / dot;
            return (t >= 0.0f);
        }
        #endregion

        #region SegmentPlaneIntersection
        /// <summary>
        /// SegmentPlaneIntersection
        /// </summary>
        /// <param name="tS"></param>
        /// <param name="seg"></param>
        /// <param name="plane"></param>
        /// <returns>bool</returns>
        public static bool SegmentPlaneIntersection(out float tS, Segment seg, Plane plane)
        {
            float denom = Vector3.Dot(plane.Normal, seg.Delta);
            if (System.Math.Abs(denom) > JiggleMath.Epsilon)
            {
                float t = -(Vector3.Dot(plane.Normal, seg.Origin) + plane.D) / denom;
                if (t < 0.0f || t > 1.0f)
                {
                    tS = 0.0f;
                    return false;
                }
                tS = t;
                return true;
            }
            else
            {
                // parallel - return false even if it's in the plane
                tS = 0.0f;
                return false;
            }
        }
        #endregion

        #region SweptSpherePlaneIntersection
        /// <summary>
        /// SweptSpherePlaneIntersection
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="finalPenetration"></param>
        /// <param name="oldSphere"></param>
        /// <param name="newSphere"></param>
        /// <param name="planeNormal"></param>
        /// <param name="pOldDistToPlane"></param>
        /// <param name="pNewDistToPlane"></param>
        /// <returns>bool</returns>
        public static bool SweptSpherePlaneIntersection(out Vector3 pt, out float finalPenetration, BoundingSphere oldSphere, BoundingSphere newSphere,
            Vector3 planeNormal, float pOldDistToPlane, float pNewDistToPlane)
        {
            float oldDistToPlane = pOldDistToPlane;
            float newDistToPlane = pNewDistToPlane;
            float radius = oldSphere.Radius;

            pt = Vector3.Zero;
            finalPenetration = 0.0f;

            if (newDistToPlane >= oldDistToPlane)
                return false;
            if (newDistToPlane > radius)
                return false;

            // intersect with plane
            float t = (newDistToPlane - radius) / (newDistToPlane - oldDistToPlane);
            if (t < 0.0f || t > 1.0f)
                return false;

            pt = oldSphere.Center + t * (newSphere.Center- oldSphere.Center) - OpenTKHelper.Min(radius, oldDistToPlane) * planeNormal;
            finalPenetration = radius - newDistToPlane;
            return true;
        }
        #endregion

        #region SweptSphereTriangleIntersection
        /// <summary>
        /// SweptSphereTriangleIntersection
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="N"></param>
        /// <param name="depth"></param>
        /// <param name="oldSphere"></param>
        /// <param name="newSphere"></param>
        /// <param name="triangle"></param>
        /// <param name="oldCentreDistToPlane"></param>
        /// <param name="newCentreDistToPlane"></param>
        /// <param name="edgesToTest"></param>
        /// <param name="cornersToTest"></param>
        /// <returns>bool</returns>
        public static bool SweptSphereTriangleIntersection(out Vector3 pt, out Vector3 N, out float depth,
            BoundingSphere oldSphere, BoundingSphere newSphere, Triangle triangle,
            float oldCentreDistToPlane, float newCentreDistToPlane,
            EdgesToTest edgesToTest, CornersToTest cornersToTest)
        {
            int i;
            Plane trianglePlane = triangle.Plane;
            N = Vector3.Zero;

            // Check against plane
            if (!SweptSpherePlaneIntersection(out pt,out depth, oldSphere, newSphere, trianglePlane.Normal, oldCentreDistToPlane, newCentreDistToPlane))
                return false;

            Vector3 v0 = triangle.GetPoint(0);
            Vector3 v1 = triangle.GetPoint(1);
            Vector3 v2 = triangle.GetPoint(2);

            Vector3 e0 = v1 - v0;
            Vector3 e1 = v2 - v1;
            Vector3 e2 = v0 - v2;

            // If the point is inside the triangle, this is a hit
            bool allInside = true;
            Vector3 outDir0 = Vector3.Cross(e0, trianglePlane.Normal);
            if (Vector3.Dot(pt - v0, outDir0) > 0.0f)
            {
                allInside = false;
            }
            Vector3 outDir1 = Vector3.Cross(e1, trianglePlane.Normal);
            if (Vector3.Dot(pt - v1, outDir1) > 0.0f)
            {
                allInside = false;
            }
            Vector3 outDir2 = Vector3.Cross(e2, trianglePlane.Normal);
            if (Vector3.Dot(pt - v2, outDir2) > 0.0f)
            {
                allInside = false;
            }

            // Quick result?
            if (allInside)
            {
                N = trianglePlane.Normal;
                return true;
            }

            // Now check against the edges
            float bestT = float.MaxValue;
            Vector3 Ks = newSphere.Center - oldSphere.Center;
            float kss = Vector3.Dot(Ks, Ks);
            float radius = newSphere.Radius;
            float radiusSq = radius * radius;
            for (i = 0; i < 3; ++i)
            {
                int mask = 1 << i;
                if (!((mask !=0) & ((int)edgesToTest != 0))) // TODO: CHECK THIS
                    continue;
                Vector3 Ke;
                Vector3 vp;

                switch (i)
                {
                    case 0:
                        Ke = e0;
                        vp = v0;
                        break;
                    case 1:
                        Ke = e1;
                        vp = v1;
                        break;
                    case 2:
                    default:
                        Ke = e2;
                        vp = v2;
                        break;

                }
                Vector3 Kg = vp - oldSphere.Center;

                float kee = Vector3.Dot(Ke, Ke);
                if (System.Math.Abs(kee) < JiggleMath.Epsilon)
                    continue;
                float kes = Vector3.Dot(Ke, Ks);
                float kgs = Vector3.Dot(Kg, Ks);
                float keg = Vector3.Dot(Ke, Kg);
                float kgg = Vector3.Dot(Kg, Kg);

                // a * t^2 + b * t + c = 0
                float a = kee * kss - (kes * kes);
                if (System.Math.Abs(a) < JiggleMath.Epsilon)
                    continue;
                float b = 2.0f * (keg * kes - kee * kgs);
                float c = kee * (kgg - radiusSq) - keg * keg;

                float blah = b*b - 4.0f * a * c;
                if (blah < 0.0f)
                    continue;

                // solve for t - take minimum
                float t = (-b - (float)System.Math.Sqrt(blah)) / (2.0f * a);

                if (t < 0.0f || t > 1.0f)
                    continue;

                if (t > bestT)
                    continue;

                // now check where it hit on the edge
                Vector3 Ct = oldSphere.Center + t * Ks;
                float d = Vector3.Dot((Ct - vp), Ke) / kee;

                if (d < 0.0f || d > 1.0f)
                    continue;

                // wahay - got hit. Already checked that t < bestT
                bestT = t;

                pt = vp + d * Ke;
                N = (Ct - pt);// .GetNormalisedSafe();
                JiggleMath.NormalizeSafe(ref N);
                // depth is already calculated
            }
            if (bestT <= 1.0f)
                return true;

            // check the corners
            bestT = float.MaxValue;
            for (i = 0; i < 3; ++i)
            {
                int mask = 1 << i;
                if (!((mask != 0) & (cornersToTest != 0))) // CHECK THIS
                    continue;
                Vector3 vp;

                switch (i)
                {
                    case 0:
                        vp = v0;
                        break;
                    case 1:
                        vp = v1;
                        break;
                    case 2:
                    default:
                        vp = v2;
                        break;

                }
                Vector3 Kg = vp - oldSphere.Center;
                float kgs = Vector3.Dot(Kg, Ks);
                float kgg = Vector3.Dot(Kg, Kg);
                float a = kss;
                if (System.Math.Abs(a) < JiggleMath.Epsilon)
                    continue;
                float b = -2.0f * kgs;
                float c = kgg - radiusSq;

                float blah = (b * b) - 4.0f * a * c;
                if (blah < 0.0f)
                    continue;

                // solve for t - take minimum
                float t = (-b - (float) System.Math.Sqrt(blah)) / (2.0f * a);

                if (t < 0.0f || t > 1.0f)
                    continue;

                if (t > bestT)
                    continue;

                bestT = t;

                Vector3 Ct = oldSphere.Center + t * Ks;
                N = (Ct - vp);//.GetNormalisedSafe();
                JiggleMath.NormalizeSafe(ref N);
            }
            if (bestT <= 1.0f)
                return true;

            return false;

        }
        #endregion

        #region SegmentSphereIntersection
        /// <summary>
        /// SegmentSphereIntersection
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="seg"></param>
        /// <param name="sphere"></param>
        /// <returns>bool</returns>
        public static bool SegmentSphereIntersection(out float ts, Segment seg, Sphere sphere)
        {
            Vector3 r = seg.Delta;
            Vector3 s = seg.Origin - sphere.Position;

            float radiusSq = sphere.Radius * sphere.Radius;
            float rSq = r.LengthSquared;

            ts = float.MaxValue;

            if (rSq < radiusSq)
            {
                // starting inside
                ts = 0.0f;
                return false;
            }

            float sDotr = Vector3.Dot(s, r);
            float sSq = s.LengthSquared;
            float sigma = (sDotr * sDotr) - rSq * (sSq - radiusSq);
            if (sigma < 0.0f)
                return false;
            float sigmaSqrt = (float)System.Math.Sqrt((float)sigma);
            float lambda1 = (-sDotr - sigmaSqrt) / rSq;
            float lambda2 = (-sDotr + sigmaSqrt) / rSq;
            if (lambda1 > 1.0f || lambda2 < 0.0f)
                return false;
            // intersection!
            ts = OpenTKHelper.Max(lambda1, 0.0f);
            return true;
        }
        #endregion

        #region SegmentCapsuleIntersection
        /// <summary>
        /// SegmentCapsuleIntersection
        /// </summary>
        /// <param name="tS"></param>
        /// <param name="seg"></param>
        /// <param name="capsule"></param>
        /// <returns>bool</returns>
        public static bool SegmentCapsuleIntersection(out float tS, Segment seg, Capsule capsule)
        {
            float bestFrac = float.MaxValue;

            tS = 0;

            // do the main sides
            float sideFrac = float.MaxValue;
            if (!SegmentInfiniteCylinderIntersection(out sideFrac, seg,
                    new Segment(capsule.Position, capsule.Orientation.Backward()),
                    capsule.Radius))
                return false; // check this

            // only keep this if the side intersection point is within the capsule segment ends
            Vector3 sidePos = seg.GetPoint(sideFrac);
            if (Vector3.Dot(sidePos - capsule.Position, capsule.Orientation.Backward()) < 0.0f)
                sideFrac = float.MaxValue;
            else if (Vector3.Dot(sidePos - capsule.GetEnd(), capsule.Orientation.Backward()) > 0.0f)
                sideFrac = float.MaxValue;

            // do the two ends
            float originFrac = float.MaxValue;
            SegmentSphereIntersection(out originFrac, seg, new Sphere(capsule.Position, capsule.Radius));
            float endFrac = float.MaxValue; // Check this!
            SegmentSphereIntersection(out endFrac, seg, new Sphere(capsule.GetEnd(), capsule.Radius));


            bestFrac = OpenTKHelper.Min(sideFrac, originFrac);
            bestFrac = OpenTKHelper.Min(bestFrac, endFrac);

            if (bestFrac <= 1.0f)
            {
                tS = bestFrac;
                return true;
            }

            return false;
        }
        #endregion

        #region SegmentInfiniteCylinderIntersection
        /// <summary>
        /// SegmentInfiniteCylinderIntersection
        /// </summary>
        /// <param name="tS"></param>
        /// <param name="seg"></param>
        /// <param name="cylinderAxis"></param>
        /// <param name="radius"></param>
        /// <returns>bool</returns>
        public static bool SegmentInfiniteCylinderIntersection(out float tS, Segment seg, Segment cylinderAxis, float radius)
        {
            Vector3 Ks = seg.Delta;
            float kss = Vector3.Dot(Ks, Ks);
            float radiusSq = radius * radius;

            Vector3 Ke = cylinderAxis.Delta;
            Vector3 Kg = cylinderAxis.Origin - seg.Origin;

            tS = 0.0f;

            float kee = Vector3.Dot(Ke, Ke);
            if (System.Math.Abs(kee) < JiggleMath.Epsilon)
                return false;

            float kes = Vector3.Dot(Ke, Ks);
            float kgs = Vector3.Dot(Kg, Ks);
            float keg = Vector3.Dot(Ke, Kg);
            float kgg = Vector3.Dot(Kg, Kg);

            // check if start is inside
            float distSq = (Kg - (keg * Ke) / kee).LengthSquared;
            if (distSq < radiusSq)
                return true;

            // a * t^2 + b * t + c = 0
            float a = kee * kss - kes * kes;
            if (System.Math.Abs(a) < JiggleMath.Epsilon)
                return false;

            float b = 2.0f * (keg * kes - kee * kgs);
            float c = kee * (kgg - radiusSq) - keg * keg;

            float blah = b * b - 4.0f * a * c;
            if (blah < 0.0f)
                return false;

            // solve for t - take minimum
            float t = (-b - (float)System.Math.Sqrt((float)blah)) / (2.0f * a);

            if (t < 0.0f || t > 1.0f)
                return false;

            tS = t;

            return true;
        }
        #endregion

        #region SegmentTriangleIntersection
        /// <summary>
        /// SegmentTriangleIntersection
        /// </summary>
        /// <param name="tS"></param>
        /// <param name="tT0"></param>
        /// <param name="tT1"></param>
        /// <param name="seg"></param>
        /// <param name="triangle"></param>
        /// <returns>bool</returns>
        public static bool SegmentTriangleIntersection(out float tS, out float tT0, out float tT1,
                                        Segment seg, Triangle triangle)
        {
            // the parameters - if hit then they get copied into the args
            float u, v, t;

            tS = 0;
            tT0 = 0;
            tT1 = 0;

            Vector3 e1 = triangle.Edge0;
            Vector3 e2 = triangle.Edge1;
            Vector3 p = Vector3.Cross(seg.Delta, e2);
            float a = Vector3.Dot(e1, p);
            if (a > -JiggleMath.Epsilon && a < JiggleMath.Epsilon)
                return false;
            float f = 1.0f / a;
            Vector3 s = seg.Origin - triangle.Origin;
            u = f * Vector3.Dot(s, p);
            if (u < 0.0f || u > 1.0f)
                return false;
            Vector3 q = Vector3.Cross(s, e1);
            v = f * Vector3.Dot(seg.Delta, q);
            if (v < 0.0f || (u + v) > 1.0f)
                return false;
            t = f * Vector3.Dot(e2, q);
            if (t < 0.0f || t > 1.0f)
                return false;

            tS = t;
            tT0 = u;
            tT1 = v;
            //if (tS != 0) tS = t;
            //if (tT0 != 0) tT0 = u;
            //if (tT1 != 0) tT1 = v;
            return true;
        }
        #endregion
    }
}
