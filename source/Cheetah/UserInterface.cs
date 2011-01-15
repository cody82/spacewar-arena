using System;
using System.Reflection;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
//using Tao.OpenAl;
using Cheetah.Graphics;
using OpenTK;

namespace Cheetah
{
    public enum ControlID
    {
        None=0,Keyboard, Mouse, Joystick0, Joystick1, Joystick2, Joystick3
    }

	public interface IUserInterface : IDisposable,ITickable
	{
		void Create(bool fullscreen,int width,int height, bool audio);
		
		void ProcessEvents();
		bool wantsQuit();

        IControl GetControl(ControlID id);
        IControl[] GetAvailableControls();

		IControl Keyboard
	{
		get;
	}
	IControl Mouse
	{
		get;
	}
    IControl Joystick
    {
        get;
    }
        IRenderer Renderer
        {
            get;
            set;
        }
	IAudio Audio
		{
			get;
		}

    void Flip();
	}



	public interface IKeyboard
	{
		bool isKeyPressed(int key);
	}

    public class DummyControl : IControl
    {
        #region IControl Members

        public float GetPosition(int axis)
        {
            return 0;
        }

        public bool GetButtonState(int n)
        {
            return false;
        }

        #endregion

        public static DummyControl Instance = new DummyControl();
    }

	public struct VectorInt2
	{
		public int x,y;
	}


	public enum RenderMode
	{
		Draw2D,Draw3D,Draw3DWireFrame,Draw3DPointSprite,DrawSkyBox
	}

    public enum TextureFormat
    {
        DXT1, DXT2, DXT3, DXT4, DXT5, RGB, RGBA
    }

	public class TextureId
	{
        public float LastBind = 0;
	}

	public interface IRenderer
	{
		void Clear(float r,float g,float b,float a);
		//void Flip();

		void SetMode(RenderMode m);
        void SetPointSize(float s);

		//drawing functions
		void Draw(VertexBuffer vertices,PrimitiveType type,int offset,int count,IndexBuffer ib);
		void Draw(string text,float x,float y,float sx,float sy,Cheetah.Texture t,Color4f c,float width);
        void Draw(string text, float x, float y, float sx, float sy, Cheetah.Texture t, Color4f c, float width,RectangleF scissor);
        void Draw(Cheetah.Graphics.VertexBuffer vertices, PrimitiveType type, int offset, int count, IndexBuffer ib, int indexoffset);

		//texture functions
        TextureId CreateCompressedTexture(byte[][] mipmaps, TextureFormat codec, int w, int h);
		TextureId CreateTexture(byte[] rgba,int w,int h,bool alpha);
        TextureId CreateTexture(int w, int h, bool alpha, bool depth);
        TextureId CreateDepthTexture(int w, int h);
		void UpdateTexture(Cheetah.TextureId t,byte[] rgba);
		void BindTexture(TextureId t);
		void BindTexture(TextureId t,int unit);
		void FreeTexture(TextureId t);
        Cheetah.TextureId CreateCubeTexture(byte[] xpos, byte[] xneg, byte[] ypos, byte[] yneg, byte[] zpos, byte[] zneg, int w, int h);
        Cheetah.TextureId CreateCompressedCubeTexture(byte[] xpos, byte[] xneg, byte[] ypos, byte[] yneg, byte[] zpos, byte[] zneg, TextureFormat codec,int w, int h);

		//vertex buffer functions
		VertexBuffer CreateStaticVertexBuffer(object data,int length);
		DynamicVertexBuffer CreateDynamicVertexBuffer(int length);
		void FreeVertexBuffer(VertexBuffer b);

        RenderTarget CreateRenderTarget(TextureId texture, Cheetah.TextureId depth);
        void BindRenderTarget(RenderTarget target);

