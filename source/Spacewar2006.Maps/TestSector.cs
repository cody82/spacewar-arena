using System;
using System.Collections.Generic;
using System.Text;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Planets;
using SpaceWar2006.Pickups;
using SpaceWar2006.Effects;

using Cheetah;
using OpenTK;

namespace SpaceWar2006.Maps
{
    public partial class TestSector : Map
    {

        public TestSector()
        {
        }

        public TestSector(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write(Sync);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Sync = context.ReadSingle();
        }

        float Sync;

        public override void Tick(float dTime)
        {
            base.Tick(dTime);

            Sync += dTime;

            Mars.Orientation = QuaternionExtensions.FromAxisAngle(Vector3.UnitY, 15.0f / 180.0f * (float)Math.PI * Sync);
        }

    }

}
