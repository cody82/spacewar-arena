#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace JigLibX.Collision
{
    /// <summary>
    /// Used during setup - allow the creator to register functors to do
    /// the actual collision detection. Each functor inherits from this
    /// - has a name to help debugging!  The functor has to be able to
    /// handle the primitivs being passed to it in either order.
    /// </summary>
    public abstract class DetectFunctor
    {
        private int type0, type1;
        private string name;
        /// <summary>
        /// Gets name
        /// </summary>
        public string Name { get { return this.name; } }
        /// <summary>
        /// Gets type0
        /// </summary>
        public int Type0 { get { return this.type0; } }
        /// <summary>
        /// Gets type1
        /// </summary>
        public int Type1 { get { return this.type1; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="primType0"></param>
        /// <param name="primType1"></param>
        public DetectFunctor(string name, int primType0, int primType1)
        {
            this.name = name;
            this.type0 = primType0;
            this.type1 = primType1;
        }

        /// <summary>
        /// CollDetect
        /// </summary>
        /// <param name="info"></param>
        /// <param name="collTolerance"></param>
        /// <param name="collisionFunctor"></param>
        public abstract void CollDetect(CollDetectInfo info, float collTolerance, CollisionFunctor collisionFunctor);
        /// <summary>
        /// Set at 2048
        /// </summary>
        public const int MaxLocalStackTris = 2048;
        /// <summary>
        /// Set at 10
        /// </summary>
        public const int MaxLocalStackSCPI = 10;
        const int InitialLocalStackDepth = 10;
        
        static Stack<int[]> freeInts = new Stack<int[]>();
        static Stack<SmallCollPointInfo[]> freeSCPIs = new Stack<SmallCollPointInfo[]>();
        static DetectFunctor()
        {
            for (int i = 0; i < InitialLocalStackDepth; ++i)
            {
                freeInts.Push(new int[MaxLocalStackTris]);
                freeSCPIs.Push(new SmallCollPointInfo[MaxLocalStackSCPI]);
            }
        }

        /// <summary>
        /// IntStackAlloc
        /// </summary>
        /// <returns>int[]</returns>
        public static int[] IntStackAlloc()
        {
            if (freeInts.Count == 0)
            {
                // BEN-OPTIMISATION: Pushing a popping is a slow operation, don't do it unnecessarily.
                return (new int[MaxLocalStackTris]);
            }
            return freeInts.Pop();
        }

        /// <summary>
        /// FreeStackAlloc
        /// </summary>
        /// <param name="alloced"></param>
        public static void FreeStackAlloc(int[] alloced)
        {
            freeInts.Push(alloced);
        }
        /// <summary>
        /// SCPIStackAlloc
        /// </summary>
        /// <returns>SmallCollPointInfo[]</returns>
        public static SmallCollPointInfo[] SCPIStackAlloc()
        {
            if (freeSCPIs.Count == 0)
            {
                // BEN-OPTIMISATION: Pushing a popping is a slow operation, don't do it unnecessarily.
                return (new SmallCollPointInfo[MaxLocalStackSCPI]);
            }
            return freeSCPIs.Pop();
        }

        /// <summary>
        /// FreeStackAlloc
        /// </summary>
        /// <param name="alloced"></param>
        public static void FreeStackAlloc(SmallCollPointInfo[] alloced)
        {
            freeSCPIs.Push(alloced);
        }

    }
}
