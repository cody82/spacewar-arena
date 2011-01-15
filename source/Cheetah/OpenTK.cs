using System;
using System.Reflection;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

using Cheetah.Graphics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Diagnostics;

namespace Cheetah
{
    public class MathUtil
    {
        [Conditional("DEBUG")]
        public static void Check(float[] f)
        {
            foreach (float f2 in f)
            {
                if (float.IsNaN(f2))
                {
                    throw new Exception("NaN");
                }
            }
        }
        [Conditional("DEBUG")]
        public static void Check(Vector3 v)
        {
            Check(v.ToFloats());
        }
        [Conditional("DEBUG")]
        public static void Check(Matrix4 m)
        {
            Check(m.ToFloats());
        }
        [Conditional("DEBUG")]
        public static void Check(Quaternion q)
        {
            Check(new float[] { q.X, q.Y, q.Z, q.W });
        }
    }

    public static class QuaternionExtensions
    {
        public static Quaternion GetInverse(this Quaternion q)
        {
            return Divide(q.GetConjugate(), q.LengthSquared);
        }
        public static Quaternion GetConjugate(this Quaternion q)
        {
            return new Quaternion(-q.X, -q.Y, -q.Z, q.W);
        }
        public static Quaternion Multiply(this Quaternion a, float scale)
        {
            return new Quaternion(a.X * scale, a.Y * scale, a.Z * scale, a.W * scale);
        }
        public static Quaternion Divide(this Quaternion a, float scale)
        {
            return Multiply(a, 1 / scale);
        }
        

        public static Matrix4 ToMatrix4(this Quaternion q)
        {
            Vector3 axis;
            float angle;
            q.ToAxisAngle(out axis, out angle);
            MathUtil.Check(axis);
            if (float.IsNaN(angle))
                angle = 0;
            Matrix4 m = Matrix4.CreateFromAxisAngle(axis, angle);

            //Matrix4 m = Matrix4.Rotate(q);
            MathUtil.Check(m);
            return m;
        }
        public static Quaternion FromAxisAngle(float ax,float ay,float az,float a)
        {
            return Quaternion.FromAxisAngle(new Vector3(ax, ay, az), a);
        }
        public static Quaternion FromAxisAngle(Vector3 axis, float a)
        {
            return Quaternion.FromAxisAngle(axis, a);
        }
        public static Quaternion FromMatrix4(Matrix4 m)
        {
            Quaternion q;
            FromMatrix4(ref m, out q);
            return q;
        }


        /// <summary>

        /// Create a <cref>OpenTK.Quaternion</cref> from a rotation <cref>OpenTK.Matrix4</cref>.

        /// </summary>

        /// <param name="matrix">The source of the rotation matrix.</param>

        /// <param name="result">The output target for the result.</param>

        /// <remarks>

        /// This method is based on the code here:

        /// http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm

        /// </remarks>

        public static void FromMatrix4(ref Matrix4 matrix, out Quaternion result)
        {

            float halfScale;

            float oneOverScale;

            // We will calculate oneOverScale by knowing that:

            // 0.5 / halfScale = 0.5 / ( 0.5 * Scale ) = 1 / Scale.



            // The goal of those if & else blocks is to avoid square root( = scale) of a nagitive number.

            // We do it by swapping each element by the other if needed.

            if ((matrix.M11 + matrix.M22 + matrix.M33) > 0.0F)// If trace > 0, it is the simplest case.
            {

                halfScale = (float)System.Math.Sqrt((double)(matrix.M11 + matrix.M22 + matrix.M33 + 1.0F));

                oneOverScale = 0.5F / halfScale;

                // In that case, XYZ is the vector, and W is the scale.

                result = new Quaternion((matrix.M23 - matrix.M32) * oneOverScale,

                    (matrix.M31 - matrix.M13) * oneOverScale,

                    (matrix.M12 - matrix.M21) * oneOverScale, halfScale * 0.5F);

                return;

            }

            //And the non-friendly cases...



            //else

            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {

                halfScale = (float)System.Math.Sqrt((double)(1.0F + matrix.M11 - matrix.M22 - matrix.M33));

                oneOverScale = 0.5F / halfScale;

                result = new Quaternion(0.5F * halfScale, (matrix.M12 + matrix.M21) * oneOverScale,

                    (matrix.M13 + matrix.M31) * oneOverScale, (matrix.M23 - matrix.M32) * oneOverScale);

                return;

            }

            //else

            if (matrix.M22 > matrix.M33)
            {

                halfScale = (float)System.Math.Sqrt((double)(1.0F + matrix.M22 - matrix.M11 - matrix.M33));

                oneOverScale = 0.5F / halfScale;

                result = new Quaternion((matrix.M21 + matrix.M12) * oneOverScale, 0.5F * halfScale,

                    (matrix.M32 + matrix.M23) * oneOverScale, (matrix.M31 - matrix.M13) * oneOverScale);

                return;

            }

            //else

            halfScale = (float)System.Math.Sqrt((double)(1.0F + matrix.M33 - matrix.M11 - matrix.M22));

            oneOverScale = 0.5F / halfScale;

            result = new Quaternion((matrix.M31 + matrix.M13) * oneOverScale, (matrix.M32 + matrix.M23) * oneOverScale,

                0.5F * halfScale, (matrix.M12 - matrix.M21) * oneOverScale);

        }
    }

