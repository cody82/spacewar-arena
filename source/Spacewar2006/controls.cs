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
using System.Runtime.Serialization.Formatters.Soap;

using SpaceWar2006.GameObjects;
using Cheetah;

using OpenTK.Input;
using OpenTK;

namespace SpaceWar2006.Controls
{

    public class ActorBaseControl : ITickable
    {
        public ActorBaseControl(Actor target)
        {
            TargetActor = target;
        }

        public void UpdateDirection(Vector3 lookat, float dtime)
        {

            //left vector projected on plane
            Vector3 left = TargetActor.Left;
            left.Y = 0;
            left.Normalize();

            //forward vector projected on plane
            Vector3 forward = TargetActor.Direction;
            forward.Y = 0;
            forward.Normalize();

            //wanted direction vector
            Vector3 want;
            try
            {
                want = lookat - TargetActor.Position;
                want.Normalize();
            }
            catch (DivideByZeroException)
            {
                System.Console.WriteLine("divide bug./%&$");
                want = Vector3.UnitX;
            }

            float cos = Vector3.Dot(left, want);
            if (cos <= 1 && cos >= -1)
            {
                //float a = (float)Math.Acos((double)cos) * 180.0f / (float)Math.PI;
                //float a = -2 * cos;
                RotationSpeed = -2 * (float)Math.Sign(cos) * (float)Math.Sqrt(Math.Abs(cos));

                Rotation += RotationSpeed * dtime;
                Roll = cos;// -RotationSpeed / 2;
            }
            else
            {
                RotationSpeed = 0;
                Roll = 0;
            }

            Quaternion q1 = QuaternionExtensions.FromAxisAngle(0, 1, 0, Rotation);
            TargetActor.Orientation = q1;
            Quaternion q2 = QuaternionExtensions.FromAxisAngle(TargetActor.Direction, Roll);
            TargetActor.Orientation = q1 * q2;

            Vector3 tmp = TargetActor.Position;
            tmp.Y = 0;
            TargetActor.Position = tmp;
        }

        protected float RotationSpeed = 0;
        protected float Rotation = 0;
        protected float Roll = 0;
        protected Actor TargetActor;

        #region ITickable Members

        public virtual void Tick(float dtime)
        {
        }

        #endregion
    }

    public class SpaceShipControlInput
    {
        public Vector3 LookAt;
        public float Thrust;
        public float Strafe;
        public bool Fire1;
        public bool Fire2;
        public bool Select;
        //public bool[] CycleWeapon = new bool[10];
        public int CycleWeapon=-1;
        public bool NextWeapon;
        public bool PreviousWeapon;
    }
    /*
    public class SpaceShipJoystickControl : SpaceShipControlBase
    {
        public SpaceShipJoystickControl(SpaceShip s, Node cursor)
            : base(s)
        {
            Cursor = cursor;
        }

        protected override SpaceShipControlInput GetControls()
        {
            SpaceShipControlInput input = new SpaceShipControlInput();
            input.Fire1 = Fire.GetButtonState();
            input.Fire2 = FireSecondary.GetButtonState();
            //input.LookAt = Cursor.Position;
            input.Strafe = Strafe.GetAxisPosition();
            input.Thrust = Thrust.GetAxisPosition();
            input.Select = Select.GetButtonState();
            input.CycleWeapon = new bool[Cycle.Length];
            for (int i = 0; i < Cycle.Length; ++i)
                input.CycleWeapon[i] = Cycle[i].GetButtonState();
            return input;
        }
        public override float GetRotation(SpaceShipControlInput input)
        {
            return Rotate.GetAxisPosition();
        }

        public ControlInfo Strafe = new ControlInfo(ControlID.Joystick0, 4, 6, 1, false);
        public ControlInfo Fire = new ControlInfo(ControlID.Joystick0, 0);
        public ControlInfo Thrust = new ControlInfo(ControlID.Joystick0, 1, 1, true, 0);
        public ControlInfo Select = new ControlInfo(ControlID.Joystick0, 1);
        public ControlInfo Rotate = new ControlInfo(ControlID.Joystick0, 0, 1, false, 0);
        public ControlInfo FireSecondary = new ControlInfo(ControlID.Joystick0, 2);
        public ControlInfo[] Cycle = new ControlInfo[]{
            new ControlInfo(ControlID.Keyboard, '1'),
            new ControlInfo(ControlID.Keyboard, '2'),
            new ControlInfo(ControlID.Keyboard, '3'),
            new ControlInfo(ControlID.Keyboard, '4'),
            new ControlInfo(ControlID.Keyboard, '5'),
            new ControlInfo(ControlID.Keyboard, '6'),
            new ControlInfo(ControlID.Keyboard, '7'),
            new ControlInfo(ControlID.Keyboard, '8'),
            new ControlInfo(ControlID.Keyboard, '9'),
            new ControlInfo(ControlID.Keyboard, '0')
        };

        public Node Cursor;
    }
    */
    public class SpaceShipControl : SpaceShipControlBase
    {
        public SpaceShipControl(SpaceShip s, Node cursor)
            : base(s)
        {
            Cursor = cursor;
        }

        public SpaceShipControl()
            :base(null)
        {
        }

