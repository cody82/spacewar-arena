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

    #region PrimitiveProperties
    /// <summary>
    /// Struct PrimitiveProperties
    /// </summary>
    public struct PrimitiveProperties
    {
        /// <summary>
        /// enum MassDistribution
        /// </summary>
        public enum MassDistributionEnum
        {
            /// <summary>
            /// Solid
            /// </summary>
            Solid,
            /// <summary>
            /// Shell
            /// </summary>
            Shell
        }

        /// <summary>
        /// enum MassTypeEnum (density is mass per volume SOLID, otherwise pass per surface area
        /// </summary>
        public enum MassTypeEnum
        {
            /// <summary>
            /// Mass
            /// </summary>
            Mass,
            /// <summary>
            /// Density
            /// </summary>
            Density
        }

        /// <summary>
        /// MassType
        /// </summary>
        public MassTypeEnum MassType;
        /// <summary>
        /// MassDistribution
        /// </summary>
        public MassDistributionEnum MassDistribution;
        /// <summary>
        /// MassOrDensity
        /// </summary>
        public float MassOrDensity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="massDistribution"></param>
        /// <param name="massType"></param>
        /// <param name="massOrDensity"></param>
        public PrimitiveProperties(MassDistributionEnum massDistribution,
            MassTypeEnum massType, float massOrDensity)
        {
            this.MassDistribution = massDistribution;
            this.MassOrDensity = massOrDensity;
            this.MassType = massType;
        }

    }
    #endregion

    #region PrimitiveType

    /// <summary>
    /// The JigLibX default Primitives.
    /// </summary>
    public enum PrimitiveType
    {
        /// <summary>
        /// AABox
        /// </summary>
        AABox,
        /// <summary>
        /// Box
        /// </summary>
        Box,
        /// <summary>
        /// Capsule
        /// </summary>
        Capsule,
        /// <summary>
        /// Heightmap
        /// </summary>
        Heightmap,
        /// <summary>
        /// Plane
        /// </summary>
        Plane,
        /// <summary>
        /// Sphere
        /// </summary>
        Sphere,
        /// <summary>
        /// TriangleMesh
        /// </summary>
        TriangleMesh,
        /// <summary>
        /// Cylinder
        /// </summary>
        Cylinder,
        /// <summary>
        /// NumTypes - can add more user defined types
        /// </summary>
        NumTypes 
    }

    #endregion

    /// All geometry primitives should derive from this so that it's possible to 
    /// cast them into the correct type without the overhead/hassle of RTTI or 
    /// virtual fns specific to just one class of primitive. Just do a static cast
    /// based on the type, or use the Get functions
    ///
    /// However, destruction requires virtual functions really, as does supporting other
    /// user-defined primitives
    public abstract class Primitive
    {

        private int type;

        internal Transform transform = Transform.Identity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        public Primitive(int type)
        {
            this.type = type;
        }

        /// <summary>
        /// Returns a copy
        /// </summary>
        /// <returns>Primitive</returns>
        public abstract Primitive Clone();

        /// <summary>
        /// Gets or Sets transform
        /// </summary>
        public virtual Transform Transform 
        { 
            get 
            { 
                return transform; 
            } 
            set 
            { 
                transform = value; 
            } 
        }

        /// <summary>
        /// Gets transform matrix
        /// </summary>
        public virtual Matrix4 TransformMatrix
        {
            get
            {
                Matrix4 trans = transform.Orientation;
                trans.Row3 = new Vector4(transform.Position, transform.Orientation.Row3.W);
                return trans;
            }
        }

        /// <summary>
        /// Gets invert transform matrix
        /// </summary>
        public virtual Matrix4 InverseTransformMatrix
        {
            get
            {
                Matrix4 trans = transform.Orientation;
                trans.Row3 = new Vector4(transform.Position, transform.Orientation.Row3.W);
                return Matrix4Extensions.Invert(trans);
            }
        }

        /// <summary>
        /// Must support intersection with a segment (ray cast)
        /// </summary>
        /// <param name="frac"></param>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        /// <param name="seg"></param>
        /// <returns>bool</returns>
        public abstract bool SegmentIntersect(out float frac,out Vector3 pos,
            out Vector3 normal,Segment seg);

        /// <summary>
        /// Calculate and return the volume
        /// </summary>
        /// <returns>float</returns>
        public abstract float GetVolume();

        /// <summary>
        /// Calculate and return the surface area
        /// </summary>
        /// <returns>float</returns>
        public abstract float GetSurfaceArea();

        /// <summary>
        /// Returns the mass, center of mass, and intertia tensor around the origin
        /// </summary>
        /// <param name="primitiveProperties"></param>
        /// <param name="mass"></param>
        /// <param name="centerOfMass"></param>
        /// <param name="inertiaTensor"></param>
        public abstract void GetMassProperties(PrimitiveProperties primitiveProperties,
            out float mass, out Vector3 centerOfMass, out Matrix4 inertiaTensor);

        /// <summary>
        /// Returns a bounding box that covers this primitive. Default returns a huge box, so
        /// implement this in the derived class for efficiency
        /// </summary>
        public virtual void GetBoundingBox(out AABox box)
        {
            box = AABox.HugeBox;
        }

        /// <summary>
        /// Returns a bounding box that covers this primitive. Default returns a huge box, so
        /// implement this in the derived class for efficiency
        /// </summary>
        public AABox GetBoundingBox()
        {
            AABox result;
            GetBoundingBox(out result);
            return result;
        }

        /// <summary>
        /// Gets type
        /// </summary>
        public int Type
        {
            get { return this.type; }
        }

    }
}
