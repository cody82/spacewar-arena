#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using JigLibX.Math;
#endregion


namespace JigLibX.Geometry
{
    /// <summary>
    /// Class Sphere
    /// </summary>
    public class Sphere : Primitive
    {
        private float radius;
        
        private static Sphere hugeSphere = new Sphere(Vector3.Zero, float.MaxValue);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        public Sphere(Vector3 pos, float radius) : base((int)PrimitiveType.Sphere)
        {
            this.transform.Position = pos;
            this.radius = radius;
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>new Sphere</returns>
        public override Primitive Clone()
        {
            return new Sphere(this.transform.Position, this.radius);
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
            result = Intersection.SegmentSphereIntersection(out frac, seg, this);

            if (result)
            {
                pos = seg.GetPoint(frac);
                normal = pos - this.transform.Position;

                JiggleMath.NormalizeSafe(ref normal);
            }
            else
            {
                pos = Vector3.Zero;
                normal = Vector3.Zero;
            }

            return result;
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
            if (primitiveProperties.MassType == PrimitiveProperties.MassTypeEnum.Mass)
            {
                mass = primitiveProperties.MassOrDensity;
            }
            else
            {
                if (primitiveProperties.MassDistribution == PrimitiveProperties.MassDistributionEnum.Solid)
                    mass = GetVolume() * primitiveProperties.MassOrDensity;
                else
                    mass = GetSurfaceArea() * primitiveProperties.MassOrDensity;
            }

            centerOfMass = this.transform.Position;
            float Ixx;
            if (primitiveProperties.MassDistribution == PrimitiveProperties.MassDistributionEnum.Solid)
                Ixx = 0.4f * mass * radius;
            else
                Ixx = (2.0f / 3.0f) * mass * radius * radius;


            inertiaTensor = Matrix4.Identity;
            inertiaTensor.M11 = inertiaTensor.M22 = inertiaTensor.M33 = Ixx;

            // Transfer of axe theorem
            inertiaTensor.M11 = inertiaTensor.M11 + mass * (centerOfMass.Y * centerOfMass.Y + centerOfMass.Z * centerOfMass.Z);
            inertiaTensor.M22 = inertiaTensor.M22 + mass * (centerOfMass.Z * centerOfMass.Z + centerOfMass.X * centerOfMass.X);
            inertiaTensor.M33 = inertiaTensor.M33 + mass * (centerOfMass.X * centerOfMass.X + centerOfMass.Y * centerOfMass.Y);

            inertiaTensor.M12 = inertiaTensor.M21 = inertiaTensor.M12 - mass * centerOfMass.X * centerOfMass.Y;
            inertiaTensor.M23 = inertiaTensor.M32 = inertiaTensor.M23 - mass * centerOfMass.Y * centerOfMass.Z;
            inertiaTensor.M31 = inertiaTensor.M13 = inertiaTensor.M31 - mass * centerOfMass.Z * centerOfMass.X;
        }

        /// <summary>
        /// Gets or Sets transform
        /// </summary>
        public override Transform Transform
        {
            get{return this.transform;}
            set{this.transform = value;}
        }

        /// <summary>
        /// GetVolume
        /// </summary>
        /// <returns>float</returns>
        public override float GetVolume()
        {
            return (4.0f / 3.0f) * MathHelper.Pi * radius * radius * radius;
        }

        /// <summary>
        /// GetSurfaceArea
        /// </summary>
        /// <returns>float</returns>
        public override float GetSurfaceArea()
        {
            return 4.0f * MathHelper.Pi * radius * radius;
        }

        /// <summary>
        /// Gets or Sets transform.Position
        /// </summary>
        public Vector3 Position
        {
            get { return this.transform.Position; }
            set { this.transform.Position = value; }
        }

        /// <summary>
        /// Gets or Sets radius
        /// </summary>
        public float Radius
        {
            get { return this.radius; }
            set { this.radius = value; }
        }

        /// <summary>
        /// Gets hugeSphere
        /// </summary>
        public static Sphere HugeSphere
        {
            get { return hugeSphere; }
        }


    }
}
