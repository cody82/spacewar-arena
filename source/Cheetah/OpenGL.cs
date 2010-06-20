using Tao.OpenGl;
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
            if (s != null)
            {
                CurrentShader = ((Shader)s);
                int id = CurrentShader.ProgramId;

                Gl.glUseProgramObjectARB(id);
            }
            else
            {
                CurrentShader = null;
                Gl.glUseProgramObjectARB(0);
            }

        }

        Shader CurrentShader;

        public void SetUniform(int location, float[] values)
        {
            switch (values.Length)
            {
                case 1:
                    Gl.glUniform1fARB(location, values[0]);
                    break;
                case 2:
                    Gl.glUniform2fARB(location, values[0], values[1]);
                    break;
                case 3:
                    Gl.glUniform3fARB(location, values[0], values[1], values[2]);
                    break;
                case 4:
                    Gl.glUniform4fARB(location, values[0], values[1], values[2], values[3]);
                    //Gl.glUniform4fvARB(location, 4, values);
                    break;
                case 16:
                    Gl.glUniformMatrix4fvARB(location, 1, 0, values);
                    break;
                default:
                    throw new Exception();
            }
        }

        public void SetUniform(int location, int[] values)
        {
            switch (values.Length)
            {
                case 1:
                    Gl.glUniform1iARB(location, values[0]);
                    break;
                case 2:
                    Gl.glUniform2iARB(location, values[0], values[1]);
                    break;
                case 3:
                    Gl.glUniform3iARB(location, values[0], values[1], values[2]);
                    break;
                case 4:
                    Gl.glUniform4iARB(location, values[0], values[1], values[2], values[3]);
                    //Gl.glUniform4fvARB(location, 4, values);
                    break;
                //case 16:
                //    Gl.glUniformMatrix4fvARB(location, 1, false, values);
                //    break;
                default:
                    throw new Exception();
            }
        }
        public void SetAttribute(int location, float[] values)
        {
            switch (values.Length)
            {
                case 1:
                    Gl.glVertexAttrib1fARB(location, values[0]);
                    break;
                case 2:
                    Gl.glVertexAttrib2fARB(location, values[0], values[1]);
                    break;
                case 3:
                    Gl.glVertexAttrib3fARB(location, values[0], values[1], values[2]);
                    break;
                case 4:
                    Gl.glVertexAttrib4fARB(location, values[0], values[1], values[2], values[3]);
                    //Gl.glUniform4fvARB(location, 4, values);
                    break;
                case 16:
                    //Gl.glVertexAttribArrayObjectATI(location, 1, false, values);
                    break;
                default:
                    throw new Exception();
            }
        }

        public void SetAttributeArray(int location, int offset, int stride, object array)
        {
        }

        public Cheetah.RenderTarget CreateRenderTarget(Cheetah.TextureId texture, Cheetah.TextureId depth)
        {
            int index;
            int[] tmp = new int[1];
            Gl.glGenFramebuffersEXT(1, tmp);
            index = tmp[0];
            if (index <= 0)
                throw new Exception("cant create framebuffer.");

            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, index);

            //Gl.glFramebufferTexture2DEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_DEPTH_ATTACHMENT_EXT, Gl.GL_TEXTURE_2D, d, 0);
            if (depth != null)
            {
                Gl.glFramebufferTexture2DEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_DEPTH_ATTACHMENT_EXT, Gl.GL_TEXTURE_2D, ((TextureId)depth).id, 0);
            }

            Gl.glFramebufferTexture2DEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_COLOR_ATTACHMENT0_EXT, Gl.GL_TEXTURE_2D, ((TextureId)texture).id, 0);


            int status = Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT);
            if (status == Gl.GL_FRAMEBUFFER_COMPLETE_EXT)
            {
            }
            else
            {
                throw new Exception("framebuffer check failed.");
            }

            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, 0);

            return new RenderTarget(index,(TextureId)texture);
        }

        public void BindRenderTarget(Cheetah.RenderTarget target)
        {
            if (target != null)
            {
                currentwidth=((TextureId)target.Texture).w;
                currentheight = ((TextureId)target.Texture).h;
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, ((RenderTarget)target).id);
            }
            else
            {
                currentwidth = width;
                currentheight = height;
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, 0);
            }
        }

        protected int GlCreateShader(string code, int type)
        {
            int[] l3 = new int[1];
            int id = Gl.glCreateShaderObjectARB(type);

            Gl.glShaderSourceARB(id, 1, new string[] { code }, new int[] { code.Length });
            Gl.glCompileShaderARB(id);
            Gl.glGetObjectParameterivARB(id, Gl.GL_OBJECT_COMPILE_STATUS_ARB, l3);
            int ok = l3[0];

            if (ok != 1)
            {
                int l, l2;
                Gl.glGetObjectParameterivARB(id, Gl.GL_OBJECT_INFO_LOG_LENGTH_ARB, l3);
                l = l3[0];
                StringBuilder str = new StringBuilder(l);
                Gl.glGetInfoLogARB(id, l, l3, str);
                l2 = l3[0];
                string log = str.ToString();

                //string log = OpenTK.Graphics.GL.GetProgramInfoLog(id);

                System.Console.WriteLine(log);
                switch (type)
                {
                    case Gl.GL_VERTEX_SHADER_ARB:
                        throw new Exception("cant compile vertexshader");
                    case Gl.GL_FRAGMENT_SHADER_ARB:
                        throw new Exception("cant compile fragmentshader");
                    case Gl.GL_GEOMETRY_SHADER_EXT:
                        throw new Exception("cant compile geometryshader");
                }
            }
            return id;
        }

        public Cheetah.Graphics.Shader CreateShader(string vertex, string fragment, string geometry, PrimitiveType input, PrimitiveType output)
        {
            int vertexid = 0;
            if (vertex != null)
            {
                vertexid = GlCreateShader(vertex, Gl.GL_VERTEX_SHADER_ARB);
            }
            
            int fragmentid = 0;
            if (fragment != null)
            {
                fragmentid = GlCreateShader(fragment, Gl.GL_FRAGMENT_SHADER_ARB);
            }
            int geometryid = 0;
            if (geometry != null && GeometryShadersSupported)
            {
                geometryid = GlCreateShader(geometry, Gl.GL_GEOMETRY_SHADER_EXT);
            }

            if (vertexid == 0 && fragmentid == 0)
                throw new Exception("no vertex/fragment shader");
            if (vertexid == 0 && geometryid != 0)
                throw new Exception("no vertex shader but geometry shader?!");

            int p = GlCreateProgram(vertexid, fragmentid, geometryid, input, output);

            return new Shader(vertex, fragment, vertexid, fragmentid, p, geometry, geometryid);
        }

        public Cheetah.Graphics.Shader CreateShader(string vertex, string fragment)
        {
            return CreateShader(vertex, fragment, null, (PrimitiveType)0,(PrimitiveType)0);
        }

        protected int GlCreateProgram(int vertex, int fragment, int geometry, PrimitiveType input, PrimitiveType output)
        {
            int[] l3 = new int[1];

            int ok;

            int p = Gl.glCreateProgramObjectARB();

            if (vertex != 0)
                Gl.glAttachObjectARB(p, vertex);
            if (fragment != 0)
                Gl.glAttachObjectARB(p, fragment);

            if (geometry != 0)
            {
                Gl.glAttachObjectARB(p, geometry);
                GlSetProgramPrimitiveType(p, input, output);
                Gl.glGetIntegerv(Gl.GL_MAX_GEOMETRY_OUTPUT_VERTICES_EXT, l3);

                Gl.glProgramParameteriEXT(p, Gl.GL_GEOMETRY_VERTICES_OUT_EXT, l3[0]);

            }

            Gl.glLinkProgramARB(p);
            Gl.glGetObjectParameterivARB(p, Gl.GL_OBJECT_LINK_STATUS_ARB, l3);
            ok = l3[0];
            if (ok != 1)
            {
                int l, l2;
                Gl.glGetObjectParameterivARB(p, Gl.GL_OBJECT_INFO_LOG_LENGTH_ARB, l3);
                l = l3[0];
                StringBuilder str = new StringBuilder();
                Gl.glGetInfoLogARB(p, l, l3, str);
                l2 = l3[0];
                string log = str.ToString();

                //string log = OpenTK.Graphics.GL.GetProgramInfoLog(p);

                System.Console.WriteLine(log);
                throw new Exception("cant link program");
            }


            return p;
        }

        protected int GlPrimitiveInputType(PrimitiveType type)
        {
            int t;
            switch (type)
            {
                case PrimitiveType.POINTS:
                    t = Gl.GL_POINTS;
                    break;
                case PrimitiveType.LINES:
                    t = Gl.GL_LINES;
                    break;
                case PrimitiveType.TRIANGLES:
                    t = Gl.GL_TRIANGLES;
                    break;
                case PrimitiveType.TRIANGLESTRIP:
                    t = Gl.GL_TRIANGLES_ADJACENCY_EXT;
                    break;
                case PrimitiveType.LINESTRIP:
                    t = Gl.GL_LINES_ADJACENCY_EXT;
                    break;
                default:
                    throw new Exception("wrong input primitive type: " + type.ToString());
            }
            return t;
        }
        protected int GlPrimitiveOutputType(PrimitiveType type)
        {
            int t;
            switch (type)
            {
                case PrimitiveType.POINTS:
                    t = Gl.GL_POINTS;
                    break;
                case PrimitiveType.TRIANGLESTRIP:
                    t = Gl.GL_TRIANGLE_STRIP;
                    break;
                case PrimitiveType.LINESTRIP:
                    t = Gl.GL_LINE_STRIP;
                    break;

                //HACK
                /*case PrimitiveType.LINES:
                    t = Gl.GL_LINES;
                    break;
                case PrimitiveType.TRIANGLES:
                    t = Gl.GL_TRIANGLES;
                    break;*/

                default:
                    throw new Exception("wrong output primitive type: " + type.ToString());
            }
            return t;
        }
        protected void GlSetProgramPrimitiveType(int p, PrimitiveType input, PrimitiveType output)
        {
            Gl.glProgramParameteriEXT(p, Gl.GL_GEOMETRY_INPUT_TYPE_EXT, GlPrimitiveInputType(input));
            Gl.glProgramParameteriEXT(p, Gl.GL_GEOMETRY_OUTPUT_TYPE_EXT, GlPrimitiveOutputType(output));
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
                    int i = Gl.glGetUniformLocationARB(ProgramId, name);
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
                    int i=Gl.glGetAttribLocationARB(ProgramId, name);
                    AttributeLocations[name] = i;
                    return i;
                }
            }
        }
        /*
		protected class CgGlEffect : CgEffect
		{
			static CgGlEffect()
			{
				Tao.Cg.CgGl.cgGLRegisterStates(cgContext);
			}

			public CgGlEffect(string code) : base(code)
			{
			}
			public override void SetParameter(EffectParameter param, Texture t)
			{
				CgEffectParameter e = (CgEffectParameter)param;
				if (t == null)
				{
					CgGl.cgGLSetTextureParameter(e.ptr, 0);
					return;
				}

				Cheetah.TextureId id= t.Id;
				CgGl.cgGLSetTextureParameter(e.ptr, ((OpenGL.TextureId)id).id);
			}

			public override void BeginPass(int pass)
			{
				base.BeginPass(pass);

				IntPtr p=Cg.cgGetEffectParameterBySemantic(cgEffect, "ModelViewProjection");
				if(p!=IntPtr.Zero)
					CgGl.cgGLSetStateMatrixParameter(p, CgGl.CG_GL_MODELVIEW_PROJECTION_MATRIX, CgGl.CG_GL_MATRIX_IDENTITY);
				
				p = Cg.cgGetEffectParameterBySemantic(cgEffect, "ModelView");
				if (p != IntPtr.Zero)
					CgGl.cgGLSetStateMatrixParameter(p, CgGl.CG_GL_MODELVIEW_MATRIX, CgGl.CG_GL_MATRIX_IDENTITY);

			}
		}
        */
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
			~VertexBuffer()
			{
				Dispose();
			}

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
				Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, id);
				return Gl.glMapBufferARB(Gl.GL_ARRAY_BUFFER_ARB, Gl.GL_WRITE_ONLY_ARB);
			}

			public override void Unlock()
			{
				Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, id);
				Gl.glUnmapBufferARB(Gl.GL_ARRAY_BUFFER_ARB);
				Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, 0);
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
                        Gl.glEnable(state);
                        boolstate[state] = true;
                    }
                }
                else
                {
                    Gl.glEnable(state);
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
                        Gl.glDisable(state);
                        boolstate[state] = false;
                    }
                }
                else
                {
                    Gl.glDisable(state);
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
                        Gl.glEnableVertexAttribArray(state);
                        boolstate[state] = true;
                    }
                }
                else
                {
                    Gl.glEnableVertexAttribArray(state);
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
                        Gl.glDisableVertexAttribArray(state);
                        boolstate[state] = false;
                    }
                }
                else
                {
                    Gl.glDisableVertexAttribArray(state);
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
                        Gl.glEnableClientState(state);
                        boolstate[state] = true;
                    }
                }
                else
                {
                    Gl.glEnableClientState(state);
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
                        Gl.glDisableClientState(state);
                        boolstate[state] = false;
                    }
                }
                else
                {
                    Gl.glDisableClientState(state);
                    boolstate[state] = false;
                }
            }

            public void ActiveTexture(int state)
            {
                if (state != activetexture)
                {
                    Gl.glActiveTextureARB(state);
                    activetexture = state;
                }
            }
            public void ClientActiveTexture(int state)
            {
                if (state != clientactivetexture)
                {
                    Gl.glClientActiveTextureARB(state);
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
			Gl.glPushMatrix();
		}

		public void PopMatrix()
		{
			Gl.glPopMatrix();
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

		public void SetFog(Fog f)
		{
			if (f != null)
			{
				int mode;
				switch (f.Mode)
				{
					case Fog.FogMode.LINEAR:
						mode = Gl.GL_LINEAR;
						break;
					default:
						throw new Exception("unknown fogmode.");
				}
				States.Enable(Gl.GL_FOG);
				Gl.glFogi(Gl.GL_FOG_MODE, mode);
				Gl.glFogfv(Gl.GL_FOG_COLOR, new float[] { f.Color.r, f.Color.g, f.Color.b, f.Color.a });
				Gl.glFogf(Gl.GL_FOG_DENSITY, f.Density);
				Gl.glHint(Gl.GL_FOG_HINT, Gl.GL_DONT_CARE);
				Gl.glFogf(Gl.GL_FOG_START, f.Start);
				Gl.glFogf(Gl.GL_FOG_END, f.End);
			}
			else
			{
				States.Disable(Gl.GL_FOG);
			}
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

		public void LoadMatrix(Matrix3 m)
		{
			Gl.glLoadMatrixf((float[])m);
		}

		public void MultMatrix(Matrix3 m)
		{
			Gl.glMultMatrixf((float[])m);
		}

		public void GetMatrix(float[] modelview, float[] projection)
		{
			if (projection != null)
				Gl.glGetFloatv(Gl.GL_PROJECTION_MATRIX, projection);
			if (modelview != null)
				Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, modelview);
		}

        public float[] UnProject(float[] winxyz, float[] model, float[] proj, int[] viewport)
        {
 			double[] projection = new double[16];
			double[] modelview = new double[16];
            int[] _viewport = new int[4];

            if (model != null && proj != null && viewport != null)
            {
                for (int i = 0; i < 16; ++i)
                {
                    projection[i] = (double)proj[16];
                    modelview[i] = (double)model[16];
                }
            }
            else
            {
                proj= new float[16];
                model = new float[16];
                Gl.glGetDoublev(Gl.GL_PROJECTION_MATRIX, projection);
                Gl.glGetDoublev(Gl.GL_MODELVIEW_MATRIX, modelview);
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, _viewport);
                Gl.glGetFloatv(Gl.GL_PROJECTION_MATRIX, proj);
                Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, model);
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, _viewport);
            }
            double objx, objy, objz;
            Glu.gluUnProject((double)winxyz[0], (double)winxyz[1], (double)winxyz[2], modelview, projection, _viewport, out objx, out objy, out objz);
            return new float[3]{(float)objx,(float)objy,(float)objz};
            //return (float[])(new Camera().gluUnProject(winxyz[0], winxyz[1], winxyz[2], model, proj, _viewport));
        }

        public float[] GetRasterPosition(float[] pos3d)
		{
			double[] projection = new double[16];
			double[] modelview = new double[16];
			int[] viewport = new int[4];

			Gl.glGetDoublev(Gl.GL_PROJECTION_MATRIX, projection);
			Gl.glGetDoublev(Gl.GL_MODELVIEW_MATRIX, modelview);
			Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

			double x, y, z;
			Glu.gluProject((double)pos3d[0], (double)pos3d[1], (double)pos3d[2],
				modelview, projection, viewport, out x, out y, out z);

			return new float[] { (float)x, (float)y, (float)z };
		}

        public void Draw(string text, float x, float y, float sx, float sy, Cheetah.Texture t, Color4f color, float width, RectangleF scissor)
        {
            int c = 16;
            float f = 1 / (float)c;
            BindTexture(t.Id);

            //if (CompabilityMode)
            //{
            //    Gl.glPushMatrix();
            //    Gl.glLoadIdentity();
            //    Gl.glMatrixMode(Gl.GL_PROJECTION);
            //    Gl.glPushMatrix();
            //    Gl.glLoadIdentity();
            //    Gl.glOrtho(0, 1, 1, 0, -1, 10);
            //    Gl.glColor4f(color.r, color.g, color.b, color.a);

            //    for (int i = 0; i < text.Length; ++i)
            //    {
            //        int a = text[i];
            //        float xf = a % c, yf = a / c;

            //        Gl.glBegin(Gl.GL_QUADS);

            //        Gl.glTexCoord2f(xf * f, yf * f);
            //        Gl.glVertex2f(x + (float)i * sx, y);

            //        Gl.glTexCoord2f(xf * f + f, yf * f);
            //        Gl.glVertex2f(x + sx + (float)i * sx, y);

            //        Gl.glTexCoord2f(xf * f + f, yf * f + f);
            //        Gl.glVertex2f(sx + x + (float)i * sx, sy + y);

            //        Gl.glTexCoord2f(xf * f, yf * f + f);
            //        Gl.glVertex2f(x + (float)i * sx, y + sy);


            //        Gl.glEnd();
            //    }
            //    Gl.glPopMatrix();
            //    Gl.glMatrixMode(Gl.GL_MODELVIEW);
            //    Gl.glPopMatrix();
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

                    Gl.glBegin(Gl.GL_QUADS);
                    Gl.glVertex2f(0, 0);
                    Gl.glVertex2f(1, 0);
                    Gl.glVertex2f(1, 1);
                    Gl.glVertex2f(0, 1);
                    Gl.glEnd();
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
            if (!Gl.IsExtensionSupported(name))
            {
                System.Console.WriteLine("extension missing: " + name);
                return false;
            }
            else
                return true;
        }

		public OpenGL(int _width, int _height)
		{
            Root.Instance.UserInterface.Renderer = this;

            System.Console.WriteLine("OpenGL Version: " + Gl.glGetString(Gl.GL_VERSION));
            System.Console.WriteLine("OpenGL Vendor: " + Gl.glGetString(Gl.GL_VENDOR));
            System.Console.WriteLine("OpenGL Renderer: " + Gl.glGetString(Gl.GL_RENDERER));

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
            LoadExtension("GL_EXT_framebuffer_object");
            LoadExtension("GL_ARB_texture_cube_map");
            LoadExtension("GL_EXT_texture_compression_s3tc");
            LoadExtension("GL_ARB_texture_compression");

            if (LoadExtension("GL_EXT_geometry_shader4"))
            {
                System.Console.WriteLine("OpenGL: Geometry Shaders supported!");
                GeometryShadersSupported = true;
            }
            LoadExtension("GL_EXT_gpu_shader4");
            LoadExtension("GL_EXT_bindable_uniform");

			width = currentwidth=_width;
			height = currentheight=_height;
			//video=SDL.Instance.Video;
            States.Enable(Gl.GL_BLEND);
			States.Enable(Gl.GL_TEXTURE_2D);
			States.Enable(Gl.GL_DEPTH_TEST);
			Gl.glShadeModel(Gl.GL_SMOOTH);
			Gl.glCullFace(Gl.GL_BACK);
			States.Enable(Gl.GL_CULL_FACE);
			//States.Disable(Gl.GL_CULL_FACE);
			Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, new float[] { 0, 0, 0, 1 });


			//if (!CompabilityMode)
			{
				string path = "shaders/glsl/";
				Console.WriteLine("creating shaders...");
				//StreamReader r;// = new StreamReader(path + "simple3d.vp");
				//simple3d = CreateVertexProgram(r.ReadToEnd());

				//r = new StreamReader(path + "text.vp");
				//text = CreateVertexProgram(r.ReadToEnd());

				//Console.WriteLine("creating fragment programs...");
				//r = new StreamReader(path + "simple.fp");
				//simple = CreateFragmentProgram(r.ReadToEnd());

				//r = new StreamReader(path + "textured.fp");
				//textured = CreateFragmentProgram(r.ReadToEnd());

				//r = new StreamReader(path + "terrain.fp");
				//terrain = CreateFragmentProgram(r.ReadToEnd());

                TextShader = (Shader)Root.Instance.ResourceManager.Load(path + "text.shader", typeof(Cheetah.Graphics.Shader));
                pointsprite = (Shader)Root.Instance.ResourceManager.Load(path + "pointsprite.shader", typeof(Cheetah.Graphics.Shader));
                
                
                States.Enable(Gl.GL_VERTEX_PROGRAM_POINT_SIZE_ARB);
            }

		}
        Shader pointsprite;

		public FragmentProgram CreateFragmentProgram(string code)
		{
			FragmentProgram fp = new FragmentProgram();

			int[] id=new int[2];
			Gl.glGenProgramsARB(1, id);
			fp.id=id[0];
			if (fp.id == 0)
				throw new Exception("cant create vertexprogram.");

			Gl.glBindProgramARB(Gl.GL_FRAGMENT_PROGRAM_ARB, fp.id);
			Gl.glProgramStringARB(Gl.GL_FRAGMENT_PROGRAM_ARB, Gl.GL_PROGRAM_FORMAT_ASCII_ARB,
				code.Length, code);

			int error;
			int[] e = new int[1];
			Gl.glGetIntegerv(Gl.GL_PROGRAM_ERROR_POSITION_ARB, e);
			error = e[0];

			if (error != -1)
			{
				throw new Exception("");//Gl.glGetString(Gl.GL_PROGRAM_ERROR_STRING_ARB));
			}

			return fp;
		}

		public void DeleteFragmentProgram(FragmentProgram fp)
		{
            Gl.glDeleteProgramsARB(1, new int[] { fp.id });
			fp.id = -1;
		}

		protected void SetVertexProgramParameter(int index, float[] v4)
		{
			Gl.glProgramLocalParameter4fvARB(Gl.GL_VERTEX_PROGRAM_ARB, index, v4);
		}

		public void BindFragmentProgram(FragmentProgram fp)
		{
            //if (CompabilityMode)
            //    return;

			if (fp != null)
			{
				Gl.glBindProgramARB(Gl.GL_FRAGMENT_PROGRAM_ARB, fp.id);
				States.Enable(Gl.GL_FRAGMENT_PROGRAM_ARB);
			}
			else
			{
				States.Disable(Gl.GL_FRAGMENT_PROGRAM_ARB);
			}
		}

		public VertexProgram CreateVertexProgram(string code)
		{
			VertexProgram vp = new VertexProgram();

			int[] id=new int[1];
			Gl.glGenProgramsARB(1, id);
			vp.id=id[0];
			if (vp.id == 0)
				throw new Exception("cant create vertexprogram.");

			Gl.glBindProgramARB(Gl.GL_VERTEX_PROGRAM_ARB, vp.id);
			Gl.glProgramStringARB(Gl.GL_VERTEX_PROGRAM_ARB, Gl.GL_PROGRAM_FORMAT_ASCII_ARB,
				code.Length, code);

			int error;
			int[] e = new int[1];
			Gl.glGetIntegerv(Gl.GL_PROGRAM_ERROR_POSITION_ARB, e);
			error = e[0];

			if (error != -1)
			{
				throw new Exception("");//Gl.glGetS(Gl.GL_PROGRAM_ERROR_STRING_ARB));
			}
			return vp;
		}

		public void DeleteVertexProgram(VertexProgram vp)
		{
            //if (CompabilityMode)
            //    return;
            Gl.glDeleteProgramsARB(1, new int[] { vp.id });
			vp.id = -1;
		}

		public void BindVertexProgram(VertexProgram vp)
		{
            //if (CompabilityMode)
            //    return;
            if (vp != null)
			{
				Gl.glBindProgramARB(Gl.GL_VERTEX_PROGRAM_ARB, vp.id);
				States.Enable(Gl.GL_VERTEX_PROGRAM_ARB);
			}
			else
			{
				States.Disable(Gl.GL_VERTEX_PROGRAM_ARB);
			}
		}

		public void SetCamera(Camera c)
		{
            cam = c;

            if (c == null)
                return;

			Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadMatrixf((float[])c.GetProjectionMatrix());
			//Gl.glLoadIdentity();
			//Glu.gluPerspective(c.Fov, (float)width / (float)height, c.nearplane, c.farplane);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);

			Matrix3 m = c.Matrix;//Matrix3.FromQuaternion(c.Orientation);

			Vector3 t = new Vector3();
			Vector3 x, y;
			Vector3 pos = m.ExtractTranslation();
			m.ExtractBasis(out x, out y, out t);
			t += pos;


            if (c.Shake > 0)
            {
                t += c.Shake * VecRandom.Instance.NextUnitVector3();
                pos += c.Shake * VecRandom.Instance.NextUnitVector3();
            }

			Gl.glLoadIdentity();
			Glu.gluLookAt(pos.X, pos.Y, pos.Z, t.X, t.Y, t.Z, y.X, y.Y, y.Z);
            //Gl.glMultMatrixf((float[])c.GetViewMatrix());

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

            States.Enable(Gl.GL_SCISSOR_TEST);
            Gl.glScissor(vp.X, vp.Y, vp.W, vp.H);
            Gl.glViewport(vp.X, vp.Y, vp.W, vp.H);
		}

		public unsafe Bitmap Screenshot()
		{
			byte[] rgb = new byte[Size.X * Size.Y * 3];
			Gl.glReadPixels(0, 0, Size.X, Size.Y, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, rgb);
			//return new Image(Size.X, Size.Y, rgb, false);

            fixed(void *ptr=rgb)
            {
                Bitmap b= new Bitmap(Size.X, Size.Y, 3 * Size.X, PixelFormat.Format24bppRgb, new IntPtr(ptr));
                
                return b;
            }
		}
        public Cheetah.Graphics.Image Screenshot2()
        {
            byte[] rgb = new byte[Size.X * Size.Y * 3];
            Gl.glReadPixels(0, 0, Size.X, Size.Y, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, rgb);
            return new Cheetah.Graphics.Image(Size.X, Size.Y, rgb, false);
        }

		public void UpdateTexture(Cheetah.TextureId t, byte[] rgba)
		{
			TextureId t1 = (TextureId)t;
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, t1.id);
			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
			//Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, 3, t1.w, t1.h,0, Gl.GL_RGB,Gl.GL_UNSIGNED_BYTE,rgba);
			Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, 3, t1.w, t1.h, 0, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, rgba);
			//Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D,3,t1.w,t1.h,Gl.GL_RGB,Gl.GL_UNSIGNED_BYTE,rgba);
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
            int error=Gl.glGetError();
            if (error != Gl.GL_NO_ERROR)
            {
                throw new Exception(Glu.gluErrorString(error));
            }
        }

        public Cheetah.TextureId CreateCompressedCubeTexture(byte[] xpos, byte[] xneg, byte[] ypos, byte[] yneg, byte[] zpos, byte[] zneg, TextureFormat codec, int w, int h)
        {
            int format;
            switch (codec)
            {
                case TextureFormat.DXT1:
                    format = Gl.GL_COMPRESSED_RGB_S3TC_DXT1_EXT;
                    break;
                case TextureFormat.DXT2:
                    throw new Exception();
                case TextureFormat.DXT3:
                    format = Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
                    break;
                case TextureFormat.DXT4:
                    throw new Exception();
                case TextureFormat.DXT5:
                    format = Gl.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
                    break;
                default:
                    throw new Exception();
            }

            int[] i = new int[1];

            Gl.glGenTextures(1, i);

            TextureId t = new TextureId(i[0], this, w, h, false, true);

            Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, t.id);
            CheckError();
            Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            CheckError();

            Gl.glCompressedTexImage2DARB(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_X_ARB, 0, format, w, h, 0, xpos.Length, xpos);
            CheckError();
            Gl.glCompressedTexImage2DARB(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_X_ARB, 0, format, w, h, 0, xneg.Length, xneg);
            CheckError();
            Gl.glCompressedTexImage2DARB(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Y_ARB, 0, format, w, h, 0, ypos.Length, ypos);
            CheckError();
            Gl.glCompressedTexImage2DARB(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y_ARB, 0, format, w, h, 0, yneg.Length, yneg);
            CheckError();
            Gl.glCompressedTexImage2DARB(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Z_ARB, 0, format, w, h, 0, zpos.Length, zpos);
            CheckError();
            Gl.glCompressedTexImage2DARB(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z_ARB, 0, format, w, h, 0, zneg.Length, zneg);


            //Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, 4, w, h, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, data);

            Textures[t.id] = t;

            return t;
        }

        public Cheetah.TextureId CreateCubeTexture(byte[] xpos,byte[] xneg,byte[] ypos,byte[] yneg,byte[] zpos,byte[] zneg, 
            int w, int h)
        {
            int[] i = new int[1];

            Gl.glGenTextures(1, i);

            TextureId t = new TextureId(i[0], this, w, h, false, true);

            Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, t.id);
            CheckError();

            //Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
            //Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);

            //Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
            //Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);

            Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            CheckError();

            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_X_ARB, 0, 3, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, xpos);
            CheckError();
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_X_ARB, 0, 3, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, xneg);
            CheckError();
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Y_ARB, 0, 3, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, ypos);
            CheckError();
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y_ARB, 0, 3, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, yneg);
            CheckError();
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Z_ARB, 0, 3, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, zpos);
            CheckError();
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z_ARB, 0, 3, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, zneg);
            
            //Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, 4, w, h, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, data);

            Textures[t.id] = t;

            return t;
        }

        public Cheetah.TextureId CreateCompressedTexture(byte[][] mipmaps, TextureFormat codec,int w, int h)
        {
            int[] i = new int[1];
            Gl.glGenTextures(1, i);
            TextureId t = new TextureId(i[0], this, w, h, true, false);
            t.LastBind = Root.Instance.Time;

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, t.id);

            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);

            if (mipmaps.Length > 1)
            {
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            }
            else
            {
                //disable mipmapping
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            }

            int format;
            switch (codec)
            {
                case TextureFormat.DXT1:
                    format = Gl.GL_COMPRESSED_RGB_S3TC_DXT1_EXT;
                    break;
                case TextureFormat.DXT2:
                    throw new Exception();
                case TextureFormat.DXT3:
                    format = Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
                    break;
                case TextureFormat.DXT4:
                    throw new Exception();
                case TextureFormat.DXT5:
                    format = Gl.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
                    break;
                default:
                    throw new Exception("unknown codec: "+codec.ToString());
            }

            for (int m = 0; m < mipmaps.Length; ++m)
            {
                Gl.glCompressedTexImage2DARB(Gl.GL_TEXTURE_2D, m, format, w, h, 0, mipmaps[m].Length, mipmaps[m]);
                w /= 2;
                h /= 2;
            }

            Textures[t.id] = t;

            return t;
        }

        public Cheetah.TextureId CreateDepthTexture(int w, int h)
        {
            int[] i = new int[1];
            Gl.glGenTextures(1, i);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, i[0]);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_DEPTH_COMPONENT32, w, h, 0, Gl.GL_DEPTH_COMPONENT, Gl.GL_FLOAT, IntPtr.Zero);
            TextureId t = new TextureId(i[0], this, w, h, false, false);
            Textures[t.id] = t;
            return t;
        }

		public Cheetah.TextureId CreateTexture(byte[] rgba, int w, int h, bool alpha)
		{
            bool mipmap = IsPowerOf2(w) && IsPowerOf2(h);
			//if (!(IsPowerOf2(w) && IsPowerOf2(h)))
			//	throw new Exception("Texture sizes must be n^2.");
			int[] i = new int[1];
			byte[] data = rgba;
			Gl.glGenTextures(1, i);
			//if(Gl.glIsTexture(i[0])!=Gl.GL_TRUE)
			//	throw new Exception("OpenGL.CreateTexture: glGenTextures failed.");

			TextureId t = new TextureId(i[0], this, w, h, alpha, false);
            t.LastBind = Root.Instance.Time;

			Gl.glBindTexture(Gl.GL_TEXTURE_2D, t.id);

			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);

            //Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
            //Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);

			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            if(mipmap)
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            else
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);

            //Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_NEAREST);
            //Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);

			/*
				if(!(IsPowerOf2(w)&&IsPowerOf2(h)))
						{
							int s=CalcGoodSize(w,h);
							data=new byte[s*s*(alpha?4:3)];
							if(alpha)
								Glu.gluScaleImage(Gl.GL_RGBA,w,h,Gl.GL_UNSIGNED_BYTE,rgba,s,s,Gl.GL_UNSIGNED_BYTE,data);
							else
								Glu.gluScaleImage(Gl.GL_RGB,w,h,Gl.GL_UNSIGNED_BYTE,rgba,s,s,Gl.GL_UNSIGNED_BYTE,data);
						}
			*/
            if (mipmap)
            {
                if (alpha)
                    Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, 4, w, h, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, data);
                else
                    Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, 3, w, h, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, data);
            }
            else
            {
                if (alpha)
                    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, 4, w, h, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, data);
                 else
                    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, 3, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, data);
            }
			//t.width=w;
			//t.height=h;
			Textures[t.id] = t;

			return t;
		}

        public Cheetah.TextureId CreateTexture(int w, int h, bool alpha, bool depth)
        {
            bool mipmap = IsPowerOf2(w) && IsPowerOf2(h);
            //if (!(IsPowerOf2(w) && IsPowerOf2(h)))
            //	throw new Exception("Texture sizes must be n^2.");
            int[] i = new int[1];
            //byte[] data = rgba;
            Gl.glGenTextures(1, i);
            //if(Gl.glIsTexture(i[0])!=Gl.GL_TRUE)
            //	throw new Exception("OpenGL.CreateTexture: glGenTextures failed.");

            TextureId t = new TextureId(i[0], this, w, h, alpha, false);
            t.LastBind = Root.Instance.Time;

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, t.id);

            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);

            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);

            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            if (mipmap)
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            else
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);

            if (depth)
                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_DEPTH_COMPONENT, w, h, 0, Gl.GL_DEPTH_COMPONENT, Gl.GL_UNSIGNED_SHORT, IntPtr.Zero);
            else
                //Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_COLOR, w, h, 0, Gl.GL_DEPTH_COMPONENT, Gl.GL_UNSIGNED_SHORT, IntPtr.Zero);
                throw new Exception();

            if (Gl.glGetError() != Gl.GL_NO_ERROR)
                throw new Exception();
            /*
            if (alpha)
                Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, 4, w, h, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, data);
            else
                Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, 3, w, h, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, data);
            */
            //t.width=w;
            //t.height=h;
            Textures[t.id] = t;

            return t;
        }

		public void BindTexture(Cheetah.TextureId t, int unit)
		{
			unit += Gl.GL_TEXTURE0;


			if (t != null)
			{
                t.LastBind = Root.Instance.Time;
                TextureId t2 = (TextureId)t;

                States.ActiveTexture(unit);
                //States.Disable(Gl.GL_TEXTURE_2D);

                int x = t2.cube ? Gl.GL_TEXTURE_CUBE_MAP_ARB : Gl.GL_TEXTURE_2D;
				States.Enable(x);
				Gl.glBindTexture(x, t2.id);
			}
			else
			{
                States.ActiveTexture(unit);
				//Gl.glBindTexture(Gl.GL_TEXTURE_2D,-1);
				States.Disable(Gl.GL_TEXTURE_2D);
                States.Disable(Gl.GL_TEXTURE_CUBE_MAP_ARB);
            }
            States.ActiveTexture(Gl.GL_TEXTURE0);


		}

		public void BindTexture(Cheetah.TextureId t)
		{
            BindTexture(t, 0);
			/*if (t != null)
			{
                t.LastBind = Root.Instance.Time;
                States.Enable(Gl.GL_TEXTURE_2D);
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, ((TextureId)t).id);
			}
			else
			{
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, -1);
				States.Disable(Gl.GL_TEXTURE_2D);
			}*/
		}
		public void FreeTexture(Cheetah.TextureId t)
		{
			int[] i = new int[] { ((TextureId)t).id };
			Gl.glDeleteTextures(1, i);
			Textures.Remove(i[0]);
		}

		public void Clear(float r, float g, float b, float a)
		{
            //Gl.glViewport(0, 0, width, height);
            //Gl.glScissor(0, 0, width, height);
            Gl.glClearColor(r, g, b, a);
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
		}

		public void SetMaterial(Material m)
		{
			if (m != null)
            {
            //    if (CompabilityMode)
            //    {
            //        int stage = 0;

            //        States.ActiveTexture(stage + Gl.GL_TEXTURE0);

            //        if (m.diffusemap != null)
            //        {
            //            BindTexture(m.diffusemap.Id);
            //            Gl.glMatrixMode(Gl.GL_TEXTURE);
            //            Gl.glLoadIdentity();
            //            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            //            States.Disable(Gl.GL_TEXTURE_GEN_S);
            //            States.Disable(Gl.GL_TEXTURE_GEN_T);
            //            stage++;
            //        }
            //        else
            //            BindTexture(null);

            //        States.ActiveTexture(stage + Gl.GL_TEXTURE0);

            //        if (m.DetailMap != null)
            //        {
            //            BindTexture(m.DetailMap.Id);
            //            Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
            //            Gl.glMatrixMode(Gl.GL_TEXTURE);
            //            Gl.glLoadIdentity();
            //            Gl.glScalef(256, 256, 256);
            //            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            //            States.Disable(Gl.GL_TEXTURE_GEN_S);
            //            States.Disable(Gl.GL_TEXTURE_GEN_T);
            //            stage++;
            //        }
            //        else
            //            BindTexture(null);

            //        States.ActiveTexture(stage + Gl.GL_TEXTURE0);

            //        if (m.EnvironmentMap != null)
            //        {
            //            BindTexture(m.EnvironmentMap.Id);
            //            Gl.glMatrixMode(Gl.GL_TEXTURE);
            //            Gl.glLoadIdentity();
            //            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            //            Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
            //            Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);//GL_SPHERE_MAP);
            //            Gl.glTexGeni(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);//GL_SPHERE_MAP);
            //            States.Enable(Gl.GL_TEXTURE_GEN_S);
            //            States.Enable(Gl.GL_TEXTURE_GEN_T);
            //            stage++;
            //        }
            //        else
            //            BindTexture(null);

            //        for (int i = stage; i < 4; ++i)
            //            BindTexture(null, i);

            //        if (stage > 0)
            //            States.Enable(Gl.GL_TEXTURE_2D);
            //        else
            //            States.Disable(Gl.GL_TEXTURE_2D);

            //        Gl.glColor3f(1, 1, 1);
            //    //Gl.glActiveTextureARB(glActiveTextureARB,Gl.GL_TEXTURE1);
            //        States.ActiveTexture(Gl.GL_TEXTURE0);

            //    if (m.NoLighting)
            //        States.Disable(Gl.GL_LIGHTING);
            //    else
            //        States.Enable(Gl.GL_LIGHTING);

            //}






				int face = Gl.GL_FRONT_AND_BACK;
				Gl.glMaterialfv(face, Gl.GL_SPECULAR, (float[])m.specular);

				Gl.glMaterialfv(face, Gl.GL_DIFFUSE, (float[])m.diffuse);

				Gl.glMaterialfv(face, Gl.GL_AMBIENT, (float[])m.ambient);

				Gl.glMaterialf(face, Gl.GL_SHININESS, m.shininess);

				if (m.wire || WireFrameMode)
					Gl.glPolygonMode(face, Gl.GL_LINE);
				else
					Gl.glPolygonMode(face, Gl.GL_FILL);

				if (m.twosided)
					States.Disable(Gl.GL_CULL_FACE);
				else
					States.Enable(Gl.GL_CULL_FACE);

                if (m.DepthTest)
                    States.Enable(Gl.GL_DEPTH_TEST);
                else
                    States.Disable(Gl.GL_DEPTH_TEST);
                Gl.glDepthMask(m.DepthWrite?1:0);

                if (m.Additive)
				{
					//Gl.glBlendFunc(Gl.GL_ONE, Gl.GL_ONE);
                    Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
                    //States.Disable(Gl.GL_DEPTH_TEST);
					States.Enable(Gl.GL_BLEND);
                    Gl.glDepthMask(0);
				}
				else
				{
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                    //States.Disable(Gl.GL_BLEND);
				}
			}
			else
			{
                States.ActiveTexture(Gl.GL_TEXTURE0);
				BindTexture(null);
				Gl.glMatrixMode(Gl.GL_TEXTURE);
				Gl.glLoadIdentity();
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				States.Disable(Gl.GL_TEXTURE_GEN_S);
				States.Disable(Gl.GL_TEXTURE_GEN_T);
                Gl.glDepthMask(1);
            }
		}

		public void SetLighting(bool b)
		{
			if (b)
				States.Enable(Gl.GL_LIGHTING);
			else
				States.Disable(Gl.GL_LIGHTING);
		}

		public Cheetah.Graphics.VertexBuffer CreateStaticVertexBuffer(object data, int length)
		{
			//if (SlowVertexBuffers || CompabilityMode)
			//	return CreateSlowVertexBuffer(data, length);
			VertexBuffer vb = new VertexBuffer();
            int[] id = new int[1];
			Gl.glGenBuffersARB(1, id);
            vb.id = id[0];
			Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, vb.id);

			Type t = data.GetType();
			if (!t.IsArray)
				throw new Exception("wrong datatype.");

			//Array a = (Array)data;
			//IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);


			//fixed(void *ptr=data)
			//{
			Gl.glBufferDataARB(Gl.GL_ARRAY_BUFFER_ARB, new IntPtr(length), data, Gl.GL_STATIC_DRAW_ARB);
			//}
			Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, 0);
			vb.Size = length;
			Buffers[vb.id] = vb;
			BufferMemory += length;
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

			DynamicVertexBuffer vb = new DynamicVertexBuffer(this);
			vb.Size = length;
            int[] id = new int[1];
			Gl.glGenBuffersARB(1, id);
            vb.id = id[0];
			Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, vb.id);
			Gl.glBufferDataARB(Gl.GL_ARRAY_BUFFER_ARB, new IntPtr(length), new byte[length], Gl.GL_DYNAMIC_DRAW_ARB);
			Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, 0);
			Buffers[vb.id] = vb;
			BufferMemory += length;
			return vb;
		}

        public void FreeVertexBuffer(Cheetah.Graphics.VertexBuffer b)
		{
			FreeQueue.Enqueue(b);
		}

		public void SetLight(int index, Light l)
        {
            float[] f = new float[4];
			if (l != null)
			{

				//glPushMatrix();
				//glLoadIdentity();

                Vector3 pos = l.AbsolutePosition;
				f[0] = pos.X;
                f[1] = pos.Y;
                f[2] = pos.Z;
				f[3] = l.directional ? 0 : 1;


				Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_POSITION, f);
				Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_AMBIENT, (float[])l.ambient);
				Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_DIFFUSE, (float[])l.diffuse);
				Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_SPECULAR, (float[])l.specular);
				Gl.glLightf(Gl.GL_LIGHT0 + index, Gl.GL_CONSTANT_ATTENUATION, l.attenuation.X);
				Gl.glLightf(Gl.GL_LIGHT0 + index, Gl.GL_LINEAR_ATTENUATION, l.attenuation.Y);
				Gl.glLightf(Gl.GL_LIGHT0 + index, Gl.GL_QUADRATIC_ATTENUATION, l.attenuation.Z);

				States.Enable(Gl.GL_LIGHT0 + index);
			}
			else
			{
                Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_AMBIENT, f);
                Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_DIFFUSE, f);
                Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_SPECULAR, f);
                States.Disable(Gl.GL_LIGHT0 + index);
			}
		}

        public void SetPointSize(float s)
        {
            pointsize = s;
        }
        float pointsize;

		public void SetMode(RenderMode m)
		{
			switch (m)
			{
				case RenderMode.Draw2D:
					States.Disable(Gl.GL_POINT_SPRITE_ARB);
					Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
					Gl.glMatrixMode(Gl.GL_PROJECTION);
					Gl.glLoadIdentity();
					Glu.gluOrtho2D(0.0, (double)width, (double)height, 0.0);
					Gl.glMatrixMode(Gl.GL_MODELVIEW);
					Gl.glLoadIdentity();
					States.Disable(Gl.GL_DEPTH_TEST);
					States.Disable(Gl.GL_CULL_FACE);
					States.Disable(Gl.GL_LIGHTING);
					Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
					States.Enable(Gl.GL_BLEND);
                    break;
				case RenderMode.Draw3D:
					Gl.glDepthMask(Gl.GL_TRUE);
					States.Disable(Gl.GL_POINT_SPRITE_ARB);
					Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
                    States.ActiveTexture(Gl.GL_TEXTURE0);
					States.Enable(Gl.GL_DEPTH_TEST);
					States.Enable(Gl.GL_CULL_FACE);
					Gl.glCullFace(Gl.GL_BACK);
					Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
					States.Enable(Gl.GL_BLEND);
					Gl.glLightModeli(Gl.GL_LIGHT_MODEL_TWO_SIDE, Gl.GL_TRUE);
					break;
				case RenderMode.Draw3DWireFrame:
                    Gl.glDepthMask(Gl.GL_TRUE);
                    States.Disable(Gl.GL_POINT_SPRITE_ARB);
                    States.ActiveTexture(Gl.GL_TEXTURE0);
                    States.Enable(Gl.GL_DEPTH_TEST);
                    States.Enable(Gl.GL_CULL_FACE);
                    Gl.glCullFace(Gl.GL_BACK);
                    Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                    States.Enable(Gl.GL_BLEND);
                    Gl.glLightModeli(Gl.GL_LIGHT_MODEL_TWO_SIDE, Gl.GL_TRUE);
                    Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
					break;
				case RenderMode.Draw3DPointSprite:
                    States.ActiveTexture(Gl.GL_TEXTURE0);
                    Gl.glTexEnvi(Gl.GL_POINT_SPRITE_ARB, Gl.GL_COORD_REPLACE_ARB, Gl.GL_TRUE);
                    States.Enable(Gl.GL_POINT_SPRITE_ARB);
                    Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_POINT);
                    UseShader(pointsprite);
                    float m1 = 2.0f * (float)Math.Atan(cam.Fov / 2.0 / 180.0 * Math.PI);
                    int[] vp = new int[4];
                    Gl.glGetIntegerv(Gl.GL_VIEWPORT, vp);
                    float v = (float)vp[3];
                    m1 /= v;
                    SetUniform(pointsprite.GetUniformLocation("WorldSize"), new float[] { pointsize });
                    SetUniform(pointsprite.GetUniformLocation("Attenuation"), new float[] { 0,0,m1 });
                    break;
				case RenderMode.DrawSkyBox:
                    States.Disable(Gl.GL_POINT_SPRITE_ARB);
                    Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
                    States.ActiveTexture(Gl.GL_TEXTURE0);
                    States.Enable(Gl.GL_CULL_FACE);
                    Gl.glCullFace(Gl.GL_BACK);
                    Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                    States.Enable(Gl.GL_BLEND);
                    Gl.glLightModeli(Gl.GL_LIGHT_MODEL_TWO_SIDE, Gl.GL_TRUE);
                    States.Disable(Gl.GL_DEPTH_TEST);
					//Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT);
					Gl.glDepthMask(Gl.GL_FALSE);

					break;
				default:
					throw new Exception("dont know rendermode " + m.ToString());
			}
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
						Gl.glDeleteBuffersARB(1, new int[] { vb.id });
						Buffers.Remove(vb.id);
						vb.id = -1;
						BufferMemory -= vb.Size;
					}
					else if(b is DynamicVertexBuffer)
					{
						DynamicVertexBuffer vb = (DynamicVertexBuffer)b;
						Gl.glDeleteBuffersARB(1, new int[] { vb.id });
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
			//DrawContext c=(DrawContext)context;
			//Type t=vertices.data.GetType();
			//if(!t.IsArray)
			//	throw new Exception("wrong datatype.");
            //lastformat = null;
            //lastbuffer = null;
            //lastshader = null;

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
						datatype = Gl.GL_FLOAT;
						break;
					case VertexFormat.ElementType.Byte:
						datatype = Gl.GL_BYTE;
						break;
					case VertexFormat.ElementType.Int:
						datatype = Gl.GL_INT;
						break;
					default:
						throw new Exception();
				}

				switch (e.Name)
				{
					case VertexFormat.ElementName.Color:
						Gl.glColorPointer(e.Count, datatype, vertexsize, (IntPtr)(buffer + offset2));
						States.EnableClientState(Gl.GL_COLOR_ARRAY);
						break;
					case VertexFormat.ElementName.Position:
						Gl.glVertexPointer(e.Count, datatype, vertexsize, (IntPtr)(buffer + offset2));
                        States.EnableClientState(Gl.GL_VERTEX_ARRAY);
						break;
					case VertexFormat.ElementName.Texture0:
						Gl.glTexCoordPointer(e.Count, datatype, vertexsize, (IntPtr)(buffer + offset2));
                        States.EnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
						break;
					case VertexFormat.ElementName.Normal:
						Gl.glNormalPointer(datatype, vertexsize, (IntPtr)(buffer + offset2));
                        States.EnableClientState(Gl.GL_NORMAL_ARRAY);
						break;
					default:
						throw new Exception();
				}

				offset2 += elementsize;
			}

			int gltype;
			switch (type)
			{
				case PrimitiveType.QUADS:
					gltype = Gl.GL_QUADS;
					break;
				case PrimitiveType.TRIANGLESTRIP:
					gltype = Gl.GL_TRIANGLE_STRIP;
					break;
				case PrimitiveType.TRIANGLES:
					gltype = Gl.GL_TRIANGLES;
					break;
				case PrimitiveType.LINES:
					gltype = Gl.GL_LINES;
					break;
				case PrimitiveType.LINESTRIP:
					gltype = Gl.GL_LINE_STRIP;
					break;
				case PrimitiveType.POINTS:
					gltype = Gl.GL_POINTS;
					break;
				default:
					throw new Exception();
			}

			if (ib == null)
			{
				Gl.glDrawArrays(gltype, offset, count);
			}
			else
			{
				Gl.glDrawElements(gltype, count, Gl.GL_UNSIGNED_INT, ib.buffer);
			}
			States.DisableClientState(Gl.GL_COLOR_ARRAY);
            States.DisableClientState(Gl.GL_VERTEX_ARRAY);
            States.DisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            States.DisableClientState(Gl.GL_NORMAL_ARRAY);
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

			VertexFormat format;
			if (vertices is VertexBuffer)
			{
				VertexBuffer vb = (VertexBuffer)vertices;
				format = vertices.Format;
				Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, vb.id);
			}
			else if (vertices is DynamicVertexBuffer)
			{
				DynamicVertexBuffer vb = (DynamicVertexBuffer)vertices;
				format = vertices.Format;
				Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, vb.id);
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
                            datatype = Gl.GL_FLOAT;
                            break;
                        case VertexFormat.ElementType.Byte:
                            datatype = Gl.GL_BYTE;
                            break;
                        case VertexFormat.ElementType.Int:
                            datatype = Gl.GL_INT;
                            break;
                        default:
                            throw new Exception("unknown vertexformat element.");
                    }

                    int startwithoffset = start;
                    if (ib != null)
                        startwithoffset += offset * format.Size;

                    switch (e.Name)
                    {
                        case VertexFormat.ElementName.Color:
                            Gl.glColorPointer(e.Count, datatype, vertexsize, new IntPtr(startwithoffset));
                            GL_COLOR_ARRAY = true;
                            //States.EnableClientState(Gl.GL_COLOR_ARRAY);
                            break;
                        case VertexFormat.ElementName.Position:
                            Gl.glVertexPointer(e.Count, datatype, vertexsize, new IntPtr(startwithoffset));
                            GL_VERTEX_ARRAY = true;
                            //States.EnableClientState(Gl.GL_VERTEX_ARRAY);
                            break;
                        case VertexFormat.ElementName.Texture0:
                            GL_TEXTURE_COORD_ARRAY = true;
                            States.ClientActiveTexture(Gl.GL_TEXTURE0);
                            Gl.glTexCoordPointer(e.Count, datatype, vertexsize, new IntPtr(startwithoffset));
                            States.EnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                            /*States.ClientActiveTexture(Gl.GL_TEXTURE1);
                            Gl.glTexCoordPointer(e.Count, datatype, vertexsize, new IntPtr(startwithoffset));
                            States.EnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                            States.ClientActiveTexture(Gl.GL_TEXTURE0);*/
                            break;
                        case VertexFormat.ElementName.Normal:
                            Gl.glNormalPointer(datatype, vertexsize, new IntPtr(startwithoffset));
                            GL_NORMAL_ARRAY = true;
                            //Gl.glVertexAttribPointer(14, 3, Gl.GL_FLOAT, false, vertexsize, new IntPtr(start));
                            //Gl.glEnableVertexAttribArray(14);
                            break;
                        case VertexFormat.ElementName.Tangent:
                            if (CurrentShader != null)
                            {
                                int loc = CurrentShader.GetAttributeLocation("tangent");
                                if (loc >= 0)
                                {
                                    Gl.glVertexAttribPointer(loc, e.Count, Gl.GL_FLOAT, 0, vertexsize, new IntPtr(startwithoffset));
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
                                    Gl.glVertexAttribPointer(loc, e.Count, Gl.GL_FLOAT, 0, vertexsize, new IntPtr(startwithoffset));
                                    attribs[loc] = true;
                                }
                            }
                            break;
                        case VertexFormat.ElementName.Texture1:
                            States.ClientActiveTexture(Gl.GL_TEXTURE1);
                            Gl.glTexCoordPointer(e.Count, datatype, vertexsize, new IntPtr(startwithoffset));
                            GL_TEXTURE_COORD_ARRAY = true;
                            States.EnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                            States.ClientActiveTexture(Gl.GL_TEXTURE0);
                            break;
                        /*case VertexFormat.ElementName.None:
                            if (CurrentShader == null)
                                throw new Exception("");
                            int loc=CurrentShader.GetAttributeLocation(e.Attrib);
                            Gl.glVertexAttribPointer(loc, e.Count, Gl.GL_FLOAT, false, vertexsize, new IntPtr(start));
                            Gl.glEnableVertexAttribArray(loc);
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
                            Gl.glVertexAttribPointer(loc, e.Count, Gl.GL_FLOAT, 0, vertexsize, new IntPtr(startwithoffset));
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
                    States.EnableClientState(Gl.GL_COLOR_ARRAY);
                else
                    States.DisableClientState(Gl.GL_COLOR_ARRAY);
                if (GL_VERTEX_ARRAY)
                    States.EnableClientState(Gl.GL_VERTEX_ARRAY);
                else
                    States.DisableClientState(Gl.GL_VERTEX_ARRAY);
                if (GL_NORMAL_ARRAY)
                    States.EnableClientState(Gl.GL_NORMAL_ARRAY);
                else
                    States.DisableClientState(Gl.GL_NORMAL_ARRAY);

                /*
                States.ClientActiveTexture(Gl.GL_TEXTURE1);
                if (GL_TEXTURE_COORD_ARRAY)
                    States.EnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                else
                    States.DisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                States.ClientActiveTexture(Gl.GL_TEXTURE0);
                if (GL_TEXTURE_COORD_ARRAY)
                    States.EnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                else
                    States.DisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                */
                //lastformat = format;
                //lastbuffer = vertices;
                //lastshader = CurrentShader;
            }

			int gltype;
			switch (type)
			{
				case PrimitiveType.QUADS:
					gltype = Gl.GL_QUADS;
					break;
				case PrimitiveType.TRIANGLESTRIP:
					gltype = Gl.GL_TRIANGLE_STRIP;
					break;
				case PrimitiveType.TRIANGLES:
					gltype = Gl.GL_TRIANGLES;
					break;
				case PrimitiveType.LINES:
					gltype = Gl.GL_LINES;
					break;
				case PrimitiveType.LINESTRIP:
					gltype = Gl.GL_LINE_STRIP;
					break;
				case PrimitiveType.POINTS:
					gltype = Gl.GL_POINTS;
					break;
				default:
					throw new Exception("unknown primitive type.");
			}

			if (ib == null)
			{
				Gl.glDrawArrays(gltype, offset, count);
			}
			else
			{
				//Gl.glDrawElements(gltype, count, Gl.GL_UNSIGNED_INT, ib.buffer);
                //fixed (int* startindex = ib.buffer)
                IntPtr startindex = Marshal.UnsafeAddrOfPinnedArrayElement(ib.buffer, indexoffset);
                {
                    //Gl.glDrawElements(gltype, count, Gl.GL_UNSIGNED_INT, new IntPtr(startindex+indexoffset));
                    Gl.glDrawElements(gltype, count, Gl.GL_UNSIGNED_INT, startindex);
                }
            }

           
			
            //for(int i=0;i<16;++i)
             //   Gl.glDisableVertexAttribArray(i);
            
			//Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, 0);
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