    public static class Matrix4Extensions
    {
        public static Matrix4 FromTranslation(Vector3 position)
        {
            return Matrix4.CreateTranslation(position);
        }
        public static Matrix4 FromTranslation(float px,float py,float pz)
        {
            return Matrix4.CreateTranslation(px,py,pz);
        }
        public static Matrix4 FromQuaternion(Quaternion q)
        {
            if (q.W < -1.0f)
                throw new Exception("q.W<-1.0");
            return Matrix4.Rotate(q);
        }
        public static Matrix4 FromBasis(Vector3 x,Vector3 y,Vector3 z)
        {
            Matrix4 m = Matrix4.Identity;
            m.Row0 = new Vector4(x.X, x.Y, x.Z, 0);
            m.Row1 = new Vector4(y.X, y.Y, y.Z, 0);
            m.Row2 = new Vector4(z.X, z.Y, z.Z, 0);
            m.Row3 = new Vector4(0, 0, 0, 1);
            return m;
        }
        public static Matrix4 FromScale(float x, float y, float z)
        {
            return Matrix4.Scale(x, y, z);
        }
        public static Matrix4 FromAngleAxis(Vector3 axis,float a)
        {
            return Matrix4.CreateFromAxisAngle(axis, a);
        }
        public static Matrix4 FromFloats(float[] f)
        {
            return new Matrix4(f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7], f[8], f[9], f[10], f[11], f[12], f[13], f[14], f[15]);
        }

        public static float[] ToFloats(this Matrix4 m)
        {
            return new float[] { m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44 };
        }

        public static Vector3 Transform(this Matrix4 m, Vector3 v)
        {
            return Vector3.Transform(v, m);
        }
        public static Quaternion ExtractRotation(this Matrix4 m)
        {
            return QuaternionExtensions.FromMatrix4(m);
        }
        public static Vector4 Transform(this Matrix4 m, Vector4 v)
        {
            return Vector4.Transform(v, m);
        }
        public static void Set(this Matrix4 m, int x,int y,float f)
        {
            throw new Exception("TODO");
        }

        public static Vector3 ExtractTranslation(this Matrix4 m)
        {
            return new Vector3(m.Row3.X, m.Row3.Y, m.Row3.Z);
        }
        public static void ExtractBasis(this Matrix4 m, out Vector3 x,out Vector3 y,out Vector3 z)
        {
            x = new Vector3(m.Row0.X, m.Row0.Y, m.Row0.Z);
            y = new Vector3(m.Row1.X, m.Row1.Y, m.Row1.Z);
            z = new Vector3(m.Row2.X, m.Row2.Y, m.Row2.Z);
        }
        public static void Translate(this Matrix4 m, Vector3 v)
        {
            m.Row3.X = m.Row0.X * v.X + m.Row1.X * v.Y + m.Row2.X * v.Z + m.Row3.X;
            m.Row3.Y = m.Row0.Y * v.X + m.Row1.Y * v.Y + m.Row2.Y * v.Z + m.Row3.Y;
            m.Row3.Z = m.Row0.Z * v.X + m.Row1.Z * v.Y + m.Row2.Z * v.Z + m.Row3.Z;
        }

        public static Matrix4 GetInverse(this Matrix4 m)
        {
            return Matrix4.Invert(m);
        }
    }

    public static class Vector3Extensions
    {
        public static float[] ToFloats(this Vector3 m)
        {
            return new float[] { m.X,m.Y,m.Z};
        }
        public static Vector3 GetUnit(this Vector3 m)
        {
            Vector3 v = m;
            v.Normalize();
            return v;
        }
    }
}

