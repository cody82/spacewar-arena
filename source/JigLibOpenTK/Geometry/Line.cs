#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using JigLibX.Math;
#endregion

namespace JigLibX.Geometry
{

    #region public struct Line
    /// <summary>
    /// A line goes through pos, and extends infinitely far in both
    /// directions along dir.
    /// </summary>
    public struct Line
    {
        /// <summary>
        /// Origin
        /// </summary>
        public Vector3 Origin;
        /// <summary>
        /// Direction
        /// </summary>
        public Vector3 Dir;

        /// <summary>
        /// Consructor
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        public Line(Vector3 origin, Vector3 dir)
        {
            this.Origin = origin;
            this.Dir = dir;
        }

        /// <summary>
        /// GetOrigin
        /// </summary>
        /// <param name="t"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetOrigin(float t)
        {
            return new Vector3(
                Origin.X + t * Dir.X,
                Origin.Y + t * Dir.Y,
                Origin.Z + t * Dir.Z);
            //return this.Origin + t * this.Dir;
        }
    }
    #endregion

    #region public struct Ray
    /// <summary>
    /// A Ray is just a line that extends in the +ve direction
    /// </summary>
    public struct Ray
    {
        /// <summary>
        /// Origin
        /// </summary>
        public Vector3 Origin;
        /// <summary>
        /// Direction
        /// </summary>
        public Vector3 Dir;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        public Ray(Vector3 origin,Vector3 dir)
        {
            this.Origin = origin;
            this.Dir = dir;
        }

        /// <summary>
        /// GetOrigin
        /// </summary>
        /// <param name="t"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetOrigin(float t)
        {
            return new Vector3(
                Origin.X + t * Dir.X,
                Origin.Y + t * Dir.Y,
                Origin.Z + t * Dir.Z);

            //return this.Origin + t * this.Dir;
        }
    }
    #endregion

    #region public struct Segment
    /// <summary>
    /// A Segment is a line that starts at origin and goes only as far as
    /// (origin + delta).
    /// </summary>
    public struct Segment
    {
        /// <summary>
        /// Origin
        /// </summary>
        public Vector3 Origin;
        /// <summary>
        /// Direction
        /// </summary>
        public Vector3 Delta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="delta"></param>
        public Segment(Vector3 origin, Vector3 delta)
        {
            this.Origin = origin;
            this.Delta = delta;
        }

        // BEN-OPTIMISATION: New method, ref origin and ref delta
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="delta"></param>
        public Segment(ref Vector3 origin, ref Vector3 delta)
        {
            this.Origin = origin;
            this.Delta = delta;
        }

        /// <summary>
        /// GetPoint
        /// </summary>
        /// <param name="t"></param>
        /// <param name="point"></param>
        public void GetPoint(float t, out Vector3 point)
        {
            point = new Vector3(
                t * Delta.X,
                t * Delta.Y,
                t * Delta.Z);

            point.X += Origin.X;
            point.Y += Origin.Y;
            point.Z += Origin.Z;
        }

        /// <summary>
        /// GetPoint
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetPoint(float t)
        {
            Vector3 result = new Vector3(
                t * Delta.X,
                t * Delta.Y,
                t * Delta.Z);

            result.X += Origin.X;
            result.Y += Origin.Y;
            result.Z += Origin.Z;
            
            return result;
        }

        // BEN-OPTIMISATION: New method, ref point.
        /// <summary>
        /// GetPoint
        /// </summary>
        /// <param name="point"></param>
        /// <param name="t"></param>
        public void GetPoint(ref Vector3 point, float t)
        {
            point.X = t * Delta.X + Origin.X;
            point.Y = t * Delta.Y + Origin.Y;
            point.Z = t * Delta.Z + Origin.Z;
        }

        /// <summary>
        /// GetEnd
        /// </summary>
        /// <returns>Vector3</returns>
        public Vector3 GetEnd()
        {
            return new Vector3(
                Delta.X + Origin.X,
                Delta.Y + Origin.Y,
                Delta.Z + Origin.Z);
            //return Origin + Delta;
        }

    }
    #endregion

}
