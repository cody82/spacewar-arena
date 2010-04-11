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

using SpaceWar2006.Rules;
using SpaceWar2006.GameObjects;
using SpaceWar2006.GameSystem;
using SpaceWar2006.Ai;
using SpaceWar2006.Windows;
using SpaceWar2006.Cameras;
using SpaceWar2006.Controls;
using SpaceWar2006.Weapons;
using SpaceWar2006.Ships;

using Cheetah;


namespace SpaceWar2006.Flows
{

    public class MapList
    {
        public struct Entry
        {
            Type Rule;
            Type Map;
            int BotCount;
        }

        List<Entry> List = new List<Entry>();
    }

    public class GameServer : Flow
    {
        QueryServer qs;

        protected GameRule CreateDeathMatch()
        {
            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");

            return new DeathMatch(c.GetInteger("deathmatch.fraglimit"), c.GetFloat("deathmatch.timelimit"));
        }
        protected GameRule CreateKingOfTheHill()
        {
            return new KingOfTheHill();
        }
        protected GameRule CreateDomination()
        {
            return new Domination(30, 10);
        }
        protected GameRule CreateMission()
        {
            return new Mission();
        }
        protected GameRule CreateRace()
        {
            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");

            return new Race(c.GetInteger("race.laps"));
        }
        protected GameRule CreateTeamDeathMatch()
        {
            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");

            int numteams = c.GetInteger("teamdeathmatch.teams");
            Team[] teams = new Team[numteams];
            for (int i = 0; i < teams.Length; ++i)
            {
                teams[i] = new Team(i, c.GetString("teamdeathmatch.team" + i + ".name"));
            }

            return new TeamDeathMatch(teams, c.GetInteger("teamdeathmatch.teamscorelimit"), c.GetFloat("teamdeathmatch.timelimit"));
        }
        protected GameRule CreateCaptureTheFlag()
        {
            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");

            int numteams = c.GetInteger("capturetheflag.teams");
            CtfTeam[] teams = new CtfTeam[numteams];
            for (int i = 0; i < teams.Length; ++i)
            {
                teams[i] = new CtfTeam(i, c.GetString("capturetheflag.team" + i + ".name"));
            }

            return new CaptureTheFlag(teams, c.GetInteger("capturetheflag.teamscorelimit"), c.GetFloat("capturetheflag.timelimit"));
        }
        public GameRule NextRule;
        public Map NextMap;
        public int NextBotCount;

        public override ISerializable Query()
        {
            return new GameServerInfo();
        }

        public void SetDefault()
        {
            Cheetah.Console.WriteLine("loading defaults from config.");
            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");

            NextMap = (Map)Activator.CreateInstance(Root.Instance.Factory.GetType(c.GetString("gameserver.map")));

            if (c.GetString("gameserver.rule") == typeof(SpaceWar2006.Rules.DeathMatch).FullName)
                NextRule = CreateDeathMatch();
            else if (c.GetString("gameserver.rule") == typeof(SpaceWar2006.Rules.TeamDeathMatch).FullName)
                NextRule = CreateTeamDeathMatch();
            else if (c.GetString("gameserver.rule") == typeof(SpaceWar2006.Rules.KingOfTheHill).FullName)
                NextRule = CreateKingOfTheHill();
            else if (c.GetString("gameserver.rule") == typeof(SpaceWar2006.Rules.Domination).FullName)
                NextRule = CreateDomination();
            else if (c.GetString("gameserver.rule") == typeof(SpaceWar2006.Rules.Race).FullName)
                NextRule = CreateRace();
            else if (c.GetString("gameserver.rule") == typeof(SpaceWar2006.Rules.CaptureTheFlag).FullName)
                NextRule = CreateCaptureTheFlag();
            else if (c.GetString("gameserver.rule") == typeof(SpaceWar2006.Rules.Mission).FullName)
                NextRule = NextMap.Mission;
            else throw new Exception("unknown rule.");


            NextBotCount = c.GetInteger("server.bots");
        }

        public GameServer(GameRule rule, Map map, int botcount)
        {
            NextMap = map;
            NextBotCount = botcount;
            NextRule = rule;
        }
        public GameServer()
        {
            Mod.Instance.Init();
        }
        public IrcReporter reporter;

        public override void Start()
        {
            base.Start();
            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");

            Root.Instance.Scene.Clear();


            if (NextMap == null || NextRule == null)
                SetDefault();

            Root.Instance.Scene.Spawn(NextMap);
            NextMap = null;

            if (NextRule == null || NextRule is Mission)
                NextRule = Root.Instance.Scene.FindEntityByType<GameRule>();
            else
                Root.Instance.Scene.Spawn(NextRule);
            Rule = NextRule;
            NextRule = null;

            Rule.AnnounceEvent += Cheetah.Console.WriteLine;

            if (Root.Instance.UserInterface==null && c.GetBool("server.queryport.enable"))
            {
                int queryport = c.GetInteger("server.queryport");
                if (queryport > 0 && qs == null)
                    Root.Instance.LocalObjects.Add(qs = new QueryServer(queryport, new IQuery[] { new OldGameSpyQuery() }));
            }





            if (Root.Instance.UserInterface==null && c.GetBool("irc.enable") && reporter == null)
            {
                reporter = new IrcReporter(c.GetString("irc.host"), c.GetInteger("irc.port"), c.GetString("irc.nick"), c.GetString("irc.realname"), c.GetString("irc.channels").Split(','));
                Root.Instance.LocalObjects.Add(reporter);
                Rule.AnnounceEvent += reporter.Announce;
            }

            int botcount = NextBotCount;
            Bots = new SpaceShipBotControl[botcount];
            BotPlayers = new Player[botcount];
            for (int i = 0; i < botcount; ++i)
            {
                int team = (Rule is TeamDeathMatch) ? c.GetInteger("server.bot" + i + ".team") : -1;
                string name = c.GetString("server.bot" + i + ".name");
                SpawnBot(i, team, name);
            }
            //SpawnBot(1);
        }
        PlayerStart GetSpawnLocation()
        {
            IList<PlayerStart> list = Root.Instance.Scene.FindEntitiesByType<PlayerStart>();
            int pos = VecRandom.Instance.Next(0, list.Count);
            return list[pos];
        }
        void SpawnBot(int slot, int team, string name)
        {
            Player p = BotPlayers[slot];
            if (p == null)
            {
                if (name == null)
                    throw new Exception();
                p = BotPlayers[slot] = Rule.CreatePlayer(0, name);
                p.Team = team;
                Root.Instance.Scene.Spawn(p);
            }
            SpaceShip s = new Dreadnaught();
            //SpaceShip s = new Sulaco();
            s.Owner = p;
            PlayerStart ps = GetSpawnLocation();
            s.Position = ps.AbsolutePosition;
            s.Orientation = ps.Orientation;
            Root.Instance.Scene.Spawn(s);
            Bots[slot] = new SpaceShipBotControl(s);
        }

