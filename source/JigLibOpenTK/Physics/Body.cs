#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using JigLibX.Collision;
using JigLibX.Math;
using JigLibX.Utils;
using JigLibOpenTK;
#endregion

namespace JigLibX.Physics
{

    /// <summary>
    /// Basic rigid body - can be used as is, or a derived class can
    /// over-ride certain behaviours.
    /// A body can only be added to one physics system at a time!
    /// </summary>
    public partial class Body
    {

        private enum Activity
        {
            Active, Inactive
        }

        private bool bodyEnabled;

        internal bool foundIsland = false;

        // Don't actually own the skin
        private CollisionSkin collSkin;

        // Prevent velocity updates etc
        private bool immovable;

        // the "working" state
        internal Transform transform = new Transform();
        internal TransformRate transformRate = new TransformRate();
        internal TransformRate transformRateAux = new TransformRate();

        internal Transform oldTransform;
        internal TransformRate oldTransformRate;

        // Flag that gets set whenever our velocity/angular velocity might have been changed.
        // Used by physics to speed up checking for activation
        private bool velChanged;
        private float invMass;

        // the previous state - copied explicitly using CopyCurrentStateToOld.
        private Transform storedTransform;
        private TransformRate storedTransformRate;

        private Matrix4 invOrientation;
        private float mass;
        private bool origImmovable;
        private bool doShockProcessing;

        // internal, so we can call them by reference
        internal Matrix4 bodyInertia;     // Inertia in body Space (not necessarily diagonal)
        internal Matrix4 worldInertia;    // inertia tensor in world space
        internal Matrix4 bodyInvInertia;  // inverse inertia in body frame
        internal Matrix4 worldInvInertia; // Inverse inertia in world frame

        private Vector3 force;          // force etc in world frame
        private Vector3 torque;         // torque in world frame

        private Activity activity;      // for deactivation
        private float inactiveTime;     // gow long we've been still

        // last position for when trying the deactivate
        private Vector3 lastPositionForDeactivation;

        // last orientation for when trying to deactivate
        private Matrix4 lastOrientationForDeactivation;

        // How long it takes to go from active to frozen when stationary.
        private float deactivationTime;

        // Velocity below which we're considered still
        private float sqVelocityActivityThreshold;

        // Velocity below which we're considered still - in (radians per sec)^2
        private float sqAngVelocityActivityThreshold;

        // The position stored stored when we need to notify other bodies
        private Vector3 storedPositionForActivation;

        // The list of bodies that need to be activated when we move away from
        // our stored position
        private List<Body> bodiesToBeActivatedOnMovement;

        // Whether this body can freeze (assuming physics freezing is enabled)
        private bool allowFreezing;

        // List of constraint that act on this body
        private List<Constraint> constraints = new List<Constraint>();

        // Helper to stop the velocities getting silly.
        private const float VelMax = 100.0f;
        private const float AngVelMax = 50.0f;

        // wether gravity should be applied
        private bool applyGravity = true;

        /// <summary>
        /// ExternalData
        /// </summary>
        public object ExternalData;

        static int idCounter;
        internal int ID;

        /// <summary>
        /// Constructor
        /// </summary>
        public Body()
        {
            this.ID = idCounter++;

            bodiesToBeActivatedOnMovement = new List<Body>();
            bodyEnabled = false;
            collSkin = null;

            this.Mass = 1.0f;
            this.BodyInertia = Matrix4.Identity;

            transform = new Transform();
            transform.Position = Vector3.Zero;
            transform.Orientation = Matrix4.Identity;

            immovable = false;
            origImmovable = false;
            doShockProcessing = true;

            force = Vector3.Zero;
            torque = Vector3.Zero;

            velChanged = true;

            activity = Activity.Active;
            inactiveTime = 0.0f;
            deactivationTime = 1.0f;
            SetActivityThreshold(0.5f, 20.0f);
            allowFreezing = true;
            lastPositionForDeactivation = transform.Position;
            lastOrientationForDeactivation = transform.Orientation;

            CopyCurrentStateToOld();
        }

        /// <summary>
        /// Gets or Sets transform
        /// </summary>
        public Transform Transform { get { return this.transform; } set { this.transform = value; } }

        /// <summary>
        /// Gets or Sets transformRate
        /// </summary>
        public TransformRate TransformRate { get { return this.transformRate; } set { this.transformRate = value; } }

        /// <summary>
        /// Gets oldTransform
        /// </summary>
        public Transform OldTransform { get { return oldTransform; } }

        /// <summary>
        /// Gets oldTransformRate
        /// </summary>
        public TransformRate OldTransformRate { get { return oldTransformRate; } }

        /// <summary>
        /// Gets or Sets transform.Position
        /// </summary>
        public Vector3 Position { get { return this.transform.Position; } set { this.transform.Position = value; } }

        /// <summary>
        /// Gets oldTransform.Position
        /// </summary>
        public Vector3 OldPosition { get { return this.oldTransform.Position; } }

        /// <summary>
        /// Gets or Sets transform.Orientation
        /// </summary>
        public Matrix4 Orientation { get { return this.transform.Orientation; } set { this.transform.Orientation = value; } }

        /// <summary>
        /// Gets oldTransform.Orientation
        /// </summary>
        public Matrix4 OldOrientation { get { return this.oldTransform.Orientation; } }

        /// <summary>
        /// Gets or Sets transformRate.Velocity
        /// </summary>
        public Vector3 Velocity { get { return this.transformRate.Velocity; } set { this.transformRate.Velocity = value; } }

        /// <summary>
        /// Gets or Sets transformRateAux.Velocity
        /// </summary>
        public Vector3 VelocityAux { get { return this.transformRateAux.Velocity; } set { this.transformRateAux.Velocity = value; } }

        /// <summary>
        /// Gets oldTransformRate.Velocity
        /// </summary>
        public Vector3 OldVelocity { get { return this.oldTransformRate.Velocity; } }

        /// <summary>
        /// Gets or Sets transformRate.AngularVelocity
        /// </summary>
        public Vector3 AngularVelocity { get { return this.transformRate.AngularVelocity; } set { this.transformRate.AngularVelocity = value; } }

        /// <summary>
        /// Gets or Sets transformAux.AngularVelocity
        /// </summary>
        public Vector3 AngularVelocityAux { get { return this.transformRateAux.AngularVelocity; } set { this.transformRateAux.AngularVelocity = value; } }

        /// <summary>
        /// Gets oldTransformRate.AngularVelocity
        /// </summary>
        public Vector3 OldAngVel { get { return this.oldTransformRate.AngularVelocity; } }

        /// <summary>
        /// Gets or Sets force
        /// </summary>
        public Vector3 Force { get { return this.force; } set { this.force = value; } }

        /// <summary>
        /// Gets or Sets torque
        /// </summary>
        public Vector3 Torque { get { return torque; } set { this.torque = value; } }

