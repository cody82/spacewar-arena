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
    public partial class SWRACE01 : Map
    {
        public SWRACE01()
        {

        }

        public override void CreateCustom()
        {
            Light l = new Light();
            l.directional = true;
            l.Position = Vector3Extensions.GetUnit(new Vector3(-1, 3, 4));
            Spawn(l, true);
        }

        public SWRACE01(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
    }
}
