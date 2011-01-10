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

using Cheetah;
using Cheetah.Graphics;

using OpenTK;

namespace SpaceWar2006.Effects
{

    public class Grid : IDrawable
    {
        public bool IsWorldSpace
        {
            get { return false; }
        }

        public Point GetCoordinates(Vector3 pos)
        {
            pos+=new Vector3(Size*(float)Width/2.0f,0,Size*(float)Height/2.0f);
            Point p = new Point(
                Math.Max(Math.Min((int)(pos.X / Size),Width-1),0), 
                Math.Max(Math.Min((int)(pos.Z / Size),Height-1),0)
                );
            return p;
        }

        float Size;
        int Width;
        int Height;
        public Grid(float size, int width, int height)
        {
            Size = size;
            Width = width;
            Height = height;

            if (Root.Instance.UserInterface == null)
                return;

            fieldcount = width * height;
            vertexcount = fieldcount * 4;
            data = new VertexP3C4[vertexcount];
            int i = 0;
            material = Material.CreateSimpleMaterial(null);
            material.DepthTest = true;
            material.DepthWrite = true;
            material.Additive = true;

            //float cy = 1.0f / (float)height * size;
            //float cy = 1.0f / (float)width * size;
            indices = new IndexBuffer();
            indices.buffer = new int[fieldcount * 6];

            for (int y = 0; y < height; ++y)
            {
                float yp = (float)y * size -(size * height / 2);
                for (int x = 0; x < width; ++x)
                {
                    float xp = (float)x * size -(size * width / 2);
                    Color4f color = new Color4f(VecRandom.Instance.NextFloat(), VecRandom.Instance.NextFloat(), VecRandom.Instance.NextFloat(),1);

                    data[i].Color = color;
                    data[i].Position = new Vector3(xp, 0, yp);
                    data[i + 1].Color = color;
                    data[i + 1].Position = new Vector3(xp + size, 0, yp);

                    data[i + 2].Color = color;
                    data[i + 2].Position = new Vector3(xp, 0, yp + size);
                    data[i + 3].Color = color;
                    data[i + 3].Position = new Vector3(xp+size, 0, yp+size);

                    int idx = i / 4 * 6;
                    indices.buffer[idx] = i;
                    indices.buffer[idx+1] = i+1;
                    indices.buffer[idx+2] = i+2;
                    indices.buffer[idx+3] = i+1;
                    indices.buffer[idx+4] = i+3;
                    indices.buffer[idx+5] = i+2;



                    i += 4;
                }
            }
            buffersize=vertexcount * (3+4) * 4;
            vertices = Root.Instance.UserInterface.Renderer.CreateDynamicVertexBuffer(buffersize);
            vertices.Format = VertexFormat.VF_P3C4;
            vertices.Update(data, buffersize);



            shader = Root.Instance.ResourceManager.LoadShader("simple3d.shader");
        }

        public void SetColor(int x, int y, Color4f c)
        {
            int start=(y*Width+x)*4;
            data[start].Color = c;
            data[start+1].Color = c;
            data[start+2].Color = c;
            data[start+3].Color = c;
            changed = true;
        }

        public void Draw(IRenderer r, Node n)
        {
            if (changed)
            {
                vertices.Update(data, buffersize);
                changed = false;
            }
            //Root.Instance.UserInterface.Renderer.SetMaterial(material);
            r.UseShader(shader);
            r.SetMaterial(material);
            r.Draw(vertices, PrimitiveType.TRIANGLES, 0, indices.buffer.Length, indices);

        }

        DynamicVertexBuffer vertices;
        IndexBuffer indices;
        Material material;
        Shader shader;
        int vertexcount;
        int fieldcount;
        int buffersize;
        VertexP3C4[] data;
        bool changed = false;
    }