        /// <summary>
        /// Gets or Sets mass
        /// </summary>
        public float Mass
        {
            get { return mass; }
            set
            {
                this.mass = value; this.invMass = 1.0f / mass; 
                ClearForces();
                AddGravityToExternalForce();
            }
        }

        /// <summary>
        /// Gets or Set invMass
        /// </summary>
        public float InverseMass { get { return invMass; } set { this.invMass = value; this.mass = 1.0f / value; } }

        /// <summary>
        /// Gets or sets applyGravity
        /// </summary>
        public bool ApplyGravity { get { return applyGravity; } set { this.applyGravity = value; } }

        /// <summary>
        /// RemoveConstraint
        /// </summary>
        /// <param name="constraint"></param>
        public void RemoveConstraint(Constraint constraint)
        {
            if (this.constraints.Contains(constraint))
                this.constraints.Remove(constraint);
        }

        /// <summary>
        /// AddConstraint
        /// </summary>
        /// <param name="constraint"></param>
        public void AddConstraint(Constraint constraint)
        {
            if (!this.constraints.Contains(constraint))
                this.constraints.Add(constraint);
        }

        /// <summary>
        /// SetBodyInertia
        /// </summary>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <param name="zz"></param>
        public void SetBodyInertia(float xx, float yy, float zz)
        {
            bodyInertia = Matrix4.Identity;
            bodyInertia.M11 = xx;
            bodyInertia.M22 = yy;
            bodyInertia.M33 = zz;

            bodyInvInertia = Matrix4.Identity;
            bodyInvInertia.M11 = JiggleMath.SafeInvScalar(xx);
            bodyInvInertia.M22 = JiggleMath.SafeInvScalar(yy);
            bodyInvInertia.M33 = JiggleMath.SafeInvScalar(zz);
        }

        /// <summary>
        /// SetBodyInvInertia
        /// </summary>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <param name="zz"></param>
        public void SetBodyInvInertia(float xx, float yy, float zz)
        {
            bodyInvInertia = Matrix4.Identity;
            bodyInvInertia.M11 = xx;
            bodyInvInertia.M22 = yy;
            bodyInvInertia.M33 = zz;

            bodyInertia = Matrix4.Identity;
            bodyInertia.M11 = JiggleMath.SafeInvScalar(xx);
            bodyInertia.M22 = JiggleMath.SafeInvScalar(yy);
            bodyInertia.M33 = JiggleMath.SafeInvScalar(zz);
        }

        /// <summary>
        /// Called right at the end of the timestep to notify the
        /// derived classes.
        /// </summary>
        /// <param name="dt"></param>
        public virtual void PostPhysics(float dt)
        {
        }

        /// <summary>
        /// Called right at the beginning of the timestep to notify the
        /// derived classes.
        /// </summary>
        /// <param name="dt">The delta time.</param>
        public virtual void PrePhysics(float dt)
        {
        }

        /// <summary>
        /// Register with the physics system.
        /// </summary>
        public virtual void EnableBody()
        {
            if (bodyEnabled) return;
            if (PhysicsSystem.CurrentPhysicsSystem == null) return;

            bodyEnabled = true;
            PhysicsSystem.CurrentPhysicsSystem.AddBody(this);
        }

        /// <summary>
        /// Deregiser from the physics system.
        /// </summary>
        public virtual void DisableBody()
        {
            if (!bodyEnabled) return;
            if (PhysicsSystem.CurrentPhysicsSystem == null) return;

            bodyEnabled = false;
            PhysicsSystem.CurrentPhysicsSystem.RemoveBody(this);
        }

        /// <summary>
        /// Are we registered with the physics system?
        /// </summary>
        public virtual bool IsBodyEnabled
        {
            get { return bodyEnabled; }
        }

        /// <summary>
        /// Allowed to return null if this body doesn't engage
        /// in collisions.
        /// </summary>
        public CollisionSkin CollisionSkin
        {
            get { return collSkin; }
            set { collSkin = value; }
        }

        /// <summary>
        /// This sets the position (sets the vel to 0), but it also tries
        /// to make sure that any frozen bodies resting against this one
        /// get activated if necessary.  Not very efficient. Be a little
        /// careful about when you call it - it will mess with the physics
        /// collision list.  Also, after this call the object will be
        /// active.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="orientation"></param>
        public void MoveTo(Vector3 pos, Matrix4 orientation)
        {
            if (bodyEnabled && !IsActive)
            {
                this.SetActive();
            }

            Position = pos;
            Orientation = orientation;
            Velocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            CopyCurrentStateToOld();

            if (this.CollisionSkin != null)
                this.CollisionSkin.SetTransform(ref oldTransform, ref transform);
        }

        
        /// <summary>
        /// Implementation updates the velocity/angular rotation with the
        /// force/torque.
        /// </summary>
        /// <param name="dt"></param>
        public void UpdateVelocity(float dt)
        {
            if (immovable || !IsActive)
                return;

            #region REFERENCE: transformRate.Velocity += (dt * invMass) * force;
            Vector3 vel;
            Vector3.Multiply(ref force,dt * invMass, out vel);
            Vector3.Add(ref transformRate.Velocity, ref vel, out transformRate.Velocity);
            #endregion

            #region REFERENCE: transformRate.AngularVelocity += Vector3.Transform(torque * dt, worldInvInertia);
            Vector3 angVel;
            Vector3.Multiply(ref torque, dt, out angVel);
            Vector3Extensions.TransformNormal(ref angVel, ref worldInvInertia, out angVel);
            Vector3.Add(ref transformRate.AngularVelocity, ref angVel, out transformRate.AngularVelocity);
            #endregion
        
            // don't quite get this - calculating angMom from angVel, then applying torque to that, then
            // converting back just results in the simple equation anyway. The extra term just produces
            // weirdness..
            // Vector3 angMom = mWorldInertia * mTransformRate.angVelocity;
            // mTransformRate.angVelocity += mWorldInvInertia * dt * (mTorque - Cross(mTransformRate.angVelocity, mWorldInertia * mTransformRate.angVelocity));

            // TODO implement rotational friction properly
            #region REFERENCE: transformRate.AngularVelocity *= 0.99f;
            if (collSkin != null && collSkin.Collisions.Count >= 1)
                Vector3.Multiply(ref transformRate.AngularVelocity, 0.99f, out transformRate.AngularVelocity);
            #endregion

        }

