using Cheetah;
//using Ode.NET;
using System;
using Cheetah.Graphics;
using OpenTK;
using Cheetah.OpenTK;

using JigLibX.Physics;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Math;
using JigLibX.Utils;
using Box = JigLibX.Geometry.Box;
using JigLibX.Vehicles;

namespace Cheetah.Physics
{
    public class PhysicsNode : Node
    {
        public PhysicsNode()
        {
        }

        public PhysicsNode(DeSerializationContext context) :this()
        {
            DeSerialize(context);
        }

        protected virtual IPhysicsObject CreatePhysicsObject(Scene s)
        {
            return s.Physics.CreateObjectBox(1, 2,2,2);
        }

        public override void OnAdd(Scene s)
        {
            base.OnAdd (s);

            if(Physics==null)
                Physics=CreatePhysicsObject(s);
        }

        public override void OnRemove(Scene s)
        {
            base.OnRemove (s);

            if(Physics!=null)
            {
                s.Physics.DeleteObject(Physics);
                Physics = null;
            }
        }

        public override void Tick(float dtime)
        {
            position.Original = Position;
            orientation.Original = Orientation;
            orientation.Tick(dtime);
            position.Tick(dtime);
            age+=dtime;
            if(Draw!=null)
                foreach(object o in Draw)
                {
                    if(o is ITickable)
                    {
                        ((ITickable)o).Tick(dtime);
                    }
                }
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            if (Physics == null)
            {
                //HACK
                Physics = CreatePhysicsObject(Root.Instance.Scene);
            }

            base.DeSerialize(context);


            Position = position.Original;
            Orientation = orientation.Original;

            //PhysicsBody.AngularVel = new Ode.dVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            //PhysicsBody.AngularVel = new Ode.dVector3((float)r.ReadInt16() / 100.0f, (float)r.ReadInt16() / 100.0f, (float)r.ReadInt16() / 100.0f);

        }
        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            //w.Write((short)(PhysicsBody.AngularVel.X * 100)); w.Write((short)(PhysicsBody.AngularVel.Y * 100)); w.Write((short)(PhysicsBody.AngularVel.Z * 100));


        }

        public override Vector3 Position
        {
            get
            {
                return Physics.Position;
            }
            set
            {
                Physics.Position=value;
                position.Original = value;
            }
        }

        public override Vector3 Speed
        {
            get
            {
                return Physics.Speed;
            }
            set
            {
                Physics.Speed = value;
            }
        }

        public override Quaternion Orientation
        {
            get
            {
                //return QuaternionExtensions.GetInverse(Physics.Orientation);
                return Physics.Orientation;
            }
            set
            {
                Physics.Orientation = value;// QuaternionExtensions.GetInverse(value);
                orientation.Original = value;
            }
        }


