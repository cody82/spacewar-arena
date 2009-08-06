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

namespace SpaceWar2006.Editor
{


    public class SpaceEditor : Cheetah.Editor
    {
        public override bool IsSaveable(Entity e)
        {
            if(!base.IsSaveable(e))
                return false;
            if (e is SpaceWar2006.Weapons.Projectile||e is SpaceWar2006.Effects.Explosion)
                return false;

            return true;
        }

        public SpaceEditor()
        {
            //LoadMap("SpaceWar2006.Maps.TestSector");
        }

        override protected void CsSave(Entity e, StreamWriter w)
        {
            base.CsSave(e, w);

            if (e is CheckPoint)
            {
                CheckPoint n = (CheckPoint)e;
                w.WriteLine("e.CheckPointIndex=" + n.CheckPointIndex + ";");
            }
            if (e is Flag)
            {
                Flag n = (Flag)e;
                w.WriteLine("e.Team=" + n.Team + ";");
                w.WriteLine("e.FlagPosition=new Vector3(" + n.FlagPosition.X+"f,"+ n.FlagPosition.Y+"f,"+n.FlagPosition.Z+"f);");
            }
            if (e is SpawnPoint)
            {
                SpawnPoint n = (SpawnPoint)e;
                w.WriteLine("e.SpawnType=typeof(" + n.SpawnType.FullName + ");");
                w.WriteLine("e.SpawnInterval=" + n.SpawnInterval+"f;");
            }
            if (e is Waypoint)
            {
                Waypoint n = (Waypoint)e;
                if(n.Name!=null)
                    w.WriteLine("e.Name=\"" + n.Name + "\";");
            }
        }


        public override void LoadMap(string name)
        {
            Map m = (Map)Root.Instance.Factory.CreateInstance(name);
            m.Scene = Root.Instance.Scene;
            m.Create();
        }
    }
}
