//Originally by Jon Watte.
//Released into the JigLibX project under the JigLibX license.
//Separately released into the public domain by the author.

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenTK;
using JigLibX.Geometry;
using JigLibOpenTK;
#endregion

namespace JigLibX.Collision
{

    /// <summary>
    /// Implementing a collision system (broad-phase test) based on the sweep-and-prune 
    /// algorithm
    /// </summary>
    public class CollisionSystemSAP : CollisionSystem, IComparer<CollisionSkin>
    {
        List<CollisionSkin> skins_ = new List<CollisionSkin>();
        bool dirty_;
        float largestX_;
        List<CollisionSkin> active_ = new List<CollisionSkin>();
        List<Primitive> testing_ = new List<Primitive>();
        List<Primitive> second_ = new List<Primitive>();
        /// <summary>
        /// Gets largestX_
        /// </summary>
        public float LargestX { get { return largestX_; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public CollisionSystemSAP()
        {
        }

        /// <summary>
        /// Adds a CollisionSkin
        /// </summary>
        /// <param name="collisionSkin"></param>
        public override void AddCollisionSkin(CollisionSkin collisionSkin)
        {
            collisionSkin.CollisionSystem = this;
            skins_.Add(collisionSkin);
            dirty_ = true;
            float dx = collisionSkin.WorldBoundingBox.Max.X - collisionSkin.WorldBoundingBox.Min.X;
            if (dx > largestX_)
                largestX_ = dx;
        }

        /// <summary>
        /// Removes a CollisionSkin
        /// </summary>
        /// <param name="collisionSkin"></param>
        /// <returns>bool</returns>
        public override bool RemoveCollisionSkin(CollisionSkin collisionSkin)
        {
            int ix = skins_.IndexOf(collisionSkin);
            if (ix >= skins_.Count || ix < 0)
                return false;
            skins_.RemoveAt(ix);
            return true;
        }

        /// <summary>
        /// Gets skins_.AsReadOnly()
        /// </summary>
        public override ReadOnlyCollection<CollisionSkin> CollisionSkins
        {
            get { return skins_.AsReadOnly(); }
        }

        /// <summary>
        /// CollisionSkin moved
        /// </summary>
        /// <param name="skin"></param>
        public override void CollisionSkinMoved(CollisionSkin skin)
        {
            dirty_ = true;
        }

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="skins"></param>
        void Extract(Vector3 min, Vector3 max, List<CollisionSkin> skins)
        {
            if (skins_.Count == 0)
                return;
            MaybeSort();
            int i = bsearch(min.X - largestX_);
            float xMax = max.X;
            while (i < skins_.Count && skins_[i].WorldBoundingBox.Min.X < xMax)
            {
                if (skins_[i].WorldBoundingBox.Max.X > min.X)
                    skins.Add(skins_[i]);
                ++i;
            }
        }

        /// <summary>
        /// bsearch
        /// </summary>
        /// <param name="x"></param>
        /// <returns>int</returns>
        int bsearch(float x)
        {
            //  It is up to the caller to make sure this isn't called on an empty collection.
            int top = skins_.Count;
            int bot = 0;
            while (top > bot)
            {
                int mid = (top + bot) >> 1;
                if (skins_[mid].WorldBoundingBox.Min.X >= x)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(top > mid);
#endif
                    top = mid;
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(bot <= mid);
#endif
                    bot = mid + 1;
                }
            }

#if DEBUG
            System.Diagnostics.Debug.Assert(top >= 0 && top <= skins_.Count);
            System.Diagnostics.Debug.Assert(top == 0 || skins_[top - 1].WorldBoundingBox.Min.X < x);
            System.Diagnostics.Debug.Assert(top == skins_.Count || skins_[top].WorldBoundingBox.Min.X >= x);
#endif

            return top;
        }

        /// <summary>
        /// DetectCollisions
        /// </summary>
        /// <param name="body"></param>
        /// <param name="collisionFunctor"></param>
        /// <param name="collisionPredicate"></param>
        /// <param name="collTolerance"></param>
        public override void DetectCollisions(JigLibX.Physics.Body body, CollisionFunctor collisionFunctor, CollisionSkinPredicate2 collisionPredicate, float collTolerance)
        {
            if (!body.IsActive)
                return;

            CollDetectInfo info = new CollDetectInfo();
            info.Skin0 = body.CollisionSkin;
            if (info.Skin0 == null)
                return;

            active_.Clear();
            testing_.Clear();
            Extract(info.Skin0.WorldBoundingBox.Min, info.Skin0.WorldBoundingBox.Max, active_);

            for (int j = 0, m = info.Skin0.NumPrimitives; j != m; ++j)
                testing_.Add(info.Skin0.GetPrimitiveNewWorld(j));

            int nBodyPrims = testing_.Count;

            for (int i = 0, n = active_.Count; i != n; ++i)
            {
                info.Skin1 = active_[i];
                if (info.Skin0 != info.Skin1 && (collisionPredicate == null ||
                    collisionPredicate.ConsiderSkinPair(info.Skin0, info.Skin1)))
                {
                    int nPrim1 = info.Skin1.NumPrimitives;
                    second_.Clear();
                    for (int k = 0; k != nPrim1; ++k)
                        second_.Add(info.Skin1.GetPrimitiveNewWorld(k));
                    for (info.IndexPrim0 = 0; info.IndexPrim0 != nBodyPrims; ++info.IndexPrim0)
                    {
                        for (info.IndexPrim1 = 0; info.IndexPrim1 != nPrim1; ++info.IndexPrim1)
                        {
                            DetectFunctor f =
                              GetCollDetectFunctor(info.Skin0.GetPrimitiveNewWorld(info.IndexPrim0).Type,
                                info.Skin1.GetPrimitiveNewWorld(info.IndexPrim1).Type);
                            if (f != null)
                                f.CollDetect(info, collTolerance, collisionFunctor);
                        }
                    }
                }
            }
        }

        SkinTester skinTester_ = new SkinTester();

        /// <summary>
        /// DetectAllCollisions
        /// </summary>
        /// <param name="bodies"></param>
        /// <param name="collisionFunctor"></param>
        /// <param name="collisionPredicate"></param>
        /// <param name="collTolerance"></param>
        public override void DetectAllCollisions(List<JigLibX.Physics.Body> bodies, CollisionFunctor collisionFunctor, CollisionSkinPredicate2 collisionPredicate, float collTolerance)
        {
            skinTester_.Set(this, collisionFunctor, collisionPredicate, collTolerance);

            MaybeSort();
            //  I know that each skin for the bodies is already in my list of skins.
            //  Thus, I can do collision between all skins, culling out non-active bodies.
            int nSkins = skins_.Count;
            active_.Clear();

            // BEN-OPTIMISATION: unsafe, remove array boundary checks.
            unsafe
            {
                for (int i = 0; i != nSkins; ++i)
                    AddToActive(skins_[i], skinTester_);
            }
        }

        /// <summary>
        /// Class SkinTester
        /// </summary>
        class SkinTester : CollisionSkinPredicate2
        {
            CollisionFunctor collisionFunctor_;
            CollisionSkinPredicate2 collisionPredicate_;
            float collTolerance_;
            CollDetectInfo info_;
            CollisionSystem sys_;

            /// <summary>
            /// Constructor
            /// </summary>
            internal SkinTester()
            {
            }

            /// <summary>
            /// Set
            /// </summary>
            /// <param name="sys"></param>
            /// <param name="collisionFunctor"></param>
            /// <param name="collisionPredicate"></param>
            /// <param name="collTolerance"></param>
            internal void Set(CollisionSystem sys, CollisionFunctor collisionFunctor, CollisionSkinPredicate2 collisionPredicate, float collTolerance)
            {
                sys_ = sys;
                collisionFunctor_ = collisionFunctor;
                collisionPredicate_ = collisionPredicate;
                if (collisionPredicate_ == null)
                    collisionPredicate_ = this;
                collTolerance_ = collTolerance;
            }

            /// <summary>
            /// CheckCollidables
            /// </summary>
            /// <param name="skin0"></param>
            /// <param name="skin1"></param>
            /// <returns>bool</returns>
            private static bool CheckCollidables(CollisionSkin skin0,CollisionSkin skin1)
            {
                List<CollisionSkin> nonColl0 = skin0.NonCollidables;
                List<CollisionSkin> nonColl1 = skin1.NonCollidables;

                // Most common case
                if (nonColl0.Count == 0 && nonColl1.Count == 0)
                    return true;

                for (int i0 = nonColl0.Count; i0-- != 0; )
                {
                    if (nonColl0[i0] == skin1)
                        return false;
                }

                for (int i1 = nonColl1.Count; i1-- != 0; )
                {
                    if (nonColl1[i1] == skin0)
                        return false;
                }

                return true;
            }

            // BEN-OPTIMISATION: unsafe i.e. Remove array boundary checks.
            /// <summary>
            /// TestSkin
            /// </summary>
            /// <param name="b"></param>
            /// <param name="s"></param>
            internal unsafe void TestSkin(CollisionSkin b, CollisionSkin s)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(b.Owner != null);
                System.Diagnostics.Debug.Assert(b.Owner.IsActive);
#endif
                if (!collisionPredicate_.ConsiderSkinPair(b, s))
                    return;

                info_.Skin0 = b;
                info_.Skin1 = s;
                int nSkin0 = info_.Skin0.NumPrimitives;
                int nSkin1 = info_.Skin1.NumPrimitives;

                // BEN-OPTIMISATION: Reuse detectFunctor.
                DetectFunctor detectFunctor;
                for (info_.IndexPrim0 = 0; info_.IndexPrim0 != nSkin0; ++info_.IndexPrim0)
                {
                    for (info_.IndexPrim1 = 0; info_.IndexPrim1 != nSkin1; ++info_.IndexPrim1)
                    {
                        if (CheckCollidables(info_.Skin0, info_.Skin1))
                        {
                            detectFunctor = sys_.GetCollDetectFunctor(
                                info_.Skin0.GetPrimitiveNewWorld(info_.IndexPrim0).Type,
                                info_.Skin1.GetPrimitiveNewWorld(info_.IndexPrim1).Type);

                            if (detectFunctor != null)
                                detectFunctor.CollDetect(info_, collTolerance_, collisionFunctor_);
                        }
                    }
                }
            }
            /// <summary>
            /// ConsiderSkinPair
            /// </summary>
            /// <param name="skin0"></param>
            /// <param name="skin1"></param>
            /// <returns>bool</returns>
            public override bool ConsiderSkinPair(CollisionSkin skin0, CollisionSkin skin1)
            {
                return true;
            }
        }

