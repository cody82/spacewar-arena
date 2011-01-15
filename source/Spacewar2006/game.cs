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
using System.Xml;

using SpaceWar2006.Rules;
using SpaceWar2006.Weapons;
using SpaceWar2006.Effects;
using SpaceWar2006.Planets;
using SpaceWar2006.Pickups;

using Cheetah;
using Cheetah.Graphics;
using OpenTK;

namespace SpaceWar2006.GameObjects
{
    [Editable]
    public class Waypoint : Node
    {
        public Waypoint()
        {
            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("waypoint/waypoint.mesh"));
        }

        [Editable]
        public string Name;
    }

    public class RandomSpawn : Entity
    {
        public RandomSpawn(Type type,float interval,int count)
        {
            SpawnType = type;
            SpawnInterval = interval;
            SpawnCount = count;
        }

        public override void Tick(float dTime)
        {
            base.Tick(dTime);

            SpawnTimer += dTime;
            while (SpawnTimer >= SpawnInterval)
            {
                SpawnTimer -= SpawnInterval;
                Spawn();
            }
        }

        public virtual void Spawn()
        {
            for (int i = 0; i < SpawnCount; ++i)
            {
                Node n = (Node)Activator.CreateInstance(SpawnType);
                n.Position = VecRandom.Instance.NextScaledVector3(10000, 0, 10000) - new Vector3(5000,0,5000);
                n.Speed = -Vector3Extensions.GetUnit(n.Position) * VecRandom.Instance.NextFloat() * 300;
                n.rotationspeed = VecRandom.Instance.NextScaledVector3(1, 1, 1);
                Root.Instance.Scene.Spawn(n);
                System.Console.WriteLine("asteroid spawned.");
            }
        }

        public float SpawnTimer = 0;
        public float SpawnInterval;
        public int SpawnCount;
        public Type SpawnType;
    }


    public class SpawnPoint : Node
    {
        public SpawnPoint()
        {
            NoReplication = true;
            RenderRadius = 200;

            Draw.Add(Root.Instance.ResourceManager.LoadMesh("spawnpoint/spawnpoint.mesh"));
        }

        public SpawnPoint(Type type, float time)
            : this()
        {
            SpawnType = type;
            SpawnTime = time;
        }

        public SpawnPoint(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (!Root.Instance.IsAuthoritive)
                return;

            if (Spawned == null || Spawned.Kill)
            {
                Spawned = null;
                SpawnTime += dtime;
                if (SpawnTime >= SpawnInterval)
                {
                    Spawn();
                    SpawnTime = 0;
                }
            }
            else
            {
            }
        }

        void Spawn()
        {
            //Cheetah.Console.WriteLine("spawned new object.");
            Spawned = (Entity)Root.Instance.Factory.CreateInstance(SpawnType);
            if (Spawned is Node)
                ((Node)Spawned).Position = AbsolutePosition;
            Root.Instance.Scene.Spawn(Spawned);
        }

        public Type SpawnType;
        public float SpawnInterval = 30;
        public float SpawnTime = 0;
        public Entity Spawned;
    }


    public class SpaceShipInputEntity : InputEntity
    {
        public SpaceShipInputEntity()
        {
        }


        SpaceWar2006.Controls.SpaceShipControlInput Input;
        SpaceShip Target;
    }

    public class Player : PlayerEntity
    {
        public Player(short clientid, string name)
            : base(clientid, name)
        {
        }

        public Player(DeSerializationContext context)
            : base(context)
        {
        }

        public void ChangeTeamEvent(string team)
        {
            Team = int.Parse(team);
        }

        public void ChangeTeam(int team)
        {
            //HACK?!
            if (Root.Instance.IsAuthoritive && Root.Instance.UserInterface!=null)
                Team = team;
            else
                ReplicateCall("ChangeTeamEvent", new string[] { team.ToString() });
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            context.Write(Frags);
            context.Write(Deaths);

            context.Write((byte)Team);
        }

        public void SayEvent(string text)
        {
            IList<Player> list = Root.Instance.Scene.FindEntitiesByType<Player>();
            foreach (Player e in list)
            {
                //if (de.Value is SpaceShip && de.Value != this)
                if (e != this)
                {
                    //SpaceShip e = (SpaceShip)de.Value;
                    e.Hear(this, text);
                }
            }
        }
        public void Hear(Player sender, string text)
        {
            if (HearEvent != null)
                HearEvent(sender, text);
        }
        public void Say(string text)
        {
            Root.Instance.EventSendQueue.Add(new EventReplicationInfo("SayEvent", this, new string[] { text }));
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
            Frags = context.ReadInt16();
            Deaths = context.ReadInt16();

            if ((Team = context.ReadByte()) == 255)
                Team = -1;
        }

        public delegate void HearDelegate(Player sender, string text);
        public event HearDelegate HearEvent;

        public short Frags = 0;
        public short Deaths = 0;
        public int Team = -1;
    }


    public abstract class Turret : Actor
    {
        //public Weapon[] Weapons;
        public Generator Generator;
        //public Battery Battery;
        public Slot[] Slots;
        //public Computer Computer;
        public float Radius;

        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, Radius);
        }
        public override bool CanCollide(Node other)
        {
            if (other is Projectile)
            {
                return ((Projectile)other).Source != this;
            }
            return true;
        }

        public override void Damage(Damage d)
        {
            /*Hull.Damage(d.Normal);
            if (Hull.CurrentHitpoints == 0)
            {
                Kill = true;
            }*/
        }

        protected float RotationSpeed = 0;
        protected float Rotation = 0;
        protected float Roll = 0;

        public override void OnRemove(Scene s)
        {
            base.OnRemove(s);
            Cheetah.Console.WriteLine("dskjvgndfskj");
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
                //float a = (float)Math.Acos((double)cos) * 180.0f / (float)Math.PI;
                //float a = -2 * cos;
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

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            Computer.Tick(dtime);
            Generator.Tick(dtime);

            foreach (Slot s in Slots)
            {
                s.Tick(dtime);
                s.Weapon.Tick(dtime);
            }
            //if (Computer.Target == null)
            {
                Computer.TargetNearest(delegate(Actor a) { return a is SpaceShip; });
            }

            if (Computer.Target != null)
            {
                //LookAt(Computer.Target.AbsolutePosition);
                //UpdateDirection(Computer.Target.AbsolutePosition, dtime);
                float cos = GetCosDirection(Computer.Target.AbsolutePosition);
                rotationspeed.Y = -cos*4;

                if (Distance(Computer.Target) < 2000)
                {
                    foreach (Slot s in Slots)
                    {
                        if (s.Weapon.Ready)
                            s.Weapon.Fire(this, s);
                    }
                }
            }
        }
    }

    [Editable]
    public class LaserTurret : Turret
    {
        public LaserTurret()
        {
            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("turret/turret.mesh"));


            Battery = new Battery(100);
            Generator = new Generator(20, Battery);
            Slots = new Slot[]{
								 new Slot(new LaserCannon(Battery),new Vector3(30,0,0),Quaternion.Identity,0),
								 new Slot(new LaserCannon(Battery),new Vector3(-30,0,0),Quaternion.Identity,0),
							 };
            Shield = new Shield(Battery, 100, 5);
            Hull = new Hull(100);
            Computer = new Computer(this);
            Radius = 40;
            RenderRadius = 50;
            SyncRefs = false;
        }

        public LaserTurret(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }

    [Editable]
    public class Flag : Node
    {
        public Flag(int team, Vector3 position)
        {
            Position = FlagPosition = position;
            Team = team;

            Init();
        }
        public Flag()
        {
            Init();
        }

        void Init()
        {
            SyncRefs = false;

            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("flag/flag.mesh"));
        }

        public Flag(DeSerializationContext context)
        {
            Init();
            DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write((byte)Team);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Team = context.ReadByte();
        }
        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, 50);
        }

        public override void SetRenderParameters(IRenderer r, IDrawable draw, Shader shade)
        {
            base.SetRenderParameters(r, draw, shade);

            int i;
            if ((i=shade.GetUniformLocation("Color"))>=0)
            {
                Color4f c=SpaceWar2006.GameObjects.Team.Colors[Team];
                r.SetUniform(i, new float[] { c.r, c.g, c.b, c.a });
            }
        }
        public override void OnCollide(Node other)
        {
            base.OnCollide(other);


            if (Ctf != null && Root.Instance.IsAuthoritive)
            {
                SpaceShip ss = other as SpaceShip;
                Flag f = other as Flag;
                if (ss != null)
                {
                    Player p = ss.GetPlayer();
                    if (Attach == null && p != null && p.Team != Team)
                    {
                        //player of other team takes the flag!
                        Taken = true;
                        Position = Vector3.Zero;
                        Carrier = ss;
                        Ctf.TakeFlag(this, ss);
                    }
                    else if (p != null && p.Team == Team && Taken)
                    {
                        //returned
                        Ctf.ReturnFlag(this, ss);
                        Kill = true;
                        //spawn new flag
                        Scene.Spawn(new Flag(Team, FlagPosition));
                    }
                }
                else if (f != null && Carrier != null && f.Carrier == null)
                {
                    //captured by carrier of this flag
                    Ctf.CaptureFlag(this, Carrier);
                    Kill = true;
                    //spawn new flag
                    Scene.Spawn(new Flag(Team, FlagPosition));

                }
            }
        }

        public override Vector3 SmoothPosition
        {
            get
            {
                return position;
            }
        }

        public override Matrix4 SmoothMatrix
        {
            get
            {
                return Matrix;
            }
        }
        public override void Tick(float dtime)
        {

            if (Ctf == null)
                Ctf = Root.Instance.Scene.FindEntityByType<CaptureTheFlag>();

            if (Root.Instance.IsAuthoritive)
            {
                if (Carrier != null && Carrier.Kill)
                {
                    if (Ctf != null)
                    {
                        Ctf.DropFlag(this, Carrier);
                    }

                    Position = Carrier.AbsolutePosition;
                    Cheetah.Console.WriteLine(Position.ToString());
                    Carrier = null;
                    //Kill = true;
                    //spawn new flag
                    //Scene.Spawn(new Flag(Team, FlagPosition));
                }
                if (Carrier == null && Taken)
                {
                    TimeWithoutCarrier += dtime;
                    if (TimeWithoutCarrier >= 30)
                    {
                        //return flag
                        Ctf.ReturnFlag(this, null);
                        Kill = true;
                        //spawn new flag
                        Scene.Spawn(new Flag(Team, FlagPosition));
                    }
                }
            }
            base.Tick(dtime);
        }

        public override bool CanCollide(Node other)
        {
            return (other is SpaceShip && other != Attach) || other is Flag;
        }

        public SpaceShip Carrier
        {
            get
            {
                return (SpaceShip)Attach;
            }
            set
            {
                Attach = value;
            }
        }

        [Editable]
        public int Team;

        [Editable]
        public Vector3 FlagPosition;

        CaptureTheFlag Ctf;
        bool Taken = false;
        float TimeWithoutCarrier;
    }

    /*public class CtfPlayer : Player
    {
        public int Captures;
    }*/
    public class CtfTeam : Team
    {
        public CtfTeam(int index, string name)
            : base(index, name)
        {

        }

        public CtfTeam()
        {
        }

        public int Captures;

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Captures = (int)context.ReadByte();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write((byte)Captures);
        }
    }



    public class Team : ISerializable
    {
        public static readonly string[] ColorNames = new string[] { "Red", "Green", "Blue", "Yellow" };
        public static readonly Color4f[] Colors = new Color4f[] { new Color4f(1, 0, 0, 1), new Color4f(0, 1, 0, 1), new Color4f(0, 0, 1, 1), new Color4f(1, 1, 0, 1) };

        public Team(int index, string name)
        {
            Index = index;
            Name = name;
        }
        public Team()
        {
        }

        public Team(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public int Index;
        public String Name;
        public int Score = 0;

        #region ISerializable Members

        public virtual void Serialize(SerializationContext context)
        {
            context.Write(Name);
            context.Write((short)Score);
        }

        public virtual void DeSerialize(DeSerializationContext context)
        {
            Name = context.ReadString();
            Score = (int)context.ReadInt16();
        }

        #endregion
    }

    public class XmlMap_test : XmlMap
    {
        public XmlMap_test()
            : base("maps/test.xml")
        {
        }
        public XmlMap_test(DeSerializationContext context)
            :base(context)
        {
        }
    }

    public class XmlMap : Map
    {
        public XmlMap(string path)
        {
            Path = path;
        }

        public XmlMap(DeSerializationContext context)
        {
            DeSerialize(context);

        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Path = context.ReadString();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write(Path);
        }

        public override void Create()
        {
            Cheetah.Editor.XmlMapReader reader = new Cheetah.Editor.XmlMapReader(
                Root.Instance.FileSystem.Get(Path).getStream());

            reader.Read();
        }

        public string Path;
    }

    public abstract class Map : Entity
    {
        public Map()
        {
        }

        public override void OnAdd(Scene s)
        {
            base.OnAdd(s);
            Scene = s;

            Create();
            CreateCustom();
            if (Mission != null)
                Spawn(Mission, false);
            Created = true;
        }

        public virtual void CreateCustom()
        {
        }

        public Map(DeSerializationContext context)
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

        public virtual void Create()
        {
        }

        protected void Spawn(Entity e, bool noreplicate)
        {
            e.NoReplication = noreplicate;
            //if(noreplicate || Root.Instance is Server)
            if (noreplicate || Root.Instance.IsAuthoritive)
                Scene.Spawn(e);
        }


        /*public void Spawn(string name, Entity e, bool noreplicate)
        {
            Spawn(e,noreplicate);
            Objects[name] = e;
        }*/
        bool Created = false;
        public Mission Mission;
        //Scene scene;
        //Dictionary<string, Entity> Objects=new Dictionary<string,Entity>();
    }

    [Editable]
    public class PlayerStart : Node
    {
        public PlayerStart()
        {
            NoReplication = true;
            RenderRadius = 200;

            Draw.Add(Root.Instance.ResourceManager.LoadMesh("spawnpoint/spawnpoint.mesh"));
            //Draw.Add(new Marker());
        }

        public PlayerStart(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }

    public struct Damage
    {
        public Damage(float normal)
        {
            Normal = normal;
            ShieldPiercing = 0;
            System = 0;
            Shield = 0;
        }

        public Damage(float normal, float shield, float shieldpiercing, float system)
        {
            Normal = normal;
            ShieldPiercing = shieldpiercing;
            System = system;
            Shield = shield;
        }

        public static Damage operator *(Damage d, float f)
        {
            return new Damage(d.Normal * f, d.Shield * f, d.ShieldPiercing * f, d.System * f);
        }
        public float Normal;
        public float ShieldPiercing;
        public float System;
        public float Shield;
    }

    public class Computer : ITickable
    {


        public Computer(Actor owner)
        {
            Owner = owner;
        }

        public void TargetNearest(Predicate<Actor> pred)
        {
            float distnow = float.PositiveInfinity;

            IList<Actor> actors = Root.Instance.Scene.FindEntitiesByType<Actor>();
            foreach (Actor a in actors)
            {
                if (a != Owner && !a.Kill && pred(a))
                {
                    float dist = (((Node)a).AbsolutePosition - Owner.AbsolutePosition).Length;
                    if (dist < distnow)
                    {
                        Target = (Actor)a;
                        distnow = dist;
                    }
                }
            }
        }

        public ICollection<Actor> Scan()
        {
            List<Actor> actors2 = new List<Actor>();
            IList<Actor> actors = Root.Instance.Scene.FindEntitiesByType<Actor>();


            foreach (Actor a in actors)
            {
                if (a != Owner && !a.Kill)
                {
                    actors2.Add(a);
                }
            }
            /*
            if (Root.Instance.Scene.ClientList != null)
                foreach (KeyValuePair<int, Entity> de in Root.Instance.Scene.ClientList)
                {
                    if (de.Value is Actor && de.Value != Owner && !((Entity)de.Value).Kill)
                    {
                        actors.Add((Actor)de.Value);
                    }
                }
            */
            return actors2;
        }

        public void TargetNearestToCursor(Vector3 c)
        {
            float distnow = float.PositiveInfinity;

            IList<Actor> actors = Root.Instance.Scene.FindEntitiesByType<Actor>();
            foreach (Actor a in actors)
            {
                if (a != Owner && !a.Kill)
                {
                    float dist = (((Node)a).AbsolutePosition - c).Length;
                    if (dist < distnow)
                    {
                        Target = (Actor)a;
                        distnow = dist;
                    }
                }
            }

            /*foreach (KeyValuePair<int, Entity> de in Root.Instance.Scene.ClientList)
            {
                if (de.Value is Actor && de.Value != Owner && !de.Value.Kill)
                {
                    float dist = (((Node)de.Value).AbsolutePosition - c).Length;
                    if (dist < distnow)
                    {
                        Target = (Actor)de.Value;
                        distnow = dist;
                    }
                }
            }*/
            //if (Target != null)
            //    System.Console.WriteLine("Computer aquired target: " + Target.ToString());
        }

        public void TargetNext()
        {
            IList<Actor> actors = Root.Instance.Scene.FindEntitiesByType<Actor>();
            foreach (Actor a in actors)
            {
                if (a != Owner && !a.Kill)
                {
                    Target = (Actor)a;
                    //System.Console.WriteLine("Computer aquired target: " + Target.ToString());
                    break;
                }
            }
        }
        /*
        public TargetInfo Scan()
        {
            if(Target==null)
                return null;

            TargetInfo ti = new TargetInfo();
            ti.Dist = (Owner.AbsolutePosition - Target.AbsolutePosition).Length;
            
        }*/

        public void Tick(float dtime)
        {
            //if (Target == null)
            //    TargetNext();
            if (Target != null && Target.Kill)
            {
                System.Console.WriteLine("Computer lost target: " + Target.ToString());
                Target = null;
            }

            //if (TextMonitor == null)
            //   throw new Exception();
        }

        public void WriteLine(string text)
        {
            if (TextMonitor != null)
                TextMonitor.WriteLine(text);
            else
                Cheetah.Console.WriteLine(text);
        }

        public Actor Target;
        public Actor Owner;
        public InGameConsole TextMonitor;


    }

    public class Hull
    {
        public Hull(float hp)
        {
            MaxHitpoints = CurrentHitpoints = hp;
        }

        public void Damage(float hp)
        {
            CurrentHitpoints = Math.Max(0.0f, CurrentHitpoints - hp);
            //System.Console.WriteLine("damage: " + hp);
            //if (CurrentHitpoints == 0.0f)
            //    Cheetah.Console.WriteLine("hull destroyed.");
        }

        public float CurrentHitpoints;
        public float MaxHitpoints;
    }


    public abstract class SpaceShip : Actor
    {
        public SpaceShip()
        {
        }

        public SpaceShip(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void OnCollide(Node other)
        {
            CollisionDirection = other.AbsolutePosition - AbsolutePosition;

            if (other is SpaceShip)
            {
                SpaceShip ss = (SpaceShip)other;
                ss.Damage(new Damage(Hull.CurrentHitpoints + Shield.CurrentCharge, Hull.CurrentHitpoints + Shield.CurrentCharge, 0, 0));


                //Cheetah.Console.WriteLine("spaceship hits spaceship!");
            }
            else if (other is Planet)
            {
                this.Damage(new Damage(10000, 10000, 10000, 10000));
                //Cheetah.Console.WriteLine("spaceship hits planet!");
            }

        }

        public override bool CanCollide(Node other)
        {
            if (other is Projectile)
            {
                return ((Projectile)other).Source != this;
            }
            return true;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            
            //HACK
            if (IsLocal)
            {
                rotationspeed.Y = RotationPower * MaxRotationSpeed;

                Quaternion q1 = QuaternionExtensions.FromAxisAngle(0, 1, 0, -Rotation);
                Orientation = q1;

                float wantedroll = -RotationPower * MaxRoll;
                float delta = wantedroll - Roll;
                float abs = Math.Abs(delta);
                Roll += Math.Min(abs, 1.0f) * Math.Sign(delta) * Math.Min(abs, dtime * RollSpeed);

                Quaternion q2 = QuaternionExtensions.FromAxisAngle(Direction, Roll);
                Orientation = q2 * q1;
            }

            position.Original.Y = 0;




            Speed += Direction * ThrustPower * MainThrust * dtime;
            Speed += Left * StrafePower * StrafeThrust * dtime;
            float factor = Math.Max(Speed.Length / 100, 1);
            Speed -= Speed * Resistance * dtime * factor;

            foreach (Slot s in Slots)
            {
                s.Tick(dtime);
                if (s.Weapon != null)
                    s.Weapon.Tick(dtime);
            }

            Generator.Tick(dtime);
            if (Shield != null)
                Shield.Tick(dtime);
            if (Computer != null)
                Computer.Tick(dtime);

            ShieldVisible -= dtime;
            if (ShieldVisible <= 0.0f)
            {
                if (Draw.Contains(ShieldModel))
                    Draw.Remove(ShieldModel);
            }
        }

        public override void DeSerializeRefs(DeSerializationContext context)
        {
        }


        public override void SerializeRefs(SerializationContext context)
        {
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            bool jamed = context.ReadBoolean();
            float hp = (float)context.ReadInt16();
            float shield = (float)context.ReadInt16();
            float battery = (float)context.ReadInt16();
            int target = context.ReadInt32();

            if (!Root.Instance.IsAuthoritive)
            {
                Hull.CurrentHitpoints = hp;
                UpdateFireTrail();
                Shield.CurrentCharge = shield;
                ControlsJamed = jamed;
                Battery.CurrentEnergy = battery;
            }
            else
                Computer.Target = (Actor)Root.Instance.Scene.ServerListGet(target);


            Inventory.DeSerialize(context);

            for (int i = 0; i < WeaponNumbers.Length; ++i)
            {
                int w = ((int)context.ReadByte()) - 1;

                if (IsLocal)
                    continue;

                WeaponNumbers[i] = w;
                if (WeaponNumbers[i] >= 0)
                {
                    Type t = WeaponList[WeaponNumbers[i]];
                    if (Slots[i].Weapon == null || Slots[i].Weapon.GetType() != t)
                    {
                        ArmWeapon(WeaponNumbers[i], i);
                    }
                }
                else
                {
                    Slots[i].Weapon = null;
                }
            }
        }
        public override void Serialize(SerializationContext context)
        {
            //int t = Root.Instance.TickCount();

            base.Serialize(context);

            context.Write(ControlsJamed);
            context.Write((short)Hull.CurrentHitpoints);
            context.Write((short)Shield.CurrentCharge);
            context.Write((short)Battery.CurrentEnergy);

            int target = (Computer.Target != null) ? Computer.Target.ServerIndex : -1;
            context.Write(target);

            Inventory.Serialize(context);

            for (int i = 0; i < WeaponNumbers.Length; ++i)
            {
                context.Write((byte)(WeaponNumbers[i] + 1));
            }

        }

        public virtual void SpawnExplosion()
        {
            Explosion x = new BigExplosion(AbsolutePosition, Vector3.Zero);
            Root.Instance.Scene.Spawn(x);

            for (int i = 0; i < 10; ++i)
            {
                Crap c = new Crap();
                c.Position = AbsolutePosition;
                c.Speed = (VecRandom.Instance.NextScaledVector3(1, 1, 1) - new Vector3(0.5f, 0.5f, 0.5f)) * (VecRandom.Instance.NextFloat() + 1.0f) * 200.0f;
                c.rotationspeed = VecRandom.Instance.NextScaledVector3(5, 5, 5);
                Root.Instance.Scene.Spawn(c);
            }
        }

        public override void SetRenderParameters(IRenderer r, IDrawable draw, Shader shade)
        {
            if (draw == ShieldModel && shade != null)
            {
                try
                {
                    int loc = shade.GetUniformLocation("hitpos");
                    Vector3 v = Vector3Extensions.GetUnit(CollisionDirection) * Radius;
                    Matrix4 m = Matrix;
                    m.Row3.X = m.Row3.Y = m.Row3.Z = 0;
                    m.Invert();
                    v = Vector3.Transform(v,m);
                    //r.SetUniform(loc, new float[] {100,0,0,1});
                    r.SetUniform(loc, new float[] { v.X, v.Y, v.Z, 1 });
                }
                catch (DivideByZeroException)
                {
                }
                r.SetUniform(shade.GetUniformLocation("strength"), new float[] { ShieldVisible });
            }
        }

        Light FireLight;
        protected void UpdateFireTrail()
        {
            if (Hull.CurrentHitpoints / Hull.MaxHitpoints <= 0.5f)
            {
                if (Smoke == null)
                {
                    Smoke = new SmokeTrail();
                    Smoke.NoReplication = true;
                    Root.Instance.Scene.Spawn(Smoke);
                    Smoke.Attach = this;

                    FireLight = new Light();
                    FireLight.NoReplication = true;
                    FireLight.directional = false;
                    FireLight.Range = 1000;
                    FireLight.diffuse = new Color4f(1, 0.8f, 0.1f, 1.0f);
                    FireLight.Attach = this;
                    Root.Instance.Scene.Spawn(FireLight);
                }
            }
            else if (Smoke != null)
            {
                Smoke.Attach = null;
                Smoke = null;

                FireLight.Kill = true;
                FireLight.Attach = null;
                FireLight = null;
            }
        }

        public override void Damage(Damage d)
        {
            if (Kill)
                return;

            if (Shield != null && Shield.CurrentCharge > 0 && (d.Normal > 0.0f || d.Shield > 0.0f || d.System > 0.0f))
            {
                if (ShieldSound != null && ShieldVisible <= 0.5f)
                    PlaySound(ShieldSound, false);
                ShieldVisible = 1;

                if (!Draw.Contains(ShieldModel))
                    Draw.Add(ShieldModel);
            }


            //damage calculation
            if (Root.Instance.IsAuthoritive)
            {
                float through = 1;
                if (Shield != null && Shield.CurrentCharge > 0.0f)
                {
                    if (d.Shield > 0)
                        through = Math.Max(0.0f, (d.Shield - Shield.CurrentCharge) / d.Shield);
                    else
                        through = 0;
                    Shield.CurrentCharge = Math.Max(0.0f, Shield.CurrentCharge - d.Shield);
                }
                float hp = d.Normal * through + d.ShieldPiercing;
                Hull.Damage(hp);
                Battery.CurrentEnergy = Math.Max(0.0f, Battery.CurrentEnergy - d.System * through);
                if (!ControlsJamed && Battery.CurrentEnergy == 0.0f && (Shield == null || Shield.CurrentCharge <= 0.0f))
                {
                    //Cheetah.Console.WriteLine("control jamed.");
                    ControlsJamed = true;
                }
            }

            UpdateFireTrail();


            if (!Root.Instance.IsAuthoritive)
                return;


            if (Hull.CurrentHitpoints == 0.0f)
            {
                //besser in onkill spawnen?
                Root.Instance.EventSendQueue.Add(new EventReplicationInfo("SpawnExplosion", this));
                SpawnExplosion();
                Kill = true;
            }
        }

        //static float count=0;
        public void Fire()
        {
            Fire(0);
            //Damage(100);
        }


        public static readonly Type[] WeaponList = new Type[] { typeof(LaserCannon), typeof(HomingMissileLauncher), typeof(DisruptorCannon), typeof(IonPulseCannon), typeof(MineLayer), typeof(RailGun), typeof(PulseLaserCannon), null };

        protected int GetWeaponNumber(Type t)
        {
            return Array.IndexOf<Type>(WeaponList, t);
        }

        public bool CanArmWeapon(Type t)
        {
            return (t!=null) &&(Inventory.Count(t) > NumberOfWeaponsMounted(t));
        }

        protected int NumberOfWeaponsMounted(Type t)
        {
            int n = GetWeaponNumber(t);
            int c = 0;
            for (int i = 0; i < Slots.Length; ++i)
            {
                if (WeaponNumbers[i] == n) c++;
            }
            return c;
        }

        public int NextAvailableWeapon(int w)
        {
            //Cheetah.Console.WriteLine("weapon now: " + w);
            for (int next = (w + 1) % WeaponList.Length; next != w; next = (next + 1) % WeaponList.Length)
            {
                if (WeaponList[next] == null)
                    //return next;
                    continue;
                if (CanArmWeapon(WeaponList[next]))
                {
                    //Cheetah.Console.WriteLine("weapon next: " + next);
                    return next;
                }
            }
            return -1;
        }

        public int PreviousAvailableWeapon(int w)
        {
            //Cheetah.Console.WriteLine("weapon now: " + w);
            for (int next = w > 0 ? (w - 1) : (WeaponList.Length - 1); 
                next != w;
                next = next > 0 ? (next - 1) : (WeaponList.Length - 1))
            {
                if (WeaponList[next] == null)
                    //return next;
                    continue;
                if (CanArmWeapon(WeaponList[next]))
                {
                    //Cheetah.Console.WriteLine("weapon next: " + next);
                    return next;
                }
            }
            return -1;
        }
        public Player GetPlayer()
        {
            return (Player)Owner;
        }

        public int[] WeaponNumbers;
        public void ArmWeapon(int w, int slot)
        {
            //HACK
            //if (!Slots[slot].Ready)
            //    return;

            Slots[slot].Ready = false;

            if (WeaponList[w] != null)
            {
                Weapon w1 = (Weapon)Activator.CreateInstance(WeaponList[w]);
                Slots[slot].Weapon = w1;
                w1.Connect(Inventory, Battery);
                WeaponNumbers[slot] = w;
                //Cheetah.Console.WriteLine("armed " + WeaponList[w].Name + " in slot " + slot.ToString());
            }
            else
            {
                WeaponNumbers[slot] = -1;
                Slots[slot].Weapon = null;
                //Cheetah.Console.WriteLine("unarmed slot " + slot.ToString());
            }

        }

        public void ArmWeaponOnAllSlots(int w)
        {
            //System.Console.WriteLine("arm on all slots: " + WeaponList[w].Name);
            Type t = WeaponList[w];
            for (int i = 0; i < Slots.Length; ++i)
            {
                if (CanArmWeapon(t))
                {
                    ArmWeapon(w, i);
                }
                else
                {
                    WeaponNumbers[i] = -1;
                    Slots[i].Weapon = null;
                }
            }
        }

        public int CurrentWeapon=0;

        public void PreviousWeapon()
        {
            int w = PreviousAvailableWeapon(CurrentWeapon);
            if (w != CurrentWeapon)
            {
                ArmWeaponOnAllSlots(w);
                CurrentWeapon = w;
            }
        }
        public void NextWeapon()
        {
            int w = NextAvailableWeapon(CurrentWeapon);
            if (w != CurrentWeapon)
            {
                ArmWeaponOnAllSlots(w);
                CurrentWeapon = w;
            }
        }

        public void CycleWeapon(int slot)
        {
            if (!Slots[slot].Ready)
                return;
            int w = WeaponNumbers[slot];
            int next = NextAvailableWeapon(w);
            if (next >= 0)
                ArmWeapon(next, slot);
        }

        public void FireEvent(string slot)
        {
            int slotnr = int.Parse(slot);
            FireSlot(slotnr, false);
        }

        public void Fire(int group)
        {
            for (int i = 0; i < Slots.Length; ++i)
            {
                Slot s = Slots[i];
                if (s.Group == group && s.Weapon != null && s.Weapon.Ready)
                {
                    FireSlot(i, true);
                }
            }
        }

        protected void FireSlot(int slotnr, bool send)
        {
            Slot s = Slots[slotnr];
            if (s != null)
            {
                if (s.Weapon != null)//&&s.Weapon.Ready)
                {
                    if (send)
                        Root.Instance.EventSendQueue.Add(new EventReplicationInfo("FireEvent", this, new string[] { slotnr.ToString() }));

                    s.Weapon.Fire(this, s);
                }
            }
            else
                foreach (Slot slot in Slots)
                {
                    if (slot.Selected)
                    {
                        //if (slot.Weapon.Ready)
                        slot.Weapon.Fire(this, slot);
                    }
                }
        }

        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, Radius);
        }

        //controls
        public float ThrustPower;
        public float StrafePower;
        public float RotationPower;

        public float MainThrust;
        public float StrafeThrust;
        public float Resistance;

        //public float CurrentRollSpeed;
        public float Roll;
        public float RollSpeed;
        public float MaxRoll;
        public float MaxRotationSpeed;
        //public float RollAcceleration=1;

        public float Radius;

        //public GenericThruster[] Thrusters;
        public Weapon[] Weapons;
        public Generator Generator;
        //public Shield Shield;
        //public Battery Battery;
        public Slot[] Slots;
        //public Inventory FreightBay;
        public int[] SelectedSlots;
        //public Hull Hull;
        //public Computer Computer;
        public SmokeTrail Smoke;
        public float ShieldVisible = 0.0f;
        public IDrawable ShieldModel;
        public Vector3 CollisionDirection;
        public Sound ShieldSound;
        public bool ControlsJamed = false;
        #region IActor Members

        #endregion
    }

    public class DominationPoint : Node
    {
        public DominationPoint()
        {
            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("dominationpoint/dominationpoint.mesh"));
            Transparent = 1;
            SyncRefs = false;
            RenderRadius = 100;
            Team = -1;
        }

        public override void SetRenderParameters(IRenderer r, IDrawable draw, Shader shade)
        {
            base.SetRenderParameters(r, draw, shade);

            int loc = shade.GetUniformLocation("Color");
            if (loc >= 0)
            {
                float f = 1.0f;
                Color4f c = new Color4f(f, f, f, 0.5f);
                if (Team >= 0)
                    c = SpaceWar2006.GameObjects.Team.Colors[Team];
                r.SetUniform(loc, new float[] { c.r * f, c.g * f, c.b * f, 0.5f });
            }
            else throw new Exception();
        }

        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(Position, 100);
        }
        public DominationPoint(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Team = ((int)context.ReadByte()) - 1;
            Name = context.ReadString();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            context.Write((byte)(Team + 1));
            context.Write(Name);
        }

        public override void OnCollide(Node other)
        {
            base.OnCollide(other);

            SpaceShip s = other as SpaceShip;
            if (s != null)
            {
                Player p = s.GetPlayer();
                ChangeTeam(p.Team);
            }
        }

        protected void ChangeTeam(int team)
        {
            if (Team != team)
            {
                Cheetah.Console.WriteLine(Name + " is now " + (team >= 0 ? SpaceWar2006.GameObjects.Team.ColorNames[team] : "neutral") + ".");
                Team = team;
            }
        }

        [Editable]
        public int Team;
        [Editable]
        public string Name;
    }

    public class RacePlayer : Player
    {
        public RacePlayer(short clientid, string name)
            : base(clientid, name)
        {
        }
        public RacePlayer(DeSerializationContext context)
            : base(context)
        {
            //DeSerialize(context);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Team = ((int)context.ReadByte()) - 1;
            Name = context.ReadString();
            Checks = context.ReadInt16();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            context.Write((byte)(Team + 1));
            context.Write(Name);
            context.Write((short)Checks);
        }
        public int Checks = -1;
    }



    public class TargetInfo
    {
        public string Name;
        public float Shield;
        public float Hull;
        public float Speed;
        public float Dist;
    }
    public class CheckPoint : Node
    {
        public CheckPoint(int index, Race race)
            : this()
        {
            CheckPointIndex = index;
            Race = race;
        }
        public CheckPoint()
        {
            NoReplication = true;
            SyncRefs = false;

            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("flag/flag.mesh"));
        }
        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, 100);
        }

        public int CheckPointIndex;

        public Race Race;

        public override void OnCollide(Node other)
        {
            base.OnCollide(other);

            if (Race == null)
                Race = Root.Instance.Scene.FindEntityByType<Race>();

            if (Root.Instance.IsAuthoritive && other is SpaceShip && Race != null)
            {
                SpaceShip s = (SpaceShip)other;

                RacePlayer p = s.GetPlayer() as RacePlayer;

                if (p != null)
                    Race.Reach(p, this);
            }
        }
    }

    public class Slot : ITickable
    {
        public Slot(Weapon w, Vector3 p, Quaternion q, int group)
        {
            Weapon = w;
            Position = p;
            Orientation = q;
            Group = group;
        }

        public void Tick(float dtime)
        {
            MountTime -= dtime;
        }

        
        public virtual Matrix4 Matrix
        {
            get
            {
                    Matrix4 m = Matrix4Extensions.FromQuaternion(Orientation);

                    m.Row3.X = Position.X;
                    m.Row3.Y = Position.Y;
                    m.Row3.Z = Position.Z;
                    return m;
            }
        }

        public bool Ready
        {
            get
            {
                return MountTime <= 0;
            }
            set
            {
                if (value)
                    MountTime = 0;
                else
                    MountTime = 1;
            }
        }
        public Weapon Weapon;
        public Vector3 Position;
        public Quaternion Orientation;
        public bool Selected;
        public int Group;
        public float MountTime = 0;
    }

    [Editable]
    public class Nebula : Node
    {
        public Nebula()
        {
            RenderRadius = 500;
            Transparent = 4;
            Draw = new ArrayList();
            Draw.Add(new ParticleNebula());
        }

        public override bool DrawLocal(IDrawable d)
        {
            return true;
        }

        public override bool CanCollide(Node other)
        {
            return other is SpaceShip;
        }

        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(this.Position, 500);
        }

        public override void OnCollide(Node other)
        {
            if (!Root.Instance.IsAuthoritive)
                return;

            SpaceShip s = (SpaceShip)other;
            s.Damage(Damage * Root.Instance.TickDelta);
        }

        Damage Damage = new Damage(1, 30, 0, 10);
    }

    public class Actor : Node
    {
        public Actor()
        {
        }
        public Actor(DeSerializationContext context)
        {
        }
        public virtual void Damage(Damage d)
        {
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            int target = context.ReadInt32();
            if (!IsLocal)
                Owner = (PlayerEntity)Root.Instance.Scene.ServerListGet(target);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            int target = (Owner != null) ? Owner.ServerIndex : -1;
            context.Write(target);
        }

        public virtual string Name
        {
            get
            {
                return "";
            }
        }

        public float Rotation
        {
            get
            {
                Vector3 d = Direction;
                d.Y = 0;
                d.Normalize();
                return -(float)Math.Atan2(d.X, d.Z);
            }
            set
            {
                Orientation = QuaternionExtensions.FromAxisAngle(0, 1, 0, value);
            }
        }

        public float GetCosDirection(Vector3 target)
        {
            Vector2 v1 = new Vector2(Position.X, Position.Z);
            Vector2 v2 = new Vector2(target.X, target.Z);
            Vector2 want = (v2 - v1);
            want.Normalize();
            Vector2 left = new Vector2(Left.X, Left.Z);

            float cos = Vector2.Dot(left, want);
            //System.Console.WriteLine(want.ToString() + left.ToString() + cos.ToString());
            MathUtil.Check(new float[] { cos });
            return -cos;
                /*
            //left vector projected on plane
            Vector3 left = Left;
            left.Y = 0;
            left.Normalize();

            //wanted direction vector
            Vector3 want;
            try
            {
                want = target - Position;
                want.Normalize();
            }
            catch (DivideByZeroException)
            {
                System.Console.WriteLine("divide bug./%&$");
                want = Direction;
            }

            float cos = Vector3.Dot(left, want);

            //HACK?
            if (cos == 0)
            {
                Vector3 f = Direction;
                f.Y = 0;
                f.Normalize();
                float cos2 = Vector3.Dot(f, want);
                if (cos2 == -1)
                    cos = 1;
            }
            
            System.Console.WriteLine(cos);
            MathUtil.Check(new float[] { cos });
            return cos;*/
        }

        public Hull Hull;

        public Shield Shield;

        public PlayerEntity Owner;
        public Computer Computer;
        public Inventory Inventory;
        public Battery Battery;
    }

    public class Battery
    {
        public Battery(float maxenergy)
        {
            CurrentEnergy = MaxEnergy = maxenergy;
        }

        public void Charge(float energy)
        {
            CurrentEnergy = Math.Min(CurrentEnergy + energy, MaxEnergy);
        }

        public float MaxEnergy;

        public float CurrentEnergy;

        public float CurrentCharge
        {
            get
            {
                return CurrentEnergy / MaxEnergy;
            }
            set
            {
                CurrentEnergy = value * MaxEnergy;
            }
        }
    }

    public class Generator : ITickable
    {
        public Generator(float power, Battery b)
        {
            ConnectedBattery = b;
            Power = power;
        }

        public void Tick(float dtime)
        {
            ConnectedBattery.Charge(Power * dtime);
        }

        public float Power;
        public Battery ConnectedBattery;
    }

    public class Inventory : ISerializable
    {
        public Inventory(int capacity)
        {
            Capacity = capacity;
        }

        public int Count(Type t)
        {
            if (Freight.ContainsKey(t))
            {
                //object o=Freight[t];
                return Freight[t];
            }
            else
            {
                return 0;
            }
        }

        public void Unload(Type t, int n)
        {
            if (Freight.ContainsKey(t))
            {
                Freight[t] = ((int)Freight[t]) - n;
            }
            else
            {
                throw new Exception("no such cargo.");
            }
        }

        public int Used
        {
            get
            {
                int f = 0;
                foreach (KeyValuePair<Type, int> de in Freight)
                {
                    f += FreightSize(de.Key) * de.Value;
                }
                return f;
            }
        }

        static int FreightSize(Type t)
        {
            FieldInfo fi = t.GetField("FreightSize", BindingFlags.Public | BindingFlags.Static);
            int f = (int)fi.GetValue(null);
            return f;
        }

        public void Load(Type t, int n)
        {
            //float f=FreightSize(t);
            if (LoadEvent != null)
            {
                for (int i = 0; i < n; ++i)
                    LoadEvent(t);
            }
            if (Freight.ContainsKey(t))
            {
                Freight[t] = Freight[t] + n;
            }
            else
            {
                Freight[t] = n;
            }
        }

        public float Free
        {
            get
            {
                return Capacity - Used;
            }
        }

        public int Capacity;
        public Dictionary<Type, int> Freight = new Dictionary<Type, int>();
        public delegate void LoadDelegate(Type t);
        public event LoadDelegate LoadEvent;

        #region ISerializable Members

        public void Serialize(SerializationContext context)
        {
            context.Write((short)Capacity);
            context.Write((short)Freight.Count);
            foreach (KeyValuePair<Type, int> kv in Freight)
            {
                context.Write(Root.Instance.Factory.GetClassId(kv.Key.FullName));
                context.Write((short)kv.Value);
            }
        }

        public void DeSerialize(DeSerializationContext context)
        {
            int tmp = context.ReadInt16();
            int c = context.ReadInt16();

            if (!Root.Instance.IsAuthoritive)
            {
                Capacity = tmp;
                Freight.Clear();
                for (int i = 0; i < c; ++i)
                {
                    Type t = Root.Instance.Factory.GetType(context.ReadInt16());
                    Freight[t] = context.ReadInt16();
                }
            }
            else
            {
                for (int i = 0; i < c; ++i)
                {
                    context.ReadInt16();
                    context.ReadInt16();
                }
            }
        }

        #endregion
    }

    public class Shield : ITickable
    {
        public Shield(Battery source, float maxenergy, float chargespeed)
        {
            MaxEnergy = maxenergy;
            ChargeSpeed = chargespeed;
            ConnectedBattery = source;
            CurrentCharge = MaxEnergy;
        }

        public void Tick(float dtime)
        {
            if (CurrentCharge <= 0.0f)
                return;
            CurrentCharge = Math.Min(
                CurrentCharge + Math.Min(ChargeSpeed * dtime, ConnectedBattery.CurrentEnergy),
                MaxEnergy
                );
        }

        public float MaxEnergy;
        public float ChargeSpeed;
        public float CurrentCharge;
        public Battery ConnectedBattery;
    }

}
