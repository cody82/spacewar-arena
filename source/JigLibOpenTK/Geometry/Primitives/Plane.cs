#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using JigLibX.Math;
using JigLibOpenTK;
#endregion

namespace JigLibX.Geometry
{
    /// <summary>
    /// Class Plane
    /// </summary>
    public class Plane : Primitive
    {

        internal Vector3 normal = Vector3.Zero;
        private float d = 0.0f;

        /// <summary>
        /// Constructor
        /// </summary>
        public Plane()
            : base((int)PrimitiveType.Plane)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        public Plane(Vector3 n, float d)
            : base((int)PrimitiveType.Plane)
        {
            JiggleMath.NormalizeSafe(ref n);
            this.normal = n;
            this.d = d;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="n"></param>
        /// <param name="pos"></param>
        public Plane(Vector3 n, Vector3 pos)
            : base((int)PrimitiveType.Plane)
        {
            JiggleMath.NormalizeSafe(ref n);
            this.normal = n;
            this.d = -Vector3.Dot(n, pos);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos0"></param>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        public Plane(Vector3 pos0,Vector3 pos1,Vector3 pos2)
            : base((int)PrimitiveType.Plane)
        {
            Vector3 dr1 = pos1 - pos0;
            Vector3 dr2 = pos2 - pos0;

            this.normal = Vector3.Cross(dr1, dr2);
            float mNLen = normal.Length;
            if (mNLen < JiggleMath.Epsilon)
            {
                this.normal = Vector3Extensions.Up;
                this.d = 0.0f;
            }
            else
            {
                this.normal /= mNLen;
                this.d = -Vector3.Dot(this.normal, pos0);
            }
        }

        /// <summary>
        /// Gets or Set normal
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set { this.normal = value; }
        }

        /// <summary>
        /// Gets or Sets d
        /// </summary>
        public float D
        {
            get { return this.d; }
            set { this.d = value; }
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>Primitive</returns>
        public override Primitive Clone()
        {
            Plane newPlane = new Plane(this.Normal, this.D);
            newPlane.Transform = Transform;
            return newPlane;
        }
        private Matrix4 transformMatrix;
        private Matrix4 invTransform;
        /// <summary>
        /// Gets Transform or Sets transformMatrix and invTransform
        /// </summary>
        public override Transform Transform
        {
            get
            {
                return base.Transform;
            }
            set
            {
                base.Transform = value;
                transformMatrix = transform.Orientation;
                transformMatrix.Row3 = new Vector4(transform.Position,transform.Orientation.Row3.W);
                invTransform = Matrix4Extensions.Invert(transformMatrix);
            }
        }

        /// <summary>
        /// Use a cached version. Gets transformMatrix
        /// </summary>
        public override Matrix4 TransformMatrix
        {
            get
            {
                return transformMatrix;
            }
        }

        /// <summary>
        /// Use a cached version. Gets invTransform
        /// </summary>
        public override Matrix4 InverseTransformMatrix
        {
            get
            {
                return invTransform;
            }
        }

        /// <summary>
        /// SegmentIntersect
        /// </summary>
        /// <param name="frac"></param>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        /// <param name="seg"></param>
        /// <returns>bool</returns>
        public override bool SegmentIntersect(out float frac, out Vector3 pos, out Vector3 normal, Segment seg)
        {
            bool result;
            if (result = Intersection.SegmentPlaneIntersection(out frac, seg, this))
            {
                pos = seg.GetPoint(frac);
                normal = this.Normal;
            }
            else
            {
                pos = Vector3.Zero;
                normal = Vector3.Zero;
            }

            return result;
        }

        /// <summary>
        /// GetVolume
        /// </summary>
        /// <returns>0.0f</returns>
        public override float GetVolume()
        {
            return 0.0f;
        }

        /// <summary>
        /// GetSurfaceArea
        /// </summary>
        /// <returns>0.0f</returns>
        public override float GetSurfaceArea()
        {
            return 0.0f;
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

        /// <summary>
        /// Invert
        /// </summary>
        public void Invert()
        {
            Vector3Extensions.Negate(ref normal, out normal);
        }

        /// <summary>
        /// GetInverse
        /// </summary>
        /// <returns>Plane</returns>
        public Plane GetInverse()
        {
            Plane plane = new Plane(this.normal, this.d);
            plane.Invert();
            return plane;
        }

    }
}
