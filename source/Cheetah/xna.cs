#if WINDOWS2
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using XnaVertexBuffer=Microsoft.Xna.Framework.Graphics.VertexBuffer;

namespace Cheetah.Xna
{
         public class XnaException : System.Exception
         {
             public XnaException(string text)
                 : base(text)
             {
             }
         }
   public class XnaRenderer : IRenderer
    {
        class TextureId : Cheetah.TextureId
        {
            public TextureId(Texture2D t)
            {
                XnaTexture = t;
            }
            public Texture2D XnaTexture;
        }

        class Include : CompilerIncludeHandler
        {
            public override System.IO.Stream Open(CompilerIncludeHandlerType includeType, string filename)
            {
                return null;
            }
        }

        class Shader : Cheetah.Shader
        {
            public Shader(CompiledShader vertex, CompiledShader fragment, VertexShader vshader, PixelShader pshader)
            {
                XnaCompiledVertexShader = vertex;
                XnaCompiledPixelShader = fragment;
                XnaVertexShader = vshader;
                XnaPixelShader = pshader;
            }
            public CompiledShader XnaCompiledVertexShader;
            public CompiledShader XnaCompiledPixelShader;
            public PixelShader XnaPixelShader;
            public VertexShader XnaVertexShader;

            public override int GetUniformLocation(string name)
            {
                //XnaCompiledVertexShader.ShaderConstantTable.Constants[
                throw new System.Exception("The method or operation is not implemented.");
            }

            public override int GetAttributeLocation(string name)
            {
                throw new System.Exception("The method or operation is not implemented.");
            }
        }
        class StaticVertexBuffer : Cheetah.VertexBuffer
        {
            public StaticVertexBuffer(XnaVertexBuffer vb)
            {
                vb = XnaBuffer;
            }
            public XnaVertexBuffer XnaBuffer;
        }
                class DynamicVertexBuffer : Cheetah.DynamicVertexBuffer
        {
                    public DynamicVertexBuffer(XnaVertexBuffer vb)
            {
                vb = XnaBuffer;
            }
            public XnaVertexBuffer XnaBuffer;
        }
        public XnaRenderer(GraphicsDevice device)
        {
            Device = device;
        }
        GraphicsDevice Device;

        #region IRenderer Members

        public void Clear(float r, float g, float b, float a)
        {
            Device.Clear(new Color((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255)));
        }

        public void Flip()
        {
            Device.Present();
        }

        public void SetMode(RenderMode m)
        {
        }

        public void SetPointSize(float s)
        {
        }

        public void Draw(VertexBuffer vertices, PrimitiveType type, int offset, int count, IndexBuffer ib)
        {
            Draw(vertices, type, offset, count, ib, 0);
        }

        public void Draw(string text, float x, float y, float sx, float sy, Texture t, Color4f c, float width)
        {
            //throw new System.Exception("The method or operation is not implemented.");
        }

        public void Draw(string text, float x, float y, float sx, float sy, Texture t, Color4f c, float width, System.Drawing.RectangleF scissor)
        {
            //throw new System.Exception("The method or operation is not implemented.");
        }