		//shader functions
		//IEffect CreateEffect(string code);
		//void FreeEffect(IEffect e);
        //Shader CreateShader(string vertex, string fragment);
        Shader CreateShader(string vertex, string fragment, string geometry, PrimitiveType input, PrimitiveType output);
        void FreeShader(Shader s);
        void UseShader(Shader s);
        void SetUniform(int location, float[] values);
        void SetUniform(int location, int[] values);
        void SetAttribute(int location, float[] values);

		void SetLighting(bool b);
		void SetLight(int index,Light l);
		void SetMaterial(Material m);
		//void SetFog(Fog f);

		void SetCamera(Camera c);

		void PushMatrix();
		void PopMatrix();
		void LoadMatrix(Matrix4 m);
		void MultMatrix(Matrix4 m);

		void GetMatrix(float[] modelview,float[] projection);
        float[] GetRasterPosition(float[] pos3d);
        Vector3 UnProject(float[] winxyz,float[] model,float[] proj,int[] viewport);

        System.Drawing.Bitmap Screenshot();
        Cheetah.Graphics.Image Screenshot2();

        Point WindowSize
        {
            get;
            set;
        }
        Point RenderSize
        {
            get;
            set;
        }
        Point Size
		{
			get;
		}
	}

	public interface IMouse
	{
		VectorInt2 getPosition();
		VectorInt2 getRelativePosition();
		bool isButtonPressed(int number);
	}

    public class DummyMouse : IMouse
    {
        #region IMouse Members

        public VectorInt2 getPosition()
        {
            return new VectorInt2();
        }

        public VectorInt2 getRelativePosition()
        {
            return new VectorInt2();
        }

        public bool isButtonPressed(int number)
        {
            return false;
        }

        #endregion
    }

    public class Channel : IDisposable
    {
        #region IDisposable Members

        public virtual void Dispose()
        {
        }

        #endregion
    }
    public class Sound : IResource
    {
        public Sound(IAudio a)
        {
            audio = a;
        }


        #region IDisposable Members

        public virtual void Dispose()
        {
            
        }

        #endregion
        IAudio audio;
    }

    public interface IAudio : ITickable, IDisposable
	{
        Sound Load(Stream s);
        //Sound Open(Stream s);
        Channel Play(Sound sound,Vector3 pos,bool loop);
        void SetListener(Vector3 pos,Vector3 forward,Vector3 up);
        void SetPosition(Channel channel, Vector3 pos);
        void Stop(Channel channel);
        void Free(Sound sound);
        bool IsPlaying(Channel channel);
    }

    public class FmodChannel : Channel
    {
        public FmodChannel(FMOD.Channel c)
        {
            channel = c;
        }
        public FMOD.Channel channel;
    }
    public class FmodSound : Sound
    {
        public FmodSound(FmodAudio a,FMOD.Sound s,bool stream)
            :base(a)
        {
            sound = s;
            IsStream = stream;
        }

        public FMOD.Sound sound;
        public bool IsStream;
    }

    public class DummySound : Sound
    {
        public DummySound()
            : base(null)
        {
        }
    }
    public class DummyChannel : Channel
    {
    }

    public class DummyAudio : IAudio
    {
        #region IAudio Members

        public Sound Load(Stream s)
        {
            return new DummySound();
        }

        public Channel Play(Sound sound, Vector3 pos,bool loop)
        {
            return new DummyChannel();
        }
        public bool IsPlaying(Channel channel)
        {
            return true;
        }

        public void SetListener(Vector3 pos, Vector3 forward, Vector3 up)
        {
        }

        public void SetPosition(Channel channel, Vector3 pos)
        {
        }

        public void Stop(Channel channel)
        {
        }

        public void Free(Sound sound)
        {
        }

        #endregion

        #region ITickable Members

        public void Tick(float dtime)
        {
        }

        #endregion

        public void Dispose()
        {
        }
    }

    public class FmodAudio : IAudio
    {
        public void Tick(float dtime)
        {
            system.update();
        }

        public void Dispose()
        {
            system.close();
        }

