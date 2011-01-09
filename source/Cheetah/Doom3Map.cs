using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Cheetah.Graphics;
using OpenTK;

namespace Cheetah.Doom3
{
	public class Doom3Parser
	{
		public Doom3Parser(Stream s)
		{
			using (StreamReader reader = new StreamReader(s))
			{
				Text = reader.ReadToEnd();
			}
			Pos = 0;
		}

		char Read()
		{
			if (Pos >= Text.Length)
				throw new Exception("file end");
			else
				return Text[Pos++];
		}

		char Peek()
		{
			return Text[Pos];
		}

		bool IsSpecialChar(char c)
		{
			return IsWhiteSpace(c) || IsToken(c);
		}
		bool IsWhiteSpace(char c)
		{
			return c == ' ' || c == '\t' || c == '\n' || c == '\r';
		}

		bool IsToken(char c)
		{
			return c == '{' || c == '}' || c == '(' || c == ')' || c == '/' || c == '*' || c == '"';
		}

		bool IsCommentKeyword(string s)
		{
			return s == "/*" || s == "*/";
		}

		public int ReadInt()
		{
			return int.Parse(ReadToken());
		}
		public float ReadFloat()
		{
			return float.Parse(ReadToken());
		}
		public void ReadToken(string s)
		{
			if (s != ReadToken())
				throw new Exception("");
		}

		public string ReadToken()
		{
			string token="";
			bool ready = false;
			do
			{
				char p = Peek();

				if (String)
				{
					if (p == '"')
					{
						Read();
						String = false;
						return token;
					}
					else
						token += Read();
				}
				else if (IsSpecialChar(p))
				{
					if (token.Length > 0)
					{
						ready = true;
					}
					else if (IsWhiteSpace(p))
					{
						Read();
					}
					else if (IsToken(p))
					{
						token += Read();
						ready = true;
					}
				}
				else
				{
					token += Read();
				}
			}
			while (!ready);

			if (token == "/" && Peek() == '*')
			{
				Read();
				while (!(Read() == '*' && Peek() == '/')) ;
				Read();
				return ReadToken();
			}
			else if (token == "\"")
			{
				String = true;
				return ReadToken();
			}

			return token;
		}

		string Text;
		int Pos;
		bool String = false;
		//Stream stream;
		//StreamReader reader;
	}

	public class Doom3MapLoader : IResourceLoader
	{
		public IResource Load(FileSystemNode n)
		{
			Doom3MapData d = new Doom3MapData();
			d.Load(n.getStream());
			Doom3Map m = new Doom3Map(d);
			return m;
		}

		public Type LoadType
		{
			get { return typeof(Doom3Map); }
		}

		public bool CanLoad(FileSystemNode n)
		{
			return n.info != null && (n.info.Extension.ToLower() == ".proc");
		}
	}

	public class Doom3Map : IDrawable, IResource
	{
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public struct Surface
		{
			public VertexBuffer Vbuffer;
			public IndexBuffer Ibuffer;
			public Material Material;
		}

		public Doom3Map(Doom3MapData map)
		{
			Surfaces=new List<Surface>();
			int verts = 0;
			int indices = 0;
			foreach(Doom3MapData.Model m in map.Models)
			{
				foreach(Doom3MapData.Surface s in m.Surfaces)
				{
					VertexBuffer vb = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(
						s.Vertices,
						s.Vertices.Length*4*(3+2+3)
						);
					vb.Format = VertexFormat.VF_P3T2N3;

					IndexBuffer ib = new IndexBuffer();
					ib.buffer = new int[s.Indices.Length];
					for (int i = 0; i < ib.buffer.Length; ++i)
						ib.buffer[i] = s.Indices[i];

					Surface s2 = new Surface();
					s2.Ibuffer = ib;
					s2.Vbuffer = vb;
					verts += s.Vertices.Length;
					indices += s.Indices.Length;
					Texture t=null;
					try
					{
						t = Root.Instance.ResourceManager.LoadTexture(s.Name + ".tga");
					}
					catch (Exception)
					{
						try
						{
							t = Root.Instance.ResourceManager.LoadTexture(s.Name + "_add.tga");
						}
						catch (Exception)
						{
							try
							{
								t = Root.Instance.ResourceManager.LoadTexture(s.Name + "_d.tga");
							}
							catch (Exception )
							{
								System.Console.WriteLine("warning: cant load "+s.Name);
							}
						}
					}
					s2.Material=Material.CreateSimpleMaterial(t);
					//s2.Material.Additive = true;
					Surfaces.Add(s2);
				}
			}
			System.Console.WriteLine("surfaces: "+Surfaces.Count.ToString());
			System.Console.WriteLine("verts: " + verts.ToString());
			System.Console.WriteLine("indices: " + indices.ToString());

			GC.Collect();
			BBox = new BoundingBox(map.BBoxMin, map.BBoxMax);
		}

        public void Draw(IRenderer r, Node n)
		{
			r.SetMode(RenderMode.Draw3D);
			foreach (Surface s in Surfaces)
			{
				r.SetMaterial(s.Material);
				r.Draw(s.Vbuffer, PrimitiveType.TRIANGLES, 0, s.Ibuffer.buffer.Length, s.Ibuffer);
			}
		}