        /// <summary>
        /// AddToActive
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="st"></param>
        void AddToActive(CollisionSkin cs, SkinTester st)
        {
            int n = active_.Count;
            float xMin = cs.WorldBoundingBox.Min.X;
            bool active = (cs.Owner != null) && cs.Owner.IsActive;
            // BEN-OPTIMISATION: unsafe i.e. Remove array boundary checks.
            unsafe
            {
                CollisionSkin asi;
                for (int i = 0; i != n; )
                {
                    asi = active_[i];
                    if (asi.WorldBoundingBox.Max.X < xMin)
                    {
                        //  prune no longer interesting boxes from potential overlaps
                        --n;
                        active_.RemoveAt(i);
                    }
                    else
                    {
                        // BEN-OPTIMISATION: Inlined BoundingBoxHelper.OverlapTest() and removed two redundant
                        //                   comparisons the X comparison and the extra "if (active)" which can
                        //                   be removed by rearranging.
                        if (active)
                        {
                            if (!((cs.WorldBoundingBox.Min.Z >= asi.WorldBoundingBox.Max.Z) ||
                                    (cs.WorldBoundingBox.Max.Z <= asi.WorldBoundingBox.Min.Z) ||
                                    (cs.WorldBoundingBox.Min.Y >= asi.WorldBoundingBox.Max.Y) ||
                                    (cs.WorldBoundingBox.Max.Y <= asi.WorldBoundingBox.Min.Y) ||
                                    (cs.WorldBoundingBox.Max.X <= asi.WorldBoundingBox.Min.X)))
                            {
                                st.TestSkin(cs, asi);
                            }
                        }
                        else if (active_[i].Owner != null && asi.Owner.IsActive
                                && !((cs.WorldBoundingBox.Min.Z >= asi.WorldBoundingBox.Max.Z) ||
                                    (cs.WorldBoundingBox.Max.Z <= asi.WorldBoundingBox.Min.Z) ||
                                    (cs.WorldBoundingBox.Min.Y >= asi.WorldBoundingBox.Max.Y) ||
                                    (cs.WorldBoundingBox.Max.Y <= asi.WorldBoundingBox.Min.Y) ||
                                    (cs.WorldBoundingBox.Max.X <= asi.WorldBoundingBox.Min.X)))
                        {
                            st.TestSkin(asi, cs);
                        }
                        ++i;
                    }
                }
            }
            active_.Add(cs);
        }

