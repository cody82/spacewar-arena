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
using Cheetah;

namespace SpaceWar2006.Planets
{
    [Editable]
    public class Phobos : Planet
    {
        public Phobos()
            : base(100, Root.Instance.ResourceManager.LoadMesh("phobos/phobos.mesh"))
        {
        }

        public Phobos(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }

    [Editable]
    public class PhobosAsteroid : Asteroid
    {
        public PhobosAsteroid()
            : base(100, Root.Instance.ResourceManager.LoadMesh("phobos/phobos.mesh"))
        {
        }

        public PhobosAsteroid(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void OnKill()
        {
            SpawnExplosion();
            base.OnKill();

        }

        private void SpawnExplosion()
        {
            SpaceWar2006.Effects.Explosion x = new SpaceWar2006.Effects.BigExplosion(AbsolutePosition, Vector3.Zero);
            Root.Instance.Scene.Spawn(x);
        }
    }

    [Editable]
    public class Mars : Planet
    {
        public Mars()
            : base(500, Root.Instance.ResourceManager.LoadMesh("mars/mars.mesh"))
        {
        }

        public Mars(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }

    [Editable]
    public class Moon : Planet
    {
        public Moon()
            : base(200, Root.Instance.ResourceManager.LoadMesh("moon/moon.mesh"))
        {
        }

        public Moon(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }

    [Editable]
    public class Saturn : Planet
    {
        public Saturn()
            : base(500, Root.Instance.ResourceManager.LoadMesh("saturn/saturn.mesh"))
        {
            //Radius = RenderRadius = Root.Instance.ResourceManager.LoadMesh("saturn/saturn.mesh").BBox.Radius;
        }

        public Saturn(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }


    public class Asteroid : Actor
    {
        public Asteroid(float radius, IDrawable model)
        {
            Draw = new ArrayList();

            Draw.Add(model);
            Radius = radius;
            RenderRadius = Radius;

            Hull = new Hull(1000);
        }

        public override string Name
        {
            get
            {
                return "Asteroid";
            }
        }

        public Asteroid(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
            Radius = context.ReadSingle();
        }
        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            context.Write(Radius);
        }
        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, Radius);
        }
        public override bool CanCollide(Node other)
        {
            return true;
        }

        public override void OnCollide(Node other)
        {
            base.OnCollide(other);

            if (!Root.Instance.IsAuthoritive)
                return;

            Actor a = other as Actor;
            if (a != null)
            {
                a.Damage(new Damage(1000,1000,1000,0));
            }
        }

        public override void Damage(Damage d)
        {
            base.Damage(d);

            Hull.Damage(d.Normal);
            if (Hull.CurrentHitpoints <= 0.0f)
            {
                Kill = true;
            }
        }
        public float Radius;
    }

    public class Planet : Actor
    {
        public Planet(float radius, IDrawable model)
        {
            Draw = new ArrayList();

            Draw.Add(model);
            Radius = radius;
            RenderRadius = Radius;
        }

        public override string Name
        {
            get
            {
                return "Planet";
            }
        }

        public Planet(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
            Radius = context.ReadSingle();
        }
        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            context.Write(Radius);
        }
        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, Radius);
        }
        public override bool CanCollide(Node other)
        {
            if (other is Planet)
                return false;
            else
                return true;
        }
        public float Radius;

        #region IActor Members

        #endregion
    }

    public class SpaceStation : Planet
    {
        public SpaceStation(float radius, IDrawable model)
            : base(radius, model)
        {
        }

        public SpaceStation(DeSerializationContext context)
            : base(context)
        {
        }

        public override string Name
        {
            get
            {
                return "Station";
            }
        }
    }

    [Editable]
    public class PornCinema : Planet
    {
        public PornCinema()
            : base(200, Root.Instance.ResourceManager.LoadMesh("cinema/cinema.mesh"))
        {
        }

        public PornCinema(DeSerializationContext context)
            : base(context)
        {
        }

        public override string Name
        {
            get
            {
                return "Porn Cinema";
            }
        }
    }
}
