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
public partial class SWCTF01 : Map{
public override void Create(){
{
SpaceWar2006.Effects.EclipticNode e=new SpaceWar2006.Effects.EclipticNode();
e.Position=new Vector3(0f,0f,0f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Flag e=new SpaceWar2006.GameObjects.Flag();
e.Position=new Vector3(-4180f,0f,120f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.Team=0;
e.FlagPosition=new Vector3(0f,0f,0f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Flag e=new SpaceWar2006.GameObjects.Flag();
e.Position=new Vector3(4120f,0f,230f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.Team=1;
e.FlagPosition=new Vector3(0f,0f,0f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(410f,0f,3970f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(570f,0f,-4020f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(0f,0f,0f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Mars e=new SpaceWar2006.Planets.Mars();
e.Position=new Vector3(810f,0f,1170f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-920f,0f,350f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1080f,0f,-70f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-860f,0f,800f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1170f,0f,-980f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-380f,0f,440f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-850f,0f,-1550f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-520f,0f,-800f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(1350f,0f,-590f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(20f,0f,-1380f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(160f,0f,-2340f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Saturn e=new SpaceWar2006.Planets.Saturn();
e.Position=new Vector3(-2560f,0f,1090f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(1340f,0f,-160f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(950f,0f,-770f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(340f,0f,-1150f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(-4230f,0f,-460f);
e.rotationspeed=new Vector3(0f,1.121039E-44f,0f);
e.Orientation=new Quaternion(0f,1.401298E-45f,0f,-1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(-4230f,0f,640f);
e.rotationspeed=new Vector3(0f,1.121039E-44f,0f);
e.Orientation=new Quaternion(0f,1f,0f,1.401298E-45f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(4270f,0f,-560f);
e.rotationspeed=new Vector3(0f,3.724964E-09f,0f);
e.Orientation=new Quaternion(0f,0.006622077f,0f,0.9999786f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(4250f,0f,950f);
e.rotationspeed=new Vector3(0f,5.673786E-10f,0f);
e.Orientation=new Quaternion(0f,0.9999781f,0f,-0.006622082f);
Spawn(e,true);
}
}}}