        /// <summary>
        /// Implementation updates the position/orientation with the
        /// current velocties. 
        /// </summary>
        /// <param name="dt"></param>
        public void UpdatePosition(float dt)
        {
            if (immovable || !IsActive)
                return;

            #region REFERENCE: Vector3 angMomBefore = Vector3.Transform(transformRate.AngularVelocity, worldInertia);
            Vector3 angMomBefore;
            Vector3Extensions.TransformNormal(ref transformRate.AngularVelocity,ref  worldInertia,out angMomBefore);
            #endregion

            transform.ApplyTransformRate(transformRate, dt);

            #region REFERENCE: invOrientation = Matrix4.Transpose(transform.Orientation);
            Matrix4.Transpose(ref transform.Orientation,out invOrientation);
            #endregion

            // recalculate the world inertia
            #region REFERENCE: worldInvInertia = invOrientation * bodyInvInertia * transform.Orientation;
            Matrix4Extensions.Multiply(ref invOrientation, ref bodyInvInertia, out worldInvInertia);
            Matrix4Extensions.Multiply(ref worldInvInertia, ref transform.Orientation, out worldInvInertia);
            #endregion

            #region REFERENCE: worldInertia = invOrientation * bodyInertia * transform.Orientation;
            Matrix4Extensions.Multiply(ref invOrientation, ref bodyInertia, out worldInertia);
            Matrix4Extensions.Multiply(ref worldInertia, ref transform.Orientation, out worldInertia);
            #endregion

            // conservation of momentum
            #region REFERENCE: transformRate.AngularVelocity = Vector3.Transform(angMomBefore, worldInvInertia);
            Vector3Extensions.TransformNormal(ref angMomBefore, ref worldInvInertia, out transformRate.AngularVelocity);
            #endregion

            if (this.CollisionSkin != null)
                CollisionSkin.SetTransform(ref oldTransform, ref transform);
        }

        /// <summary>
        /// Updates the position with the auxilary velocities, and zeros them.
        /// </summary>
        /// <param name="dt"></param>
        public void UpdatePositionWithAux(float dt)
        {
            if (immovable || !IsActive)
            {
                transformRateAux = TransformRate.Zero;
                return;
            }

            PhysicsSystem physics = PhysicsSystem.CurrentPhysicsSystem;
            int ga = physics.MainGravityAxis;

            if (ga != -1)
            {
                int ga2 = (ga + 1) % 3;
                if (ga2 == 0) { transformRateAux.Velocity.X *= 0.1f; transformRateAux.Velocity.Y *= 0.1f; }
                else if (ga2 == 1) { transformRateAux.Velocity.Y *= 0.1f; transformRateAux.Velocity.Z *= 0.1f; }
                else if (ga2 == 2) { transformRateAux.Velocity.Z *= 0.1f; transformRateAux.Velocity.X *= 0.1f; }
            }

            #region REFERENCE: Vector3 angMomBefore = Vector3.Transform(transformRate.AngularVelocity,worldInertia;
            Vector3 angMomBefore;
            Vector3Extensions.TransformNormal(ref transformRate.AngularVelocity, ref worldInertia, out angMomBefore);
            #endregion

            #region REFERENCE: TransformRate rate = transformRate + tranformRateAux;
            TransformRate rate;
            TransformRate.Add(ref transformRate, ref transformRateAux, out rate);
            #endregion

            transform.ApplyTransformRate(ref rate, dt);

            #region INLINE: tranformRateAux = TransformRate.Zero;
            transformRateAux.AngularVelocity.X = 0.0f;
            transformRateAux.AngularVelocity.Y = 0.0f;
            transformRateAux.AngularVelocity.Z = 0.0f;
            transformRateAux.Velocity.X = 0.0f;
            transformRateAux.Velocity.Y = 0.0f;
            transformRateAux.Velocity.Z = 0.0f;
            #endregion

            #region REFERENCE: invOrientation = Matrix4.Transpose(transform.Orientation);
            Matrix4.Transpose(ref transform.Orientation, out invOrientation);
            #endregion

            // recalculate the world inertia
            //worldInvInertia = transform.Orientation * bodyInvInertia * invOrientation;
            #region REFERENCE: worldInvInertia =  invOrientation * bodyInvInertia* transform.Orientation;
            Matrix4Extensions.Multiply(ref invOrientation, ref bodyInvInertia, out worldInvInertia);
            Matrix4Extensions.Multiply(ref worldInvInertia, ref transform.Orientation, out worldInvInertia);
            #endregion

            //worldInertia = transform.Orientation * bodyInertia * invOrientation;
            #region REFERENCE: worldInertia = invOrientation * bodyInertia* transform.Orientation;
            Matrix4Extensions.Multiply(ref invOrientation, ref bodyInertia, out worldInertia);
            Matrix4Extensions.Multiply(ref worldInertia, ref transform.Orientation, out worldInertia);
            #endregion

            // conservation of momentum
            #region transformRate.AngularVelocity = Vector3.Transform(angMomBefore, worldInvInertia);
            Vector3Extensions.TransformNormal(ref angMomBefore, ref worldInvInertia, out transformRate.AngularVelocity);
            #endregion

            if (this.CollisionSkin != null)
                this.CollisionSkin.SetTransform(ref oldTransform, ref transform);
        }

        /// <summary>
        /// Used by physics to temporarily make an object immovable -
        /// needs to restore afterwards!
        /// </summary>
        public void InternalSetImmovable()
        {
            origImmovable = immovable;
            immovable = true;
        }

        /// <summary>
        /// Used by physics to temporarily make an object immovable -
        /// needs to restore afterwars!
        /// </summary>
        internal void InternalRestoreImmovable() {immovable = origImmovable;}

        /// <summary>
        /// Gets velChanged
        /// </summary>
        public bool VelChanged
        {
            get { return velChanged; }
        }

        /// <summary>
        /// Sets velChanged to false
        /// </summary>
        public void ClearVelChanged()
        {
            velChanged = false;
        }

        /// <summary>
        /// Gets or Sets bodyInertia
        /// </summary>
        public Matrix4 BodyInertia
        {
            get { return bodyInertia; }
            set
            {
                this.bodyInertia = value;
                Matrix4Extensions.Invert(ref value, out this.bodyInvInertia);
            }
        }

        /// <summary>
        /// Gets or Sets bodyInvInertia
        /// </summary>
        public Matrix4 BodyInvInertia
        {
            get { return bodyInvInertia; }
            set
            {
                this.bodyInvInertia = value;
                Matrix4Extensions.Invert(ref value, out this.bodyInertia);
            }
        }

        /// <summary>
        /// Gets worldInertia
        /// </summary>
        public Matrix4 WorldInertia
        {
            get { return worldInertia; }
        }

        /// <summary>
        /// Gets worldInvInertia
        /// </summary>
        public Matrix4 WorldInvInertia
        {
            get { return worldInvInertia; }
        }

        /// <summary>
        /// Ensures that this object never moves, and reduces collision
        /// checking.
        /// </summary>
        public bool Immovable
        {
            get { return immovable; }
            set
            {
                immovable = value;
                origImmovable = immovable;
            }
        }

        /// <summary>
        /// Gets activity (bool)
        /// </summary>
        public bool IsActive
        {
            get { return (activity == Activity.Active); }
        }