        SpaceShipBotControl[] Bots = new SpaceShipBotControl[2];
        Player[] BotPlayers;
        GameRule Rule;

        public override void OnCollision(Entity e1, Entity e2)
        {
            /*Projectile p = null;
            SpaceShip ss = null;
            SpaceShip other = null;
            if (e1 is Projectile)
                p = (Projectile)e1;
            else if (e2 is Projectile)
                p = (Projectile)e2;
            if (e1 is SpaceShip)
            {
                ss = (SpaceShip)e1;
                if (e2 is SpaceShip)
                    other = (SpaceShip)e2;
            }
            else if (e2 is SpaceShip)
            {
                ss = (SpaceShip)e2;
                if (e1 is SpaceShip)
                    other = (SpaceShip)e1;
            }

            if (ss == null)
                return;

            if (ss.Kill)
            {
                SpaceShip ss2 = null;
                if (p != null)
                {
                    ss2 = (SpaceShip)p.Source;
                    Rule.ActorDestroy(ss2, ss, p);
                }
                else if (other != null)
                {
                    Rule.ActorDestroy(null, other, p);
                    Rule.ActorDestroy(null, ss, p);
                }
                else
                {
                    Rule.ActorDestroy(null, ss, p);
                }
            }*/
            //System.Console.WriteLine(e1.ToString()+" "+e2.ToString());

            Projectile p = null;
            Actor ss = null;
            Actor other = null;
            if (e1 is Projectile)
                p = (Projectile)e1;
            else if (e2 is Projectile)
                p = (Projectile)e2;
            if (e1 is Actor)
            {
                ss = (Actor)e1;
                if (e2 is Actor)
                    other = (Actor)e2;
            }
            else if (e2 is Actor)
            {
                ss = (Actor)e2;
                if (e1 is Actor)
                    other = (Actor)e1;
            }

            if (ss == null)
                return;

            if (ss.Kill)
            {
                Actor ss2 = null;
                if (p != null)
                {
                    ss2 = (Actor)p.Source;
                    Rule.ActorDestroy(ss2, ss, p);
                }
                else if (other != null)
                {
                    if(other.Kill)
                        Rule.ActorDestroy(null, other, p);
                    Rule.ActorDestroy(null, ss, p);
                }
                else
                {
                    Rule.ActorDestroy(null, ss, p);
                }
            }
            else if (other!=null && other.Kill)
            {
                Actor ss2 = null;
                if (p != null)
                {
                    ss2 = (Actor)p.Source;
                    Rule.ActorDestroy(ss2, other, p);
                }
                else// if (other != null)
                {
                    Rule.ActorDestroy(null, other, p);
                    //Rule.ActorDestroy(null, ss, p);
                }
                //else
                //{
                //    Rule.ActorDestroy(null, ss, p);
                //}
            }
        }

        public override void OnJoin(short clientid, string name)
        {
            //Stats.Players.Add(clientid, new Player(clientid, name));
        }
        public override void OnLeave(short clientid, string name)
        {
            IList<Player> list = Root.Instance.Scene.FindEntitiesByType<Player>();
            foreach (Player p in list)
            {
                if (p.ClientId == clientid)
                    p.Kill = true;
            }
        }

        public override void Tick(float dtime)
        {
            for (int i = 0; i < Bots.Length; ++i)
            {
                if (Bots[i] != null)
                {
                    if (Bots[i].Target.Kill)
                    {
                        SpawnBot(i, -1, null);
                    }
                    else
                        Bots[i].Tick(dtime);
                }

            }
            IList<Player> list = Root.Instance.Scene.FindEntitiesByType<Player>();
            foreach (Player p in list)
            {
                if (p.Team >= 0 && !(Rule is TeamDeathMatch || Rule is Domination || Rule is Mission))
                {
                    Cheetah.Console.WriteLine(p.Name + ": team not allowed.");
                    p.Team = -1;
                }
            }


            base.Tick(dtime);
            //Rule.Tick(dtime);
        }

        public void Restart()
        {
            Root.Instance.ServerConnection.RemoveAllClients();
            Root.Instance.Scene.Clear();
            Cheetah.Console.WriteLine("preparing server restart...");
            Thread.Sleep(1000);
            Root.Instance.CurrentFlow = this;
            Finished = false;
            Start();
            Cheetah.Console.WriteLine("done.");
        }

        public override void Stop()
        {
            base.Stop();

            Cheetah.Console.WriteLine("gameserver stopped.");
            Root.Instance.ServerConnection.RemoveAllClients();
            Root.Instance.Scene.Clear();

            if (qs != null)
            {
                Root.Instance.LocalObjects.Remove(qs);
                qs.Dispose();
                qs = null;
            }

            if (reporter != null)
            {
                Root.Instance.LocalObjects.Remove(reporter);

                reporter.Dispose();
                reporter = null;
            }
            //Restart();
        }

        //GameStats Stats;
    }

    public class PhysicsTest : Flow
    {

