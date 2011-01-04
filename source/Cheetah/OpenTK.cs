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