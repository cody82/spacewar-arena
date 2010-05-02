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

using Cheetah;

using OpenTK.Input;

namespace SpaceWar2006.Cameras
{
    public class FollowCamera : Camera
    {
        public FollowCamera()
        {
            Fov = 60;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            Vector3 campos;

            if (Target == null)
            {
                campos = Position;
            }
            else
            {
                Position = Target.AbsolutePosition - Target.Direction * 1000 + new Vector3(0,500,0);
                //Position = campos;
                LookAt(Target.AbsolutePosition);
            }
            /*
            Vector3 wantedpos;
            Vector3 targetpos;
            if (Target != null)
            {
                targetpos = Target.SmoothPosition;
            }
            else
                targetpos = Vector3.Zero;

            wantedpos = targetpos + campos;

            Position += (wantedpos - Position) * dtime * 0.5f;
            //Position = Target.SmoothPosition + campos;
            */

        }

        public Node Target;
    }

    public class TopCamera : Camera
    {
        public TopCamera()
        {
            Fov = 45;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if(Target!=null)
            {
                Position = Target.AbsolutePosition + RelativePosition+Target.Direction*300+Target.Speed;
                LookAt(Target.AbsolutePosition + Target.Direction*300 + Target.Speed);
            }
        }

        Vector3 RelativePosition = new Vector3(0,2500,1);
        public Node Target;
    }

    public class IsoCamera : Camera
    {
        public IsoCamera()
        {
            Fov = 45;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if(Target!=null)
            {
                Position = Target.AbsolutePosition + RelativePosition;
                LookAt(Target.AbsolutePosition);
            }
        }

        Vector3 RelativePosition = new Vector3(1000,1500,1000);
        public Node Target;
    }

    public class OverviewCamera : Camera
    {
        public OverviewCamera()
        {
            Fov = 45;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            Vector3 campos;

            if (Target == null)
            {
                campos = Position;
            }
            //else if (Target.Kill)
            //{
            //    campos = Target.Position+new Vector3(-1500, 1000, -1000);
            //}
            else
            {
                float camheight = 1500 + 2 * Target.SmoothSpeed.GetMagnitude();
                campos = new Vector3(0, 0, 800);
                if (Root.Instance.UserInterface.Keyboard.GetButtonState((int)Key.Space))
                    camheight *= 4;
                if (Root.Instance.UserInterface.Keyboard.GetButtonState((int)Key.Z))
                {
                    camheight /= 5;
                    campos /= 3;
                }
                campos.Y = camheight;
            }

            Vector3 wantedpos;
            Vector3 targetpos;
            if (Target != null)
            {
                smoothtargetspeed += (Target.SmoothSpeed - smoothtargetspeed) * dtime * 0.5f;
                targetpos = Target.SmoothPosition + smoothtargetspeed;
            }
            else
                targetpos = Vector3.Zero;

            wantedpos = targetpos + campos;

            Position += (wantedpos - Position) * dtime * 0.5f;
            //Position = Target.SmoothPosition + campos;

            LookAt(targetpos);
        }

        Vector3 smoothtargetspeed;
        public Node Target;
    }

}