        public bool IsPlaying(Channel channel)
        {
            FMOD.RESULT result;
            bool b = false;
            result=((FmodChannel)channel).channel.isPlaying(ref b);
            //ERRCHECK(result);
            return b;
        }
        public void SetListener(Vector3 pos, Vector3 forward, Vector3 up)
        {
            FMOD.RESULT result;
            FMOD.VECTOR pos2;
            FMOD.VECTOR forward2;
            FMOD.VECTOR up2;
            FMOD.VECTOR v=new FMOD.VECTOR();
            pos2.x = pos.X;
            pos2.y = pos.Y;
            pos2.z = pos.Z;
            forward2.x = forward.X;
            forward2.y = forward.Y;
            forward2.z = forward.Z;
            up2.x = up.X;
            up2.y = up.Y;
            up2.z = up.Z;

            result = system.set3DListenerAttributes(0, ref pos2, ref v, ref forward2, ref up2);
            ERRCHECK(result);
        }
        public void SetPosition(Channel channel, Vector3 pos)
        {
            FMOD.RESULT result;
            FMOD.VECTOR pos2;
            FMOD.VECTOR vel2=new FMOD.VECTOR();
            pos2.x = pos.X;
            pos2.y = pos.Y;
            pos2.z = pos.Z;
            FmodChannel c = (FmodChannel)channel;
            result = c.channel.set3DAttributes(ref pos2, ref vel2);
            ERRCHECK(result);
        }

        private FMOD.System system = null;
        private void ERRCHECK(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                throw new Exception("FMOD error! " + result + " - " + FMOD.Error.String(result));
            }
        }

        public FmodAudio()
        {
            uint version = 0;
            FMOD.RESULT result;

            result = FMOD.Factory.System_Create(ref system);
            ERRCHECK(result);

            result = system.getVersion(ref version);
            ERRCHECK(result);
            if (version < FMOD.VERSION.number)
            {
                throw new Exception("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
            }

            //result = system.init(40, FMOD.INITFLAG.NORMAL| FMOD.INITFLAG._3D_RIGHTHANDED, (IntPtr)null);
            result = system.init(40, FMOD.INITFLAG.NORMAL, (IntPtr)null);
            ERRCHECK(result);
            result = system.set3DSettings(1, 1, 10);
            ERRCHECK(result);
        }

        public Sound Load(Stream s)
        {
            byte[] buffer=new byte[s.Length];
            s.Read(buffer, 0, (int)s.Length);

            FMOD.RESULT result;
            FMOD.Sound sound1=null;
            FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();
            exinfo.cbsize = Marshal.SizeOf(exinfo);
            exinfo.length = (uint)s.Length;

            //result = system.createSound(buffer, (FMOD.MODE.HARDWARE | FMOD.MODE._3D | FMOD.MODE.OPENMEMORY), ref exinfo, ref sound1);
            result = system.createSound(buffer, (FMOD.MODE.HARDWARE | FMOD.MODE._3D | FMOD.MODE.OPENMEMORY | FMOD.MODE._3D_LINEARROLLOFF), ref exinfo, ref sound1);
            ERRCHECK(result);

            sound1.set3DMinMaxDistance(500, 5000);
            return new FmodSound(this,sound1,false);
        }

        public Channel Play(Sound sound, Vector3 pos, bool loop)
        {
            FMOD.Channel channel = null;
            FMOD.RESULT result;

            result = system.playSound(FMOD.CHANNELINDEX.FREE, ((FmodSound)sound).sound, true, ref channel);
            ERRCHECK(result);

            FmodChannel c = new FmodChannel(channel);
            SetPosition(c, pos);
            if (loop)
                channel.setLoopCount(-1);

            channel.setPaused(false);
            return c;
        }

        public void Stop(Channel channel)
        {
            FMOD.RESULT result=((FmodChannel)channel).channel.stop();
            //ERRCHECK(result);
        }

        public void Free(Sound sound)
        {
            FMOD.RESULT result = ((FmodSound)sound).sound.release();
            ERRCHECK(result);
            ((FmodSound)sound).sound = null;
        }

    }