        public void Draw(Cheetah.VertexBuffer vertices, PrimitiveType type, int offset, int count, IndexBuffer ib, int indexoffset)
        {
            Device.BeginScene();

            XnaVertexBuffer vb;
            if (vertices is StaticVertexBuffer)
                vb = ((StaticVertexBuffer)vertices).XnaBuffer;
            else
                vb = ((DynamicVertexBuffer)vertices).XnaBuffer;

            Microsoft.Xna.Framework.Graphics.PrimitiveType pt;
            int primcount;
            switch (type)
            {
                case PrimitiveType.LINES:
                    pt = Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList;
                    primcount = count / 2;
                    break;
                case PrimitiveType.LINESTRIP:
                    pt = Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip;
                    primcount = count - 1;
                    break;
                case PrimitiveType.POINTS:
                    pt = Microsoft.Xna.Framework.Graphics.PrimitiveType.PointList;
                    primcount = count;
                    break;
                case PrimitiveType.QUADS:
                    throw new System.Exception("no QUADS!");
                    break;
                case PrimitiveType.TRIANGLES:
                    pt = Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList;
                    primcount = count / 3;
                    break;
                case PrimitiveType.TRIANGLESTRIP:
                    pt = Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleStrip;
                    primcount = count - 2;
                    break;
                case PrimitiveType.TRIANGLEFAN:
                    throw new System.Exception("no TRIANGLEFAN!");
                    break;
                default:
                    throw new System.Exception("??");

            }

            Device.Vertices[0].SetSource(vb, 0, vertices.Format.Size);
            VertexElement[] ve = new VertexElement[vertices.Format.Count];
            int eoffset=0;
            for (int i = 0; i < ve.Length; ++i)
            {
                switch (vertices.Format[i].Count)
                {
                    case 1:
                        ve[i].VertexElementFormat = VertexElementFormat.Single;
                        break;
                    case 2:
                        ve[i].VertexElementFormat = VertexElementFormat.Vector2;
                        break;
                    case 3:
                        ve[i].VertexElementFormat = VertexElementFormat.Vector3;
                        break;
                    case 4:
                        ve[i].VertexElementFormat = VertexElementFormat.Vector4;
                        break;
                    default:
                        throw new System.Exception("???");
                }

                ve[i].Offset = (short)eoffset;
                //ve[i].UsageIndex

                eoffset+=vertices.Format[i].Count*4;
            }

            Device.VertexDeclaration=new VertexDeclaration(Device,ve);

            Device.DrawIndexedPrimitives(pt, offset, 0, vertices.Count, indexoffset, primcount);

            Device.EndScene();
        }

        public Cheetah.TextureId CreateCompressedTexture(byte[][] mipmaps, TextureFormat codec, int w, int h)
        {
            Texture2D t=new Texture2D(Device, w, h, mipmaps.Length, ResourceUsage.None, SurfaceFormat.Dxt5, ResourcePool.Managed);
            t.SetData<byte>(0, null, mipmaps[0], 0, mipmaps[0].Length, SetDataOptions.Discard);
            return new TextureId(t);
        }

        public Cheetah.TextureId CreateTexture(byte[] rgba, int w, int h, bool alpha)
        {
            Texture2D t = new Texture2D(Device, w, h, 1, ResourceUsage.None, SurfaceFormat.Rgba32, ResourcePool.Managed);
            t.SetData<byte>(rgba, 0, rgba.Length, SetDataOptions.Discard);
            return new TextureId(t);
        }

        public Cheetah.TextureId CreateTexture(int w, int h, bool alpha, bool depth)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void UpdateTexture(Cheetah.TextureId t, byte[] rgba)
        {
            ((TextureId)t).XnaTexture.SetData<byte>(rgba, 0, rgba.Length, SetDataOptions.Discard);
        }

        public void BindTexture(Cheetah.TextureId t)
        {
            BindTexture(t, 0);
        }

        public void BindTexture(Cheetah.TextureId t, int unit)
        {
            //
            Device.Textures[unit] = ((TextureId)t).XnaTexture;
        }

        public void FreeTexture(Cheetah.TextureId t)
        {
            ((TextureId)t).XnaTexture.Dispose();
            ((TextureId)t).XnaTexture = null;
        }

        public Cheetah.TextureId CreateCubeTexture(byte[] xpos, byte[] xneg, byte[] ypos, byte[] yneg, byte[] zpos, byte[] zneg, int w, int h)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public Cheetah.TextureId CreateCompressedCubeTexture(byte[] xpos, byte[] xneg, byte[] ypos, byte[] yneg, byte[] zpos, byte[] zneg, TextureFormat codec, int w, int h)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public Cheetah.VertexBuffer CreateStaticVertexBuffer(object data, int length)
        {
            XnaVertexBuffer vb = new XnaVertexBuffer(Device, length, ResourceUsage.None, ResourcePool.Managed);
            return new StaticVertexBuffer(vb);
        }

        public Cheetah.DynamicVertexBuffer CreateDynamicVertexBuffer(int length)
        {
            XnaVertexBuffer vb = new XnaVertexBuffer(Device, length, ResourceUsage.None, ResourcePool.Managed);
            return new DynamicVertexBuffer(vb);
        }

        public void FreeVertexBuffer(VertexBuffer b)
        {
        }