        public override float GetRotation(SpaceShipControlInput input)
        {
            if (MouseLook)
                return base.GetRotation(input);
            else
                return Rotate.GetAxisPosition();
        }

        protected override SpaceShipControlInput GetControls()
        {
            SpaceShipControlInput input = new SpaceShipControlInput();
            input.Fire1 = Fire.GetButtonState();
            input.Fire2 = FireSecondary.GetButtonState();
            input.LookAt = Cursor.Position;
            input.Strafe = Strafe.GetAxisPosition();
            input.Thrust = Thrust.GetAxisPosition();
            input.Select = Select.GetButtonState();
            for (int i = 0; i < Cycle.Length; ++i)
            {
                if(Cycle[i].GetButtonState())
                {
                    input.CycleWeapon = i;
                }
            }
            input.NextWeapon = NextWeapon.GetButtonState();
            input.PreviousWeapon = PreviousWeapon.GetButtonState();
            return input;
        }
        public void Save()
        {
            /*SoapFormatter soap = new SoapFormatter();

            Stream s = new FileStream("controls.xml", FileMode.Create);
            soap.Serialize(s, new object[] { Thrust, Rotate, Strafe, Fire, FireSecondary, Select, MouseLook, NextWeapon, PreviousWeapon });
            s.Close();*/
        }


        public void Load()
        {
            return;
            SoapFormatter soap = new SoapFormatter();

            try
            {
                Stream s = new FileStream("controls.xml", FileMode.Open);
                object[] ci = (object[])soap.Deserialize(s);
                s.Close();
                Thrust = (ControlInfo)ci[0];
                Rotate = (ControlInfo)ci[1];
                Strafe = (ControlInfo)ci[2];
                Fire = (ControlInfo)ci[3];
                FireSecondary = (ControlInfo)ci[4];
                Select = (ControlInfo)ci[5];
                MouseLook = (bool)ci[6];
                NextWeapon = (ControlInfo)ci[7];
                PreviousWeapon = (ControlInfo)ci[8];
            }
            catch (Exception)
            {
                Save();
            }

        }

        public ControlInfo Strafe = new ControlInfo(ControlID.Keyboard, (int)Key.A, (int)Key.D, 1, false);
        public ControlInfo Fire = new ControlInfo(ControlID.Mouse, 1);
        public ControlInfo Thrust = new ControlInfo(ControlID.Keyboard, (int)Key.W, (int)Key.S, 1, false);
        public ControlInfo Select = new ControlInfo(ControlID.Mouse, 2);
        public ControlInfo FireSecondary = new ControlInfo(ControlID.Mouse, 3);
        public ControlInfo Rotate = new ControlInfo(ControlID.Joystick0, 0, 1, false, 0);
        public ControlInfo[] Cycle = new ControlInfo[]{
            new ControlInfo(ControlID.Keyboard, (int)Key.Number1),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number2),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number3),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number4),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number5),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number6),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number7),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number8),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number9),
            new ControlInfo(ControlID.Keyboard, (int)Key.Number0)
        };
        public ControlInfo NextWeapon = new ControlInfo(ControlID.Mouse, 4);
        public ControlInfo PreviousWeapon = new ControlInfo(ControlID.Mouse, 5);

        public Node Cursor;
        public bool MouseLook=true;
    }

    public abstract class SpaceShipControlBase : ActorBaseControl
    {
        public SpaceShipControlBase(SpaceShip s)
            : base(s)
        {
            Target = s;
        }

        public override void Tick(float dtime)
        {
            SpaceShipControlInput input = GetControls();

            if (input == null)
                return;

            if (Target.ControlsJamed)
            {
                Target.ThrustPower = 0;
                Target.StrafePower = 0;
                Vector3 tmp = Target.Position;
                tmp.Y = 0;
                TargetActor.Position = tmp;
            }
            else
            {
                Target.ThrustPower = input.Thrust;
                Target.StrafePower = input.Strafe;

                if (input.NextWeapon)
                {
                    Target.NextWeapon();
                }
                if (input.PreviousWeapon)
                {
                    Target.PreviousWeapon();
                }

                if (input.Fire1)
                {
                    Target.Fire();
                }
                if (input.Fire2)
                {
                    Target.Fire(1);
                }
                if (input.Select)
                {
                    Target.Computer.TargetNearestToCursor(input.LookAt);
                }
                //for (int i = 0; i < Target.Slots.Length; ++i)
                {
                    if (input.CycleWeapon >= 0 && input.CycleWeapon < Target.Slots.Length)
                    {
                        Target.CycleWeapon(input.CycleWeapon);
                    }
                }
                //float cos = Target.GetCosDirection(input.LookAt);
                Target.RotationPower = GetRotation(input);
                //Cheetah.Console.WriteLine(Target.RotationPower.ToString());
            }
        }

        public virtual float GetRotation(SpaceShipControlInput input)
        {
            float cos = Target.GetCosDirection(input.LookAt);
            float f = -Math.Max(Math.Min(cos * 3, 1.0f), -1.0f);
            if (float.IsNaN(f))
                throw new Exception("NaN");
            return f;
        }

        protected abstract SpaceShipControlInput GetControls();


        public SpaceShip Target;
    }

}
