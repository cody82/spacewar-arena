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
    public partial class SWDOM01 : Map
    {
        public SWDOM01()
        {
        }

        public SWDOM01(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
        public override void CreateCustom()
        {
            Light l = new Light();
            l.directional = true;
            l.Position = new Vector3(-1, 3, 4).GetUnit();
            Spawn(l, true);
        }
    }
}
