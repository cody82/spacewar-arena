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
using SpaceWar2006.Weapons;
using SpaceWar2006.Effects;

using Cheetah;
using Cheetah.Graphics;

namespace SpaceWar2006.Ships
{

    [Editable]
    public class Sulaco : SpaceShip
    {
        public Sulaco()
        {

            Draw = new ArrayList();
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("cnv301-low/cnv301-low.mesh"));
            Draw.Add(model=Root.Instance.ResourceManager.LoadMesh("sulaco/sulaco.mesh"));
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("saucer1/saucer1.mesh"));
            //Transparent = true;
            //model.BBox = new BoundingBox(new Vector3(-50, -100, -350), new Vector3(50, 100, 350));
            //ShieldModel = Root.Instance.ResourceManager.LoadMesh("cnv-shield/cnv-shield.mesh");
            ShieldModel = Root.Instance.ResourceManager.LoadMesh("sulaco-shield/sulaco-shield.mesh");


            Battery = new Battery(200);
            Generator = new Generator(20, Battery);
            Inventory = new Inventory(200);
            //Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(LaserCannon), 2);
            Inventory.Load(typeof(RailGun), 1);
            Inventory.Load(typeof(IonPulseCannon), 2);
            Inventory.Load(typeof(DisruptorCannon), 2);
            Inventory.Load(typeof(PulseLaserCannon), 2);
            Inventory.Load(typeof(Mine), 10);
            Inventory.Load(typeof(MineLayer), 1);
            //Inventory.Load(typeof(MissileLauncher), 1);
            Slots = new Slot[3]{
								 new Slot(null,new Vector3(46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(-46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(64,0,0),Quaternion.Identity,0)
							 };
            Shield = new Shield(Battery, 300, 5);
            Hull = new Hull(200);
            Computer = new Computer(this);
            Radius = 150;
            RenderRadius = 700;
            SyncRefs = false;
            ShieldSound = Root.Instance.ResourceManager.LoadSound("ST_IMP_FEDROM_Photon_Shield_1.wav");

            WeaponNumbers = new int[] { -1, -1, -1 };
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 0);
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 1);

