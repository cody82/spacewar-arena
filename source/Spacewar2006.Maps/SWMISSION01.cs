using System;
using System.Collections.Generic;
using System.Text;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Planets;
using SpaceWar2006.Pickups;
using SpaceWar2006.Effects;

using Cheetah;
using Cheetah.Graphics;

namespace SpaceWar2006.Maps
{
    public partial class SWMISSION01 : Map
    {
        public SWMISSION01()
        {

        }

        public override void CreateCustom()
        {
            Light l = new Light();
            l.directional = true;
            l.Position = new Vector3(1, 3, 4).GetUnit();

            l.diffuse = new Color4f(0.0f, 1.0f, 0.0f, 1);
            Spawn(l, true);


            {
                SpaceWar2006.Effects.EclipticNode e = new SpaceWar2006.Effects.EclipticNode();
                e.Position = new Vector3(0f, 0f, 0f);
                e.rotationspeed = new Vector3(0f, 0f, 0f);
                e.Orientation = new Quaternion(0f, 0f, 0f, 1f);
                Spawn(e, true);
            }
            {
                SpaceWar2006.GameObjects.PlayerStart e = new SpaceWar2006.GameObjects.PlayerStart();
                e.Position = new Vector3(410f, 0f, 3970f);
                e.rotationspeed = new Vector3(0f, 0f, 0f);
                e.Orientation = new Quaternion(0f, 0f, 0f, 1f);
                Spawn(e, true);
            }
            {
                SpaceWar2006.Planets.Mars e = new SpaceWar2006.Planets.Mars();
                e.Position = new Vector3(810f, 0f, 1170f);
                e.rotationspeed = new Vector3(0f, 0f, 0f);
                e.Orientation = new Quaternion(0f, 0f, 0f, 1f);
                Spawn(e, true);
            }
            {
                Node e1 = new Waypoint();
                e1.Position = new Vector3(10f, 0f, -1170f);
                Spawn(e1, true);

                Node e2 = new Waypoint();
                e2.Position = new Vector3(10f, 0f, 2170f);
                Spawn(e2, true);

                SpaceWar2006.Ships.BorgCube e = borg = new SpaceWar2006.Ships.BorgCube();
                e.Position = new Vector3(10f, 0f, 1170f);
                e.rotationspeed = new Vector3(0f, 0f, 0f);
                e.Orientation = new Quaternion(0f, 0f, 0f, 1f);
                Spawn(e, true);

                Ai.SpaceShipBotControl ai = new SpaceWar2006.Ai.SpaceShipBotControl(borg);
                ai.ChangeTask(new Ai.Patrol(borg, new Node[] { e2, e1 }));
                Root.Instance.LocalObjects.Add(ai);
            }

            //Mission = new SpaceWar2006.Rules.Mission();
            //Mission.PrimaryObjectives = new SpaceWar2006.Rules.Objective[]{
            //    new SpaceWar2006.Rules.DestroyObjective("Destroy the Borg cube",new Actor[]{borg})
            //};
            //Mission.SecondaryObjectives = new SpaceWar2006.Rules.Objective[] { };
        }

        Ships.BorgCube borg;
        public SWMISSION01(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }
}