namespace Cheetah.OpenTK
{
    public class OpenTkSound : Sound
    {
        internal int id=-1;
        public OpenTkSound(Stream s)
            : base(null)
        {
            throw new Exception("audioreader missing(BUG)");
            /*AudioReader sound;
            try
            {
                sound = new AudioReader(s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }


            AL.GenBuffers(1, out id);


            AL.BufferData(id, sound.ReadToEnd());
            //sound.Dispose();
            if (AL.GetError() != ALError.NoError)
            {
                throw new Exception();
            }*/
        }

        public override void Dispose()
        {
            if(id!=-1)
                AL.DeleteBuffer(id);
        }
    }

    public class OpenTkChannel : Channel
    {
        public OpenTkChannel(OpenTkSound sound, Vector3 pos, bool loop)
        {
            if (sound.id == -1)
                return;

            AL.GenSources(1, out id);

            AL.Source(id, ALSourcei.Buffer, sound.id); // attach the buffer to a source

            AL.SourcePlay(id); // start playback
            AL.Source(id, ALSourceb.Looping, loop); // source loops infinitely
        }

        public override void Dispose()
        {
            if(id!=-1)
                AL.DeleteSource(id);
        }

        internal int id=-1;
    }

    public class OpenTkAudio : IAudio
    {
        AudioContext context;

        public OpenTkAudio()
        {
            context = new AudioContext();
        }

        #region IAudio Members

        public Sound Load(Stream s)
        {
            return new OpenTkSound(s);
        }

        public Channel Play(Sound sound, Vector3 pos, bool loop)
        {
            OpenTkChannel c = new OpenTkChannel((OpenTkSound)sound,pos,loop);
            return c;
        }

        public bool IsPlaying(Channel channel)
        {
            int id = ((OpenTkChannel)channel).id;
            if (id != -1)
                return AL.GetSourceState(id) == ALSourceState.Playing;
            else return false;
        }

        public void SetListener(Vector3 pos, Vector3 forward, Vector3 up)
        {
        }

        public void SetPosition(Channel channel, Vector3 pos)
        {
        }

        public void Stop(Channel channel)
        {
            int id = ((OpenTkChannel)channel).id;
            if(id!=-1)
                AL.SourceStop(((OpenTkChannel)channel).id);
        }

        public void Free(Sound sound)
        {
            sound.Dispose();
        }

        #endregion

        #region ITickable Members

        public void Tick(float dtime)
        {
            context.Process();
        }

        #endregion

        public void Dispose()
        {
            context.Dispose();
        }
    }

    public class Keyboard : IControl
    {
        KeyboardDevice dev;
        bool[] keys = new bool[256];

        public Keyboard(GameWindow w)
        {
            this.dev = w.Keyboard;
            dev.KeyDown += new EventHandler<KeyboardKeyEventArgs>(dev_KeyDown);
            dev.KeyUp += new EventHandler<KeyboardKeyEventArgs>(dev_KeyUp);
            w.KeyPress += new EventHandler<KeyPressEventArgs>(w_KeyPress);
        }

        void w_KeyPress(object sender, KeyPressEventArgs e)
        {
            Root.Instance.ClientOnKeyPress(e.KeyChar);
        }

        void dev_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            keys[(int)e.Key] = false;
        }

