using System;
using System.Collections.Generic;
using System.Collections;

using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

using System.Text;
using Cheetah.Graphics;

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Cheetah.Graphics
{
    public class ShaderManager
    {
        public class Entry
        {
            public Entry(string v, string f, Shader s)
            {
                VertexProgram = v;
                FragmentProgram = f;
                Shader = s;
            }
            public string VertexProgram;
            public string FragmentProgram;
            public Shader Shader;
        }

        public ShaderManager(IRenderer r)
        {
            Renderer = r;
        }

        protected void Add(int id, string vertex, string fragment, Shader s)
        {
            Shaders[id] = new Entry(vertex, fragment,s);
        }

        protected bool Contains(int id)
        {
            return Shaders.ContainsKey(id);
        }

        public Shader GetShader(ShaderConfig cfg)
        {
            return Get(cfg).Shader;
        }
        public Shader GetShader(Material m)
        {
            return Get(m).Shader;
        }

        public Shader GetShader(int id)
        {
            return Shaders[id].Shader;
        }


        public Entry Get(int id)
        {
            return Shaders[id];
        }

        public Entry Get(ShaderConfig cfg)
        {
            int id = Generator.GetId(cfg);
            if (Contains(id))
                return Shaders[id];
            else
            {
                string v, f;
                if (Generator.GenerateShader(cfg, out v, out f) != id)
                    throw new Exception();
                Shader s = null;
                if (Renderer != null)
                {
                    //try
                    {
                        s = Renderer.CreateShader(v, f, null, 0, 0);
                    }
                    //catch (Exception e)
                    {
                        //StreamWriter sw1=new StreamWriter("c:\\vertex.txt", false);
                        //StreamWriter sw2 = new StreamWriter("c:\\fragment.txt", false);
                        //sw1.Write(v);
                        //sw2.Write(f);
                        //sw1.Close();
                        //sw2.Close();
                        //throw e;
                    }
                }
                Add(id, v, f, s);
                return Shaders[id];
            }
        }

        public ShaderConfig GetShaderConfig(Material m)
        {
            return Generator.MaterialToConfig(m);
        }

        public Entry Get(Material m)
        {
            ShaderConfig cfg=Generator.MaterialToConfig(m);
            return Get(cfg);
        }

        Dictionary<int, Entry> Shaders = new Dictionary<int, Entry>();
        IRenderer Renderer;
        GlslShaderGenerator Generator=new GlslShaderGenerator();
    }

    public class GlslShaderGenerator
    {
        public int GenerateShader(ShaderConfig cfg, out string vertexprogram, out string fragmentprogram)
        {
            Config = cfg;
            int id = GetId(cfg);

            string f = GenerateFragmentProgram();
            string v = GenerateVertexProgram();

            vertexprogram = v;
            fragmentprogram = f;

            //System.Console.WriteLine(v);
            //System.Console.WriteLine(f);

            return id;
        }

        public ShaderConfig MaterialToConfig(Material m)
        {
            ShaderConfig c = new ShaderConfig();
            c.BumpMap = m.BumpMap != null;
            c.DiffuseMap = m.diffusemap != null;
            c.EmissiveMap = m.EmissiveMap != null;
            c.LightCount = 8;
            c.LightRangeAttenuation = true;
            c.ReflectionMap = m.ReflectionMap != null;
            c.SpecularMap = m.SpecularMap != null;
            c.SphereMap = m.EnvironmentMap != null;
            c.TangentSpace = m.BumpMap != null;
            return c;
        }

        public int GenerateShader(Material m, out string vertexprogram, out string fragmentprogram)
        {
            return GenerateShader(MaterialToConfig(m), out vertexprogram, out fragmentprogram);
        }

        protected string GenerateFragmentProgram()
        {

            Output = new StringBuilder();
            Output.AppendLine("// Fragment Program");
            Output.AppendLine("// Uniforms");
            GenerateFragmentProgramUniforms();
            Output.AppendLine("");
            Output.AppendLine("// Varyings");
            GenerateVaryings();
            Output.AppendLine("");
            Output.AppendLine("// Attributes");
            GenerateFragmentProgramAttributes();
            Output.AppendLine("");
            Output.AppendLine("// Variables");
            GenerateFragmentProgramVariables();
            Output.AppendLine("");
            Output.AppendLine("// Functions");
            GenerateFragmentProgramFunctions();
            Output.AppendLine("");
            Output.AppendLine("// Main");
            GenerateFragmentProgramMain();
            Output.AppendLine("");
            Output.AppendLine("// End Fragment Program");

            return Output.ToString();
        }

        protected string GenerateVertexProgram()
        {
            Output = new StringBuilder();
            Output.AppendLine("// Vertex Program");
            Output.AppendLine("// Uniforms");
            GenerateVertexProgramUniforms();
            Output.AppendLine("");
            Output.AppendLine("// Varyings");
            GenerateVaryings();
            Output.AppendLine("");
            Output.AppendLine("// Attributes");
            GenerateVertexProgramAttributes();
            Output.AppendLine("");
            Output.AppendLine("// Variables");
            GenerateVertexProgramVariables();
            Output.AppendLine("");
            Output.AppendLine("// Functions");
            GenerateVertexProgramFunctions();
            Output.AppendLine("");
            Output.AppendLine("// Main");
            GenerateVertexProgramMain();
            Output.AppendLine("");
            Output.AppendLine("// End Vertex Program");

            return Output.ToString();
        }

        protected void GenerateFragmentProgramUniforms()
        {
            if (Config.DiffuseMap)
            {
                Output.AppendLine("uniform sampler2D DiffuseMap;");
            }
            if (Config.BumpMap)
            {
                Output.AppendLine("uniform sampler2D BumpMap;");
            }
            if (Config.SpecularMap)
            {
                Output.AppendLine("uniform sampler2D SpecularMap;");
            }
            if (Config.EmissiveMap)
            {
                Output.AppendLine("uniform sampler2D EmissiveMap;");
            }
            if (Config.ReflectionMap)
            {
                Output.AppendLine("uniform sampler2D ReflectionMap;");
            }
            if (Config.SphereMap)
            {
                Output.AppendLine("uniform sampler2D EnvironmentMap;");
            }
        }

        protected void GenerateVertexProgramUniforms()
        {
        }

        protected string[] GetLightTerms()
        {
            List<string> terms = new List<string>();
            if (Config.DiffuseMap)
            {
                terms.Add("(gl_LightSource[i].diffuse * Diffuse * NdotL(Normal,ToLight[i]))");
            }
            if (Config.SpecularMap)
            {
                terms.Add("gl_LightSource[i].specular * Specular * pow(max(dot(Reflected, Eye), 0.0), gl_FrontMaterial.shininess);");
            }
            return terms.ToArray();
        }

        protected void GenerateLightFunction()
        {
            string[] terms = GetLightTerms();
            Output.AppendLine("vec4 Light(int i)");
            Output.AppendLine("{");

            if (terms.Length == 0)
            {
                Output.AppendLine("return vec4(0.0,0.0,0.0,1.0);");
            }
            else
            {
                if (Config.SpecularMap)
                {
                    Output.AppendLine("vec3 Reflected = normalize( 2.0 * dot(Normal , ToLight[i]) *  Normal - ToLight[i]);");
                }

                Output.AppendLine("vec4 color=" + terms[0] + ";");
                for (int i = 1; i < terms.Length; ++i)
                    Output.AppendLine("color+=" + terms[i] + ";");

                if (Config.LightRangeAttenuation)
                {
                    Output.AppendLine("float range = gl_LightSource[i].quadraticAttenuation;");
                    Output.AppendLine("if (range > 0.0)");
                    Output.AppendLine("{");
                    Output.AppendLine("float dist=length(ToLight[i]);");
                    Output.AppendLine("color *= max(0.0,range - dist) / range;");
                    Output.AppendLine("}");
                }
                Output.AppendLine("color.a=1.0;");
                Output.AppendLine("return color;");
            }

            Output.AppendLine("}");
        }

        protected void GenerateFragmentProgramVariables()
        {
            if (Config.DiffuseMap)
            {
                Output.AppendLine("vec4 Diffuse;");
            }

            if (Config.BumpMap)
            {
                Output.AppendLine("vec3 Normal;");
            }

            if (Config.SpecularMap)
            {
                Output.AppendLine("vec4 Specular;");
                Output.AppendLine("vec3 Eye;");
            }

            if (Config.ReflectionMap)
            {
                Output.AppendLine("vec4 Reflection;");
            }
        }

        protected void GenerateFragmentProgramAttributes()
        {
        }

        protected void GenerateVertexProgramAttributes()
        {
            if (Config.TangentSpace)
            {
                Output.AppendLine("attribute vec3 tangent;");
                Output.AppendLine("attribute vec3 binormal;");
            }
        }

        protected void GenerateVertexProgramVariables()
        {
        }

        protected void GenerateFragmentProgramFunctions()
        {
            GenerateNdotLFunction();
            GenerateLightFunction();
            if (Config.SphereMap)
                GenerateSphereMapFunction();
        }
        protected void GenerateVertexProgramFunctions()
        {
        }

        protected void GenerateNdotLFunction()
        {
            Output.AppendLine("float NdotL(vec3 normal,vec3 light)");
            Output.AppendLine("{");
            Output.AppendLine("return max(0.0, dot(normal,normalize(light)));");
            Output.AppendLine("}");

        }

        protected void GenerateSphereMapFunction()
        {
            Output.AppendLine("vec2 SphereMap(const in vec3 U,const in vec3 N)");
            Output.AppendLine("{");
	        Output.AppendLine("vec3 R;");
	        Output.AppendLine("R = reflect(U,N);");
	        Output.AppendLine("R.z += 1.0;");
	        Output.AppendLine("R = normalize(R);");
	        Output.AppendLine("return R.xy*0.5+0.5;");
            Output.AppendLine("}");
        }

        protected void GenerateVaryings()
        {
            if (Config.BumpMap)
            {
            }
            else
            {
                Output.AppendLine("varying vec3 Normal;");
            }

            if (Config.SpecularMap||Config.SphereMap)
            {
                Output.AppendLine("varying vec3 VertexPos;");
                if (Config.TangentSpace)
                {
                    Output.AppendLine("varying vec3 VertexPosTangentSpace;");
                }

            }

            if (Config.LightCount > 0)
            {
                Output.AppendLine("varying vec3 ToLight[" + Config.LightCount + "];");
            }
        }

        protected void GenerateFragmentProgramMain()
        {
            Output.AppendLine("void main()");
            Output.AppendLine("{");
            Output.AppendLine("int i;");

            if (Config.EmissiveMap)
            {
                Output.AppendLine("vec4 color=texture2D(EmissiveMap, gl_TexCoord[0].xy);");
            }
            else
            {
                Output.AppendLine("vec4 color;");
            }



            if (Config.ReflectionMap)
            {
                Output.AppendLine("Reflection=texture2D(ReflectionMap, gl_TexCoord[0].xy);");
            }

            if (Config.DiffuseMap)
            {
                Output.AppendLine("Diffuse=texture2D(DiffuseMap, gl_TexCoord[0].xy);");
            }
            if (Config.BumpMap)
            {
                Output.AppendLine("Normal = (texture2D(BumpMap, gl_TexCoord[0].xy).xyz -0.5)*2.0;");
            }


            if (Config.SpecularMap)
            {
                Output.AppendLine("Specular = texture2D(SpecularMap, gl_TexCoord[0].xy);");
                Output.AppendLine("Eye = normalize(-VertexPosTangentSpace);");
            }

            if (Config.SphereMap)
            {
                if (Config.TangentSpace)
                {
                    Output.AppendLine("color+=texture2D(EnvironmentMap, SphereMap(normalize(VertexPosTangentSpace), normalize(gl_NormalMatrix * Normal)))"
                         + (Config.ReflectionMap ? "*Reflection" : "")
                        + ";");               }
                else
                {
                    Output.AppendLine("color+=texture2D(EnvironmentMap, SphereMap(normalize(VertexPos), normalize(Normal)))"
                        + (Config.ReflectionMap ? "*Reflection" : "")
                        + ";");
                }
            }

            Output.AppendLine("for(i=0;i<" + Config.LightCount + ";++i)");
            Output.AppendLine("{");
            Output.AppendLine("color+=Light(i);");
            Output.AppendLine("}");
            Output.AppendLine("color.a=1.0;");
            Output.AppendLine("gl_FragColor=color;");
            Output.AppendLine("}");

        }
        protected void GenerateVertexProgramMain()
        {
            Output.AppendLine("void main()");
            Output.AppendLine("{");
            Output.AppendLine("gl_TexCoord[0] = gl_MultiTexCoord0;");
            Output.AppendLine("vec4 vertexPos = gl_ModelViewMatrix * gl_Vertex;");

            if (Config.TangentSpace)
            {
                Output.AppendLine("vec3 n = normalize(gl_NormalMatrix * gl_Normal);");
                Output.AppendLine("vec3 t = normalize(gl_NormalMatrix * tangent);");
                Output.AppendLine("vec3 b = normalize(gl_NormalMatrix * binormal);");
                Output.AppendLine("mat3 tbn = mat3(t, b, n);");
                //Output.AppendLine("mat3 tbn = mat3(n, b, t);");
            }

            if (Config.SpecularMap||Config.SphereMap)
            {
                Output.AppendLine("VertexPos=vertexPos.xyz;");
                if (Config.TangentSpace)
                {
                    Output.AppendLine("VertexPosTangentSpace=VertexPos*tbn;");
                }
            }

            if (!Config.BumpMap)
            {
                Output.AppendLine("Normal=gl_NormalMatrix * gl_Normal;");
            }

            if (Config.LightCount > 0)
            {
                Output.AppendLine("int i;");
                Output.AppendLine("for (i = 0; i < " + Config.LightCount + "; ++i)");
                Output.AppendLine("{");
                if (Config.TangentSpace)
                {
                    Output.AppendLine("ToLight[i] = (gl_LightSource[i].position - vertexPos).xyz * tbn;");
                }
                else
                {
                    Output.AppendLine("ToLight[i] = (gl_LightSource[i].position - vertexPos).xyz;");
                }
                Output.AppendLine("}");
            }

            Output.AppendLine("gl_Position=ftransform();");
            Output.AppendLine("}");

        }

        public int GetId(ShaderConfig cfg)
        {
            return
                (cfg.LightCount & ((1 << 5) - 1)) |
                ((cfg.LightRangeAttenuation ? 1 : 0) << 5) |
                ((cfg.DiffuseMap ? 1 : 0) << 6) |
                ((cfg.TangentSpace ? 1 : 0) << 7) |
                ((cfg.BumpMap ? 1 : 0) << 8) |
                ((cfg.SpecularMap ? 1 : 0) << 9) |
                ((cfg.EmissiveMap ? 1 : 0) << 10) |
                ((cfg.ReflectionMap ? 1 : 0) << 11) |
                ((cfg.SphereMap ? 1 : 0) << 12);
        }

        ShaderConfig Config;

        StringBuilder Output; 
    }

    public class ShaderConfig
    {
        public int LightCount = 8;
        public bool LightRangeAttenuation = true;

        public bool DiffuseMap = true;
        public bool TangentSpace = true;
        public bool BumpMap = true;
        public bool SpecularMap = false;
        public bool EmissiveMap = false;

        public bool ReflectionMap = false;
        public bool SphereMap = false;
    }

	public class OpenGL : IRenderer
	{
		protected class TextureId : Cheetah.TextureId
		{
			public TextureId(int _id, OpenGL _gl, int _w, int _h, bool _a, bool _cube)
			{
				id = _id;
				gl = _gl;
				w = _w;
				h = _h;
				a = _a;
                cube = _cube;
			}

			public int id;
			public OpenGL gl;
			public int w, h;
			public bool a;
            public bool cube;
		}

        public void UseShader(Cheetah.Graphics.Shader s)
        {
            CheckError();
            if (s != null)
            {
                CurrentShader = ((Shader)s);
                int id = CurrentShader.ProgramId;

                GL.UseProgram(id);
            }
            else
            {
                CurrentShader = null;
                GL.UseProgram(0);
            }
            CheckError();

        }

        Shader CurrentShader;

        public void SetUniform(int location, float[] values)
        {
            CheckError();
            switch (values.Length)
            {
                case 1:
                    GL.Uniform1(location, values[0]);
                    break;
                case 2:
                    GL.Uniform2(location, values[0], values[1]);
                    break;
                case 3:
                    GL.Uniform3(location, values[0], values[1], values[2]);
                    break;
                case 4:
                    GL.Uniform4(location, values[0], values[1], values[2], values[3]);
                    //GL.Uniform4fvARB(location, 4, values);
                    break;
                case 16:
                    GL.UniformMatrix4(location, 1, false, values);
                    break;
                default:
                    throw new Exception();
            }
        }

        public void SetUniform(int location, int[] values)
        {
            CheckError();
            switch (values.Length)
            {
                case 1:
                    GL.Uniform1(location, values[0]);
                    break;
                case 2:
                    GL.Uniform2(location, values[0], values[1]);
                    break;
                case 3:
                    GL.Uniform3(location, values[0], values[1], values[2]);
                    break;
                case 4:
                    GL.Uniform4(location, values[0], values[1], values[2], values[3]);
                    //GL.Uniform4fvARB(location, 4, values);
                    break;
                //case 16:
                //    GL.UniformMatrix4fvARB(location, 1, false, values);
                //    break;
                default:
                    throw new Exception();
            }
        }
        public void SetAttribute(int location, float[] values)
        {
            CheckError();
            switch (values.Length)
            {
                case 1:
                    GL.VertexAttrib1(location, values[0]);
                    break;
                case 2:
                    GL.VertexAttrib2(location, values[0], values[1]);
                    break;
                case 3:
                    GL.VertexAttrib3(location, values[0], values[1], values[2]);
                    break;
                case 4:
                    GL.VertexAttrib4(location, values[0], values[1], values[2], values[3]);
                    //GL.Uniform4fvARB(location, 4, values);
                    break;
                case 16:
                    //GL.VertexAttribArrayObjectATI(location, 1, false, values);
                    break;
                default:
                    throw new Exception();
            }
        }

        public void SetAttributeArray(int location, int offset, int stride, object array)
        {
        }

        bool fbo_disabled = false;

        public Cheetah.RenderTarget CreateRenderTarget(Cheetah.TextureId texture, Cheetah.TextureId depth)
        {
            if (texture == null && depth == null)
            {
                if (fbo_disabled)
                    throw new Exception();
                return null;
            }
            CheckError();

            int index;
            int[] tmp = new int[1];
            GL.GenFramebuffers(1, tmp);
            index = tmp[0];
            if (index <= 0)
                throw new Exception("cant create framebuffer.");

            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, index);

            //GL.FramebufferTexture2DEXT(GL._FRAMEBUFFER_EXT, GL._DEPTH_ATTACHMENT_EXT, TextureTarget.Texture2D, d, 0);
            if (depth != null)
            {
                GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt,TextureTarget.Texture2D, ((TextureId)depth).id, 0);
            }

            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, ((TextureId)texture).id, 0);


            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);
            if (status == FramebufferErrorCode.FramebufferCompleteExt)
            {
            }
            else
            {
                throw new Exception("framebuffer check failed.");
            }

            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);

            CheckError();
            return new RenderTarget(index, (TextureId)texture);
        }

        public void BindRenderTarget(Cheetah.RenderTarget target)
        {
            CheckError();
            if (target != null)
            {
                currentwidth=((TextureId)target.Texture).w;
                currentheight = ((TextureId)target.Texture).h;
                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, ((RenderTarget)target).id);
            }
            else
            {
                currentwidth = width;
                currentheight = height;
                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            }
            CheckError();
        }

        protected int GlCreateShader(string code, ShaderType type)
        {
            CheckError();
            int id = GL.CreateShader(type);

            int ok;
            GL.ShaderSource(id, code);
            GL.CompileShader(id);
            GL.GetShader(id, ShaderParameter.CompileStatus, out ok);

            if (ok != 1)
            {
                string log=GL.GetShaderInfoLog(id);

                System.Console.WriteLine(log);
                switch (type)
                {
                    case ShaderType.VertexShader:
                        throw new Exception("cant compile vertexshader");
                    case ShaderType.FragmentShader:
                        throw new Exception("cant compile fragmentshader");
                    case ShaderType.GeometryShader:
                        throw new Exception("cant compile geometryshader");
                }
            }
            CheckError();
            return id;
        }

        public Cheetah.Graphics.Shader CreateShader(string vertex, string fragment, string geometry, PrimitiveType input, PrimitiveType output)
        {
            CheckError();
            int vertexid = 0;
            if (vertex != null)
            {
                vertexid = GlCreateShader(vertex, ShaderType.VertexShader);
            }
            
            int fragmentid = 0;
            if (fragment != null)
            {
                fragmentid = GlCreateShader(fragment, ShaderType.FragmentShader);
            }
            int geometryid = 0;
            if (geometry != null && GeometryShadersSupported)
            {
                geometryid = GlCreateShader(geometry, ShaderType.GeometryShader);
            }

            if (vertexid == 0 && fragmentid == 0)
                throw new Exception("no vertex/fragment shader");
            if (vertexid == 0 && geometryid != 0)
                throw new Exception("no vertex shader but geometry shader?!");

            int p = GlCreateProgram(vertexid, fragmentid, geometryid, input, output);

            CheckError();
            return new Shader(vertex, fragment, vertexid, fragmentid, p, geometry, geometryid);
        }

        public Cheetah.Graphics.Shader CreateShader(string vertex, string fragment)
        {
            return CreateShader(vertex, fragment, null, (PrimitiveType)0,(PrimitiveType)0);
        }

        protected int GlCreateProgram(int vertex, int fragment, int geometry, PrimitiveType input, PrimitiveType output)
        {
            CheckError();
            int[] l3 = new int[1];

            int ok;

            int p = GL.CreateProgram();

            if (vertex != 0)
                GL.AttachShader(p, vertex);
            if (fragment != 0)
                GL.AttachShader(p, fragment);

            if (geometry != 0)
            {
                GL.AttachShader(p, geometry);
                GlSetProgramPrimitiveType(p, input, output);
                //GL.GetIntegerv(GL._MAX_GEOMETRY_OUTPUT_VERTICES_EXT, l3);

                //GL.ProgramParameteriEXT(p, GL._GEOMETRY_VERTICES_OUT_EXT, l3[0]);

            }

            GL.LinkProgram(p);
            GL.GetProgram(p, ProgramParameter.LinkStatus, out ok);
            if (ok != 1)
            {
                string log = GL.GetProgramInfoLog(p);

                //string log = OpenTK.Graphics.GL.GetProgramInfoLog(p);

                System.Console.WriteLine(log);
                throw new Exception("cant link program");
            }


            CheckError();
            return p;
        }

        protected BeginMode GlPrimitiveInputType(PrimitiveType type)
        {
            BeginMode t;
            switch (type)
            {
                case PrimitiveType.POINTS:
                    t = BeginMode.Points;
                    break;
                case PrimitiveType.LINES:
                    t = BeginMode.Lines;
                    break;
                case PrimitiveType.TRIANGLES:
                    t = BeginMode.Triangles;
                    break;
                case PrimitiveType.TRIANGLESTRIP:
                    t = BeginMode.TriangleStrip;
                    break;
                case PrimitiveType.LINESTRIP:
                    t = BeginMode.LineStrip;
                    break;
                default:
                    throw new Exception("wrong input primitive type: " + type.ToString());
            }
            return t;
        }
        protected BeginMode GlPrimitiveOutputType(PrimitiveType type)
        {
            BeginMode t;
            switch (type)
            {
                case PrimitiveType.POINTS:
                    t = BeginMode.Points;
                    break;
                case PrimitiveType.TRIANGLESTRIP:
                    t = BeginMode.TriangleStrip;
                    break;
                case PrimitiveType.LINESTRIP:
                    t = BeginMode.LineStrip;
                    break;

                //HACK
                /*case PrimitiveType.LINES:
                    t = GL._LINES;
                    break;
                case PrimitiveType.TRIANGLES:
                    t = GL._TRIANGLES;
                    break;*/

                default:
                    throw new Exception("wrong output primitive type: " + type.ToString());
            }
            return t;
        }
        protected void GlSetProgramPrimitiveType(int p, PrimitiveType input, PrimitiveType output)
        {
            CheckError();
            GL.ProgramParameter(p,AssemblyProgramParameterArb.GeometryInputType, (int)GlPrimitiveInputType(input));
            GL.ProgramParameter(p, AssemblyProgramParameterArb.GeometryOutputType, (int)GlPrimitiveOutputType(output));
            CheckError();
        }

        public void FreeShader(Cheetah.Graphics.Shader s)
        {
        }

        protected class RenderTarget : Cheetah.RenderTarget
        {
            public RenderTarget(int _id, TextureId tex)
            {
                id = _id;
                Texture = tex;
            }

            public int id;
        }

        protected class Shader : Cheetah.Graphics.Shader
        {
            public Shader(string vertex, string fragment, int vertexid, int fragmentid, int p, string geometry, int geometryid)
            {
                VertexSource = vertex;
                FragmentSource = fragment;
                VertexId = vertexid;
                FragmentId = fragmentid;
                ProgramId = p;
                GeometrySource = geometry;
                GeometryId = geometryid;
            }

            public string VertexSource;
            public string FragmentSource;
            public string GeometrySource;
            public int VertexId;
            public int FragmentId;
            public int ProgramId;
            public int GeometryId;

            public Dictionary<string, int> UniformLocations = new Dictionary<string, int>();
            public Dictionary<string, int> AttributeLocations = new Dictionary<string, int>();

            public override int GetUniformLocation(string name)
            {
                try
                {
                    return UniformLocations[name];
                }
                catch (KeyNotFoundException)
                {
                    int i = GL.GetUniformLocation(ProgramId, name);
                    UniformLocations[name] = i;
                    return i;
                }
                
            }

            public override int GetAttributeLocation(string name)
            {
                try
                {
                    return AttributeLocations[name];
                }
                catch (KeyNotFoundException)
                {
                    int i=GL.GetAttribLocation(ProgramId, name);
                    AttributeLocations[name] = i;
                    return i;
                }
            }
        }
		public class Exception : System.Exception
		{
			public Exception(string text)
				: base(text)
			{
			}
			public Exception()
			{
			}
		}

        protected class VertexBuffer : Cheetah.Graphics.VertexBuffer
		{
			public int id;
			/*~VertexBuffer()
			{
				Dispose();
			}*/

			void Dispose()
			{
			}
		}
        protected class SlowVertexBuffer : Cheetah.Graphics.VertexBuffer
		{
			/*public unsafe void Lock(LockDelegate lockfunc,object context)
				{
				fixed(byte* b=data)
				{
					lockfunc(b,this,context);
					//int i=sizeof(VF_Pos3f_Normal3f_Tex2f);
				}
			}*/

			public object data;
		}

        protected class DynamicVertexBuffer : Cheetah.Graphics.DynamicVertexBuffer
		{
			public DynamicVertexBuffer(OpenGL _gl)
			{
				gl = _gl;
			}

			public override IntPtr Lock()
			{
				GL.BindBuffer(BufferTarget.ArrayBuffer, id);
                return GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
			}

			public override void Unlock()
			{
                GL.BindBuffer(BufferTarget.ArrayBuffer, id);
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			}

			OpenGL gl;
			public int id;
		}

        protected class State
        {
            public void Enable(int state)
            {
                bool v;
                if (boolstate.TryGetValue(state, out v))
                {
                    if (!v)
                    {
                        GL.Enable((EnableCap)state);
                        boolstate[state] = true;
                    }
                }
                else
                {
                    GL.Enable((EnableCap)state);
                    boolstate[state] = true;
                }
            }
            public void Disable(int state)
            {
                bool v;
                if (boolstate.TryGetValue(state, out v))
                {
                    if (v)
                    {
                        GL.Disable((EnableCap)state);
                        boolstate[state] = false;
                    }
                }
                else
                {
                    GL.Disable((EnableCap)state);
                    boolstate[state] = false;
                }
            }
                  
            
             public void EnableVertexAttribArray(int state)
            {
                bool v;
                if (boolstate.TryGetValue(state, out v))
                {
                    if (!v)
                    {
                        GL.EnableVertexAttribArray(state);
                        boolstate[state] = true;
                    }
                }
                else
                {
                    GL.EnableVertexAttribArray(state);
                    boolstate[state] = true;
                }
            }
            public void DisableVertexAttribArray(int state)
            {
                bool v;
                if (boolstate.TryGetValue(state, out v))
                {
                    if (v)
                    {
                        GL.DisableVertexAttribArray(state);
                        boolstate[state] = false;
                    }
                }
                else
                {
                    GL.DisableVertexAttribArray(state);
                    boolstate[state] = false;
                }
            }

            
             public void EnableClientState(int state)
            {
                bool v;
                if (boolstate.TryGetValue(state, out v))
                {
                    if (!v)
                    {
                        GL.EnableClientState((ArrayCap)state);
                        boolstate[state] = true;
                    }
                }
                else
                {
                    GL.EnableClientState((ArrayCap)state);
                    boolstate[state] = true;
                }
            }
            public void DisableClientState(int state)
            {
                bool v;
                if (boolstate.TryGetValue(state, out v))
                {
                    if (v)
                    {
                        GL.DisableClientState((ArrayCap)state);
                        boolstate[state] = false;
                    }
                }
                else
                {
                    GL.DisableClientState((ArrayCap)state);
                    boolstate[state] = false;
                }
            }

            public void ActiveTexture(int state)
            {
                if (state != activetexture)
                {
                    GL.ActiveTexture((TextureUnit)state);
                    activetexture = state;
                }
            }
            public void ClientActiveTexture(int state)
            {
                if (state != clientactivetexture)
                {
                    GL.ClientActiveTexture((TextureUnit)state);
                    clientactivetexture = state;
                }
            }

            int activetexture = -1;
            int clientactivetexture = -1;

            Dictionary<int,bool> boolstate=new Dictionary<int,bool>();
        }

        protected class SlowDynamicVertexBuffer : Cheetah.Graphics.DynamicVertexBuffer
		{
			public SlowDynamicVertexBuffer(OpenGL _gl)
			{
				gl = _gl;
			}

			public override IntPtr Lock()
			{
				Array a = (Array)data;
				return Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);
			}

			public override void Unlock()
			{
			}

			public override void Update(object newdata, int size)
			{
				data = newdata;
			}

			OpenGL gl;
			public object data;

		}

		public class VertexProgram
		{
			public int id;
		}

		public class FragmentProgram
		{
			public int id;
		}

		public void PushMatrix()
		{
            GL.PushMatrix();
		}

		public void PopMatrix()
		{
			GL.PopMatrix();
		}

		public IEffect CreateEffect(string code)
		{
			//CgEffect effect = new CgGlEffect(code);
			//return effect;
            return null;
		}

		public void FreeEffect(IEffect e)
		{
			e.Dispose();
		}


        public Point WindowSize
        {
            get { return new Point(width, height); }
            set { width = value.X; height = value.Y; }
        }
        public Point RenderSize
        {
            get { return new Point(currentwidth, currentheight); }
            set { currentwidth = value.X; currentheight = value.Y; }
        }

		public Point Size
		{
			get { return new Point(width, height); }
		}

		public void LoadMatrix(Matrix4 m)
		{
            foreach (float f in Matrix4Extensions.ToFloats(m))
                if (float.IsNaN(f))
                    throw new Exception("NaN");
			GL.LoadMatrix(ref m);
		}

		public void MultMatrix(Matrix4 m)
		{
            foreach (float f in Matrix4Extensions.ToFloats(m))
                if (float.IsNaN(f))
                    throw new Exception("NaN");
            GL.MultMatrix(ref m);
		}

		public void GetMatrix(float[] modelview, float[] projection)
		{
			if (projection != null)
				GL.GetFloat(GetPName.ProjectionMatrix, projection);
			if (modelview != null)
				GL.GetFloat(GetPName.ModelviewMatrix, modelview);
		}

        public Vector3 UnProject(float[] winxyz, float[] model, float[] proj, int[] viewport)
        {
            global::OpenTK.Matrix4d projection;
            global::OpenTK.Matrix4d modelview;
            int[] _viewport = new int[4];

            if (model != null && proj != null && viewport != null)
            {
                throw new Exception("BUG");
            }
            else
            {
                GL.GetDouble(GetPName.ProjectionMatrix, out projection);
                GL.GetDouble(GetPName.ModelviewMatrix, out modelview);
                GL.GetInteger(GetPName.Viewport, _viewport);
            }
            global::OpenTK.Vector3d obj=new global::OpenTK.Vector3d();

            Imgui.Glu.UnProject(new global::OpenTK.Vector3d((double)winxyz[0], (double)winxyz[1], (double)winxyz[2]), modelview, projection, _viewport, ref obj);

            if (double.IsNaN(obj.X))
                throw new Exception("NaN");

            return new Vector3((float)obj.X,(float)obj.Y,(float)obj.Z);
        }

        public float[] GetRasterPosition(float[] pos3d)
		{
            global::OpenTK.Matrix4d projection;
            global::OpenTK.Matrix4d modelview;
            int[] viewport = new int[4];

            GL.GetDouble(GetPName.ProjectionMatrix, out projection);
            GL.GetDouble(GetPName.ModelviewMatrix, out modelview);
            GL.GetInteger(GetPName.Viewport, viewport);

            global::OpenTK.Vector3d win=new global::OpenTK.Vector3d();
            Imgui.Glu.Project(new global::OpenTK.Vector3d((double)pos3d[0], (double)pos3d[1], (double)pos3d[2]), modelview, projection, viewport, ref win);

			return new float[] { (float)win.X, (float)win.Y, (float)win.Z };
		}

        public void Draw(string text, float x, float y, float sx, float sy, Cheetah.Texture t, Color4f color, float width, RectangleF scissor)
        {
            int c = 16;
            float f = 1 / (float)c;
            BindTexture(t.Id);

            //if (CompabilityMode)
            //{
            //    GL.PushMatrix();
            //    GL.LoadIdentity();
            //    GL.MatrixMode(GL._PROJECTION);
            //    GL.PushMatrix();
            //    GL.LoadIdentity();
            //    GL.Ortho(0, 1, 1, 0, -1, 10);
            //    GL.Color4f(color.r, color.g, color.b, color.a);

            //    for (int i = 0; i < text.Length; ++i)
            //    {
            //        int a = text[i];
            //        float xf = a % c, yf = a / c;

            //        GL.Begin(GL._QUADS);

            //        GL.TexCoord2f(xf * f, yf * f);
            //        GL.Vertex2f(x + (float)i * sx, y);

            //        GL.TexCoord2f(xf * f + f, yf * f);
            //        GL.Vertex2f(x + sx + (float)i * sx, y);

            //        GL.TexCoord2f(xf * f + f, yf * f + f);
            //        GL.Vertex2f(sx + x + (float)i * sx, sy + y);

            //        GL.TexCoord2f(xf * f, yf * f + f);
            //        GL.Vertex2f(x + (float)i * sx, y + sy);


            //        GL.End();
            //    }
            //    GL.PopMatrix();
            //    GL.MatrixMode(GL._MODELVIEW);
            //    GL.PopMatrix();
            //}
            //else
            {
                UseShader(TextShader);

                int fontsizeindex = TextShader.GetUniformLocation("FontSize");
                int colorindex = TextShader.GetUniformLocation("Color");
                int charsizeindex = TextShader.GetUniformLocation("CharSize");
                int textureindex = TextShader.GetUniformLocation("Texture");
                int scissorindex = TextShader.GetUniformLocation("Scissor");

                SetUniform(fontsizeindex, new float[] { sx, sy });
                SetUniform(colorindex, new float[] { color.r, color.g, color.b, color.a });
                SetUniform(charsizeindex, new float[] { f });
                if(scissorindex>=0)
                    SetUniform(scissorindex, new float[] { scissor.Left,scissor.Top,scissor.Right,scissor.Bottom });

                int charposindex = TextShader.GetAttributeLocation("CharPos");
                int posindex = TextShader.GetAttributeLocation("Pos");

                for (int i = 0; i < text.Length; ++i)
                {
                    int a = text[i];
                    float xf = a % c, yf = a / c;

                    SetAttribute(charposindex, new float[] { xf, yf });
                    SetAttribute(posindex, new float[] { x + ((float)i) * width, y });

                    GL.Begin(BeginMode.Quads);
                    GL.Vertex2(0, 0);
                    GL.Vertex2(1, 0);
                    GL.Vertex2(1, 1);
                    GL.Vertex2(0, 1);
                    GL.End();
                }

                UseShader(null);
            }


            BindTexture(null);
        }
		public void Draw(string text, float x, float y, float sx, float sy, Cheetah.Texture t,Color4f color,float width)
		{
            Draw(text, x, y, sx, sy, t, color, width, new RectangleF(0, 0, 1, 1));
		}

        bool LoadExtension(string name)
        {
            if (!GL.GetString(StringName.Extensions).Contains(name))
            {
                System.Console.WriteLine("extension missing: " + name);
                return false;
            }
            else
                return true;
        }

        bool can_generate_mipmaps = false;
		bool supports_compressed_textures = false;
		
		public OpenGL(int _width, int _height)
		{
            Root.Instance.UserInterface.Renderer = this;

            System.Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Version));
            System.Console.WriteLine("OpenGL Vendor: " + GL.GetString(StringName.Vendor));
            System.Console.WriteLine("OpenGL Renderer: " + GL.GetString(StringName.Renderer));

            CheckError();
            LoadExtension("GL_ARB_vertex_program");
            LoadExtension("GL_ARB_point_sprite");
            LoadExtension("GL_ARB_point_parameters");
            LoadExtension("GL_ARB_fragment_program");
            LoadExtension("GL_ARB_multitexture");
            LoadExtension("GL_ARB_vertex_shader");
            LoadExtension("GL_ARB_shader_objects");
            LoadExtension("GL_ARB_fragment_shader");
            LoadExtension("GL_VERSION_2_0");
            LoadExtension("GL_ARB_vertex_buffer_object");
            CheckError();

            bool b = LoadExtension("GL_ARB_framebuffer_object");
            fbo_disabled |= !b;
            can_generate_mipmaps = b;
            //fbo_disabled |= !LoadExtension("GL_EXT_framebuffer_object");
            LoadExtension("GL_ARB_texture_cube_map");
            supports_compressed_textures = LoadExtension("GL_EXT_texture_compression_s3tc");
            LoadExtension("GL_ARB_texture_compression");
            CheckError();

            if (LoadExtension("GL_EXT_geometry_shader4"))
            {
                System.Console.WriteLine("OpenGL: Geometry Shaders supported!");
                GeometryShadersSupported = false;
            }
            LoadExtension("GL_EXT_gpu_shader4");
            LoadExtension("GL_EXT_bindable_uniform");
            fbo_disabled |= !LoadExtension("GL_ARB_texture_non_power_of_two");

			width = currentwidth=_width;
			height = currentheight=_height;
			//video=SDL.Instance.Video;
            States.Enable((int)GetPName.Blend);
			States.Enable((int)GetPName.Texture2D);
			States.Enable((int)GetPName.DepthTest);
			GL.ShadeModel(ShadingModel.Smooth);
			GL.CullFace(CullFaceMode.Back);
			States.Enable((int)GetPName.CullFace);
			//States.Disable(GL._CULL_FACE);
			GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0, 0, 0, 1 });
            CheckError();


            string path = "shaders/glsl/";
            Console.WriteLine("creating shaders...");

            TextShader = (Shader)Root.Instance.ResourceManager.Load(path + "text.shader", typeof(Cheetah.Graphics.Shader));
            CheckError();
            pointsprite = (Shader)Root.Instance.ResourceManager.Load(path + "pointsprite.shader", typeof(Cheetah.Graphics.Shader));
            CheckError();


            //States.Enable((int)GetPName.PointSize);
            States.Enable((int)GetPName.ProgramPointSize);
            CheckError();

        }
        Shader pointsprite;

		public void SetCamera(Camera c)
		{
            cam = c;

            if (c == null)
                return;
            CheckError();

			GL.MatrixMode(MatrixMode.Projection);
            Matrix4 m3 = c.GetProjectionMatrix();
            GL.LoadMatrix(ref m3);
            GL.MatrixMode(MatrixMode.Modelview);

			Matrix4 m = c.Matrix;//Matrix4Extensions.FromQuaternion(c.Orientation);

			Vector3 t = new Vector3();
			Vector3 x, y;
            Vector3 pos = Matrix4Extensions.ExtractTranslation(m);
            Matrix4Extensions.ExtractBasis(m, out x, out y, out t);
			t += pos;


            if (c.Shake > 0)
            {
                t += c.Shake * VecRandom.Instance.NextUnitVector3();
                pos += c.Shake * VecRandom.Instance.NextUnitVector3();
            }

            global::OpenTK.Matrix4 m2=global::OpenTK.Matrix4.LookAt(pos.X, pos.Y, pos.Z, t.X, t.Y, t.Z, y.X, y.Y, y.Z);
            GL.LoadMatrix(ref m2);

            Viewport vp;
            if(c.View!=null)
            {
                vp = c.View;
            }
            else
            {
                //vp = new Viewport(0,0,Size.X,Size.Y);
                vp = new Viewport(0, 0, currentwidth, currentheight);
            }

            States.Enable((int)GetPName.ScissorTest);
            GL.Scissor(vp.X, vp.Y, vp.W, vp.H);
            GL.Viewport(vp.X, vp.Y, vp.W, vp.H);
            CheckError();

		}

		public unsafe Bitmap Screenshot()
		{
			byte[] rgb = new byte[Size.X * Size.Y * 3];
			GL.ReadPixels(0, 0, Size.X, Size.Y, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, rgb);
			//return new Image(Size.X, Size.Y, rgb, false);

            fixed(void *ptr=rgb)
            {
                Bitmap b= new Bitmap(Size.X, Size.Y, 3 * Size.X, System.Drawing.Imaging.PixelFormat.Format24bppRgb, new IntPtr(ptr));
                
                return b;
            }
		}
        public Cheetah.Graphics.Image Screenshot2()
        {
            byte[] rgb = new byte[Size.X * Size.Y * 3];
            GL.ReadPixels(0, 0, Size.X, Size.Y, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, rgb);
            return new Cheetah.Graphics.Image(Size.X, Size.Y, rgb, false);
        }

		public void UpdateTexture(Cheetah.TextureId t, byte[] rgba)
		{
            CheckError();
            TextureId t1 = (TextureId)t;
			GL.BindTexture(TextureTarget.Texture2D, t1.id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.Three, t1.w, t1.h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, rgba);
            CheckError();
        }

		protected bool IsPowerOf2(int x)
		{
			return ((x & (x - 1)) == 0);
		}

		protected int NextPowerOf2(int v)
		{
			v--;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v++;
			return v;
		}

		protected int CalcGoodSize(int x, int y)
		{
			int a = (int)Math.Sqrt(x * y);
			return NextPowerOf2(a) / 2;
		}

        void CheckError()
        {
            ErrorCode error=GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception("OpenGL error: "+error.ToString());
            }
        }

        public Cheetah.TextureId CreateCompressedCubeTexture(byte[] xpos, byte[] xneg, byte[] ypos, byte[] yneg, byte[] zpos, byte[] zneg, TextureFormat codec, int w, int h)
        {
			if(!supports_compressed_textures)
				throw new Exception("compressed textures not supported.");
			
            PixelInternalFormat format;
            switch (codec)
            {
                case TextureFormat.DXT1:
                    format = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case TextureFormat.DXT2:
                    throw new Exception();
                case TextureFormat.DXT3:
                    format = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case TextureFormat.DXT4:
                    throw new Exception();
                case TextureFormat.DXT5:
                    format = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                default:
                    throw new Exception();
            }

            int[] i = new int[1];
            CheckError();

            GL.GenTextures(1, i);
            CheckError();

            TextureId t = new TextureId(i[0], this, w, h, false, true);

            GL.BindTexture(TextureTarget.TextureCubeMap, t.id);
            CheckError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            CheckError();

            GL.CompressedTexImage2D<byte>(TextureTarget.TextureCubeMapPositiveX, 0, format, w, h, 0, xpos.Length, xpos);
            CheckError();
            GL.CompressedTexImage2D<byte>(TextureTarget.TextureCubeMapNegativeX, 0, format, w, h, 0, xneg.Length, xneg);
            CheckError();
            GL.CompressedTexImage2D<byte>(TextureTarget.TextureCubeMapPositiveY, 0, format, w, h, 0, ypos.Length, ypos);
            CheckError();
            GL.CompressedTexImage2D<byte>(TextureTarget.TextureCubeMapNegativeY, 0, format, w, h, 0, yneg.Length, yneg);
            CheckError();
            GL.CompressedTexImage2D<byte>(TextureTarget.TextureCubeMapPositiveZ, 0, format, w, h, 0, zpos.Length, zpos);
            CheckError();
            GL.CompressedTexImage2D<byte>(TextureTarget.TextureCubeMapNegativeZ, 0, format, w, h, 0, zneg.Length, zneg);
            CheckError();
    
            Textures[t.id] = t;

            return t;
        }

        public Cheetah.TextureId CreateCubeTexture(byte[] xpos,byte[] xneg,byte[] ypos,byte[] yneg,byte[] zpos,byte[] zneg, 
            int w, int h)
        {
            CheckError();
            int[] i = new int[1];

            GL.GenTextures(1, i);

            TextureId t = new TextureId(i[0], this, w, h, false, true);

            GL.BindTexture(TextureTarget.TextureCubeMap, t.id);
            CheckError();

            //GL.TexParameterf(GL._TEXTURE_CUBE_MAP, TextureParameterName.TextureWrapS, GL._REPEAT);
            //GL.TexParameterf(GL._TEXTURE_CUBE_MAP, TextureParameterName.TextureWrapT, GL._REPEAT);

            //GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, GL._CLAMP_TO_EDGE);
            //GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, GL._CLAMP_TO_EDGE);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            CheckError();

            GL.TexImage2D<byte>(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Three, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, xpos);
            CheckError();
            GL.TexImage2D<byte>(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Three, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, xneg);
            CheckError();
            GL.TexImage2D<byte>(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Three, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, ypos);
            CheckError();
            GL.TexImage2D<byte>(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Three, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, yneg);
            CheckError();
            GL.TexImage2D<byte>(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Three, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, zpos);
            CheckError();
            GL.TexImage2D<byte>(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Three, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, zneg);
            
            Textures[t.id] = t;

            CheckError();
            return t;
        }

        public Cheetah.TextureId CreateCompressedTexture(byte[][] mipmaps, TextureFormat codec,int w, int h)
        {
			if(!supports_compressed_textures)
				throw new Exception("compressed textures not supported.");

			CheckError();
            int[] i = new int[1];
            GL.GenTextures(1, i);
            TextureId t = new TextureId(i[0], this, w, h, true, false);
            t.LastBind = Root.Instance.Time;

            GL.BindTexture(TextureTarget.Texture2D, t.id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            if (mipmaps.Length > 1)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            }
            else
            {
                //disable mipmapping
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            }

            PixelInternalFormat format;
            switch (codec)
            {
                case TextureFormat.DXT1:
                    format = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case TextureFormat.DXT2:
                    throw new Exception();
                case TextureFormat.DXT3:
                    format = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case TextureFormat.DXT4:
                    throw new Exception();
                case TextureFormat.DXT5:
                    format = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                default:
                    throw new Exception();
            }

            for (int m = 0; m < mipmaps.Length; ++m)
            {
                GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, m, format, w, h, 0, mipmaps[m].Length, mipmaps[m]);
                w /= 2;
                h /= 2;
            }

            Textures[t.id] = t;

            CheckError();
            return t;
        }

        public Cheetah.TextureId CreateDepthTexture(int w, int h)
        {
            int[] i = new int[1];
            CheckError();
            GL.GenTextures(1, i);
            GL.BindTexture(TextureTarget.Texture2D, i[0]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            TextureId t = new TextureId(i[0], this, w, h, false, false);
            Textures[t.id] = t;
            CheckError();
            return t;
        }

		public Cheetah.TextureId CreateTexture(byte[] rgba, int w, int h, bool alpha)
		{
            CheckError();
            bool mipmap = IsPowerOf2(w) && IsPowerOf2(h);
			//if (!(IsPowerOf2(w) && IsPowerOf2(h)))
			//	throw new Exception("Texture sizes must be n^2.");
			int[] i = new int[1];
			byte[] data = rgba;
			GL.GenTextures(1, i);
			//if(GL.IsTexture(i[0])!=GL._TRUE)
			//	throw new Exception("OpenGL.CreateTexture: glGenTextures failed.");
            CheckError();

			TextureId t = new TextureId(i[0], this, w, h, alpha, false);
            t.LastBind = Root.Instance.Time;

			GL.BindTexture(TextureTarget.Texture2D, t.id);
            CheckError();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            CheckError();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            CheckError();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            CheckError();

            if (mipmap)
            {
                if (!can_generate_mipmaps)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
                    CheckError();
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            }
            else
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            CheckError();

            if (alpha)
            {
                GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data);
                CheckError();
            }
            else
            {
                GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.Three, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, data);
                CheckError();
            }

            if (can_generate_mipmaps && mipmap)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                CheckError();
            }
			//t.Width=w;
			//t.height=h;
			Textures[t.id] = t;

			return t;
		}

        public Cheetah.TextureId CreateTexture(int w, int h, bool alpha, bool depth)
        {
            CheckError();
            bool mipmap = IsPowerOf2(w) && IsPowerOf2(h);
            //if (!(IsPowerOf2(w) && IsPowerOf2(h)))
            //	throw new Exception("Texture sizes must be n^2.");
            int[] i = new int[1];
            //byte[] data = rgba;
            GL.GenTextures(1, i);
            //if(GL.IsTexture(i[0])!=GL._TRUE)
            //	throw new Exception("OpenGL.CreateTexture: glGenTextures failed.");

            TextureId t = new TextureId(i[0], this, w, h, alpha, false);
            t.LastBind = Root.Instance.Time;

            GL.BindTexture(TextureTarget.Texture2D, t.id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            if (mipmap)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            else
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            if (depth)
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, w, h, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.UnsignedShort, IntPtr.Zero);
            else
                //GL.TexImage2D(TextureTarget.Texture2D, 0, GL._COLOR, w, h, 0, GL._DEPTH_COMPONENT, GL._UNSIGNED_SHORT, IntPtr.Zero);
                throw new Exception();

            //t.Width=w;
            //t.height=h;
            Textures[t.id] = t;
            CheckError();

            return t;
        }

		public void BindTexture(Cheetah.TextureId t, int unit)
		{
			unit += (int)TextureUnit.Texture0;
            CheckError();


			if (t != null)
			{
                t.LastBind = Root.Instance.Time;
                TextureId t2 = (TextureId)t;

                States.ActiveTexture(unit);
                //States.Disable(TextureTarget.Texture2D);

                TextureTarget x = t2.cube ? TextureTarget.TextureCubeMap : TextureTarget.Texture2D;
				States.Enable((int)x);
				GL.BindTexture(x, t2.id);
			}
			else
			{
                States.ActiveTexture(unit);
				//GL.BindTexture(TextureTarget.Texture2D,-1);
				States.Disable((int)TextureTarget.Texture2D);
                States.Disable((int)TextureTarget.TextureCubeMap);
            }
            States.ActiveTexture((int)TextureUnit.Texture0);

            CheckError();

		}

		public void BindTexture(Cheetah.TextureId t)
		{
            BindTexture(t, 0);
			/*if (t != null)
			{
                t.LastBind = Root.Instance.Time;
                States.Enable(TextureTarget.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, ((TextureId)t).id);
			}
			else
			{
				GL.BindTexture(TextureTarget.Texture2D, -1);
				States.Disable(TextureTarget.Texture2D);
			}*/
		}
		public void FreeTexture(Cheetah.TextureId t)
		{
			int[] i = new int[] { ((TextureId)t).id };
			GL.DeleteTextures(1, i);
			Textures.Remove(i[0]);
            CheckError();
		}

		public void Clear(float r, float g, float b, float a)
		{
            //GL.Viewport(0, 0, width, height);
            //GL.Scissor(0, 0, width, height);
            GL.ClearColor(r, g, b, a);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            CheckError();
        }

		public void SetMaterial(Material m)
		{
			if (m != null)
            {
            //    if (CompabilityMode)
            //    {
            //        int stage = 0;

            //        States.ActiveTexture(stage + GL._TEXTURE0);

            //        if (m.diffusemap != null)
            //        {
            //            BindTexture(m.diffusemap.Id);
            //            GL.MatrixMode(GL._TEXTURE);
            //            GL.LoadIdentity();
            //            GL.MatrixMode(GL._MODELVIEW);
            //            States.Disable(GL._TEXTURE_GEN_S);
            //            States.Disable(GL._TEXTURE_GEN_T);
            //            stage++;
            //        }
            //        else
            //            BindTexture(null);

            //        States.ActiveTexture(stage + GL._TEXTURE0);

            //        if (m.DetailMap != null)
            //        {
            //            BindTexture(m.DetailMap.Id);
            //            GL.TexEnvi(GL._TEXTURE_ENV, GL._TEXTURE_ENV_MODE, GL._MODULATE);
            //            GL.MatrixMode(GL._TEXTURE);
            //            GL.LoadIdentity();
            //            GL.Scalef(256, 256, 256);
            //            GL.MatrixMode(GL._MODELVIEW);
            //            States.Disable(GL._TEXTURE_GEN_S);
            //            States.Disable(GL._TEXTURE_GEN_T);
            //            stage++;
            //        }
            //        else
            //            BindTexture(null);

            //        States.ActiveTexture(stage + GL._TEXTURE0);

            //        if (m.EnvironmentMap != null)
            //        {
            //            BindTexture(m.EnvironmentMap.Id);
            //            GL.MatrixMode(GL._TEXTURE);
            //            GL.LoadIdentity();
            //            GL.MatrixMode(GL._MODELVIEW);
            //            GL.TexEnvi(GL._TEXTURE_ENV, GL._TEXTURE_ENV_MODE, GL._MODULATE);
            //            GL.TexGeni(GL._S, GL._TEXTURE_GEN_MODE, GL._SPHERE_MAP);//GL_SPHERE_MAP);
            //            GL.TexGeni(GL._T, GL._TEXTURE_GEN_MODE, GL._SPHERE_MAP);//GL_SPHERE_MAP);
            //            States.Enable(GL._TEXTURE_GEN_S);
            //            States.Enable(GL._TEXTURE_GEN_T);
            //            stage++;
            //        }
            //        else
            //            BindTexture(null);

            //        for (int i = stage; i < 4; ++i)
            //            BindTexture(null, i);

            //        if (stage > 0)
            //            States.Enable(TextureTarget.Texture2D);
            //        else
            //            States.Disable(TextureTarget.Texture2D);

            //        GL.Color3f(1, 1, 1);
            //    //GL.ActiveTextureARB(glActiveTextureARB,GL._TEXTURE1);
            //        States.ActiveTexture(GL._TEXTURE0);

            //    if (m.NoLighting)
            //        States.Disable(GL._LIGHTING);
            //    else
            //        States.Enable(GL._LIGHTING);

            //}



                CheckError();



                MaterialFace face = MaterialFace.FrontAndBack;
				GL.Material(face, MaterialParameter.Specular, (float[])m.specular);

                GL.Material(face, MaterialParameter.Diffuse, (float[])m.diffuse);

                GL.Material(face, MaterialParameter.Ambient, (float[])m.ambient);

                GL.Material(face, MaterialParameter.Shininess, m.shininess);

				if (m.wire || WireFrameMode)
					GL.PolygonMode(face, PolygonMode.Line);
				else
					GL.PolygonMode(face, PolygonMode.Fill);

				if (m.twosided)
					States.Disable((int)GetPName.CullFace);
				else
                    States.Enable((int)GetPName.CullFace);

                if (m.DepthTest)
                    States.Enable((int)GetPName.DepthTest);
                else
                    States.Disable((int)GetPName.DepthTest);
                GL.DepthMask(m.DepthWrite);

                if (m.Additive)
				{
					//GL.BlendFunc(GL._ONE, GL._ONE);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                    //States.Disable(GL._DEPTH_TEST);
					States.Enable((int)GetPName.Blend);
                    GL.DepthMask(false);
				}
				else
				{
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    //States.Disable(GL._BLEND);
				}
			}
			else
			{
                States.ActiveTexture((int)TextureUnit.Texture0);
				BindTexture(null);
				GL.MatrixMode(MatrixMode.Texture);
				GL.LoadIdentity();
                GL.MatrixMode(MatrixMode.Modelview);
				States.Disable((int)GetPName.TextureGenS);
                States.Disable((int)GetPName.TextureGenT);
                GL.DepthMask(true);
            }
            CheckError();
        }

		public void SetLighting(bool b)
		{
			if (b)
                States.Enable((int)GetPName.Lighting);
			else
                States.Disable((int)GetPName.Lighting);
            CheckError();
        }

		public Cheetah.Graphics.VertexBuffer CreateStaticVertexBuffer(object data, int length)
		{
            CheckError();
            //if (SlowVertexBuffers || CompabilityMode)
			//	return CreateSlowVertexBuffer(data, length);
			VertexBuffer vb = new VertexBuffer();
            int[] id = new int[1];
			GL.GenBuffers(1, id);
            vb.id = id[0];
			GL.BindBuffer(BufferTarget.ArrayBuffer, vb.id);

			Type t = data.GetType();
			if (!t.IsArray)
				throw new Exception("wrong datatype.");

			//Array a = (Array)data;
			IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement((Array)data, 0);


			//fixed(void *ptr=data)
			//{
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(length), p, BufferUsageHint.StaticDraw);
			//}
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			vb.Size = length;
			Buffers[vb.id] = vb;
			BufferMemory += length;
            CheckError();
            return vb;
		}

		public Cheetah.Graphics.VertexBuffer CreateSlowVertexBuffer(object data, int length)
		{
			SlowVertexBuffer vb = new SlowVertexBuffer();
			vb.data = data;
			vb.Size = length;
			return vb;
		}

        private Cheetah.Graphics.DynamicVertexBuffer CreateSlowDynamicVertexBuffer(int length)
		{
			SlowDynamicVertexBuffer vb = new SlowDynamicVertexBuffer(this);
			vb.Size = length;
			vb.data = new byte[length];
			return vb;
		}

        public Cheetah.Graphics.DynamicVertexBuffer CreateDynamicVertexBuffer(int length)
		{
			//if (SlowVertexBuffers || CompabilityMode)
			//	return CreateSlowDynamicVertexBuffer(length);
            CheckError();

			DynamicVertexBuffer vb = new DynamicVertexBuffer(this);
			vb.Size = length;
            int[] id = new int[1];
			GL.GenBuffers(1, id);
            vb.id = id[0];
			GL.BindBuffer(BufferTarget.ArrayBuffer, vb.id);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(length), new byte[length], BufferUsageHint.DynamicDraw);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			Buffers[vb.id] = vb;
			BufferMemory += length;
            CheckError();
            return vb;
		}

        public void FreeVertexBuffer(Cheetah.Graphics.VertexBuffer b)
		{
			FreeQueue.Enqueue(b);
		}

		public void SetLight(int index, Light l)
        {
            CheckError();
            float[] f = new float[4];

            LightName ln = (LightName)(LightName.Light0 + index);

			if (l != null)
			{

				//glPushMatrix();
				//glLoadIdentity();

                Vector3 pos = l.AbsolutePosition;
				f[0] = pos.X;
                f[1] = pos.Y;
                f[2] = pos.Z;
				f[3] = l.directional ? 0 : 1;

				GL.Light(ln, LightParameter.Position, f);
                GL.Light(ln, LightParameter.Ambient, (float[])l.ambient);
                GL.Light(ln, LightParameter.Diffuse, (float[])l.diffuse);
                GL.Light(ln, LightParameter.Specular, (float[])l.specular);
                GL.Light(ln, LightParameter.ConstantAttenuation, l.attenuation.X);
                GL.Light(ln, LightParameter.LinearAttenuation, l.attenuation.Y);
                GL.Light(ln, LightParameter.QuadraticAttenuation, l.attenuation.Z);

                States.Enable((int)ln);
			}
			else
			{
                GL.Light(ln, LightParameter.Ambient, f);
                GL.Light(ln, LightParameter.Diffuse, f);
                GL.Light(ln, LightParameter.Specular, f);
                States.Disable((int)ln);
			}
            CheckError();
        }

        public void SetPointSize(float s)
        {
            pointsize = s;
        }
        float pointsize;

		public void SetMode(RenderMode m)
		{
            CheckError();
            switch (m)
			{
				case RenderMode.Draw2D:
					States.Disable((int)GetPName.PointSprite);
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
					GL.MatrixMode(MatrixMode.Projection);
                    global::OpenTK.Matrix4 m2 = global::OpenTK.Matrix4.CreateOrthographicOffCenter(0.0f, (float)width, (float)height, 0.0f, -1.0f, 1000.0f);
                    GL.LoadMatrix(ref m2);
                    GL.MatrixMode(MatrixMode.Modelview);
					GL.LoadIdentity();
                    States.Disable((int)GetPName.DepthTest);
                    States.Disable((int)GetPName.CullFace);
                    States.Disable((int)GetPName.Lighting);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    States.Enable((int)GetPName.Blend);
                    break;
				case RenderMode.Draw3D:
					GL.DepthMask(true);
                    States.Disable((int)GetPName.PointSprite);
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    States.ActiveTexture((int)TextureUnit.Texture0);
                    States.Enable((int)GetPName.DepthTest);
                    States.Enable((int)GetPName.CullFace);
					GL.CullFace(CullFaceMode.Back);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    States.Enable((int)GetPName.Blend);
                    GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
					break;
				case RenderMode.Draw3DWireFrame:
                    GL.DepthMask(true);
                    States.Disable((int)GetPName.PointSprite);
                    States.ActiveTexture((int)TextureUnit.Texture0);
                    States.Enable((int)GetPName.DepthTest);
                    States.Enable((int)GetPName.CullFace);
                    GL.CullFace(CullFaceMode.Back);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    States.Enable((int)GetPName.Blend);
                    GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
					break;
				case RenderMode.Draw3DPointSprite:
                    States.ActiveTexture((int)TextureUnit.Texture0);
                    GL.TexEnv(TextureEnvTarget.PointSprite, TextureEnvParameter.CoordReplace, 1);
                    States.Enable((int)GetPName.PointSprite);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    UseShader(pointsprite);
                    float m1 = 2.0f * (float)Math.Atan(cam.Fov / 2.0 / 180.0 * Math.PI);
                    int[] vp = new int[4];
                    GL.GetInteger(GetPName.Viewport, vp);
                    float v = (float)vp[3];
                    m1 /= v;
                    SetUniform(pointsprite.GetUniformLocation("WorldSize"), new float[] { pointsize });
                    SetUniform(pointsprite.GetUniformLocation("Attenuation"), new float[] { 0,0,m1 });
                    break;
				case RenderMode.DrawSkyBox:
                    States.Disable((int)GetPName.PointSprite);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    States.ActiveTexture((int)TextureUnit.Texture0);
                    States.Enable((int)GetPName.CullFace);
                    GL.CullFace(CullFaceMode.Back);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    States.Enable((int)GetPName.Blend);
                    GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
                    States.Disable((int)GetPName.DepthTest);
					GL.DepthMask(false);

					break;
				default:
					throw new Exception("dont know rendermode " + m.ToString());
			}
            CheckError();
        }

		public void Flip()
		{
            //lastformat = null;
            //lastshader = null;
            //lastbuffer = null;
			object b;
			try
			{
				while ((b = FreeQueue.Dequeue()) != null)
				{
					if (b is VertexBuffer)
					{
						VertexBuffer vb = (VertexBuffer)b;
						GL.DeleteBuffers(1, new int[] { vb.id });
						Buffers.Remove(vb.id);
						vb.id = -1;
						BufferMemory -= vb.Size;
					}
					else if(b is DynamicVertexBuffer)
					{
						DynamicVertexBuffer vb = (DynamicVertexBuffer)b;
						GL.DeleteBuffers(1, new int[] { vb.id });
						Buffers.Remove(vb.id);
						vb.id = -1;
						BufferMemory -= vb.Size;
					}
				}
			}
			catch (InvalidOperationException)
			{
			}

            if (SwapFunc != null)
                SwapFunc();
            //else
			//    Sdl.SDL_GL_SwapBuffers();
			if (WireFrameMode)
				Clear(0, 0, 0, 1);
		}
        public delegate void VoidDelegate();
        public VoidDelegate SwapFunc;

        unsafe protected void DrawSlow(Cheetah.Graphics.VertexBuffer vertices, PrimitiveType type, int offset, int count, IndexBuffer ib)
		{
			VertexFormat format = vertices.Format;
            CheckError();


			Array a;
			if (vertices is SlowVertexBuffer)
				a = (Array)((SlowVertexBuffer)vertices).data;
			else if (vertices is SlowDynamicVertexBuffer)
				a = (Array)((SlowDynamicVertexBuffer)vertices).data;
			else
				throw new Exception();

			IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);

			int offset2 = 0;
			byte* buffer = (byte*)p.ToPointer();
			int vertexsize = format.Size;

			for (int i = 0; i < format.Count; ++i)
			{
				VertexFormat.Element e = format[i];
				int datatype;
				int elementsize = e.Size;
				switch (e.Type)
				{
					case VertexFormat.ElementType.Float:
						datatype = (int)ColorPointerType.Float;
						break;
					case VertexFormat.ElementType.Byte:
                        datatype = (int)ColorPointerType.Byte;
						break;
					case VertexFormat.ElementType.Int:
                        datatype = (int)ColorPointerType.Int;
						break;
					default:
						throw new Exception();
				}

				switch (e.Name)
				{
					case VertexFormat.ElementName.Color:
						GL.ColorPointer(e.Count, (ColorPointerType)datatype, vertexsize, (IntPtr)(buffer + offset2));
						States.EnableClientState((int)GetPName.ColorArray);
						break;
					case VertexFormat.ElementName.Position:
                        GL.VertexPointer(e.Count, (VertexPointerType)datatype, vertexsize, (IntPtr)(buffer + offset2));
                        States.EnableClientState((int)GetPName.VertexArray);
						break;
					case VertexFormat.ElementName.Texture0:
                        GL.TexCoordPointer(e.Count, (TexCoordPointerType)datatype, vertexsize, (IntPtr)(buffer + offset2));
                        States.EnableClientState((int)GetPName.TextureCoordArray);
						break;
					case VertexFormat.ElementName.Normal:
                        GL.NormalPointer((NormalPointerType)datatype, vertexsize, (IntPtr)(buffer + offset2));
                        States.EnableClientState((int)GetPName.NormalArray);
						break;
					default:
						throw new Exception();
				}

				offset2 += elementsize;
			}

			BeginMode gltype;
			switch (type)
			{
				case PrimitiveType.QUADS:
					gltype = BeginMode.Quads;
					break;
				case PrimitiveType.TRIANGLESTRIP:
					gltype = BeginMode.TriangleStrip;
					break;
				case PrimitiveType.TRIANGLES:
					gltype = BeginMode.Triangles;
					break;
				case PrimitiveType.LINES:
					gltype = BeginMode.Lines;
					break;
				case PrimitiveType.LINESTRIP:
					gltype = BeginMode.LineStrip;
					break;
				case PrimitiveType.POINTS:
                    gltype = BeginMode.Points;
					break;
				default:
					throw new Exception();
			}

			if (ib == null)
			{
				GL.DrawArrays(gltype, offset, count);
			}
			else
			{
				GL.DrawElements(gltype, count, DrawElementsType.UnsignedInt, ib.buffer);
			}
            States.DisableClientState((int)GetPName.ColorArray);
            States.DisableClientState((int)GetPName.VertexArray);
            States.DisableClientState((int)GetPName.TextureCoordArray);
            States.DisableClientState((int)GetPName.NormalArray);
            CheckError();
        }

        public void Draw(Cheetah.Graphics.VertexBuffer vertices, PrimitiveType type, int offset, int count, IndexBuffer ib)
        {
            Draw(vertices, type, offset, count, ib, 0);
        }

        //VertexFormat lastformat = null;
        //Cheetah.VertexBuffer lastbuffer = null;
        //Cheetah.Shader lastshader = null;
        public void Draw(Cheetah.Graphics.VertexBuffer vertices, PrimitiveType type, int offset, int count, IndexBuffer ib, int indexoffset)
		{
			if (vertices is SlowVertexBuffer || vertices is SlowDynamicVertexBuffer)
			{
				DrawSlow(vertices, type, offset, count, ib);
				return;
			}
            CheckError();

			VertexFormat format;
			if (vertices is VertexBuffer)
			{
				VertexBuffer vb = (VertexBuffer)vertices;
				format = vertices.Format;
				GL.BindBuffer(BufferTarget.ArrayBuffer, vb.id);
			}
			else if (vertices is DynamicVertexBuffer)
			{
				DynamicVertexBuffer vb = (DynamicVertexBuffer)vertices;
				format = vertices.Format;
				GL.BindBuffer(BufferTarget.ArrayBuffer, vb.id);
			}
			else
				throw new Exception("wrong vertexbuffer type: "+vertices.GetType().ToString());

            if (format == null)
                throw new Exception("no vertex format.");

			int vertexsize = format.Size;

            //if (lastformat == null || format != lastformat || lastbuffer==null||vertices!=lastbuffer || lastshader==null||lastshader!=CurrentShader)
            {
                bool[] attribs = new bool[16];
                for (int i = 0; i < attribs.Length; ++i)
                    attribs[i] = false;

                bool GL_COLOR_ARRAY = false;
                bool GL_VERTEX_ARRAY = false;
                bool GL_NORMAL_ARRAY = false;
                bool GL_TEXTURE_COORD_ARRAY = false;


                int start = 0;
                for (int i = 0; i < format.Count; ++i)
                {
                    VertexFormat.Element e = format[i];
                    int datatype;
                    int elementsize = e.Size;
                    switch (e.Type)
                    {
                        case VertexFormat.ElementType.Float:
                            datatype = (int)ColorPointerType.Float;
                            break;
                        case VertexFormat.ElementType.Byte:
                            datatype = (int)ColorPointerType.Byte;
                            break;
                        case VertexFormat.ElementType.Int:
                            datatype = (int)ColorPointerType.Int;
                            break;
                        default:
                            throw new Exception();
                    }

                    int startwithoffset = start;
                    if (ib != null)
                        startwithoffset += offset * format.Size;

                    switch (e.Name)
                    {
                        case VertexFormat.ElementName.Color:
                            GL.ColorPointer(e.Count, (ColorPointerType)datatype, vertexsize, new IntPtr(startwithoffset));
                            GL_COLOR_ARRAY = true;
                            //States.EnableClientState(GL._COLOR_ARRAY);
                            break;
                        case VertexFormat.ElementName.Position:
                            GL.VertexPointer(e.Count, (VertexPointerType)datatype, vertexsize, new IntPtr(startwithoffset));
                            GL_VERTEX_ARRAY = true;
                            //States.EnableClientState(GL._VERTEX_ARRAY);
                            break;
                        case VertexFormat.ElementName.Texture0:
                            GL_TEXTURE_COORD_ARRAY = true;
                            States.ClientActiveTexture((int)TextureUnit.Texture0);
                            GL.TexCoordPointer(e.Count, (TexCoordPointerType)datatype, vertexsize, new IntPtr(startwithoffset));
                            States.EnableClientState((int)GetPName.TextureCoordArray);
                            /*States.ClientActiveTexture(GL._TEXTURE1);
                            GL.TexCoordPointer(e.Count, datatype, vertexsize, new IntPtr(startwithoffset));
                            States.EnableClientState(GL._TEXTURE_COORD_ARRAY);
                            States.ClientActiveTexture(GL._TEXTURE0);*/
                            break;
                        case VertexFormat.ElementName.Normal:
                            GL.NormalPointer((NormalPointerType)datatype, vertexsize, new IntPtr(startwithoffset));
                            GL_NORMAL_ARRAY = true;
                            //GL.VertexAttribPointer(14, 3, GL._FLOAT, false, vertexsize, new IntPtr(start));
                            //GL.EnableVertexAttribArray(14);
                            break;
                        case VertexFormat.ElementName.Tangent:
                            if (CurrentShader != null)
                            {
                                int loc = CurrentShader.GetAttributeLocation("tangent");
                                if (loc >= 0)
                                {
                                    GL.VertexAttribPointer(loc, e.Count, VertexAttribPointerType.Float, false, vertexsize, new IntPtr(startwithoffset));
                                    attribs[loc]=true;
                                }
                            }
                            break;
                        case VertexFormat.ElementName.Binormal:
                            if (CurrentShader != null)
                            {
                                int loc = CurrentShader.GetAttributeLocation("binormal");
                                if (loc >= 0)
                                {
                                    GL.VertexAttribPointer(loc, e.Count, VertexAttribPointerType.Float, false, vertexsize, new IntPtr(startwithoffset));
                                    attribs[loc] = true;
                                }
                            }
                            break;
                        case VertexFormat.ElementName.Texture1:
                            States.ClientActiveTexture((int)TextureUnit.Texture1);
                            GL.TexCoordPointer(e.Count, (TexCoordPointerType)datatype, vertexsize, new IntPtr(startwithoffset));
                            GL_TEXTURE_COORD_ARRAY = true;
                            States.EnableClientState((int)GetPName.TextureCoordArray);
                            States.ClientActiveTexture((int)TextureUnit.Texture0);
                            break;
                        /*case VertexFormat.ElementName.None:
                            if (CurrentShader == null)
                                throw new Exception("");
                            int loc=CurrentShader.GetAttributeLocation(e.Attrib);
                            GL.VertexAttribPointer(loc, e.Count, GL._FLOAT, false, vertexsize, new IntPtr(start));
                            GL.EnableVertexAttribArray(loc);
                            break;*/
                        default:
                            throw new Exception("unknown vertexformat name.");
                    }
                    if (e.Attrib != null && e.Attrib != "")
                    {
                        //if (CurrentShader == null)
                        //    throw new Exception("");
                        int loc = CurrentShader.GetAttributeLocation(e.Attrib);
                        if (loc >= 0)
                        {
                            GL.VertexAttribPointer(loc, e.Count, VertexAttribPointerType.Float, false, vertexsize, new IntPtr(startwithoffset));
                            attribs[loc] = true;
                        }
                    }
                    else if (e.Name == VertexFormat.ElementName.None)
                    {
                        throw new Exception("no name and no attrib");
                    }


                    start += elementsize;
                }

                for (int i = 0; i < 16; ++i)
                {
                    if(attribs[i])
                        States.EnableVertexAttribArray(i);
                    else
                        States.DisableVertexAttribArray(i);
                }



                if(GL_COLOR_ARRAY)
                    States.EnableClientState((int)GetPName.ColorArray);
                else
                    States.DisableClientState((int)GetPName.ColorArray);
                if (GL_VERTEX_ARRAY)
                    States.EnableClientState((int)GetPName.VertexArray);
                else
                    States.DisableClientState((int)GetPName.VertexArray);
                if (GL_NORMAL_ARRAY)
                    States.EnableClientState((int)GetPName.NormalArray);
                else
                    States.DisableClientState((int)GetPName.NormalArray);

                CheckError();

            }

            BeginMode gltype;
            switch (type)
            {
                case PrimitiveType.QUADS:
                    gltype = BeginMode.Quads;
                    break;
                case PrimitiveType.TRIANGLESTRIP:
                    gltype = BeginMode.TriangleStrip;
                    break;
                case PrimitiveType.TRIANGLES:
                    gltype = BeginMode.Triangles;
                    break;
                case PrimitiveType.LINES:
                    gltype = BeginMode.Lines;
                    break;
                case PrimitiveType.LINESTRIP:
                    gltype = BeginMode.LineStrip;
                    break;
                case PrimitiveType.POINTS:
                    gltype = BeginMode.Points;
                    break;
                default:
                    throw new Exception();
            }

			if (ib == null)
			{
				GL.DrawArrays(gltype, offset, count);
			}
			else
			{
				//GL.DrawElements(gltype, count, GL._UNSIGNED_INT, ib.buffer);
                //fixed (int* startindex = ib.buffer)
                IntPtr startindex = Marshal.UnsafeAddrOfPinnedArrayElement(ib.buffer, indexoffset);
                {
                    //GL.DrawElements(gltype, count, GL._UNSIGNED_INT, new IntPtr(startindex+indexoffset));
                    GL.DrawElements(gltype, count, DrawElementsType.UnsignedInt, startindex);
                }
            }

            CheckError();

		}

		//		protected Video video;
		protected int width, height;
        protected int currentwidth, currentheight;

		protected Shader TextShader;

        State States = new State();

        public bool GeometryShadersSupported = false;

		public int BufferMemory = 0;
		private Queue FreeQueue = new Queue();
		public Hashtable Buffers = new Hashtable();
		public Hashtable Textures = new Hashtable();
        Camera cam;

		//[Configurable(new string[] { "video", "wireframemode" })]
		public bool WireFrameMode
		{
			get
			{
				return _WireFrameMode;
			}
			set
			{
				_WireFrameMode = value;
			}
		}
		protected bool _WireFrameMode = false;
	}
}