        /// <summary>
        /// SegmentIntersect
        /// </summary>
        /// <param name="fracOut"></param>
        /// <param name="skinOut"></param>
        /// <param name="posOut"></param>
        /// <param name="normalOut"></param>
        /// <param name="seg"></param>
        /// <param name="collisionPredicate"></param>
        /// <returns>bool</returns>
        public override bool SegmentIntersect(out float fracOut, out CollisionSkin skinOut, out Vector3 posOut, out Vector3 normalOut, JigLibX.Geometry.Segment seg, CollisionSkinPredicate1 collisionPredicate)
        {
            fracOut = float.MaxValue;
            skinOut = null;
            posOut = normalOut = Vector3.Zero;

            float frac;
            Vector3 pos;
            Vector3 normal;

            Vector3 segmentBeginning = seg.Origin;
            Vector3 segmentEnd = seg.Origin + seg.Delta;

            Vector3 min = Vector3Extensions.Min(segmentBeginning, segmentEnd);
            Vector3 max = Vector3Extensions.Max(segmentBeginning, segmentEnd);

            active_.Clear();

            BoundingBox box = new BoundingBox(min, max);
            Extract(min, max, active_);

            float distanceSquared = float.MaxValue;
            int nActive = active_.Count;
            for (int i = 0; i != nActive; ++i)
            {
                CollisionSkin skin = active_[i];
                if (collisionPredicate == null || collisionPredicate.ConsiderSkin(skin))
                {
                    if (BoundingBoxHelper.OverlapTest(ref box, ref skin.WorldBoundingBox))
                    {
                        if (skin.SegmentIntersect(out frac, out pos, out normal, seg))
                        {
                            if (frac >= 0)
                            {
                                float newDistanceSquared = Vector3Extensions.DistanceSquared(segmentBeginning, pos);
                                if (newDistanceSquared < distanceSquared)
                                {
                                    distanceSquared = newDistanceSquared;

                                    fracOut = frac;
                                    skinOut = skin;
                                    posOut = pos;
                                    normalOut = normal;
                                }
                            }
                        }
                    }
                }
            }
            
            return (fracOut <= 1);
        }

        /// <summary>
        /// MaybeSort
        /// </summary>
        void MaybeSort()
        {
            if (dirty_)
            {
                skins_.Sort(this);
                dirty_ = false;
            }
        }

        /// <summary>
        /// Compare
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>int</returns>
        public int Compare(CollisionSkin x, CollisionSkin y)
        {
            float f = x.WorldBoundingBox.Min.X - y.WorldBoundingBox.Min.X;
            return (f < 0) ? -1 : (f > 0) ? 1 : 0;
        }
    }
}