        public override void Start()
        {
            base.Start();
            Root.Instance.Gui.windows.Clear();
            Root.Instance.Scene.Clear();
            Camera c = Root.Instance.Scene.camera = new Camera();
            Root.Instance.Scene.Spawn(c);
            c.Position = new Vector3(2000, 2000, 2000);
            c.LookAt(0, 0, 0);

            Node n = new Cheetah.Physics.PhysicsNode();
            Root.Instance.Scene.Spawn(n);
            n.Position = new Vector3(0, 3000, 0);

            n = new Cheetah.Physics.PhysicsNode();
            Root.Instance.Scene.Spawn(n);
            n.Position = new Vector3(0, 1000, 0);
            n.Speed = new Vector3(0, 100, 0);
            n.rotationspeed.Y = 1;

            l = new Light();
            //l.Position = new Vector3(10, 10, -10);
            //l.Draw.Add(new Marker());
            l.Position = new Vector3(2000, 2000, 2000);
            Root.Instance.Scene.Spawn(l);


        }
        Light l;

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
        }
        public override void Stop()
        {
            base.Stop();
        }
    }

    public class Viewer : Flow
    {
        public Viewer(string mesh)
        {
            //SetMesh("units/UEL0001");
            //bsp = new BSPFile("maps\\q3dm17.bsp");
            //bsp = new BSPFile("maps\\Egyptian Palace.bsp");
            SetMesh(mesh);
        }

        protected void SetMesh(string mesh)
        {
            //mesh = "models/supcom/default.mesh";
            //m = Root.Instance.ResourceManager.Load<SkeletalMesh>(mesh);

            //try
            {
                m = Root.Instance.ResourceManager.Load<Mesh>(mesh);
            }
            //catch (Exception e)
            //{
            //    m = Root.Instance.ResourceManager.Load<Model>(mesh);
            //}
            //

            //m = Root.Instance.ResourceManager.Load<SupComMap>("maps/SCMP_005/SCMP_005.scmap");

            if (m is Model)
            {
                bbox = ((Model)m).Mesh.BBox;
            }
            else if (m is Mesh)
            {
                bbox = ((Mesh)m).BBox;
            }
        }

        protected void ChangeMesh()
        {
            n.Draw.Clear();
            n.Draw.Add(m);
            ViewAll();
        }
        public void ChangeMesh(string mesh)
        {
            SetMesh(mesh);
            ChangeMesh();
        }
        public void ChangeAnim(int index)
        {
            ((Model)m).CurrentAnimation = index;
        }

        public override void Start()
        {
            base.Start();
            Root.Instance.Gui.windows.Clear();
            Root.Instance.Scene.Clear();
            c = Root.Instance.Scene.camera = new Camera();
            //c.rotationspeed = new Vector3(0, 1, 0);
            //c.Orientation = Quaternion.FromAxisAngle(Vector3.YAxis, (float)Math.PI);
            Root.Instance.Scene.Spawn(c);
            n = new Node();
            //n.Position = new Vector3(0, 0, 0);
            Root.Instance.Scene.Spawn(n);
            //n.rotationspeed = new Vector3(0, 1, 0);
            //n.Position = new Vector3(1, 1, 1);
            //n.Speed = new Vector3(10, 1, 1);

            l = new Light();
            //l.Position = new Vector3(10, 10, -10);
            l.Draw.Add(new Marker());
            l.Position = new Vector3(10, 10, 10);
            //l.rotationspeed = new Vector3(0, 1, 0);
            //Root.Instance.UserInterface.Audio.Play(Root.Instance.ResourceManager.LoadSound("z57_v0d.xm"), l.Position,true);
            l.diffuse = new Color4f(1, 1, 1, 1);

            Root.Instance.Scene.Spawn(l);
            /*
            l = new Light();
            l.Draw.Add(new Marker());
            l.Position = new Vector3(-10, 10, 10);
            l.diffuse = new Color4f(1, 0, 0, 1);
            Root.Instance.Scene.Spawn(l);

            l = new Light();
            l.Draw.Add(new Marker());
            l.Position = new Vector3(10, -10, 10);
            l.diffuse = new Color4f(0, 0, 1, 1);
            Root.Instance.Scene.Spawn(l);*/
            
            ChangeMesh();

        }

        public override void Stop()
        {
            base.Stop();
        }
        public override void OnDraw()
        {
            /*Root.Instance.UserInterface.Renderer.SetCamera(c);
            Frustum f = new Frustum();
            f.GetFrustum(Root.Instance.UserInterface.Renderer);
            Material m = new Material();
            m.DepthTest = true;
            m.DepthWrite = true;
            m.twosided = true;
            Root.Instance.UserInterface.Renderer.SetMaterial(m);
            Root.Instance.UserInterface.Renderer.UseShader(Root.Instance.ResourceManager.LoadShader("simple3d.textured.shader"));
            bsp.RenderLevel(c.Position, f);*/
        }
        BoundingBox? bbox = null;
        public void ViewAll()
        {

            if(bbox.HasValue)
                dist = bbox.Value.Radius / (float)Math.Sin(c.Fov / 180.0f * (float)Math.PI / 2.0f);
            else
                dist = 10;// 

            //dist = 2000;
            // ( Set the clipping planes )
            //m_near = m_camDist - r;
            //a bit more distance for the far plane
            //m_far = m_camDist + 2 * r

            c.Position = new Vector3(1, 1, 1).GetUnit() * dist;
            if (bbox.HasValue)
                c.LookAt(bbox.Value.Center);
            else
                c.LookAt(0, 0, 0);
            l.Position = new Vector3(0, 1, 1).GetUnit() * dist;
            if (bbox.HasValue)
                l.LookAt(bbox.Value.Center);
            else
                l.LookAt(0, 0, 0);

        }

        //BSPFile bsp;

        bool Rotate()
        {
            return Root.Instance.UserInterface.Keyboard.GetButtonState('r') || Root.Instance.UserInterface.Keyboard.GetButtonState('l')
                ||Root.Instance.UserInterface.Mouse.GetButtonState(0);
        }
        bool Distance()
        {
            return Root.Instance.UserInterface.Keyboard.GetButtonState('d') || Root.Instance.UserInterface.Keyboard.GetButtonState('k')
                || Root.Instance.UserInterface.Mouse.GetButtonState(1);
        }

        Node Select()
        {
            switch (Selection)
            {
                case Objects.None:
                    if (Root.Instance.UserInterface.Keyboard.GetButtonState('r') || Root.Instance.UserInterface.Keyboard.GetButtonState('d'))
                    {
                        return c;
                    }
                    else
                    {
                        return l;
                    }
                case Objects.Light:
                    return l;
                case Objects.Camera:
                    return c;
                default:
                    throw new Exception();
            }
        }

        public enum Objects
        {
            None=0,
            Camera,
            Light
        }

        public Objects Selection = Objects.None;

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            Node selected = Select();

            if (Rotate())
            {
                if (rotate)
                {
                    Vector3 pos = new Vector3(Root.Instance.UserInterface.Mouse.GetPosition(0), Root.Instance.UserInterface.Mouse.GetPosition(1), 0);
                    Vector3 delta = pos - lastpos;
                    lastpos = pos;

                    Quaternion q = Quaternion.FromAxisAngle(0, 1, 0, delta.X * 0.01f) * Quaternion.FromAxisAngle(selected.Left, -delta.Y * 0.01f);
                    selected.Position = q.ToMatrix3().Transform(selected.Position);
                    if (bbox.HasValue)
                        selected.LookAt(bbox.Value.Center);
                    else
                        selected.LookAt(0, 0, 0);
                }
                else
                {
                    rotate = true;
                    lastpos = new Vector3(Root.Instance.UserInterface.Mouse.GetPosition(0), Root.Instance.UserInterface.Mouse.GetPosition(1), 0);
                }
                return;
            }
            else
                rotate = false;


            if (Distance())
            {
                if (zoom)
                {
                    Vector3 pos = new Vector3(Root.Instance.UserInterface.Mouse.GetPosition(0), Root.Instance.UserInterface.Mouse.GetPosition(1), 0);
                    Vector3 delta = pos - lastpos;
                    lastpos = pos;

                    selected.Position += selected.Position * Math.Min(0.5f, delta.Y * 0.01f);

                    if (bbox.HasValue)
                        selected.LookAt(bbox.Value.Center);
                    else
                        selected.LookAt(0, 0, 0);
                }
                else
                {
                    zoom = true;
                    lastpos = new Vector3(Root.Instance.UserInterface.Mouse.GetPosition(0), Root.Instance.UserInterface.Mouse.GetPosition(1), 0);
                }
            }
            else
                zoom = false;


            //Vector3 v = new Vector3((float)Math.Sin((double)Root.Instance.Time), 0.5f, (float)Math.Cos((double)Root.Instance.Time));
            //l.Position = v.GetUnit() * dist;
            //c.Position = l.Position + new Vector3((float)Math.Sin((double)Root.Instance.Time) * 10, 5,(float)Math.Cos((double)Root.Instance.Time) * 10);
            //c.LookAt(l.Position+new Vector3(7,2,-1));


        }
        //Channel chan;
        Node n;
        Light l;
        Camera c;
        public IDrawable m;
        float dist;
        bool rotate=false;
        bool zoom = false;
        bool light = false;
        bool lightdist = false;
        Vector3 lastpos;
    }

    public class ShowRoom : Flow
    {
        public ShowRoom(string filename)
        {
            Root.Instance.Player = new DemoPlayer(filename);
        }

        protected void InitGame()
        {
            Root r = Root.Instance;
            Root.Instance.Gui.windows.Clear();

            PlayerCam = new AdvancedCamera(CameraMode.Normal, null, 50);
            PlayerCam.Position = new Vector3(0, 1.5f, 0);
            //PlayerCam.Smooth = true;


            FlyByCam = new AdvancedCamera(CameraMode.FlyBy, null, 1000);
            //FlyByCam.Smooth = true;

            FreeCam = new AdvancedCamera(CameraMode.Normal);
            FreeCam.Position = new Vector3(0, 50, 0);

            RotateCam = new AdvancedCamera(CameraMode.Normal);
            RotateCam.Position = new Vector3(1429, 202, 62);
            RotateCam.rotationspeed.Y = 45.0f / 180.0f * (float)Math.PI;
            RotateCam.Fov = 90;

            Cameras = new AdvancedCamera[] { FlyByCam, PlayerCam, FreeCam, RotateCam };

            r.Scene.camera = FlyByCam;

            CameraControl = new StandardControl(FreeCam);
            CurrentCameraNumber = 0;
        }

        public override void Start()
        {
            base.Start();


            InitGame();


        }

        public override void OnKeyPress(Key k)
        {
            if (k.Code == KeyCode.c)
            {
                CycleCamera();
            }
            /*if (k.Code == KeyCode.ESCAPE)
            {
                Window w = Root.Instance.Gui.FindWindowByType(typeof(InGameMenu));
                if (w == null)
                    Root.Instance.Gui.windows.Add(new InGameMenu());
                else
                    Root.Instance.Gui.windows.Remove(w);
            }*/
        }

        public Camera CycleCamera()
        {
            CurrentCameraNumber = (CurrentCameraNumber + 1) % Cameras.Length;
            Root.Instance.Scene.camera = Cameras[CurrentCameraNumber];

            return Root.Instance.Scene.camera;
        }

        public Camera CurrentCamera
        {
            get
            {
                return Cameras[CurrentCameraNumber];
            }
        }

        public override void Tick(float dtime)
        {
            Entity ss;
            if ((ss = Root.Instance.Scene.FindEntityByType<SpaceShip>()) != null)
            {
                //Cheetah.Console.WriteLine("spaceship found, switching camera.");
                FlyByCam.Attach = PlayerCam.Attach = (Node)ss;
            }

            CurrentCamera.Tick(dtime);
            if (CurrentCamera == FreeCam)
            {
                CameraControl.Tick(dtime);
            }


            base.Tick(dtime);
        }

        public override void Stop()
        {
            base.Stop();
            Root.Instance.Connection = null;
            Root.Instance.Player = null;
            Root.Instance.Scene = new Scene();
            Root.Instance.LocalObjects.Clear();
            Root.Instance.Gui.windows.Clear();
        }

        //public string host;
        //public SpaceShip LocalPlayer;
        public AdvancedCamera[] Cameras;
        public int CurrentCameraNumber;
        //public SpaceShipStandardControl SpaceShipControl;
        public StandardControl CameraControl;
        public AdvancedCamera PlayerCam;
        public AdvancedCamera FlyByCam;
        public AdvancedCamera FreeCam;
        public AdvancedCamera RotateCam;
    }

    public class GameLog : ITickable
    {
        public class Entry
        {
            public Entry(float time, string text, Vector3 pos, float size)
            {
                Time = time;
                Text = text;
                CurrentPosition = pos;
                CurrentSize = size;

            }

            public float CurrentSize;
            public float Time;
            public string Text;
            public Vector3 CurrentPosition;
        }

        float StartFontSize;
        float FontSize;
        public GameLog()
        {
            float sx = (float)Root.Instance.UserInterface.Renderer.Size.X;
            float sy = (float)Root.Instance.UserInterface.Renderer.Size.Y;

            StartFontSize = sy / 20.0f;
            FontSize = sy / 40.0f;

            /*Animation.Channel pos = new Animation.Channel();
            pos.Frames.Add(new Animation.KeyFrame(0, new float[] { sx / 2, sy * 0.33f }));
            pos.Frames.Add(new Animation.KeyFrame(5, new float[] { sx / 2, 0 }));
            anim.Channels.Add("pos", pos);
            Animation.Channel size = new Animation.Channel();
            size.Frames.Add(new Animation.KeyFrame(0, new float[] { sy/20 }));
            size.Frames.Add(new Animation.KeyFrame(3, new float[] { sy / 20 }));
            size.Frames.Add(new Animation.KeyFrame(5, new float[] { 0 }));
            anim.Channels.Add("size", size);*/

            mf = (MeshFont)Root.Instance.ResourceManager.Load("models/font-arial-black", typeof(MeshFont));

        }
        MeshFont mf;

        public void WriteLine(string text)
        {
            float sx = (float)Root.Instance.UserInterface.Renderer.Size.X;
            float sy = (float)Root.Instance.UserInterface.Renderer.Size.Y;

            Cheetah.Console.WriteLine(text);

            Lines.Add(new Entry(0, text, new Vector3(sx * 0.5f, sy * 0.5f, 0),StartFontSize));
        }

        List<Entry> Lines = new List<Entry>();
        //Animation anim = new Animation();

        #region ITickable Members

        public void Tick(float dtime)
        {
            float sx = (float)Root.Instance.UserInterface.Renderer.Size.X;
            float sy = (float)Root.Instance.UserInterface.Renderer.Size.Y;

            for (int i = 0; i < Lines.Count; ++i)
            {
                Entry e = Lines[i];
                Vector3 wantedpostion = new Vector3(sx * 0.5f, ((float)i+0.5f) * FontSize * 1.1f, 0);

                e.CurrentPosition += (wantedpostion - e.CurrentPosition) * dtime;
                e.CurrentSize += (FontSize - e.CurrentSize) * dtime;

                e.Time += dtime;
                
            }
            if (Lines.Count > 0)
            {
                Vector3 wantedpostion = new Vector3(sx * 0.5f, ((float)0+0.5f) * FontSize * 1.1f, 0);
                if ((Lines[0].CurrentPosition - wantedpostion).GetMagnitude() < 3)
                {
                    Lines.RemoveAt(0);
                }
            }
        }

        #endregion

        public void Draw(IRenderer r)
        {
            r.SetMode(RenderMode.Draw2D);
            foreach (Entry e in Lines)
            {
                string text = e.Text;
                //float[] v = anim.GetValues("pos", e.Time);
                //Vector2 pos = new Vector2(v[0], v[1]);
                //float s = anim.GetValues("size", e.Time)[0];

                mf.Draw(r, text, e.CurrentPosition.X, e.CurrentPosition.Y, new Color4f(1, 1, 1), false, e.CurrentSize * 0.667f, e.CurrentSize);
            }
        }

    }

    public class Game : Flow
    {
        public Game(string host, string demo, bool spectate, bool local)
        {
            Host = host;
            Demo = demo;
            Spectate = spectate;
            if (demo != null && demo != "")
                Spectate = true;

            if (local)
                Server = new GameServer();

            PreCache();
        }

        public Game(GameRule rule, Map map, int botcount,bool spectate)
        {
            Host = null;
            Demo = null;
            Spectate = spectate;

            Server = new GameServer(rule, map, botcount);

            PreCache();
        }

        public override ISerializable Query()
        {
            if (Server != null)
                return Server.Query();
            else
                return null;
        }
        public void ShowText(string text)
        {
            if (Root.Instance.UserInterface != null)
            {
                IRenderer r = Root.Instance.UserInterface.Renderer;
                Cheetah.Font f = Root.Instance.Gui.DefaultFont;
                r.Clear(0, 0, 0, 1);
                r.SetMode(RenderMode.Draw2D);

                string s = text;
                float x = r.Size.X / 2 - f.Width / 2.0f * s.Length;
                float y = r.Size.Y / 2 - f.size / 2.0f;
                f.Draw(r, s, x, y);
                r.Flip();
            }
        }
        public void PreCache()
        {


            foreach (Type t in Root.Instance.Factory.FindTypes(null, typeof(Node), false))
            {
                if (t.GetConstructor(new Type[] { }) != null)
                {
                    ShowText("Precaching: " + t.Name + "...");
                    Cheetah.Console.WriteLine("precaching: " + t.Name);
                    Root.Instance.Factory.CreateInstance(t);
                }
            }
        }

        public GameServer Server;
        public bool Local
        {
            get
            {
                return Server != null;
            }
        }

        GameMenu GameMenu;
        protected void ToggleGameMenu()
        {
            if (GameMenu != null)
            {
                GameMenu.Close();
                GameMenu = null;
            }
            else
            {
                GameMenu = new GameMenu(Player.Team, ShipType);
                Root.Instance.Gui.windows.Add(GameMenu);
                GameMenu.Select += new GameMenu.SelectDelegate(Select);
                //if (ControlMenu.Visible)
                //    ControlMenu.Visible = false;
            }
        }
        public override void OnCollision(Entity e1, Entity e2)
        {
            if (Local)
                Server.OnCollision(e1, e2);
        }
        public override void OnKeyPress(Key k)
        {
            base.OnKeyPress(k);

            if (k.Code == KeyCode.TAB)
            {
                /*if (InfoWindow != null)
                {
                    InfoWindow.Visible = !InfoWindow.Visible;
                }*/
                /*if (ControlMenu != null)
                {
                    ControlMenu.Visible = !ControlMenu.Visible;
                    if (ControlMenu.Visible && GameMenu != null)
                        ToggleGameMenu();
                }*/
            }
            else if (k.Code == KeyCode.ESCAPE)
            {
                ToggleGameMenu();
            }
            else if (Spectate && (k.Code == KeyCode.SPACE))
            {
                IList<SpaceShip> l = Root.Instance.Scene.FindEntitiesByType<SpaceShip>();
                SpaceShip s = MainCamera.Target as SpaceShip;
                if (l.Count > 0)
                {
                    if (s != null)
                    {
                        s = l[(l.IndexOf(s) + 1) % l.Count];
                    }
                    else
                        s = l[0];
                    MainCamera.Target = s;
                }
            }
            else if (Spectate && Root.Instance.Player != null)
            {
                float f = Root.Instance.Player.CurrentTime;
                if (k.Code == KeyCode.LEFT)
                {
                    Root.Instance.Player.GoTo(f - 10);
                }
                else if (k.Code == KeyCode.RIGHT)
                {
                    Root.Instance.Player.GoTo(f + 10);
                }
            }
        }

        Sound Music = Root.Instance.ResourceManager.LoadSound("battle12.mp3");
        Channel MusicChannel;

        public override void Start()
        {
            base.Start();

            Node n;
            Root client = Root.Instance;

            if (Demo != null && Demo != "")
            {
                client.Player = new DemoPlayer(Demo);
                DemoControl dc = new DemoControl();
                dc.Position = new Vector2(0, 40);
                dc.Size = new Vector2(500, 30);
                Root.Instance.Gui.windows.Add(dc);
            }
            else if (!Local)
            {
                Root.Instance.Scene.Clear();
                client.ClientConnect(Host);
            }

            if (Local)
            {
                Root.Instance.Authoritive = true;
                Server.Start();
            }
            MainCamera = new OverviewCamera();
            MainCamera.Aspect = (float)Root.Instance.UserInterface.Renderer.Size.X / (float)Root.Instance.UserInterface.Renderer.Size.Y;
            //MainCamera.View = new Viewport(0, 0, 720, 720);
            //MainCamera = new FollowCamera();
            Root.Instance.LocalObjects.Add(MainCamera);

            MusicChannel = MainCamera.PlaySound(Music, true);

            //FlyByCamera = new AdvancedCamera(CameraMode.FlyBy, null, 1000);
            //FlyByCamera.View = new Viewport(720, 0, 720, 720);
            //Root.Instance.LocalObjects.Add(FlyByCamera);

            Root.Instance.Scene.camera = MainCamera;
            //Root.Instance.Scene.camera2 = FlyByCamera;

            n = new Node();
            //n.Draw.Add(new Cursor(new Color3f(0, 1, 0), 100));
            n.Draw.Add(Root.Instance.ResourceManager.LoadMesh("cursor01/cursor01.mesh"));
            Root.Instance.Scene.Spawn(n);
            n.Transparent = 1;
            n.rotationspeed = new Vector3(0, 90.0f / 180.0f * (float)Math.PI, 0);
            n.NoReplication = true;
            cursor = n;

            //ChatWindow = new Chat(null);
            //Root.Instance.Gui.windows.Add(ChatWindow);

            n = new Node();
            n.Draw.Add(Root.Instance.ResourceManager.LoadMesh("arrow1/arrow1.mesh"));
            Root.Instance.Scene.Spawn(n);
            n.NoReplication = true;
            Arrow = n;
        }

        //Label Message;
        protected void Init()
        {
            Node n;

            Player = Rule.CreatePlayer(Root.Instance.Scene.ClientNumber, Root.Instance.ResourceManager.LoadConfig("config/global.config").GetString("player.name"));
            Player.Team = 0;
            Player.HearEvent += HearPlayer;
            Root.Instance.Scene.Spawn(Player);

            ShipType = typeof(Dreadnaught);

            if (!Spectate)
            {
                SpawnShip();

                PlayerInfo = new ActorInfoWindow();
                Root.Instance.Gui.windows.Add(PlayerInfo);
                TargetInfo = new ActorInfoWindow();
                Root.Instance.Gui.windows.Add(TargetInfo);

            }
            else
            {
                MainCamera.Position = new Vector3(10000, 10000, 10000);
                MainCamera.LookAt(0, 0, 0);
            }

            n = new Node();
            //n.Draw.Add(new Cursor(new Color3f(1, 0, 0), 300));
            n.Draw.Add(Root.Instance.ResourceManager.LoadMesh("cursor01/cursor01.mesh"));
            Root.Instance.Scene.Spawn(n);
            n.rotationspeed = new Vector3(0, 90.0f / 180.0f * (float)Math.PI, 0);
            n.NoReplication = true;
            TargetMarker = n;

            Root.Instance.Scene.Background = Root.Instance.ResourceManager.LoadMesh("skycubemap/skycubemap.mesh");
        }

        public void HearPlayer(Player sender, string text)
        {
            string line = "<" + sender.Name + "> " + text;
            //ControlMenu.Comm.WriteLine(line);
        }
        //WeaponDisplay WeaponInfo;
        //InventoryDisplay InventoryInfo;
        Player Player;
        ActorInfoWindow PlayerInfo;
        ActorInfoWindow TargetInfo;
        Type ShipType;

        PlayerStart GetSpawnLocation()
        {
            IList<PlayerStart> list = Root.Instance.Scene.FindEntitiesByType<PlayerStart>();
            if (list.Count == 0)
                return null;
            int pos = VecRandom.Instance.Next(0, list.Count);
            return list[pos];
        }

        void Select(int team, Type ship)
        {
            playership.Kill = true;
            Player.ChangeTeam(team);
            ShipType = ship;
            SpawnShip();
        }

        void SpawnShip()
        {
            TimeToSpawn = SpawnTime;
            //playership = new TestShip();
            playership = (SpaceShip)Activator.CreateInstance(ShipType);
            PlayerStart ps = GetSpawnLocation();
            if (ps != null)
            {
                playership.Position = ps.AbsolutePosition;
                playership.Orientation = ps.Orientation;
            }

            playership.Owner = Player;
            Root.Instance.Scene.Spawn(playership);
            Control = new SpaceShipControl(playership, cursor);
            
            MainCamera.Target = playership;
            //FlyByCamera.Attach = playership;

           // if (ControlMenu != null)
            //{
                //Rule.AnnounceEvent -= ControlMenu.Comm.WriteLine;
                Rule.AnnounceEvent -= Log.WriteLine;
                //ControlMenu.Close();
            //}
            //Root.Instance.Gui.windows.Add(ControlMenu = new ControlDisplay(300, playership));
            //playership.Computer.TextMonitor = ControlMenu.Comm;
            //Rule.AnnounceEvent += ControlMenu.Comm.WriteLine;
            Rule.AnnounceEvent += Log.WriteLine;

            if (Display != null)
            {
                Display.Close();
            }
            Root.Instance.Gui.windows.Add(Display = new WeaponDisplay(playership.Slots));
            if (Bar != null)
            {
                Bar.Close();
            }
            Root.Instance.Gui.windows.Add(Bar = new WeaponBar(playership,Display));

            if (Radar != null)
            {
                Radar.Close();
            }
            Root.Instance.Gui.windows.Add(Radar = new RadarDisplay());

            //ControlMenu.Ship = playership;
            //ControlMenu.Visible=false;
        }
        WeaponBar Bar;
        WeaponDisplay Display;
        RadarDisplay Radar;

        void UpdateInfo(ActorInfoWindow info, Actor actor)
        {
            if (info == null)
                return;

            if (actor != null && !actor.Kill)
            {
                info.Visible = true;
                Actor t = actor;

                Vector2 v = new Vector2(Root.Instance.UserInterface.Renderer.GetRasterPosition((float[])t.SmoothPosition));
                v.y = Root.Instance.UserInterface.Renderer.Size.Y - v.y;
                info.Position = new Vector2((int)(v.x + 0.5f), (int)(v.y + 0.5f));

                if (t.Shield != null)
                    info.ShieldBar.Value = t.Shield.CurrentCharge / t.Shield.MaxEnergy;
                else
                    info.ShieldBar.Value = 0;
                if (t.Hull != null)
                    info.HitpointBar.Value = t.Hull.CurrentHitpoints / t.Hull.MaxHitpoints;
                else
                    info.HitpointBar.Value = 0;

                if (t.Battery != null)
                    info.EneryBar.Value = t.Battery.CurrentCharge;
                else
                    info.EneryBar.Value = 0;

                /*
                if (t is SpaceShip)
                {
                    SpaceShip s = (SpaceShip)t;
                    info.EneryBar.Value = s.Battery.CurrentCharge;
                }
                else
                    info.EneryBar.Value = 0;
                */

                if (actor.Owner != null)
                {
                    info.Name.Caption = actor.Owner.Name;
                    if (actor.Owner is Player && ((Player)actor.Owner).Team >= 0)
                        info.Name.TextColor = Team.Colors[((Player)actor.Owner).Team];
                    else
                        info.Name.TextColor = new Color4f(1, 1, 1, 1);
                }
                else
                {
                    info.Name.TextColor = new Color4f(1, 1, 1, 1);
                    info.Name.Caption = actor.Name;
                }


            }
            else
                info.Visible = false;
        }

        Ray GetMouseVector()
        {
            IRenderer r = Root.Instance.UserInterface.Renderer;
            r.SetCamera(Root.Instance.Scene.camera);
            //float[] modelview=new float[16];
            //float[] projection=new float[16];

            float x = Root.Instance.UserInterface.Mouse.GetPosition(0);
            float y = r.Size.Y - Root.Instance.UserInterface.Mouse.GetPosition(1);

            x *= (float)r.RenderSize.X / (float)r.WindowSize.X;
            y *= (float)r.RenderSize.Y / (float)r.WindowSize.Y;

            Vector3 v1 = new Vector3(r.UnProject(new float[] { x, y, 1 }, null, null, null));
            Vector3 v2 = new Vector3(r.UnProject(new float[] { x, y, 100000 }, null, null, null));

            UpdateInfo(PlayerInfo, playership);
            if (playership != null)
                UpdateInfo(TargetInfo, playership.Computer.Target);

            return new Ray(v1, v2);
        }

        void UpdateCursorPosition(Ray ray)
        {
            Plane p = new Plane(0, 1, 0, 0);
            try
            {
                cursor.Position = p.GetIntersection(ray.Start, ray.End);
            }
            catch (DivideByZeroException)
            {
            }
        }

        GameRule Rule;

        public override void Tick(float dtime)
        {
            if (Local)
                Server.Tick(dtime);

            base.Tick(dtime);

            Log.Tick(dtime);

            if (MusicChannel != null)
            {
                if (!Root.Instance.UserInterface.Audio.IsPlaying(MusicChannel))
                {
                    MusicChannel = MainCamera.PlaySound(Music, true);
                }
            }

            if (Rule == null)
            {
                Rule = Root.Instance.Scene.FindEntityByType<GameRule>();
                if (Rule != null)
                {
                    //Rule.AnnounceEvent += ControlMenu.Comm.WriteLine;
                }
            }
            if (NeedInit)
            {
                if (Root.Instance.Scene.FindEntityByType<Map>() != null)
                {
                    Init();
                    NeedInit = false;
                }
                else return;
            }


            if (!Spectate)
            {
                Control.Tick(dtime);
                if (playership.Kill)
                {
                    TimeToSpawn -= dtime;
                    if (TimeToSpawn <= 0)
                    {
                        SpawnShip();
                    }
                }
            }
            MainCamera.Tick(dtime);

            UpdateCursorPosition(GetMouseVector());
            if (playership != null && Rule is Race)
            {
                CheckPoint cp = ((Race)Rule).GetNextCheckPoint((RacePlayer)Player);
                if (cp != null)
                {
                    Vector3 dir = cp.AbsolutePosition - playership.AbsolutePosition;
                    dir.Normalize();
                    dir *= 200;
                    Arrow.Position = playership.AbsolutePosition + dir;
                    Arrow.LookAt(cp.AbsolutePosition);
                    Arrow.Visible = true;
                }
                else
                    Arrow.Visible = false;
            }
            else
                Arrow.Visible = false;



            if (Spectate)
            {
            }
            else
            {
                if (playership != null && !playership.Kill && playership.Computer.Target != null)
                {
                    //valid target
                    TargetMarker.Visible = true;
                    TargetMarker.Attach = playership.Computer.Target;
                }
                else
                {
                    //no target
                    TargetMarker.Visible = false;
                }
            }
        }

        GameLog Log = new GameLog();
        public override void OnDraw()
        {
            base.OnDraw();

            Log.Draw(Root.Instance.UserInterface.Renderer);

        }
        public override void Stop()
        {
            base.Stop();
            Root.Instance.ClientDisconnect();
            Root.Instance.Gui.windows.Clear();
            Root.Instance.Scene = new Scene();
            if (MusicChannel != null)
                Root.Instance.UserInterface.Audio.Stop(MusicChannel);
            //Root.Instance.CurrentFlow = new ClientStart();
            //Root.Instance.CurrentFlow.Start();
        }

        Node Arrow;
        Node cursor;
        SpaceShip playership;
        //Vector3 campos;
        Node TargetMarker;
        Camera TargetCamera;
        OverviewCamera MainCamera;
        //FollowCamera MainCamera;
        //AdvancedCamera FlyByCamera;
        float TimeToSpawn;
        float SpawnTime = 5;
        //SpaceWar2006.Windows.Monitor monitor;
        string Host;
        SpaceShipControlBase Control;
        //GameInfoDisplay InfoWindow;
        //Chat ChatWindow;
        bool Spectate = true;
        string Demo = null;
        bool NeedInit = true;
        //ControlDisplay ControlMenu;
    }

    public class ClientStart : Flow
    {
        /*public ClientStart()
            : base("demos\\test.demo")
        {
            Map map = new TestSector();
            map.SetScene(Root.Instance.Scene);
            map.Create();
        }
        */
        public override void Start()
        {
            base.Start();

            Mod.Instance.Init();

            //Root.Instance.ResourceManager.LoadSound("music01.mp3").Play(true);

            

            int i;
            if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-connect"; }))) != -1)
            {
                string host = Root.Instance.Args[i + 1];

                Game g = new Game(host, null, false, false);
                Root.Instance.CurrentFlow = g;
                g.Start();
                return;
            }
            if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-spectate"; }))) != -1)
            {
                string host = Root.Instance.Args[i + 1];

                Game g = new Game(host, null, true, false);
                Root.Instance.CurrentFlow = g;
                g.Start();
                return;
            }
            if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-demo"; }))) != -1)
            {
                string host = Root.Instance.Args[i + 1];

                Game g = new Game(null, host, true, false);
                Root.Instance.CurrentFlow = g;
                g.Start();
                return;
            }
            if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-view"; }))) != -1)
            {
                string mesh = Root.Instance.Args[i + 1];

                Viewer v = new Viewer(mesh);
                Root.Instance.CurrentFlow = v;
                v.Start();
                return;
            }
            if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-edit"; }))) != -1)
            {
                //string mesh = Root.Instance.Args[i + 1];

                SpaceWar2006.Editor.SpaceEditor v = new SpaceWar2006.Editor.SpaceEditor();
                if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-map"; }))) != -1)
                {
                    string map = Root.Instance.Args[i + 1];
                    v.LoadMap(map);
                }
                if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-save"; }))) != -1)
                {
                    string source = Root.Instance.Args[i + 1];
                    v.SaveFileName = source;
                }
                if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-name"; }))) != -1)
                {
                    string name = Root.Instance.Args[i + 1];
                    v.MapName = name;
                }
                Root.Instance.CurrentFlow = v;
                v.Start();
                return;
            }
            if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-local"; }))) != -1)
            {
                Game g = new Game(null, null, false, true);
                Root.Instance.CurrentFlow = g;
                g.Start();
                return;
            }
            if ((i = Array.FindIndex<string>(Root.Instance.Args, new Predicate<string>(delegate(string s) { return s == "-localspectate"; }))) != -1)
            {
                Game g = new Game(null, null, true, true);
                Root.Instance.CurrentFlow = g;
                g.Start();
                return;
            }
            Root.Instance.Scene.camera = new Camera();

            Root.Instance.Gui.windows.Add(new MainMenu());

            //Root.Instance.ResourceManager.LoadSound("z57_v0d.xm").Play(true);
        }

        public override void Stop()
        {
            base.Stop();
            Root.Instance.Gui.windows.Clear();
        }
    }

}