    public class Ecliptic : IDrawable
    {
        public bool IsWorldSpace
        {
            get { return false; }
        }
        /*public Ecliptic()
            : this(new Color3f(1, 1, 1), 10000, 100)
        {
        }*/
        public Ecliptic(Color3f color, float size, int num)
        {
            if (Root.Instance.UserInterface == null)
                return;

            VertexP3C3[] data = new VertexP3C3[(num + 1) * 4];
            int i = 0;
            material = Material.CreateSimpleMaterial(null);
            material.DepthTest = true;
            material.DepthWrite = true;
            material.Additive = true;
           

            for (int x = 0; x <= num; ++x)
            {
                float c = (float)x / (float)num;
                float a = c * size - size / 2;
                float b = size / 2;
                c /= 6;

                data[i].Color = new Color3f(c, 0, 0.5f);
                data[i++].Position = new Vector3(a, 0, b);
                data[i].Color = new Color3f(c, 0, 0.5f);
                data[i++].Position = new Vector3(a, 0, -b);

                data[i].Color = new Color3f(0, c, 0.5f);
                data[i++].Position = new Vector3(b, 0, a);
                data[i].Color = new Color3f(0, c, 0.5f);
                data[i++].Position = new Vector3(-b, 0, a);
            }
            vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data, (num + 1) * 4 * 2 * 3 * 4);
            vertices.Format = VertexFormat.VF_P3C3;

            shader = Root.Instance.ResourceManager.LoadShader("simple3d.shader");
        }

        public void Draw(IRenderer r, Node n)
        {
            //Root.Instance.UserInterface.Renderer.SetMaterial(material);
            r.UseShader(shader);
            r.SetMaterial(material);
            r.Draw(vertices, PrimitiveType.LINES, 0, vertices.Count, null);
        }

