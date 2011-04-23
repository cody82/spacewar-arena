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

namespace SpaceWar2006.Maps
{
    public partial class Terrain : Map
    {

        public Terrain()
        {
        }
        public override void Create()
        {
            {
                Node t = new Node();


                t.Draw = new ArrayList(new IDrawable[] { Root.Instance.ResourceManager.Load<SupComMap>("terrain/SCMP_015.scmap") });
                t.Position = new Vector3(0, -900, 0);
                Spawn(t, true);
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
