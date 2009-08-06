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
public partial class SWMISSION02 : Map{
public override void Create(){
{
SpaceWar2006.Effects.EclipticNode e=new SpaceWar2006.Effects.EclipticNode();
e.Position=new Vector3(0f,0f,0f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(-3850f,0f,-4050f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(1950f,0f,1100f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(1340f,0f,1590f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(870f,0f,2180f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(1600f,0f,2350f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(2140f,0f,1850f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(2860f,0f,1440f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(1100f,0f,3080f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2980f,0f,1960f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2770f,0f,2590f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2380f,0f,-230f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(3360f,0f,180f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-110f,0f,3650f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(1780f,0f,1200f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(2750f,0f,-450f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-480f,0f,3070f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(4350f,0f,-240f);
e.rotationspeed=new Vector3(0f,-4.223401E-07f,0f);
e.Orientation=new Quaternion(0f,0.5520186f,0f,0.8338345f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(-180f,0f,2460f);
e.rotationspeed=new Vector3(0f,1.753648E-09f,0f);
e.Orientation=new Quaternion(0f,0.2265501f,0f,0.9740002f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Waypoint e=new SpaceWar2006.GameObjects.Waypoint();
e.Position=new Vector3(20f,0f,40f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.Name="waypoint04";
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(1920f,0f,330f);
e.rotationspeed=new Vector3(0f,-1.842323E-07f,0f);
e.Orientation=new Quaternion(0f,-0.9414684f,0f,0.3371005f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Waypoint e=new SpaceWar2006.GameObjects.Waypoint();
e.Position=new Vector3(4310f,0f,1060f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.Name="waypoint02";
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Waypoint e=new SpaceWar2006.GameObjects.Waypoint();
e.Position=new Vector3(-370f,0f,5230f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.Name="waypoint06";
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Waypoint e=new SpaceWar2006.GameObjects.Waypoint();
e.Position=new Vector3(3880f,0f,3960f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.Name="waypoint01";
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.LaserTurret e=new SpaceWar2006.GameObjects.LaserTurret();
e.Position=new Vector3(-320f,0f,4440f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,-0.9915734f,0f,0.1295419f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Waypoint e=new SpaceWar2006.GameObjects.Waypoint();
e.Position=new Vector3(3410f,0f,-1620f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.Name="waypoint03";
Spawn(e,true);
}
{
SpaceWar2006.Planets.Mars e=new SpaceWar2006.Planets.Mars();
e.Position=new Vector3(-1420f,0f,-2890f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Waypoint e=new SpaceWar2006.GameObjects.Waypoint();
e.Position=new Vector3(-2040f,0f,3040f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
e.Name="waypoint05";
Spawn(e,true);
}
}}}