        void dev_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            keys[(int)e.Key] = true;
            Root.Instance.ClientOnKeyDown(e.Key);
        }

        #region IControl Members

        public float GetPosition(int axis)
        {
            throw new NotImplementedException();
        }

        public bool GetButtonState(int n)
        {
            return keys[n];
        }

        #endregion
    }

    public class Mouse : IControl
    {
        MouseDevice dev;
        bool[] buttons;
        int wheelup;
        int wheeldown;

        public Mouse(MouseDevice dev)
        {
            this.dev = dev;
            dev.ButtonDown += new EventHandler<MouseButtonEventArgs>(dev_ButtonDown);
            dev.ButtonUp += new EventHandler<MouseButtonEventArgs>(dev_ButtonUp);
            dev.WheelChanged += new EventHandler<MouseWheelEventArgs>(dev_WheelChanged);
            dev.Move += new EventHandler<MouseMoveEventArgs>(dev_Move);
            Console.WriteLine("mouse:" + dev.NumberOfButtons.ToString() + " buttons.");
            buttons = new bool[dev.NumberOfButtons];
        }

        void dev_Move(object sender, MouseMoveEventArgs e)
        {
            Root.Instance.ClientOnMouseMove(e.X, e.Y);

        }

        void dev_WheelChanged(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                wheelup += e.Delta;
            else
                wheeldown -= e.Delta;
        }

        void dev_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
					if(buttons.Length>0)
                    	buttons[0] = false;
                    break;
                case MouseButton.Right:
					if(buttons.Length>1)
                    	buttons[1] = false;
                    break;
                case MouseButton.Middle:
 					if(buttons.Length>2)
                   		buttons[2] = false;
                    break;
                default:
                    break;
            }
        }

        void dev_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            int button=0;
            switch (e.Button)
            {
                case MouseButton.Left:
                    button = 0;
                    break;
                case MouseButton.Right:
                    button = 1;
                    break;
                case MouseButton.Middle:
                    button = 2;
                    break;
                default:
                    break;
            }
			if(button>=0 && button<buttons.Length)
            	buttons[button]=true;
            Root.Instance.ClientOnMouseDown(button + 1, e.X, e.Y);
       }

        #region IControl Members

        public float GetPosition(int axis)
        {
            switch (axis)
            {
                case 0:
                    return dev.X;
                case 1:
                    return dev.Y;
                default:
                    throw new Exception();
            }
        }

        public bool GetButtonState(int n)
        {
            if (n == 4 || n == 5)
            {
                if (n == 4 && wheelup>0)
                {
                    wheelup = 0;
                    return true;
                }
                else if(n == 5 &&wheeldown>0)
                {
                    wheeldown = 0;
                    return true;
                }
                return false;
            }
			if(n>0&&n<=buttons.Length)
            	return buttons[n-1];
			return false;
        }

        #endregion
    }

    public class UserInterface : IUserInterface
    {
        GameWindow window;
        OpenGL gl;
        Keyboard kb;
        Mouse m;
        IAudio audio;

        #region IUserInterface Members

        public void Create(bool fullscreen, int width, int height, bool audio)
        {
            Root.Instance.UserInterface = this;
            window = new GameWindow(width, height, GraphicsMode.Default, "OpenTK Window", fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default);
            gl = new OpenGL(width, height);
            m = new Mouse(window.Mouse);
            kb = new Keyboard(window);
            if (audio)
                this.audio = new FmodAudio();
                //this.audio = new OpenTkAudio();
            else
                this.audio = new DummyAudio();

            window.Visible = true;
			
			Config c = (Config)Root.Instance.ResourceManager.Load("config/global.config", typeof(Config));
            if(c.Table.ContainsKey("video.vsync"))
			{
				string vsync = c.GetString("video.vsync");
				if(vsync=="on"||vsync=="true")
				{
					window.VSync=VSyncMode.On;
				}
				else if(vsync=="off"||vsync=="false")
				{
					window.VSync=VSyncMode.Off;
				}
				else
				{
					window.VSync=VSyncMode.Adaptive;
				}
			}
			else
			{
				window.VSync=VSyncMode.Adaptive;
			}
        }

        public void ProcessEvents()
        {
            window.ProcessEvents();
        }

        public bool wantsQuit()
        {
            return window.IsExiting;
        }

        public IControl GetControl(ControlID id)
        {
            switch (id)
            {
                case ControlID.Keyboard:
                    return kb;
                case ControlID.Mouse:
                    return m;
                default:
                    return null;
            }
        }

        public IControl[] GetAvailableControls()
        {
            return new IControl[] { kb, m };
        }

        public IControl Keyboard
        {
            get { return kb; }
        }

        public IControl Mouse
        {
            get { return m; }
        }

        public IControl Joystick
        {
            get { return null; }
        }

        public IRenderer Renderer
        {
            get
            {
                return gl;
            }
            set
            {
                gl = (OpenGL)value;
            }
        }

        public void Flip()
        {
            if (!window.IsExiting && window.Exists && !window.IsExiting)
            {
                gl.Flip();
                window.SwapBuffers();
            }
        }

        public IAudio Audio
        {
            get { return audio; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            audio.Dispose();
            window.Close();
            window.Dispose();
        }

        #endregion

        #region ITickable Members

        public void Tick(float dtime)
        {
        }

        #endregion
    }
}