        /// <summary>
        /// Make the body active.
        /// </summary>
        public void SetActive()
        {
            if (activity == Activity.Active) return;
            inactiveTime = 0.0f;
            activity = Activity.Active;
        }

        /// <summary>
        /// SetInactive
        /// </summary>
        public void SetInactive()
        {
            if (allowFreezing && PhysicsSystem.CurrentPhysicsSystem.IsFreezingEnabled)
            {
                inactiveTime = deactivationTime;
                activity = Activity.Inactive;
            }
        }

        ///// <summary>
        ///// Damp movement as the body approaches deactivation
        ///// </summary>
        //public void DampForDeactivation()
        //{
        //    float frac = inactiveTime / deactivationTime;

        //    // r = 1 means don't ever damp
        //    // r = 0.5 means start to damp when half way
        //    float r = 0.5f;

        //    if (frac < r) return;

        //    float scale = 1.0f - ((frac - r) / (1.0f - r));
        //    scale = OpenTKHelper.Clamp(scale, 0.0f, 1.0f);

        //    #region REFERENCE: transformRate.Velocity *= scale;
        //    Vector3.Multiply(ref transformRate.Velocity, scale, out transformRate.Velocity);
        //    #endregion

        //    #region REFERENCE: transformRate.AngularVelocity *= scale;
        //    Vector3.Multiply(ref transformRate.AngularVelocity, scale, out transformRate.AngularVelocity);
        //    #endregion
        //}

        /// <summary>
        /// Indicates if the velocity is above the threshold for freezing
        /// </summary>
        /// <returns>bool</returns>
        public bool GetShouldBeActive()
        {
            if (inactiveTime >= deactivationTime)
                return false;
            else
                return true;
        }


        /// <summary>
        /// Damp movement as the body approaches deactivation
        /// </summary>
        public void DampForDeactivation()
        {
            float frac = inactiveTime / deactivationTime;

            // r = 1 means don't ever damp
            // r = 0.5 means start to damp when half way
            float r = 0.5f;

            if (frac < r) return;

            float scale = 1.0f - ((frac - r) / (1.0f - r));
            scale = OpenTKHelper.Clamp(scale, 0.0f, 1.0f);

            // the idea is to damp the body but not below the threshold for deactivation
            float frac0 = transformRate.Velocity.LengthSquared * scale * scale / sqVelocityActivityThreshold;
            float frac1 = transformRate.AngularVelocity.LengthSquared * scale * scale / sqAngVelocityActivityThreshold;

            if (frac0 > 1.0f) Vector3.Multiply(ref transformRate.Velocity, scale, out transformRate.Velocity);
            if (frac1 > 1.0f) Vector3.Multiply(ref transformRate.AngularVelocity, scale, out transformRate.AngularVelocity);
        }

        /// <summary>
        /// UpdateDeactivation
        /// </summary>
        /// <param name="dt"></param>
        public void UpdateDeactivation(float dt)
        {
            if ((transformRate.Velocity.LengthSquared > sqVelocityActivityThreshold) ||
                (transformRate.AngularVelocity.LengthSquared > sqAngVelocityActivityThreshold))
                inactiveTime = 0.0f;
            else
                inactiveTime += dt;
        }

        /// <summary>
        /// GetShouldBeActiveAux
        /// </summary>
        /// <returns>bool</returns>
        public bool GetShouldBeActiveAux()
        {
            return ((transformRateAux.Velocity.LengthSquared >
                sqVelocityActivityThreshold) ||
                (transformRateAux.AngularVelocity.LengthSquared >
                sqAngVelocityActivityThreshold));
        }

        /// <summary>
        /// Set how long it takes to deactivate.
        /// </summary>
        /// <param name="seconds"></param>
        public void SetDeactivationTime(float seconds)
        {
            deactivationTime = seconds;
        }

        /// <summary>
        /// Set what the velocity threshold is for activation.
        /// rot is in deg per second.
        /// </summary>
        /// <param name="vel"></param>
        /// <param name="angVel"></param>
        public void SetActivityThreshold(float vel, float angVel)
        {
            sqVelocityActivityThreshold = vel * vel;
            sqAngVelocityActivityThreshold = OpenTKHelper.ToRadians(angVel) * OpenTKHelper.ToRadians(angVel);
        }

        /// <summary>
        /// Allows getting/setting of whether this body ever freezes
        /// </summary>
        public bool AllowFreezing
        {
            get { return allowFreezing; }
            set
            {
                allowFreezing = value;
                // ??
                if (!value) SetActive();
            }
        }

        /// <summary>
        /// Bodies that use a lot of constraints don't behave very well during the
        /// physics shock step, so they can bypass it.
        /// </summary>
        public bool DoShockProcessing
        {
            get { return doShockProcessing; }
            set { doShockProcessing = value; }
        }

        /// <summary>
        /// LimitVel
        /// </summary>
        public void LimitVel()
        {
			//if at least one velocity component violates the max velocity
			if ((transformRate.Velocity.X < -VelMax || transformRate.Velocity.X > VelMax) || (transformRate.Velocity.Y < -VelMax || transformRate.Velocity.Y > VelMax) || (transformRate.Velocity.Z < -VelMax || transformRate.Velocity.Z > VelMax))
			{
				float v_min, v_max, dMin, dMax, scaleFactor;

				//determine minimum
				#region REFERENCE: v_min = System.Math.Min(System.Math.Min(transformRate.Velocity.X, transformRate.Velocity.Y), transformRate.Velocity.Z);
				if (transformRate.Velocity.X < transformRate.Velocity.Y)
				{
					v_min = (transformRate.Velocity.X < transformRate.Velocity.Z) ? transformRate.Velocity.X : transformRate.Velocity.Z;
				}
				else
				{
					v_min = (transformRate.Velocity.Y < transformRate.Velocity.Z) ? transformRate.Velocity.Y : transformRate.Velocity.Z;
				}
				#endregion

				//determine maximum
				#region REFERENCE: v_max = System.Math.Max(System.Math.Max(transformRate.Velocity.X, transformRate.Velocity.Y), transformRate.Velocity.Z);
				if (transformRate.Velocity.X > transformRate.Velocity.Y)
				{
					v_max = (transformRate.Velocity.X > transformRate.Velocity.Z) ? transformRate.Velocity.X : transformRate.Velocity.Z;
				}
				else
				{
					v_max = (transformRate.Velocity.Y > transformRate.Velocity.Z) ? transformRate.Velocity.Y : transformRate.Velocity.Z;
				}
				#endregion

				//calculate divergence
				dMin = (v_min < -VelMax) ? v_min + VelMax : 0f;
				dMax = (v_max > VelMax) ? v_max - VelMax : 0f;
				#region REFERENCE: dMin = System.Math.Abs(dMin);
				dMin = (dMin < 0) ? -dMin : dMin;
				#endregion
				#region REFERENCE: dMax = System.Math.Abs(dMax);
				dMax = (dMax < 0) ? -dMax : dMax;
				#endregion

				//calculate scaling factor
				scaleFactor = (dMin > dMax) ? (-VelMax / v_min) : (VelMax / v_max);

				//rescale velcocity to match max velocity
				#region REFERENCE: Vector3.Multiply(ref transformRate.Velocity, clampFactor, out transformRate.Velocity);
				transformRate.Velocity.X *= scaleFactor;
				transformRate.Velocity.Y *= scaleFactor;
				transformRate.Velocity.Z *= scaleFactor;
				#endregion
			}
        }