            MainThrust = 80;
            StrafeThrust = 30;
            Resistance = 0.1f;
            RollSpeed = 0.5f;
            MaxRoll = 0.5f;
            MaxRotationSpeed = 0.3f;
        }
        Mesh model;
        BoundingBox bbox = new BoundingBox(new Vector3(-50, -100, -350), new Vector3(50, 100, 350));
        public override CollisionInfo GetCollisionInfo()
        {
            return new BoxCollisionInfo(Matrix, bbox);
        }

        public override string Name
        {
            get { return "Sulaco"; }
        }

        public Sulaco(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
        public override void SpawnExplosion()
        {
            base.SpawnExplosion();

            Root.Instance.Scene.Spawn(new BigExplosion(AbsolutePosition+Direction*300, Vector3.Zero));
            Root.Instance.Scene.Spawn(new BigExplosion(AbsolutePosition - Direction * 300, Vector3.Zero));
         
        }
        public static string Thumbnail = "white.png";
    }

    [Editable]
    public abstract class MK9Hawk : SpaceShip
    {
        public MK9Hawk()
        {

            Draw = new ArrayList();
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("cnv301-low/cnv301-low.mesh"));
            Draw.Add(model=Root.Instance.ResourceManager.LoadMesh("mk9hawk/mk9hawk.mesh"));
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("saucer1/saucer1.mesh"));
            //Transparent = true;

            //ShieldModel = Root.Instance.ResourceManager.LoadMesh("cnv-shield/cnv-shield.mesh");
            ShieldModel = Root.Instance.ResourceManager.LoadMesh("mk9hawk-shield/mk9hawk-shield.mesh");


            Battery = new Battery(100);
            Generator = new Generator(10, Battery);
            Inventory = new Inventory(100);
            //Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(LaserCannon), 2);
            Inventory.Load(typeof(RailGun), 2);
            Inventory.Load(typeof(IonPulseCannon), 2);
            Inventory.Load(typeof(DisruptorCannon), 2);
            Inventory.Load(typeof(PulseLaserCannon), 2);
            Inventory.Load(typeof(Mine), 10);
            Inventory.Load(typeof(MineLayer), 2);
            //Inventory.Load(typeof(MissileLauncher), 1);
            Slots = new Slot[3]{
								 new Slot(null,new Vector3(46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(-46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(64,0,0),Quaternion.Identity,0)
							 };
            Shield = new Shield(Battery, 100, 5);
            Hull = new Hull(100);
            Computer = new Computer(this);
            Radius = 150;
            RenderRadius = 350;
            SyncRefs = false;
            ShieldSound = Root.Instance.ResourceManager.LoadSound("ST_IMP_FEDROM_Photon_Shield_1.wav");

            WeaponNumbers = new int[] { -1, -1, -1 };
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 0);
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 1);

            MainThrust = 600;
            StrafeThrust = 300;
            Resistance = 0.3f;
            RollSpeed = 1.5f;
            MaxRoll = 1.0f;
            MaxRotationSpeed = 0.5f;
        }
        Mesh model;
        public override CollisionInfo GetCollisionInfo()
        {
            return new BoxCollisionInfo(Matrix, model.BBox);
        }



        public override string Name
        {
            get { return "MK9 Hawk"; }
        }

        public override void OnRemove(Scene s)
        {
            base.OnRemove(s);
        }

        public MK9Hawk(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public static string Thumbnail = null;
    }


    [Editable]
    public abstract class Khitomer : SpaceShip
    {
        public Khitomer()
        {

            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("khitomer/khitomer.mesh"));
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("mk9hawk/mk9hawk.mesh"));
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("saucer1/saucer1.mesh"));
            //Transparent = true;

            ShieldModel = Root.Instance.ResourceManager.LoadMesh("cnv-shield/cnv-shield.mesh");
            //ShieldModel = Root.Instance.ResourceManager.LoadMesh("mk9hawk-shield/mk9hawk-shield.mesh");


            Battery = new Battery(100);
            Generator = new Generator(10, Battery);
            Inventory = new Inventory(100);
            //Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(LaserCannon), 2);
            Inventory.Load(typeof(RailGun), 2);
            Inventory.Load(typeof(IonPulseCannon), 2);
            Inventory.Load(typeof(DisruptorCannon), 2);
            Inventory.Load(typeof(PulseLaserCannon), 2);
            Inventory.Load(typeof(Mine), 10);
            Inventory.Load(typeof(MineLayer), 2);
            Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(HomingMissileLauncher), 2);
            //Inventory.Load(typeof(MissileLauncher), 1);
            Slots = new Slot[3]{
								 new Slot(null,new Vector3(46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(-46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(64,0,0),Quaternion.Identity,0)
							 };
            Shield = new Shield(Battery, 100, 5);
            Hull = new Hull(100);
            Computer = new Computer(this);
            Radius = 60;
            RenderRadius = 90;
            SyncRefs = false;
            ShieldSound = Root.Instance.ResourceManager.LoadSound("ST_IMP_FEDROM_Photon_Shield_1.wav");

            WeaponNumbers = new int[] { -1, -1, -1 };
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 0);
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 1);

            MainThrust = 600;
            StrafeThrust = 300;
            Resistance = 0.3f;
            RollSpeed = 1.5f;
            MaxRoll = 1.0f;
            MaxRotationSpeed = 2;
        }



        public override string Name
        {
            get { return "USS Khitomer"; }
        }

        public override void OnRemove(Scene s)
        {
            base.OnRemove(s);
        }

        public Khitomer(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public static string Thumbnail = null;
    }

    [Editable]
    public class Dreadnaught : SpaceShip
    {
        public Dreadnaught()
        {

            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("cnv301-low/cnv301-low.mesh"));
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("mk9hawk/mk9hawk.mesh"));
            //Draw.Add(new Marker());
            //Transparent = true;

            ShieldModel = Root.Instance.ResourceManager.LoadMesh("cnv-shield/cnv-shield.mesh");
            //ShieldModel = Root.Instance.ResourceManager.LoadMesh("mk9hawk-shield/mk9hawk-shield.mesh");


            Battery = new Battery(100);
            Generator = new Generator(10, Battery);
            Inventory = new Inventory(100);
            //Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(LaserCannon), 2);
            Inventory.Load(typeof(RailGun), 1);
            Inventory.Load(typeof(IonPulseCannon), 2);
            Inventory.Load(typeof(DisruptorCannon), 2);
            Inventory.Load(typeof(PulseLaserCannon), 2);
            Inventory.Load(typeof(Mine), 10);
            Inventory.Load(typeof(MineLayer), 1);
            Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(HomingMissileLauncher), 1);
            //Inventory.Load(typeof(MissileLauncher), 1);
            Slots = new Slot[3]{
								 new Slot(null,new Vector3(46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(-46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(64,0,0),Quaternion.Identity,0)
							 };
            Shield = new Shield(Battery, 100, 5);
            Hull = new Hull(100);
            Computer = new Computer(this);
            Radius = 60;
            RenderRadius = 90;
            SyncRefs = false;
            ShieldSound = Root.Instance.ResourceManager.LoadSound("cnv-shield.wav");

            WeaponNumbers = new int[] { -1, -1, -1 };
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 0);
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 1);

            MainThrust = 600;
            StrafeThrust = 300;
            Resistance = 0.3f;
            RollSpeed = 1.5f;
            MaxRoll = 1.0f;
            MaxRotationSpeed = 2;
        }



        public override string Name
        {
            get { return "CNV-301 Dreadnaught"; }
        }

        public override void OnRemove(Scene s)
        {
            base.OnRemove(s);
        }

        public Dreadnaught(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public static string Thumbnail = "cnv301-image.dds";
    }

    [Editable]
    public class BorgCube : SpaceShip
    {
        public BorgCube()
        {

            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("borgcube/borgcube.mesh"));
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("saucer1/saucer1.mesh"));
            //Transparent = true;

            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("defiant/defiant.mesh"));
            //Draw.Add(Root.Instance.ResourceManager.LoadMesh("ncc1701e/ncc1701e.mesh"));
            ShieldModel = Root.Instance.ResourceManager.LoadMesh("borgcube-shield/borgcube-shield.mesh");


            Battery = new Battery(200);
            Generator = new Generator(20, Battery);
            Inventory = new Inventory(100);
            //Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(LaserCannon), 2);
            Inventory.Load(typeof(RailGun), 2);
            Inventory.Load(typeof(IonPulseCannon), 2);
            Inventory.Load(typeof(DisruptorCannon), 2);
            Inventory.Load(typeof(Mine), 10);
            Inventory.Load(typeof(MineLayer), 2);
            //Inventory.Load(typeof(MissileLauncher), 1);
            Slots = new Slot[3]{
								 new Slot(null,new Vector3(46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(-46,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(64,0,0),Quaternion.Identity,0)
							 };
            Shield = new Shield(Battery, 300, 5);
            Hull = new Hull(300);
            Computer = new Computer(this);
            Radius = 100;
            RenderRadius = 150;
            SyncRefs = false;
            ShieldSound = Root.Instance.ResourceManager.LoadSound("ST_IMP_FEDROM_Photon_Shield_1.wav");

            WeaponNumbers = new int[] { -1, -1, -1 };
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 0);
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 1);

            MainThrust = 200;
            StrafeThrust = 150;
            Resistance = 0.2f;
            RollSpeed = 1.0f;
            MaxRoll = 0.4f;
            MaxRotationSpeed = 1;

        }



        public override string Name
        {
            get { return "Borg Cube"; }
        }

        public BorgCube(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public static string Thumbnail = "borg-image.dds";
    }

    public abstract class MilleniumFalcon : SpaceShip
    {
        public static string Thumbnail = "yt1300-image.dds";
    }
    public abstract class TieInterceptor : SpaceShip
    {
        public static string Thumbnail = "tiei-image.dds";
    }
    public abstract class EnterpriseE : SpaceShip
    {
        public static string Thumbnail = "ncc1701e-image.dds";
    }
    public abstract class Slave1 : SpaceShip
    {
        public static string Thumbnail = "slave1-image.dds";
    }
    public abstract class KangKodos : SpaceShip
    {
        public static string Thumbnail = "kangkodos-image.dds";
    }
    public abstract class Maquis : SpaceShip
    {
        public static string Thumbnail = "maquis-image.dds";
    }
    public abstract class ImperialStarDestroyer : SpaceShip
    {
        public static string Thumbnail = "isd-image.dds";
    }
    public abstract class Voyager : SpaceShip
    {
        public static string Thumbnail = "voyager-image.dds";
    }
    [Editable]
    public class TieFighter : SpaceShip
    {
        public TieFighter()
        {

            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("tie/tie.mesh"));
            //Transparent = true;

            //ShieldModel = Root.Instance.ResourceManager.LoadMesh("saucer1-shield/saucer1-shield.mesh");


            //typeof(Battery).GetCustomAttributes(
            Battery = new Battery(50);
            Generator = new Generator(10, Battery);
            Inventory = new Inventory(40);
            //Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(LaserCannon), 2);
            Inventory.Load(typeof(IonPulseCannon), 2);
            Inventory.Load(typeof(DisruptorCannon), 2);
            Inventory.Load(typeof(Mine), 10);
            Inventory.Load(typeof(MineLayer), 2);
            //Inventory.Load(typeof(MissileLauncher), 1);
            Slots = new Slot[]{
								 new Slot(null,new Vector3(20,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(-20,0,0),Quaternion.Identity,0)
							 };
            Shield = new Shield(Battery, 0, 0);
            Hull = new Hull(50);
            Computer = new Computer(this);
            Radius = 39;
            RenderRadius = 40;
            SyncRefs = false;
            //ShieldSound = Root.Instance.ResourceManager.LoadSound("cnv-shield.wav");

            WeaponNumbers = new int[] { -1, -1 };
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 0);
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 1);

            MainThrust = 2400;
            StrafeThrust = 600;
            Resistance = 0.7f;
            RollSpeed = 2.5f;
            MaxRoll = (float)Math.PI/5.0f;
            MaxRotationSpeed = 3;

        }



        public override string Name
        {
            get { return "Tie Fighter"; }
        }

        public TieFighter(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public static string Thumbnail = "tie-image.dds";
    }


    [Editable]
    public class Tydirium : SpaceShip
    {
        public Tydirium()
        {

            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("tydirium/tydirium.mesh"));
            //Transparent = true;

            ShieldModel = Root.Instance.ResourceManager.LoadMesh("saucer1-shield/saucer1-shield.mesh");


            Battery = new Battery(100);
            Generator = new Generator(10, Battery);
            Inventory = new Inventory(100);
            //Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(LaserCannon), 2);
            Inventory.Load(typeof(IonPulseCannon), 2);
            Inventory.Load(typeof(DisruptorCannon), 2);
            Inventory.Load(typeof(Mine), 10);
            Inventory.Load(typeof(MineLayer), 2);
            //Inventory.Load(typeof(MissileLauncher), 1);
            Slots = new Slot[]{
								 new Slot(null,new Vector3(36,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(-36,0,0),Quaternion.Identity,0)
							 };
            Shield = new Shield(Battery, 100, 5);
            Hull = new Hull(100);
            Computer = new Computer(this);
            Radius = 39;
            RenderRadius = 40;
            SyncRefs = false;
            ShieldSound = Root.Instance.ResourceManager.LoadSound("ST_IMP_FEDROM_Photon_Shield_1.wav");

            WeaponNumbers = new int[] { -1, -1 };
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 0);
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 1);
            MainThrust = 600;
            StrafeThrust = 300;
            Resistance = 0.3f;
            RollSpeed = 1.5f;
            MaxRoll = 1.0f;
            MaxRotationSpeed = 2;

        }



        public override string Name
        {
            get { return "Tydirium"; }
        }

        public Tydirium(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public static string Thumbnail = "tydirium-image.dds";
    }

    [Editable]
    public class Saucer : SpaceShip
    {
        public Saucer()
        {

            Draw = new ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("saucer1/saucer1.mesh"));
            //Transparent = true;

            ShieldModel = Root.Instance.ResourceManager.LoadMesh("saucer1-shield/saucer1-shield.mesh");


            Battery = new Battery(100);
            Generator = new Generator(10, Battery);
            Inventory = new Inventory(100);
            //Inventory.Load(typeof(HomingMissile), 10);
            Inventory.Load(typeof(LaserCannon), 2);
            Inventory.Load(typeof(IonPulseCannon), 2);
            Inventory.Load(typeof(DisruptorCannon), 2);
            Inventory.Load(typeof(Mine), 10);
            Inventory.Load(typeof(MineLayer), 2);
            //Inventory.Load(typeof(MissileLauncher), 1);
            Slots = new Slot[]{
								 new Slot(null,new Vector3(36,0,0),Quaternion.Identity,0),
								 new Slot(null,new Vector3(-36,0,0),Quaternion.Identity,0)
							 };
            Shield = new Shield(Battery, 100, 5);
            Hull = new Hull(100);
            Computer = new Computer(this);
            Radius = 39;
            RenderRadius = 40;
            SyncRefs = false;
            ShieldSound = Root.Instance.ResourceManager.LoadSound("ST_IMP_FEDROM_Photon_Shield_1.wav");

            WeaponNumbers = new int[] { -1, -1 };
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 0);
            ArmWeapon(GetWeaponNumber(typeof(LaserCannon)), 1);
            MainThrust = 600;
            StrafeThrust = 300;
            Resistance = 0.3f;
            RollSpeed = 1.5f;
            MaxRoll = 1.0f;
            MaxRotationSpeed = 2;

        }



        public override string Name
        {
            get { return "Flying Saucer"; }
        }

        public override void OnRemove(Scene s)
        {
            base.OnRemove(s);
        }

        public Saucer(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
        public static string Thumbnail = "saucer1-image.dds";

    }

}
