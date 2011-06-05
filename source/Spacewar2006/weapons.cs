using System;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Effects;

using Cheetah;
using Cheetah.Graphics;
using OpenTK;
using Cheetah.Physics;

namespace SpaceWar2006.Weapons
{



    public abstract class Weapon : ITickable
    {
        public virtual void Connect(Inventory i, Battery b)
        {
            EnergySource = b;
            AmmoSource = i;
        }
        public Battery EnergySource;
        public Inventory AmmoSource;

        public virtual void Fire(Actor ss, Slot s)
        {
            CurrentReloadTime = 0;


            Projectile p = (Projectile)Root.Instance.Factory.CreateInstance(ProjectileType);
            if (p.NoReplication || Root.Instance.IsAuthoritive)
            {
                p.Target = ss.Computer.Target;

                Matrix4 ship = ss.Matrix;
                Matrix4 slot = s.Matrix;
                Matrix4 combined = slot * ship;
                Vector3 x, y, z;
                Matrix4Extensions.ExtractBasis(combined, out x, out y, out z);

                p.Position = Matrix4Extensions.ExtractTranslation(combined);
                p.Orientation = Matrix4Extensions.ExtractRotation(combined);
                //p.localspeed=new Vector3(0,0,ExitingSpeed);
                p.Speed = z * ExitingSpeed;
                p.Source = ss;
                Root.Instance.Scene.Spawn(p);

                if (FireSound != null)
                    p.PlaySound(FireSound, false);
            }
            //else p.Kill=true;
        }

        public virtual bool Ready
        {
            get
            {
                if (CurrentReloadTime < ReloadTime)
                    return false;

                return true;
            }
        }

        public virtual void Tick(float dtime)
        {
            CurrentReloadTime += dtime;
        }

        public float CurrentReloadTime;
        public float ReloadTime;
        public Type ProjectileType;
        public float ExitingSpeed;
        public Sound FireSound;
        //public bool CreateOnlyOnServer=false;
    }

    public abstract class BeamWeapon : Weapon
    {
        public BeamWeapon()
        {
        }
        public BeamWeapon(Battery source)
        {
            Connect(null, source);
        }
        public override bool Ready
        {
            get
            {
                if (!base.Ready)
                    return false;

                if (EnergySource.CurrentEnergy < EnergyPerShot)
                    return false;

                return true;
            }
        }

        public override void Fire(Actor ss, Slot s)
        {
            base.Fire(ss, s);

            EnergySource.CurrentEnergy -= EnergyPerShot;
        }

        public override void Connect(Inventory i, Battery b)
        {
            base.Connect(null, b);
        }

        public float EnergyPerShot;
    }

    public class LaserBeam : Projectile
    {
        public LaserBeam()
            : base()
        {
            Draw = new ArrayList();
            Mesh m = Root.Instance.ResourceManager.LoadMesh("laser/laser.mesh");
            Draw.Add(m);
            LifeTime = 3;
            Transparent = 1;
            Damage = new Damage(25, 25, 0, 0);
            SyncRefs = false;
        }

        Light l;
        public override void OnAdd(Scene s)
        {
            base.OnAdd(s);

            l = new Light();
            l.diffuse = new Color4f(0, 1, 0, 1);
            l.directional = false;
            l.Range = 1000;
            //l.Attach = this;
            l.Position = AbsolutePosition;
            l.Speed = Speed;
            l.NoReplication=true;
            s.Spawn(l);
        }

        public override void OnCollide(Node other)
        {
            base.OnCollide(other);
            SmallExplosion x = new SmallExplosion(AbsolutePosition);
            Root.Instance.Scene.Spawn(x);

            if (l != null)
            {
                l.Kill = true;
                l.Attach = null;
                l = null;
            }
        }

        public override void OnKill()
        {
            base.OnKill();
            if (l != null)
            {
                l.Kill = true;
                l.Attach = null;
                l = null;
            }
        }

        public override void OnRemove(Scene s)
        {
            base.OnRemove(s);

            if (l != null)
            {
                l.Kill = true;
                l.Attach = null;
                l = null;
            }
        }

        public LaserBeam(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }
    
