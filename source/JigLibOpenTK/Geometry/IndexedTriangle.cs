#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Diagnostics;
#endregion

namespace JigLibX.Geometry
{
    /// <summary>
    /// Support for an indexed triangle - assumes ownership by something that 
    /// has an array of vertices and an array of tIndexedTriangle
    /// </summary>
    public struct IndexedTriangle
    {
        // used when traversing to stop us visiting the same triangle twice
        internal int counter;
        /// indices into our owner's array of vertices 
        private int vertexIndices0;
        private int vertexIndices1;
        private int vertexIndices2;
        private int convexFlags;
        private JigLibOpenTK.Plane plane;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <param name="vertexArray"></param>
        public IndexedTriangle(int i0, int i1, int i2, List<Vector3> vertexArray)
        {
            counter = 0;
            vertexIndices0 = i0;
            vertexIndices1 = i1;
            vertexIndices2 = i2;

            convexFlags = unchecked((ushort)~0); // TODO check this
            plane = new JigLibOpenTK.Plane(vertexArray[i0], vertexArray[i1], vertexArray[i2]);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <param name="vertexArray"></param>
        public IndexedTriangle(int i0, int i1, int i2, Vector3[] vertexArray)
        {
            counter = 0;

            vertexIndices0 = i0;
            vertexIndices1 = i1;
            vertexIndices2 = i2;

            convexFlags = unchecked((ushort)~0); // TODO check this
            plane = new JigLibOpenTK.Plane(vertexArray[i0], vertexArray[i1], vertexArray[i2]);
        }

        /// <summary>
        /// Set the indices into the relevant vertex array for this
        /// triangle. Also sets the plane and bounding box
        /// </summary>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <param name="vertexArray"></param>
        public void SetVertexIndices(int i0, int i1, int i2, List<Vector3> vertexArray)
        {
            vertexIndices0 = i0;
            vertexIndices1 = i1;
            vertexIndices2 = i2;

            plane = new JigLibOpenTK.Plane(vertexArray[i0], vertexArray[i1], vertexArray[i2]);
            
        }
        /// <summary>
        /// Set the indices into the relevant vertex array for this
        /// triangle. Also sets the plane and bounding box
        /// </summary>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <param name="vertexArray"></param>
        public void SetVertexIndices(int i0, int i1, int i2, Vector3[] vertexArray)
        {
            vertexIndices0 = i0;
            vertexIndices1 = i1;
            vertexIndices2 = i2;

            plane = new JigLibOpenTK.Plane(vertexArray[i0], vertexArray[i1], vertexArray[i2]);
        }
        /// <summary>
        /// Get the indices into the relevant vertex array for this triangle.
        /// </summary>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        public void GetVertexIndices(out int i0, out int i1, out int i2)
        {
            i0 = vertexIndices0;
            i1 = vertexIndices1;
            i2 = vertexIndices2;
        }

        /// <summary>
        /// Get the vertex index association with iCorner (which should be 0, 1, or 2)
        /// </summary>
        /// <param name="iCorner"></param>
        /// <returns>int</returns>
        public int GetVertexIndex(int iCorner)
        {
            switch (iCorner)
            {
                case 0:
                    return vertexIndices0;
                case 1:
                    return vertexIndices1;
                case 2:
                    return vertexIndices2;
                default:
                    Debug.Assert(false);
                    return vertexIndices0;
            }
        }

        /// <summary>
        /// Gets the triangle plane
        /// </summary>
        public JigLibOpenTK.Plane Plane
        {
            get { return plane; }
        }

        /// <summary>
        /// Has the edge been marked as convex. Same convention for edge numbering as in tTriangle
        /// </summary>
        /// <param name="iEdge"></param>
        /// <returns>bool</returns>
        public bool IsEdgeConvex(int iEdge)
        {
            return 0 != (convexFlags & (1 << iEdge));
        }

        /// <summary>
        /// SetEdgeConvex
        /// </summary>
        /// <param name="iEdge"></param>
        /// <param name="convex"></param>
        public void SetEdgeConvex(int iEdge, bool convex)
        {
            if (convex)
                convexFlags |= (ushort)(1 << iEdge);
            else
                convexFlags &= (ushort)~(1 << iEdge);
        }

        /// <summary>
        /// Has the point been marked as convex. Samve convention for point numbering as in tTriangle
        /// </summary>
        /// <param name="iPoint"></param>
        /// <returns>bool</returns>
        public bool IsPointConvex(int iPoint)
        {
            return 0 != (convexFlags & (1 << (iPoint + 3)));
        }

        /// <summary>
        /// SetPointConvex
        /// </summary>
        /// <param name="iPoint"></param>
        /// <param name="convex"></param>
        public void SetPointConvex(int iPoint, bool convex)
        {
            if (convex)
                convexFlags |= (ushort)(1 << (iPoint + 3));
            else
                convexFlags &= (ushort)~(1 << (iPoint + 3));
        }

    }

    /// <summary>
    /// Structure used to set up the mesh
    /// </summary>
    public struct TriangleVertexIndices
    {
        /// <summary>
        /// Integers I0, I1, I2
        /// </summary>
        public int I0, I1, I2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        public TriangleVertexIndices(int i0, int i1, int i2) 
        {
            this.I0 = i0; 
            this.I1 = i1; 
            this.I2 = i2;
        }

        /// <summary>
        /// Set
        /// </summary>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        public void Set(int i0, int i1, int i2)
        {
            I0 = i0; I1 = i1; I2 = i2;
        }
    }

}
