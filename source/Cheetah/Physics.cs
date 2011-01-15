using Cheetah;
//using Ode.NET;
using System;
using Cheetah.Graphics;
using OpenTK;
using Cheetah.OpenTK;

namespace Cheetah.Physics
{
    public class PhysicsNode : Node
    {
        public PhysicsNode()
        {
            Draw = new System.Collections.ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("mars/mars.mesh"));
            Draw.Add(new Marker());
        }

        public PhysicsNode(DeSerializationContext context) :this()
        {
            DeSerialize(context);
        }

        public override void OnAdd(Scene s)
        {
            base.OnAdd (s);

            Physics = s.Physics.CreateObjectSphere(500, 0.1f);
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
                return Physics.Orientation.GetInverse();
            }
            set
            {
                Physics.Orientation=value.GetInverse();
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

        void DeleteObject(IPhysicsObject po);
        //event Physics.TwoCollisionDelegate Collision;
    }

    public static class Physics
    {
        public static IPhysicsWorld Create()
        {
            return null;
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

        ICollisionMesh CMesh
        {
            get;
            set;
        }

        //event Physics.OneCollisionDelegate Collision;
    }

}