        public RenderTarget CreateRenderTarget(Cheetah.TextureId texture, Cheetah.TextureId depth)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void BindRenderTarget(RenderTarget target)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public Cheetah.Shader CreateShader(string vertex, string fragment)
        {
            if (vertex == null)
                throw new XnaException("no vertexshader");
            if (fragment == null)
                throw new XnaException("no fragmentshader");

            CompiledShader v = ShaderCompiler.CompileFromSource(vertex, new CompilerMacro[] { }, new Include(), CompilerOptions.None, "main", ShaderProfile.VS_2_0, false);
            CompiledShader p = ShaderCompiler.CompileFromSource(fragment, new CompilerMacro[] { }, new Include(), CompilerOptions.None, "main", ShaderProfile.PS_2_0, false);
            return new Shader(
                v,p,new VertexShader(Device,v.GetShaderCode()),new PixelShader(Device,p.GetShaderCode())
            );
        }

        public void FreeShader(Cheetah.Shader s)
        {
        }

        public void UseShader(Cheetah.Shader s)
        {
            Device.VertexShader = ((Shader)s).XnaVertexShader;
            Device.PixelShader = ((Shader)s).XnaPixelShader;
        }

        public void SetUniform(int location, float[] values)
        {
            //Device.VertexShader.
        }

        public void SetUniform(int location, int[] values)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void SetAttribute(int location, float[] values)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void SetLighting(bool b)
        {
            //throw new System.Exception("The method or operation is not implemented.");
        }

        public void SetLight(int index, Light l)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void SetMaterial(Material m)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }


        public void SetCamera(Camera c)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void PushMatrix()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void PopMatrix()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void LoadMatrix(Matrix3 m)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void MultMatrix(Matrix3 m)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void GetMatrix(float[] modelview, float[] projection)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public float[] GetRasterPosition(float[] pos3d)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public float[] UnProject(float[] winxyz, float[] model, float[] proj, int[] viewport)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public System.Drawing.Bitmap Screenshot()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public Image Screenshot2()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public System.Drawing.Point WindowSize
        {
            get
            {
                return new System.Drawing.Point(Device.DisplayMode.Width, Device.DisplayMode.Height);
            }
        }

        public System.Drawing.Point RenderSize
        {
            get
            {
                return new System.Drawing.Point(Device.DisplayMode.Width, Device.DisplayMode.Height);
            }
        }

        public System.Drawing.Point Size
        {
            get
            {
                return new System.Drawing.Point(Device.DisplayMode.Width, Device.DisplayMode.Height);
            }
        }

        #endregion
    }

    public class XnaUserInterface : IUserInterface
    {
        Form Window;

        XnaRenderer renderer;
        #region IUserInterface Members

        public void Create(bool fullscreen, int width, int height, bool audio)
        {
            GraphicsDevice Device;
            Window = new Form();
            Window.ClientSize = new System.Drawing.Size(width, height);

            Window.Show();

            //GraphicsAdapter.DefaultAdapter.
            PresentationParameters param=new PresentationParameters();
            param.BackBufferCount=1;
            //param.BackBufferFormat=SurfaceFormat.Rgba32;
            param.BackBufferHeight=height;
            param.BackBufferWidth=width;
            //param.FullScreenRefreshRateInHz=60;
            param.IsFullScreen=fullscreen;
            param.PresentFlag=PresentFlag.None;
            param.SwapEffect=fullscreen?SwapEffect.Flip:SwapEffect.Discard;
            param.PresentationInterval=PresentInterval.One;
            param.AutoDepthStencilFormat = DepthFormat.Depth16;
            param.BackBufferFormat = SurfaceFormat.Bgr32;
            param.EnableAutoDepthStencil = true;
            param.PresentationInterval = PresentInterval.Immediate;

            try
            {
                Device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, DeviceType.Hardware, Window.Handle, CreateOptions.HardwareVertexProcessing, param);
            }
            catch (InvalidCallException e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.ErrorCode);
                throw e;
            }

            Device.Clear(new Color(0, 0, 0, 1));
            Device.Present();

            renderer = new XnaRenderer(Device);
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
            return null;
        }

        public IControl[] GetAvailableControls()
        {
            return new IControl[] { };
        }

        public IControl Keyboard
        {
            get
            {
                return null;
            }
        }

        public IControl Mouse
        {
            get
            {
                return null;
            }
        }

        public IControl Joystick
        {
            get
            {
                return null;
            }
        }

        public IRenderer Renderer
        {
            get
            {
                return renderer;
            }
            set
            {
                throw new System.Exception("The method or operation is not implemented.");
            }
        }

        public IAudio Audio
        {
            get { return null; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion

        #region ITickable Members

        public void Tick(float dtime)
        {
            
        }

        #endregion
    }
}
#endif