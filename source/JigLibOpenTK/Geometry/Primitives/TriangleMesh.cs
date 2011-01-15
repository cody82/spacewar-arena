#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using JigLibX.Math;
using JigLibX.Collision;
using JigLibOpenTK;
#endregion

namespace JigLibX.Geometry
{
    /// <summary>
    /// Class TriangleMesh
    /// </summary>
    public class TriangleMesh : Primitive
    {
        private Octree octree = new Octree();

        private int maxTrianglesPerCell;
        private float minCellSize;

        /// <summary>
        /// Constructor
        /// </summary>
        public TriangleMesh()
            : base((int)PrimitiveType.TriangleMesh)
        {
        }

        /// <summary>
        /// CreateMesh
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="triangleVertexIndices"></param>
        /// <param name="maxTrianglesPerCell"></param>
        /// <param name="minCellSize"></param>
        public void CreateMesh(List<Vector3> vertices,
            List<TriangleVertexIndices> triangleVertexIndices,
            int maxTrianglesPerCell, float minCellSize)
        {
            int numVertices = vertices.Count;

            octree.Clear(true);
            octree.AddTriangles(vertices, triangleVertexIndices);
            octree.BuildOctree(maxTrianglesPerCell, minCellSize);

            this.maxTrianglesPerCell = maxTrianglesPerCell;
            this.minCellSize = minCellSize;
        }

        /// <summary>
        /// GetBoundingBox
        /// </summary>
        /// <param name="box"></param>
        public override void GetBoundingBox(out AABox box)
        {
            box = octree.BoundingBox.Clone() as AABox;
            box.Transform = Transform;
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
                transformMatrix.Row3 = new Vector4(transform.Position, transform.Orientation.Row3.W);
                invTransform = Matrix4Extensions.Invert(transformMatrix);
            }
        }

        /// <summary>
        /// Use a cached version as this occurs ALOT during triangle mesh traversal
        /// </summary>
        public override Matrix4 TransformMatrix
        {
            get
            {
                return transformMatrix;
            }
        }

        /// <summary>
        /// Use a cached version as this occurs ALOT during triangle mesh traversal
        /// </summary>
        public override Matrix4 InverseTransformMatrix
        {
            get
            {
                return invTransform;
            }
        }

        /// <summary>
        /// Gets octree
        /// </summary>
        public Octree Octree
        {
            get { return this.octree; }
        }

        /// <summary>
        /// GetNumTriangles
        /// </summary>
        /// <returns>int</returns>
        public int GetNumTriangles()
        {
            return octree.NumTriangles;
        }

        /// <summary>
        /// GetTriangle
        /// </summary>
        /// <param name="iTriangle"></param>
        /// <returns>IndexedTriangle</returns>
        public IndexedTriangle GetTriangle(int iTriangle)
        {
            return octree.GetTriangle(iTriangle);
        }

        /// <summary>
        /// GetVertex
        /// </summary>
        /// <param name="iVertex"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetVertex(int iVertex)
        {
            return octree.GetVertex(iVertex);
        }

        /// <summary>
        /// GetVertex
        /// </summary>
        /// <param name="iVertex"></param>
        /// <param name="result"></param>
        public void GetVertex(int iVertex, out Vector3 result)
        {
            result = octree.GetVertex(iVertex);
        }

        // BEN-PATCH: Someone else's patch... needs testing but the idea seems correct.
        /// <summary>
        /// GetTrianglesIntersectingtAABox
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="maxTriangles"></param>
        /// <param name="bb"></param>
        /// <returns>int</returns>
        public unsafe int GetTrianglesIntersectingtAABox(int* triangles, int maxTriangles, ref BoundingBox bb)
        {
            // rotated aabb
            BoundingBox rotBB = bb;

            Vector3 bbCorner = new Vector3();
            Vector3 bbCornerT;

            for (int a = 0; a < 2; a++)
            {
                for (int b = 0; b < 2; b++)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        bbCorner.X = ((a == 0) ? bb.Min.X : bb.Max.X);
                        bbCorner.Y = ((b == 0) ? bb.Min.Y : bb.Max.Y);
                        bbCorner.Z = ((c == 0) ? bb.Min.Z : bb.Max.Z);
                        bbCornerT = Vector3.Transform(bbCorner, invTransform);

                        BoundingBoxHelper.AddPoint(ref bbCornerT, ref rotBB);
                    }
                }
            }
            return octree.GetTrianglesIntersectingtAABox(triangles, maxTriangles, ref rotBB);
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>Primitive</returns>
        public override Primitive Clone()
        {
            TriangleMesh triangleMesh = new TriangleMesh();
            //            triangleMesh.CreateMesh(vertices, triangleVertexIndices, maxTrianglesPerCell, minCellSize);
            // its okay to share the octree
            triangleMesh.octree = this.octree;
            triangleMesh.Transform = Transform;
            return triangleMesh;
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
            // move segment into octree space
            seg.Origin = Vector3.Transform(seg.Origin, invTransform);
            seg.Delta = Vector3Extensions.TransformNormal(seg.Delta, invTransform);


            BoundingBox segBox = BoundingBoxHelper.InitialBox;
            BoundingBoxHelper.AddSegment(seg, ref segBox);

            unsafe
            {
#if USE_STACKALLOC
                int* potentialTriangles = stackalloc int[MaxLocalStackTris];
                {
#else
                int[] potTriArray = DetectFunctor.IntStackAlloc();
                fixed (int* potentialTriangles = potTriArray)
                {
#endif
                    int numTriangles = GetTrianglesIntersectingtAABox(potentialTriangles, DetectFunctor.MaxLocalStackTris, ref segBox);

                    float tv1, tv2;

                    pos = Vector3.Zero;
                    normal = Vector3.Zero;

                    float bestFrac = float.MaxValue;
                    for (int iTriangle = 0; iTriangle < numTriangles; ++iTriangle)
                    {
                        IndexedTriangle meshTriangle = GetTriangle(potentialTriangles[iTriangle]);
                        float thisFrac;
                        Triangle tri = new Triangle(GetVertex(meshTriangle.GetVertexIndex(0)),
                          GetVertex(meshTriangle.GetVertexIndex(1)),
                          GetVertex(meshTriangle.GetVertexIndex(2)));

                        if (Intersection.SegmentTriangleIntersection(out thisFrac, out tv1, out tv2, seg, tri))
                        {
                            if (thisFrac < bestFrac)
                            {
                                bestFrac = thisFrac;
                                // re-project
                                pos = Vector3.Transform(seg.GetPoint(thisFrac), transformMatrix);
                                normal = Vector3Extensions.TransformNormal(meshTriangle.Plane.Normal, transformMatrix);
                            }
                        }
                    }

                    frac = bestFrac;
                    if (bestFrac < float.MaxValue)
                    {
                        DetectFunctor.FreeStackAlloc(potTriArray);
                        return true;
                    }
                    else
                    {
                        DetectFunctor.FreeStackAlloc(potTriArray);
                        return false;
                    }
#if USE_STACKALLOC
                }
#else
                }
#endif
            }
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
    }
}
