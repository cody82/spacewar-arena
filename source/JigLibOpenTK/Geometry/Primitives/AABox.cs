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
    /// An axis aligned Box.
    /// </summary>
    public class AABox : Primitive
    {

        private Vector3 minPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 maxPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        private static AABox hugeBox = new AABox(
            new Vector3(float.MinValue, float.MinValue, float.MinValue), new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

        /// <summary>
        /// Position based on one corner. sideLengths are the full side
        /// lengths (each element must be >= 0)
        /// </summary>
        /// <param name="minPos"></param>
        /// <param name="maxPos"></param>
        public AABox(Vector3 minPos, Vector3 maxPos) :
            base((int)PrimitiveType.AABox)
        {
            this.minPos = minPos;
            this.maxPos = maxPos;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AABox() : 
            base((int)PrimitiveType.AABox)
        {
           this.Clear();
        }

        private Vector3 offset = Vector3.Zero;
        /// <summary>
        /// Gets new Transform or Sets offset to value.Position
        /// </summary>
        public override Transform Transform
        {
            get
            {
                return new Transform(offset, Matrix4.Identity);
            }
            set
            {
                maxPos = maxPos - offset + value.Position;
                minPos = minPos - offset + value.Position;
                offset = value.Position;
            }
        }

        /// <summary>
        /// Adding points etc.
        /// </summary>
        public void Clear()
        {
            minPos.X = minPos.Y = minPos.Z = float.MaxValue;
            maxPos.X = maxPos.Y = maxPos.Z = float.MinValue;
        }

        /// <summary>
        /// Adds a point to the AABB.
        /// </summary>
        /// <remarks>
        /// This function is heavily used to calculate the axis
        /// aligned bounding boxes arround any object. Calling 
        /// by reference is unusal but makes it faster.
        /// </remarks> 
        /// <param name="pos"></param>
        public void AddPoint(ref Vector3 pos)
        {
            if (pos.X < minPos.X) minPos.X = pos.X - JiggleMath.Epsilon;
            if (pos.X > maxPos.X) maxPos.X = pos.X + JiggleMath.Epsilon;

            if (pos.Y < minPos.Y) minPos.Y = pos.Y - JiggleMath.Epsilon;
            if (pos.Y > maxPos.Y) maxPos.Y = pos.Y + JiggleMath.Epsilon;

            if (pos.Z < minPos.Z) minPos.Z = pos.Z - JiggleMath.Epsilon;
            if (pos.Z > maxPos.Z) maxPos.Z = pos.Z + JiggleMath.Epsilon;
        }

        /// <summary>
        /// Adds a point to the AABB
        /// </summary>
        /// <param name="pos"></param>
        public void AddPoint(Vector3 pos)
        {
            if (pos.X < minPos.X) minPos.X = pos.X - JiggleMath.Epsilon;
            if (pos.X > maxPos.X) maxPos.X = pos.X + JiggleMath.Epsilon;

            if (pos.Y < minPos.Y) minPos.Y = pos.Y - JiggleMath.Epsilon;
            if (pos.Y > maxPos.Y) maxPos.Y = pos.Y + JiggleMath.Epsilon;

            if (pos.Z < minPos.Z) minPos.Z = pos.Z - JiggleMath.Epsilon;
            if (pos.Z > maxPos.Z) maxPos.Z = pos.Z + JiggleMath.Epsilon;
        }
/*
        public void AddBox(Box box)
        {
            Vector3[] pts = new Vector3[8];
            box.GetCornerPoints(out pts);

            AddPoint(ref pts[0]);
            AddPoint(ref pts[1]);
            AddPoint(ref pts[2]);
            AddPoint(ref pts[3]);
            AddPoint(ref pts[4]);
            AddPoint(ref pts[5]);
            AddPoint(ref pts[6]);
            AddPoint(ref pts[7]);
        }

        public void AddSegment(Segment seg)
        {
            AddPoint(seg.Origin);
            AddPoint(seg.GetEnd());
        }

        public void AddAABox(AABox aabox)
        {
            AddPoint(aabox.MaxPos);
            AddPoint(aabox.MinPos);
        }

        public void AddSphere(Sphere sphere)
        {
            if ((sphere.Position.X - sphere.Radius) < minPos.X)
                minPos.X = (sphere.Position.X - sphere.Radius) - JiggleMath.Epsilon;
            if ((sphere.Position.X + sphere.Radius) > maxPos.X)
                maxPos.X = (sphere.Position.X + sphere.Radius) + JiggleMath.Epsilon;

            if ((sphere.Position.Y - sphere.Radius) < minPos.Y)
                minPos.Y = (sphere.Position.Y - sphere.Radius) - JiggleMath.Epsilon;
            if ((sphere.Position.Y + sphere.Radius) > maxPos.Y)
                maxPos.Y = (sphere.Position.Y + sphere.Radius) + JiggleMath.Epsilon;

            if ((sphere.Position.Z - sphere.Radius) < minPos.Z)
                minPos.Z = (sphere.Position.Z - sphere.Radius) - JiggleMath.Epsilon;
            if ((sphere.Position.Z + sphere.Radius) > maxPos.Z)
                maxPos.Z = (sphere.Position.Z + sphere.Radius) + JiggleMath.Epsilon;
        }

        public void AddCapsule(Capsule capsule)
        {
            AddSphere(new Sphere(capsule.Position, capsule.Radius));
            AddSphere(new Sphere(capsule.Position + capsule.Length * capsule.Orientation.Backward, capsule.Radius));
        }

        public void AddPrimitive(Primitive prim)
        {
            switch ((PrimitiveType)prim.Type)
            {
                case PrimitiveType.Box:
                    AddBox((Box)prim);
                    break;
                case PrimitiveType.Sphere:
                    AddSphere((Sphere)prim);
                    break;
                case PrimitiveType.Capsule:
                    AddCapsule((Capsule)prim);
                    break;
                default:
                    AddAABox(prim.GetBoundingBox());
                    break;
            }
        }
*/
        /// <summary>
        /// Move minPos and maxPos += delta
        /// </summary>
        /// <param name="delta"></param>
        public void Move(Vector3 delta)
        {
            minPos += delta;
            maxPos += delta;
        }

        /// <summary>
        /// IsPointInside
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>bool</returns>
        public bool IsPointInside(Vector3 pos)
        {
            return ((pos.X >= minPos.X) &&
                (pos.X <= maxPos.X) &&
                (pos.Y >= minPos.Y) &&
                (pos.Y <= maxPos.Y) &&
                (pos.Z >= minPos.Z) &&
                (pos.Z <= maxPos.Z));
        }

        /// <summary>
        /// OverlapTest
        /// </summary>
        /// <param name="box0"></param>
        /// <param name="box1"></param>
        /// <returns>bool</returns>
        public static bool OverlapTest(AABox box0, AABox box1)
        {
            return ((box0.minPos.Z >= box1.maxPos.Z) ||
                (box0.maxPos.Z <= box1.minPos.Z) ||
                (box0.minPos.Y >= box1.maxPos.Y) ||
                (box0.maxPos.Y <= box1.minPos.Y) ||
                (box0.minPos.X >= box1.maxPos.X) ||
                (box0.maxPos.X <= box1.minPos.X)) ? false : true;
        }

        /// <summary>
        /// OverlapTest
        /// </summary>
        /// <param name="box0"></param>
        /// <param name="box1"></param>
        /// <param name="tol"></param>
        /// <returns>bool</returns>
        public static bool OverlapTest(AABox box0, AABox box1, float tol)
        {
            return ((box0.minPos.Z >= box1.maxPos.Z + tol) ||
                (box0.maxPos.Z <= box1.minPos.Z - tol) ||
                (box0.minPos.Y >= box1.maxPos.Y + tol) ||
                (box0.maxPos.Y <= box1.minPos.Y - tol) ||
                (box0.minPos.X >= box1.maxPos.X + tol) ||
                (box0.maxPos.X <= box1.minPos.X - tol)) ? false : true;
        }

        /// <summary>
        /// GetCentre
        /// </summary>
        /// <returns><c>0.5f * (minPos + maxPos)</c></returns>
        public Vector3 GetCentre()
        {
            return 0.5f * (minPos + maxPos);
        }

        /// <summary>
        /// Gets or Sets minPos
        /// </summary>
        public Vector3 MinPos
        {
            get { return this.minPos; }
            set { this.minPos = value; }
        }

        /// <summary>
        /// Gets or Sets maxPos
        /// </summary>
        public Vector3 MaxPos
        {
            get { return this.maxPos; }
            set { this.maxPos = value; }
        }

        /// <summary>
        /// GetSideLengths
        /// </summary>
        /// <returns>maxPos - minPos</returns>
        public Vector3 GetSideLengths()
        {
            return maxPos - minPos;
        }

        /// <summary>
        /// GetRadiusAboutCentre
        /// </summary>
        /// <returns><c>0.5f * (maxPos - minPos).Length</c></returns>
        public float GetRadiusAboutCentre()
        {
            return 0.5f * (maxPos - minPos).Length;
        }

        /// <summary>
        /// GetRadiusSqAboutCentre
        /// </summary>
        /// <returns>float</returns>
        public float GetRadiusSqAboutCentre()
        {
            float result = this.GetRadiusAboutCentre();
            return result * result;
        }

        /// <summary>
        /// Gets hugeBox
        /// </summary>
        public static AABox HugeBox
        {
            get { return hugeBox; }
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>new AABox</returns>
        public override Primitive Clone()
        {
            return new AABox(this.minPos, this.maxPos);
        }

        /// <summary>
        /// SegmentIntersect
        /// </summary>
        /// <remarks>This is not implemented. It will throw an exception.</remarks>
        /// <param name="frac"></param>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        /// <param name="seg"></param>
        /// <returns>bool</returns>
        public override bool SegmentIntersect(out float frac, out Vector3 pos, out Vector3 normal, Segment seg)
        {
            // todo implement
            // throw new JigLibXException("Not implemented!");
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetVolume
        /// </summary>
        /// <returns><c>(this.maxPos - this.minPos).LengthSquared</c></returns>
        public override float GetVolume()
        {
            return (this.maxPos - this.minPos).LengthSquared;
        }

        /// <summary>
        /// GetSurfaceArea
        /// </summary>
        /// <returns>float</returns>
        public override float GetSurfaceArea()
        {
            Vector3 sl = this.maxPos - this.minPos;
            return 2.0f * (sl.X * sl.Y + sl.X * sl.Z + sl.Y * sl.Z);
        }

        /// <summary>
        /// GetMassProperties
        /// </summary>
        /// <param name="primitiveProperties"></param>
        /// <param name="mass"></param>
        /// <param name="centerOfMass"></param>
        /// <param name="inertiaTensor"></param>
        public override void GetMassProperties(PrimitiveProperties primitiveProperties, out float mass, out Vector3 centerOfMass, out Matrix4 inertiaTensor)
        {
            mass = 0.0f;
            centerOfMass = Vector3.Zero;
            inertiaTensor = Matrix4.Identity;
        }
    }

    /// <summary>
    /// Class BoundingBoxHelper
    /// </summary>
    public class BoundingBoxHelper
    {
        /// <summary>
        /// InitialBox
        /// </summary>
        static public BoundingBox InitialBox = new BoundingBox(new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity), new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity));
        /// <summary>
        /// AddPoint
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="bb"></param>
        static public void AddPoint(ref Vector3 pos, ref BoundingBox bb)
        {
            Vector3Extensions.Min(ref bb.Min, ref pos, out bb.Min);
            Vector3Extensions.Max(ref bb.Max, ref pos, out bb.Max);
        }
        /// <summary>
        /// AddPoint
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="bb"></param>
        static public void AddPoint(Vector3 pos, ref BoundingBox bb)
        {
            Vector3Extensions.Min(ref bb.Min, ref pos, out bb.Min);
            Vector3Extensions.Max(ref bb.Max, ref pos, out bb.Max);
        }

        static Vector3[] pts = new Vector3[8];
        /// <summary>
        /// AddBox
        /// </summary>
        /// <remarks>Note: Not thread safe or re-entrant as it uses pts</remarks>
        /// <param name="box"></param>
        /// <param name="bb"></param>
        static public void AddBox(Box box, ref BoundingBox bb)
        {
            // NOTE Not thread safe or rentrant as its uses pts
            box.GetCornerPoints(out pts);

            AddPoint(ref pts[0],ref bb);
            AddPoint(ref pts[1],ref bb);
            AddPoint(ref pts[2],ref bb);
            AddPoint(ref pts[3],ref bb);
            AddPoint(ref pts[4],ref bb);
            AddPoint(ref pts[5],ref bb);
            AddPoint(ref pts[6],ref bb);
            AddPoint(ref pts[7],ref bb);
        }
        /// <summary>
        /// AddSegment
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="bb"></param>
        static public void AddSegment(Segment seg, ref BoundingBox bb)
        {
            AddPoint(seg.Origin,ref bb);
            AddPoint(seg.GetEnd(), ref bb);
        }

        /// <summary>
        /// AddAABox
        /// </summary>
        /// <param name="aabox"></param>
        /// <param name="bb"></param>
        static public void AddAABox(AABox aabox, ref BoundingBox bb)
        {
            bb.Min = Vector3Extensions.Min(aabox.MinPos, bb.Min);
            bb.Max = Vector3Extensions.Max(aabox.MaxPos, bb.Max);
        }

        /// <summary>
        /// AddBBox
        /// </summary>
        /// <param name="bbox"></param>
        /// <param name="bb"></param>
        static public void AddBBox(BoundingBox bbox, ref BoundingBox bb)
        {
            bb.Min = Vector3Extensions.Min(bbox.Min, bb.Min);
            bb.Max = Vector3Extensions.Max(bbox.Max, bb.Max);
        }

        /// <summary>
        /// AddSphere
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="bb"></param>
        static public void AddSphere(Sphere sphere, ref BoundingBox bb)
        {
            Vector3 radius = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);
            Vector3 minSphere = sphere.Position;
            Vector3 maxSphere = sphere.Position;

            Vector3.Subtract(ref minSphere, ref radius, out minSphere);
            Vector3.Add(ref maxSphere, ref radius, out maxSphere);

            Vector3Extensions.Min(ref bb.Min, ref minSphere, out bb.Min);
            Vector3Extensions.Max(ref bb.Max, ref maxSphere, out bb.Max);
        }

        /// <summary>
        /// AddSphere
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="bb"></param>
        static public void AddSphere(BoundingSphere sphere, ref BoundingBox bb)
        {
            Vector3 radius = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);
            Vector3 minSphere = sphere.Center;
            Vector3 maxSphere = sphere.Center;

            Vector3.Subtract(ref minSphere, ref radius, out minSphere);
            Vector3.Add(ref maxSphere, ref radius, out maxSphere);

            Vector3Extensions.Min(ref bb.Min, ref minSphere, out bb.Min);
            Vector3Extensions.Max(ref bb.Max, ref maxSphere, out bb.Max);
        }

        /// <summary>
        /// AddCapsule
        /// </summary>
        /// <param name="capsule"></param>
        /// <param name="bb"></param>
        static public void AddCapsule(Capsule capsule, ref BoundingBox bb)
        {
            AddSphere(new BoundingSphere(capsule.Position, capsule.Radius), ref bb);
            AddSphere(new BoundingSphere(capsule.Position + capsule.Length * capsule.Orientation.Backward(), capsule.Radius), ref bb);
        }

        /// <summary>
        /// AddPrimitive
        /// </summary>
        /// <param name="prim"></param>
        /// <param name="bb"></param>
        static public void AddPrimitive(Primitive prim, ref BoundingBox bb)
        {
            switch ((PrimitiveType)prim.Type)
            {
                case PrimitiveType.Box:
                    AddBox((Box)prim, ref bb);
                    break;
                case PrimitiveType.Sphere:
                    AddSphere((Sphere)prim, ref bb);
                    break;
                case PrimitiveType.Capsule:
                    AddCapsule((Capsule)prim, ref bb);
                    break;
                default:
                    AddAABox(prim.GetBoundingBox(), ref bb);
                    break;
            }
        }

        /// <summary>
        /// OverlapTest
        /// </summary>
        /// <param name="box0"></param>
        /// <param name="box1"></param>
        /// <returns>bool</returns>
        public static bool OverlapTest(ref BoundingBox box0, ref BoundingBox box1)
        {
            return ((box0.Min.Z >= box1.Max.Z) ||
                (box0.Max.Z <= box1.Min.Z) ||
                (box0.Min.Y >= box1.Max.Y) ||
                (box0.Max.Y <= box1.Min.Y) ||
                (box0.Min.X >= box1.Max.X) ||
                (box0.Max.X <= box1.Min.X)) ? false : true;
        }

        /// <summary>
        /// OverlapTest
        /// </summary>
        /// <param name="box0"></param>
        /// <param name="box1"></param>
        /// <param name="tol"></param>
        /// <returns>bool</returns>
        public static bool OverlapTest(ref BoundingBox box0, ref BoundingBox box1, float tol)
        {
            return ((box0.Min.Z >= box1.Max.Z + tol) ||
                (box0.Max.Z <= box1.Min.Z - tol) ||
                (box0.Min.Y >= box1.Max.Y + tol) ||
                (box0.Max.Y <= box1.Min.Y - tol) ||
                (box0.Min.X >= box1.Max.X + tol) ||
                (box0.Max.X <= box1.Min.X - tol)) ? false : true;
        }


    }
}
