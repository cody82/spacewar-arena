using System;
using System.Collections.Generic;
using System.Text;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Planets;
using SpaceWar2006.Pickups;
using SpaceWar2006.Effects;

using Cheetah;

namespace SpaceWar2006.Maps
{
    public partial class TestSector : Map
    {
        Planet Moon;
        Planet Mars;

        public override void Create()
        {
            Node n, n2;

            Mars = new Mars();
            //Mars.rotationspeed.Y = 15.0f / 180.0f * (float)Math.PI;
            Mars.Position = new Vector3(2000, 0, 3000);
            Spawn(Mars, true);

            Moon = new Moon();
            Moon.rotationspeed.Y = 15.0f / 180.0f * (float)Math.PI;
            Moon.Position = new Vector3(1000, 0, 0);
            Moon.Attach = Mars;
            Spawn(Moon, true);

            n = new Saturn();
            n.rotationspeed.Y = 10.0f / 180.0f * (float)Math.PI;
            n.Orientation = Quaternion.FromAxisAngle(0, 1, 0, (float)Math.PI);
            n.Position = new Vector3(-2000, 0, 3000);
            Spawn(n, true);

            DominationPoint dp = new DominationPoint();
            dp.Name = "DomPoint 1";
            dp.Position = new Vector3(2000, 0, 2000);
            dp.rotationspeed = new Vector3(0, 1, 0);
            Spawn(dp, false);

            CheckPoint cp = new CheckPoint();
            cp.CheckPointIndex = 0;
            cp.Position = new Vector3(3000, 0, 2000);
            Spawn(cp, true);
            cp = new CheckPoint();
            cp.CheckPointIndex = 1;
            cp.Position = new Vector3(4000, 0, 2000);
            Spawn(cp, true);

            VecRandom r = new VecRandom(1000);
            Vector3 center = new Vector3(-4000, 0, -2000);
            for (int i = 0; i < 20; ++i)
            {
                Vector3 rv = r.NextScaledVector3(3000, 300, 2000);
                n = new Phobos();
                Spawn(n, true);
                float s = 30.0f / 180.0f * (float)Math.PI;
                n.rotationspeed = r.NextScaledVector3(s, s, s);
                n.Position = center + rv;

            }

            //if (Root.Instance is Server)
            {
                n = new SpawnPoint(typeof(Repair), 20);
                n.rotationspeed = new Vector3(0, 1, 0);
                n.Position = new Vector3(-500, 0, 0);
                Spawn(n, true);
            }
            {
                n = new SpawnPoint(typeof(MissilePickup), 30);
                n.rotationspeed = new Vector3(0, 1, 0);
                n.Position = new Vector3(2000, 0, -1000);
                Spawn(n, true);
            }

            {
                n = new PlayerStart();
                n.rotationspeed = new Vector3(0, 1, 0);
                n.Position = new Vector3(4000, 0, 4000);
                Spawn(n, true);
            }
            {
                n = new PlayerStart();
                n.rotationspeed = new Vector3(0, 1, 0);
                n.Position = new Vector3(-4000, 0, -4000);
                Spawn(n, true);
            }
            {
                n = new PlayerStart();
                n.rotationspeed = new Vector3(0, 1, 0);
                n.Position = new Vector3(-4000, 0, 4000);
                Spawn(n, true);
            }
            {
                n = new PlayerStart();
                n.rotationspeed = new Vector3(0, 1, 0);
                n.Position = new Vector3(4000, 0, -4000);
                Spawn(n, true);
            }

            {
                n = new Flag(1, new Vector3(4800, 0, -4800));
                Spawn(n, false);
            }
            {
                n = new Flag(0, new Vector3(4400, 0, -4400));
                Spawn(n, false);
            }

            //ParticleNebula pn = new ParticleNebula();
            n = new Nebula();
            //n.Draw.Add(pn);
            //n.Transparent = true;
            n.Position = new Vector3(0, 0, 0);
            Spawn(n, true);

            n = new Nebula();
            //n.Draw.Add(pn);
            //n.Transparent = true;
            n.Position = new Vector3(100, 0, 600);
            Spawn(n, true);

            //n = new LaserTurret();
            //n = new Nebula();
            //n.Draw.Add(pn);
            n = new PornCinema();

            //Text3D tx = new Cheetah.Text3D("abcabcabc", (MeshFont)Root.Instance.ResourceManager.Load("models/font-arial-black", typeof(MeshFont)));
            //n.Draw.Add(tx);
            n.Position = new Vector3(300, 0, 1200);
            n.Orientation = Quaternion.FromAxisAngle(0, 1, 0, (float)Math.PI);
            Spawn(n, true);

            //n = new TestShip();
            //Root.Instance.Scene.Spawn(n);
            //n.Position = new Vector3(400, 0, 0);

            Light l = new Light();
            l.directional = true;
            l.Position = new Vector3(1, 3, 4).GetUnit();
            //l.directional = false;

            l.diffuse = new Color4f(0.5f, 0.8f, 1.0f, 1);
            Spawn(l, true);

            
            l = new Light();
            l.Position = new Vector3(0, 0, 0);
            l.directional = false;
            l.diffuse = new Color4f(1, 0, 0, 1);
            Spawn(l, true);
            

            n = new EclipticNode();
            //n.Draw.Add(new Ecliptic(new Color3f(0, 0, 0.5f), 10000, 100));
            Spawn(n, true);

            //Spawn(new RandomSpawn(typeof(SpaceWar2006.Planets.PhobosAsteroid), 1, 1), true);
        }
    }

}