    public abstract class Projectile : PhysicsNode
    {
        public Projectile()
        {
            NoReplication = true;
            RenderRadius = 0;
        }

        
        protected override Cheetah.Physics.IPhysicsObject CreatePhysicsObject(Scene s)
        {
            CollisionInfo info = GetCollisionInfo();
            SphereCollisionInfo sphere = info as SphereCollisionInfo;
            if (sphere != null)
            {
                IPhysicsObject obj = s.Physics.CreateObjectSphere(sphere.Sphere.Radius, 1);
                obj.Position = base.Position;
                obj.Speed = base.Speed;
                obj.Orientation = base.Orientation;
                obj.Owner = this;
                return obj;
            }
            else
                return base.CreatePhysicsObject(s);
        }
        
        public Projectile(DeSerializationContext context)
        {
            NoReplication = true;
            DeSerialize(context);
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (IsLocal && LifeTime > 0 && age > LifeTime)
            {
                this.Kill = true;
            }
        }

        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, 0.1f);
        }
        public override void OnKill()
        {
        }
        /*
        public virtual void ApplyDamage(Actor victim)
        {
        }
        */
        public override void OnCollide(Node other)
        {
            //if (NoReplication || Root.Instance is Server)
            if (NoReplication || Root.Instance.IsAuthoritive)
            {
                if (other is Actor)
                {
                    Actor ss = (Actor)other;
                    ss.Damage(Damage);
                }

                Kill = true;
            }
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            if (Source != null)
            {
                context.Write(Source.ServerIndex);
            }
            else
                context.Write(-1);
        }
        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
            int index = context.ReadInt32();
            Source = (Node)Root.Instance.Scene.ServerListGet(index);
        }

        public Node Source;
        public float LifeTime;
        public Damage Damage;
        public Node Target;
    }

    public class DisruptorBeam : Projectile
    {
        public DisruptorBeam()
            : base()
        {
            Draw = new ArrayList();
            Mesh m = Root.Instance.ResourceManager.LoadMesh("laser/laser.mesh");
            Draw.Add(m);
            LifeTime = 2;
            Transparent = 1;
            Damage = new Damage(0, 0, 10, 0);
            SyncRefs = false;
        }
        public override void OnCollide(Node other)
        {
            base.OnCollide(other);
            SmallExplosion x = new SmallExplosion(AbsolutePosition);
            Root.Instance.Scene.Spawn(x);
        }
    }

    public class IonPulseBeam : Projectile
    {
        public IonPulseBeam()
            : base()
        {
            Draw = new ArrayList();
            Mesh m = Root.Instance.ResourceManager.LoadMesh("laser/laser.mesh");
            Draw.Add(m);
            LifeTime = 2;
            Transparent = 1;
            Damage = new Damage(0, 0, 0, 15);
            SyncRefs = false;
        }
        public override void OnCollide(Node other)
        {
            base.OnCollide(other);
            SmallExplosion x = new SmallExplosion(AbsolutePosition);
            Root.Instance.Scene.Spawn(x);
            /*if(other is SpaceShip)
			{
				SpaceShip ss=(SpaceShip)other;

                if (ss.Shield.CurrentCharge <= 10)
                {
                    ss.Battery.CurrentEnergy -= EnergyDamage;
                    if (ss.Battery.CurrentEnergy <= 0)
                    {
                        ss.Battery.CurrentEnergy = 0;
                        ss.ControlsJamed = true;
                    }
                }
			}*/
        }
        public IonPulseBeam(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        //public float EnergyDamage;
    }

    public abstract class Beam : Projectile
    {
        public Beam()
        {
            DamageFrame = Root.Instance.frame + 1;
            SyncRefs = false;
            Transparent = 1;
        }

        public override void OnCollide(Node other)
        {
            if (DamageFrame == Root.Instance.frame)
            {
                if (NoReplication || Root.Instance.IsAuthoritive)
                {
                    if (other is SpaceShip)
                    {
                        SpaceShip ss = (SpaceShip)other;
                        ss.Damage(Damage);
                    }
                }
            }
        }

        int DamageFrame;
    }


    public class PulseLaserBeam : Beam
    {
        public PulseLaserBeam()
        {
            Draw = new ArrayList();
            Mesh m = Root.Instance.ResourceManager.LoadMesh("laserbeam/laserbeam.mesh");
            Draw.Add(new BeamRenderer(15, m, new Vector3(0, 0, 200)));

            LifeTime = 1;
            Damage = new Damage(30, 30, 0, 0);

        }

        public override void SetRenderParameters(IRenderer r, IDrawable draw, Shader shade)
        {
            base.SetRenderParameters(r, draw, shade);

            r.SetUniform(shade.GetUniformLocation("Alpha"), new float[] { 1.0f - age });
        }

        public PulseLaserBeam(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override CollisionInfo GetCollisionInfo()
        {
            return new RayCollisionInfo(Position, Position + Direction * 200 * 15);
        }
    }

    public class RailBeam : Beam
    {
        public RailBeam()
        {
            Draw = new ArrayList();
            Mesh m = Root.Instance.ResourceManager.LoadMesh("rail/rail.mesh");
            Draw.Add(new BeamRenderer(15, m, new Vector3(0, 0, 200)));

            LifeTime = 1;
            Damage = new Damage(250, 250, 0, 0);

        }

        public override void SetRenderParameters(IRenderer r, IDrawable draw, Shader shade)
        {
            base.SetRenderParameters(r, draw, shade);

            r.SetUniform(shade.GetUniformLocation("Alpha"), new float[] { 1.0f - age });
        }

        public RailBeam(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override CollisionInfo GetCollisionInfo()
        {
            return new RayCollisionInfo(Position, Position + Direction * 200 * 15);
        }
    }

    public class IonPulseCannon : BeamWeapon
    {
        public IonPulseCannon()
            : base()
        {
            EnergyPerShot = 10;
            ReloadTime = 0.3f;
            ProjectileType = typeof(IonPulseBeam);
            ExitingSpeed = 1400;
            FireSound = Root.Instance.ResourceManager.LoadSound("ST_WEA_ROM_Photon_Blast_3_1.wav");
        }
    }

    public class LaserCannon : BeamWeapon
    {
        public LaserCannon()
            : base()
        {
            Init();
        }

        void Init()
        {
            EnergyPerShot = 10;
            ReloadTime = 0.5f;
            ProjectileType = typeof(LaserBeam);
            ExitingSpeed = 1600;
            FireSound = Root.Instance.ResourceManager.LoadSound("ST_WEA_FED_Ent_Phaser_Blast_1_1.wav");
        }

        public LaserCannon(Battery source)
            : base(source)
        {
            Init();
        }
    }

    public class PulseLaserCannon : BeamWeapon
    {
        public PulseLaserCannon()
            : base()
        {
            Init();
        }

        void Init()
        {
            EnergyPerShot = 5;
            ReloadTime = 0.3f;
            ProjectileType = typeof(PulseLaserBeam);
            ExitingSpeed = 0;
            FireSound = Root.Instance.ResourceManager.LoadSound("pulselaser.wav");
        }

        public PulseLaserCannon(Battery source)
            : base(source)
        {
            Init();
        }
    }

    public class RailGun : BeamWeapon
    {
        public RailGun()
            : base()
        {
            EnergyPerShot = 10;
            ReloadTime = 1.5f;
            ProjectileType = typeof(RailBeam);
            ExitingSpeed = 0;
            FireSound = Root.Instance.ResourceManager.LoadSound("railgf1a.wav");
        }
    }
    public class DisruptorCannon : BeamWeapon
    {
        public DisruptorCannon()
            : base()
        {
            EnergyPerShot = 10;
            ReloadTime = 0.7f;
            ProjectileType = typeof(DisruptorBeam);
            ExitingSpeed = 1800;
            FireSound = Root.Instance.ResourceManager.LoadSound("ST_WEA_FED_Pulse_Cannon_3_1.wav");
        }
    }


    public class Nuke : Missile
    {
        public Nuke()
        {
            Draw = new ArrayList();
            Mesh m = Root.Instance.ResourceManager.LoadMesh("missile/missile.mesh");
            Draw.Add(m);
            LifeTime = 5;
            Damage = new Damage(100, 100, 0, 0);
            Transparent = 1;
            SyncRefs = false;
        }

        public Nuke(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
        public override void OnKill()
        {
            //base.OnKill();
            //System.Console.WriteLine("missle destryoed.");
            Explosion x = new NukeExplosion(AbsolutePosition, Vector3.Zero);
            Root.Instance.Scene.Spawn(x);
            //x.Position = AbsolutePosition;

        }
    }

    public abstract class Missile : Projectile
    {
        public Missile()
        {
            NoReplication = false;
            Smoke = new SmokeTrail();
            Smoke.NoReplication = true;
            Root.Instance.Scene.Spawn(Smoke);
            Smoke.Attach = this;
            Smoke.Smoke.PointSize = 50;
        }

        public Missile(DeSerializationContext context)
            : base(context)
        {
            NoReplication = false;
            Smoke = new SmokeTrail();
            Smoke.NoReplication = true;
            Root.Instance.Scene.Spawn(Smoke);
            Smoke.Attach = this;
            Smoke.Smoke.PointSize = 50;
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
        }

        public override void OnKill()
        {
            base.OnKill();
            //System.Console.WriteLine("missle destryoed.");
            Explosion x = new Explosion(AbsolutePosition, Vector3.Zero);
            Root.Instance.Scene.Spawn(x);

            x.PlaySound(Root.Instance.ResourceManager.LoadSound("omegaxpl.WAV"), false);
            //x.Position = AbsolutePosition;

        }
        public static float FreightSize = 1;

        SmokeTrail Smoke;
    }

    public class HomingMissile : Missile
    {
        public HomingMissile()
        {
            Draw = new ArrayList();
            Mesh m = Root.Instance.ResourceManager.LoadMesh("missile/missile.mesh");
            Draw.Add(m);
            //Smoke = new SmokeTrail();
            //Root.Instance.Spa
            LifeTime = 10;
            Damage = new Damage(100, 100, 0, 0);
            Transparent = 1;
            SyncRefs = false;
        }

        public HomingMissile(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
        }


        public override void OnRemove(Scene s)
        {
            base.OnRemove(s);


        }
        /*public override void OnKill()
        {
            if (IsLocal)
            {
                Explosion x = new Explosion(AbsolutePosition);
                Root.Instance.Scene.Spawn(x);
                x.Position = AbsolutePosition;
            }
        }*/

        public override void Tick(float dTime)
        {
            base.Tick(dTime);

            if (Target != null)
            {
                /*UpdateDirection(Target.AbsolutePosition, dTime);
                Vector3 forward = Direction;
                forward.Y = 0;
                forward.Normalize();
                Speed = forward * 1000;
                return;*/
                
                if (first)
                {
                    first = false;
                    //rotation = (float)Math.Acos(Vector3.Dot(Direction, Vector3.ZAxis));
                    Vector3 d = Direction;
                    rotation = (float)Math.Atan2(d.X, d.Z);
                    //System.Console.WriteLine(rotation.ToString());
                    Orientation = QuaternionExtensions.FromAxisAngle(0, 1, 0, rotation);
                    //rotation = -(float)Math.Atan2(Direction.X, Direction.Z);
                    //System.Console.WriteLine(rotation.ToString());
                }


                //left vector projected on plane
                Vector3 left = Left;
                left.Y = 0;
                left.Normalize();

                //forward vector projected on plane
                Vector3 forward = Direction;
                forward.Y = 0;
                forward.Normalize();

                //wanted direction vector
                Vector3 want = Target.Position - Position;
                want.Normalize();

                float wantedrotation = (float)Math.Atan2(want.X, want.Z);


                //cos=0
                float cos = Vector3.Dot(left, want);
                float a = (float)Math.Acos((double)cos) * 180.0f / (float)Math.PI;
                // rotationspeed = new Vector3(0, -2 * cos, 0);
                rotationspeed = 2 * cos;

                rotation += rotationspeed * dTime;
                roll = -rotationspeed / 2;

                Quaternion q1 = QuaternionExtensions.FromAxisAngle(0, 1, 0, rotation);
                Orientation = q1;
                Quaternion q2 = QuaternionExtensions.FromAxisAngle(Direction, roll);
                Orientation = q1 * q2;

                Vector3 tmp = Position;
                tmp.Y = 0;
                Position = tmp;



                Speed = Direction * 1000;
                //v.Normalize();
                //Speed = v * 1000;*/




                /*LookAt(Target.AbsolutePosition);
                Vector3 v = Target.AbsolutePosition - AbsolutePosition;
                v.Normalize();
                Speed = v * 1000;*/
            }

            Timer += dTime;


        }

        public void UpdateDirection(Vector3 lookat, float dtime)
        {
            //left vector projected on plane
            Vector3 left = Left;
            left.Y = 0;
            left.Normalize();

            //forward vector projected on plane
            Vector3 forward = Direction;
            forward.Y = 0;
            forward.Normalize();

            //wanted direction vector
            Vector3 want;
            try
            {
                want = lookat - Position;
                want.Normalize();
            }
            catch (DivideByZeroException)
            {
                System.Console.WriteLine("divide bug./%&$");
                want = Vector3.UnitX;
            }

            float cos = Vector3.Dot(left, want);
            if (cos <= 1 && cos >= -1)
            {
                RotationSpeed = -2 * (float)Math.Sign(cos) * (float)Math.Sqrt(Math.Abs(cos));

                Rotation += RotationSpeed * dtime;
                Roll = cos;// -RotationSpeed / 2;
            }
            else
            {
                RotationSpeed = 0;
                Roll = 0;
            }

            Quaternion q1 = QuaternionExtensions.FromAxisAngle(0, 1, 0, Rotation);
            Orientation = q1;
            Quaternion q2 = QuaternionExtensions.FromAxisAngle(Direction, Roll);
            Orientation = q1 * q2;

            Vector3 tmp = Position;
            tmp.Y = 0;
            Position = tmp;
        }
        public new static float FreightSize = 1;

        //SmokeTrail Smoke;
        float Timer = 0;
        float roll;
        new float rotationspeed;
        float RotationSpeed;
        float Rotation;
        float Roll;
        float rotation;
        bool first = true;
        //Random r = new Random();
    }

    public class Mine : Projectile
    {
        public Mine()
        {
            NoReplication = false;
            Draw = new ArrayList();
            Mesh m = Root.Instance.ResourceManager.LoadMesh("mine/mine.mesh");
            Draw.Add(m);
            LifeTime = 60;
            Damage = new Damage(100, 100, 0, 0);
            SyncRefs = false;
            rotationspeed = VecRandom.Instance.NextUnitVector3();
        }

        public Mine(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void OnKill()
        {
            base.OnKill();
            //System.Console.WriteLine("mine destryoed.");
            Explosion x = new Explosion(AbsolutePosition, Vector3.Zero);
            Root.Instance.Scene.Spawn(x);
            //x.Position = AbsolutePosition;

        }
        public static float FreightSize = 1;
    }

    public class MineLayer : MissileLauncher
    {
        public MineLayer()
            :base(typeof(Mine))
        {
            //AmmoType = typeof(Mine);
            ReloadTime = 1.0f;
            //ProjectileType = typeof(Mine);
            ExitingSpeed = 0;
            FireSound = Root.Instance.ResourceManager.LoadSound("41.WAV");

        }
    }

    public class HomingMissileLauncher : MissileLauncher
    {
        public HomingMissileLauncher()
            : base(typeof(HomingMissile))
        {
            FireSound = Root.Instance.ResourceManager.LoadSound("Marsec2.wav");
        }
    }

    public class NukeLauncher : MissileLauncher
    {
        public NukeLauncher()
            : base(typeof(Nuke))
        {
        }
    }

    public abstract class MissileLauncher : Weapon
    {
        public MissileLauncher(Type ammotype)
        {
            AmmoType = ammotype;
            ReloadTime = 2.0f;
            ProjectileType = ammotype;
            ExitingSpeed = 500;
        }/*
        public MissileLauncher():
            this(typeof(Nuke))
        {
        }*/

        public override bool Ready
        {
            get
            {
                if (!base.Ready)
                    return false;

                if (AmmoSource.Count(AmmoType) < 1)
                    return false;

                return true;
            }
        }

        public override void Fire(Actor ss, Slot s)
        {
            base.Fire(ss, s);

            AmmoSource.Unload(AmmoType, 1);
        }

        public Type AmmoType;
    }

}