        /// <summary>
        /// LimitAngVel
        /// </summary>
        public void LimitAngVel()
        {
			//float fX = System.Math.Abs(transformRate.AngularVelocity.X) / AngVelMax;
			//float fY = System.Math.Abs(transformRate.AngularVelocity.Y) / AngVelMax;
			//float fZ = System.Math.Abs(transformRate.AngularVelocity.Z) / AngVelMax;

			//float f = OpenTKHelper.Max(fX, fY);
			//f = OpenTKHelper.Max(f, fZ);

			//if (f > 1.0f)
			//    #region REFERENCE: transformRate.AngularVelocity /= f;
			//    Vector3.Divide(ref transformRate.AngularVelocity, f, out transformRate.AngularVelocity);
			//    #endregion


			//if at least one angular velocity component violates the max angular velocity
			if ((transformRate.AngularVelocity.X < -AngVelMax || transformRate.AngularVelocity.X > AngVelMax) || (transformRate.AngularVelocity.Y < -AngVelMax || transformRate.AngularVelocity.Y > AngVelMax) || (transformRate.AngularVelocity.Z < -AngVelMax || transformRate.AngularVelocity.Z > AngVelMax))
			{
				float v_min, v_max, dMin, dMax, scaleFactor;

				//determine minimum
				#region REFERENCE: v_min = System.Math.Min(System.Math.Min(transformRate.AngularVelocity.X, transformRate.AngularVelocity.Y), transformRate.AngularVelocity.Z);
				if (transformRate.AngularVelocity.X < transformRate.AngularVelocity.Y)
				{
					v_min = (transformRate.AngularVelocity.X < transformRate.AngularVelocity.Z) ? transformRate.AngularVelocity.X : transformRate.AngularVelocity.Z;
				}
				else
				{
					v_min = (transformRate.AngularVelocity.Y < transformRate.AngularVelocity.Z) ? transformRate.AngularVelocity.Y : transformRate.AngularVelocity.Z;
				}
				#endregion

				//determine maximum
				#region REFERENCE: v_max = System.Math.Max(System.Math.Max(transformRate.AngularVelocity.X, transformRate.AngularVelocity.Y), transformRate.AngularVelocity.Z);
				if (transformRate.AngularVelocity.X > transformRate.AngularVelocity.Y)
				{
					v_max = (transformRate.AngularVelocity.X > transformRate.AngularVelocity.Z) ? transformRate.AngularVelocity.X : transformRate.AngularVelocity.Z;
				}
				else
				{
					v_max = (transformRate.AngularVelocity.Y > transformRate.AngularVelocity.Z) ? transformRate.AngularVelocity.Y : transformRate.AngularVelocity.Z;
				}
				#endregion

				//calculate divergence
				dMin = (v_min < -AngVelMax) ? v_min + AngVelMax : 0f;
				dMax = (v_max > AngVelMax) ? v_max - AngVelMax : 0f;
				#region REFERENCE: dMin = System.Math.Abs(dMin);
				dMin = (dMin < 0) ? -dMin : dMin;
				#endregion
				#region REFERENCE: dMax = System.Math.Abs(dMax);
				dMax = (dMax < 0) ? -dMax : dMax;
				#endregion

				//calculate scaling factor
				scaleFactor = (dMin > dMax) ? (-AngVelMax / v_min) : (AngVelMax / v_max);

				//rescale angular velcocity to match max angular velocity
				#region REFERENCE: Vector3.Multiply(ref transformRate.AngularVelocity, clampFactor, out transformRate.AngularVelocity);
				transformRate.AngularVelocity.X *= scaleFactor;
				transformRate.AngularVelocity.Y *= scaleFactor;
				transformRate.AngularVelocity.Z *= scaleFactor;
				#endregion
			}

        }

        /// <summary>
        /// Returns the velocity of a point at body-relative position
        /// (in world frame) relPos
        /// </summary>
        /// <param name="relPos"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetVelocity(Vector3 relPos)
        {
            return new Vector3(
                    transformRate.Velocity.X + transformRate.AngularVelocity.Y * relPos.Z - transformRate.AngularVelocity.Z * relPos.Y,
                    transformRate.Velocity.Y + transformRate.AngularVelocity.Z * relPos.X - transformRate.AngularVelocity.X * relPos.Z,
                    transformRate.Velocity.Z + transformRate.AngularVelocity.X * relPos.Y - transformRate.AngularVelocity.Y * relPos.X);
        }


        /// <summary>
        /// Returns the velocity of a point at body-relative position
        /// (in world frame) relPos
        /// </summary>
        /// <param name="relPos"></param>
        /// <param name="result"></param>
        public void GetVelocity(ref Vector3 relPos, out Vector3 result)
        {
            result.X = transformRate.Velocity.X + transformRate.AngularVelocity.Y * relPos.Z - transformRate.AngularVelocity.Z * relPos.Y;
            result.Y = transformRate.Velocity.Y + transformRate.AngularVelocity.Z * relPos.X - transformRate.AngularVelocity.X * relPos.Z;
            result.Z = transformRate.Velocity.Z + transformRate.AngularVelocity.X * relPos.Y - transformRate.AngularVelocity.Y * relPos.X;
        }

        /// <summary>
        /// As GetVelocity but just uses the aux velocities
        /// </summary>
        /// <param name="relPos"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetVelocityAux(Vector3 relPos)
        {
            return new Vector3(
                transformRateAux.Velocity.X + transformRateAux.AngularVelocity.Y * relPos.Z - transformRateAux.AngularVelocity.Z * relPos.Y,
                transformRateAux.Velocity.Y + transformRateAux.AngularVelocity.Z * relPos.X - transformRateAux.AngularVelocity.X * relPos.Z,
                transformRateAux.Velocity.Z + transformRateAux.AngularVelocity.X * relPos.Y - transformRateAux.AngularVelocity.Y * relPos.X);
        }

