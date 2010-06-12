using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Cheetah.Graphics
{
    public partial class GlControl : global::OpenTK.GLControl, IUserInterface
    {
        class MouseControl : IControl
        {
            #region IControl Members

            public float GetPosition(int axis)
            {
                switch (axis)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        throw new Exception();
                }
            }

            public bool GetButtonState(int n)
            {
                if (n >= Buttons.Length)
                    return false;
                return Buttons[n];
            }

            #endregion

            public int X;
            public int Y;
            public bool[] Buttons = new bool[5];
        }

        class KeyboardControl : IControl
        {
            #region IControl Members

            public float GetPosition(int axis)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public bool GetButtonState(int n)
            {
                return Keys[n];
            }

            #endregion
            public bool[] Keys = new bool[512];
        }

        //private OpenTK.OpenGL.ColorDepth cd = new OpenTK.OpenGL.ColorDepth(24);
        //private OpenTK.OpenGL.GLContext glc = null;
        private OpenGL gl;

        public GlControl()
        {
            InitializeComponent();
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            if (this.DesignMode) return;
            //glc = OpenTK.OpenGL.GLContext.Create(this, cd, 8, 0);
            //glc.MakeCurrent();
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (this.DesignMode)
            {
                e.Graphics.Clear(this.BackColor);
                e.Graphics.Flush();
                return;
            }
            base.OnPaint(e);
            //glc.MakeCurrent();
            //OpenTK.OpenGL.GL.Clear(OpenTK.OpenGL.Enums.ClearBufferMask.COLOR_BUFFER_BIT | OpenTK.OpenGL.Enums.ClearBufferMask.DEPTH_BUFFER_BIT);
            //glc.SwapBuffers();
        }

        #region IUserInterface Members

        public void Swap()
        {
            this.Context.SwapBuffers();
        }
        public void Create(bool fullscreen, int width, int height, bool audio)
        {
            //glc.MakeCurrent();
            if (!this.Context.IsCurrent)
            {
                throw new Exception("not current");
            }
            gl = new OpenGL(Width, Height);
            gl.SwapFunc = Swap;
        }

        public void ProcessEvents()
        {
            Application.DoEvents();
        }

        public bool wantsQuit()
        {
            return false;
        }

        public IControl GetControl(ControlID id)
        {
            switch (id)
            {
                case ControlID.Keyboard:
                    return k;
                case ControlID.Mouse:
                    return m;
            }
            return null;
        }

        public IControl[] GetAvailableControls()
        {
            return new IControl[0];
        }

        KeyboardControl k = new KeyboardControl();
        public IControl Keyboard
        {
            get { return k; }
        }

        MouseControl m = new MouseControl();
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

        IAudio audio=new DummyAudio();
        public IAudio Audio
        {
            get { return audio; }
        }

        #endregion

        #region ITickable Members

        public void Tick(float dtime)
        {
        }

        #endregion

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            m.X = e.Location.X;
            m.Y = e.Location.Y;

            Root.Instance.ClientOnMouseMove(m.X, m.Y);

        }

        int ButtonNumber(MouseButtons b)
        {
            switch (b)
            {
                case MouseButtons.Left:
                    return 0;
                case MouseButtons.Right:
                    return 1;
                case MouseButtons.Middle:
                    return 2;
                default:
                    return -1;
            }
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            int n = ButtonNumber(e.Button);
            if (n >= 0)
                m.Buttons[n] = true;

            Root.Instance.ClientOnMouseDown(n, e.Location.X, e.Location.Y);
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            int n = ButtonNumber(e.Button);
            if (n >= 0)
                m.Buttons[n] = false;
        }

        /*KeyCode Translate(KeyEventArgs e)
        {
            if(e.KeyValue>=(int)Keys.A && e.KeyValue<=(int)Keys.Z)
            {
                int c = new string(new char[] { (char)e.KeyValue }).ToLower()[0];
                return (KeyCode)c;
            }
            switch (e.KeyCode)
            {
                case Keys.PageUp:
                    return KeyCode.PAGEUP;
                case Keys.PageDown:
                    return KeyCode.PAGEDOWN;
                case Keys.Space:
                    return KeyCode.SPACE;
                case Keys.Tab:
                    return KeyCode.TAB;
                case Keys.Return:
                    return KeyCode.RETURN;
                case Keys.D0:
                    return KeyCode._0;
                case Keys.D1:
                    return KeyCode._1;
                case Keys.D2:
                    return KeyCode._2;
                case Keys.D3:
                    return KeyCode._3;
                case Keys.D4:
                    return KeyCode._4;
                case Keys.D5:
                    return KeyCode._5;
                case Keys.D6:
                    return KeyCode._6;
                case Keys.D7:
                    return KeyCode._7;
                case Keys.D8:
                    return KeyCode._8;
                case Keys.D9:
                    return KeyCode._9;
                default:
                    System.Console.WriteLine("unknown key: " + e.KeyCode);
                    return KeyCode.WORLD_95;
            }
        }*/

        private void GlControl_KeyDown(object sender, KeyEventArgs e)
        {

            /*KeyCode kc = Translate(e);
            System.Console.WriteLine(e.KeyValue + " " + e.KeyCode + "=>" + (int)kc + " " + kc);

            k.Keys[(int)kc] = true;

            Root.Instance.ClientOnKeyDown(new Key(kc, KeyModifier.NONE));
            */

        }

        private void GlControl_KeyUp(object sender, KeyEventArgs e)
        {
            //k.Keys[(int)Translate(e)] = false;
        }

        private void GlControl_SizeChanged(object sender, EventArgs e)
        {
            if (!this.DesignMode && Renderer!=null)
            {
                Renderer.WindowSize = new Point(Size.Width,Size.Height);
                Renderer.RenderSize = new Point(Size.Width, Size.Height);
            }
        }
    }
}