        VertexBuffer vertices;
        Material material;
        Shader shader;
    }

    public class Explosion : Node
    {
        public Explosion(Vector3 pos, Vector3 speed)
        {
            NoReplication = true;
            Position = pos;
            ParticleSpeed = speed;
            Init();
            SyncRefs = false;
            RenderRadius = 500;
        }

        protected virtual ParticleExplosion CreateParticleSystem()
        {
            //Sound = Root.Instance.ResourceManager.LoadSound("booma2.wav");
            return new ParticleExplosion(AbsolutePosition, ParticleSpeed);
        }

        protected virtual void Init()
        {
            Draw = new ArrayList();
            Fire = CreateParticleSystem();
            //ShockWave wave = new ShockWave();
            //CreateRandomParticles(100);
            Draw.Add(Fire);
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("fullscreenquad/fullscreenquad.mesh"));
            //Draw.Add(x);
            Transparent = 2;

        }

        public Explosion(DeSerializationContext context)
        {
            NoReplication = true;
            SyncRefs = false;
            DeSerialize(context);
            Init();
        }

        public override void Tick(float dTime)
        {
            base.Tick(dTime);

            if (age > LifeTime)
                Kill = true;

            if (!SoundPlayed && Sound != null)
            {
                SoundPlayed = true;
                PlaySound(Sound, false);
            }
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
        }
        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
        }

        protected ParticleSystem Fire;
        protected float LifeTime = 5;
        public Sound Sound;
        bool SoundPlayed = false;
        protected Vector3 ParticleSpeed;
    }

    public class NukeExplosion : Explosion
    {
        public NukeExplosion(Vector3 pos, Vector3 speed)
            : base(pos, speed)
        {
            Transparent = 5;
        }

        public override bool CanCollide(Node other)
        {
            return other is SpaceWar2006.GameObjects.Actor;
        }
        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, wave.CurrentSize / 2);
        }

        public override void OnCollide(Node other)
        {
            base.OnCollide(other);
            ((SpaceWar2006.GameObjects.Actor)other).Damage(new SpaceWar2006.GameObjects.Damage(1000, 1000, 1000, 0));
        }
        public override bool DrawLocal(IDrawable d)
        {
            return false;
        }
        protected override ParticleExplosion CreateParticleSystem()
        {
            Sound = Root.Instance.ResourceManager.LoadSound("booma2.wav");
            return new ParticleExplosion(AbsolutePosition, ParticleSpeed * 0.25f);
        }
        protected override void Init()
        {
            base.Init();
            Draw.Add(wave=new ShockWave(Root.Instance.ResourceManager.LoadTexture("shock1.dds"), 3000, 4, false));
        }

        ShockWave wave;
    }

    public class BigExplosion : Explosion
    {
        protected override ParticleExplosion CreateParticleSystem()
        {
            Sound = Root.Instance.ResourceManager.LoadSound("booma2.wav");
            return new ParticleExplosion(AbsolutePosition, ParticleSpeed * 0.25f);
        }

        public BigExplosion(Vector3 pos, Vector3 speed)
            : base(pos, speed)
        {
            Transparent = 5;
        }

        public override bool DrawLocal(IDrawable d)
        {
            return false;
        }
        public BigExplosion(DeSerializationContext context)
            : base(context)
        {
            Transparent = 5;
        }

        public override void Tick(float dTime)
        {
            base.Tick(dTime);

            l.diffuse = GetLightColor();

            if (age > 3 && quad != null)
            {
                Draw.Remove(quad);
                quad = null;
            }
        }

        Color4f GetLightColor()
        {
            float f = (3 - age) / 3;
            return new Color4f(1.0f*f, 1.0f*f, 0.1f*f, 1);
        }

        Light l;
        public override void OnAdd(Scene s)
        {
            base.OnAdd(s);

            l = new Light();
            l.diffuse = GetLightColor();// new Color4f(1, 1, 0.1f, 1);
            l.directional = false;
            l.Position = AbsolutePosition;
            l.NoReplication = true;
            l.Range = 2500;

            s.Spawn(l);
        }

        public override void OnRemove(Scene s)
        {
            base.OnRemove(s);

            if (l != null)
            {
                l.Kill = true;
                l = null;
            }
        }
        public override void OnKill()
        {
            base.OnKill();

            if (l != null)
            {
                l.Kill = true;
                l = null;
            }
        }

        protected override void Init()
        {
            base.Init();
            Draw.Add(new ShockWave());
            //Draw.Add(quad = Root.Instance.ResourceManager.LoadMesh("fullscreenquad/fullscreenquad.mesh"));
            //Root.Instance.Scene.camera.Shake = 0.02f;
        }

        IDrawable quad;
    }

    public class SmallExplosion : Explosion
    {
        protected override ParticleExplosion CreateParticleSystem()
        {
            Sound = Root.Instance.ResourceManager.LoadSound("nes2.wav");
            return new ParticleExplosion(AbsolutePosition, 30, Root.Instance.ResourceManager.LoadTexture("explo.dds"), 1, 1.5f, 20, 5, 5, 0.25f, 0.5f, 30, Vector3.Zero);
        }

        public SmallExplosion(Vector3 pos)
            : base(pos, Vector3.Zero)
        {
            LifeTime = 2;
            RenderRadius = 0;
        }

        public SmallExplosion(DeSerializationContext context)
            : base(context)
        {
            LifeTime = 2;
            RenderRadius = 0;
        }
    }

    public class Crap : Node
    {
        public Crap()
        {
            NoReplication = true;
            SyncRefs = false;
            RenderRadius = 30;
            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("crap1/crap1.mesh"));
            maxage = 3 + VecRandom.Instance.NextFloat() * 2;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            if (age > maxage)
            {
                Kill = true;
            }
        }
        public Crap(DeSerializationContext context)
            : this()
        {
            base.DeSerialize(context);
        }

        float maxage;
    }

    [Editable]
    public class EclipticNode : Node
    {
        public EclipticNode()
        {
            Draw.Add(new Ecliptic(new Color3f(1, 1, 1), Size=10000, 100));
            HalfSize = Size * 0.5f;
            Transparent = 1;
        }

        public override bool CanCollide(Node other)
        {
            Vector3 p = other.AbsolutePosition;
            if (Math.Abs(p.X) < HalfSize && Math.Abs(p.Z) < HalfSize)
                return false;

            return other is GameObjects.Actor;
        }

        public override CollisionInfo GetCollisionInfo()
        {
            return new AlwaysCollisionInfo();
        }
        public override void OnCollide(Node other)
        {
            //Vector3 p = other.AbsolutePosition;
            //if (Math.Abs(p.X) < HalfSize && Math.Abs(p.Z) < HalfSize)
            //    return;

            base.OnCollide(other);

            ((GameObjects.Actor)other).Damage(new SpaceWar2006.GameObjects.Damage(DamageSpeed * Root.Instance.TickDelta, DamageSpeed * Root.Instance.TickDelta, 0, 0));
        }

        float DamageSpeed=50;
        float Size;
        float HalfSize;
    }



    [Editable]
    public class GridNode : Node
    {
        public GridNode()
        {
            Draw.Add(Grid=new Grid(1000,10,10));
            Transparent = 1;
        }

        public override bool CanCollide(Node other)
        {
            return false;
        }
        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            Ray r=Utility.GetMouseVector();

            Vector3 pos = new Plane(0, 1, 0, 0).GetIntersection(r.Start, r.End);

            Point coord=Grid.GetCoordinates(pos);
            Grid.SetColor(coord.X, coord.Y, new Color4f(VecRandom.Instance.NextFloat(), VecRandom.Instance.NextFloat(), VecRandom.Instance.NextFloat(), 1));
        }
        public Grid Grid;
    }

    public class ParticleExplosion : ParticleSystem
    {
        class Emitter
        {
            public Emitter(Vector3 p, Vector3 s, float accel, float agebase, float agevar)
                : this(accel, agebase, agevar)
            {
                Position = p;
                Speed = s;
            }
            public Emitter(float accel, float agebase, float agevar)
            {
                Acceleration = VecRandom.Instance.NextUnitVector3() * accel;
                MaxAge = agebase + VecRandom.Instance.NextFloat() * agevar;
            }

            public virtual void Tick(float dtime)
            {
                Position += Speed * dtime;
                Speed += Acceleration * dtime;
                Age += dtime;
            }
            public Vector3 Position;
            public Vector3 Speed;
            public Vector3 Acceleration;
            static Random r = new Random();
            public float Age = 0;
            public float MaxAge;
        }

        protected void CreateExplosion(Vector3 pos)
        {
            for (int i = 0; i < EmitterCount; ++i)
            {
                Emitter p = new Emitter(MaxEmitterAccel, MaxEmitterAgeBase, MaxEmitterAgeVariance);
                p.Position = pos;
                p.Speed = BaseSpeed + VecRandom.Instance.NextUnitVector3() * MaxEmitterSpeed;
                emitters.Add(p);
            }

            Material.Additive = true;
            Material.DepthWrite = false;

        }
        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            Age += dtime;
            Timer += dtime;
            bool spawn = false;
            if (Timer > 0.1f)
            {
                Timer = 0;
                spawn = true;
            }
            List<Emitter> kill = new List<Emitter>();
            foreach (Emitter e in emitters)
            {
                e.Tick(dtime);
                //e.Speed -= e.Speed * dtime / 4;
                if (spawn)
                    Spawn(new Particle(e.Position, VecRandom.Instance.NextUnitVector3() * 20));

                if (e.Age > e.MaxAge)
                    kill.Add(e);
            }
            foreach (Emitter e in kill)
                emitters.Remove(e);

            if (Age > FadeStartTime)
            {
                float a = (Age > FadeEndTime) ? 0 : (1 - (Age - FadeStartTime) / (FadeEndTime - FadeStartTime));
                foreach (Particle p in Particles)
                {
                    if (p != null)
                        p.Color = new Color4f(a, a, a, 1);
                }
            }
        }

        public ParticleExplosion(Vector3 pos, Vector3 speed)
            : base(100,
                Root.Instance.ResourceManager.LoadTexture("explo.dds"))
        {
            BaseSpeed = speed;
            CreateExplosion(pos);
        }

        public ParticleExplosion(Vector3 pos, int maxparticles, Texture t,
            float fadestarttime, float fadeendtime, float maxemitspeed, float maxemitaccel,
            int emittercount, float maxemitteragebase, float maxemitteragevar, float pointsize, Vector3 basespeed)
            : base(maxparticles, t)
        {
            FadeStartTime = fadestarttime;
            FadeEndTime = fadeendtime;
            MaxEmitterSpeed = maxemitspeed;
            MaxEmitterAccel = maxemitaccel;
            EmitterCount = emittercount;
            MaxEmitterAgeBase = maxemitteragebase;
            MaxEmitterAgeVariance = maxemitteragevar;
            PointSize = pointsize;
            BaseSpeed = basespeed;

            CreateExplosion(pos);
        }
        float Timer = 0;
        static Random r = new Random();
        List<Emitter> emitters = new List<Emitter>();
        float Age = 0;
        float FadeStartTime = 3;
        float FadeEndTime = 5;
        float MaxEmitterSpeed = 120;
        float MaxEmitterAccel = 5;
        int EmitterCount = 25;
        float MaxEmitterAgeBase = 0.5f;
        float MaxEmitterAgeVariance = 1;
        Vector3 BaseSpeed = Vector3.Zero;
    }


    public class SmokeTrail : Node
    {
        public SmokeTrail()
        {
            Smoke = new ParticleSystem(100,
                Root.Instance.ResourceManager.LoadTexture("explo.dds"));
            Smoke.PointSize = 100;
            Smoke.Material.Additive = true;
            Smoke.Material.DepthWrite = false;

            Draw.Add(Smoke);
            Transparent = 2;
            SyncRefs = false;
        }

        public SmokeTrail(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void Tick(float dTime)
        {
            base.Tick(dTime);

            Timer += dTime;

            if ((Attach != null || !KillWhenDetached) && Timer > 0.05f)
            {
                for (int i = 0; i < 10; ++i)
                {
                    Particle p = Smoke.NewParticle();
                    p.Position = SmoothAbsolutePosition + VecRandom.Instance.NextUnitVector3() * Smoke.PointSize / 4;
                    p.Speed = VecRandom.Instance.NextUnitVector3() * Smoke.PointSize / 4;
                    Smoke.Spawn(p);
                }
                Timer = 0;
            }

            if (Attach == null && KillWhenDetached)
            {
                TimeDetached += dTime;
                if (TimeDetached > 5)
                    Kill = true;
            }


            foreach (Particle p in Smoke.Particles)
                if (p != null)
                {
                    if (p.Age > 0.3f)
                    {
                        float a = (p.Age > 0.6f) ? 0 : (1 - (p.Age - 0.3f) * (1.0f / 0.3f));
                        p.Color = new Color4f(a, a, a, 1);
                    }
                }
        }

        public ParticleSystem Smoke;
        float Timer = 0;
        bool KillWhenDetached = true;
        float TimeDetached = 0;
    }


    public class ShockWave : IDrawable, ITickable
    {
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public ShockWave()
            : this(Root.Instance.ResourceManager.LoadTexture("shock1.dds"), 1000, 4)
        {
        }

        public ShockWave(Texture t, float size, float lifetime)
            :this(t,size,lifetime,true)
        {
        }
        public ShockWave(Texture t, float size, float lifetime, bool random)
        {
            Size = size;
            LifeTime = lifetime;
            f = size / (float)Math.Sqrt(lifetime);
            Mat = new Material();
            Mat.Additive = true;
            Mat.diffusemap = t;
            Mat.twosided = true;
            Mat.NoLighting = true;
            Mat.DepthWrite = false;
            Shader = Root.Instance.ResourceManager.LoadShader("shock.shader");
            CreateVertexBuffer();
            BaseMatrix = random?Matrix4Extensions.FromAngleAxis(VecRandom.Instance.NextUnitVector3(), 30.0f / 180.0f * (float)Math.PI):Matrix4.Identity;

        }
        void CreateVertexBuffer()
        {
            if (Root.Instance.UserInterface==null)
                return;

            VertexP3C4T2[] data = new VertexP3C4T2[]{
                new VertexP3C4T2(1,0,-1, 1,1,1,1, 1,0),
                 new VertexP3C4T2(-1,0,-1, 1,1,1,1, 0,0),
               new VertexP3C4T2(-1,0,1, 1,1,1,1, 0,1),
                new VertexP3C4T2(1,0,1, 1,1,1,1, 1,1)
            };
            Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data, (3 + 4 + 2) * 4 * 4);
            Vertices.Format = VertexFormat.VF_P3C4T2;
        }

        #region IDrawable Members

        public void Draw(IRenderer r, Node n)
        {
            if (Age > LifeTime)
                return;

            r.SetMode(RenderMode.Draw3D);
            r.SetMaterial(Mat);
            r.BindTexture(Mat.diffusemap.Id);
            r.UseShader(Shader);
            r.SetUniform(Shader.GetUniformLocation("Alpha"), new float[] { CurrentAlpha });

            float s = CurrentSize / 2;
            Matrix4 m = BaseMatrix * Matrix4Extensions.FromScale(s, s, s);
            r.PushMatrix();
            r.MultMatrix(m);
            r.Draw(Vertices, PrimitiveType.QUADS, 0, 4, null);
            r.PopMatrix();
        }

        #endregion

        //float StartSize;
        float Size;
        Material Mat;
        float Age = 0;
        float LifeTime;
        VertexBuffer Vertices;
        float f;//f*sqrt(lifetime)=s!
        Shader Shader;
        Matrix4 BaseMatrix;

        public float CurrentSize
        {
            get
            {
                //return Age/LifeTime * Size;
                return f * (float)Math.Sqrt(Age);
            }
        }
        float CurrentAlpha
        {
            get
            {
                return 1 - Age / LifeTime;
            }
        }

        #region ITickable Members

        public void Tick(float dtime)
        {
            Age += dtime;

        }

        #endregion
    }

    public class ParticleNebula : ParticleSystem
    {
        public override bool IsWorldSpace
        {
            get { return false; }
        }
        public ParticleNebula()
            : this(200, Root.Instance.ResourceManager.LoadTexture("smoke4.dds"), 500, 200)
        {
        }

        public ParticleNebula(int max, Texture t, float radius, int pointsize)
            : base(max, t)
        {
            CreateNebula(radius, pointsize);
        }
        void CreateNebula(float radius, int pointsize)
        {
            for (int i = 0; i < Max; ++i)
            {
                Vector3 pos = Vector3.Zero;
                Spawn(new Particle(pos + VecRandom.Instance.NextUnitScaledVector3(radius, radius / 2, radius) * VecRandom.Instance.NextFloat(), Vector3.Zero));
            }
            Material.Additive = false;
            Material.DepthWrite = false;
            PointSize = pointsize;
            Dynamic = false;
            FillBuffer();
        }
    }

    public class BeamRenderer : IDrawable
    {
        public BeamRenderer(int count, IDrawable part, Vector3 dist)
        {
            Count = count;
            Part = part;
            Distance = dist;
            Offset = Matrix4Extensions.FromTranslation(Distance);
        }

        int Count;
        IDrawable Part;
        Vector3 Distance;
        Matrix4 Offset;

        #region IDrawable Members

        public void Draw(IRenderer r, Node n)
        {
            r.PushMatrix();
            for (int i = 0; i < Count; ++i)
            {
                Part.Draw(r, n);
                r.MultMatrix(Offset);
            }
            r.PopMatrix();
        }

        public bool IsWorldSpace
        {
            get { return false; }
        }

        #endregion
    }

    public class Cursor : IDrawable
    {
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public Cursor(Color3f color, float size)
        {
            VertexP3C3[] data = new VertexP3C3[4];
            int i = 0;
            size /= 2;
            material = Material.CreateSimpleMaterial(null);

            data[i].Color = color;
            data[i++].Position = new Vector3(size, 0, size);
            data[i].Color = color;
            data[i++].Position = new Vector3(-size, 0, -size);

            data[i].Color = color;
            data[i++].Position = new Vector3(size, 0, -size);
            data[i].Color = color;
            data[i++].Position = new Vector3(-size, 0, size);

            vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data, 4 * 2 * 3 * 4);
            vertices.Format = VertexFormat.VF_P3C3;

            shader = Root.Instance.ResourceManager.LoadShader("simple3d.shader");
        }

        public void Draw(IRenderer r, Node n)
        {

            //Root.Instance.UserInterface.Renderer.SetMaterial(material);
            r.UseShader(shader);
            Root.Instance.UserInterface.Renderer.Draw(vertices, PrimitiveType.LINES, 0, vertices.Count, null);
        }

        VertexBuffer vertices;
        Material material;
        Shader shader;
    }


}