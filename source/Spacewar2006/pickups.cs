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
using Cheetah;
using Cheetah.Graphics;

namespace SpaceWar2006.Pickups
{
    [Editable]
    public class MissilePickup : Pickup
    {
        public MissilePickup()
            : base(new Dictionary<Type, int>())
        {
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("missilepickup/missilepickup.mesh"));
            Items.Add(typeof(HomingMissileLauncher), 1);
            Items.Add(typeof(HomingMissile), 10);
            RenderRadius = 100;
            SyncRefs = false;
        }
        public MissilePickup(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, 50);
        }
    }


    [Editable]
    public class PulseLaserPickup : Pickup
    {
        public PulseLaserPickup()
            : base(new Dictionary<Type, int>())
        {
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("missilepickup/missilepickup.mesh"));
            Items.Add(typeof(PulseLaserCannon), 2);
            RenderRadius = 100;
            SyncRefs = false;
        }
        public PulseLaserPickup(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, 50);
        }
    }

    public abstract class Pickup : Cheetah.Physics.PhysicsNode
    {
        public Pickup(Type t, int n)
        {
            Items = new Dictionary<Type, int>();
            Items[t] = n;
        }

        public Pickup()
        {
        }

        protected override Cheetah.Physics.IPhysicsObject CreatePhysicsObject(Scene s)
        {
            CollisionInfo info = GetCollisionInfo();
            SphereCollisionInfo sphere = info as SphereCollisionInfo;
            if (sphere != null)
            {
                Cheetah.Physics.IPhysicsObject obj = s.Physics.CreateObjectSphere(sphere.Sphere.Radius, 1);
                obj.Position = base.Position;
                obj.Speed = base.Speed;
                obj.Orientation = base.Orientation;
                obj.Owner = this;
                return obj;
            }
            else
                return base.CreatePhysicsObject(s);
        }

        public Pickup(Dictionary<Type, int> items)
        {
            Items = items;
        }
        public override void OnCollide(Node other)
        {
            base.OnCollide(other);

            if (!(Root.Instance.IsAuthoritive))
                return;

            if (Transfer((Actor)other))
            {
                Kill = true;
            }
        }

        public override bool CanCollide(Node other)
        {
            return other is Actor;
        }

        public bool Transfer(Actor a)
        {
            if (a.Inventory != null && Items != null)
            {
                foreach (KeyValuePair<Type, int> kv in Items)
                {
                    Cheetah.Console.WriteLine("transfering " + kv.Value.ToString() + " " + kv.Key.Name + " to " + a.ToString());
                    a.Inventory.Load(kv.Key, kv.Value);
                }
                return true;
            }
            return false;
        }

        public Dictionary<Type, int> Items;
    }

    [Editable]
    public class Repair : Pickup
    {
        public Repair()
        {
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("repair/repair.mesh"));
            RenderRadius = 100;
            SyncRefs = false;

        }
        public Repair(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
        public override CollisionInfo GetCollisionInfo()
        {
            return new SphereCollisionInfo(AbsolutePosition, 50);
        }
        public override bool CanCollide(Node other)
        {
            return other is SpaceShip;
        }
        public override void OnCollide(Node other)
        {
            if (Root.Instance.IsAuthoritive)
            {
                base.OnCollide(other);

                SpaceShip s = other as SpaceShip;
                if (s != null)
                {
                    s.Hull.CurrentHitpoints = s.Hull.MaxHitpoints;
                    s.Shield.CurrentCharge = s.Shield.MaxEnergy;
                    //System.Console.WriteLine("repaired.");
                    Kill = true;
                }
            }
        }
    }
}