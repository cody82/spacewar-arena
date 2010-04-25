using System;
using System.Reflection;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Audio;

namespace Cheetah.OpenTK
{
    public class Keyboard : IControl
    {
        KeyboardDevice dev;
        bool[] keys = new bool[256];

        public Keyboard(KeyboardDevice dev)
        {
            this.dev = dev;
            dev.KeyDown += new EventHandler<KeyboardKeyEventArgs>(dev_KeyDown);
            dev.KeyUp += new EventHandler<KeyboardKeyEventArgs>(dev_KeyUp);
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

    public class Audio : IAudio
    {
        #region IAudio Members

        public Sound Load(Stream s)
        {
            throw new NotImplementedException();
        }

        public Channel Play(Sound sound, Vector3 pos, bool loop)
        {
            throw new NotImplementedException();
        }

        public void SetListener(Vector3 pos, Vector3 forward, Vector3 up)
        {
            throw new NotImplementedException();
        }

        public void SetPosition(Channel channel, Vector3 pos)
        {
            throw new NotImplementedException();
        }

        public void Stop(Channel channel)
        {
            throw new NotImplementedException();
        }

        public void Free(Sound sound)
        {
            throw new NotImplementedException();
        }

        public bool IsPlaying(Channel channel)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITickable Members

        public void Tick(float dtime)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class Mouse : IControl
    {
        MouseDevice dev;
        bool[] buttons;

        public Mouse(MouseDevice dev)
        {
            this.dev = dev;
            dev.ButtonDown += new EventHandler<MouseButtonEventArgs>(dev_ButtonDown);
            dev.ButtonUp += new EventHandler<MouseButtonEventArgs>(dev_ButtonUp);
            buttons = new bool[dev.NumberOfButtons];
        }

        void dev_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    buttons[0] = false;
                    break;
                case MouseButton.Right:
                    buttons[1] = false;
                    break;
                case MouseButton.Middle:
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
                //bool b = buttons[n];
                //buttons[n] = false;
                //return b;
                return false;
            }
            return buttons[n-1];
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
            kb = new Keyboard(window.Keyboard);
            if (audio)
                this.audio = new FmodAudio();
            else
                this.audio = new DummyAudio();

            window.Visible = true;
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