		List<Surface> Surfaces;

		public void Dispose()
		{
		}

		public BoundingBox BBox;
	}

	public class Doom3MapData
	{
		public class Model
		{
			public String Name;
			public Surface[] Surfaces;
		}
		public class Surface
		{
			public string Name;
			public Vertex[] Vertices;
			public int[] Indices;
		}
		public struct Vertex
		{
			public Vector3 Position;
			public Vector2 TexCoord;
			public Vector3 Normal;
		}

		public void Load(Stream s)
		{
			p = new Doom3Parser(s);

			p.ReadToken("mapProcFile003");

			Models = new List<Model>();

			while (true)
			{
				string what;
				try
				{
					what = p.ReadToken();
				}
				catch (Exception)
				{
					break;
				}
				switch (what)
				{
					case "model":
						Models.Add(ReadModel());
						break;
					case "nodes":
						ReadNodes();
						break;
					case "interAreaPortals":
						ReadPortals();
						break;
					case "shadowModel":
						ReadShadowModel();
						break;
					default:
						throw new Exception("dont know " + what);
				}
			}
			p = null;
		}

		void ReadNode()
		{
			p.ReadToken("(");
			float a = p.ReadFloat();
			float b = p.ReadFloat();
			float c = p.ReadFloat();
			float d = p.ReadFloat();
			p.ReadToken(")");
			int poschild = p.ReadInt();
			int negchild = p.ReadInt();
		}

		void ReadNodes()
		{
			p.ReadToken("{");
			int numnodes = p.ReadInt();
			for (int i = 0; i < numnodes; ++i)
				ReadNode();
			p.ReadToken("}");
		}

		void ReadPoint()
		{
			p.ReadToken("(");
			float x = p.ReadFloat();
			float y = p.ReadFloat();
			float z = p.ReadFloat();
			p.ReadToken(")");
		}

		void ReadIAP()
		{
			int numpoints = p.ReadInt();
			int posarea = p.ReadInt();
			int negarea = p.ReadInt();
			for (int i = 0; i < numpoints; ++i)
				ReadPoint();
		}

		void ReadPortals()
		{
			p.ReadToken("{");

			int areacount = p.ReadInt();
			int iapcount = p.ReadInt();
			for (int i = 0; i < iapcount; ++i)
				ReadIAP();

			p.ReadToken("}");
		}

		void ReadShadowModel()
		{
			p.ReadToken("{");
			string name = p.ReadToken();
			int vertexcount = p.ReadInt();
			int nocaps = p.ReadInt();
			int nofrontcaps = p.ReadInt();
			int numindexes = p.ReadInt();
			int planebits = p.ReadInt();
			for (int i = 0; i < vertexcount; ++i)
				ReadPoint();
			for (int i = 0; i < numindexes; ++i)
				p.ReadInt();
			p.ReadToken("}");
		}

		Vertex ReadVertex()
		{
			p.ReadToken("(");

			Vertex v;
			v.Position.X = p.ReadFloat();
			v.Position.Y = p.ReadFloat();
			v.Position.Z = p.ReadFloat();
			v.TexCoord.X = p.ReadFloat();
			v.TexCoord.Y = p.ReadFloat();
			v.Normal.X = p.ReadFloat();
			v.Normal.Y = p.ReadFloat();
			v.Normal.Z = p.ReadFloat();

			BBoxMax.X = Math.Max(BBoxMax.X, v.Position.X);
			BBoxMax.Y = Math.Max(BBoxMax.Y, v.Position.Y);
			BBoxMax.Z = Math.Max(BBoxMax.Z, v.Position.Z);
			BBoxMin.X = Math.Min(BBoxMin.X, v.Position.X);
			BBoxMin.Y = Math.Min(BBoxMin.Y, v.Position.Y);
			BBoxMin.Z = Math.Min(BBoxMin.Z, v.Position.Z);

			p.ReadToken(")");

			return v;
		}

		Surface ReadSurface()
		{
			p.ReadToken("{");

			Surface s = new Surface();

			s.Name = p.ReadToken();
			s.Vertices = new Vertex[p.ReadInt()];
			s.Indices = new int[p.ReadInt()];
			for (int i = 0; i < s.Vertices.Length; ++i)
				s.Vertices[i] = ReadVertex();
			for (int i = 0; i < s.Indices.Length; ++i)
				s.Indices[i] = p.ReadInt();

			p.ReadToken("}");
			return s;
		}

		Model ReadModel()
		{
			p.ReadToken("{");
			Model m = new Model();
			m.Name = p.ReadToken();
			m.Surfaces = new Surface[p.ReadInt()];
			for (int i = 0; i < m.Surfaces.Length; ++i)
				m.Surfaces[i] = ReadSurface();
			p.ReadToken("}");
			return m;
		}

		public Vector3 BBoxSize
		{
			get
			{
				return BBoxMax - BBoxMin;
			}
		}

		Doom3Parser p;
		public List<Model> Models;
		public Vector3 BBoxMax;
		public Vector3 BBoxMin;
	}


}
