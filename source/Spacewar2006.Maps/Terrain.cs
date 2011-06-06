using System;
using System.Collections.Generic;
using System.Text;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Planets;
using SpaceWar2006.Pickups;
using SpaceWar2006.Effects;

using Cheetah;
using OpenTK;
using System.Collections;
using Cheetah.Graphics;
using Cheetah.Physics;

namespace SpaceWar2006.Maps
{

    public class TerrainNode : PhysicsNode
    {
        public TerrainNode(DeSerializationContext context)
            : base(context)
        {
        }

        SupComMap map;

        public TerrainNode()
        {
            NoReplication = true;

            map = Root.Instance.ResourceManager.Load<SupComMap>("terrain/SCMP_015.scmap");
            //map.Wireframe = true;
            Draw = new ArrayList(new IDrawable[] { map });
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
                a.Damage(new Damage(1000, 1000, 1000, 0));
            }
        }

        protected override IPhysicsObject CreatePhysicsObject(Scene s)
        {
            //new SupComMapLoader.Heightmap(map),33,10000.0f / 16.0f,0.15f
            IHeightMap hm = new SupComMapLoader.Heightmap(map.MapFile);
            //100,0.03f
            IPhysicsObject obj = s.Physics.CreateHeightmap(hm, 10000.0f / 512.0f, 0, 0, 0.15f);
            obj.Position = base.Position;
            obj.Speed = base.Speed;
            obj.Orientation = base.Orientation;
            obj.Owner = this;
            //obj.Movable = false;
            return obj;
        }
    }

    public partial class Terrain : Map
    {

        public Terrain()
        {
        }
        public override void Create()
        {
            {
                TerrainNode t = new TerrainNode();


                //t.Draw = new ArrayList(new IDrawable[] { Root.Instance.ResourceManager.Load<SupComMap>("terrain/SCMP_015.scmap") });
                t.Position = new Vector3(0, -900, 0);
                Spawn(t, true);
                t.Position = new Vector3(0, -900, 0);
            }

            {
                PlayerStart n = new PlayerStart();
                n.rotationspeed = new Vector3(0, 1, 0);
                n.Position = new Vector3(0, 0, 500);
                Spawn(n, true);
            }

            {

                Light l = new Light();
                l.directional = true;
                l.Position = Vector3Extensions.GetUnit(new Vector3(1, 3, 4));
				Spawn(l,true);
            }

            {
                EclipticNode n = new EclipticNode();
                //n.Draw.Add(new Ecliptic(new Color3f(0, 0, 0.5f), 10000, 100));
                Spawn(n, true);
            }
        }

        public Terrain(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
        }


    }

}