        /// <summary>
        /// As GetVelocity but just uses the aux velocities
        /// </summary>
        /// <param name="relPos"></param>
        /// <param name="result"></param>
        public void GetVelocityAux(ref Vector3 relPos, out Vector3 result)
        {
            result = new Vector3(
                transformRateAux.Velocity.X + transformRateAux.AngularVelocity.Y * relPos.Z - transformRateAux.AngularVelocity.Z * relPos.Y,
                transformRateAux.Velocity.Y + transformRateAux.AngularVelocity.Z * relPos.X - transformRateAux.AngularVelocity.X * relPos.Z,
                transformRateAux.Velocity.Z + transformRateAux.AngularVelocity.X * relPos.Y - transformRateAux.AngularVelocity.Y * relPos.X);
        }
        


        /// <summary>
        /// Adds the other body to the list of bodies to be activated if
        /// this body moves more than a certain distance from either a
        /// previously stored position, or the position passed in.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="otherBody"></param>
        public void AddMovementActivation(Vector3 pos, Body otherBody)
        {
            if (bodiesToBeActivatedOnMovement.Contains(otherBody)) return;

            if (bodiesToBeActivatedOnMovement.Count == 0)
                storedPositionForActivation = pos;

            bodiesToBeActivatedOnMovement.Add(otherBody);
        }

        /// <summary>
        /// SetOrientation
        /// </summary>
        /// <param name="orient"></param>
        public void SetOrientation(Matrix4 orient)
        {
            transform.Orientation = orient;
            Matrix4.Transpose(ref transform.Orientation, out invOrientation);
            Matrix4Extensions.Multiply(ref bodyInvInertia, ref invOrientation, out worldInvInertia);
            Matrix4Extensions.Multiply(ref worldInvInertia, ref transform.Orientation, out worldInvInertia);
            Matrix4Extensions.Multiply(ref bodyInertia, ref invOrientation, out worldInertia);
            Matrix4Extensions.Multiply(ref worldInertia, ref transform.Orientation, out worldInertia);
        }

