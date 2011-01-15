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

using OpenTK;


namespace SpaceWar2006.Maps{
public partial class SWDOM01 : Map{
public override void Create(){
{
SpaceWar2006.Effects.EclipticNode e=new SpaceWar2006.Effects.EclipticNode();
e.Position=new Vector3(0f,0f,0f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.DominationPoint e=new SpaceWar2006.GameObjects.DominationPoint();
e.Position=new Vector3(3820f,0f,150f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.DominationPoint e=new SpaceWar2006.GameObjects.DominationPoint();
e.Position=new Vector3(90f,0f,4020f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.DominationPoint e=new SpaceWar2006.GameObjects.DominationPoint();
e.Position=new Vector3(-3640f,0f,230f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.DominationPoint e=new SpaceWar2006.GameObjects.DominationPoint();
e.Position=new Vector3(120f,0f,-3710f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(760f,0f,870f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(-580f,0f,890f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(780f,0f,-480f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.PlayerStart e=new SpaceWar2006.GameObjects.PlayerStart();
e.Position=new Vector3(-540f,0f,-500f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-1740f,0f,-630f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-1760f,0f,170f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-2010f,0f,820f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-2270f,0f,-1230f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.GameObjects.Nebula e=new SpaceWar2006.GameObjects.Nebula();
e.Position=new Vector3(-2780f,0f,-1770f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-620f,0f,1940f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-880f,0f,2260f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-640f,0f,2220f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1030f,0f,2460f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1260f,0f,2660f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1400f,0f,2160f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1720f,0f,2720f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1420f,0f,3100f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1690f,0f,3200f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1480f,0f,2500f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-1850f,0f,3430f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-2290f,0f,3060f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-470f,0f,1670f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-580f,0f,1410f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-2190f,0f,3440f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-2450f,0f,3980f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-2990f,0f,4120f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-3380f,0f,4510f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(70f,0f,1490f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Phobos e=new SpaceWar2006.Planets.Phobos();
e.Position=new Vector3(-2210f,0f,2240f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Saturn e=new SpaceWar2006.Planets.Saturn();
e.Position=new Vector3(570f,0f,-1520f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
{
SpaceWar2006.Planets.Mars e=new SpaceWar2006.Planets.Mars();
e.Position=new Vector3(2220f,0f,490f);
e.rotationspeed=new Vector3(0f,0f,0f);
e.Orientation=new Quaternion(0f,0f,0f,1f);
Spawn(e,true);
}
}}}