	public class DummyTexture : Texture
	{
	}

    public class RenderTarget
    {
        public TextureId Texture;
    }

	public class Texture : IResource
	{
		public Texture()
		{
		}
		public Texture(TextureId id)
		{
			Id=id;
		}

		public virtual void Dispose()
		{
		}
		//public Texture(Color4b[] data,object tag);
		//public void Bind();
		public int width;
		public int height;
		public TextureId Id;
		//public int format;
	}

	public class DynamicTexture : Texture
	{
	}

	public class VideoTexture : DynamicTexture,ITickable
	{
		public VideoTexture(Stream s)
		{
            Avi = new AviLib.AviFile(s);
            try
            {
                XviD = new XviD.XviD(Avi.Width, Avi.Height);
            }
            catch (Exception e)
            {
                Cheetah.Console.WriteLine("xvid doesnt work");
                XviD = null;
                return;
            }



			Frame=new byte[Avi.MaxFrameSize];
			int l=Avi.ReadFrame(Frame);

			ActiveSurface=new byte[Avi.Width*Avi.Height*3];
			DecodingSurface=new byte[Avi.Width*Avi.Height*3];

            XviD.Decode(Frame,l,ActiveSurface);
			Id=Root.Instance.UserInterface.Renderer.CreateTexture(ActiveSurface,Avi.Width,Avi.Height,false);
            DecodedFrame = LoadedFrame = 0;
            UpdateThread=new Thread(new ThreadStart(UpdateFunc));
			UpdateThread.Start();
		}

		public override void Dispose()
		{
			if (UpdateThread!=null && !ExitThread)
			{
                StopThread();
			}
		}

        protected void StopThread()
        {
            ExitThread = true;
            if (Idle)
                UpdateThread.Resume();
            System.Console.WriteLine("stopping video update thread...");
            UpdateThread.Join();
            System.Console.WriteLine("video update thread ended.");
        }

		~VideoTexture()
		{
			Dispose();
		}

		protected void UpdateSurface()
		{
			if(LoadedFrame!=DecodedFrame)
			{
				SwapMutex.WaitOne();
				Root.Instance.UserInterface.Renderer.UpdateTexture(Id,ActiveSurface);
				LoadedFrame=DecodedFrame;
				SwapMutex.ReleaseMutex();
			}
		}

		protected void Swap()
		{
			SwapMutex.WaitOne();
			byte[] tmp=ActiveSurface;
			ActiveSurface=DecodingSurface;
			DecodingSurface=tmp;
			SwapMutex.ReleaseMutex();
		}

		public void Tick(float dtime)
		{
            if (XviD == null)
                return;

            if (Root.Instance.Time - Id.LastBind > 5)
            {
                if (!Idle)
                {
                    Cheetah.Console.WriteLine("suspending video thread.");
                    UpdateThread.Suspend();
                    Idle = true;
                }
            }
            else
            {
                if (Idle)
                {
                    Cheetah.Console.WriteLine("resuming video thread.");
                    UpdateThread.Resume();
                    Idle = false;
                }
                Time += dtime;
                UpdateSurface();
            }
		}

		protected void UpdateFunc()
		{
			while(!ExitThread)
			{
                int wantframe = (int)(Avi.Fps * Time);
                bool newframe=false;
                while (wantframe > DecodedFrame)
                {
                    int l = Avi.ReadFrame(Frame);
                    newframe=true;
                    XviD.Decode(Frame, l, DecodingSurface);
                    DecodedFrame++;
                }
                if(newframe)
                    Swap();
                Thread.Sleep(0);
			}
		}

        private bool Idle=false;
		private Thread UpdateThread;
		private XviD.XviD XviD;
		private AviLib.AviFile Avi;
		private byte[] Frame;
		private byte[] ActiveSurface;
		private byte[] DecodingSurface;
		private Mutex SwapMutex=new Mutex();
		private int LoadedFrame;
		private int DecodedFrame;
		private float Time;
		private bool ExitThread=false;
	}
}
