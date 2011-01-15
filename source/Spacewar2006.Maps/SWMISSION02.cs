using System;
using System.Collections.Generic;
using System.Text;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Planets;
using SpaceWar2006.Pickups;
using SpaceWar2006.Effects;

using Cheetah;
using Cheetah.Graphics;
using OpenTK;

namespace SpaceWar2006.Maps
{
    public partial class SWMISSION02 : Map
    {
        public SWMISSION02()
        {

        }

        public override void CreateCustom()
        {
            IList<Waypoint> wp = Root.Instance.Scene.FindEntitiesByType<Waypoint>();
            Waypoint[] waypoints = new Waypoint[wp.Count];
            for (int i = 0; i < wp.Count; ++i)
                waypoints[i] = wp[i];

            Array.Sort<Waypoint>(waypoints, new Comparison<Waypoint>(delegate(Waypoint wp1, Waypoint wp2)
            {
                return string.Compare(wp1.Name, wp2.Name);
            }));
            
            SpaceWar2006.Ships.BorgCube e = new SpaceWar2006.Ships.BorgCube();
            e.Position = waypoints[0].Position;
            Spawn(e, false);

            if (Root.Instance.IsAuthoritive)
            {
                Ai.SpaceShipBotControl ai = new SpaceWar2006.Ai.SpaceShipBotControl(e);
                ai.ChangeTask(new Ai.Patrol(e, waypoints));
                Root.Instance.LocalObjects.Add(ai);

                Mission = new SpaceWar2006.Rules.Mission();
                Mission.Missions = new SpaceWar2006.Rules.SingleMission[2];
                Mission.Missions[0]=new SpaceWar2006.Rules.SingleMission(
                    new SpaceWar2006.Rules.Objective[]{
                new SpaceWar2006.Rules.DestroyObjective("Destroy the Borg cube",new Actor[]{e})
            });
                Mission.Missions[1] = new SpaceWar2006.Rules.SingleMission(
                    new SpaceWar2006.Rules.Objective[]{
                new SpaceWar2006.Rules.EscortObjective("Escort the Borg cube",new Actor[]{e})
            });
                Mission.TimeLimit = 60;
            }
            //Mission.SecondaryObjectives = new SpaceWar2006.Rules.Objective[] { };

            Light l = new Light();
            l.directional = true;
            l.Position = new Vector3(-1, 3, 4).GetUnit();
            Spawn(l, true);

        }

        public SWMISSION02(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }
}
