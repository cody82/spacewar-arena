#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
#endregion

namespace JigLibX.Geometry
{
    /// <summary>
    /// Class Rectangle
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// Origin
        /// </summary>
        public Vector3 Origin;
        /// <summary>
        /// Edge0
        /// </summary>
        public Vector3 Edge0;
        /// <summary>
        /// Edge1
        /// </summary>
        public Vector3 Edge1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="edge0"></param>
        /// <param name="edge1"></param>
        public Rectangle(Vector3 origin, Vector3 edge0, Vector3 edge1)
        {
            this.Origin = origin;
            this.Edge0 = edge0;
            this.Edge1 = edge1;
        }

        /// <summary>
        /// GetPoint
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetPoint(float t0, float t1)
        {
            return Origin + t0 * Edge0 + t1 * Edge1;
        }
        
    }
}
