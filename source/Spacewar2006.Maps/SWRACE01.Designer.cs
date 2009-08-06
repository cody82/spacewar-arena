using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections.Generic;
using System.Threading;

using Cheetah;
using SpaceWar2006.GameObjects;
namespace SpaceWar2006.Maps{
public partial class SWRACE01 : Map{
public override void Create(){
{
SpaceWar2006.Effects.EclipticNode e=new SpaceWar2006.Effects.EclipticNode();
e.Position=new Vector3(0f,0f,0f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.CheckPoint e=new SpaceWar2006.GameObjects.CheckPoint();
e.Position=new Vector3(10f,0f,4330f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.CheckPointIndex=0;
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.CheckPoint e=new SpaceWar2006.GameObjects.CheckPoint();
e.Position=new Vector3(-4250f,0f,-130f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.CheckPointIndex=1;
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.CheckPoint e=new SpaceWar2006.GameObjects.CheckPoint();
e.Position=new Vector3(170f,0f,-4110f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.CheckPointIndex=2;
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.CheckPoint e=new SpaceWar2006.GameObjects.CheckPoint();
e.Position=new Vector3(3550f,0f,-70f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.CheckPointIndex=3;
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(50f,0f,3750f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(60f,0f,4910f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(590f,0f,3750f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(650f,0f,4900f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(-3540f,0f,370f);
e.rotationspeed=new Vector3(0f,-2.957514E-07f,0f);
e.Orientation=new Quaternion(0f,0.4357549f,0f,0.9000649f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(-3970f,0f,710f);
e.rotationspeed=new Vector3(0f,-3.348949E-07f,0f);
e.Orientation=new Quaternion(0f,0.9000659f,0f,-0.4357557f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2490f,0f,-1160f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2560f,0f,670f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3130f,0f,1060f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Saturn e=new SpaceWar2006.Planets.Saturn();
e.Position=new Vector3(-1810f,0f,-1950f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-250f,0f,-3410f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Mars e=new SpaceWar2006.Planets.Mars();
e.Position=new Vector3(1290f,0f,-3760f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2780f,0f,-1110f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3270f,0f,-880f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2790f,0f,1760f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-1650f,0f,-3280f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-570f,0f,-4070f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-1040f,0f,-3500f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3690f,0f,1430f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2810f,0f,-390f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2920f,0f,-850f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3490f,0f,-1480f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(4310f,0f,-1060f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3000f,0f,-1400f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2480f,0f,1790f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(470f,0f,-2860f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3480f,0f,1950f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(4090f,0f,2820f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3990f,0f,-450f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3320f,0f,1390f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2370f,0f,-2020f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2500f,0f,-1600f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2750f,0f,940f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
}}}
