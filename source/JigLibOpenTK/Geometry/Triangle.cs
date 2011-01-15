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
    /// Defines a 3d triangle. Each edge goes from the origin.
    /// Cross(edge0, edge1)  gives the triangle normal.
    /// </summary>
    public struct Triangle
    {
        private Vector3 origin;
        private Vector3 edge0;
        private Vector3 edge1;

        /// <summary>
        /// Points specified so that pt1-pt0 is edge0 and p2-pt0 is edge1
        /// </summary>
        /// <param name="pt0"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        public Triangle(Vector3 pt0, Vector3 pt1, Vector3 pt2)
        {
            origin = pt0;
            edge0 = pt1 - pt0;
            edge1 = pt2 - pt0;
        }

        /// <summary>
        /// Points specified so that pt1-pt0 is edge0 and p2-pt0 is edge1
        /// </summary>
        /// <param name="pt0"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        public Triangle(ref Vector3 pt0,ref Vector3 pt1,ref Vector3 pt2)
        {
            origin = pt0;
            edge0 = pt1 - pt0;
            edge1 = pt2 - pt0;
        }

        /// <summary>
        /// Same numbering as in the constructor
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetPoint(int i)
        {
            if (i == 1)
                return origin + edge0;

            if (i == 2)
                 return origin + edge1;

            return origin;
        }


        /// <summary>
        /// Same numbering as in the constructor
        /// </summary>
        /// <param name="i"></param>
        /// <param name="point"></param>
        public void GetPoint(int i, out Vector3 point)
        {
            // BEN-BUG-FIX: Previous method always returned origin!
            if (i == 1)
                point = origin + edge0;
            else if (i == 2)
                point = origin + edge1;
            else
                point = origin;

        }

        // BEN-OPTIMISATION: New method with ref point, also accounts for the bug fix.
        /// <summary>
        /// Same numbering as in the constructor
        /// </summary>
        /// <param name="point"></param>
        /// <param name="i"></param>
        public void GetPoint(ref Vector3 point, int i)
        {
            if (i == 1)
            {
                point.X = origin.X + edge0.X;
                point.Y = origin.Y + edge0.Y;
                point.Z = origin.Z + edge0.Z;
            }
            else if (i == 2)
            {
                point.X = origin.X + edge1.X;
                point.Y = origin.Y + edge1.Y;
                point.Z = origin.Z + edge1.Z;
            }
            else
            {
                point.X = origin.X;
                point.Y = origin.Y;
                point.Z = origin.Z;
            }
        }

        /// <summary>
        /// Returns the point parameterised by t0 and t1
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetPoint(float t0, float t1) 
        {
            return origin + t0 * edge0 + t1 * edge1;
        }

        /// <summary>
        /// Gets the minimum and maximum extents of the triangle along the axis
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="axis"></param>
        public void GetSpan(out float min, out float max, Vector3 axis)
        {
            float d0 = Vector3.Dot(GetPoint(0), axis);
            float d1 = Vector3.Dot(GetPoint(1), axis);
            float d2 = Vector3.Dot(GetPoint(2), axis);

            min = JiggleMath.Min(d0, d1, d2);
            max = JiggleMath.Max(d0, d1, d2);
        }

        // BEN-OPTIMISATION: New method, ref axis
        /// <summary>
        /// Gets the minimum and maximum extents of the triangle along the axis
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="axis"></param>
        public void GetSpan(out float min, out float max, ref Vector3 axis)
        {
            Vector3 point = new Vector3();

            GetPoint(ref point, 0);
            float d0 = point.X * axis.X + point.Y * axis.Y + point.Z * axis.Z;
            GetPoint(ref point, 1);
            float d1 = point.X * axis.X + point.Y * axis.Y + point.Z * axis.Z;
            GetPoint(ref point, 2);
            float d2 = point.X * axis.X + point.Y * axis.Y + point.Z * axis.Z;

            min = JiggleMath.Min(d0, d1, d2);
            max = JiggleMath.Max(d0, d1, d2);
        }

        /// <summary>
        /// Gets centre
        /// </summary>
        public Vector3 Centre
        {
            get { return origin + 0.333333333333f * (edge0 + edge1); }
        }

        /// <summary>
        /// Gets or Sets origin
        /// </summary>
        public Vector3 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        /// <summary>
        /// Gets or Sets edge0
        /// </summary>
        public Vector3 Edge0
        {
            get { return edge0; }
            set { edge0 = value; }
        }

        /// <summary>
        /// Gets or Sets edge1
        /// </summary>
        public Vector3 Edge1
        {
            get { return edge1; }
            set { edge1 = value; }
        }

        /// <summary>
        /// Edge2 goes from pt1 to pt2
        /// </summary>
        public Vector3 Edge2
        {
            get { return edge1 - edge0; }
        }


        /// <summary>
        /// Gets the plane containing the triangle
        /// </summary>
        public Plane Plane
        {
            get
            {
                return new Plane(GetPoint(0), GetPoint(1), GetPoint(2));
            }
        }

        /// <summary>
        /// Gets the triangle normal. If degenerate it will be normalised, but
        /// the direction may be wrong!
        /// </summary>
        public Vector3 Normal
        {
            get
            {
                Vector3 norm = Vector3.Cross(edge0, edge1);
                JiggleMath.NormalizeSafe(ref norm);

                return norm;
            }
        }
    }
}