        #region Add impulses in the world coordinate frame
        /// <summary>
        /// ApplyWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyWorldImpulse(Vector3 impulse)
        {
            if (immovable) return;

            #region INLINE: transformRate.Velocity += invMass * impulse;
            transformRate.Velocity.X += impulse.X * invMass;
            transformRate.Velocity.Y += impulse.Y * invMass;
            transformRate.Velocity.Z += impulse.Z * invMass;
            #endregion

            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyNegativeWorldImpulse(Vector3 impulse)
        {
            if (immovable) return;

            #region INLINE: transformRate.Velocity -= invMass * impulse;
            transformRate.Velocity.X -= impulse.X * invMass;
            transformRate.Velocity.Y -= impulse.Y * invMass;
            transformRate.Velocity.Z -= impulse.Z * invMass;
            #endregion

            velChanged = true;
        }

        /// <summary>
        /// ApplyWorldImpulseAux
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyWorldImpulseAux(Vector3 impulse)
        {
            if (immovable) return;
            Vector3.Multiply(ref impulse, invMass, out impulse);
            Vector3.Add(ref transformRateAux.Velocity, ref impulse,out transformRateAux.Velocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeWorldImpulseAux
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyNegativeWorldImpulseAux(Vector3 impulse)
        {
            if (immovable) return;
            Vector3.Multiply(ref impulse, -invMass, out impulse);
            Vector3.Add(ref transformRateAux.Velocity, ref impulse, out transformRateAux.Velocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="pos"></param>
        public void ApplyWorldImpulse(ref Vector3 impulse,ref Vector3 pos)
        {
            if (immovable) return;
            Vector3 v0;
            Vector3.Subtract(ref pos, ref transform.Position, out v0);
            Vector3 v1;
            Vector3.Multiply(ref impulse, invMass, out v1);
            Vector3.Add(ref transformRate.Velocity, ref v1, out transformRate.Velocity);
            Vector3.Cross(ref v0, ref impulse, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRate.AngularVelocity, ref v1, out transformRate.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="pos"></param>
        public void ApplyWorldImpulse(Vector3 impulse, Vector3 pos)
        {
            if (immovable) return;
            Vector3.Subtract(ref pos, ref transform.Position, out pos);
            Vector3 v1;
            Vector3.Multiply(ref impulse, invMass, out v1);
            Vector3.Add(ref transformRate.Velocity, ref v1, out transformRate.Velocity);
            Vector3.Cross(ref pos, ref impulse, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRate.AngularVelocity, ref v1, out transformRate.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="pos"></param>
        public void ApplyNegativeWorldImpulse(Vector3 impulse, Vector3 pos)
        {
            if (immovable) return;
            Vector3.Subtract(ref pos, ref transform.Position, out pos);
            Vector3 v1;
            Vector3.Multiply(ref impulse, -invMass, out v1);
            Vector3.Add(ref transformRate.Velocity, ref v1, out transformRate.Velocity);
            Vector3.Cross(ref impulse, ref pos, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRate.AngularVelocity, ref v1, out transformRate.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyWorldImpulseAux
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="pos"></param>
        public void ApplyWorldImpulseAux(Vector3 impulse, Vector3 pos)
        {
            if (immovable) return;
            Vector3.Subtract(ref pos, ref transform.Position, out pos);
            Vector3 v1;
            Vector3.Multiply(ref impulse, invMass, out v1);
            Vector3.Add(ref transformRateAux.Velocity, ref v1, out transformRateAux.Velocity);
            Vector3.Cross(ref pos, ref impulse, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRateAux.AngularVelocity, ref v1, out transformRateAux.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeWorldImpulseAux
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="pos"></param>
        public void ApplyNegativeWorldImpulseAux(Vector3 impulse, Vector3 pos)
        {
            if (immovable) return;
            Vector3.Subtract(ref pos, ref transform.Position, out pos);
            Vector3 v1;
            Vector3.Multiply(ref impulse, -invMass, out v1);
            Vector3.Add(ref transformRateAux.Velocity, ref v1, out transformRateAux.Velocity);
            Vector3.Cross(ref impulse, ref pos, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRateAux.AngularVelocity, ref v1, out transformRateAux.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyWorldAngImpulse
        /// </summary>
        /// <param name="angImpulse"></param>
        public void ApplyWorldAngImpulse(Vector3 angImpulse)
        {
            if (immovable) return;
            Vector3Extensions.TransformNormal(ref angImpulse, ref worldInvInertia, out angImpulse);
            Vector3.Add(ref transformRate.AngularVelocity, ref angImpulse, out transformRate.AngularVelocity);
            velChanged = true;
        }

        #endregion

        #region Add impulses at a position offset in world space
        /// <summary>
        /// ApplyBodyWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="delta"></param>
        public void ApplyBodyWorldImpulse(Vector3 impulse, Vector3 delta)
        {
            if (immovable) return;
            Vector3 v1;
            Vector3.Multiply(ref impulse, invMass, out v1);
            Vector3.Add(ref transformRate.Velocity, ref v1, out transformRate.Velocity);
            Vector3.Cross(ref delta, ref impulse, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRate.AngularVelocity, ref v1, out transformRate.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyBodyWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="delta"></param>
        public void ApplyBodyWorldImpulse(ref Vector3 impulse,ref Vector3 delta)
        {
            if (immovable) return;

            #region INLINE: transformRate.Velocity = transformRate.Velocity + invMass * impulse;
            transformRate.Velocity.X = transformRate.Velocity.X + (impulse.X * invMass);
            transformRate.Velocity.Y = transformRate.Velocity.Y + (impulse.Y * invMass);
            transformRate.Velocity.Z = transformRate.Velocity.Z + (impulse.Z * invMass);
            #endregion

            #region INLINE: transformRate.AngularVelocity += Vector3Extensions.TransformNormal(Vector3.Cross(delta, impulse), worldInvInertia);
            float num0, num1, num2;

            num0 = delta.Y * impulse.Z - delta.Z * impulse.Y;
            num1 = delta.Z * impulse.X - delta.X * impulse.Z;
            num2 = delta.X * impulse.Y - delta.Y * impulse.X;

            float num3 = (((num0 * worldInvInertia.M11) + (num1 * worldInvInertia.M21)) + (num2 * worldInvInertia.M31));
            float num4 = (((num0 * worldInvInertia.M12) + (num1 * worldInvInertia.M22)) + (num2 * worldInvInertia.M32));
            float num5 = (((num0 * worldInvInertia.M13) + (num1 * worldInvInertia.M23)) + (num2 * worldInvInertia.M33));

            transformRate.AngularVelocity.X += num3;
            transformRate.AngularVelocity.Y += num4;
            transformRate.AngularVelocity.Z += num5;
            #endregion

            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeBodyWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="delta"></param>
        public void ApplyNegativeBodyWorldImpulse(Vector3 impulse, Vector3 delta)
        {
            if (immovable) return;
            Vector3 v1;
            Vector3.Multiply(ref impulse, -invMass, out v1);
            Vector3.Add(ref transformRate.Velocity, ref v1, out transformRate.Velocity);
            Vector3.Cross(ref impulse,ref delta, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRate.AngularVelocity, ref v1, out transformRate.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeBodyWorldImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="delta"></param>
        public void ApplyNegativeBodyWorldImpulse(ref Vector3 impulse, ref Vector3 delta)
        {
            if (immovable) return;

            #region INLINE: transformRate.Velocity = transformRate.Velocity - invMass * impulse;
            transformRate.Velocity.X = transformRate.Velocity.X - (impulse.X * invMass);
            transformRate.Velocity.Y = transformRate.Velocity.Y - (impulse.Y * invMass);
            transformRate.Velocity.Z = transformRate.Velocity.Z - (impulse.Z * invMass);
            #endregion

            #region INLINE: transformRate.AngularVelocity += Vector3Extensions.TransformNormal(Vector3.Cross(impulse, delta), worldInvInertia);
            float num0, num1, num2;

            num0 = delta.Z * impulse.Y - delta.Y * impulse.Z;
            num1 = delta.X * impulse.Z - delta.Z * impulse.X;
            num2 = delta.Y * impulse.X - delta.X * impulse.Y;

            float num3 = (((num0 * worldInvInertia.M11) + (num1 * worldInvInertia.M21)) + (num2 * worldInvInertia.M31));
            float num4 = (((num0 * worldInvInertia.M12) + (num1 * worldInvInertia.M22)) + (num2 * worldInvInertia.M32));
            float num5 = (((num0 * worldInvInertia.M13) + (num1 * worldInvInertia.M23)) + (num2 * worldInvInertia.M33));

            transformRate.AngularVelocity.X += num3;
            transformRate.AngularVelocity.Y += num4;
            transformRate.AngularVelocity.Z += num5;
            #endregion

            velChanged = true;
        }

        /// <summary>
        /// ApplyBodyWorldImpulseAux
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="delta"></param>
        public void ApplyBodyWorldImpulseAux(ref Vector3 impulse,ref Vector3 delta)
        {
            if (immovable) return;
            Vector3 v1;
            Vector3.Multiply(ref impulse, invMass, out v1);
            Vector3.Add(ref transformRateAux.Velocity, ref v1, out transformRateAux.Velocity);
            Vector3.Cross(ref delta, ref impulse, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRateAux.AngularVelocity, ref v1, out transformRateAux.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeBodyWorldImpulseAux
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="delta"></param>
        public void ApplyNegativeBodyWorldImpulseAux(ref Vector3 impulse,ref Vector3 delta)
        {
            if (immovable) return;
            Vector3 v1;
            Vector3.Multiply(ref impulse, -invMass, out v1);
            Vector3.Add(ref transformRateAux.Velocity, ref v1, out transformRateAux.Velocity);
            Vector3.Cross(ref impulse, ref delta, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRateAux.AngularVelocity, ref v1, out transformRateAux.AngularVelocity);
            velChanged = true;
        }

        #endregion

        #region Add impulses in the body coordinate frame
        /// <summary>
        /// ApplyBodyImpulse
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyBodyImpulse(Vector3 impulse)
        {
            if (immovable) return;
            Vector3Extensions.TransformNormal(ref impulse, ref transform.Orientation, out impulse);
            Vector3.Multiply(ref impulse,invMass, out impulse);
            Vector3.Add(ref transformRate.Velocity, ref impulse, out transformRate.Velocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeBodyImpulse
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyNegativeBodyImpulse(Vector3 impulse)
        {
            if (immovable) return;
            Vector3Extensions.TransformNormal(ref impulse, ref transform.Orientation, out impulse);
            Vector3.Multiply(ref impulse, -invMass, out impulse);
            Vector3.Add(ref transformRate.Velocity, ref impulse, out transformRate.Velocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyBodyImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="pos"></param>
        public void ApplyBodyImpulse(Vector3 impulse, Vector3 pos)
        {
            if (immovable) return;

            Vector3Extensions.TransformNormal(ref impulse, ref transform.Orientation, out impulse);
            Vector3Extensions.TransformNormal(ref pos, ref transform.Orientation, out pos);
            Vector3.Add(ref transform.Position, ref pos, out pos);
            Vector3.Subtract(ref pos, ref transform.Position, out pos);
            Vector3 v1;
            Vector3.Multiply(ref impulse, invMass, out v1);
            Vector3.Add(ref transformRate.Velocity, ref v1, out transformRate.Velocity);
            Vector3.Cross(ref pos, ref impulse, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRate.AngularVelocity, ref v1, out transformRate.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyNegativeBodyImpulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="pos"></param>
        public void ApplyNegativeBodyImpulse(Vector3 impulse, Vector3 pos)
        {
            if (immovable) return;
            Vector3Extensions.TransformNormal(ref impulse, ref transform.Orientation, out impulse);
            Vector3Extensions.TransformNormal(ref pos, ref transform.Orientation, out pos);
            Vector3.Add(ref transform.Position, ref pos, out pos);
            Vector3.Subtract(ref pos, ref transform.Position, out pos);
            Vector3 v1;
            Vector3.Multiply(ref impulse, -invMass, out v1);
            Vector3.Add(ref transformRate.Velocity, ref v1, out transformRate.Velocity);
            Vector3.Cross(ref impulse, ref pos, out v1);
            Vector3Extensions.TransformNormal(ref v1, ref worldInvInertia, out v1);
            Vector3.Add(ref transformRate.AngularVelocity, ref v1, out transformRate.AngularVelocity);
            velChanged = true;
        }

        /// <summary>
        /// ApplyBodyAngImpulse
        /// </summary>
        /// <param name="angImpulse"></param>
        public void ApplyBodyAngImpulse(Vector3 angImpulse)
        {
            if (immovable) return;
            Vector3Extensions.TransformNormal(ref angImpulse, ref transform.Orientation, out angImpulse);
            Vector3Extensions.TransformNormal(ref angImpulse, ref worldInvInertia, out angImpulse);
            Vector3.Add(ref transformRate.AngularVelocity, ref angImpulse, out transformRate.AngularVelocity);
            velChanged = true;
        }

        #endregion

        #region Add forces in the world coordinate frame
        /// <summary>
        /// AddWorldForce
        /// </summary>
        /// <param name="force"></param>
        public void AddWorldForce(Vector3 force)
        {
            if (immovable) return;
            Vector3.Add(ref this.force, ref force, out this.force);
            this.velChanged = true;
        }

        /// <summary>
        /// AddWordForce
        /// </summary>
        /// <param name="force"></param>
        /// <param name="pos"></param>
        public void AddWorldForce(Vector3 force, Vector3 pos)
        {
            if (immovable) return;
            Vector3.Add(ref this.force, ref force, out this.force);
            Vector3.Subtract(ref pos, ref transform.Position, out pos);
            Vector3.Cross(ref pos, ref force, out pos);
            Vector3.Add(ref pos, ref this.torque, out this.torque);
            velChanged = true;
        }

        /// <summary>
        /// AddWorldTorque
        /// </summary>
        /// <param name="torque"></param>
        public void AddWorldTorque(Vector3 torque)
        {
            if (immovable) return;
            Vector3.Add(ref this.torque, ref torque, out this.torque);
            velChanged = true;
        }

        #endregion

        #region Add forces in the body coordinate frame
        /// <summary>
        /// AddBodyForce
        /// </summary>
        /// <param name="force"></param>
        public void AddBodyForce(Vector3 force)
        {
            if (immovable) return;
            Vector3Extensions.TransformNormal(ref force, ref transform.Orientation, out force);
            Vector3.Add(ref this.force, ref force, out this.force);
            this.velChanged = true;
        }

        /// <summary>
        /// AddBodyForce
        /// </summary>
        /// <param name="force"></param>
        /// <param name="pos"></param>
        public void AddBodyForce(Vector3 force, Vector3 pos)
        {
            if (immovable) return;
            Vector3Extensions.TransformNormal(ref force, ref transform.Orientation, out force);
            Vector3Extensions.TransformNormal(ref pos, ref transform.Orientation, out pos);
            Vector3.Add(ref transform.Position, ref pos, out pos);
            Vector3.Add(ref this.force, ref force, out this.force);
            Vector3.Subtract(ref pos, ref transform.Position, out pos);
            Vector3.Cross(ref pos, ref force, out pos);
            Vector3.Add(ref pos, ref this.torque, out this.torque);
            velChanged = true;
        }

        /// <summary>
        /// AddBodyTorque
        /// </summary>
        /// <param name="torque"></param>
        public void AddBodyTorque(Vector3 torque)
        {
            if (immovable) return;
            Vector3Extensions.TransformNormal(ref torque, ref transform.Orientation, out torque);
            Vector3.Add(ref this.torque, ref torque, out this.torque);
            velChanged = true;
        }

        #endregion

        /// <summary>
        /// This just sets all forces/impulses etc to zero
        /// </summary>
        public void ClearForces()
        {
            force = torque = Vector3.Zero;
        }

        /// <summary>
        /// Adds the force of global gravity to the external force, if the body is marked
        /// to be effected via global gravity
        /// </summary>
        public void AddGravityToExternalForce()
        {
            if( (PhysicsSystem.CurrentPhysicsSystem != null) &&  applyGravity)
                force += Vector3.Multiply(PhysicsSystem.CurrentPhysicsSystem.Gravity, mass);
        }

        /// <summary>
        /// Allow the body to add on any additional forces (including
        /// gravity)/impulses etc. Default behaviour sets to gravity.
        /// </summary>
        /// <param name="dt"></param>
        public virtual void AddExternalForces(float dt)
        {
            ClearForces();
            AddGravityToExternalForce();
        }

        /// <summary>
        /// Copies the current position etc to old - normally called only
        /// by tPhysicsSystem.
        /// </summary>
        public void CopyCurrentStateToOld()
        {
            oldTransform = transform;
            oldTransformRate = transformRate;
        }

        /// <summary>
        /// SetConstraintsAndCollisionsUnsatisfied
        /// </summary>
        public void SetConstraintsAndCollisionsUnsatisfied()
        {
            int count;

            count = constraints.Count;
            for (int i = 0; i < count; i++) 
                constraints[i].Satisfied = false;

            if (collSkin == null) return;

            count = collSkin.Collisions.Count;
            for (int i = 0; i < count; i++) 
                collSkin.Collisions[i].Satisfied = false;
        }

        /// <summary>
        /// Copy our current state (position, velocity etc) into the stored state
        /// </summary>
        public void StoreState()
        {
            storedTransform = transform;
            storedTransformRate = transformRate;
        }

        /// <summary>
        /// Restore from the stored state into our current state.
        /// </summary>
        public void RestoreState()
        {
            transform = storedTransform;
            transformRate = storedTransformRate;

            #region REFERENCE: invOrientation = Matrix4.Transpose(transform.Orientation);
            Matrix4.Transpose(ref transform.Orientation, out invOrientation);
            #endregion

            //worldInvInertia = transform.Orientation * bodyInvInertia * invOrientation;
            #region REFERENCE: worldInvInertia = invOrientation *bodyInvInertia* transform.Orientation;
            Matrix4Extensions.Multiply(ref invOrientation, ref bodyInvInertia, out worldInvInertia);
            Matrix4Extensions.Multiply(ref worldInvInertia, ref transform.Orientation, out worldInvInertia);
            #endregion

            //worldInertia = transform.Orientation * bodyInertia * invOrientation;
            #region REFERENCE: worldInertia = invOrientation *bodyInertia * transform.Orientation;
            Matrix4Extensions.Multiply(ref invOrientation, ref bodyInertia, out worldInertia);
            Matrix4Extensions.Multiply(ref worldInertia, ref transform.Orientation, out worldInertia);
            #endregion
        }
    }
}