        public IPhysicsObject Physics;
        //public Mesh PhysicsMesh;
    }

    /*
    public class OdeObject : IPhysicsObject
    {
        public OdeObject(IntPtr b, IntPtr g)
        {
            body = b;
            geom = g;
        }

        IntPtr body;
        IntPtr geom;

        #region IPhysicsObject Members

        public Vector3 Position
        {
            get
            {
                d.Vector3 pos;
                d.BodyCopyPosition(body, out pos);
                return new Vector3(pos.X, pos.Y, pos.Z);
            }
            set
            {
                d.BodySetPosition(body, value.X, value.Y, value.Z);
            }
        }

        public Vector3 Speed
        {
            get
            {
                d.Vector3 speed;
                speed=d.BodyGetLinearVel(body);
                return new Vector3(speed.X, speed.Y, speed.Z);
            }
            set
            {
                d.BodySetLinearVel(body, value.X, value.Y, value.Z);
            }
        }

        public Quaternion Orientation
        {
            get
            {
                d.Quaternion q;
                //d.BodyCopyRotation(body, out R);
                d.BodyCopyQuaternion(body,out q);
                return new Quaternion(q.X, q.Y, q.Z, q.W);
            }
            set
            {
                d.Quaternion q;
                q.X=value.X;
                q.Y=value.Y;
                q.Z=value.Z;
                q.W=value.W;
                d.BodySetQuaternion(body, ref q);
            }
        }

        public ICollisionMesh CMesh
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            d.BodyDestroy(body);
            d.GeomDestroy(geom);
        }

        public event Physics.OneCollisionDelegate Collision;

        #endregion
    }

    public class OdeWorld : IPhysicsWorld
    {
        static OdeWorld()
        {
            d.InitODE();
        }

        public OdeWorld()
        {

            world = d.WorldCreate();
            space = d.HashSpaceCreate(IntPtr.Zero);
            contactgroup = d.JointGroupCreate(0);

            d.WorldSetGravity(world, 0.0f, -9.81f,0.0f );
            d.WorldSetCFM(world, 1e-5f);
            d.WorldSetAutoDisableFlag(world, true);
            d.WorldSetContactMaxCorrectingVel(world, 1000.0f);
            d.WorldSetContactSurfaceLayer(world, 0.001f);

            d.CreatePlane(space, 0, 1, 0, 0);

            // Set up contact response parameters
            contact.surface.mode = d.ContactFlags.Bounce | d.ContactFlags.SoftCFM;
            contact.surface.mu = d.Infinity;
            contact.surface.mu2 = 0.0f;
            contact.surface.bounce = 0.1f;
            contact.surface.bounce_vel = 0.1f;
            contact.surface.soft_cfm = 0.01f;
        }

        public IPhysicsObject CreateObjectSphere(float radius, float density)
        {
            d.Mass mass;

            d.MassSetSphere(out mass, density, radius);

            IntPtr body = d.BodyCreate(world);
            IntPtr geom = d.CreateSphere(space, radius);
            d.GeomSetBody(geom, body);
            d.BodySetMass(body, ref mass);

            return new OdeObject(body, geom);
        }

        public IPhysicsObject CreateObjectBox(float density,float lx,float ly,float lz)
        {
            d.Mass mass;
            d.MassSetBox(out mass, density, lx, ly, lz);

            IntPtr body = d.BodyCreate(world);
            IntPtr geom = d.CreateBox(space, lx,ly,lz);
            d.GeomSetBody(geom, body);
            d.BodySetMass(body, ref mass);

            return new OdeObject(body, geom);
        }

        public void Dispose()
        {
            d.JointGroupDestroy(contactgroup);
            d.SpaceDestroy(space);
            d.WorldDestroy(world);
            //d.CloseODE();
        }

        public void DeleteObject(IPhysicsObject po)
        {
            ((OdeObject)po).Dispose();
        }

        //public event Physics.TwoCollisionDelegate Collision;

        public void Tick(float dtime)
        {
            d.SpaceCollide(space, IntPtr.Zero, NearCallback);
            //if (pause == 0)
            d.WorldQuickStep(world, dtime);
            d.JointGroupEmpty(contactgroup);
        }

        void NearCallback(IntPtr space, IntPtr g1, IntPtr g2)
        {
            IntPtr b1 = d.GeomGetBody(g1);
            IntPtr b2 = d.GeomGetBody(g2);
            if (b1 != IntPtr.Zero && b2 != IntPtr.Zero && d.AreConnectedExcluding(b1, b2, d.JointType.Contact))
                return;

            int count = d.Collide(g1, g2, contacts.Length, contacts, d.ContactGeom.SizeOf);
            for (int i = 0; i < count; ++i)
            {
                contact.geom = contacts[i];
                IntPtr joint = d.JointCreateContact(world, contactgroup, ref contact);
                d.JointAttach(joint, b1, b2);
            }
        }

        IntPtr world;
        IntPtr space;

        d.ContactGeom[] contacts = new d.ContactGeom[10];
        d.Contact contact;
        IntPtr contactgroup;
    }
    */

    public interface IPhysicsWorld : ITickable
    {
        IPhysicsObject CreateObjectSphere(float radius,float density);
        IPhysicsObject CreateObjectBox(float density, float lx, float ly, float lz);
        IPhysicsObject CreateObjectCar();
        IPhysicsObject CreateHeightmap(IHeightMap heightMapInfo, float scale, float shiftx, float shifty, float heightscale);

        void DeleteObject(IPhysicsObject po);
        //event Physics.TwoCollisionDelegate Collision;
    }

    public static class Physics
    {
        public static IPhysicsWorld Create()
        {
            return new JigLibWorld();
            //return new OdeWorld();
        }

        public delegate void OneCollisionDelegate(IPhysicsObject o1, IPhysicsObject o2);
        public delegate void TwoCollisionDelegate(IPhysicsObject o1);
    }

    public interface IPhysicsObject
    {
        Vector3 Position
        {
            get;
            set;
        }

        Vector3 Speed
        {
            get;
            set;
        }

        Quaternion Orientation
        {
            get;
            set;
        }

        bool Movable
        {
            get;
            set;
        }
        //event Physics.OneCollisionDelegate Collision;
    }

    public class JigLibObject : IPhysicsObject
    {
        public JigLibObject(Body b)
        {
            body = b;
        }
        protected Body body;

        public Vector3 Position
        {
            get
            {
                return body.Position;
            }
            set
            {
                body.Position = value;
            }
        }

        public Vector3 Speed
        {
            get
            {
                return body.Velocity;
            }
            set
            {
                body.Velocity = value;
            }
        }

        public Quaternion Orientation
        {
            get
            {
                return QuaternionExtensions.FromMatrix4(body.Orientation);
            }
            set
            {
                body.SetOrientation(Matrix4.Rotate(value));
            }
        }

        public bool Movable
        {
            get
            {
                return !body.Immovable;
            }
            set
            {
                body.Immovable = !value;
            }
        }
    }

    public class JigLibWorld : IPhysicsWorld
    {
        PhysicsSystem world;

        public JigLibWorld()
        {
            world = new PhysicsSystem();
            world.CollisionSystem = new CollisionSystemSAP();
        }

        public IPhysicsObject CreateObjectSphere(float radius, float density)
        {
            throw new NotImplementedException();
        }

        public IPhysicsObject CreateObjectCar()
        {
            JigLibCar car = new JigLibCar(true, true, 30.0f, 5.0f, 4.7f, 5.0f, 0.20f, 0.4f, 0.05f, 0.45f, 0.3f, 1, 520.0f, world.Gravity.Length);
            //car.Car.Chassis.Body.MoveTo(new Vector3(-5, -13, 5), Matrix4.Identity);
            car.Car.EnableCar();
            car.Car.Chassis.Body.AllowFreezing = false;
            return car;
        }

        public IPhysicsObject CreateHeightmap(IHeightMap heightMapInfo, float scale, float shiftx, float shifty, float heightscale)
        {
            CollisionSkin collision = new CollisionSkin(null);
            Body _body = new Body();
            
            Array2D field = new Array2D(heightMapInfo.Size.X, heightMapInfo.Size.Y);

            for (int x = 0; x < field.Nx; x++)
            {
                for (int z = 0; z < field.Nz; z++)
                {
                    field.SetAt(x, z, heightscale * heightMapInfo.GetHeight(x,z));
                }
            }

            // move the body. The body (because its not connected to the collision
            // skin) is just a dummy. But the base class shoudl know where to
            // draw the model.
            _body.MoveTo(new Vector3(shiftx, 0, shifty), Matrix4.Identity);

            //collision.AddPrimitive(new Heightmap(field, shift.X, shift.Y, heightMapInfo.terrainScale, heightMapInfo.terrainScale), new MaterialProperties(0.7f, 0.7f, 0.6f));
            collision.AddPrimitive(new Heightmap(field, shiftx, shifty, scale, scale), new MaterialProperties(0.7f, 0.7f, 0.6f));

            _body.CollisionSkin = collision;
            _body.Immovable = true;
            _body.EnableBody();

            return new JigLibObject(_body);
        }

        private static Vector3 SetMass(float mass, CollisionSkin skin, Body body)
        {
            PrimitiveProperties primitiveProperties = new PrimitiveProperties(
                PrimitiveProperties.MassDistributionEnum.Solid,
                PrimitiveProperties.MassTypeEnum.Mass, mass);

            float junk;
            Vector3 com;
            Matrix4 it;
            Matrix4 itCoM;

            skin.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);

            body.BodyInertia = itCoM;
            body.Mass = junk;

            return com;
        }

        static Body CreateCube(Vector3 pos, Vector3 size)
        {
            Body _body = new Body();
            CollisionSkin _skin = new CollisionSkin(_body);
            _body.CollisionSkin = _skin;
            Box box = new Box(pos, Matrix4.Identity, size);
            _skin.AddPrimitive(box, new MaterialProperties(0.8f, 0.8f, 0.7f));

            Vector3 com = SetMass(1.0f, _skin, _body);

            _body.MoveTo(pos, Matrix4.Identity);
            _skin.ApplyLocalTransform(new Transform(-com, Matrix4.Identity));
            _body.EnableBody();
            return _body;
        }

        public IPhysicsObject CreateObjectBox(float density, float lx, float ly, float lz)
        {
            return new JigLibObject(CreateCube(Vector3.Zero,new Vector3(lx,ly,lz)));
        }

        public void DeleteObject(IPhysicsObject po)
        {
            throw new NotImplementedException();
        }

        public void Tick(float dtime)
        {
            float timestep = 0.01f;

            while (dtime > timestep)
            {
                world.Integrate(timestep);
                dtime -= timestep;
            }

            world.Integrate(dtime);
        }
    }


    class JigLibCar : JigLibObject
    {

        private Car car;
        private CollisionSkin collision;

        public JigLibCar(bool FWDrive,
                       bool RWDrive,
                       float maxSteerAngle,
                       float steerRate,
                       float wheelSideFriction,
                       float wheelFwdFriction,
                       float wheelTravel,
                       float wheelRadius,
                       float wheelZOffset,
                       float wheelRestingFrac,
                       float wheelDampingFrac,
                       int wheelNumRays,
                       float driveTorque,
                       float gravity)
            :base(null)
        {
            car = new Car(FWDrive, RWDrive, maxSteerAngle, steerRate,
                wheelSideFriction, wheelFwdFriction, wheelTravel, wheelRadius,
                wheelZOffset, wheelRestingFrac, wheelDampingFrac,
                wheelNumRays, driveTorque, gravity);

            this.body = car.Chassis.Body;
            this.collision = car.Chassis.Skin;
            //this.wheel = wheels;

            SetCarMass(100.0f);
        }

        private void DrawWheel(Wheel wh, bool rotated)
        {
            //foreach (ModelMesh mesh in wheel.Meshes)
            {
                //foreach (BasicEffect effect in mesh.Effects)
                {
                    float steer = wh.SteerAngle;

                    /*Matrix rot;
                    if (rotated) rot = Matrix.CreateRotationY(MathHelper.ToRadians(180.0f));
                    else rot = Matrix.Identity;

                    effect.World = rot * Matrix.CreateRotationZ(MathHelper.ToRadians(-wh.AxisAngle)) * // rotate the wheels
                        Matrix.CreateRotationY(MathHelper.ToRadians(steer)) *
                        Matrix.CreateTranslation(wh.Pos + wh.Displacement * wh.LocalAxisUp) * car.Chassis.Body.Orientation * // oritentation of wheels
                        Matrix.CreateTranslation(car.Chassis.Body.Position); // translation
                    */
                }
                //mesh.Draw();
            }
        }

        /*
        public override void Draw()
        {
            DrawWheel(car.Wheels[0], true);
            DrawWheel(car.Wheels[1], true);
            DrawWheel(car.Wheels[2], false);
            DrawWheel(car.Wheels[3], false);
        }
        */
        public Car Car
        {
            get { return this.car; }
        }

        private void SetCarMass(float mass)
        {
            body.Mass = mass;
            Vector3 min, max;
            car.Chassis.GetDims(out min, out max);
            Vector3 sides = max - min;

            float Ixx = (1.0f / 12.0f) * mass * (sides.Y * sides.Y + sides.Z * sides.Z);
            float Iyy = (1.0f / 12.0f) * mass * (sides.X * sides.X + sides.Z * sides.Z);
            float Izz = (1.0f / 12.0f) * mass * (sides.X * sides.X + sides.Y * sides.Y);

            Matrix4 inertia = Matrix4.Identity;
            inertia.M11 = Ixx; inertia.M22 = Iyy; inertia.M33 = Izz;
            car.Chassis.Body.BodyInertia = inertia;
            car.SetupDefaultWheels();
        }

    }
}