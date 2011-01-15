#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using JigLibOpenTK;
#endregion

namespace JigLibX.Math
{

    #region public struct Transform

    /// <summary>
    /// Transform is unneeded and should be removed soon. The XNA matrix4x4 can store
    /// the orientation and position.
    /// </summary>
    public struct Transform
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Orientation
        /// </summary>
        public Matrix4 Orientation;

        /// <summary>
        /// Gets net Tranform
        /// </summary>
        public static Transform Identity
        {
            get { return new Transform(Vector3.Zero, Matrix4.Identity); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position"></param>
        /// <param name="orientation"></param>
        public Transform(Vector3 position, Matrix4 orientation)
        {
            this.Position = position;
            this.Orientation = orientation;
        }

        /// <summary>
        /// ApplyTransformRate
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="dt"></param>
        public void ApplyTransformRate(ref TransformRate rate, float dt)
        {
            //Position += dt * rate.Velocity;
            Vector3 pos;
            Vector3.Multiply(ref rate.Velocity, dt, out pos);
            Vector3.Add(ref Position, ref pos, out Position);

            Vector3 dir = rate.AngularVelocity;
            float ang = dir.Length;

            if (ang > 0.0f)
            {
                Vector3.Divide(ref dir, ang, out dir);  // dir /= ang;
                ang *= dt;
                Matrix4 rot=Matrix4.CreateFromAxisAngle(dir, ang);
                Matrix4Extensions.Multiply(ref Orientation, ref rot, out Orientation);
            }

            //JiggleMath.Orthonormalise(ref this.Orientation);
        }

        /// <summary>
        /// ApplyTranformRate
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="dt"></param>
        public void ApplyTransformRate(TransformRate rate, float dt)
        {
            //Position += dt * rate.Velocity;
            Vector3 pos;
            Vector3.Multiply(ref rate.Velocity, dt, out pos);
            Vector3.Add(ref Position, ref pos, out Position);

            Vector3 dir = rate.AngularVelocity;
            float ang = dir.Length;

            if (ang > 0.0f)
            {
                Vector3.Divide(ref dir, ang, out dir);  // dir /= ang;
                ang *= dt;
                Matrix4 rot=Matrix4.CreateFromAxisAngle(dir, ang);
                Matrix4Extensions.Multiply(ref Orientation, ref rot, out Orientation);
            }

          //  JiggleMath.Orthonormalise(ref this.Orientation);
        }

        /// <summary>
        /// Operator overload of "*"
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>Tranform</returns>
        public static Transform operator *(Transform lhs, Transform rhs)
        {
            Transform result;
            Transform.Multiply(ref lhs, ref rhs, out result);
            return result;
        }

        /// <summary>
        /// Multiply
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>Transform</returns>
        public static Transform Multiply(Transform lhs, Transform rhs)
        {
            Transform result = new Transform();
            Matrix4Extensions.Multiply(ref rhs.Orientation, ref lhs.Orientation, out result.Orientation);
            //result.Orientation = rhs.Orientation * lhs.Orientation;
            Vector3Extensions.TransformNormal(ref rhs.Position, ref lhs.Orientation, out result.Position);
            Vector3.Add(ref lhs.Position, ref result.Position, out result.Position);
            //result.Position = lhs.Position + Vector3.Transform(rhs.Position, lhs.Orientation);

            return result;
        }

        /// <summary>
        /// Multiply
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="result"></param>
        public static void Multiply(ref Transform lhs, ref Transform rhs, out Transform result)
        {
            result = new Transform();

            Matrix4Extensions.Multiply(ref rhs.Orientation, ref lhs.Orientation, out result.Orientation);
            //result.Orientation = rhs.Orientation * lhs.Orientation;
            Vector3Extensions.TransformNormal(ref rhs.Position, ref lhs.Orientation, out result.Position);
            Vector3.Add(ref lhs.Position, ref result.Position, out result.Position);
            //result.Position = lhs.Position + Vector3.Transform(rhs.Position, lhs.Orientation);
        }

    }

    #endregion

    #region public struct TransformRate
    /// <summary>
    /// Stuct TransormRate
    /// </summary>
    public struct TransformRate
    {
        /// <summary>
        /// Velocity
        /// </summary>
        public Vector3 Velocity;
        /// <summary>
        /// AngularVelocity
        /// </summary>
        public Vector3 AngularVelocity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="angularVelocity"></param>
        public TransformRate(Vector3 velocity, Vector3 angularVelocity)
        {
            this.Velocity = velocity;
            this.AngularVelocity = angularVelocity;
        }

        /// <summary>
        /// Gets new TransformRate
        /// </summary>
        public static TransformRate Zero { get { return new TransformRate(); } }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="rate1"></param>
        /// <param name="rate2"></param>
        /// <returns>TransformRate</returns>
        public static TransformRate Add(TransformRate rate1, TransformRate rate2)
        {
            TransformRate result = new TransformRate();
            Vector3.Add(ref rate1.Velocity, ref rate2.Velocity, out result.Velocity);
            Vector3.Add(ref rate1.AngularVelocity, ref rate2.AngularVelocity, out result.AngularVelocity);
            return result;
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="rate1"></param>
        /// <param name="rate2"></param>
        /// <param name="result"></param>
        public static void Add(ref TransformRate rate1, ref TransformRate rate2 ,out TransformRate result)
        {
            Vector3.Add(ref rate1.Velocity, ref rate2.Velocity, out result.Velocity);
            Vector3.Add(ref rate1.AngularVelocity, ref rate2.AngularVelocity, out result.AngularVelocity);
        }

        //public static TransformRate operator +(TransformRate rate1, TransformRate rate2)
        //{
        //    return new TransformRate(rate1.Velocity + rate2.Velocity, rate1.AngularVelocity + rate2.AngularVelocity);
        //}


    }
    #endregion

}
