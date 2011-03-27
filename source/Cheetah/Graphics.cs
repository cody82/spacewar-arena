using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
//using Tao.OpenGl;
//using Tao.Sdl;
//using Tao.DevIl;
//using SDLDotNet.Image;
//using SDLDotNet;
using System.Text;
using System.Globalization;
//using OpenDe;
using System.Collections.Generic;
using OpenTK;

namespace Cheetah.Graphics
{
	[StructLayout(LayoutKind.Sequential)]
    public struct NormalMappingVertex
    {
        public Vector3 Position;
        public Vector2 Texture0;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiNormal;

        public static float[] ToFloatArray(NormalMappingVertex[] v)
        {
            int vertexfloatsize=Format.Size / 4;
            if (vertexfloatsize != 14)
                throw new Exception();
            float[] f = new float[v.Length * vertexfloatsize];
            for (int i = 0; i < v.Length; ++i)
            {
                f[i * vertexfloatsize + 0] = v[i].Position.X;
                f[i * vertexfloatsize + 1] = v[i].Position.Y;
                f[i * vertexfloatsize + 2] = v[i].Position.Z;
                f[i * vertexfloatsize + 3] = v[i].Texture0.X;
                f[i * vertexfloatsize + 4] = v[i].Texture0.Y;
                f[i * vertexfloatsize + 5] = v[i].Normal.X;
                f[i * vertexfloatsize + 6] = v[i].Normal.Y;
                f[i * vertexfloatsize + 7] = v[i].Normal.Z;
                f[i * vertexfloatsize + 8] = v[i].Tangent.X;
                f[i * vertexfloatsize + 9] = v[i].Tangent.Y;
                f[i * vertexfloatsize + 10] = v[i].Tangent.Z;
                f[i * vertexfloatsize + 11] = v[i].BiNormal.X;
                f[i * vertexfloatsize + 12] = v[i].BiNormal.Y;
                f[i * vertexfloatsize + 13] = v[i].BiNormal.Z;
            }

            return f;
        }

        public static readonly VertexFormat Format = new VertexFormat(new VertexFormat.Element[]{
																					  new VertexFormat.Element(VertexFormat.ElementName.Position,3),
																					  new VertexFormat.Element(VertexFormat.ElementName.Texture0,2),
																						new VertexFormat.Element(VertexFormat.ElementName.Normal,3),
																						new VertexFormat.Element(VertexFormat.ElementName.Tangent,3),
																						new VertexFormat.Element(VertexFormat.ElementName.Binormal,3)
		});

    }

    public struct Triangle
    {
        public int Index0;
        public int Index1;
        public int Index2;
    }

    public static class MeshUtil
    {
        public static void CalculateTangentSpace(NormalMappingVertex[] vertices, Triangle[] triangles)
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].Tangent = vertices[i].BiNormal = Vector3.Zero;
            }

            Vector3[] tan1 = new Vector3[vertices.Length];
            Vector3[] tan2 = new Vector3[vertices.Length];

            for (int a = 0; a < triangles.Length; a++)
            {
                int i1 = triangles[a].Index0;
                int i2 = triangles[a].Index1;
                int i3 = triangles[a].Index2;
        
                Vector3 v1 = vertices[i1].Position;
                Vector3 v2 = vertices[i2].Position;
                Vector3 v3 = vertices[i3].Position;
        
                Vector2 w1 = vertices[i1].Texture0;
                Vector2 w2 = vertices[i2].Texture0;
                Vector2 w3 = vertices[i3].Texture0;
        
                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;
        
                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;
        
                float r = 1.0F / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r,
                    (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r,
                    (s1 * z2 - s2 * z1) * r);
        
                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;
        
                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }
            
            for (int a = 0; a < vertices.Length; a++)
            {
                Vector3 n = vertices[a].Normal;
                Vector3 t = tan1[a];
        
                // Gram-Schmidt orthogonalize
                vertices[a].Tangent = (t - n * Vector3.Dot(n, t));
                vertices[a].Tangent.Normalize();

                vertices[a].BiNormal = Vector3.Cross(vertices[a].Normal, vertices[a].Tangent);
                // Calculate handedness
                // tangent[a].W = (Dot(Cross(n, t), tan2[a]) < 0.0F) ? -1.0F : 1.0F;
            }

        }
    }

	public class SubMesh : IDisposable, ISaveable, IResource
	{
		public void Dispose()
		{
			//Indices.Dispose();
			//Indices=null;
		}

        public void Save(SaveContext sc)
        {
        }

        public void Draw(IRenderer r)
        {
            if (Vertices != null)
                r.Draw(Vertices, PrimitiveType.TRIANGLES, 0, Indices.buffer.Length, Indices);
            else if (Indices != null)
                r.Draw(Vertices, PrimitiveType.TRIANGLES, 0, Indices.buffer.Length, Indices);
            else
                r.Draw(Vertices, PrimitiveType.TRIANGLES, 0, VertexCount, null);
        }

        public IndexBuffer Indices;
		public Material Material;
		public VertexBuffer Vertices;
		public int VertexCount;
		public VertexBuffer Normals;
        public BoundingBox BBox;
        //public Shader Shader;


    }

	public class XFile
	{
		public class Template
		{
			public class Member
			{
				public Member(string t,string n)
				{
					type=t;
					name=n;
				}
				public Member(string t,string n,string ai)
				{
					type=t;
					name=n;
					arrayindex=ai;
				}
				public string type;
				public string name;
				public string arrayindex;
			}
			public class Restriction
			{
				public Restriction(string n,string g)
				{
					name=n;
					guid=g;
				}
				public string name;
				public string guid;
			}

			public string name;
			public string guid;
			public ArrayList members=new ArrayList();
			public ArrayList restrictions=new ArrayList();
		}

		public class EofException: Exception
		{
		}

		protected string tokens="{}[]><,;";
		protected string whitespaces=" \r\n\t";
		protected string nexttoken=null;
		public Hashtable templates=new Hashtable();
		protected string[] types={"FLOAT","WORD","DWORD","DOUBLE","CHAR","UCHAR","BYTE","STRING"};
		public Hashtable data=new Hashtable();

		protected char Peek()
		{
			//if(textreader.BaseStream.Position==textreader.BaseStream.Length)
			//	throw new EofException();
			char c=(char)textreader.Peek();
			if(c==(char)65535)throw new EofException();
			return c;
		}

		protected char Read()
		{
			//if(textreader.BaseStream.Position==textreader.BaseStream.Length)
			//	throw new EofException();
			char c=(char)textreader.Read();
			if(c==(char)65535)throw new EofException();
			return c;
		}

		protected bool IsToken(char c)
		{
			return tokens.IndexOf(c)!=-1;
		}
		protected bool IsWhiteSpace(char c)
		{
			return whitespaces.IndexOf(c)!=-1;
		}

		protected string PeekToken()
		{
			string s;
			if(nexttoken!=null)
			{
				s=nexttoken;
			}
			else
			{
				s=ReadToken4();
				while(s=="//")
				{
					while(Read()!='\n');
					s=ReadToken4();
				}
				nexttoken=s;
			}
			return s;
		}

		protected string ReadToken()
		{
			string s=ReadToken3();
			while(s=="//")
			{
				while(Read()!='\n');
				s=ReadToken3();
			}
			//Console.WriteLine(s);
			return s;
		}

		protected string ReadToken3()
		{
			if(nexttoken!=null)
			{
				string s1=nexttoken;
				nexttoken=null;
				return s1;
			}
			else
				return ReadToken4();
		}

		protected string ReadToken4()
		{

			//bool b=true;
			string s="";

			while(true)
			{
				char c=Peek();
				if(s.Length>0)
				{
					if(IsWhiteSpace(c))
					{
						Read();
						return s;
					}
					else if(IsToken(c))
					{
						return s;
					}
					else
					{
						Read();
						s+=new string(c,1);
					}
				}
				else
				{
					if(IsWhiteSpace(c))
					{
						Read();
					}
					else if(IsToken(c))
					{
						Read();
						return new string(c,1);
					}
					else
					{
						Read();
						s+=new string(c,1);
					}
				}
			}
		}

		protected void ReadHeader()
		{
			if(ReadToken()!="xof")
			{
				throw new Exception("header error");
			}
			
			if(!ReadToken().EndsWith("txt"))
			{
				throw new Exception("not txt format");
			}

			ReadToken();
		}

		protected string ReadGuid()
		{
			if(ReadToken()!="<")
				throw new Exception("error in guid");
			string s=ReadToken();
			if(ReadToken()!=">")
				throw new Exception("error in guid");
			return s;
		}

		protected void ReadRestrictions(Template t)
		{
			if(ReadToken()!="[")
				throw new Exception("error in template member");

			while(PeekToken()!="]")
			{
				string type=ReadToken();
				Template.Restriction r=new Template.Restriction(type,null);
				if(PeekToken()=="<")
					r.guid=ReadGuid();
				if(PeekToken()==",")
					ReadToken();
				t.restrictions.Add(r);
			}

			if(ReadToken()!="]")
				throw new Exception("error in template member");
		}

		protected void ReadTemplateMembers(Template t)
		{
			while(true)
			switch(PeekToken())
			{
				case "}":
					return;
				case "array":
				{
					ReadToken();
					string type=ReadToken();
					if(IsToken(type[0]))
						throw new Exception("error in template member");
					string name=ReadToken();
					if(IsToken(name[0]))
						throw new Exception("error in template member");
					if(ReadToken()!="[")
						throw new Exception("error in template member");
					string array=ReadToken();
					if(ReadToken()!="]"||ReadToken()!=";")
						throw new Exception("error in template member");
					t.members.Add(new Template.Member(type,name,array));
					break;
				}
				case "[":
					ReadRestrictions(t);
					break;
				default:
				{
					string type=ReadToken();
					if(IsToken(type[0]))
						throw new Exception("error in template member");
					string name=ReadToken();
					if(IsToken(name[0]))
						throw new Exception("error in template member");
					if(ReadToken()!=";")
						throw new Exception("error in template member");
					t.members.Add(new Template.Member(type,name));
					break;
				}
			}
		}

		protected void ReadTemplates()
		{
			string s;
			try{s=PeekToken();}catch(EofException){return;}
			while(s=="template")
			{
				ReadToken();
				Template t=new Template();
				t.name=ReadToken();
				if(ReadToken()!="{")
					throw new Exception("error in template");
				if(PeekToken()=="<")
					t.guid=ReadGuid();
				ReadTemplateMembers(t);
				if(ReadToken()!="}")
					throw new Exception("error in template");
				templates[t.name]=t;

				try{s=PeekToken();}
				catch(EofException){return;}
			}
		}

		protected bool IsType(string s)
		{
			foreach(string t in types)
				if(s==t)return true;
			return false;
		}

		protected Hashtable ReadTemplateData(Template t,string name)
		{
			Hashtable data=new Hashtable();
			foreach(Template.Member m in t.members)
			{
				if(m.arrayindex!=null)
				{
					int i=0;
					try
					{
						i=int.Parse(m.arrayindex);
					}
					catch(Exception)
					{
						i=(int)data[m.arrayindex];
					}
					ArrayList list=new ArrayList();
					for(int j=0;j<i;++j)
					{
						if(IsType(m.type))
						{
							string s=ReadToken();
							if(m.type=="DWORD"||m.type=="WORD")
							{
								list.Add(int.Parse(s));
							}
							else if(m.type=="DOUBLE"||m.type=="FLOAT")
							{
								list.Add(float.Parse(s,NumberFormatInfo.InvariantInfo));
							}
							else if(m.type=="STRING")
							{
								list.Add(s.Trim(new char[]{'\"'}));
							}
							else
							{
								list.Add(s);
							}
						}
						else
						{
							Template t2=(Template)templates[m.type];
							//throw new Exception();
							//Hashtable ht2=new Hashtable();
							//ht[name!=null?name:t2.name]=ht2;
							list.Add(ReadTemplateData(t2,null));
						}
						if(j<i-1)
							if(ReadToken()!=",")
								throw new Exception("error in template data 1");
					}
					data[m.name]=list;
					//if(ReadToken()!=";")
					//	throw new Exception("error in template data");
				}
				else
				{
					if(IsType(m.type))
					{
						string s=ReadToken();
						if(m.type=="DWORD"||m.type=="WORD")
						{
							data[m.name]=int.Parse(s);
						}
						else if(m.type=="DOUBLE"||m.type=="FLOAT")
						{
							data[m.name]=float.Parse(s,NumberFormatInfo.InvariantInfo);
						}
						else if(m.type=="STRING")
						{
							data[m.name]=s.Trim(new char[]{'\"'});
						}
						else
						{
							data[m.name]=s;
						}
					}
					else
					{
						Template t2=(Template)templates[m.type];
						data[m.name]=ReadTemplateData(t2,m.name);
					}
				}
				if(PeekToken()!=";")
				{
					if(ReadToken()==",")
						Console.WriteLine("file is strange: should be ';' but got ','");
					else
						throw new Exception("error in template data 2");
				}
				else
					ReadToken();
			}
			if(t.restrictions.Count>0)
			{
				while(PeekToken()!="}")
				{
					string type=ReadToken();
					if(type==";")
					{
						Console.WriteLine("file strange: expected type, got ';'");
						type=ReadToken();
					}
					Template t2=(Template)templates[type];
					string s=ReadToken();
					if(s=="{")
						s=null;
					else if(ReadToken()!="{")
						throw new Exception("error in restriction data 1");
					data[s!=null?s:type]=ReadTemplateData(t2,s);
					if(ReadToken()!="}")
						throw new Exception("error in restriction data 2");
				}
			}
			return data;
		}

		protected void ReadData()
		{
			while(ReadDataItem());
				//ReadDataItem();
		}

		protected void WriteLine(string s)
		{
			
			//Console.WriteLine(s);
			print.Write(Encoding.ASCII.GetBytes(s+"\r\n"),0,Encoding.ASCII.GetBytes(s).Length+2);
		}

		public void Print(Hashtable data,int level)
		{
			string s=new string(' ',level);
			foreach(DictionaryEntry de in data)
			{
				if(de.Value is Hashtable)
				{
					WriteLine(s+de.Key.ToString()+"->");
					Print((Hashtable)de.Value,level+1);
				}
				else if(de.Value is ArrayList)
				{
					WriteLine(s+de.Key.ToString()+"[]->");
					WriteLine(s+"-");
					foreach(object o in (ArrayList)de.Value)
					{
						if(o is Hashtable)
						{
							Print((Hashtable)o,level+1);
						}
						else
							WriteLine(s+o.ToString());
						WriteLine(s+"-");
					}
				}
				else
				{
					WriteLine(s+de.Key.ToString()+" = "+de.Value.ToString());
				}
			}
		}

		Stream print;

		public void Print(string file)
		{
			print=new FileStream(file,FileMode.Create);
			Print(data,0);
			print.Close();
		}

		protected bool ReadDataItem()
		{
			string type;
			try
			{
				type=ReadToken();
			}
			catch(EofException)
			{
				return false;
			}

			if(IsToken(type[0]))
				throw new Exception("error in data");
			string name=ReadToken();
			if(name=="{")
				name=null;
			else if(ReadToken()!="{")
				throw new Exception("error in data");

			Template t=(Template)templates[type];
			data[name!=null?name:type]=ReadTemplateData(t,name);
			string s=ReadToken();
			if(s!="}")
				throw new Exception("error in data");
			return true;
		}

		public void Parse(string file)
		{
			Parse(new FileStream(file,FileMode.Open));
		}

		public void Parse(Stream s)
		{
			stream=s;
			textreader=new StreamReader(s);
			ReadHeader();
			ReadTemplates();
			ReadData();
		}

		protected Stream stream;
		protected StreamReader textreader;
	}
	public struct ObjVertex
			  {
				  public ObjVertex(float p0,float p1,float p2,float t0,float t1,float n0,float n1,float n2)
				  {
					  Position.X=p0;
					  Position.Y=p1;
					  Position.Z=p2;
					  Texture0.X=t0;
					  Texture0.Y=t1;
					  Normal.X=n0;
					  Normal.Y=n1;
					  Normal.Z=n2;
				  }

		public override bool Equals(object o)
		{
			if(!(o is ObjVertex))
			{
				return false;
			}
			ObjVertex v=(ObjVertex)o;
			if(
				Position.X==v.Position.X&&
				Position.Y==v.Position.Y&&
				Position.Z==v.Position.Z&&
				Texture0.X==v.Texture0.X&&
				Texture0.Y==v.Texture0.Y&&
				Normal.X==v.Normal.X&&
				Normal.Y==v.Normal.Y&&
				Normal.Z==v.Normal.Z
				)
				return true;
			return false;
		}

		public Vector3 Position;
				  public Vector2 Texture0;
				  public Vector3 Normal;
			  }

	public struct ObjFace
	{
		public ObjVertex Vertex1;
		public ObjVertex Vertex2;
		public ObjVertex Vertex3;
		public int SmoothingGroup;
		public ObjVertex this[int index]
		{
			get
			{
				switch(index)
				{
					case 0:
						return Vertex1;
					case 1:
						return Vertex2;
					case 2:
						return Vertex3;
					default:
						throw new Exception("wrong index.");
				}
			}
			set
			{
				switch(index)
				{
					case 0:
						Vertex1=value;
						break;
					case 1:
						Vertex2=value;
						break;
					case 2:
						Vertex3=value;
						break;
				}
			}
	}
		public Vector3 Normal;
			  }

	public struct Face
			  {
				  public int Vertex1,Vertex2,Vertex3;
				  public int SmoothingGroup;
			  }

	public class Mesher
	{
		public void FillFace(ObjFace f)
		{
			BoundingBox.X=Math.Max(BoundingBox.X,f.Vertex1.Position.X);
			BoundingBox.Y=Math.Max(BoundingBox.Y,f.Vertex1.Position.Y);
			BoundingBox.Z=Math.Max(BoundingBox.Z,f.Vertex1.Position.Z);
			BoundingBox.X=Math.Max(BoundingBox.X,f.Vertex2.Position.X);
			BoundingBox.Y=Math.Max(BoundingBox.Y,f.Vertex2.Position.Y);
			BoundingBox.Z=Math.Max(BoundingBox.Z,f.Vertex2.Position.Z);
			BoundingBox.X=Math.Max(BoundingBox.X,f.Vertex3.Position.X);
			BoundingBox.Y=Math.Max(BoundingBox.Y,f.Vertex3.Position.Y);
			BoundingBox.Z=Math.Max(BoundingBox.Z,f.Vertex3.Position.Z);

			Faces.Add(f);
		}

		protected ObjFace[] FindSharedFaces(ObjVertex v,ObjFace f)//==position&&==sg
		{
			ArrayList l=new ArrayList();
			for(int i=0;i<Faces.Count;++i)
			{
				ObjFace f2=(ObjFace)Faces[i];
				ObjVertex v1=f2.Vertex1;
				if(v1.Position.X==v.Position.X&&v1.Position.Y==v.Position.Y&&v1.Position.Z==v.Position.Z&&f.SmoothingGroup==f2.SmoothingGroup)
				{
					l.Add(f2);
					continue;
				}
				ObjVertex v2=f2.Vertex2;
				if(v2.Position.X==v.Position.X&&v2.Position.Y==v.Position.Y&&v2.Position.Z==v.Position.Z&&f.SmoothingGroup==f2.SmoothingGroup)
				{
					l.Add(f2);
					continue;
				}
				ObjVertex v3=f2.Vertex3;
				if(v3.Position.X==v.Position.X&&v3.Position.Y==v.Position.Y&&v3.Position.Z==v.Position.Z&&f.SmoothingGroup==f2.SmoothingGroup)
				{
					l.Add(f2);
					continue;
				}
			}
			return (ObjFace[])l.ToArray(typeof(ObjFace));
		}

		protected Vector3 CalcNormal(ObjVertex v,ObjFace f)
		{
			ObjFace[] f2=FindSharedFaces(v,f);
			Vector3 n=new Vector3();
			for(int i=0;i<f2.Length;++i)
			{
				n+=f2[i].Normal;
			}
			if(n.LengthSquared>0)
				n.Normalize();
			return n;
		}

		protected void BuildIndex()
		{
			const bool loadnormals=false;
			for(int i=0;i<Faces.Count;++i)
			{
				ObjFace f=(ObjFace)Faces[i];
				Vector3 a=f.Vertex1.Position;
				Vector3 b=f.Vertex2.Position;
				Vector3 c=f.Vertex3.Position;
				Vector3 n=Vector3.Cross(b-a,c-a);
				if(n.LengthSquared>0)n.Normalize();
				f.Normal=n;
				Faces[i]=f;
			}
			for(int i=0;i<Faces.Count;++i)
			{
				ObjFace f=(ObjFace)Faces[i];
				f.Vertex1.Normal=CalcNormal(f.Vertex1,f);
				f.Vertex2.Normal=CalcNormal(f.Vertex2,f);
				f.Vertex3.Normal=CalcNormal(f.Vertex3,f);
				Faces[i]=f;
			}
			foreach(ObjFace f in Faces)
			{
				if(Vertices.Contains(f.Vertex1))
					Index.Add(Vertices.IndexOf(f.Vertex1));
				else
					Index.Add(Vertices.Add(f.Vertex1));
				if(Vertices.Contains(f.Vertex2))
					Index.Add(Vertices.IndexOf(f.Vertex2));
				else
					Index.Add(Vertices.Add(f.Vertex2));
				if(Vertices.Contains(f[2]))
					Index.Add(Vertices.IndexOf(f[2]));
				else
					Index.Add(Vertices.Add(f[2]));
			}
			Console.WriteLine(Faces.Count*3+"->"+Vertices.Count);

			if(loadnormals)
			{
				/*int j=0;
				Normals=new Vector3f[Vertices.Count*2];
				foreach(ObjVertex v in Vertices)
				{
					Normals[j++]=v.Position;
					Normals[j].X=v.Position.X+v.Normal.X;
					Normals[j].Y=v.Position.Y+v.Normal.Y;
					Normals[j++].z=v.Position.z+v.Normal.z;
				}*/
			}
		}

		public void Consolidate()
		{
			BuildIndex();
		}

		public ArrayList Index=new ArrayList();//int
		public ArrayList Vertices=new ArrayList();//ObjVertex
		protected ArrayList Faces=new ArrayList();//ObjFace
		public Vector3[] Normals;
		public Vector3 BoundingBox;
	}

    public class MaterialLoader : IResourceLoader
    {
        public Type LoadType
        {
            get { return typeof(Material); }
        }
        public IResource Load(FileSystemNode n)
        {
            Material m = new Material();

            StreamReader r = new StreamReader(n.getStream());
            string line;

            string header = r.ReadLine().Trim();
            if (header != "MATERIALTEXT")
                return null;

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("diffuse:"))
                {
                    string[] values=line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    m.diffuse = new Color4f(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
                else if (line.StartsWith("ambient:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    m.ambient = new Color4f(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
                else if (line.StartsWith("specular:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    m.specular = new Color4f(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
                else if (line.StartsWith("diffusemap:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    m.diffusemap = Root.Instance.ResourceManager.LoadTexture(filename);
                    continue;
                }
                else if (line.StartsWith("heightmap:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    m.HeightMap = Root.Instance.ResourceManager.LoadTexture(filename);
                    continue;
                }
                else if (line.StartsWith("environmentmap:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    m.EnvironmentMap = Root.Instance.ResourceManager.LoadTexture(filename);
                    continue;
                }
                else if (line.StartsWith("glossiness:"))
                {
                    string val = line.Split(new char[] { ':' })[1].Trim();
                    m.shininess = float.Parse(val);
                    continue;
                }
                else if (line.StartsWith("twosided:"))
                {
                    string val = line.Split(new char[] { ':' })[1].Trim();
                    m.twosided = bool.Parse(val);
                    continue;
                }
                else if (line.StartsWith("depthwrite:"))
                {
                    string val = line.Split(new char[] { ':' })[1].Trim();
                    m.DepthWrite = bool.Parse(val);
                    continue;
                }
                else if (line.StartsWith("wire:"))
                {
                    string val = line.Split(new char[] { ':' })[1].Trim();
                    m.wire = bool.Parse(val);
                    continue;
                }
                else if (line.StartsWith("depthtest:"))
                {
                    string val = line.Split(new char[] { ':' })[1].Trim();
                    m.DepthTest = bool.Parse(val);
                    continue;
                }
                else if (line.StartsWith("nolighting:"))
                {
                    string val = line.Split(new char[] { ':' })[1].Trim();
                    m.NoLighting = bool.Parse(val);
                    continue;
                }
				else if (line.StartsWith("bumpmap:"))
				{
					string filename = line.Split(new char[] { ':' })[1].Trim();
					m.BumpMap = Root.Instance.ResourceManager.LoadTexture(filename);
					continue;
				}
                else if (line.StartsWith("specularmap:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    m.SpecularMap = Root.Instance.ResourceManager.LoadTexture(filename);
                    continue;
                }
                else if (line.StartsWith("reflectionmap:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    m.ReflectionMap = Root.Instance.ResourceManager.LoadTexture(filename);
                    continue;
                }
                else if (line.StartsWith("emissivemap:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    m.EmissiveMap = Root.Instance.ResourceManager.LoadTexture(filename);
                    continue;
                }
                else if (line.StartsWith("shader:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    m.Shader = Root.Instance.ResourceManager.LoadShader(filename);
                    continue;
                }
                else if (line.StartsWith("additive:"))
                {
                    string val = line.Split(new char[] { ':' })[1].Trim();
                    m.Additive = bool.Parse(val);
                    continue;
                }
                else if (line.StartsWith("shader.uniform."))
                {
                    string[] l=line.Substring(15).Split(new char[] { ':' });
                    string[] values = l[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    float[] v = new float[values.Length];
                    for (int i = 0; i < values.Length; ++i)
                        v[i] = float.Parse(values[i]);

                    m.Uniforms[l[0].Trim()] = v;
                    //m.Additive = bool.Parse(val);
                    continue;
                }
            }


            return m;
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info!=null&&(n.info.Extension.ToLower() == ".material");
        }
    }

	public class SubMeshWriter
	{
		public void Save(SubMesh m,Stream s)
		{
			StreamWriter w = new StreamWriter(s);
			w.WriteLine("SUBMESHTEXT0001");
			w.Write("vertex format: ");

			for (int i = 0; i < m.Vertices.Format.Count; ++i)
			{
				VertexFormat.Element e = m.Vertices.Format[i];
				switch (e.Name)
				{
					case VertexFormat.ElementName.Position:
						w.Write("position:");
						break;
					case VertexFormat.ElementName.Color:
						w.Write("color:");
						break;
					case VertexFormat.ElementName.Binormal:
						w.Write("binormal:");
						break;
					case VertexFormat.ElementName.Normal:
						w.Write("normal:");
						break;
					case VertexFormat.ElementName.Tangent:
						w.Write("tangent:");
						break;
					case VertexFormat.ElementName.Texture0:
						w.Write("texture0:");
						break;
					case VertexFormat.ElementName.Texture1:
						w.Write("texture1:");
						break;
				}
				w.Write(e.Count);
				if (i < m.Vertices.Format.Count - 1)
					w.Write(",");
			}

			w.WriteLine("");
			w.WriteLine("vertex count: " + m.Vertices.Count.ToString());

			for (int i = 0; i < m.Vertices.Count; ++i)
			{
				for (int j = 0; j < m.Vertices.Format.Count; ++j)
				{
					VertexFormat.Element e = m.Vertices.Format[j];
					float[] attr = m.Vertices.GetAttribute(i, e.Name);
					w.Write("[");
					for (int k = 0; k < attr.Length; ++k)
					{
						w.Write(attr[k]);
						if (k < attr.Length - 1)
							w.Write(",");
					}
					w.Write("]");
					if (j < m.Vertices.Format.Count - 1)
						w.Write(" ");
				}
				w.WriteLine("");
			}

			w.WriteLine("triangle count: " + (m.Indices.buffer.Length / 3).ToString());

			for (int i = 0; i < m.Indices.buffer.Length / 3;++i )
			{
				w.WriteLine("[" + m.Indices.buffer[i * 3].ToString() + "," + m.Indices.buffer[i * 3 + 1].ToString() + "," + m.Indices.buffer[i * 3 + 2].ToString() + "]");
			}
			w.Flush();
		}
	}

    public class FixedStreamReader : StreamReader
    {
        public FixedStreamReader(Stream s)
            : base(s)
        {
        }

        public override string ReadLine()
        {
            if (buf.Count > 0)
            {
                string s = buf[0];
                buf.RemoveAt(0);
                return s;
            }

            string line = base.ReadLine();
            if (line == null)
                return null;

            if (line.Contains("\n") || line.Contains("\r"))
            {
                System.Console.WriteLine("readline bug.");
                string[] ss = line.Split('\r', '\n');
                foreach (string d in ss)
                {
                    string d2 = d.Trim();
                    if (d2.Length > 0)
                    {
                        buf.Add(d2);
                    }
                }
            }
            else
                return line;

            if (buf.Count > 0)
            {
                string s = buf[0];
                buf.RemoveAt(0);
                return s;
            }
            else
                return null;
        }

        List<string> buf=new List<string>();
    }

    public class SubMeshSaver : ISaver<SubMesh>
    {
        public void Save(SubMesh sm, Stream s)
        {
            /*using (StreamWriter sw = new StreamWriter(s))
            {
                for (int i = 0; i < sm.Vertices.Count; ++i)
                {
                    float[] v = sm.Vertices.GetAttribute(i, VertexFormat.ElementName.Position);
                    sw.WriteLine(v[0].ToString() + ", " + v[1].ToString() + ", " + v[2].ToString());
                }
            }*/
            
            /*using (BinaryWriter sw = new BinaryWriter(s))
            {
                for (int i = 0; i < sm.Indices.buffer.Length; ++i)
                {
                    float[] v = sm.Vertices.GetAttribute(sm.Indices.buffer[i], VertexFormat.ElementName.Position);
                    for (int j = 0; j < v.Length; ++j)
                        sw.Write(v[j]);
                    v = sm.Vertices.GetAttribute(sm.Indices.buffer[i], VertexFormat.ElementName.Normal);
                    for (int j = 0; j < v.Length; ++j)
                        sw.Write(v[j]);
                    v = sm.Vertices.GetAttribute(sm.Indices.buffer[i], VertexFormat.ElementName.Texture0);
                    for (int j = 0; j < v.Length; ++j)
                        sw.Write(v[j]);
                }
            }*/
            using (BinaryWriter sw = new BinaryWriter(s))
            {
                sw.Write("SUBMESHBIN001");

                throw new Exception("HACK");
                //sm.Vertices.Format.Serialize(new SerializationContext(Root.Instance.Factory, s, sw));

                sw.Write(sm.Vertices.Buffer.Length*4);
                for (int i = 0; i < sm.Vertices.Buffer.Length;++i )
                {
                    sw.Write(sm.Vertices.Buffer[i]);
                }
                sw.Write(sm.Indices.buffer.Length);
                for (int i = 0; i < sm.Indices.buffer.Length; ++i)
                {
                    sw.Write(sm.Indices.buffer[i]);
                }
            }
        }
    }

    public class SubMeshPlyLoader : IResourceLoader
    {
        float GetFloatProperty(string[] split, string property)
        {
            return float.Parse(split[vertexproperties.FindIndex(delegate(string x) { return x == property; })]);
        }

        bool has_normals;
        bool has_texcoord;
        bool has_position;


        IResource OldLoadFunction()
        {
            string line;
            string[] split;

            List<VertexFormat.Element> elements = new List<VertexFormat.Element>();

            elements.Add(new VertexFormat.Element(VertexFormat.ElementName.Position, 3));
            if (has_normals)
                elements.Add(new VertexFormat.Element(VertexFormat.ElementName.Normal, 3));
            if (has_texcoord)
                elements.Add(new VertexFormat.Element(VertexFormat.ElementName.Texture0, 2));

            fmt = new VertexFormat(elements.ToArray());

            int vertexfloatsize = fmt.Size / 4;
            float[] vertices = new float[vertexfloatsize * vertexcount];

            for (int i = 0; i < vertexcount; ++i)
            {
                line = r.ReadLine();
                split = line.Split(' ');

                vertices[i * vertexfloatsize] = GetFloatProperty(split, "x");
                vertices[i * vertexfloatsize + 1] = GetFloatProperty(split, "y");
                vertices[i * vertexfloatsize + 2] = GetFloatProperty(split, "z");

                if (has_normals)
                {
                    vertices[i * vertexfloatsize + 3] = GetFloatProperty(split, "nx");
                    vertices[i * vertexfloatsize + 4] = GetFloatProperty(split, "ny");
                    vertices[i * vertexfloatsize + 5] = GetFloatProperty(split, "nz");
                }

                if (has_texcoord)
                {
                    vertices[i * vertexfloatsize + (has_normals ? 6 : 3)] = GetFloatProperty(split, "s");
                    vertices[i * vertexfloatsize + (has_normals ? 7 : 4)] = GetFloatProperty(split, "t");
                }
            }

            List<int> indices = new List<int>();
            for (int i = 0; i < facecount; ++i)
            {
                line = r.ReadLine();
                split = line.Split(' ');

                int c = int.Parse(split[0]);
                if (c == 3)
                {
                    indices.Add(int.Parse(split[1]));
                    indices.Add(int.Parse(split[2]));
                    indices.Add(int.Parse(split[3]));
                }
                else if (c == 4)
                {
                    indices.Add(int.Parse(split[1]));
                    indices.Add(int.Parse(split[2]));
                    indices.Add(int.Parse(split[3]));

                    indices.Add(int.Parse(split[1]));
                    indices.Add(int.Parse(split[3]));
                    indices.Add(int.Parse(split[4]));
                }
                else
                {
                    throw new Exception("ply: no triangle or quad");
                }
            }

            SubMesh mesh = new SubMesh();

            if (Root.Instance.UserInterface != null)
                mesh.Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(vertices, vertices.Length * 4);
            else
                mesh.Vertices = new VertexBuffer();

            mesh.Vertices.Format = fmt;
            mesh.Vertices.Buffer = vertices;
            mesh.VertexCount = vertexcount;
            mesh.Indices = new IndexBuffer();
            mesh.Indices.buffer = indices.ToArray();

            return mesh;
        }

        #region IResourceLoader Members


        public IResource Load(FileSystemNode n)
        {
            fmt = null;
            r = null;
            vertexcount = -1;
            facecount = -1;
            vertexproperties = null;

            string line;
            string[] split;

            r = new FixedStreamReader(n.getStream());

            ReadHeader();

            has_normals = vertexproperties.Contains("nx");
            has_texcoord = vertexproperties.Contains("s");
            has_position = vertexproperties.Contains("x");

            if (!has_position)
                throw new Exception("ply: no position");
            if (!has_texcoord || !has_normals)
            {
                Console.WriteLine("ply: no normals/texcoords -> normal mapping wont work.");
                return OldLoadFunction();
            }

            fmt = NormalMappingVertex.Format;

            //int vertexfloatsize = fmt.Size / 4;
            //float[] vertices = new float[vertexfloatsize * vertexcount];
            NormalMappingVertex[] vertices = new NormalMappingVertex[vertexcount];

            for (int i = 0; i < vertexcount; ++i)
            {
                line = r.ReadLine();
                split = line.Split(' ');

                vertices[i].Position.X = GetFloatProperty(split, "x");
                vertices[i].Position.Y = GetFloatProperty(split, "y");
                vertices[i].Position.Z = GetFloatProperty(split, "z");

                //if (has_normals)
                {
                    vertices[i].Normal.X = GetFloatProperty(split, "nx");
                    vertices[i].Normal.Y = GetFloatProperty(split, "ny");
                    vertices[i].Normal.Z = GetFloatProperty(split, "nz");
                }

                //if (has_texcoord)
                {
                    vertices[i].Texture0.X = GetFloatProperty(split, "s");
                    vertices[i].Texture0.Y = GetFloatProperty(split, "t");
                    //vertices[i * vertexfloatsize + (has_normals ? 6 : 3)] = GetFloatProperty(split, "s");
                    //vertices[i * vertexfloatsize + (has_normals ? 7 : 4)] = GetFloatProperty(split, "t");
                }
            }

            List<int> indices = new List<int>();
            for (int i = 0; i < facecount; ++i)
            {
                line = r.ReadLine();
                split = line.Split(' ');

                int c = int.Parse(split[0]);
                if (c == 3)
                {
                    indices.Add(int.Parse(split[1]));
                    indices.Add(int.Parse(split[2]));
                    indices.Add(int.Parse(split[3]));
                }
                else if (c == 4)
                {
                    indices.Add(int.Parse(split[1]));
                    indices.Add(int.Parse(split[2]));
                    indices.Add(int.Parse(split[3]));

                    indices.Add(int.Parse(split[1]));
                    indices.Add(int.Parse(split[3]));
                    indices.Add(int.Parse(split[4]));
                }
                else
                {
                    throw new Exception("ply: no triangle or quad");
                }
            }
            Triangle[] triangles = new Triangle[indices.Count/3];
            for(int i=0;i<triangles.Length;++i)
            {
                triangles[i].Index0=indices[i*3+0];
                triangles[i].Index1=indices[i*3+1];
                triangles[i].Index2=indices[i*3+2];
            }

            MeshUtil.CalculateTangentSpace(vertices, triangles);

            SubMesh mesh = new SubMesh();
			
			float[] vtmp = NormalMappingVertex.ToFloatArray(vertices);
            if (Root.Instance.UserInterface != null)
                //HACK why doesnt this work?
				//mesh.Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(vertices, vertices.Length * fmt.Size);
                mesh.Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(vtmp, vtmp.Length * 4);
            else
                mesh.Vertices = new VertexBuffer();

            mesh.Vertices.Format = fmt;
            mesh.Vertices.Buffer = vtmp;
            mesh.VertexCount = vertexcount;
            mesh.Indices = new IndexBuffer();
            mesh.Indices.buffer = indices.ToArray();

            return mesh;
        }

        VertexFormat fmt;
        StreamReader r;
        int vertexcount = -1;
        int facecount = -1;

        List<string> vertexproperties;

        private void ReadHeader()
        {
            string line;
            string[] split;

            line = r.ReadLine();
            if (line != "ply")
                throw new Exception("not ply format");

            line = r.ReadLine();
            split = line.Split(' ');
            if (split[0] != "format" || split[1] != "ascii")
                throw new Exception("wrong ply header");

            vertexproperties = new List<string>();

            while ((line = r.ReadLine()) != null)
            {
                if (line == "end_header")
                    return;

                split = line.Split(' ');
                if (split[0] == "comment")
                {
                }
                else if (split[0] == "element")
                {
                    if (split[1] == "vertex")
                    {
                        vertexcount = int.Parse(split[2]);
                    }
                    else if (split[1] == "face")
                    {
                        facecount = int.Parse(split[2]);
                    }
                }
                else if (split[0] == "property")
                {
                    if (facecount == -1 && vertexcount != -1)
                    {
                        vertexproperties.Add(split[2]);
                    }
                    else if (facecount != -1)
                    {
                    }
                }
            }

            throw new Exception("ply header error");
        }

        public Type LoadType
        {
            get { return typeof(SubMesh); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info.Name.ToLower().EndsWith(".ply");
        }

        #endregion
    }

    public class SubMeshBinLoader : IResourceLoader
    {
        #region IResourceLoader Members

        public IResource Load(FileSystemNode n)
        {
            DeSerializationContext c;
            {
                Stream s=n.getStream();
                BinaryReader r = new BinaryReader(s);
        
                c = new DeSerializationContext(Root.Instance.Factory, s, r);
            }

            if (c.ReadString() != "SUBMESHBIN001")
            {
                return null;
            }

            VertexFormat fmt = new VertexFormat(c);

            int length = c.ReadInt32()/4;
            float[] verts = new float[length];
            for(int i=0;i<length;++i)
                verts[i]=c.ReadSingle();


            int length2 = c.ReadInt32();
            int[] inds = new int[length2];
            for (int i = 0; i < length2; ++i)
            {
                inds[i] = c.ReadInt32();
            }

            SubMesh sm = new SubMesh();
            if (Root.Instance.UserInterface != null)
                sm.Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(verts, length * 4);
            else
                sm.Vertices = new VertexBuffer();

            sm.Vertices.Format = fmt;
            sm.Vertices.Buffer = verts;
            sm.VertexCount = length*4/fmt.Size;
            sm.Indices = new IndexBuffer();
            sm.Indices.buffer = inds;

            return sm;
        }

        public Type LoadType
        {
            get { return typeof(SubMesh); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info.Name.EndsWith(".submesh.bin");
        }

        #endregion
    }

    public class SubMeshScmLoader : IResourceLoader
    {

        public IResource Load(FileSystemNode n)
        {
            SupCom.ScmFile scm = new SupCom.ScmFile(n.getStream());

            int[] indices = new int[scm.TriangleData.Length*3];
            for (int i = 0; i < scm.TriangleData.Length; ++i)
            {
                indices[i * 3] = scm.TriangleData[i].Index0;
                indices[i * 3 + 1] = scm.TriangleData[i].Index1;
                indices[i * 3 + 2] = scm.TriangleData[i].Index2;
            }

            float[] verts=new float[scm.VertexData.Length*8];

            int j=0;
                for (int i = 0; i < scm.VertexData.Length; ++i)
                {
                    verts[j++]=scm.VertexData[i].mPosition.X;
                    verts[j++]=scm.VertexData[i].mPosition.Y;
                    verts[j++]=scm.VertexData[i].mPosition.Z;
                    verts[j++]=scm.VertexData[i].mUV0.X;
                    verts[j++]=scm.VertexData[i].mUV0.Y;
                    verts[j++]=scm.VertexData[i].mNormal.X;
                    verts[j++]=scm.VertexData[i].mNormal.Y;
                    verts[j++]=scm.VertexData[i].mNormal.Z;
                }

            SubMesh sm = new SubMesh();
            if (Root.Instance.UserInterface != null)
                sm.Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(verts, verts.Length*4);
            else
                sm.Vertices = new VertexBuffer();

            sm.Vertices.Format = VertexFormat.VF_P3T2N3;
            sm.Vertices.Buffer = verts;
            sm.VertexCount = scm.VertexData.Length;
            sm.Indices = new IndexBuffer();
            sm.Indices.buffer = indices;

            return sm;
        }

        public Type LoadType
        {
            get { return typeof(SubMesh); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info.Name.EndsWith(".scm");
        }

    }

    public class SubMeshLoader : IResourceLoader
    {
        public Type LoadType
        {
            get { return typeof(SubMesh); }
        }
        public IResource Load(FileSystemNode n)
        {
            return LoadSubMesh(n);
        }

        /*struct Vertex
        {
            public Vertex(float px, float py, float pz, float nx, float ny, float nz, float tu, float tv)
            {
                Position.X = px;
                Position.Y = py;
                Position.Z = pz;
                Normal.X = nx;
                Normal.Y = ny;
                Normal.Z = nz;
                Texture0.X = tu;
                Texture0.Y = tv;
            }
            public Vector3 Position;
            public Vector2 Texture0;
            public Vector3 Normal;
        }*/

		public SubMesh LoadSubMesh(StreamReader r)
		{
			string line;

			string header = r.ReadLine().Trim();
			if (header != "SUBMESHTEXT0001")
				return null;

			int current = -1;

			float[] vertices = null;
			int[] indices = null;
			int vertexpos = 0;
			Vector3[] normals = null;
			SubMesh sm = new SubMesh();
			ArrayList elementlist = new ArrayList();
			VertexFormat vertexformat = null;

			while ((line = r.ReadLine()) != null)
			{
				line = line.Trim();
				if (line.Length == 0 || line.StartsWith("#"))
					continue;

				if (line.StartsWith("vertex count:"))
				{
					int vertexcount = int.Parse(line.Split(new char[] { ':' })[1].Trim());
					vertices = new float[vertexcount * vertexformat.Size / 4];
					normals = new Vector3[vertexcount * 2];
					current = 0;
					continue;
				}
                else if (line.StartsWith("triangle count:"))
				{
					int trianglecount = int.Parse(line.Split(new char[] { ':' })[1].Trim());
					indices = new int[trianglecount * 3];
					current = 0;
					continue;
				}
				else if (line.StartsWith("vertex format:"))
				{
					line = line.Substring(14);
					line = line.Trim();
					string[] components = line.Split(',');
					foreach (string component in components)
					{
						string component1 = component.Trim();
						string[] tmp = component1.Split(':');
						string name = tmp[0];
                        int c;
                        string attrib=null;
                        try
                        {
						    c = int.Parse(tmp[1]);
                        }
                        catch(Exception)
                        {
                            //war ein string
                            attrib=tmp[1];
                            //count ist erst das naechste
                            c = int.Parse(tmp[2]);
                        }

						VertexFormat.Element e;
						//System.Console.WriteLine(name);
						switch (name)
						{
							case "position":
								e = new VertexFormat.Element(VertexFormat.ElementName.Position, attrib, c);
								break;
							case "normal":
                                e = new VertexFormat.Element(VertexFormat.ElementName.Normal, attrib, c);
								break;
							case "binormal":
                                e = new VertexFormat.Element(VertexFormat.ElementName.Binormal, attrib, c);
								break;
							case "tangent":
                                e = new VertexFormat.Element(VertexFormat.ElementName.Tangent, attrib, c);
								break;
							case "texture0":
                                e = new VertexFormat.Element(VertexFormat.ElementName.Texture0, attrib, c);
								break;
							case "texture1":
                                e = new VertexFormat.Element(VertexFormat.ElementName.Texture1, attrib, c);
								break;
							case "texture2":
                                e = new VertexFormat.Element(VertexFormat.ElementName.Texture2, attrib, c);
								break;
                            case "none":
                                e = new VertexFormat.Element(VertexFormat.ElementName.None, attrib, c);
                                break;
							default:
								throw new Exception("unknown vertex element name.");
						}
						elementlist.Add(e);
					}
					VertexFormat.Element[] el = new VertexFormat.Element[elementlist.Count];
					elementlist.CopyTo(el);
					vertexformat = new VertexFormat(el);
					continue;
				}
				else if (line.StartsWith("material:"))
				{
					//string material = line.Split(new char[] { ':' })[1].Trim();
					//sm.Material = (Material)Root.Instance.ResourceManager.Load(
					//   Root.Instance.FileSystem.Get(n.parent, material+".material"), typeof(Material));
					continue;
				}

				if (indices != null)
				{
					line = line.Trim(new char[] { '[', ']' });
					string[] idx = line.Split(new char[] { ',' });
					indices[current++] = int.Parse(idx[0]);
					indices[current++] = int.Parse(idx[1]);
					indices[current++] = int.Parse(idx[2]);
				}
				else if (vertices != null)
				{
                    //Console.WriteLine(line);
					string[] components = line.Split(new char[] { ' ' });
					int i = 0;
					Vector3 position = new Vector3();
					Vector3 normal = new Vector3();
					bool hasnormal = false;
					foreach (VertexFormat.Element e in elementlist)
					{
						string[] v = components[i++].Trim(new char[] { ']', '[' }).Split(new char[] { ',' });
                        //foreach (string s in v)
                         //   Console.WriteLine(s);
						if (e.Name == VertexFormat.ElementName.Position)
						{
							position = new Vector3(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
							sm.BBox.Add(position);
						}
						else if (e.Name == VertexFormat.ElementName.Normal)
						{
							normal = new Vector3(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
							hasnormal = true;
						}
						{
							for (int j = 0; j < e.Count; ++j)
							{
								vertices[vertexpos++] = float.Parse(v[j]);
							}
						}
					}
										/*string[] pos = components[0].Trim(new char[]{']','['}).Split(new char[] { ',' });
					string[] norm = components[1].Trim(new char[] { ']', '[' }).Split(new char[] { ',' });
					string[] tex0 = components[2].Trim(new char[] { ']', '[' }).Split(new char[] { ',' });
					Vector3 normal;
					Vector3 position;
					try
					{
						normal = new Vector3(float.Parse(norm[0]), float.Parse(norm[1]), float.Parse(norm[2]));
					}
					catch (Exception)
					{
						normal = new Vector3(0, 0, 0);
					}
                    position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));*/

					if (hasnormal)
					{
						normals[current * 2] = position;
						normals[current * 2 + 1] = position + normal;
						/*vertices[current++] = new Vertex(position.X, position.Y, position.Z,
							normal.X, normal.Y, normal.Z,
							float.Parse(tex0[0]), float.Parse(tex0[1])
							);*/
					}
				}
			}

			if (Root.Instance != null && Root.Instance.UserInterface != null)
			{
				sm.Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(vertices, vertices.Length * 4);
				//sm.Vertices.Format = VertexFormat.VF_P3T2N3;

				//sm.Normals=Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(normals,normals.Length*4*3);
				//sm.Normals.Format=new VertexFormat(new VertexFormat.Element[]{
				//																 new VertexFormat.Element(VertexFormat.ElementName.Position,3)
				//																	 });
			}
			else
			{
                //if (Root.Instance is Client)
                //    throw new Exception("renderer not initialized while loading mesh.");

				sm.Vertices = new VertexBuffer();
				sm.Vertices.Buffer = vertices;
				sm.Vertices.Size = vertices.Length * 4;
			}

			sm.Vertices.Format = vertexformat;
			sm.VertexCount = vertices.Length;
			sm.Indices = new IndexBuffer();
			sm.Indices.buffer = indices;
            sm.Vertices.Buffer = vertices;

			return sm;
		}

        public SubMesh LoadSubMesh(FileSystemNode n)
        {
            StreamReader r = new FixedStreamReader(n.getStream());
			return LoadSubMesh(r);
        }

		public SubMesh LoadSMF(StreamReader r)
		{
			string line;
			List<float> v=new List<float>();
			List<int> f=new List<int>();
			while ((line = r.ReadLine()) != null)
			{
				string[] split = line.Split(' ');
				switch (split[0][0])
				{
					case 'v':
						//Vector3 v = new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3]));
						v.Add(float.Parse(split[1]));
						v.Add(float.Parse(split[2]));
						v.Add(float.Parse(split[3]));
						break;
					case 'f':
						//int[] f=new int[3]{int.Parse(split[1]),int.Parse(split[2]),int.Parse(split[3])};
						f.Add(int.Parse(split[1]));
						f.Add(int.Parse(split[2]));
						f.Add(int.Parse(split[3]));
						break;
				}
			}

			float[] va = new float[v.Count];
			int[] fa = new int[f.Count];
			v.CopyTo(va);
			f.CopyTo(fa);

			SubMesh m = new SubMesh();
			m.Indices = new IndexBuffer();
			m.Indices.buffer = fa;
			m.Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(va, 12 * va.Length);
			m.Vertices.Buffer = va;
			m.Vertices.Format = VertexFormat.VF_P3;
			m.Vertices.Size = 12 * va.Length;

			return m;
		}

        public bool CanLoad(FileSystemNode n)
        {
            return n.info.Extension.ToLower() == ".submesh";
        }
    }

    public class CameraLoader : IResourceLoader
    {
        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Extension.ToLower() == ".camera";
        }
        public Type LoadType
        {
            get { return typeof(Camera); }
        }
        public IResource Load(FileSystemNode n)
        {
            return LoadCamera(n);
        }

        public Camera LoadCamera(FileSystemNode n)
        {
            StreamReader r = new StreamReader(n.getStream());
            string line;

            string header = r.ReadLine().Trim();
            if (header != "CAMERATEXT")
                return null;

            Camera cam = new Camera();

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("position:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    cam.Position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
                else if (line.StartsWith("rotation:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    cam.Orientation = new Quaternion(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                    //cam.Orientation = QuaternionExtensions.FromAxisAngle(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])/180.0f*3.14f);
                    continue;
                }
                else if (line.StartsWith("target:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    cam.LookAt( new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])));
                    continue;
                }
            }
            return cam;
        }
    }

    public class SceneLoader : IResourceLoader
    {
        public bool CanLoad(FileSystemNode n)
        {
            return n.info == null && n.GetName().ToLower().EndsWith(".scene");
        }
        public Type LoadType
        {
            get { return typeof(Scene); }
        }
        public IResource Load(FileSystemNode n)
        {
            return LoadScene(n);
        }

        public Scene LoadScene(FileSystemNode n)
        {
            Scene s = new Scene();
            foreach (DictionaryEntry de in n)
            {
                s.Spawn((Entity)Root.Instance.ResourceManager.Load((FileSystemNode)de.Value));
            }
            return s;
        }
    }

    public class NodeLoader : IResourceLoader
    {
        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Extension.ToLower() == ".node";
        }
        public Type LoadType
        {
            get { return typeof(Node); }
        }
        public IResource Load(FileSystemNode n)
        {
            return LoadNode(n);
        }

        public Node LoadNode(FileSystemNode n)
        {
            StreamReader r = new StreamReader(n.getStream());
            string line;

            string header = r.ReadLine().Trim();
            if (header != "NODETEXT0001")
                return null;
            Node node = new Node();

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("position:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    node.Position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
                else if (line.StartsWith("rotation:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    node.Orientation = new Quaternion(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                    continue;
                }
                else if (line.StartsWith("Mesh:"))
                {
                    string name=line.Split(new char[] { ':' })[1].Trim();
                    node.Draw.Add(
                        Root.Instance.ResourceManager.LoadMesh(
                            Root.Instance.FileSystem.Get(name)
                            ));
                    continue;
                }
            }
            return node;
        }
    }

    public class LightLoader : IResourceLoader
    {
        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Extension.ToLower() == ".light";
        }
        public Type LoadType
        {
            get { return typeof(Light); }
        }
        public IResource Load(FileSystemNode n)
        {
            return LoadLight(n);
        }

        public Light LoadLight(FileSystemNode n)
        {
            StreamReader r = new StreamReader(n.getStream());
            string line;

            string header = r.ReadLine().Trim();
            if (header != "LIGHTTEXT")
                return null;

            Light light = new Light();

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("position:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    light.Position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
                else if (line.StartsWith("rotation:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    light.Orientation = new Quaternion(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                    continue;
                }
                if (line.StartsWith("diffuse:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    light.diffuse = new Color4f(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
                else if (line.StartsWith("ambient:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    light.ambient = new Color4f(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
                else if (line.StartsWith("specular:"))
                {
                    string[] values = line.Split(new char[] { ':' })[1].Trim(new char[] { ' ', '[', ']' }).Split(',');
                    light.specular = new Color4f(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    continue;
                }
            }
            return light;
        }
    }

	public class EffectParameter
	{
		public IEffect Effect;
		public EffectParameter(IEffect e)
		{
			Effect = e;
		}
		public string Name
		{
			get
			{
				return Effect.GetParameterName(this);
			}
		}
	}

	public class EffectPass
	{
	}

	public enum EffectParameterType
	{
		Sampler2D, Float, Float4x4, Int
	}

    /*
	public class CgEffect : IEffect
	{
		public class CgEffectParameter : EffectParameter
		{
			public CgEffectParameter(CgEffect e) : base(e)
			{
			}

			public IntPtr ptr;
		}
		public class CgEffectPass : EffectPass
		{
			public IntPtr ptr;
		}

		public void Dispose()
		{
		}

		static CgEffect()
		{
			cgContext = Tao.Cg.Cg.cgCreateContext();
		}

		public virtual void SetParameter(EffectParameter param, Texture t)
		{
		}

		public CgEffect(string code)
		{
			cgEffect = Tao.Cg.Cg.cgCreateEffect(cgContext, code, IntPtr.Zero);
			if (cgEffect == IntPtr.Zero)
			{
				string s=Tao.Cg.Cg.cgGetLastListing(cgContext);
				throw new Exception(s);
			}

			IntPtr technique = Tao.Cg.Cg.cgGetFirstTechnique(cgEffect);
			while (technique!=IntPtr.Zero)
			{
				if (Tao.Cg.Cg.cgValidateTechnique(technique) == Tao.Cg.Cg.CG_FALSE)
					Console.WriteLine("Technique did not validate.  Skipping.");
				technique = Tao.Cg.Cg.cgGetNextTechnique(technique);
			}

			technique = Tao.Cg.Cg.cgGetFirstTechnique(cgEffect);
			while (Tao.Cg.Cg.cgIsTechniqueValidated(technique)==0)
				technique = Tao.Cg.Cg.cgGetNextTechnique(technique);
			if (technique==IntPtr.Zero)
			{
				throw new Exception("No valid techniques in effect file!  Exiting...");
			}

			IntPtr p = Tao.Cg.Cg.cgGetFirstEffectParameter(cgEffect);
			int c = 0;
			while (p != IntPtr.Zero)
			{
				c++;
				p = Tao.Cg.Cg.cgGetNextParameter(p);
			}
			parameters = new EffectParameter[c];
			p = Tao.Cg.Cg.cgGetFirstEffectParameter(cgEffect);
			c = 0;
			while (p != IntPtr.Zero)
			{
				CgEffectParameter param = new CgEffectParameter(this);
				param.ptr = p;
				parameters[c++] = param;
				p = Tao.Cg.Cg.cgGetNextParameter(p);
			}


			p = Tao.Cg.Cg.cgGetFirstPass(technique);
			c = 0;
			while (p != IntPtr.Zero)
			{
				c++;
				p = Tao.Cg.Cg.cgGetNextPass(p);
			}
			passes = new EffectPass[c];
			c = 0;
			p = Tao.Cg.Cg.cgGetFirstPass(technique);
			while (p != IntPtr.Zero)
			{
				CgEffectPass pass = new CgEffectPass();
				pass.ptr = p;
				passes[c++] = pass;
				p = Tao.Cg.Cg.cgGetNextPass(p);
			}
		}

		public EffectParameter[] Parameters
		{
			get{return parameters;}
		}
		public EffectPass[] Passes
		{
			get{return passes;}
		}

		public virtual void BeginPass(int pass)
		{
			CgEffectPass p = (CgEffectPass)(passes[pass]);
			Tao.Cg.Cg.cgSetPassState(p.ptr);
		}

		public void EndPass(int pass)
		{
			CgEffectPass p = (CgEffectPass)(passes[pass]);
			Tao.Cg.Cg.cgResetPassState(p.ptr);
		}

		public EffectParameter GetParameter(string name)
		{
			CgEffectParameter p = new CgEffectParameter(this);

			p.ptr=Tao.Cg.Cg.cgGetNamedEffectParameter(cgEffect, name);

			return p;
		}

		public EffectParameterType GetParameterType(EffectParameter param)
		{
			int i=Tao.Cg.Cg.cgGetParameterType(((CgEffectParameter)param).ptr);
			switch (i)
			{
				default:
					return EffectParameterType.Float;
			}
		}

		public string GetParameterName(EffectParameter param)
		{
			return Tao.Cg.Cg.cgGetParameterName(((CgEffectParameter)param).ptr);
		}

		public void SetParameter(EffectParameter param, float[] vector)
		{
			IntPtr p=((CgEffectParameter)param).ptr;
			switch (vector.Length)
			{
				case 1:
					Tao.Cg.Cg.cgSetParameter1f(p, vector[0]);
					break;
				case 2:
					Tao.Cg.Cg.cgSetParameter2fv(p, vector);
					break;
				case 3:
					Tao.Cg.Cg.cgSetParameter3fv(p, vector);
					break;
				case 4:
					Tao.Cg.Cg.cgSetParameter4fv(p, vector);
					break;
				case 16:
					Tao.Cg.Cg.cgSetMatrixParameterfc(p, vector);
					break;
				default:
					throw new Exception();
			}
		}

		protected static IntPtr cgContext;
		protected IntPtr cgEffect;
		protected EffectParameter[] parameters;
		protected EffectPass[] passes;
	}
    */
    public class ShaderParams : Dictionary<int,float[]>
    {
        public void Apply(IRenderer r)
        {
            foreach(KeyValuePair<int,float[]> kv in this)
            {
                r.SetUniform(kv.Key, kv.Value);
            }
        }
    }

    public class DummyShader : Shader
    {
        public override int GetUniformLocation(string name)
        {
            return 0;
        }
        public override int GetAttributeLocation(string name)
        {
            return 0;
        }
    }

    public abstract class Shader : IResource
    {
        public abstract int GetUniformLocation(string name);
        public abstract int GetAttributeLocation(string name);

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }

	public interface IEffect : IResource
	{
		EffectParameter[] Parameters
		{
			get;
		}
		EffectPass[] Passes
		{
			get;
		}
		void BeginPass(int pass);
		void EndPass(int pass);

		EffectParameter GetParameter(string name);

		EffectParameterType GetParameterType(EffectParameter param);

		string GetParameterName(EffectParameter param);
		void SetParameter(EffectParameter param,float[] vector);
		void SetParameter(EffectParameter param, Texture t);
	}

	public class EffectLoader : IResourceLoader
	{
		public IResource Load(FileSystemNode n)
		{
			Stream s=n.getStream();
			StreamReader r=new StreamReader(s);
            return null;
			/*if (Root.Instance is Client)
                throw new Exception("not implemented.");
				//return Root.Instance.UserInterface.Renderer.CreateEffect(r.ReadToEnd());
			else
				return null;*/
		}

		public Type LoadType
		{
			get { return typeof(IEffect); }
		}

		public bool CanLoad(FileSystemNode n)
		{
			return n.info != null && n.info.Extension.ToLower() == ".cgfx";
		}
	}

    public class ShaderLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            return LoadShader(n);
        }

        public Type LoadType
        {
            get { return typeof(Shader); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Extension.ToLower() == ".shader";
        }

        PrimitiveType ParsePrimitiveType(string text)
        {
            switch (text)
            {
                case "lines":
                    return PrimitiveType.LINES;
                case "points":
                    return PrimitiveType.POINTS;
                case "triangles":
                    return PrimitiveType.TRIANGLES;
                case "linestrip":
                    return PrimitiveType.LINESTRIP;
                case "trianglestrip":
                    return PrimitiveType.TRIANGLESTRIP;
                default:
                    throw new Exception("dont know primitive type: " + text);
            }
        }

        public Shader LoadShader(FileSystemNode n)
        {
            if (Root.Instance.UserInterface==null)
                return new DummyShader();

            Stream s = n.getStream();
            StreamReader r = new StreamReader(s);

            string line;

            string header = r.ReadLine().Trim();
            if (header != "SHADERTEXT")
                return null;

            string vertexshader=null;
            string fragmentshader=null;
            string geometryshader = null;
            bool vinline = false;
            bool finline = false;
            bool ginline = false;
            string vtext = "";
            string ftext = "";
            string gtext = "";
            PrimitiveType ?input=null;
            PrimitiveType ?output=null;

            while ((line = r.ReadLine()) != null)
            {
                string[] tmp;

                if(!(vinline || ginline || finline))
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("geometry input:"))
                    {
                        tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        input = ParsePrimitiveType(tmp[1].Trim());
                    }
                    else if (line.StartsWith("geometry output:"))
                    {
                        tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        output = ParsePrimitiveType(tmp[1].Trim());
                    }
                }

                if (line.StartsWith("vertexshader:"))
                {
                    tmp=line.Split(new char[] { ':' },StringSplitOptions.RemoveEmptyEntries);
                    if (tmp.Length > 1 && tmp[1].Trim()!=string.Empty)
                        vertexshader = tmp[1].Trim();
                    else
                    {
                        vinline = true;
                        finline = ginline = false;
                    }
                    continue;
                }
                else if (line.StartsWith("fragmentshader:"))
                {
                    tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tmp.Length > 1 && tmp[1].Trim() != string.Empty)
                        fragmentshader = tmp[1].Trim();
                    else
                    {
                        vinline = ginline = false;
                        finline = true;
                    }
                    continue;
                }
                else if (line.StartsWith("geometryshader:"))
                {
                    tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tmp.Length > 1 && tmp[1].Trim() != string.Empty)
                        geometryshader = tmp[1].Trim();
                    else
                    {
                        vinline = finline = false;
                        ginline = true;
                    }
                    continue;
                }

                if (vinline)
                {
                    vtext += line + "\r\n";
                }
                else if (finline)
                {
                    ftext += line + "\r\n";
                }
                else if (ginline)
                {
                    gtext += line + "\r\n";
                }
            }



            if (vertexshader != null)
            {
                vertexshader = new StreamReader(((FileSystemNode)n.parent[vertexshader]).getStream()).ReadToEnd();
                //vertexshader = new StreamReader(Root.Instance.FileSystem.Get(n.parent, vertexshader).getStream()).ReadToEnd();
            }
            else if(vtext!="")
            {
                vertexshader = vtext;
                //System.Console.WriteLine(vtext);
            }

            if (fragmentshader != null)
            {
                fragmentshader = new StreamReader(((FileSystemNode)n.parent[fragmentshader]).getStream()).ReadToEnd();
                //fragmentshader = new StreamReader(Root.Instance.FileSystem.Get(n.parent, fragmentshader).getStream()).ReadToEnd();
            }
            else if (ftext != "")
            {
                fragmentshader = ftext;
                //System.Console.WriteLine(ftext);
            }

            if (geometryshader != null)
            {
                geometryshader = new StreamReader(((FileSystemNode)n.parent[geometryshader]).getStream()).ReadToEnd();
                //fragmentshader = new StreamReader(Root.Instance.FileSystem.Get(n.parent, fragmentshader).getStream()).ReadToEnd();
            }
            else if (gtext != "")
            {
                geometryshader = gtext;
                //System.Console.WriteLine(ftext);
            }

            if (geometryshader!=null && !(input.HasValue && output.HasValue))
                throw new Exception("geometry shader primitive type missing.");

            return Root.Instance.UserInterface.Renderer.CreateShader(vertexshader, fragmentshader, geometryshader, input.HasValue ? input.Value : (PrimitiveType)0, output.HasValue ? output.Value : (PrimitiveType)0);
        }
    }


    public class MeshFontLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            //if (Root.Instance is Server)
            //    return null;

            Mesh[] meshes=new Mesh[256];

            for (int i = 0; i < meshes.Length; ++i)
            {
                string name1=i.ToString()+".mesh";
                string name2=((char)i).ToString()+".mesh";
                string name3 = ((char)i).ToString().ToLower() + ".mesh";
                string name4 = "_"+((char)i).ToString().ToLower() + ".mesh";
                Mesh m = null;
                if (n.ContainsKey(name1))
                {
                    m = Root.Instance.ResourceManager.LoadMesh((FileSystemNode)n[name1]);
                }
                else if (n.ContainsKey(name2))
                {
                    m = Root.Instance.ResourceManager.LoadMesh((FileSystemNode)n[name2]);
                }
                else if (n.ContainsKey(name3))
                {
                    m = Root.Instance.ResourceManager.LoadMesh((FileSystemNode)n[name3]);
                }
                else if (n.ContainsKey(name4))
                {
                    m = Root.Instance.ResourceManager.LoadMesh((FileSystemNode)n[name4]);
                }
                meshes[i] = m;
            }

            MeshFont f = new MeshFont(meshes);
            f.size = 48;
            f.Width =32;
            return f;
        }

        public Type LoadType
        {
            get { return typeof(MeshFont); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info==null;
        }

    }

    public class FontLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            Stream s = n.getStream();
            StreamReader r = new StreamReader(s);

            string line;

            string header = r.ReadLine().Trim();
            if (header != "FONTTEXT")
                return null;

            string texture=null;
            float width=-1;
            float size=-1;

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("size:"))
                {
                    size = float.Parse(line.Split(new char[] { ':' })[1].Trim());
                }
                else if (line.StartsWith("width:"))
                {
                    width = float.Parse(line.Split(new char[] { ':' })[1].Trim());
                }
                else if (line.StartsWith("texture:"))
                {
                    texture = line.Split(new char[] { ':' })[1].Trim();
                }
            }

            Font f = new Font(Root.Instance.ResourceManager.LoadTexture(texture));
            f.size = size;
            f.Width = width;

            return f;
        }

        public Type LoadType
        {
            get { return typeof(Font); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Extension.ToLower() == ".font";
        }

    }

    public class ShaderLib
    {
        public Shader Test
        {
            get
            {
                if (test == null)
                {
                    test = (Shader)Root.Instance.ResourceManager.Load("shaders/glsl/test.shader", typeof(Shader));
                }
                return test;
            }
        }

        Shader test;

        public static ShaderLib Instance
        {
            get
            {
                if (instance == null)
                    instance = new ShaderLib();
                return instance;
            }
        }

        static ShaderLib instance;
    }

	public class Freespace2MeshLoader : IResourceLoader
	{
		public IResource Load(FileSystemNode n)
		{
			Stream s = n.getStream();
			BinaryReader r = new BinaryReader(s);

			string id=new string(r.ReadChars(4));
			if (id != "PSPO")
				throw new Exception("wrong id.");
			int version = r.ReadInt32();

			while(true)
			{
				string chunk=new string(r.ReadChars(4));
				if (chunk.Length == 0)
					break;
				int length=r.ReadInt32();
				long curpos = s.Position;
				switch (chunk)
				{
					case "OHDR":
						break;
					case "TXTR":
						break;
					case "SPCL":
						break;
					case "SHLD":
						break;
					case " EYE":
						break;
					case "GPNT":
						break;
					case "MPNT":
						break;
					case "TGUN":
						break;
					case "TMIS":
						break;
					case "FUEL":
						break;
					case "OBJ2":
						ReadOBJ2(r);
						break;
				}
				s.Seek(curpos + length, SeekOrigin.Begin);
			}
			return null;
		}

		protected string ReadString(BinaryReader r)
		{
			int length = r.ReadInt32();
			return new string(r.ReadChars(length),0,length-1);
		}

		protected void ReadVertices(BinaryReader r)
		{
			long pos=r.BaseStream.Position;
			int numvertices = r.ReadInt32();
			int numnorms = r.ReadInt32();
			int offset = r.ReadInt32();
			int[] normcounts = new int[numvertices];
			for (int i = 0; i < numvertices; ++i)
				normcounts[i] = (int)r.ReadByte();
			r.BaseStream.Seek(pos + offset - 8, SeekOrigin.Begin);
			for (int i = 0; i < numvertices; ++i)
			{
				Vector3 v = ReadVector(r);
				for (int j = 0; j < normcounts[i]; ++j)
				{
					Vector3 n = ReadVector(r);
				}
			}
		}

		protected void ReadBspData(BinaryReader r)
		{
			while (true)
			{
				int what = r.ReadInt32();
				int length=r.ReadInt32();
				long pos = r.BaseStream.Position;
				switch (what)
				{
					case 0://end
						return;
					case 1://vertices
						ReadVertices(r);
						break;
					case 2://flat polys
						break;
					case 3://textured polys
						break;
				}
				r.BaseStream.Seek(pos + length - 8, SeekOrigin.Begin);
			}
		}

		protected Vector3 ReadVector(BinaryReader r)
		{
			return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
		}
		protected void ReadOBJ2(BinaryReader r)
		{
			int submodel = r.ReadInt32();
			float radius = r.ReadSingle();
			int parent = r.ReadInt32();
			Vector3 offset = ReadVector(r);
			Vector3 center = ReadVector(r);
			Vector3 bboxmin = ReadVector(r);
			Vector3 bboxmax = ReadVector(r);
			string name = ReadString(r);
			string properties = ReadString(r);
			int movetype = r.ReadInt32();
			int moveaxis = r.ReadInt32();
			int reserved = r.ReadInt32();
			int bspsize = r.ReadInt32();
			long pos = r.BaseStream.Position;
			ReadBspData(r);
			r.BaseStream.Seek(pos + bspsize, SeekOrigin.Begin);

		}

		public Type LoadType
		{
			get { return typeof(Mesh); }
		}

		public bool CanLoad(FileSystemNode n)
		{
			return n.info != null && n.info.Extension.ToLower() == ".pof";
		}
	}

    public class MeshLoader : IResourceLoader
	{
		public IResource Load(FileSystemNode n)
		{
            if (n.info != null)
            {
                if (n.info.Extension.ToLower() == ".obj")
                    return LoadObj(n);
                else if (n.info.Extension.ToLower() == ".lwo")
                    return LoadLWO(n);
                else
                    return LoadMesh(n);
            }
            return null;
        }

        protected Mesh LoadLWO(FileSystemNode n)
        {
            Cheetah.Lightwave.LWO lwo = new Cheetah.Lightwave.LWO(n.getStream());
            Mesh m = new Mesh();
            //List<Cheetah.Lightwave.LWO.Part> parts=lwo.GetParts();
            foreach(Cheetah.Lightwave.LWO.Part p in lwo.GetParts())
            {
                SubMesh sm = new SubMesh();
                m.SubMeshes.Add(sm);
                sm.Material = Material.CreateSimpleMaterial(null);
                sm.Material.NoLighting = true;
                sm.Material.Shader = Root.Instance.ResourceManager.LoadShader("emissivemap.shader");
                //sm.Material.Wire = true;

                sm.Indices = new IndexBuffer();
                sm.Indices.buffer = p.Triangles.ToArray();

                string texture=lwo.Surfaces[p.Surface].DiffuseTexture;
                string[] split=texture.Split('/');
                texture = split[split.Length - 1];
                //if (texture == "Assault_back_export[1].bmp")
                //    texture = "Assault_back_export[1].dds";
                sm.Material.diffusemap = Root.Instance.ResourceManager.LoadTexture(texture);
                sm.Material.EmissiveMap = Root.Instance.ResourceManager.LoadTexture(texture);

                VertexP3T2[] v = new VertexP3T2[p.Points.Length / 3];
                for (int j = 0; j < v.Length; ++j)
                {
                    v[j].position = new Vector3(p.Points[j * 3 + 0], p.Points[j * 3 + 1], p.Points[j * 3 + 2]);
                    Vector2 uv = new Vector2(p.TextureUV[j * 2 + 0], 1-p.TextureUV[j * 2 + 1]);

                    v[j].texture0 = uv;
                }
                sm.Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(v, v.Length*(3+2)*4);
                sm.VertexCount = v.Length;
                sm.Vertices.Format = VertexFormat.VF_P3T2;
               
                //m.
            }
            m.BBox = new BoundingBox(new Vector3(-50, -50, -50), new Vector3(50, 50, 50));
            return m;
        }

        protected Mesh LoadMesh(FileSystemNode n)
        {
            Mesh m = new Mesh();
            /*foreach (DictionaryEntry de in n)
            {
                FileSystemNode n2 = (FileSystemNode)de.Value;
                if (n2.info!=null && n2.info.Extension.ToLower() == ".submesh")
                {
                    SubMesh sm=(SubMesh)Root.Instance.ResourceManager.Load(n2, typeof(SubMesh));
                    m.SubMeshes.Add(sm);
                    //sm.Material.Wire = true;
                }
            }*/

           StreamReader r = new StreamReader(n.getStream());
            string line;

            string header = r.ReadLine().Trim();
            if (header != "MESHTEXT")
                return null;


            Material currentmaterial = null;
            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("material:"))
                {
                    string filename=line.Split(new char[] { ':' })[1].Trim();
                    currentmaterial = (Material)Root.Instance.ResourceManager.Load(
                        Root.Instance.FileSystem.Get(n.parent,filename), typeof(Material));
                    continue;
                }
                else if (line.StartsWith("submesh:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();

                    try
                    {
                        Root.Instance.FileSystem.Get(n.parent, filename + ".bin");
                        filename += ".bin";
                    }
                    catch(Exception e)
                    {
                    }

                    SubMesh sm = (SubMesh)Root.Instance.ResourceManager.Load(
                        Root.Instance.FileSystem.Get(n.parent, filename), typeof(SubMesh));
                    sm.Material = currentmaterial;
                    m.BBox.Combine(sm.BBox);
                    m.SubMeshes.Add(sm);
                    continue;
                }
            }


            return m;
        }

        public Type LoadType
		{
			get{return typeof(Mesh);}
		}
	
		protected Mesh CreateMesh(ObjFace[] faces)
		{
			return null;
		}

		protected void LoadMtl(FileSystemNode n,Hashtable list)
		{
			StreamReader r=new StreamReader(n.getStream());
			string line;
			string newmtl="";
			Material mat=null;

			while((line=r.ReadLine())!=null)
			{
				line=line.Trim();

				string replaced;
				while(true)
				{
					replaced=line.Replace("  "," ");
					if(replaced==line)
						break;
					else
						line=replaced;
				}

				if(line.Length==0||line[0]=='#')continue;
				
				string[] tokens=line.Split(new char[]{' ','\t'});
				switch(tokens[0])
				{
					case "newmtl":
						newmtl=tokens[1];
						list[newmtl]=mat=new Material();
						break;
					case "map_Kd":
						mat.diffusemap=Root.Instance.ResourceManager.LoadTexture(tokens[1]);
						break;
					case "map_Env"://gibts normalerweise nicht in .obj
						mat.EnvironmentMap=Root.Instance.ResourceManager.LoadTexture(tokens[1]);
						break;
					case "Ka":
						if(tokens.Length==4)
							mat.ambient=new Color4f(float.Parse(tokens[1]),float.Parse(tokens[2]),float.Parse(tokens[3]));
						else if(tokens.Length==5)
							mat.ambient=new Color4f(float.Parse(tokens[1]),float.Parse(tokens[2]),float.Parse(tokens[3]),float.Parse(tokens[4]));
						else
							throw new Exception("cant parse color.");
						break;
					case "Ks":
						if(tokens.Length==4)
							mat.specular=new Color4f(float.Parse(tokens[1]),float.Parse(tokens[2]),float.Parse(tokens[3]));
						else if(tokens.Length==5)
							mat.specular=new Color4f(float.Parse(tokens[1]),float.Parse(tokens[2]),float.Parse(tokens[3]),float.Parse(tokens[4]));
						else
							throw new Exception("cant parse color.");
						break;
					case "Kd":
						if(tokens.Length==4)
							mat.diffuse=new Color4f(float.Parse(tokens[1]),float.Parse(tokens[2]),float.Parse(tokens[3]));
						else if(tokens.Length==5)
							mat.diffuse=new Color4f(float.Parse(tokens[1]),float.Parse(tokens[2]),float.Parse(tokens[3]),float.Parse(tokens[4]));
						else
							throw new Exception("cant parse color.");
						break;
					case "illum":
						int i=int.Parse(tokens[1]);
						mat.NoLighting=(i==0);
                        break;
                    case "twosided":
                        mat.twosided = true;
                        break;
				}
			}
		}
		public Mesh LoadObj(FileSystemNode n)
		{
			StreamReader r=new StreamReader(n.getStream());

			ArrayList v=new ArrayList();
			ArrayList vn=new ArrayList();
			ArrayList vt=new ArrayList();
			//ArrayList f=new ArrayList();
			Hashtable g_usemtl=new Hashtable();
			//["<g> <usemtl>"]=>ArrayList<ObjVertex>
			Hashtable mat=new Hashtable();

			string line;
			string usemtl="";
			string g="";
			string o="";
			int sg=-1;
			while((line=r.ReadLine())!=null)
			{
				line=line.Trim();
                
                while(line.EndsWith("\\"))
                {
                    line=line.TrimEnd(new char[] { '\\' });
                    line+=r.ReadLine();
                    line.Trim();
                }

				string replaced;
				while(true)
				{
					replaced=line.Replace("  "," ");
					if(replaced==line)
						break;
					else
						line=replaced;
				}


				if(line.Length==0||line[0]=='#')continue;

				string[] tokens=line.Split(new char[]{' ','\t'});
				switch(tokens[0])
				{
					case "v":
						if(FlipYz)
							v.Add(new Vector3(float.Parse(tokens[1]),float.Parse(tokens[3]),float.Parse(tokens[2])));
						else
							v.Add(new Vector3(float.Parse(tokens[1]),float.Parse(tokens[2]),float.Parse(tokens[3])));
						break;
					case "vn":
						if(FlipYz)
							vn.Add(new Vector3(float.Parse(tokens[1]),float.Parse(tokens[3]),float.Parse(tokens[2])));
						else
							vn.Add(new Vector3(float.Parse(tokens[1]),float.Parse(tokens[2]),float.Parse(tokens[3])));
						break;
					case "vt":
						vt.Add(new Vector2(float.Parse(tokens[1]),float.Parse(tokens[2])));
						break;
					case "scale":
						Scale=float.Parse(tokens[1]);
						break;
					case "f":
						string[] f1=tokens[1].Split(new Char[]{'/'});
						string[] f2=tokens[2].Split(new Char[]{'/'});
						string[] f3=tokens[3].Split(new Char[]{'/'});
						if(!(f1.Length==f2.Length&&f2.Length==f3.Length))
							throw new Exception();
						bool has_texture;
						bool has_normal;
						ObjFace face=new ObjFace();

						if(f1.Length>1)
						{
							if(f1.Length==3)
							{
								has_texture=(f1[1].Length>0);
								has_normal=true;
							}
							else if(f1.Length==2)
							{
								has_normal=false;
								has_texture=true;
							}
							else
								throw new Exception();
						}
						else
						{
							has_texture=false;
							has_normal=false;
						}

						string key=g+" "+usemtl;
						ArrayList list=(ArrayList)g_usemtl[key];
						if(list==null)
							g_usemtl[key]=list=new ArrayList();
						
						Vector3 p;
						Vector2 t;
						Vector3 nv;

						p=(Vector3)v[int.Parse(f1[0])-1];
						if(has_texture)
							t=(Vector2)vt[int.Parse(f1[1])-1];
						else
							t=new Vector2(0,0);
						if(has_normal)
							nv=(Vector3)vn[int.Parse(f1[2])-1];
						else
							nv=new Vector3(0,0,0);
						//list.Add(new ObjVertex(p.X,p.Y,p.z,t.X,-t.Y,nv.X,nv.Y,nv.z));
						face.Vertex1=new ObjVertex(p.X,p.Y,p.Z,t.X,-t.Y,nv.X,nv.Y,nv.Z);

						p=(Vector3)v[int.Parse(f2[0])-1];
						if(has_texture)
							t=(Vector2)vt[int.Parse(f2[1])-1];
						else
							t=new Vector2(0,0);
						if(has_normal)
							nv=(Vector3)vn[int.Parse(f2[2])-1];
						else
							nv=new Vector3(0,0,0);
						//list.Add(new ObjVertex(p.X,p.Y,p.z,t.X,-t.Y,nv.X,nv.Y,nv.z));
						face.Vertex2=new ObjVertex(p.X,p.Y,p.Z,t.X,-t.Y,nv.X,nv.Y,nv.Z);

						p=(Vector3)v[int.Parse(f3[0])-1];
						if(has_texture)
							t=(Vector2)vt[int.Parse(f3[1])-1];
						else
							t=new Vector2(0,0);
						if(has_normal)
							nv=(Vector3)vn[int.Parse(f3[2])-1];
						else
							nv=new Vector3(0,0,0);
						//list.Add(new ObjVertex(p.X,p.Y,p.z,t.X,-t.Y,nv.X,nv.Y,nv.z));
						face.Vertex3=new ObjVertex(p.X,p.Y,p.Z,t.X,-t.Y,nv.X,nv.Y,nv.Z);
						face.SmoothingGroup=sg;
						list.Add(face);
						break;
					case "mtllib":
						string filename=tokens[1];
						if(filename.StartsWith("./"))
							filename=filename.Remove(0,2);
						LoadMtl((FileSystemNode)n.parent[filename],mat);
						break;
					case "usemtl":
						usemtl=tokens[1];
						break;
					case "s":
						try
						{
							sg=int.Parse(tokens[1]);
						}
						catch(Exception)
						{
							sg=-1;
						}
						break;
					case "o":
						o=tokens[1];
						break;
					case "g":
						if(tokens.Length>1)
							g=tokens[1];
						else
							g="";
						break;
					default:
						throw new Exception("unknown token: "+tokens[0]);
				}
			}

			Mesh mesh=new Mesh();
			//Mesher completemesh=new Mesher();
			foreach(DictionaryEntry de in g_usemtl)
			{
				string key=(string)de.Key;
				SubMesh sm=new SubMesh();
				ArrayList list=(ArrayList)g_usemtl[key];
				if(key==" ")
				{
					//kein material,keine gruppe
				}
				else if(key[0]==' ')
				{
					//keine gruppe
					sm.Material=(Material)mat[key.Substring(1)];
					if(sm.Material==null)
						throw new Exception();
				}
				else if(key[key.Length-1]==' ')
				{
					//kein material
				}
				else
				{
					string[] s=key.Split(new char[]{' '});
					sm.Material=(Material)mat[s[1]];
					if(sm.Material==null)
						throw new Exception();
					//byte[] data=
				}
				
				VertexFormat format=new VertexFormat(new VertexFormat.Element[]{
																				   new VertexFormat.Element(VertexFormat.ElementName.Position,3),
																				   new VertexFormat.Element(VertexFormat.ElementName.Texture0,2),
																				   new VertexFormat.Element(VertexFormat.ElementName.Normal,3)
																			   });
				Mesher mesher=new Mesher();
				foreach(ObjFace f in list)
				{
					mesher.FillFace(f);
					//completemesh.FillFace(f);
				}
				mesher.Consolidate();
				
				int vertexcount=mesher.Vertices.Count;
				byte[] data=new byte[vertexcount*format.Size];
				BinaryWriter w=new BinaryWriter(new MemoryStream(data));
				for(int i=0;i<vertexcount;++i)
				{
					ObjVertex vtx=(ObjVertex)mesher.Vertices[i];
					w.Write(vtx.Position.X*Scale);w.Write(vtx.Position.Y*Scale);w.Write(vtx.Position.Z*Scale);
					w.Write(vtx.Texture0.X);w.Write(vtx.Texture0.Y);
					w.Write(vtx.Normal.X);w.Write(vtx.Normal.Y);w.Write(vtx.Normal.Z);

                    mesh.BBox.Add(vtx.Position*Scale);
                    //mesh.BoundingBox.X = Math.Max(mesh.BoundingBox.X, Math.Abs(vtx.Position.X) * Scale * 2);
                    //mesh.BoundingBox.Y = Math.Max(mesh.BoundingBox.Y, Math.Abs(vtx.Position.Y) * Scale * 2);
                    //mesh.BoundingBox.Z = Math.Max(mesh.BoundingBox.Z, Math.Abs(vtx.Position.Z) * Scale * 2);
                }

                if(Root.Instance.UserInterface!=null)
				{
					sm.Vertices=Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data,data.Length);
					sm.Vertices.Format=format;
					if(mesher.Normals!=null)
					{
						sm.Normals=Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(mesher.Normals,mesher.Normals.Length*4*3);
						sm.Normals.Format=new VertexFormat(new VertexFormat.Element[]{
																						 new VertexFormat.Element(VertexFormat.ElementName.Position,3)
																					 });
					}
				}
				sm.VertexCount=vertexcount;
				sm.Indices=new IndexBuffer();
				sm.Indices.buffer=new int[mesher.Index.Count];
				//for(int i=0;i<mesher.Index.Count;++i)
				//	sm.Indices.buffer[i]=(uint)((int)mesher.Index[i]);
                for (int i = 0; i < mesher.Index.Count / 3; ++i)
                {
                    sm.Indices.buffer[i*3] = (int)mesher.Index[i*3+2];
                    sm.Indices.buffer[i*3+1] = (int)mesher.Index[i*3+1];
                    sm.Indices.buffer[i*3+2] = (int)mesher.Index[i*3];
                }

                mesh.SubMeshes.Add(sm);
			}

			/*completemesh.Consolidate();
			int[] cindex=new int[completemesh.Index.Count];
			Ode.dVector3[] cvertices=new Ode.dVector3[completemesh.Vertices.Count];
			for(int i=0;i<completemesh.Vertices.Count;++i)
			{
				ObjVertex vtx=(ObjVertex)completemesh.Vertices[i];
				cvertices[i]=new Ode.dVector3(vtx.Position.X*Scale,vtx.Position.Y*Scale,vtx.Position.Z*Scale);
				mesh.BoundingBox.X=Math.Max(mesh.BoundingBox.X,Math.Abs(vtx.Position.X)*Scale*2);
				mesh.BoundingBox.Y=Math.Max(mesh.BoundingBox.Y,Math.Abs(vtx.Position.Y)*Scale*2);
				mesh.BoundingBox.Z=Math.Max(mesh.BoundingBox.Z,Math.Abs(vtx.Position.Z)*Scale*2);
			}
			for(int i=0;i<completemesh.Index.Count;++i)
				cindex[i]=(int)completemesh.Index[i];
			mesh.CollisionMesh=new OdeTriMeshData(cvertices,cindex);*/
			return mesh;
		}

		public bool CanLoad(FileSystemNode n)
		{
            return n.info != null && (n.info.Extension.ToLower() == ".mesh" || n.info.Extension.ToLower() == ".obj"|| n.info.Extension.ToLower() == ".lwo");
        }

		public bool FlipYz=true;
		public float Scale=1;//0.1f;
	}

	/*
	 * 		protected void ExportModel(Model model,BinaryWriter w)
		{
			w.Write(model.GetMaterialCount());
			for(int i=0;i<model.GetMaterialCount();++i)
			{
				ExportMaterial(model.GetMaterialAt(i),w);
			}
			w.Write(model.GetMeshCount());
			for(int i=0;i<model.GetMeshCount();++i)
			{
				ExportMesh(model.GetMeshAt(i),w);
			}
		}

		protected void ExportMesh(Mesh mesh,BinaryWriter w)
		{
			//vertex format: pos3,tex2,norm3
			//primtype:trianglelist
			w.Write(mesh.GetName());
			w.Write(mesh.GetMaterialIndex());
			w.Write(mesh.GetTriangleCount()*3);
			for(int i=0;i<mesh.GetTriangleCount();++i)
			{
				Triangle t=mesh.GetTriangleAt(i);
				ushort3 vi=t.GetVertexIndices();
				ushort3 ni=t.GetNormalIndices();
				for(int j=0;j<3;++j)
				{
					Vertex v=mesh.GetVertexAt(vi[j]);
					MsVec3 n=mesh.GetVertexNormalAt(ni[j]);
					w.Write(v.GetVertex().X);w.Write(v.GetVertex().Y);w.Write(v.GetVertex().z);
					w.Write(v.GetTexCoords().X);w.Write(v.GetTexCoords().Y);
					w.Write(n.X);w.Write(n.Y);w.Write(n.z);
				}
			}
		}

		protected void ExportMaterial(Material mat,BinaryWriter w)
		{
			w.Write(mat.GetDiffuseTexture());
		}
	 */

    public struct BoundingBox
    {
        public BoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
            Valid = true;
        }

        public void Add(Vector3 v)
        {
            if (Valid)
            {
                Min.X = Math.Min(v.X, Min.X);
                Min.Y = Math.Min(v.Y, Min.Y);
                Min.Z = Math.Min(v.Z, Min.Z);
                Max.X = Math.Max(v.X, Max.X);
                Max.Y = Math.Max(v.Y, Max.Y);
                Max.Z = Math.Max(v.Z, Max.Z);
            }
            else
            {
                Min = Max = v;
                Valid = true;
            }
        }

        public void Combine(BoundingBox bbox2)
        {
            if (bbox2.Valid)
            {
                Add(bbox2.Min);
                Add(bbox2.Max);
            }
        }

        public Vector3 Size
        {
            get
            {
                return Max - Min;
            }
        }

        public Vector3 Center
        {
            get
            {
                return (Min + Max) * 0.5f;
            }
        }

        public float Radius
        {
            get
            {
                Vector3 maxsize = new Vector3(
                    Math.Max(Math.Abs(Min.X), Math.Abs(Max.X)),
                    Math.Max(Math.Abs(Min.Y), Math.Abs(Max.Y)),
                    Math.Max(Math.Abs(Min.Z), Math.Abs(Max.Z))
                    );
                return (maxsize).Length;
            }
        }

        public bool Valid;
        public Vector3 Min;
        public Vector3 Max;
    }
/*
	public class VertexProgram : IResource
	{
		public void Dispose()
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
	public class FragmentProgram : IResource
	{
		public void Dispose()
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public class VertexProgramLoader : IResourceLoader
	{
		public IResource Load(FileSystemNode n)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public Type LoadType
		{
			get { throw typeof(VertexProgram); }
		}

		public bool CanLoad(FileSystemNode n)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public class FragmentProgramLoader : IResourceLoader
	{
		public IResource Load(FileSystemNode n)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public Type LoadType
		{
			get { return typeof(FragmentProgram); }
		}

		public bool CanLoad(FileSystemNode n)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
*/
    public class SupComMeshLoader : IResourceLoader
    {
        #region IResourceLoader Members

        public IResource Load(FileSystemNode n)
        {
            SupCom.ScmFile scm = new SupCom.ScmFile(n.getStream());

            VertexBone[] v = new VertexBone[scm.VertexData.Length];
            VertexP3C4T2[] normals = new VertexP3C4T2[v.Length * 2 * 3];

            BoundingBox bbox = new BoundingBox();

            for (int i = 0; i < scm.VertexData.Length; ++i)
            {
                v[i].Position = new Vector3(scm.VertexData[i].mPosition.X, scm.VertexData[i].mPosition.Y, scm.VertexData[i].mPosition.Z);
                v[i].Normal = new Vector3(scm.VertexData[i].mNormal.X, scm.VertexData[i].mNormal.Y, scm.VertexData[i].mNormal.Z);
                v[i].Texture = new Vector3(scm.VertexData[i].mUV0.X, scm.VertexData[i].mUV0.Y, scm.VertexData[i].mBoneIndex0);
                v[i].Binormal = new Vector3(scm.VertexData[i].mBinormal.X, scm.VertexData[i].mBinormal.Y, scm.VertexData[i].mBinormal.Z);
                v[i].Tangent = new Vector3(scm.VertexData[i].mTangent.X, scm.VertexData[i].mTangent.Y, scm.VertexData[i].mTangent.Z);

                normals[i * 6].position = v[i].Position;
                normals[i * 6].texture0.X = scm.VertexData[i].mBoneIndex0;
                normals[i * 6].color = new Color4f(1.0f, 0.0f, 0.0f, 1.0f);
                normals[i * 6 + 1].position = v[i].Position + v[i].Tangent;
                normals[i * 6 + 1].texture0.X = scm.VertexData[i].mBoneIndex0;
                normals[i * 6 + 1].color = new Color4f(1.0f, 0.0f, 0.0f, 1.0f);

                normals[i * 6 + 2].position = v[i].Position;
                normals[i * 6 + 2].texture0.X = scm.VertexData[i].mBoneIndex0;
                normals[i * 6 + 2].color = new Color4f(0.0f, 1.0f, 0.0f, 1.0f);
                normals[i * 6 + 3].position = v[i].Position + v[i].Binormal;
                normals[i * 6 + 3].texture0.X = scm.VertexData[i].mBoneIndex0;
                normals[i * 6 + 3].color = new Color4f(0.0f, 1.0f, 0.0f, 1.0f);

                normals[i * 6 + 4].position = v[i].Position;
                normals[i * 6 + 4].texture0.X = scm.VertexData[i].mBoneIndex0;
                normals[i * 6 + 4].color = new Color4f(0.0f, 0.0f, 1.0f, 1.0f);
                normals[i * 6 + 5].position = v[i].Position + v[i].Normal;
                normals[i * 6 + 5].texture0.X = scm.VertexData[i].mBoneIndex0;
                normals[i * 6 + 5].color = new Color4f(0.0f, 0.0f, 1.0f, 1.0f);

                bbox.Add(v[i].Position);
            }

            IndexBuffer ib=new IndexBuffer();
            ib.buffer=new int[scm.TriangleData.Length*3];
            for(int i=0;i<scm.TriangleData.Length;++i)
            {
                ib.buffer[i * 3] = scm.TriangleData[i].Index0;
                ib.buffer[i * 3 + 1] = scm.TriangleData[i].Index1;
                ib.buffer[i * 3 + 2] = scm.TriangleData[i].Index2;
            }

            VertexBuffer vb = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(v, VertexBone.Format.Size * v.Length);
            vb.Format=VertexBone.Format;

            VertexBuffer vbn = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(normals, VertexFormat.VF_P3C4T2.Size * normals.Length);
            vbn.Format = VertexFormat.VF_P3C4T2;

            Bone[] bones = new Bone[scm.BoneData.Length];
            for (int i = 0; i < scm.BoneData.Length;++i )
            {
                bones[i]=new Bone();
                bones[i].Index=i;
                bones[i].Name=scm.BoneNames[i];
                bones[i].Orientation=new Quaternion(scm.BoneData[i].mRotation.X,scm.BoneData[i].mRotation.Y,scm.BoneData[i].mRotation.Z,-scm.BoneData[i].mRotation.W);
                bones[i].Position = new Vector3(scm.BoneData[i].mPosition.X, scm.BoneData[i].mPosition.Y, scm.BoneData[i].mPosition.Z);
                bones[i].RestPoseInverse = new Matrix4();
                for (int x = 0; x < 4; ++x)
                {
                    for (int y = 0; y < 4; ++y)
                    {
                        Matrix4Extensions.Set(bones[i].RestPoseInverse, x, y, scm.BoneData[i].mRestPoseInverse[y, x]);
                    }
                }
            }
            for (int i = 0; i < scm.BoneData.Length;++i )
            {
                if (scm.BoneData[i].HasParent)
                {
                    if (bones[i].Parent != null)
                        throw new Exception();

                    bones[i].Parent = bones[scm.BoneData[i].mParentBoneIndex];
                    bones[i].Parent.Children.Add(bones[i]);
                }
            }

            SkeletalMesh m = new SkeletalMesh(
                vb,
                ib,
                bones,
                Root.Instance.ResourceManager.LoadShader("bones.light.shader"),
                vbn,
                bbox
                );

            return m;
        }

        public Type LoadType
        {
            get { return typeof(SkeletalMesh); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info!=null && n.info.Name.ToLower().EndsWith(".scm");
        }

        #endregion
    }

    public class SupComModelLoader : IResourceLoader
    {
        #region IResourceLoader Members

        public IResource Load(FileSystemNode n)
        {
            string name = n.dir.Name.ToLower();
            string meshname = name + "_lod0.scm";
            string texture = name + "_albedo.dds";
            string normal = name + "_normalsts.dds";
            string team = name + "_specteam.dds";

            Material mat=null;
            List<SkeletalAnimation> anims = new List<SkeletalAnimation>();
            SkeletalMesh mesh=null;

            Texture albedotex=null;
            Texture teamtex=null;
            Texture normaltex = null;

            foreach (DictionaryEntry de in n)
            {
                FileSystemNode n2 = (FileSystemNode)de.Value;
                string filename = ((string)de.Key).ToLower();
                if (filename == meshname)
                {
                    mesh = (SkeletalMesh)Root.Instance.ResourceManager.Load(n2, typeof(SkeletalMesh));
                }
                else if (filename == team)//(filename == texture)
                {
                    teamtex = Root.Instance.ResourceManager.LoadTexture(n2);
                }
                else if (filename == texture)
                {
                    albedotex = Root.Instance.ResourceManager.LoadTexture(n2);
                }
                else if (filename == normal)
                {
                    normaltex = Root.Instance.ResourceManager.LoadTexture(n2);
                }
            }
            mat = new Material();
            mat.DepthTest = true;
            mat.DepthWrite = true;
            mat.twosided = false;
            mat.wire = false;
            mat.Shader = Root.Instance.ResourceManager.LoadShader("bones.light.shader");
            mat.NoLighting = false;
            mat.diffusemap = albedotex;
            mat.EmissiveMap = teamtex;
            mat.BumpMap = normaltex;

            // mesh must be loaded before animations

            foreach (DictionaryEntry de in n)
            {
                FileSystemNode n2 = (FileSystemNode)de.Value;
                string filename = ((string)de.Key).ToLower();
                if (filename.EndsWith(".sca"))
                {
                    string animname = filename.Substring(name.Length + 1, filename.Length - name.Length - 1 - 4);

                    SkeletalAnimation a = (SkeletalAnimation)Root.Instance.ResourceManager.Load(n2, typeof(SkeletalAnimation));
                    a.Mesh = mesh;
                    a.Name = animname;
                    anims.Add(a);
                }
            }

            if (mesh == null || mat == null)
                throw new Exception();

            
            return new Model(mat, anims.ToArray(), mesh);
        }

        public Type LoadType
        {
            get { return typeof(Model); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info == null && n.dir!=null;
        }

        #endregion
    }

    public class SupComMapLoader : IResourceLoader
    {
        public class Heightmap : IHeightMap
        {
            public Heightmap(SupCom.ScmapFile scmap)
            {
                Scmap = scmap;
            }
            SupCom.ScmapFile Scmap;

            #region IHeightMap Members

            public int GetHeight(int x, int y)
            {
                return Scmap.Heightmap[x, y];
            }

            public Point Size
            {
                get
                {
                    return new Point(Scmap.Heightmap.GetLength(0), Scmap.Heightmap.GetLength(1));
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion
        }

        #region IResourceLoader Members

        public IResource Load(FileSystemNode n)
        {
            SupCom.ScmapFile scmap = new SupCom.ScmapFile(n.getStream());

            return new SupComMap(scmap);
        }

        public Type LoadType
        {
            get { return typeof(SupComMap); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Name.ToLower().EndsWith(".scmap");
        }

        #endregion
    }

    public class SupComAnimationLoader : IResourceLoader
    {
        #region IResourceLoader Members

        public IResource Load(FileSystemNode n)
        {
            SupCom.ScaFile sca = new SupCom.ScaFile(n.getStream());

            SkeletalAnimation.KeyFrame[] frames = new SkeletalAnimation.KeyFrame[sca.AnimData.mFrames.Length];
            for (int i = 0; i < frames.Length; ++i)
            {
                frames[i].Time = sca.AnimData.mFrames[i].mTime;
                frames[i].Bones = new SkeletalAnimation.BoneFrame[sca.AnimData.mFrames[i].mBones.Length];
                for (int j = 0; j < frames[i].Bones.Length; ++j)
                {
                    frames[i].Bones[j].Orientation = new Quaternion(
                        sca.AnimData.mFrames[i].mBones[j].mRotation.X,
                        sca.AnimData.mFrames[i].mBones[j].mRotation.Y,
                        sca.AnimData.mFrames[i].mBones[j].mRotation.Z,
                        -sca.AnimData.mFrames[i].mBones[j].mRotation.W);
                    frames[i].Bones[j].Position = new Vector3(
                        sca.AnimData.mFrames[i].mBones[j].mPosition.X,
                        sca.AnimData.mFrames[i].mBones[j].mPosition.Y,
                        sca.AnimData.mFrames[i].mBones[j].mPosition.Z);
                }
            }

            return new SkeletalAnimation(frames,sca.BoneNames);
        }

        public Type LoadType
        {
            get { return typeof(SkeletalAnimation); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Name.ToLower().EndsWith(".sca");
        }

        #endregion
    }

    public class Bone
    {
        public Vector3 Position;
        public Quaternion Orientation;
        public string Name;
        public Bone Parent;
        public List<Bone> Children=new List<Bone>();
        public int Index;
        public Matrix4 RestPoseInverse;

        public Matrix4 GetMatrix()
        {
            Matrix4 m = Matrix4Extensions.FromQuaternion(Orientation);
            //m[12] = Position.X;
            //m[13] = Position.Y;
            //m[14] = Position.Z;
            m.Row3.X = Position.X;
            m.Row3.Y = Position.Y;
            m.Row3.Z = Position.Z;

            return m;
        }
    }

    public class SkeletalAnimation : IResource, IDrawable, ITickable
    {
        public SkeletalAnimation(KeyFrame[] frames, string[] names)
        {
            KeyFrames = frames;
            BoneNames = names;
        }

        public struct BoneFrame
        {
            public Vector3 Position;
            public Quaternion Orientation;
            public Matrix4 GetMatrix()
            {
                Matrix4 m = Matrix4Extensions.FromQuaternion(Orientation);
                //m[12] = Position.X;
                //m[13] = Position.Y;
                //m[14] = Position.Z;
                m.Row3.X = Position.X;
                m.Row3.Y = Position.Y;
                m.Row3.Z = Position.Z;

                return m;
            }
        }

        public struct KeyFrame
        {
            public float Time;
            public BoneFrame[] Bones;
        }
        public string Name;
        public string[] BoneNames;
        public KeyFrame[] KeyFrames;

        public SkeletalMesh Mesh;
        public float CurrentTime;

        public BoneFrame GetBoneFrame(int index, int frame)
        {
            return KeyFrames[frame].Bones[index];
        }
        public BoneFrame GetBoneFrame(int index)
        {
            int i1=0,i2=0;
            for (int i = 0; i < KeyFrames.Length; ++i)
            {
                if (KeyFrames[i].Time > CurrentTime)
                {
                    i1 = i > 0 ? i - 1 : 0;
                    i2 = i;
                    break;
                }
            }
            if(i1==i2)
                return KeyFrames[i1].Bones[index];

            float a=(CurrentTime-KeyFrames[i1].Time)/(KeyFrames[i2].Time-KeyFrames[i1].Time);
            BoneFrame interpolated;
            interpolated.Orientation = Quaternion.Slerp(KeyFrames[i1].Bones[index].Orientation, KeyFrames[i2].Bones[index].Orientation, a);
            interpolated.Position = (1.0f-a) * KeyFrames[i1].Bones[index].Position + a * KeyFrames[i2].Bones[index].Position;
            return interpolated;
        }

        public int GetBoneIndex(string name)
        {
            return Array.FindIndex<string>(BoneNames, delegate(string n) { return name == n; });
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDrawable Members

        public void Draw(IRenderer r, Node n)
        {
            if (Mesh == null)
                throw new Exception("cant draw animation without mesh.");
            Mesh.Draw(r, this);
        }

        public bool IsWorldSpace
        {
            get { return false; }
        }

        #endregion

        #region ITickable Members

        //int x = 0;
        public void Tick(float dtime)
        {
            //x++;
            //if(x%15==0)
            //CurrentFrame=(++CurrentFrame)%KeyFrames.Length;
            CurrentTime += dtime;
            if (CurrentTime >= KeyFrames[KeyFrames.Length - 1].Time)
                CurrentTime = 0;
        }

        #endregion
    }

    public class Model : IResource, IDrawable
    {
        public Model(Material m, SkeletalAnimation[] anims, SkeletalMesh mesh)
        {
            Material = m;
            Animations = anims;
            Mesh = mesh;
        }
        public Material Material;
        public SkeletalAnimation[] Animations;
        public SkeletalMesh Mesh;

        public int CurrentAnimation = 0;
        
        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDrawable Members

        public void Draw(IRenderer r, Node n)
        {
            Root.Instance.UserInterface.Renderer.UseShader(Material.Shader);
            Material.Apply(Material.Shader, r);
            if (CurrentAnimation >= 0 && Animations != null && Animations.Length > 0 && CurrentAnimation < Animations.Length)
            {
                Animations[CurrentAnimation].Draw(r, n);
            }
            else
            {
                Mesh.Draw(r, n);
            }
        }

        public bool IsWorldSpace
        {
            get { return false; }
        }

        #endregion
    }

    public class SkeletalMesh : IDrawable, IResource
    {
        public SkeletalMesh(VertexBuffer vb, IndexBuffer ib, Bone[] b, Shader s, VertexBuffer normals,BoundingBox bbox)
        {
            Vertices = vb;
            Indices = ib;
            Bones = b;
            Shader = s;
            Normals = normals;
            NormalShader = Root.Instance.ResourceManager.LoadShader("bones.normal.shader");
            BBox = bbox;
        }
        Shader NormalShader;
        //Material Material;

        public bool DrawNormals = false;

        public void Draw(IRenderer r, SkeletalAnimation a)
        {
            Matrix4 m = Matrix4.Identity;
            SetBones(r, Bones[0], m, a, Shader);
            r.Draw(Vertices, PrimitiveType.TRIANGLES, 0, Indices.buffer.Length, Indices);

            if (DrawNormals)
            {
                r.UseShader(NormalShader);
                SetBones(r, Bones[0], m, a, NormalShader);
                r.Draw(Normals, PrimitiveType.LINES, 0, Normals.Count, null);
            }
        }
        public BoundingBox BBox;

        #region IDrawable Members

        public void Draw(IRenderer r, Node n)
        {
            Draw(r, (SkeletalAnimation)null);
        }

        public void SetBones(IRenderer r, Bone b, Matrix4 m,SkeletalAnimation a, Shader s)
        {
            int index = s.GetUniformLocation("Bones["+b.Index+"]");
            if (index < 0)
            {
                throw new Exception("cant get location of bone "+b.Index+".");
            }
            //Matrix4 m1 = b.GetMatrix() * m;
            Matrix4 m1;
            if (a == null)
            {
                m1 = m * b.GetMatrix();
            }
            else
            {
                int i = a.GetBoneIndex(b.Name);
                if(i>=0)
                    m1 = m * a.GetBoneFrame(i).GetMatrix();
                else
                    m1 = m * b.GetMatrix();
            }
            //Matrix4 m2 = b.RestPoseInverse * m1;
            Matrix4 m2 = m1 * b.RestPoseInverse;

            //Matrix4 m1 = b.GetMatrix() * m;
            //Matrix4 m2 = b.RestPoseInverse * b.GetMatrix() * m;

            //m2[12] = m2[13] = m2[14] = 0;
            //m2.SetToIdentity();
            r.SetUniform(index, Matrix4Extensions.ToFloats(m2));
            foreach (Bone b2 in b.Children)
                SetBones(r, b2, m1, a, s);
        }

        public bool IsWorldSpace
        {
            get { return false; }
        }

        #endregion

        VertexBuffer Vertices;
        IndexBuffer Indices;
        Shader Shader;
        Bone[] Bones;
        VertexBuffer Normals;

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

    }

    public class Mesh : IDrawable,IResource,ISaveable
	{
		public Mesh()
		{
		}
        public bool IsWorldSpace
        {
            get { return false; }
        }
		public void Dispose()
		{
			foreach(SubMesh sm in SubMeshes)
				sm.Dispose();
			SubMeshes.Clear();
		}

        public void Draw(IRenderer r, Node n)
		{
		    //r.SetMode(RenderMode.Draw3D);
			foreach(SubMesh sm in SubMeshes)
			{
                Shader s;
                Material m = sm.Material;

                if (m.Shader == null && Root.Instance.ShaderManager != null)
                {
                    ShaderConfig cfg = Root.Instance.ShaderManager.GetShaderConfig(m);
                    if (n != null)
                        cfg.LightCount = Math.Min(n.CurrentNumberOfLights, 8);
                    else
                        cfg.LightCount = 8;
                    s = Root.Instance.ShaderManager.GetShader(cfg);
                }
                else
                {
                    s = m.Shader;
                }

                r.UseShader(s);			
				r.SetMaterial(m);
                if(s!=null)
                    m.Apply(s, r);
                if(n!=null)
	                n.SetRenderParameters(r,this,s);

                sm.Draw(r);

				if (sm.Normals != null)
				{
					r.SetMode(RenderMode.Draw3DWireFrame);
					r.SetLighting(false);
					r.Draw(sm.Normals, PrimitiveType.LINES, 0, sm.VertexCount * 2, null);
				}
			}
            //r.UseShader(null);
		}

		public void Save(SaveContext sc)
		{
            /*sc.Write(BoundingBox.X); sc.Write(BoundingBox.Y); sc.Write(BoundingBox.Z);
            w.Write(SubMeshes.Count);
            foreach(SubMesh sm in SubMeshes)sm.Save(s);
            Vertices.Save(s);*/
        }

        public Mesh(Factory f, Stream s, BinaryReader r)
        {
			//DeSerialize(f,s,r);
			string filename=r.ReadString();
		}

		public virtual void Serialize(Factory f,Stream s,BinaryWriter w)
		{
			string filename=Root.Instance.ResourceManager.Find(this).GetFullPath();
			w.Write(filename);
		}
		
		public virtual void DeSerialize(Factory f,Stream s,BinaryReader r)
		{
			r.ReadString();
		}
		/*public void Load(BinaryReader r)
		{
			//material laden
			int materialcount=r.ReadInt32();
			Material[] materials=new Material[materialcount];
			for(int i=0;i<materialcount;++i)
			{
				materials[i]=new Material();
				string texture=r.ReadString();
				if(texture.Length>0)
				{
					FileSystemNode n=Root.Instance.filesystem.Get(texture);
					Texture t=Root.Instance.resourcemanager.LoadTexture(n);
					materials[i].diffusemap=t;
				}
			}

			UniversalVertexFormat format=new UniversalVertexFormat();
			format.elements=new ArrayList();
			format.elements.Add(new VertexFormatElement(VertexElementName.Position,3,VertexElementType.Float));
			format.elements.Add(new VertexFormatElement(VertexElementName.Texture0,2,VertexElementType.Float));
			format.elements.Add(new VertexFormatElement(VertexElementName.Normal,3,VertexElementType.Float));
			
			int meshcount=r.ReadInt32();

			for(int i=0;i<meshcount;++i)
			{
				SubMesh sm=new SubMesh();

				string name=r.ReadString();
				int material=r.ReadInt32();
				int vertexcount=r.ReadInt32();
				
				byte[] data=new byte[vertexcount*format.getSize()];
				r.Read(data,0,vertexcount*format.getSize());
			
				if(Root.getInstance().userinterface!=null)
				{
					sm.Vertices=Root.getInstance().userinterface.getRenderer().CreateStaticVertexBuffer(data,data.Length);
					sm.Vertices.Format=format;
				}
				sm.VertexCount=vertexcount;
				sm.Material=materials[material];

				SubMeshes.Add(sm);
			}
		}
		/*public void Load(BinaryReader r)
		{
			//material laden
			int materialcount=r.ReadInt32();
			Material[] materials=new Material[materialcount];
			for(int i=0;i<materialcount;++i)
			{
				Material m=materials[i]=new Material();
				int id=r.ReadInt32();
				m.ambient.r=r.ReadSingle();
				m.ambient.g=r.ReadSingle();
				m.ambient.b=r.ReadSingle();
				m.diffuse.r=r.ReadSingle();
				m.diffuse.g=r.ReadSingle();
				m.diffuse.b=r.ReadSingle();
				m.specular.r=r.ReadSingle();
				m.specular.g=r.ReadSingle();
				m.specular.b=r.ReadSingle();
				m.Wire=r.ReadBoolean();
				m.twosided=r.ReadBoolean();
				if(r.ReadBoolean())
				{
					string file=r.ReadString();
					m.diffusemap=Root.getInstance().resourcemanager.LoadTexture(
						(FileSystemNode)Root.getInstance().filesystem["bla.png"]
						);
				}
			}

			r.ReadInt32();

			//format laden
			int components=r.ReadInt32();
			
			UniversalVertexFormat format=new UniversalVertexFormat();
			format.elements=new ArrayList();
			for(int i=0;i<components;++i)
			{
				VertexElementName name=(VertexElementName)r.ReadInt32();
				int count=r.ReadInt32();
				format.elements.Add(new VertexFormatElement(name,count,VertexElementType.Float));
			}

			//vertex buffer
			int vertexcount=r.ReadInt32();
			byte[] data=new byte[vertexcount*format.getSize()];
			r.Read(data,0,vertexcount*format.getSize());
			
			vertices=Root.getInstance().userinterface.getRenderer().CreateStaticVertexBuffer(data);
			vertices.Format=format;

			//submeshes
			submeshes.Clear();
			int submeshcount=r.ReadInt32();
			for(int i=0;i<submeshcount;++i)
			{
				SubMesh sm=new SubMesh();

				int facecount=r.ReadInt32();
				sm.indices=new IndexBuffer();
				sm.indices.buffer=new uint[facecount*3];
				for(int j=0;j<facecount*3;++j)
				{
					sm.indices.buffer[j]=r.ReadUInt32();
				}
				for(int j=0;j<facecount;++j)
				{
					uint t=sm.indices.buffer[j*3];
					sm.indices.buffer[j*3]=sm.indices.buffer[j*3+2];
					sm.indices.buffer[j*3+2]=t;
				}
				int material=r.ReadInt32();

				sm.material=materials[material];
				submeshes.Add(sm);
			}
		}*/

		public ArrayList SubMeshes=new ArrayList();
		public VertexBuffer Vertices;
		//public OdeTriMeshData CollisionMesh;
        public BoundingBox BBox;
    }

	public class FunctionGraph : IDrawable
	{
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public FunctionGraph(FunctionDelegate f, int size, float scale)
		{
			function=f;

			VertexFormat format=new VertexFormat(new VertexFormat.Element[]{
																			   new VertexFormat.Element(VertexFormat.ElementName.Position,3),
																			   new VertexFormat.Element(VertexFormat.ElementName.Color,3)
																		   });
			count=(size*2+1)*(size*2+1);
			byte[] data=new byte[format.Size*count];
			MemoryStream ms=new MemoryStream(data);
			BinaryWriter bw=new BinaryWriter(ms);
			
			for(int x=-size;x<=size;x+=1)
			{
				for(int y=-size;y<=size;y+=1)
				{
					float z=f((float)x*scale,(float)y*scale);
					bw.Write((float)x);bw.Write((float)z);bw.Write((float)y);
					bw.Write(1.0f);bw.Write(0.0f);bw.Write(0.0f);
				}
			}
			
			vb=Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data,data.Length);
			vb.Format=format;
		}

        public void Draw(IRenderer r, Node n)
		{
			r.SetLighting(false);
			r.Draw(vb,PrimitiveType.POINTS,0,count,null);
		}
	
		public delegate float FunctionDelegate(float x,float y);
		public FunctionDelegate function;
		public VertexBuffer vb;
		int count;
	}


	public class Box : IDrawable,ISerializable
	{
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public Box(Vector3 size)
		{
			Size=size;
			CreateVB(size);
		}

        public Box(DeSerializationContext context)
        {
			DeSerialize(context);
			CreateVB(Size);
		}

        public void Draw(IRenderer r, Node n)
		{
			r.SetLighting(false);
			//r.SetMode(RenderMode.Draw3DWireFrame);
			r.BindTexture(null,0);
			r.BindTexture(null,1);
			r.Draw(vb,PrimitiveType.POINTS,0,8,null);
		}

		private void CreateVB(Vector3 size)
		{
            if (Root.Instance.UserInterface == null)
				return;

			VertexP3C3[] data=new VertexP3C3[8];
			
			int i=0;
			for(int x=-1;x<=1;x+=2)
			{
				for(int y=-1;y<=1;y+=2)
				{
					for(int z=-1;z<=1;z+=2)
					{
						data[i].Color=new Color3f(1,1,1);
						data[i++].Position=new Vector3((float)x*size.X,(float)y*size.Y,(float)z*size.Z);
					}
				}
			}

			vb=Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data,data.Length*6*4);
			vb.Format=VertexFormat.VF_P3C3;
		}

		private VertexBuffer vb;
		private Vector3 Size;

        public void Serialize(SerializationContext context)
        {
            context.Write(Size.X); context.Write(Size.Y); context.Write(Size.Z);
        }

        public void DeSerialize(DeSerializationContext context)
        {
            Size.X = context.ReadSingle(); Size.Y = context.ReadSingle(); Size.Z = context.ReadSingle();
        }
	}

    public class BillBoard : IDrawable, ITickable
    {
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public BillBoard(float size)
        {
            if (Root.Instance.UserInterface != null)
            {
                vertices = new VertexP3T2[4];
                vertices[2].position = new Vector3(-size, -size, 0);
                vertices[2].texture0 = new Vector2(0, 1);
                vertices[1].position = new Vector3(size, -size, 0);
                vertices[1].texture0 = new Vector2(1, 1);
                vertices[3].position = new Vector3(-size, size, 0);
                vertices[3].texture0 = new Vector2(0, 0);
                vertices[0].position = new Vector3(size, size, 0);
                vertices[0].texture0 = new Vector2(1, 0);

                Buffer = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(vertices, 5 * 4 * 4);
                Buffer.Format = VertexFormat.VF_P3T2;

                Textures = new Texture[60];
                for (int i = 0; i < Textures.Length; ++i)
                {
                    string s = string.Format("explosion{0,3}.png", i);
                    s = s.Replace(' ', '0');
                    Textures[i] = Root.Instance.ResourceManager.LoadTexture(s);
                }

                Mat = new Material();
                Mat.NoLighting = true;
                Mat.twosided = true;
                Mat.Additive = true;
            }
        }

        public void Tick(float dtime)
        {
            Time += dtime;
            CurrentFrame = (int)(Time / 0.040f);
            if (Loop)
                CurrentFrame = CurrentFrame % 60;
            else if (CurrentFrame >= 60)
                CurrentFrame = -1;
        }

        public void Draw(IRenderer r, Node n)
        {
            if (CurrentFrame < 0)
                return;

            float[] modelview=new float[16];
            float[] projection=new float[16];

            Mat.diffusemap = Textures[CurrentFrame];
            r.SetMaterial(Mat);
            r.GetMatrix(modelview, projection);
            r.PushMatrix();

            Matrix4 m = Matrix4.Identity;
            Vector3 pos = Matrix4Extensions.ExtractTranslation(m);
            Matrix4Extensions.Translate(m,pos);
            r.LoadMatrix(m);

            r.Draw(Buffer, PrimitiveType.QUADS, 0, 4, null);
            r.PopMatrix();
        }

        VertexP3T2[] vertices;
        VertexBuffer Buffer;
        public Texture[] Textures;
        public Material Mat;
        float Time=0;
        public int CurrentFrame = 0;
        public bool Loop = false;
    }

    public class SelectionMarker : Node
    {
        public SelectionMarker()
        {
            Draw = new ArrayList(new IDrawable[] { new SelectionDrawer() });
        }
    }

    public class SelectionDrawer : IDrawable
    {
        public SelectionDrawer()
        {
            if (Root.Instance.UserInterface == null)
                return;

            float d=100000;
            vertices[0].Position = new Vector3(-d, 0, 0);
            vertices[1].Position = new Vector3(d, 0, 0);
            vertices[2].Position = new Vector3(0, -d, 0);
            vertices[3].Position = new Vector3(0, d, 0);
            vertices[4].Position = new Vector3(0, 0, -d);
            vertices[5].Position = new Vector3(0, 0, d);
            vertices[0].Color = new Color4f(1, 0, 0, 1);
            vertices[1].Color = new Color4f(1, 0, 0, 1);
            vertices[2].Color = new Color4f(0, 1, 0, 1);
            vertices[3].Color = new Color4f(0, 1, 0, 1);
            vertices[4].Color = new Color4f(0, 0, 1, 1);
            vertices[5].Color = new Color4f(0, 0, 1, 1);

            shader = Root.Instance.ResourceManager.LoadShader("simple3d.shader");

            vb = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(vertices,vertices.Length * VertexFormat.VF_P3C4.Size);
            vb.Format = VertexFormat.VF_P3C4;
        }

        public void Draw(IRenderer r, Node n)
        {
            r.UseShader(shader);
            r.Draw(vb, PrimitiveType.LINES, 0, 6, null);
        }
        public bool IsWorldSpace
        {
            get { return false; }
        }

        VertexP3C4[] vertices=new VertexP3C4[6];
        VertexBuffer vb;
        Shader shader;
    }

    public class Marker : IDrawable
	{
        public bool IsWorldSpace
        {
            get { return false; }
        }
        static Marker()
		{
		}

		public Marker()
		{
            if (Root.Instance.UserInterface == null)
                return;

			vb=CreateVB(500);
            shader = Root.Instance.ResourceManager.LoadShader("simple3d.color.shader");
		}

        Shader shader;

        public void Draw(IRenderer r, Node n)
		{
			/*r.SetLighting(false);
			//r.SetMode(RenderMode.Draw3DWireFrame);
			r.BindTexture(null,0);
			r.BindTexture(null,1);
            r.BindTexture(null, 2);
            r.BindTexture(null, 3);
            r.BindTexture(null, 4);
            r.BindTexture(null, 5);*/
            r.UseShader(shader);
            r.Draw(vb, PrimitiveType.LINES, 0, 6, null);
		}
		
		
		protected static VertexBuffer CreateVB(float size)
		{
			VertexFormat format=new VertexFormat(new VertexFormat.Element[]{
																			   new VertexFormat.Element(VertexFormat.ElementName.Position,3),
																			   new VertexFormat.Element(VertexFormat.ElementName.Color,3)
																		   });
			
			byte[] data=new byte[format.Size*6];
			MemoryStream ms=new MemoryStream(data);
			BinaryWriter bw=new BinaryWriter(ms);
			
			bw.Write(0.0f);bw.Write(0.0f);bw.Write(0.0f);
			bw.Write(1.0f);bw.Write(0.0f);bw.Write(0.0f);
			bw.Write(size);bw.Write(0.0f);bw.Write(0.0f);
			bw.Write(1.0f);bw.Write(0.0f);bw.Write(0.0f);

			bw.Write(0.0f);bw.Write(0.0f);bw.Write(0.0f);
			bw.Write(0.0f);bw.Write(1.0f);bw.Write(0.0f);
			bw.Write(0.0f);bw.Write(size);bw.Write(0.0f);
			bw.Write(0.0f);bw.Write(1.0f);bw.Write(0.0f);

			bw.Write(0.0f);bw.Write(0.0f);bw.Write(0.0f);
			bw.Write(0.0f);bw.Write(0.0f);bw.Write(1.0f);
			bw.Write(0.0f);bw.Write(0.0f);bw.Write(size);
			bw.Write(0.0f);bw.Write(0.0f);bw.Write(1.0f);

			VertexBuffer vb=Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data,data.Length);
			vb.Format=format;
			return vb;
		}

		public int i;
		public int[] array={1,2,3,4};
		public VertexBuffer vb;
	}

    public class ImageWindow : Window
    {
        public ImageWindow(Texture t)
            : base(new Color4f(1,1,1,1))
        {
            Shader = Root.Instance.ResourceManager.LoadShader("window.simple.textured.shader");
            texture = t;
        }


    }


    public class ImageButton : Window
    {
        public ImageButton(Texture pic, string text, Button.ClickDelegate click)
            : base(new Layout(2, 1))
        {
            Picture = new Button(click);
            Picture.texture = pic;
            Picture.Shader = Root.Instance.ResourceManager.LoadShader("window.simple.textured.shader");
            Picture.Color = Picture.NormalColor = new Color4f(1, 1, 1, 1);
            Picture.FocusColor = Picture.Color;// new Color4f(0.8f, 1, 0.8f, 0.9f);
            Add(Picture, 0, 0);

            Text = new Button(click, text);
            Transparent = true;
            Add(Text, 1, 0);

            Layout.Update(Size);
        }
        public ImageButton(Texture pic, string text)
            : this(pic,text,null)
        {
        }

        public override void OnResize()
        {
            Layout.Widths[0] = Size.Y;
            Layout.Widths[1] = Size.X - Layout.Widths[0];
            base.OnResize();
        }
        public Button Picture;
        public Button Text;
    }

    /*
    public class ImageButton : Button
    {
        public ImageButton(Texture t)
            : base("")
        {
            Shader = Root.Instance.ResourceManager.LoadShader("window.simple.textured.shader");
            texture = t;
            Color = NormalColor = new Color4f(1, 1, 1, 1);
            FocusColor = new Color4f(0.8f, 1, 0.8f, 0.9f);
        }
        public ImageButton(Texture t,string caption)
            : base(caption)
        {
            Shader = Root.Instance.ResourceManager.LoadShader("window.simple.textured.shader");
            texture = t;
            Color = NormalColor = new Color4f(1, 1, 1, 1);
            FocusColor = new Color4f(1.5f, 1.5f, 1.5f, 0.9f);
        }
    }*/

    public class ImageListBox : ListBox
    {
        public ImageListBox(ListBoxItem[] items)
            : base(items)
        {
            ItemHeight = 128;
            ItemsPerRow = 1;
        }
        public override void OnResize()
        {
            //ItemHeight = Size.X/2;
            base.OnResize();
        }
        public ImageListBox()
        {
        }

        protected override Window CreateWindowForItem(ListBoxItem item)
        {
            //string file=o.ToString();

            //Texture t=Root.Instance.ResourceManager.LoadTexture(file);

            //string[] split = file.Split('/');
            return new ImageButton(item.Image,item.ToString());
        }
    }

    public struct ListBoxItem
    {
        public ListBoxItem(object o,Texture t,string text)
        {
            Object = o;
            Image = t;
            Text = text;
        }

        public override string ToString()
        {
            if (Text != null)
                return Text;
            else if (Object != null)
                return Object.ToString();
            else
                return base.ToString();
        }
        public object Object;
        public Texture Image;
        public string Text;
    }

    public class ListBox : Window
    {


        public ListBox(ListBoxItem[] items)
        {
            Transparent = true;
            SetContents(items);
        }

        public ListBox()
        {
            Transparent = true;
        }

        public void SetContents(ListBoxItem[] items)
        {
            Items = items;
            ItemButtons = new Window[items.Length];
            Children.Clear();
            for (int i = 0; i < items.Length; ++i)
            {
                Add(ItemButtons[i] = CreateWindowForItem(items[i]));
            }
            OnResize();
        }

        protected virtual Window CreateWindowForItem(ListBoxItem item)
        {
            return new Button(item.ToString());
        }

        public override void OnResize()
        {
            if(Items!=null)
            for (int i = 0; i < Items.Length; ++i)
            {
                ItemButtons[i].Size = new Vector2(size.X/(float)ItemsPerRow, ItemHeight);
                ItemButtons[i].Position = new Vector2((i % ItemsPerRow) * size.X / (float)ItemsPerRow, ItemHeight * (i / ItemsPerRow));
            }
        }

        public void ScrollToItem(int index)
        {
            index=Math.Max(Math.Min(index,Items.Length-1),0);

            ScrollPosition.Y = -ItemButtons[index].Position.Y;
            ScrollIndex = index;
        }

        public void ScrollItem(int delta)
        {
            ScrollToItem(ScrollIndex + delta);
        }

        public override void OnChildClick(Window w, int button)
        {
            base.OnChildClick(w, button);

            for (int i = 0; i < ItemButtons.Length; ++i)
            {
                if (w == ItemButtons[i])
                {
                    Selected = Items[i];
                    SelectedIndex = i;

                    if (SelectionChangedEvent != null)
                    {
                        SelectionChangedEvent(this);
                    }
                    break;
                }
            }
        }

        public override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);

            if (!AutoScroll)
                return;

            Vector2 min, max;
            if (GetChildBounding(out min, out max))
            {
                float v = y / Size.Y;
                ScrollPosition.Y = CurrentScrollPosition.Y = -v * (max.Y - min.Y) + Size.Y / 2.0f;
                if (-ScrollPosition.Y + Size.Y > max.Y)
                    ScrollPosition.Y = CurrentScrollPosition.Y = -max.Y + Size.Y;
                else if (ScrollPosition.Y > min.Y)
                    ScrollPosition.Y = CurrentScrollPosition.Y = min.Y;
                
                if (ScrollPosition.Y > 0)
                    ScrollPosition.Y = CurrentScrollPosition.Y = 0;
                else if (ItemHeight * Items.Length > Size.Y && ScrollPosition.Y < Size.Y - ItemHeight * Items.Length)
                    ScrollPosition.Y = CurrentScrollPosition.Y = Size.Y - ItemHeight * Items.Length;

            }
        }
        public override void Tick(float dtime)
        {
            if (Items != null)
            {
                //Cheetah.Console.WriteLine(Items.Length.ToString());
                if (ScrollPosition.Y > 0)
                    ScrollPosition.Y = 0;
                else if (ItemHeight * Items.Length>Size.Y&&ScrollPosition.Y < Size.Y - ItemHeight * Items.Length)
                    ScrollPosition.Y = Size.Y - ItemHeight * Items.Length;
            }
            base.Tick(dtime);
        }
        public ListBoxItem[] Items;
        public Window[] ItemButtons;
        public float ItemHeight = 32;
        public ListBoxItem Selected;
        public int SelectedIndex = -1;
        public int ScrollIndex = -1;
        public int ItemsPerRow = 1;
        public delegate void SelectionChangedDelegate(ListBox lb);
        public event SelectionChangedDelegate SelectionChangedEvent;
        public bool AutoScroll = true;
    }

    public class ScrollButton : Button
    {
        public ScrollButton(string caption,Window[] w, Vector2 scroll)
            :base(caption)
        {
            Targets = w;
            ScrollDelta = scroll;
        }

        public override void OnClick(int button, float x, float y)
        {
            base.OnClick(button, x, y);

            foreach (Window w in Targets)
            {
                if(w is ListBox)
                    ((ListBox)w).ScrollItem((int)ScrollDelta.Y);
                else
                    w.ScrollPosition -= ScrollDelta;
            }
        }
        Window[] Targets;
        Vector2 ScrollDelta;
    }


    public class ScrollBar : Window
    {
        public ScrollBar(Window w)
        {
            BarColor = new Color4f(1, 1, 1, 1);
            Orientation = OrientationType.Vertical;
            Value1 = 0.25f;
            Value2 = 0.75f;
            Shader = Root.Instance.ResourceManager.LoadShader("scrollbar.shader");
            ShaderParams = new ShaderParams();
            Target = w;
        }

        public ScrollBar(Color4f color, Color4f barcolor, OrientationType orientation,Window w)
            : base(color)
        {
            BarColor = barcolor;
            Orientation = orientation;
            Value1 = 0.25f;
            Value2 = 0.75f;
            Shader = Root.Instance.ResourceManager.LoadShader("scrollbar.shader");
            ShaderParams = new ShaderParams();
            Target = w;
        }

        public override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);

            //Console.WriteLine(y.ToString());
            float v = y / Size.Y;
            Vector2 min,max;
            if (Target.GetChildBounding(out min, out max))
            {
                Target.ScrollPosition.Y = Target.CurrentScrollPosition.Y = - v * (max.Y - min.Y)+Target.Size.Y/2.0f;
                if (-Target.ScrollPosition.Y + Target.Size.Y > max.Y)
                    Target.ScrollPosition.Y = Target.CurrentScrollPosition.Y = -max.Y + Target.Size.Y;
                else if (Target.ScrollPosition.Y > min.Y)
                    Target.ScrollPosition.Y = Target.CurrentScrollPosition.Y = min.Y;

            }
        }
        public override void Draw(IRenderer r, RectangleF rect)
        {
            ShaderParams[Shader.GetUniformLocation("Value1")] = new float[] { Value1 };
            ShaderParams[Shader.GetUniformLocation("Value2")] = new float[] { Value2 };

            float[] c = (float[])BarColor;
            if (Fade >= 0)
                c[3] *= Fade;
            ShaderParams[Shader.GetUniformLocation("BarColor")] = c;

            base.Draw(r, rect);
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

           
            Vector2 min, max;
            if (Target.GetChildBounding(out min, out max))
            {
                Value1 = (-Target.CurrentScrollPosition.Y - min.Y) / (max.Y - min.Y);
                Value2 = (-Target.CurrentScrollPosition.Y + Target.Size.Y - min.Y) / (max.Y - min.Y);
            }
            else
            {
                Value1 = 0;
                Value2 = 1;
            }
        }

        public float Value1;
        public float Value2;
        public OrientationType Orientation;
        public Color4f BarColor;
        public Window Target;
    }

    public class TextBox : Window
	{
		public TextBox(float x,float y,float w,float h) : base(x,y,w,h)
		{
            Append("");
        }
		public TextBox() : base()
		{
            Append("");
		}
		public TextBox(bool multiline) : base()
		{
			MultiLine=multiline;
		}
		public TextBox(string text) : base()
		{
			Append(text);
		}
		
		public TextBox(string text,bool multiline) : base()
		{
			Append(text);
			MultiLine=multiline;
		}

		public enum AlignmentY
		{
			Top,Bottom,Center
		}
		public enum AlignmentX
		{
			Left,Right,Center
		}

        public override void Draw(IRenderer r, RectangleF rect)
		{
			//Matrix4 pos=Matrix4Extensions.FromTranslation(position.X,position.Y,0);//*Matrix4Extensions.FromScale(size.X,size.Y,0);
			//r.BindTexture(texture);
			//r.PushMatrix();
			//r.MultMatrix(pos);
			//r.Draw(vertices,PrimitiveType.QUADS,0,4,null);
			
			base.Draw(r,rect);

			Font f=Root.Instance.Gui.DefaultFont;

			Vector2 pos;
			if(MultiLine)
			{
                if (CenterText)
                    pos = new Vector2(AbsoluteCenterPosition.X,AbsolutePosition.Y);
                else
                    pos = AbsolutePosition;
            }
			else
			{
				//pos=AbsoluteCenterPosition-new Vector2(f.size*GetLine(0).Length/2*0.5f,f.size/2);
                pos = AbsoluteCenterPosition - new Vector2(f.Width * GetLine(0).Length / 2, f.size / 2);
            }
            //Vector2 pos = AbsoluteCenterPosition - new Vector2(f.size / 2 * Caption.Length, f.size / 2);
            Color4f c = TextColor;
            if (Fade >= 0)
                c.a *= Fade;

            rect.Offset(-position.X, -position.Y);
            rect.Offset(AbsolutePosition.X, AbsolutePosition.Y);

            for (int i = FirstLine; i <= LastLine; ++i)
            {
                string line=(string)lines[i];
                f.Draw(r, line, pos.X-(CenterText?(0.5f*f.size*(float)line.Length):0), pos.Y + (i - FirstLine) * f.size,c,rect);
            }

            if(!ReadOnly&&CursorVisible)
			{
                int l = Math.Max(LastLine, 0);
				//f.Draw(r,"_",pos.X+GetLine(LastLine).Length*f.size*0.5f,pos.Y+(l-FirstLine)*f.size);
                f.Draw(r, "_", pos.X + GetLine(LastLine).Length * f.Width, pos.Y + (l - FirstLine) * f.size, c, rect);
            }

			//r.PopMatrix();
		}

		public void AppendLine(string line)
		{
			lines.Add(line);
			if(AutoScroll)
			{
				FirstLine=Math.Max(0,lines.Count-MaxLines);
			}
		}

		public void Append(string text)
		{
			if(lines.Count==0)
			{
				lines.Add(text);
			}
			else
			{
				lines[lines.Count-1]=((string)lines[lines.Count-1])+text;
			}
		}

		public override void Tick(float dtime)
		{
			base.Tick(dtime);
			CursorTime+=dtime;
			while(CursorTime>CursorBlinkInterval)
			{
				CursorTime-=CursorBlinkInterval;
				CursorVisible=!CursorVisible;
			}
		}

		public string GetLine(int index)
		{
			string s=null;
			if(index>=0)
			{
				if(index<lines.Count)
					s=(string)lines[index];
			}
			else
			{
				if(lines.Count+index<lines.Count&&lines.Count+index>=0)
					s=(string)lines[lines.Count+index];
			}
			if(s==null)
				return "";
			return s;
		}

        public void LoadText(Stream s)
        {
            StreamReader r = new StreamReader(s);

            Clear();
            MultiLine = true;

            string line;
            while ((line = r.ReadLine()) != null)
            {
                AppendLine(line);
            }
        }

        public void SetLine(int index,string line)
		{
			if(index>=0)
				lines[index]=line;
			else
				lines[lines.Count+index]=line;
		}

		public void RemoveChar(int line,int index)
		{
			string s=GetLine(line);
			if(s.Length>0)
			{
				if(index>=0)
					s=s.Remove(index,1);
				else
					s=s.Remove(s.Length+index,1);
				SetLine(line,s);
			}
		}

		public void RemoveLine(int index)
		{
			if(index>=0)
				lines[index]="";
			else
				lines[lines.Count+index]="";
		}

		public int MaxLines
		{
			get
			{
				if(MultiLine)
					return (int)(Size.Y/Root.Instance.Gui.DefaultFont.size);
				else
					return 1;
			}
		}

		public void Scroll(int deltalines)
		{
			FirstLine+=deltalines;
		}

		public string[] Lines
		{
			get
			{
				return (string[])lines.ToArray(typeof(string));
			}
			set
			{
				lines=new ArrayList(value);
			}
		}

		public int LineCount
		{
			get
			{
				return lines.Count;
			}
		}

		public void Clear()
		{
			lines.Clear();
		}

		public int LastLine
		{
			get
			{
				return Math.Min(FirstLine+MaxLines,LineCount)-1;
			}
		}

		public int FirstLine
		{
			get
			{
				return _FirstLine;
			}
			set
			{
				_FirstLine=Math.Max(Math.Min(value,LineCount-MaxLines),0);
			}
		}

        public override void OnGetFocus()
        {
            Color = FocusColor;
        }
        public override void OnLoseFocus()
        {
            Color = NormalColor;
        }

        public override void OnKeyDown(global::OpenTK.Input.Key key)
		{
            base.OnKeyDown(key);

            if (ReadOnly)
                return;

			/*if(key.IsPrintable)
			{
				Append(key.GetString());
			}*/

            else if (key == global::OpenTK.Input.Key.BackSpace)
			{
				RemoveChar(-1,-1);
			}
            else if (key == global::OpenTK.Input.Key.PageUp)
			{
				Scroll(-5);
			}
            else if (key == global::OpenTK.Input.Key.PageDown)
			{
				Scroll(5);
			}
		}
        
        public Color4f NormalColor=new Color4f(0.2f,0.2f,0.2f,0.7f);
        public Color4f FocusColor = new Color4f(0.4f, 0.4f, 0.4f, 0.7f);
        public Color4f TextColor = new Color4f(1, 1, 1, 1);

        protected ArrayList lines=new ArrayList();
		public int _FirstLine;
		public bool AutoScroll=true;
		public bool MultiLine=true;
		public bool ReadOnly=false;
		protected float CursorTime;
		public float CursorBlinkInterval=0.25f;
		protected bool CursorVisible;
        public bool CenterText;
    }

    public class StateButton : Button
    {
        public StateButton(object[] states):base(states[0].ToString())
        {
            _States = states;
        }
        public StateButton(object[] states,StateChangeDelegate statechange):base(states[0].ToString())
        {
            _States = states;
            StateChangeEvent += statechange;
        }

        public override void OnClick(int button, float x, float y)
        {
            int old=_CurrentStateNumber;
            _CurrentStateNumber = (_CurrentStateNumber + 1) % _States.Length;
            OnStateChange(old, _CurrentStateNumber, _States[old], _States[_CurrentStateNumber]);
            base.OnClick(button, x, y);
        }

        protected void OnStateChange(int nfrom, int nto, object from, object to)
        {
            Caption = _States[_CurrentStateNumber].ToString();
            if (StateChangeEvent != null)
                StateChangeEvent(nfrom, nto, from, to);
        }

        public object CurrentState
        {
            get
            {
                return _States[_CurrentStateNumber];
            }
        }

        public int CurrentStateNumber
        {
            get
            {
                return _CurrentStateNumber;
            }
            set
            {
                _CurrentStateNumber = value;
            }
        }

        protected object[] _States;
        protected int _CurrentStateNumber;
        public delegate void StateChangeDelegate(int nfrom, int nto, object from, object to);
        public event StateChangeDelegate StateChangeEvent;
    }

    public class ColladaLoader : IResourceLoader
    {
        #region IResourceLoader Members

        public IResource Load(FileSystemNode n)
        {
            COLLADA.Document doc = new COLLADA.Document(n.getStream());

            COLLADA.Document.Primitive primitive = doc.geometries[0].mesh.primitives[0];

            PrimitiveType primitiveType;

            if (primitive is COLLADA.Document.Triangle)
                primitiveType = PrimitiveType.TRIANGLES;
            else if (primitive is COLLADA.Document.Line)
                primitiveType = PrimitiveType.LINES;
            else
                throw new Exception("collada: Unexpected primitiveType=" + primitive.GetType().ToString());


            //primitiveCount = primitive.count; // number of primitives to draw
            //streamOffset = 0; // selection which input stream to use
            //startIndex = 0; // first index element to read
            //baseVertex = 0; // vertex buffer offset to add to each element of the index buffer

            List<VertexFormat.Element> vertexElements = new List<VertexFormat.Element>();
            short deltaOffset = 0;
            short offset = 0;

            foreach (COLLADA.Document.Input input in COLLADA.Util.getAllInputs(doc, primitive))
            {
                VertexFormat.Element vertexElementFormat;
                // assuming floats !
                switch (((COLLADA.Document.Source)input.source).accessor.ParameterCount)
                {
                    case 2:
                        vertexElementFormat = new VertexFormat.Element(VertexFormat.ElementName.None,2,VertexFormat.ElementType.Float);
                        deltaOffset = 2 * 4;
                        break;
                    case 3:
                        vertexElementFormat = new VertexFormat.Element(VertexFormat.ElementName.None, 3, VertexFormat.ElementType.Float);
                        deltaOffset = 3 * 4;
                        break;
                    default:
                        throw new Exception("Unexpected vertexElementFormat");

                }

                //VertexElementUsage vertexElementUsage;
                //byte usageIndex;

                switch (input.semantic)
                {
                    case "POSITION":
                        vertexElementFormat.Name = VertexFormat.ElementName.Position;
                        //vertexElementUsage = VertexElementUsage.Position;
                        //usageIndex = 0;
                        // number of vertices in mesh part
                        //numVertices = ((COLLADA.Document.Source)input.source).accessor.count;
                        break;
                    case "NORMAL":
                        vertexElementFormat.Name = VertexFormat.ElementName.Normal;
                        //vertexElementUsage = VertexElementUsage.Normal;
                        //usageIndex = 0;
                        break;
                    case "COLOR":
                        vertexElementFormat.Name = VertexFormat.ElementName.Color;
                        //vertexElementUsage = VertexElementUsage.Color;
                        //usageIndex = 0;
                        break;
                    case "TEXCOORD":
                        vertexElementFormat.Name = VertexFormat.ElementName.Texture0;
                        //vertexElementUsage = VertexElementUsage.TextureCoordinate;
                        //usageIndex = 0; // TODO handle several texture (need to replace BasicMaterial first)
                        break;
                    default:
                        throw new Exception("Unexeptected vertexElementUsage=" + input.semantic);
                }
                vertexElements.Add(vertexElementFormat);
                //vertexElements.Add(new VertexElement(streamOffset /* stream */,
                //                                        offset  /* offset */,
                //                                        vertexElementFormat,
                //                                        VertexElementMethod.Default,
                //                                        vertexElementUsage,
                //                                        usageIndex));
                offset += deltaOffset;
            }
            VertexFormat format = new VertexFormat(vertexElements.ToArray());
            /*
            foreach (COLLADA.Document.Input input in COLLADA.Util.getAllInputs(doc, primitive))
            {
                if (inputs.ContainsKey(input.semantic))
                    throw new Exception("Cannot handle multiple " + input.semantic + " case in COLLADAModel");
                else
                {
                    inputs[input.semantic] = input;
                    if (input.semantic == "POSITION")
                        vertexArray = ((Document.Source)input.source).array as Document.Array<float>;
                    else
                        if (((Document.Source)input.source).array != null)
                            throw new Exception("Model was *not* transformed in vertexArray by Reindexor or equivalent conditionner");
                }
            }
            */

            //vertexElementsArray = vertexElements.ToArray();
            short vertexStride = offset;

            // TODO: make index a short if possible
            int[] indexArray = primitive.p;

            // reverse triangle order for directX
            /*if (primitive is COLLADA.Document.Triangle)
                for (int i = 0; i < primitive.p.Length; i += 3)
                {
                    indexArray[i] = primitive.p[i + 2];
                    indexArray[i + 1] = primitive.p[i + 1];
                    indexArray[i + 2] = primitive.p[i];
                }
            */
            throw new Exception("NYI");
        }

        public Type LoadType
        {
            get { return typeof(Mesh); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Name.EndsWith(".dae");
        }

        #endregion
    }

    public class CheckBox : Button
    {
        public CheckBox()
            :base("false")
        {
        }
        public CheckBox(ClickDelegate click)
            : base(click,"false")
        {
        }

        public override void OnClick(int button, float x, float y)
        {
            Checked = !Checked;

            base.OnClick(button, x, y);

        }

        bool check = false;

        public bool Checked
        {
            get
            {
                return check;
            }
            set
            {
                check = value;
                Caption = check ? "true" : "false";
            }
        }
    }

    public class Button : Window
	{
		public Button(float x,float y,float w,float h,ClickDelegate click): base(x,y,w,h)
		{
            if(click!=null)
			    ClickEvent+=click;
			Color=NormalColor=new Color4f(0.2f,0.2f,0.2f,0.7f);
			FocusColor=new Color4f(0.4f,0.4f,0.4f,0.7f);
		}
		public Button(ClickDelegate click): base()
		{
            if (click != null)
                ClickEvent += click;
            Color = NormalColor = new Color4f(0.2f, 0.2f, 0.2f, 0.7f);
			FocusColor=new Color4f(0.4f,0.4f,0.4f,0.7f);
		}
		public Button(string caption): base()
		{
			Caption=caption;
			Color=NormalColor=new Color4f(0.2f,0.2f,0.2f,0.7f);
			FocusColor=new Color4f(0.4f,0.4f,0.4f,0.7f);
		}

		public Button(float x,float y,float w,float h,ClickDelegate click,string text): this(x,y,w,h,click)
		{
			Caption=text;
		}

		public Button(ClickDelegate click,string text): this(click)
		{
			Caption=text;
		}

		public override void OnClick(int button,float x,float y)
		{
            if(ClickEvent!=null)
			    ClickEvent(this,button,x,y);
            if (Parent != null)
                Parent.OnChildClick(this, button);
        }
		public override void OnGetFocus()
		{
            base.OnGetFocus();
			Color=FocusColor;
		}
		public override void OnLoseFocus()
		{
            base.OnLoseFocus();
			Color=NormalColor;
		}

        public override void Draw(IRenderer r, RectangleF rect)
		{
			base.Draw(r,rect);

            if (Caption != null && Caption.Length > 0)
            {
                Font f = Root.Instance.Gui.DefaultFont;

                //MeshFont f = (MeshFont)Root.Instance.ResourceManager.Load("models/font-arial-black", typeof(MeshFont));

                Vector2 pos = AbsoluteCenterPosition - new Vector2(f.Width * Caption.Length * 0.5f, f.size / 2);
                //Vector2 pos = CenterPosition - new Vector2(f.Width * Caption.Length * 0.5f, f.size / 2);

                Color4f c = TextColor;
                if (Fade >= 0)
                    c.a *= Fade;

                rect.Offset(-position.X, -position.Y);
                rect.Offset(AbsolutePosition.X, AbsolutePosition.Y);
                f.Draw(r, Caption, pos.X, pos.Y, c, rect);
            }
			//f.size=old;
			//r.PopMatrix();
		}

		public delegate void ClickDelegate(Button source,int button,float x,float y);
		public event ClickDelegate ClickEvent;
		public string Caption;
        public Color4f TextColor=new Color4f(1,1,1,1);
		public Color4f FocusColor;
		public Color4f NormalColor;
	}

    public enum OrientationType
    {
        Horizontal, Vertical
    }

    public class Layout
	{
		public enum CoordinateType
		{
			Fraction,Absolute,DontCare
		}

		public class Cell
		{
			public Cell(OrientationType o)
			{
				Orientation=o;
			}

			public OrientationType Orientation;
			public ArrayList Windows=new ArrayList();
			public Point Span=new Point(1,1);
		}

		public void Update(Vector2 totalsize)
		{
			//float tw=TotalWidth;
			//float th=TotalHeight;

			for(int x=0;x<Width;++x)
			{
				for(int y=0;y<Height;++y)
				{
					Cell c=GetCell(x,y);
					Vector2 cp=GetCellPosition(x,y);
					Vector2 cs=GetCellSize(x,y);
					for(int i=0;i<c.Windows.Count;++i)
					{
						Window w=(Window)c.Windows[i];
						w.Position=new Vector2(cp.X*totalsize.X+Spacing,cp.Y*totalsize.Y+Spacing);
						w.Size=new Vector2(cs.X*totalsize.X-2*Spacing,cs.Y*totalsize.Y-2*Spacing);
                        //if (w.Layout != null)
                        //    w.Layout.Update(w.Size);
					}
				}
			}

		}

		public Vector2 GetCellPosition(int x,int y)
		{
			float px=0;
			float py=0;
            Cell c = GetCell(x, y);


            if (c.Span.X > 0)
                for (int i = 0; i < x; ++i)
				    px+=Widths[i];
            else
                for (int i = 0; i < x + c.Span.X + 1; ++i)
                    px += Widths[i];

            if (c.Span.Y > 0)
                for (int i = 0; i < y; ++i)
				    py+=Heights[i];
            else
                for (int i = 0; i < y + c.Span.Y + 1; ++i)
                    py += Widths[i];

			return new Vector2(px/TotalWidth,py/TotalHeight);
		}

		public Vector2 GetCellSize(int x,int y)
		{
			Cell c=GetCell(x,y);
			float rsx=0;
			float rsy=0;

            if(c.Span.X>0)
			    for(int i=0;i<c.Span.X;++i)
				    rsx+=Widths[x+i]/TotalWidth;
            else
                for (int i = 0; i > c.Span.X; --i)
                    rsx += Widths[x + i] / TotalWidth;

            if (c.Span.Y > 0)
                for (int i = 0; i < c.Span.Y; ++i)
				    rsy+=Heights[y+i]/TotalHeight;
            else
                for (int i = 0; i > c.Span.Y; --i)
                    rsy += Heights[y + i] / TotalHeight;

			return new Vector2(rsx,rsy);
		}

		public Layout(int x,int y)
		{
			Cells=new Cell[x][];
			for(int i=0;i<x;++i)
			{
				Cells[i]=new Cell[y];
				for(int j=0;j<y;++j)
				{
					Cells[i][j]=new Cell(OrientationType.Vertical);
				}
			}
			Widths=new float[x];
			for(int i=0;i<x;++i)Widths[i]=1;
			Heights=new float[y];
			for(int i=0;i<y;++i)Heights[i]=1;
		}

		public Cell GetCell(int x,int y)
		{
			return Cells[x][y];
		}

		public float TotalWidth
		{
			get
			{
				float w=0;
				for(int i=0;i<Width;++i)w+=Widths[i];
				return w;
			}
		}
		
		public float TotalHeight
		{
			get
			{
				float w=0;
				for(int i=0;i<Height;++i)w+=Heights[i];
				return w;
			}
		}

		public int Width
		{
			get
			{
				return Cells.Length;
			}
		}

		public int Height
		{
			get
			{
				return Cells[0].Length;
			}
		}

		protected Cell[][] Cells;
		public float[] Widths;
		public float[] Heights;
        public float Spacing=1;
    }

    public class ProgressBar : Window
    {
        public ProgressBar()
        {
            BarColor = new Color4f(1, 1, 1, 1);
            Orientation = OrientationType.Vertical;
            Value = 0.5f;
            Shader = Root.Instance.ResourceManager.LoadShader("progressbar.shader");
            ShaderParams = new ShaderParams();
        }
        
        public ProgressBar(Color4f color,Color4f barcolor,OrientationType orientation):base(color)
        {
            BarColor = barcolor;
            Orientation = orientation;
            Value = 0.5f;
            Shader = Root.Instance.ResourceManager.LoadShader("progressbar.shader");
            ShaderParams = new ShaderParams();
        }

        public override void Draw(IRenderer r, RectangleF rect)
        {
            ShaderParams[Shader.GetUniformLocation("Value")] = new float[] { Value };
            ShaderParams[Shader.GetUniformLocation("BarColor")] = (float[])BarColor;

            base.Draw(r,rect);
            /*
            Color4f oldcolor = Color;
            Vector2 oldposition = Position;
            Vector2 oldsize = Size;

            switch (Orientation)
            {
                case OrientationType.Horizontal:
                    Size = new Vector2(Value * Size.X, Size.Y);
                    break;
                case OrientationType.Vertical:
                    Size = new Vector2(Size.X, Value*Size.Y);
                    Position = new Vector2(Position.X, oldposition.Y+(1.0f - Value) * oldsize.Y);
                    break;
            }
            Color = BarColor;

            base.Draw(r,n);

            Color = oldcolor;
            Position = oldposition;
            Size = oldsize;*/
        }

        public float Value;
        public OrientationType Orientation;
        public Color4f BarColor;
    }

    public class TabWindow : Window
    {
        public TabWindow(Type[] windows, int show)
            : base(new Layout(windows.Length,2))
        {
            Windows = windows;
            Buttons = new Button[windows.Length];
            Layout.GetCell(0, 1).Span.X = windows.Length;
            Layout.Heights[0] = 0.1f;

            for (int i = 0; i < windows.Length; ++i)
            {
                Add(Buttons[i] = new Button(OnButtonPressed, windows[i].Name), i, 0);
            }

            Show(show);
        }
        public void OnButtonPressed(Button source, int button, float x, float y)
        {
            int i = Array.IndexOf<Button>(Buttons, source);
            Show(i);
        }

        public void Show(int window)
        {
            Show(Windows[window]);
        }

        public void Show(Type window)
        {

            if (Current != null)
            {
                Current.Close();
                Current = null;
            }

            Current = (Window)Activator.CreateInstance(window);
            Add(Current, 0, 1);

            Layout.Update(size);
        }

        Type[] Windows;
        Window Current;
        Button[] Buttons;
    }

    public class MeshWindow : Window
    {
        public MeshWindow(Mesh m)
        {
            Mesh = m;
        }
        public MeshWindow(float x, float y, float w, float h, Mesh m)
            : base(x,y,w,h)
        {
            Mesh = m;
        }

        public override void DrawInternal(IRenderer r, RectangleF rect)
        {
            base.DrawInternal(r, rect);

            r.PushMatrix();
            r.MultMatrix(Matrix4Extensions.FromScale(Size.X, -Size.Y, 0));
            r.MultMatrix(Matrix4Extensions.FromTranslation(0,-1,0));
            r.MultMatrix(Matrix4Extensions.FromTranslation(0.5f, 0.5f, 0));
            r.MultMatrix(Matrix4Extensions.FromScale(Scale.X, Scale.Y, 0));
            //r.MultMatrix(Matrix4.FromAngleAxis(Vector3.ZAxis, Root.Instance.Time));
            //r.UseShader(Mesh.Material.Shader);
            Mesh.Draw(r,null);
            r.PopMatrix();
        }

        public Vector2 Scale=new Vector2(1,1);
        Mesh Mesh;
    }

    public class Label : Window
    {
        public Label(MeshFont font,string text)
        {
            Font = font;
            Caption = text;
            //Transparent = true;
        }
        public Label(float x, float y, float w, float h, MeshFont font, string text)
            : base(x, y, w, h)
        {
            Font = font;
            Caption = text;
            //Transparent = true;
        }

        public override void DrawInternal(IRenderer r, RectangleF rect)
        {
            base.DrawInternal(r, rect);

            //r.PushMatrix();
            //r.MultMatrix(Matrix4Extensions.FromTranslation(0, -1, 0));
            //r.UseShader(Mesh.Material.Shader);
            Vector2 pos = (Size/2)+new Vector2(-Font.Width * (float)Caption.Length*0.5f, -Font.size/2);
            Font.Draw(r,Caption,pos.X,pos.Y,new Color4f(1,1,1),false,32,48);
            //r.PopMatrix();
        }

        MeshFont Font;
        public string Caption;
    }

    public class Window : ITickable
	{
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public Window(float x, float y, float w, float h)
		{
			position.X=x;
			position.Y=y;
			size.X=w;
			size.Y=h;
			vertices=CreateVB();
			FillVB();
		}
        /*~Window()
        {
            //Console.WriteLine("~Window: vertexbuffer freed.");
            if(Root.Instance.UserInterface!=null)
            Root.Instance.UserInterface.Renderer.FreeVertexBuffer(vertices);
        }*/

		public Window()
		{
			vertices=CreateVB();
			FillVB();
		}

        public void Remove()
        {
            Kill = true;
        }

        public virtual void Close()
        {
            foreach (Window w in Children)
            {
                w.Close();
            }
            Closing = true;
            Fade = 1;
        }

		public Window(Layout lo)
		{
			vertices=CreateVB();
			Layout=lo;
			FillVB();
		}
		
        public Window(Color4f c)
		{
            color=c;
			vertices=CreateVB();
			FillVB();
		}

		public Window(float x,float y,float w,float h,Layout lo)
		{
			position.X=x;
			position.Y=y;
			size.X=w;
			size.Y=h;
			vertices=CreateVB();
			Layout=lo;
			FillVB();
		}

        public virtual void DrawInternal(IRenderer r, RectangleF rect)
        {
        }

        public virtual void Draw(IRenderer r, RectangleF rect)
		{
            if (!Visible)
                return;

            rect.Offset(-position.X-CurrentScrollPosition.X, -position.Y-CurrentScrollPosition.Y);
            //RectangleF rect2 = rect;
            rect.Intersect(new RectangleF(-CurrentScrollPosition.X, -CurrentScrollPosition.Y, size.X, size.Y));
            float[] Scissor = new float[] { rect.Left, rect.Top, rect.Right, rect.Bottom };
            //rect.Intersect(new RectangleF(CurrentScrollPosition.X, CurrentScrollPosition.Y, size.X, size.Y));
   
           
            Matrix4 pos=Matrix4Extensions.FromTranslation(position.X,position.Y,0);//*Matrix4Extensions.FromScale(size.X,size.Y,0);
			if(texture!=null)
				r.BindTexture(texture.Id);
			else
				r.BindTexture(null);

			r.PushMatrix();
			r.MultMatrix(pos);


            if ((Color.a > 0.0f || Shader!=null) && !Transparent)
            {
                r.UseShader(Shader);

                if (Shader != null)
                {
                    r.SetUniform(Shader.GetUniformLocation("WindowSize"), new float[] { size.X, size.Y });
                    r.SetUniform(Shader.GetUniformLocation("Time"), new float[] { Root.Instance.Time });

                    int loc = Shader.GetUniformLocation("Scissor");
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, Scissor);
                    }
                    if (ShaderParams != null)
                        ShaderParams.Apply(r);
                }
                r.Draw(vertices, PrimitiveType.QUADS, 0, 4, null);
            }


            Matrix4 scroll = Matrix4Extensions.FromTranslation(CurrentScrollPosition.X, CurrentScrollPosition.Y, 0);
            r.MultMatrix(scroll);
            foreach (Window w in Children)
			{
                if (
                    w.Position.X + CurrentScrollPosition.X + w.Size.X < 0 || w.Position.X + CurrentScrollPosition.X  > Size.X ||
                    w.Position.Y + CurrentScrollPosition.Y + w.Size.Y < 0 || w.Position.Y + CurrentScrollPosition.Y  > Size.Y
                    )
                {
                    continue;
                }

                w.Draw(r, rect);
			}
            DrawInternal(r, rect);

            r.PopMatrix();
		}

        public ShaderParams ShaderParams;
        public Shader Shader = Root.Instance.ResourceManager.LoadShader("window.border.shader");
	
		protected DynamicVertexBuffer CreateVB()
		{
            if (Root.Instance.UserInterface == null)
                return null;

            vertices = Root.Instance.UserInterface.Renderer.CreateDynamicVertexBuffer(VertexFormat.VF_P2C4T2.Size * 4);
            vertices.Format = VertexFormat.VF_P2C4T2;
			data=new VertexP2C4T2[4];
			
			return vertices;
		}
		
		protected void FillVB()
		{
            if (Root.Instance.UserInterface == null)
                return;

            float u1 = 0.0f;
            float v1 = FlipTexture ? 1.0f : 0.0f;
			float u2=1.0f;
            float v2 = 1.0f - v1;

            float alpha = color.a;
            if (Fade >= 0)
                alpha *= Fade;

            data[0].position.X = 0; data[0].position.Y = 0;
            data[0].color.r = color.r; data[0].color.g = color.g; data[0].color.b = color.b; data[0].color.a = alpha;
            data[0].texture0.X = u1; data[0].texture0.Y = v1;

            data[1].position.X = size.X; data[1].position.Y = 0;
            data[1].color.r = color.r; data[1].color.g = color.g; data[1].color.b = color.b; data[1].color.a = alpha;
            data[1].texture0.X = u2; data[1].texture0.Y = v1;

            data[2].position.X = size.X; data[2].position.Y = size.Y;
            data[2].color.r = color.r; data[2].color.g = color.g; data[2].color.b = color.b; data[2].color.a = alpha;
            data[2].texture0.X = u2; data[2].texture0.Y = v2;

            data[3].position.X = 0; data[3].position.Y = size.Y;
            data[3].color.r = color.r; data[3].color.g = color.g; data[3].color.b = color.b; data[3].color.a = alpha;
            data[3].texture0.X = u1; data[3].texture0.Y = v2;

			vertices.Update(data,data.Length*vertices.Format.Size);
		}

		public virtual void OnClick(int button,float x,float y)
		{
            if (Parent != null)
                Parent.OnChildClick(this, button);
            //Console.WriteLine("window:onclick: " + x.ToString() + ", " + y.ToString());

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Window w = (Window)Children[i];
                if (
                    x >= w.Position.X + CurrentScrollPosition.X && x <= w.Position.X + CurrentScrollPosition.X + w.Size.X &&
                    y >= w.Position.Y + CurrentScrollPosition.Y && y <= w.Position.Y + CurrentScrollPosition.Y + w.Size.Y &&
                    w.Visible
					)
				{
                    w.OnClick(button, x - w.Position.X - CurrentScrollPosition.X, y - w.Position.Y - CurrentScrollPosition.Y);
					break;
				}
			}
		}

        public virtual void OnChildClick(Window w, int button)
        {
        }

        public virtual void OnKeyPress(char c)
        {
        }

        public virtual void OnChildKeyDown(Window w, global::OpenTK.Input.Key key)
        {
        }

        public virtual void OnKeyDown(global::OpenTK.Input.Key key)
		{
            if (Parent != null)
            {
                Parent.OnChildKeyDown(this, key);
            }
		}

		public void Center()
		{
            if (Root.Instance.UserInterface == null)
                return;

			Point p=Root.Instance.UserInterface.Renderer.Size;
			Position=new Vector2(p.X/2-Size.X/2,p.Y/2-Size.Y/2);
		}

        public delegate void TooltipDelegate(string text);
        public TooltipDelegate Tooltip;
        public string TooltipText;

		public virtual void OnGetFocus()
		{
            if (Tooltip != null && TooltipText != null)
                Tooltip(TooltipText);
		}
		public virtual void OnLoseFocus()
		{
            //if (Tooltip != null && TooltipText != null)
           //     Tooltip("");
        }

		public virtual void OnMouseMove(float x,float y)
		{
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Window w = (Window)Children[i];
                if (
                    x >= w.Position.X + ScrollPosition.X && x <= w.Position.X + CurrentScrollPosition.X + w.Size.X &&
                    y >= w.Position.Y + ScrollPosition.Y && y <= w.Position.Y + CurrentScrollPosition.Y + w.Size.Y &&
                    w.Visible
					)
				{
                    w.OnMouseMove(x - w.Position.X - CurrentScrollPosition.X, y - w.Position.Y - ScrollPosition.Y);
					return;
				}
			}
			if(Root.Instance.Gui.Focus!=null)
			{
				if(Root.Instance.Gui.Focus!=this)
				{
					OnGetFocus();
					Root.Instance.Gui.Focus.OnLoseFocus();
					Root.Instance.Gui.Focus=this;
				}
			}
			else
			{
				OnGetFocus();
				Root.Instance.Gui.Focus=this;
			}

		}

		public Color4f Color
		{
			get
			{
				return color;
			}
			set
			{
				color=value;
				FillVB();
			}
		}
		public Vector2 Position
		{
			get
			{
				return position;
			}
			set
			{
				position=value;
				//FillVB();
			}
		}

        public virtual void OnResize()
        {
        }

		public Vector2 Size
		{
			get
			{
				return size;
			}
			set
			{
				size=value;
                OnResize();
                if (Layout != null)
                    Layout.Update(size);
                FillVB();
			}
		}

		public Vector2 CenterPosition
		{
			get
			{
				return Position+Size/2;
			}
            set
            {
                Position = value - Size / 2;
            }
        }

		public Vector2 AbsoluteCenterPosition
		{
			get
			{
				if(Parent!=null)
				{
                    return CenterPosition + Parent.AbsolutePosition + Parent.CurrentScrollPosition;
				}
				else
				{
					return CenterPosition;
				}
			}
		}

        public bool GetChildBounding(out Vector2 min, out Vector2 max)
        {
            min = new Vector2(float.PositiveInfinity,float.PositiveInfinity);
            max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            if (Children.Count == 0)
                return false;

            foreach (Window w in Children)
            {
                min.X = Math.Min(min.X, w.Position.X);
                min.Y = Math.Min(min.Y, w.Position.Y);
                max.X = Math.Max(max.X, w.Position.X + w.Size.X);
                max.Y = Math.Max(max.Y, w.Position.Y + w.Size.Y);
            }
            return true;
        }

		public Vector2 AbsolutePosition
		{
			get
			{
				if(Parent!=null)
				{
					return position+Parent.AbsolutePosition+Parent.CurrentScrollPosition;
				}
				else
				{
					return position;
				}
			}
		}

		public void Add(Window w)
		{
			w.Parent=this;
			Children.Add(w);
		}
		
		public void Add(Window w,int layoutposx,int layoutposy)
		{
			w.Parent=this;
			Children.Add(w);
			Layout.GetCell(layoutposx,layoutposy).Windows.Add(w);
		}

		public virtual void Tick(float dtime)
		{
            position += Speed * dtime;
            Age += dtime;

            if (Fade >= 0)
            {
                if (Closing)
                {
                    Fade -= dtime;
                    if (Fade >= 0)
                        FillVB();
                    else
                    {
                        Fade = -1;
                        Remove();
                    }
                }
                else
                {
                    Fade += dtime;
                    if (Fade < 1)
                        FillVB();
                    else
                    {
                        Fade = -1;
                        FillVB();
                    }
                }
            }

            Vector2 delta = ScrollPosition - CurrentScrollPosition;
            float l1 = dtime * ScrollSpeed;
            float l2=delta.Length;
            if (l1 >= l2)
                CurrentScrollPosition = ScrollPosition;
            else
            {
                Vector2 v = delta;
                v.Normalize();
                CurrentScrollPosition += v*l1;
            }

            Window[] kill = new Window[Children.Count];
            int i = 0;
            foreach (Window w in Children)
            {
                w.Tick(dtime);
                if (w.Kill)
                    kill[i++] = w;
            }

            for (int j = 0; j < i; ++j)
            {
                Window w = kill[j];
                if (w.Parent != null)
                    w.Parent.Children.Remove(w);
                else
                    Root.Instance.Gui.windows.Remove(w);
            }
        }

		protected Vector2 position;
		protected Vector2 size;
		public Texture texture;
		protected Color4f color=new Color4f(0.3f,0.3f,0.3f,0.7f);
		protected DynamicVertexBuffer vertices;
		protected VertexP2C4T2[] data;
		public ArrayList Children=new ArrayList();
		public Window Parent;
		public bool Moveable=false;
		public bool Sizeable=false;
		protected bool Moving=false;
		protected bool Sizing=false;
		public Layout Layout;
        public Vector2 Speed;
        public object UserData=null;
        public bool Visible = true;
        public bool Transparent = false;
        public bool FlipTexture = false;
        public Vector2 ScrollPosition;
        public Vector2 CurrentScrollPosition;
        public float ScrollSpeed = 512;
        public float Age=0;
        public float Fade = 0;
        public bool Closing = false;
        public bool Kill = false;
    }

    public class Style
    {
        public Color4f BackgroundColor;
        public Color4f HighlightColor;
        public Color4f TextColor;
    }

    public class Passthrough : PostProcess.IPass
    {
        public Passthrough()
        {
            w=new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            w.FlipTexture = true;
            w.Color = new Color4f(1, 1, 1, 1);
            w.Shader = null;// Root.Instance.ResourceManager.LoadShader("window.textured.shader");
        }

        #region IPass Members

        public void Render(IRenderer r, RenderTarget t)
        {
            r.SetMode(RenderMode.Draw2D);
            w.texture = new Texture(t.Texture);
        }

        #endregion

        Window w;
    }

    public class Distort : PostProcess.IPass
    {
        public Distort()
        {
            w = new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            w.FlipTexture = true;
            w.Color = new Color4f(1, 1, 1, 1);
            w.Shader = Root.Instance.ResourceManager.LoadShader("postprocess.distort.shader");
        }

        #region IPass Members

        public void Render(IRenderer r, RenderTarget t)
        {
            r.SetMode(RenderMode.Draw2D);
            w.texture = new Texture(t.Texture);
            //Window w = new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            //w.Draw(r, null);
        }

        #endregion
        Window w;
    }

    public class Grayscale : PostProcess.IPass
    {
        public Grayscale()
        {
            w = new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            w.FlipTexture = true;
            w.Color = new Color4f(1, 1, 1, 1);
            w.Shader = Root.Instance.ResourceManager.LoadShader("postprocess.grayscale.shader");
        }

        #region IPass Members

        public void Render(IRenderer r, RenderTarget t)
        {
            r.SetMode(RenderMode.Draw2D);
            w.texture = new Texture(t.Texture);
            w.Draw(r, new RectangleF(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y));
        }

        #endregion
        Window w;
    }
    public class Invert : PostProcess.IPass
    {
        public Invert()
        {
            w = new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            w.FlipTexture = true;
            w.Color = new Color4f(1, 1, 1, 1);
            w.Shader = Root.Instance.ResourceManager.LoadShader("postprocess.invert.shader");
        }

        #region IPass Members

        public void Render(IRenderer r, RenderTarget t)
        {
            r.SetMode(RenderMode.Draw2D);
            w.texture = new Texture(t.Texture);
            w.Draw(r, new RectangleF(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y));
        }

        #endregion
        Window w;
    }

    public class Shift : PostProcess.IPass
    {
        public Shift()
        {
            w = new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            w.FlipTexture = true;
            w.Color = new Color4f(1, 1, 1, 1);
            w.Shader = Root.Instance.ResourceManager.LoadShader("postprocess.shift.shader");
        }

        #region IPass Members

        public void Render(IRenderer r, RenderTarget t)
        {
            r.SetMode(RenderMode.Draw2D);
            w.texture = new Texture(t.Texture);
            w.Draw(r, new RectangleF(0,0,1000,1000));
            w.Draw(r, new RectangleF(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y));
        }

        #endregion
        Window w;
    }
    public class Smooth : PostProcess.IPass
    {
        public Smooth()
        {
            w = new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            w.FlipTexture = true;
            w.Color = new Color4f(1, 1, 1, 1);
            w.Shader = Root.Instance.ResourceManager.LoadShader("postprocess.smooth.shader");
        }

        #region IPass Members

        public void Render(IRenderer r, RenderTarget t)
        {
            r.SetMode(RenderMode.Draw2D);
            w.texture = new Texture(t.Texture);
            w.Draw(r, new RectangleF(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y));
        }

        #endregion
        Window w;
    }

    public class Bloom : PostProcess.IPass
    {
        public Bloom()
        {
            w = new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            w.FlipTexture = true;
            w.Color = new Color4f(1, 1, 1, 1);
            w.Shader = Root.Instance.ResourceManager.LoadShader("postprocess.bloom.shader");
        }

        #region IPass Members

        public void Render(IRenderer r, RenderTarget t)
        {
            r.SetMode(RenderMode.Draw2D);
            w.texture = new Texture(t.Texture);
            w.Draw(r, new RectangleF(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y));
        }

        #endregion
        Window w;
    }

    public class Ripple : PostProcess.IPass
    {
        public Ripple()
        {
            w = new Window(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y);
            w.FlipTexture = true;
            w.Color = new Color4f(1, 1, 1, 1);
            w.Shader = Root.Instance.ResourceManager.LoadShader("postprocess.ripple.shader");
        }

        #region IPass Members

        public void Render(IRenderer r, RenderTarget t)
        {
            r.SetMode(RenderMode.Draw2D);
            w.texture = new Texture(t.Texture);
            w.Draw(r, new RectangleF(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y));
        }

        #endregion
        Window w;
    }

    public class PostProcess
    {
        public interface IPass
        {
            void Render(IRenderer r, RenderTarget t);
        }

        public RenderTarget Target;
        public Point Size;
        public List<IPass> Passes = new List<IPass>();
        IRenderer Renderer;

        public PostProcess(int width,int height,IRenderer renderer)
        {
            Size.X=width;
            Size.Y=height;
            Renderer=renderer;
            //Passes.Add(new Ripple());
            //Passes.Add(new Shift());
            //Passes.Add(new Distort());
            //Passes.Add(new Invert());
            //Passes.Add(new Smooth());
            try
            {
                Renderer.CreateRenderTarget(null, null);
            }
            catch (Exception e)
            {
                return;
            }
          
            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");
            bool bloom = true;
            if (c.Table.ContainsKey("postprocess.bloom"))
            {
                bloom = c.GetBool("postprocess.bloom");
            }
            if(bloom)
                Passes.Add(new Bloom());
        }

        public void Enable(IRenderer r)
        {
            if (Passes.Count > 0)
            {
                if (Target == null)
                {
                    Target = r.CreateRenderTarget(r.CreateTexture(new byte[Size.X * Size.Y * 3], Size.X, Size.Y, false), r.CreateDepthTexture(Size.X,Size.Y));
                }

                r.BindRenderTarget(Target);
                Renderer = r;
            }
        }

        public void Render()
        {
            if (Passes.Count > 0)
            {
                for (int i = 0; i < Passes.Count - 1; ++i)
                {
                    Passes[i].Render(Renderer,Target);
                }

                Renderer.BindRenderTarget(null);
                Renderer.SetCamera(new Camera());
                Passes[Passes.Count - 1].Render(Renderer, Target);
            }
        }
    }

    public class Gui : ITickable
	{
		public Gui()
		{
			//DefaultFont=new Font("font3.png");
            DefaultFont = Root.Instance.ResourceManager.LoadFont("default.font");
            Console = new InGameConsole(this);
		}

        RectangleF CreateFullRect()
        {
            Point s = Root.Instance.UserInterface.Renderer.Size;
            return new RectangleF(0, 0, s.X, s.Y);
            //return new RectangleF(float.MinValue, float.MinValue, float.PositiveInfinity, float.PositiveInfinity);
        }

		public void Draw(IRenderer r)
		{
            RectangleF rect=CreateFullRect();
            foreach (Window w in windows)
			{
                w.Draw(r, rect);
			}
			if(Console.enabled)
                Console.Draw(r, rect);
		}

		public void OnMouseDown(int button,int x,int y)
		{
			//Cheetah.Console.WriteLine("button: "+button.ToString()+" pos: "+x.ToString()+", "+y.ToString());
			//foreach(Window w in windows)
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                Window w = (Window)windows[i];
                    if (
					x>=w.Position.X&&x<=w.Position.X+w.Size.X&&
					y>=w.Position.Y&&y<=w.Position.Y+w.Size.Y&&
                    w.Visible
					)
				{
					w.OnClick(button,x-w.Position.X,y-w.Position.Y);
                    //Focus = w;
					break;
				}

			}
		}

		public void OnMouseMove(int x,int y)
		{
			//foreach(Window w in windows)
            for(int i=windows.Count-1;i>=0;i--)
			{
                Window w = (Window)windows[i];
				if(
					x>=w.Position.X&&x<=w.Position.X+w.Size.X&&
					y>=w.Position.Y&&y<=w.Position.Y+w.Size.Y&&
                    w.Visible
					)
				{
					w.OnMouseMove(x-w.Position.X,y-w.Position.Y);
					return;
				}

			}
			if(Focus!=null)
			{
				Focus.OnLoseFocus();
				Focus=null;
			}
		}

        public void OnKeyPress(char key)
        {
			if(Console.enabled)
			{
				Console.OnKeyPress(key);
			}
        }

        public void OnKeyDown(global::OpenTK.Input.Key key)
		{
            if (key == global::OpenTK.Input.Key.F10)
			{
				Console.enabled=!Console.enabled;
			}
			else if(Console.enabled)
			{
				Console.OnKeyDown(key);
			}
			else if(Focus!=null)
			{
				Focus.OnKeyDown(key);
			}
		}

		public Window FindWindowByType(Type t)
		{
			foreach(Window w in windows)
				if(w.GetType()==t)
					return w;
			return null;
		}
		
		public void Tick(float dtime)
		{
            Window[] kill = new Window[windows.Count];
            int i = 0;
            foreach (Window w in windows)
            {
                w.Tick(dtime);
                if (w.Kill)
                    kill[i++] = w;
            }

            for (int j = 0; j < i; ++j)
            {
                Window w = kill[j];
                if (w.Parent != null)
                    w.Parent.Children.Remove(w);
                else
                    Root.Instance.Gui.windows.Remove(w);
            }

            Console.Tick(dtime);
		}


		public ArrayList windows=new ArrayList();
		public Window Focus;
		public Font DefaultFont;
		public InGameConsole Console;
	}

    public class EntityCollection : IEnumerable<KeyValuePair<int, Entity>>
    {
        class Enumerator : IEnumerator<KeyValuePair<int,Entity>>
        {
            public Enumerator(EntityCollection c)
            {
                coll = c;
                idx = -1;
            }
            EntityCollection coll;
            int idx;


            public void Dispose()
            {
            }

            KeyValuePair<int, Entity> IEnumerator<KeyValuePair<int, Entity>>.Current
            {
                get { return coll.List[idx]; }
            }
            object IEnumerator.Current
            {
                get { return coll.List[idx]; }
            }
            public bool MoveNext()
            {
                if (idx < coll.Count - 1)
                {
                    idx++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                idx = -1;
            }
        }

        /*struct Entry
        {
            public Entry(Entity obj, int idx)
            {
                Obj = obj;
                Idx = idx;
            }
            public Entity Obj;
            public int Idx;
        }*/

        public void Clear()
        {
            Count = 0;
        }

        public Entity this[int index]
        {
            get
            {
                for (int i = 0; i < Count; ++i)
                    if (List[i].Key == index)
                        return List[i].Value;
                return null;
            }
            set
            {
                for (int i = 0; i < Count; ++i)
                {
                    if (List[i].Key == index)
                    {
                        List[i] = new KeyValuePair<int, Entity>(index, value);
                        return;
                    }
                }
                if (Count >= Capacity)
                    throw new Exception("list full");

                List[Count++] = new KeyValuePair<int, Entity>(index, value);

            }
        }

        public void Remove(int index)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (List[i].Key == index)
                {
                    List[i] = List[--Count];
                    return;
                }
            }
        }

        const int Capacity = 256;
        KeyValuePair<int, Entity>[] List = new KeyValuePair<int, Entity>[Capacity];
        int Count;

        #region IEnumerable<Entity> Members

        IEnumerator<KeyValuePair<int, Entity>> IEnumerable<KeyValuePair<int, Entity>>.GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion

    }

	public class Scene : ISerializable,IResource
	{
        public void Dispose()
        {
        }

        class LightComparer : IComparer
		{
			public LightComparer(Node n)
			{
				target=n;
			}

			public int Compare(object o1,object o2)
			{
                //HACK?!
                if (o1 == null || o2 == null || o1==o2)
                    return 0;

				Light l1=(Light)o1;
				Light l2=(Light)o2;

                if (l1.directional)
                {
                    return -1;
                }
                else if(l2.directional)
                {
                    return 1;
                }

                Vector3 v1 = target.AbsolutePosition - l1.AbsolutePosition;
                Vector3 v2 = target.AbsolutePosition - l2.AbsolutePosition;
                float d1=v1.Length;
				float d2=v2.Length;

                if (l1.Range > 0 && d1 - target.RenderRadius > l1.Range)
                {
                    return 1;
                }
                else if (l2.Range > 0 && d2 - target.RenderRadius > l2.Range)
                {
                    return -1;
                }

				if(d1<d2)
				{
					return -1;
				}
				else if(d1>d2)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}

			Node target;
		}

        class NodeComparer : IComparer
        {
            public NodeComparer()
            {
            }

            public int Compare(object o1, object o2)
            {
                Node l1 = (Node)o1;
                Node l2 = (Node)o2;
                if (l2.Transparent < l1.Transparent)
                {
                    return 1;
                }
                else if (l2.Transparent > l1.Transparent)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
        
        public Scene(DeSerializationContext context)
		{
			DeSerialize(context);
		}

		public Scene()
		{
			if(!Root.Instance.IsAuthoritive)
			{
                //System.Console.WriteLine("creating client list");
				//ClientList=new Dictionary<int,Entity>();
                ClientList = new EntityCollection();
            }

            Physics = Cheetah.Physics.Physics.Create();
		}

        public bool ServerListCheck(int id)
        {
            //return ServerList.ContainsKey(id);
            return ServerList[id]!=null;
        }
        public void ServerListAdd(int id, Entity e)
        {
            //if (ServerList.(e))
            //    throw new Exception();
            ServerList[id] = e;
            e.Scene = this;

            //System.Console.WriteLine("ADDING: " + e+ "id: "+id);
            for (Type t = e.GetType(); t != typeof(Entity); t = t.BaseType)
            {
                if (TypeList.ContainsKey(t))
                {
                    TypeList[t][id]=e;
                }
                else
                {
                   // Dictionary<bool, Dictionary<int, Entity>> d = new Dictionary<bool, Dictionary<int, Entity>>();
                    Dictionary<int, Entity> d2 = new Dictionary<int, Entity>();
                    //d.Add(true,d2=new Dictionary<int,Entity>());
                    //if(ClientList!=null)
                    //    d.Add(false,new Dictionary<int,Entity>());

                    d2.Add(id, e);
                    TypeList[t] = d2;
                }
            }
            if (e is Node && ((Node)e).GetCollisionInfo()!=null)
            {
                ServerCollideList.Add(id, (Node)e);
            }
            e.OnAdd(this);
            if (SpawnEvent!=null)
                SpawnEvent(e);
        }
        public void ServerListRemove(int id)
        {
            Entity e = ServerList[id];
          //e.Scene = null;

            //Console.WriteLine("REMOVING: " + e);
            for (Type t = e.GetType(); t != typeof(Entity); t = t.BaseType)
            {
                //if (TypeList.ContainsKey(t))
                {
                    TypeList[t].Remove(id);
                }
            }
            ServerList.Remove(id);

            if (ServerCollideList.ContainsKey(id))
                ServerCollideList.Remove(id);
        }
        public Entity ServerListGet(int id)
        {
            try
            {
                return ServerList[id];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ClientListCheck(int id)
        {
            return ClientList[id] != null;
        }
        public void ClientListAdd(int id, Entity e)
        {
            ClientList[id] = e;
            e.Scene = this;

            for (Type t = e.GetType(); t != typeof(Entity); t = t.BaseType)
            {
                if (TypeList.ContainsKey(t))
                {
                    TypeList[t][id] = e;
                }
                else
                {
                    //Dictionary<bool, Dictionary<int, Entity>> d = new Dictionary<bool, Dictionary<int, Entity>>();
                    Dictionary<int, Entity> d2 = new Dictionary<int, Entity>();
                    //d.Add(true, new Dictionary<int, Entity>());
                    //d.Add(false, d2 = new Dictionary<int, Entity>());

                    d2.Add(id, e);
                    TypeList[t] = d2;
                }
            }

            if (e is Node && ((Node)e).GetCollisionInfo() != null)
            {
                ClientCollideList.Add(id, (Node)e);
            }

            e.OnAdd(this);
            if (SpawnEvent != null)
                SpawnEvent(e);
        }
        public void ClientListRemove(int id)
        {
            Entity e = ClientList[id];
            //e.Scene = null;
            ClientList.Remove(id);

            for (Type t = e.GetType(); t != typeof(Entity); t = t.BaseType)
            {
                //if (TypeList.ContainsKey(t))
                {
                    TypeList[t].Remove(id);
                }
            }
            if (ClientCollideList.ContainsKey(id))
                ClientCollideList.Remove(id);
        }
        public Entity ClientListGet(int id)
        {
            try
            {
                return ClientList[id];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public T FindEntityByType<T>() where T:Entity
        {

            if (TypeList.ContainsKey(typeof(T)))
            {
                foreach (KeyValuePair<int, Entity> kv in TypeList[typeof(T)])
                {
                            return (T)kv.Value;
                }
                return null;
            }
            else
                return null;
        }

        public void Clear()
        {
            ServerList.Clear();
            if (ClientList != null)
            {
                ClientList.Clear();
                //ClientTypeList.Clear();
            }
            BlackList.Clear();
            TypeList.Clear();
            KillQueue.Clear();
            ClientCollideList.Clear();
            ServerCollideList.Clear();
            SpawnEvent = null;
            RemoveEvent = null;
        }

        public IList<T> FindEntitiesByType<T>() where T:Entity
        {
            List<T> list = new List<T>();

            if (typeof(T) == typeof(Entity))
            {
                foreach (KeyValuePair<int, Entity> kv in ServerList)
                {
                    list.Add((T)kv.Value);
                }
                if(ClientList!=null)
                    foreach (KeyValuePair<int, Entity> kv in ClientList)
                {
                    list.Add((T)kv.Value);
                }
            }
            else
            {
                if (TypeList.ContainsKey(typeof(T)))
                {
                    foreach (KeyValuePair<int, Entity> kv in TypeList[typeof(T)])
                    {
                        list.Add((T)kv.Value);
                    }
                }
            }
            return list;
        }

        public IList FindEntitiesByType(Type t)
        {
            ArrayList list = new ArrayList();

            if (TypeList.ContainsKey(t))
            {
                foreach (KeyValuePair<int, Entity> kv in TypeList[t])
                {
                    list.Add(kv.Value);
                }
            }

            return list;
        }

        public Entity Find(int ownernumber, int ownerindex)
		{
            foreach (KeyValuePair<int, Entity> de in ServerList)
            //foreach (Entity e in ServerList)
            {
				Entity e=de.Value;
				if(e.ClientIndex==ownerindex&&e.OwnerNumber==ownernumber)
					return e;
			}
			return null;
		}
        public Entity Find(int ownernumber, int ownerindex,int serverindex)
        {
            foreach (KeyValuePair<int, Entity> de in ServerList)
            //foreach (Entity e in ServerList)
            {
                Entity e = de.Value;
                if (e.ClientIndex == ownerindex && e.OwnerNumber == ownernumber && e.ServerIndex == serverindex)
                    return e;
            }
            return null;
        }
		protected void Draw(IRenderer r,Node n,Light[] lights)
		{
            Matrix4 m;
            if (!Root.Instance.IsAuthoritive && 
                (
                (Root.Instance.Connection !=null&& n.OwnerNumber != ((UdpClient)Root.Instance.Connection).ClientNumber)
                ||
                (Root.Instance.Connection != null && n.Attach!=null && n.Attach.OwnerNumber != ((UdpClient)Root.Instance.Connection).ClientNumber)
                ||
                Root.Instance.Player!=null
                )
                )
            {
                m = n.SmoothMatrix;
            }
            else
            {
                m = n.Matrix;
            }

            int numlights = SetupLighting(r,n,lights);


            foreach (IDrawable d in n.Draw)
            {
                if (!d.IsWorldSpace)
                {
                    r.PushMatrix();
                    r.MultMatrix(m);
                }
                d.Draw(r, n);

                if (!d.IsWorldSpace)
                    r.PopMatrix();
            }
        }

		protected bool IsServer
		{
			get{return ClientList==null;}
		}
		protected bool IsClient
		{
			get{return ClientList!=null;}
		}

        protected bool IsOnBlackList(int serverid)
        {
            return BlackList.Contains(serverid);
        }

        Queue<Entity> KillQueue = new Queue<Entity>();

        public void MarkKill(Entity e)
        {
            //if (e.Scene != this)
            //    throw new Exception();

            //Console.WriteLine("marked: " + e.ToString());
            //Console.WriteLine(ToString());
            KillQueue.Enqueue(e);
        }

        public event SpawnDelegate RemoveEvent;

        public void RemoveQueuedEntities()
        {
            Entity e;
            while(KillQueue.Count>0)
            {
                e = KillQueue.Dequeue();
                if (!e.Kill)
                {
                    Console.WriteLine("bla");
                    continue;
                }

                //Console.WriteLine("removing entity " + e.ToString());
                //if (ServerList.ContainsKey(e.ServerIndex))
                if (ServerList[e.ServerIndex]!=null)
                {
                    e.OnKill();
                    if (Root.Instance.CurrentFlow != null)
                        Root.Instance.CurrentFlow.OnEntityKill(e);
                    if (ServerList[e.ServerIndex] != e)
                        throw new Exception();
                    ServerListRemove(e.ServerIndex);
                }
                else if (ClientList != null && ClientList[e.ClientIndex]!=null)
                {
                    e.OnKill();
                    if (Root.Instance.CurrentFlow != null)
                        Root.Instance.CurrentFlow.OnEntityKill(e);
                    if (ClientList[e.ClientIndex] != e)
                        throw new Exception();
                    ClientListRemove(e.ClientIndex);
                }
                else
                {
                    Console.WriteLine("strange: " + e.ToString() + " not in list.");
                }

                if (RemoveEvent != null)
                    RemoveEvent(e);

                BlackList.Enqueue(e.ServerIndex);

                if (BlackList.Count > 20)
                    BlackList.Dequeue();
            }

        }

        protected void KillDirtyEntities()
		{
            foreach (KeyValuePair<int, Entity> de in ServerList)
            //foreach(Entity e in ServerList)
            {
				Entity e=de.Value;
				if(e.Dirty||e.Kill)
				{
                    if (!e.Kill && e.NoReplication)
                        continue;
                    e.Kill = true;
                    
                }
			}
		}

		public override string ToString()
		{
			string s="ClientList:\r\n";
			if(ClientList!=null)
                foreach (KeyValuePair<int, Entity> de in ClientList)
                //foreach (Entity e in ServerList)
                {
                    Entity e=de.Value;
                    int clientindex = de.Key;

					s+=clientindex+":\t"+e.ToString()+"\r\n";
				}
			s+="ServerList:\r\n";
			if(ServerList!=null)
                foreach (KeyValuePair<int, Entity> de in ServerList)
                //foreach (Entity e in ServerList)
                {
					int serverindex=de.Key;
					Entity e=de.Value;

					s+=serverindex+":\t"+e.ToString()+"\r\n";
				}
            s += "TypeList:\r\n";
            if (TypeList != null)
                s+=TypeList.Count.ToString();

            return s;
		}

        public delegate void SpawnDelegate(Entity e);
        public event SpawnDelegate SpawnEvent;

		public void Spawn(Entity e)
		{
			int n=Root.Instance.NextIndex++;
				//e.OnAdd(this);
            e.Kill = e.Dirty = false;
            if(IsServer)
			{
                ServerListAdd(n, e);
				e.ServerIndex=n;
			}
			else
			{
                n *= -1;
				ClientListAdd(n,e);
				e.ClientIndex=n;
				e.OwnerNumber=ClientNumber;
			}
		}

        public void ClientStateReceive(DeSerializationContext context, short client)
        {
            //BinaryReader r = context;
            //Stream s = context.Stream;

            foreach (KeyValuePair<int, Entity> de in ServerList)
            {
                de.Value.Dirty = true;
            }


            //ClientListe laden, server schickt nie eine...
            if (context.ReadBoolean())
            {
                throw new Exception("kann nich angehn...");
            }

            //ServerListe laden
            if (context.ReadBoolean())
            {
                int n = (int)context.ReadInt16();
                for (int i = 0; i < n; ++i)
                {
                    int serverindex = context.ReadInt32();
                    EntityFlags flags = (EntityFlags)context.ReadByte();
                    context.Flags = flags;
                    if (ServerList[serverindex]!=null)
                    {
                        //Entity existiert schon
                        Entity e2 = ServerList[serverindex];
                        if (Root.Instance.Connection == null || e2.OwnerNumber != client)
                        {
                            //entity gehört nicht dem client->update
                            //HACK
                            short type = context.ReadInt16();
                            if (type != Root.Instance.Factory.GetClassId(e2.GetType().FullName))
                                throw new Exception("type of object cant change.");
                            e2.DeSerialize(context);
                        }
                        else
                        {
                            //entity gehört dem client->kein update, es sei denn override!
                            //HACK
                            //irgendwas is hier nicht korrekt

                            if ((flags & EntityFlags.Override) == EntityFlags.Override)
                            {

                                Console.WriteLine("overring update for " + e2.ToString());
                                //HACK
                                short type = context.ReadInt16();
                                if (type != Root.Instance.Factory.GetClassId(e2.GetType().FullName))
                                    throw new Exception("type of object cant change.");
                                e2.DeSerialize(context);
                            }
                            else
                            {
                                context.Flags |= EntityFlags.ServerNoOverride;
                                short type = context.ReadInt16();
                                if (type != Root.Instance.Factory.GetClassId(e2.GetType().FullName))
                                    throw new Exception("type of object cant change.");
                                e2.DeSerialize(context);

                            }
                        }
                        e2.Dirty = false;
                    }
                    else
                    {
                        //Entity existiert noch nicht
                        Entity e1 = (Entity)context.DeSerialize();
                        if (ClientList[e1.ClientIndex]!=null)
                        {
                            if (Root.Instance.Connection == null || e1.OwnerNumber != client)
                            {
                                //entity gehört nicht diesem client
                                //Console.WriteLine("creating new obj.(2)");
                                if (Root.Instance.Connection != null && IsOnBlackList(serverindex))
                                    Console.WriteLine("blacklist!(2)");
                                else
                                {
                                    ServerListAdd(serverindex, e1);
                                    //if (e1 is Node)
                                    //    ((Node)e1).OnAdd(this);
                                    e1.Dirty = false;
                                }
                            }
                            else
                            {
                                //entity von clientliste nach serverliste kopieren
                                Entity old = ClientList[e1.ClientIndex];
                                ClientListRemove(e1.ClientIndex);
                                //Console.WriteLine("clientlist->serverlist.");
                                ServerListAdd(serverindex, old);
                                old.ServerIndex = serverindex;
                                old.Dirty = false;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("creating new obj.");
                            if (Root.Instance.Connection != null && IsOnBlackList(serverindex))
                                Console.WriteLine("blacklist!");
                            else
                            {
                                ServerListAdd(serverindex, e1);
                                //if (e1 is Node)
                                //    ((Node)e1).OnAdd(this);
                                e1.Dirty = false;
                            }
                        }
                    }
                }
            }

            KillDirtyEntities();
        }

        public void ServerStateReceive(DeSerializationContext context, short client)
        {
            //BinaryReader r = context.Reader;
            //Stream s = context.Stream;

            foreach (KeyValuePair<int, Entity> de in ServerList)
            {
                Entity e = de.Value;
                if (e.OwnerNumber == client)
                    e.Dirty = true;
            }

            //ClientListe laden
            if (context.ReadBoolean())
            {
                int n = (int)context.ReadInt16();
                //if (n > 0)
                //    Console.WriteLine("clientlist from " + client.ToString() + " with " + n.ToString() + " entities.");
                for (int i = 0; i < n; ++i)
                {
                    int clientindex = context.ReadInt32();
                    EntityFlags flags = (EntityFlags)context.ReadByte();
                    Entity e = (Entity)context.DeSerialize();

                    Entity local = Find(client, clientindex);
                    if (local != null)
                    {
                        //Console.WriteLine("found local entity: " + client.ToString() + ", " + clientindex.ToString());
                        local.Dirty = false;
                    }
                    else
                    {
                        //Console.WriteLine("spawning new entity: " + e.ToString());
                        Spawn(e);
                        //Console.WriteLine("spawned entity: " + e.ToString());
                    }
                }
            }

            //ServerListe laden
            if (context.ReadBoolean())
            {
                int n = (int)context.ReadInt16();
                for (int i = 0; i < n; ++i)
                {
                    int serverindex = context.ReadInt32();
                    EntityFlags flags = (EntityFlags)context.ReadByte();
                    if (ServerList[serverindex]!=null)
                    {
                        //Entity existiert schon
                        Entity e2 = ServerList[serverindex];
                        if (e2.OwnerNumber == client)
                        {
                            //entity gehört dem client->update
                            //HACK
                            short type = context.ReadInt16();
                            if (type != Root.Instance.Factory.GetClassId(e2.GetType().FullName))
                                throw new Exception("type of object cant change.");
                            e2.DeSerialize(context);
                            e2.Dirty = false;
                            //debug += "updating serverindex " + serverindex + ", type: " + e2.GetType().ToString();
                        }
                        else
                        {
                            //entity gehört dem server->kein update
                            ISerializable drop = context.DeSerialize();
                            //debug += "dropping serverindex " + serverindex + ", type: " + drop.GetType().ToString();
                        }
                    }
                    else
                    {
                        //Entity existiert noch nicht
                        if (IsOnBlackList(serverindex))
                        {
                            Console.WriteLine("blacklist!(server)");
                            ISerializable drop = context.DeSerialize();
                            //debug += "blacklist serverindex " + serverindex +", type: " + drop.GetType().ToString();
                        }
                        else
                        {

                            Entity e1 = (Entity)context.DeSerialize();
                            ServerListAdd(serverindex, e1);
                            //debug += "creating serverindex " + serverindex + ", type: " + e1.GetType().ToString();
                            e1.Dirty = false;
                        }
                    }
                    //debug += "\n";
                }
            }

            KillDirtyEntities();
        }

		public void DeSerialize(DeSerializationContext context)
		{
		}

        public void Serialize(SerializationContext context)
        {
			if(ClientList!=null)
			{
                context.Write(true);
                int n = 0;
                //foreach (DictionaryEntry de in ClientList)
                    foreach (KeyValuePair<int, Entity> de in ClientList)
                    {
                    Entity e = de.Value;

                    if (!e.NoReplication)
                        ++n;
                }
                context.Write((short)n);
               // foreach(DictionaryEntry de in ClientList)
                foreach (KeyValuePair<int, Entity> de in ClientList)
                {
					int clientindex=de.Key;
					Entity e=de.Value;
                    if (e.NoReplication)
                        continue;

                    context.Write(clientindex);
                    context.Write((byte)EntityFlags.None);
                    context.Factory.Serialize(context, e);
                }
			}
			else
                context.Write(false);

            if(ServerList!=null)
			{
                context.Write(true);
				int n=0;
                //long countposition=context.Stream.Position;

                foreach (KeyValuePair<int, Entity> de in ServerList)
                {
					int serverindex=de.Key;
					Entity e=de.Value;

					if(!e.NoReplication&&Root.Instance.CategorizeSendEntity(serverindex,e.ClientIndex,e.OwnerNumber,
						Root.Instance.Scene.IsServer?(short)0:Root.Instance.Scene.ClientNumber))
					{
						n++;
					}
				}
                context.Write((short)n);

                foreach (KeyValuePair<int, Entity> de in ServerList)
                {
					int serverindex=de.Key;
					Entity e=de.Value;

                    //HACK
                    //if (e.Kill)//passiert manchmal...
                    //    throw new Exception("BUGBUG");

                    if (!e.NoReplication && Root.Instance.CategorizeSendEntity(serverindex, e.ClientIndex, e.OwnerNumber,
                        Root.Instance.Scene.IsServer ? (short)0 : Root.Instance.Scene.ClientNumber))
                    {

                        context.Write(serverindex);
                        if (e.Override&&IsServer)
                        {
                            Console.WriteLine("override! "+e.ToString());
                            e.Override = false;
                            context.Write((byte)EntityFlags.Override);
                        }
                        else
                            context.Write((byte)EntityFlags.None);


                        context.Serialize(e);

                        n++;

                    }
                }
                /*long pos = context.Stream.Position;
                context.Stream.Position = countposition;
                context.Write((short)n);
                context.Stream.Position = pos;*/

			}
			else
                context.Write(false);


        }

		protected int SetupLighting(IRenderer r,Node n,Light[] l)
		{
            int lights = lightcount;
			if(lightcount>1)
			{
				LightComparer lc=new LightComparer(n);
                l = Array.FindAll(l, 
                    delegate(Light l1)
                    {
                        return !(l1==null || (l1.Range > 0 && (n.AbsolutePosition - l1.AbsolutePosition).Length - n.RenderRadius > l1.Range));
                    });
                lights = l.Length;
                Array.Sort(l, 0, lights, lc);
			}
			else if(lightcount==0)
			{
				r.SetLighting(false);
				return 0;
			}

			int max=8;
			int i;
            if (lights < max) max = lights;

            //System.Console.WriteLine(max.ToString());

			for(i=0;i<max;++i)
			{
				r.SetLight(i,l[i]);
			}
			for(;i<8;++i)
			{
				r.SetLight(i,null);
			}
			r.SetLighting(true);

            n.CurrentNumberOfLights = max;

            return max;
		}

        public int lightcount;
        Light[] lightlist;
        NodeComparer nodecompare=new NodeComparer();
        Node[] nodes = new Node[1024];
		public void Draw(IRenderer r)
        {
            //lightcount = 0;

            IList<Light> ll = FindEntitiesByType<Light>();
            lightlist = new Light[ll.Count];
            ll.CopyTo(lightlist, 0);
            lightcount = ll.Count;

            IList<Node> nl = FindEntitiesByType<Node>();
            int nodecount=nl.Count;
            nl.CopyTo(nodes, 0);
            Array.Sort(nodes,0,nodecount,nodecompare);
            //nodes.Sort(nodecompare);
            //int nodecount=nodes.Count;

            //camera.Frustum = new Frustum();
            //camera.Frustum.GetFrustum(r);

            {
                r.SetCamera(camera);
                Frustum f = new Frustum();
                f.GetFrustum(r);
                if (Background != null)
                {
                    r.Clear(0.0f, 0.0f, 0.0f, 1);
                    Background.Draw(r,null);
                }
                else
                {
                    r.Clear(0.0f, 0.0f, 0.0f, 1);
                }

                VisibleNodes = 0;
                int i;
                Node n;
                for(i=0;i<nodecount;++i)
                {
                    n = (Node)nodes[i];
                    if ((n.Draw!=null&&n.Draw.Count>0&&n.Visible) && (n.RenderRadius < 0.0f || f.SphereInFrustum(n.AbsolutePosition, n.RenderRadius)))
                    {
                        VisibleNodes++;
                        Draw(r, n, lightlist);
                    }
                }
            }
            if(camera2!=null)
            {
                r.SetCamera(camera2);
                Frustum f = new Frustum();
                f.GetFrustum(r);
                if (Background != null)
                {
                    r.Clear(0.0f, 0.0f, 0.0f, 1);
                    Background.Draw(r,null);
                }
                else
                {
                    r.Clear(0.0f, 0.0f, 0.0f, 1);
                }
                int i;
                Node n;
                for(i=0;i<nodecount;++i)
                {
                    n = (Node)nodes[i];
                    if ((n.Draw != null && n.Draw.Count > 0 && n.Visible) && (n.RenderRadius < 0.0f || f.SphereInFrustum(n.AbsolutePosition, n.RenderRadius)))
                        Draw(r, n, lightlist);
                }
            }
            //r.SetCamera(camera);
        }

        Entity[] tick = new Entity[1024];
        protected void Tick(float dtime, EntityCollection list, int timer)
		{
            int t = 0;

            foreach (KeyValuePair<int, Entity> de in list)
            {
                tick[t++] = de.Value;
			}

            /*if(timer>=0)
                for (int j = 0; j < t; ++j)
                {
                    tick[j].Timer(timer);
                    tick[j].Tick(dtime);
                }
            else*/
                for (int j = 0; j < t; ++j)
                    tick[j].Tick(dtime);

		}

        //private OdeJointGroup ContactGroup=new OdeJointGroup(10);

        Node[] clist = new Node[1024];
        public void Tick(float dtime)
        {
            
            {
                int i = 0;

                foreach (KeyValuePair<int, Node> de in ServerCollideList)
                {
                            clist[i++] = de.Value;
                }
                if (ClientList != null)
                    foreach (KeyValuePair<int, Node> de in ClientCollideList)
                    {
                        clist[i++] = de.Value;
                    }

                for (int k = 0; k < i; ++k)
                {
                    Node o1 = clist[k];
                    CollisionInfo c1 = o1.GetCollisionInfo();
                    for (int j = k + 1; j < i; ++j)
                    {
                        Node o2 = clist[j];
                        if (o1.Kill || o2.Kill)
                            continue;

                        CollisionInfo c2 = o2.GetCollisionInfo();
                        if (!(o1.CanCollide(o2) && o2.CanCollide(o1)))
                            continue;
                        if (!c1.Check(c2))
                            continue;

                        o1.OnCollide(o2);
                        o2.OnCollide(o1);

                        if (Root.Instance.CurrentFlow != null)
                            Root.Instance.CurrentFlow.OnCollision(o1, o2);

                    }
                }
            }


            if (Root.Instance.UserInterface!=null)
            {
                NumSounds = 0;
                List<Node> kill = new List<Node>();
                foreach (KeyValuePair<Node, Channel> kv in Sounds)
                {
                    if (kv.Key.Kill)
                        kill.Add(kv.Key);
                    else
                    {
                        try
                        {
                            Root.Instance.UserInterface.Audio.SetPosition(kv.Value, kv.Key.AbsolutePosition);
                            NumSounds++;
                        }
                        catch (Exception)
                        {
                            kill.Add(kv.Key);
                        }
                    }
                }

                foreach (Node n in kill)
                    Sounds.Remove(n);

                if (camera != null)
                {
                    Vector3 up = Vector3.Cross(camera.Left, camera.Direction);
                    Root.Instance.UserInterface.Audio.SetListener(camera.Position, camera.Direction, up);
                }
            }

            int t = (int)Root.Instance.Time;
            int t2 = -1;
            if (t - LastTimer > 0)
            {
                LastTimer = t;
                t2 = t;
            }

            if (Physics != null)
                Physics.Tick(dtime);

            Tick(dtime, ServerList, t2);
            if (ClientList!=null)
                Tick(dtime, ClientList, t2);

            RemoveQueuedEntities();

            /*foreach (KeyValuePair<int, Entity> de in ServerList)
            {
                if (de.Value.Kill)
                    Console.WriteLine("adfdfjn");
            }
            if (ClientList != null)
                foreach (KeyValuePair<int, Entity> de in ClientList)
                {
                    if (de.Value.Kill)
                        Console.WriteLine("adfdfjn");
                }*/
        }

        public int NumSounds = 0;

        public short ClientNumber
		{
			get
			{
				if(IsServer)
					return 0;
                if (Root.Instance.Connection != null && Root.Instance.Connection is UdpClient)
					return ((UdpClient)Root.Instance.Connection).ClientNumber;
				return 0;
			}
		}

        protected Dictionary<Type, Dictionary<int, Entity>> TypeList = new Dictionary<Type, Dictionary<int, Entity>>();
        protected Dictionary<int, Node> ClientCollideList = new Dictionary<int, Node>();
        protected Dictionary<int, Node> ServerCollideList = new Dictionary<int, Node>();

        //protected Dictionary<Type, Dictionary<int, Entity>> ClientTypeList;

        //protected Dictionary<int, Entity> ServerList = new Dictionary<int, Entity>();
        //protected Dictionary<int, Entity> ClientList;
        protected EntityCollection ServerList = new EntityCollection();
        protected EntityCollection ClientList;

        public Dictionary<Node,Channel> Sounds = new Dictionary<Node,Channel>();
		public Camera camera;
        public Camera camera2;
		public IDrawable Background;
		protected Marker marker;
        protected Queue<int> BlackList = new Queue<int>();
        public int VisibleNodes;
        int LastTimer=0;

        public Physics.IPhysicsWorld Physics;

        public void KillEntitiesOfClient(short clientid)
        {
            foreach (KeyValuePair<int, Entity> de in ServerList)
            {
                Entity e = de.Value;
                if (e.OwnerNumber==clientid)
                {
                    e.Kill = true;
                }
            }
        }
    }

    public class Viewport
    {
        public Viewport(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public int X
        {
            get
            {
                return viewport[0];
            }
            set
            {
                viewport[0] = value;
            }
        }

        public int Y
        {
            get
            {
                return viewport[1];
            }
            set
            {
                viewport[1] = value;
            }
        }

        public int W
        {
            get
            {
                return viewport[2];
            }
            set
            {
                viewport[2] = value;
            }
        }

        public int H
        {
            get
            {
                return viewport[3];
            }
            set
            {
                viewport[3] = value;
            }
        }

        public int[] Values
        {
            get
            {
                return viewport;
            }
            set
            {
                viewport = value;
            }
        }

        int[] viewport=new int[4];
    }

	public class Camera : Node, IResource
	{
		public Camera()
		{
			//position.X=position.Y=position.Z=100;
			//LookAt(0,0,0);
		}


        public virtual float Fov
        {
            get
            {
                return _Fov;
            }
            set
            {
                _Fov = value;
            }
        }

        Matrix4 gluPerspective(float fovy, float aspect, float zNear, float zFar)
        {
            Matrix4 m = Matrix4.Identity;
            float sine, cotangent, deltaZ;
            float radians = fovy / 2 * (float)Math.PI / 180;

            deltaZ = zFar - zNear;
            sine = (float)Math.Sin(radians);
            if ((deltaZ == 0) || (sine == 0) || (aspect == 0))
            {
                throw new Exception();
            }
            cotangent = (float)Math.Cos(radians) / sine;

            //m[0] = cotangent / aspect;
            //m[5] = cotangent;
            //m[10] = -(zFar + zNear) / deltaZ;
            //m[11] = -1;//HACK?!
            //m[14] = -2 * zNear * zFar / deltaZ;
            //m[15] = 0;

            m.Row0.X = cotangent / aspect;
            m.Row1.Y = cotangent;
            m.Row2.Z = -(zFar + zNear) / deltaZ;
            m.Row2.W = -1;//HACK?!
            m.Row3.Z = -2 * zNear * zFar / deltaZ;
            m.Row3.W = 0;



            return m;
        }

        Matrix4 
gluLookAt(float eyex, float eyey, float eyez, float centerx,
	  float centery, float centerz, float upx, float upy,
	  float upz)
{
    Vector3 forward = Vector3.Zero, side = Vector3.Zero, up = Vector3.Zero;
    Matrix4 m=Matrix4.Identity;

    forward.X = centerx - eyex;
    forward.Y = centery - eyey;
    forward.Z = centerz - eyez;

    up.X = upx;
    up.Y = upy;
    up.Z = upz;

    forward.Normalize();

    /* Side = forward x up */
    //cross(forward, up, side);
    side = Vector3.Cross(forward, up);
    side.Normalize();

    /* Recompute up as: up = side x forward */
    //cross(side, forward, up);
    up = Vector3.Cross(side, forward);

    //__gluMakeIdentityf(&m[0][0]);
    m.Row0.X = side.X;
    m.Row0.Y = side.Y;
    m.Row0.Z = side.Z;

    m.Row1.X = up.X;
    m.Row1.Y = up.Y;
    m.Row1.Z = up.Z;

    m.Row2.X = -forward.X;
    m.Row2.Y = -forward.Y;
    m.Row2.Z = -forward.Z;

    //m = Matrix4Extensions.FromBasis(side, up, -forward);

    m = m * Matrix4Extensions.FromTranslation(-eyex, -eyey, -eyez);
    //m = Matrix4Extensions.FromTranslation(-eyex, -eyey, -eyez)*m;
    //m.Translate(-eyex, -eyey, -eyez);
    //glMultMatrixf(&m[0][0]);
    //glTranslated(-eyex, -eyey, -eyez);
    return m;
}

        public Matrix4 GetProjectionMatrix()
        {
            return gluPerspective(Fov, Aspect, nearplane, farplane);
        }
        public Matrix4 GetViewMatrix()
        {
            Matrix4 m = Matrix;
            Vector3 t = new Vector3();
            Vector3 x, y;
            Vector3 pos = Matrix4Extensions.ExtractTranslation(m);
            Matrix4Extensions.ExtractBasis(m, out x, out y, out t);
            t = -t;
            t += pos;


            m=gluLookAt(pos.X, pos.Y, pos.Z, t.X, t.Y, t.Z, y.X, y.Y, y.Z);

            return m;
        }

        public Vector3 gluUnProject(float winx, float winy, float winz,
	     float[] model, float[] proj,
	     int[] viewport)
{
   /* matrice de transformation */
   Matrix4 m;//, A;
   Vector4 inv, outv;

   /* transformation coordonnees normalisees entre -1 et 1 */
   inv.X = (winx - (float)viewport[0]) * 2.0f / (float)viewport[2] - 1.0f;
   inv.Y = (winy - (float)viewport[1]) * 2.0f / (float)viewport[3] - 1.0f;
   inv.Z = 2.0f * winz - 1.0f;
   inv.W = 1.0f;

   /* calcul transformation inverse */
   //matmul(A, proj, model);
   m = (Matrix4Extensions.FromFloats(proj) * Matrix4Extensions.FromFloats(model));
   m.Invert();
   //m.Transpose();
   //A.Invert();
            //m = A;
   //invert_matrix(A, m);

   /* d'ou les coordonnees objets */

   //transform_point(out, m, in);
            outv = Vector4.Transform(inv,m);
   //if (outv.W == 0.0)
   //   throw new Exception();
   //*objx = out[0] / out[3];
   //*objy = out[1] / out[3];
   //*objz = out[2] / out[3];
   //return 1;
  return new Vector3(outv.X / outv.W, outv.Y / outv.W, outv.Z / outv.W);
  //return new Vector3(outv.X, outv.Y , outv.Z );
}
        protected float _Fov=60.0f;
		public float nearplane=1.0f;
		public float farplane=30000.0f;
		public Frustum Frustum;
        public Viewport View=null;
        public float Shake=0;
        public float Aspect = 4.0f/3.0f;
        /*public float Shake
        {
            get
            {
                return (float)Math.Cos(Root.Instance.Time * 0.1f)*0.02f;
            }
        }*/

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (Shake >= 0)
            {
                Shake -= dtime*0.01f;
                if (Shake < 0)
                    Shake = 0;
            }
        }

    }

    public enum CameraMode
    {
        Chase, FlyBy, Normal
    }

    public class ImageSaver : ISaver<Image>
    {
        #region ISaver<Image> Members

        public void Save(Image obj, Stream s)
        {
            int stride=(obj.Width*3);
            if(stride%4!=0)
                throw new Exception();

            IntPtr p=Marshal.UnsafeAddrOfPinnedArrayElement(obj.Data,0);

            Bitmap bmp = new Bitmap(obj.Width, obj.Height,stride,System.Drawing.Imaging.PixelFormat.Format24bppRgb,p);
            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
            bmp.Dispose();
        }

        #endregion
    }

    public class TgaImageSaver : ISaver<Image>
    {
        #region ISaver<Image> Members

        public void Save(Image obj, Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            s.WriteByte(0);
            s.WriteByte(0);
            s.WriteByte(2);//typ
            s.WriteByte(0);//palettenbegin
            s.WriteByte(0);
            s.WriteByte(0);//palettenlaenge
            s.WriteByte(0);
            s.WriteByte(0);//groesse
            s.WriteByte(0);//x
            s.WriteByte(0);
            s.WriteByte(0);//y
            s.WriteByte(0);

            w.Write((short)obj.Width);
            w.Write((short)obj.Height);
            w.Flush();
            s.WriteByte(24);
            s.WriteByte(0);

            //System.Drawing.Imaging.BitmapData data=obj.LockBits(null, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //obj.
            byte[] copy = new byte[obj.Data.Length];
            for (int i = 0; i < obj.Data.Length; ++i)
            {
                if (i % 3 == 0)
                    copy[i] = obj.Data[i + 2];
                else if (i % 3 == 2)
                    copy[i] = obj.Data[i - 2];
                else
                    copy[i] = obj.Data[i];
            }
            s.Write(copy, 0, copy.Length);
            /*for(int y=0;y<obj.Height;++y)
                for (int x = 0; x < obj.Width; ++x)
                {
                    Color c=obj.GetPixel(x, y);
                    s.WriteByte(c.B);
                    s.WriteByte(c.G);
                    s.WriteByte(c.R);
                }*/
            s.Flush();
        }

        #endregion
    }
    public class AdvancedCamera : Camera
    {

        public AdvancedCamera(CameraMode m)
        {
            Mode = m;
        }
        public AdvancedCamera(CameraMode m,Node target,float distance)
        {
            Attach = target;
            Dist = distance;
            Mode = m;
        }

        public CameraMode Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                _Mode = value;
                Reset();
            }
        }

        public override Matrix4 Matrix
        {
            get
            {
                if (Mode!=CameraMode.FlyBy)
                {
                    /*Matrix4 m1 = Matrix4Extensions.FromQuaternion(SmoothOrientation.Smoothed);

                    m1[12] = SmoothPosition.Smoothed.X;
                    m1[13] = SmoothPosition.Smoothed.Y;
                    m1[14] = SmoothPosition.Smoothed.Z;

                    Matrix4 m2 = Matrix4Extensions.FromQuaternion(Orientation);

                    m2[12] = Position.X;
                    m2[13] = Position.Y;
                    m2[14] = Position.Z;

                    return m1 * m2;*/
                }

                if (Attach == null)
                {
          
                    Matrix4 m = Matrix4Extensions.FromQuaternion(Orientation);

                    m.Row3.X = Position.X;
                    m.Row3.Y = Position.Y;
                    m.Row3.Z = Position.Z;

                    return m;
                }
                else
                {
                    Matrix4 m;
                    m= Matrix4Extensions.FromQuaternion(Orientation);

                    m.Row3.X = Position.X;
                    m.Row3.Y = Position.Y;
                    m.Row3.Z = Position.Z;

                    switch(_Mode)
                    {
                        case CameraMode.Normal:
                            return Attach.Matrix * m;
                        default:
                            return m;
                    }
               }
            }
        }

        public override float Fov
        {
            get
            {
                return _Fov;
                if (Attach == null)
                    return _Fov;
                float d = (Attach.AbsolutePosition - AbsolutePosition).Length;
                if (d > Dist * 0.75f)
                {
                    return _Fov / MaxZoom;
                }
                else
                    return _Fov;
            }
            set
            {
                _Fov = value;
            }
        }


        public override void Tick(float dTime)
        {
            if (Attach!=null)
            {
                /*SmoothPosition.Original = Attach.Position;
                SmoothOrientation.Original = Attach.Orientation;
                SmoothPosition.Tick(dTime, 0.5f);
                SmoothOrientation.Tick(dTime, 0.5f);*/
            }
            base.Tick(dTime);

            if (Attach != null)
            {
                switch (Mode)
                {
                    case CameraMode.FlyBy:
                        float d = (Attach.AbsolutePosition - Position).Length;

                        if (d > Dist * 1.1f)
                            Reset();
                        else
                        {
                            LookAt(Attach.SmoothAbsolutePosition);
                        }
                        break;
                    case CameraMode.Chase:
                        LookAt(Matrix4Extensions.ExtractTranslation(Attach.SmoothMatrix));
                        break;
                    default:
                        break;
                }
            }
        }

        public void Reset()
        {
            switch (Mode)
            {
                case CameraMode.FlyBy:
                    //Console.WriteLine("cam reset");
                    if (Attach!=null)
                    {
                        Vector3 v = Attach.Speed;
                        if (v.LengthSquared<0.00001f)
                            v = Vector3.Transform(Vector3.UnitZ,Attach.Matrix);
                        v.Normalize();
                        v *= Dist;
                        Position = Attach.AbsolutePosition + v;
                        position.Original.Y = 100;
                        LookAt(Attach.SmoothAbsolutePosition);
                    }
                    break;
                default:
                    break;
            }
        }

        protected CameraMode _Mode;
        public float Dist;
        public float MaxZoom = 1;
        //public bool Smooth = true;
        //protected SmoothVector3 SmoothPosition;
        //protected SmoothQuaternion SmoothOrientation;
    }

    public class ImageResource : Image, IResource
	{
		public ImageResource(int w,int h,byte[] _data,bool _alpha) :  base(w,h,_data,_alpha)
		{
		}

		public void Dispose()
		{
		}
	}

	public class Image
	{
		public Image(int w,int h,byte[] _data,bool _alpha)
		{
			size.X=w;
			size.Y=h;
			data=_data;
			alpha=_alpha;
		}

		public int Width
		{
			get{return size.X;}
		}
		
		public int Height
		{
			get{return size.Y;}
		}

		public byte[] Data
		{
			get{return data;}
		}

		public bool Alpha
		{
			get{return alpha;}
		}

		public int BytesPerPixel
		{
			get{return alpha?4:3;}
		}
		
		public int BitsPerPixel
		{
			get{return BytesPerPixel*8;}
		}

        public System.Drawing.Color GetPixel(int x, int y)
        {
            int pos = y * Width * BytesPerPixel + x * BytesPerPixel;
            return System.Drawing.Color.FromArgb(data[pos], data[pos+1], data[pos+2]);
        }
		protected Point size;
		protected byte[] data;
		protected bool alpha;
	}

	public interface IImageDecoder
	{
		//void Load(string filename);
		void Load(Stream stream);
		byte[] getRGBA();
		int getWidth();
		int getHeight();
		bool hasAlpha();
	}

    public class Particle
    {
        public Particle(Vector3 p, Vector3 s)
        {
            Position = p;
            Speed = s;
        }
        public Particle()
        {
        }

        public virtual void Tick(float dtime)
        {
            Position += Speed * dtime;
            Age += dtime;
        }

        public Vector3 Position;
        public Vector3 Speed;
        public Color4f Color=new Color4f(1, 1, 1, 1);
        public float Age=0;
    }

    public class ParticleSystem : IDrawable, ITickable
    {
        public virtual bool IsWorldSpace
        {
            get { return true; }
        }
		public ParticleSystem(int max,Texture t)
		{
			Max=max;
			Texture=t;
			Particles=new Particle[max];
			Count=0;
            Next = 0;

            if (Root.Instance.UserInterface != null)
            {
                vb = Root.Instance.UserInterface.Renderer.CreateDynamicVertexBuffer(Max * (3+4) * 4);

                VertexFormat format = VertexFormat.VF_P3C4;
                vb.Format = format;
            }
        }

        public virtual Particle NewParticle()
        {
            return new Particle();
        }

        public virtual void Tick(float dtime, Particle p)
        {
            p.Tick(dtime);
        }

        public virtual void Tick(float dtime)
		{
			foreach(Particle p in Particles)
			{
				if(p!=null)
				{
                    Tick(dtime, p);
                }
			}
		}

		protected unsafe void FillBuffer()
		{
            if (vb != null)
            {
                float* ptr = (float*)vb.Lock();
                foreach (Particle p in Particles)
                {
                    if (p != null)
                    {
                        *ptr++ = p.Position.X;
                        *ptr++ = p.Position.Y;
                        *ptr++ = p.Position.Z;
                        *ptr++ = p.Color.r;
                        *ptr++ = p.Color.g;
                        *ptr++ = p.Color.b;
                        *ptr++ = p.Color.a;
                    }
                }
                vb.Unlock();
            }
		}

		public virtual void Spawn(Particle p)
		{
			/*if(Count>=Max)
				throw new Exception("too many particles.");
			for(int i=0;i<Max;++i)
			{
				if(Particles[i]==null)
				{
					Particles[i]=p;
					Count++;
					return;
				}
			}
			throw new Exception("particle error.");*/
            Particles[Next] = p;
            Count++;
            if (Count > Particles.Length)
                Count = Particles.Length;
            Next = (Next + 1) % Particles.Length;
        }
/*
		public void CreateRandomParticles(int count)
		{
			for(int i=0;i<count;++i)
			{
				Spawn(0,0,0,(float)r.NextDouble()*20-10,(float)r.NextDouble()*20,(float)r.NextDouble()*20-10);
			}
		}
*/
        public void Draw(IRenderer r, Node n)
		{
            r.SetPointSize(PointSize);
            r.SetMode(RenderMode.Draw3DPointSprite);
            r.SetMaterial(Material);
			r.BindTexture(Texture.Id,0);
			//r.BindTexture(null,1);
            if(Dynamic)
			    FillBuffer();
			r.Draw(vb,PrimitiveType.POINTS,0,Count,null);
		}

		public Particle[] Particles;
		protected DynamicVertexBuffer vb;
		public int Max;
        public int Next;
        public int Count;
        public float PointSize=200;
		public Texture Texture;
        public Material Material = new Material();
		Random r=new Random();
        public bool Dynamic=true;
	}

    public class Text3D : IDrawable
    {
        public Text3D(string text, MeshFont font)
        {
            Text = text;
            Font = font;
        }

        #region IDrawable Members

        public void Draw(IRenderer r, Node n)
        {
            Font.Draw(r, Text, 0, 0, new Color4f(1,1,1),false,8,16);
        }

        public bool IsWorldSpace
        {
            get { return false; }
        }

        #endregion

        MeshFont Font;
        string Text;
    }

    public class MeshFont : IResource
    {
        public MeshFont(Mesh[] chars)
        {
            Chars = chars;
        }
        /*
        public void Draw(IRenderer r, string text, float x, float y)
        {
            Draw(r, text, x, y, color);
        }*/
        public void Draw(IRenderer r, string text, float x, float y, Color4f color, bool center, float width, float size)
        {
            r.PushMatrix();
            r.MultMatrix(Matrix4Extensions.FromTranslation(x-width*((float)text.Length)/2.0f, y, 0));
            r.MultMatrix(Matrix4Extensions.FromScale(size, -size, size));
            for (int i = 0; i < text.Length; ++i)
            {
                char c=text[i];
                int index = (int)c;
                if (index < Chars.Length)
                {
                    Mesh m = Chars[index];

                    if(m!=null)
                        m.Draw(r, null);
                }
                r.MultMatrix(Matrix4Extensions.FromTranslation(width / size, 0, 0));
            }
            r.PopMatrix();
        }
        /*
        public void Draw(IRenderer r, string text, float x, float y, Color4f color, RectangleF scissor)
        {
            Draw(r, text, x, y, color);
        }*/

        Mesh[] Chars;
        public Color4f color = new Color4f(1, 1, 1, 1);
        public float size = 16;
        public float Width = 8;

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

	public class Font : IResource
	{
		public Font(Texture t)
		{
			texture=t;
		}

		public Font(string image)
		{
			texture=Root.Instance.ResourceManager.LoadTexture(image);
		}

		public void Draw(IRenderer r,string text,float x,float y)
		{
			float resx=r.Size.X;
			float resy=r.Size.Y;
			//r.Draw(text,x/resx,y/resy,size/resx,size/resy,texture);
            r.Draw(text, (int)x / resx, (int)y / resy, size / resx, size / resy, texture,color,Width/resx);
        }
        public void Draw(IRenderer r, string text, float x, float y, Color4f color)
        {
            float resx = r.Size.X;
            float resy = r.Size.Y;
            //r.Draw(text,x/resx,y/resy,size/resx,size/resy,texture);
            r.Draw(text, (int)x / resx, (int)y / resy, size / resx, size / resy, texture, color, Width / resx);
        }

        public void Draw(IRenderer r, string text, float x, float y, Color4f color,RectangleF scissor)
        {
            float resx = r.Size.X;
            float resy = r.Size.Y;
            scissor = new RectangleF(scissor.Left / resx, scissor.Top / resy, scissor.Width / resx, scissor.Height / resy);
            r.Draw(text, (int)x / resx, (int)y / resy, size / resx, size / resy, texture, color, Width / resx,scissor);
        }

        public Texture texture;
		public Color4f color=new Color4f(1,1,1,1);
		public float size = 16;
        public float Width = 8;

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

    public class SkyBoxLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            StreamReader r = new StreamReader(n.getStream());
            string line;

            string header = r.ReadLine().Trim();
            if (header != "SKYBOXTEXT")
                return null;

            Texture left = null;
            Texture right = null;
            Texture top = null;
            Texture bottom = null;
            Texture front = null;
            Texture back = null;
            int tiles = 1;

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                string[] split = line.Split(new char[] { ':' });
                string key = split[0].Trim();
                string val = split[1].Trim();

                switch (key)
                {
                    case "left":
                        left = Root.Instance.ResourceManager.LoadTexture(val);
                        break;
                    case "right":
                        right = Root.Instance.ResourceManager.LoadTexture(val);
                        break;
                    case "top":
                        top = Root.Instance.ResourceManager.LoadTexture(val);
                        break;
                    case "bottom":
                        bottom = Root.Instance.ResourceManager.LoadTexture(val);
                        break;
                    case "front":
                        front = Root.Instance.ResourceManager.LoadTexture(val);
                        break;
                    case "back":
                        back = Root.Instance.ResourceManager.LoadTexture(val);
                        break;

                    case "tiles":
                        tiles = int.Parse(val);
                        break;
                }


            }

            SkyBox sb = new SkyBox(left, right, top, bottom, front, back, tiles);

            return sb;
        }

        public Type LoadType
        {
            get { return typeof(SkyBox); }
        }


        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Extension.ToLower() == ".skybox";
        }

    }

    /*public class SkyCubeMap : IDrawable
    {
        public SkyCubeMap()
        {
        }
    }*/

    public class SkyBox : IDrawable,ICloneable,IResource
	{
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public SkyBox(float tiles)
		{
			Tiles=tiles;
			CreateVertexBuffer();
			Material=Cheetah.Graphics.Material.CreateSimpleMaterial(null);
            Shader = Root.Instance.ResourceManager.LoadShader("emissivemap.shader");
        }

        public SkyBox(Texture left,Texture right,Texture top,Texture bottom,Texture front,Texture back,float tiles)
        {
            Tiles = tiles;
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            Front = front;
            Back = back;
            CreateVertexBuffer();
            Material = Cheetah.Graphics.Material.CreateSimpleMaterial(null);
            Shader = Root.Instance.ResourceManager.LoadShader("emissivemap.shader");
        }
        
        protected void CreateVertexBuffer()
		{
			VertexFormat format=new VertexFormat(new VertexFormat.Element[]{
																			   new VertexFormat.Element(VertexFormat.ElementName.Position,3),
																			   new VertexFormat.Element(VertexFormat.ElementName.Texture0,2)
																		   });
			byte[] data=new byte[format.Size*4*6];
			MemoryStream ms=new MemoryStream(data);
			BinaryWriter bw=new BinaryWriter(ms);

			float s=10;
			float d=0;//0.1f;
			//float h=0.999f;
			//float l=0.001f;
			float h=Tiles;
			float l=0.0f;

			//front
			bw.Write(-s);bw.Write(s);bw.Write(s-d);
			bw.Write(l);bw.Write(l);
			
			bw.Write(s);bw.Write(s);bw.Write(s-d);
			bw.Write(h);bw.Write(l);


			bw.Write(s);bw.Write(-s);bw.Write(s-d);
			bw.Write(h);bw.Write(h);

			bw.Write(-s);bw.Write(-s);bw.Write(s-d);
			bw.Write(l);bw.Write(h);

			//back
			bw.Write(-s);bw.Write(s);bw.Write(-s+d);
			bw.Write(h);bw.Write(l);

			bw.Write(-s);bw.Write(-s);bw.Write(-s+d);
			bw.Write(h);bw.Write(h);

			bw.Write(s);bw.Write(-s);bw.Write(-s+d);
			bw.Write(l);bw.Write(h);

			bw.Write(s);bw.Write(s);bw.Write(-s+d);
			bw.Write(l);bw.Write(l);

			//left
			bw.Write(-s+d);bw.Write(-s);bw.Write(s);
			bw.Write(h);bw.Write(h);


			bw.Write(-s+d);bw.Write(-s);bw.Write(-s);
			bw.Write(l);bw.Write(h);

			bw.Write(-s+d);bw.Write(s);bw.Write(-s);
			bw.Write(0.0f);bw.Write(l);

			bw.Write(-s+d);bw.Write(s);bw.Write(s);
			bw.Write(h);bw.Write(l);
		
			//right
			bw.Write(s-d);bw.Write(s);bw.Write(s);
			bw.Write(l);bw.Write(l);

			bw.Write(s-d);bw.Write(s);bw.Write(-s);
			bw.Write(h);bw.Write(l);

			bw.Write(s-d);bw.Write(-s);bw.Write(-s);
			bw.Write(h);bw.Write(h);

			bw.Write(s-d);bw.Write(-s);bw.Write(s);
			bw.Write(l);bw.Write(h);

			//top
			bw.Write(s);bw.Write(s);bw.Write(-s);
			bw.Write(h);bw.Write(0.0f);

			bw.Write(s);bw.Write(s);bw.Write(s);
			bw.Write(h);bw.Write(h);

			bw.Write(-s);bw.Write(s);bw.Write(s);
			bw.Write(l);bw.Write(h);

			bw.Write(-s);bw.Write(s);bw.Write(-s);
			bw.Write(l);bw.Write(l);

			//bottom
			bw.Write(-s);bw.Write(-s);bw.Write(-s);
			//bw.Write(l);bw.Write(h);
            bw.Write(-h); bw.Write(l);
	

			bw.Write(-s);bw.Write(-s);bw.Write(s);
			//bw.Write(l);bw.Write(l);
            bw.Write(l); bw.Write(l);

			bw.Write(s);bw.Write(-s);bw.Write(s);
			//bw.Write(h);bw.Write(l);
            bw.Write(l); bw.Write(h);
			
			bw.Write(s);bw.Write(-s);bw.Write(-s);
			//bw.Write(h);bw.Write(h);
            bw.Write(-h); bw.Write(h);

			vb=Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data,data.Length);
			vb.Format=format;
		}

        public void Draw(IRenderer r, Node n)
		{
			r.SetMode(RenderMode.DrawSkyBox);

			float[] m=new float[16];
			r.GetMatrix(m,null);
			r.PushMatrix();
			m[12]=m[13]=m[14]=0;
			r.LoadMatrix(Matrix4Extensions.FromFloats(m));

            r.UseShader(Shader);

            Material.DepthWrite = false;


			Material.EmissiveMap=Front;
            Material.Apply(Shader, r);
            r.SetMaterial(Material);
			//r.BindTexture(Front.Id,0);
			r.Draw(vb,PrimitiveType.QUADS,0,4,null);

            Material.EmissiveMap = Back;
            Material.Apply(Shader, r);
            r.SetMaterial(Material);
			//r.BindTexture(Back.Id,0);
			r.Draw(vb,PrimitiveType.QUADS,4,4,null);

            Material.EmissiveMap = Left;
            Material.Apply(Shader, r);
            r.SetMaterial(Material);
			//r.BindTexture(Left.Id,0);
			r.Draw(vb,PrimitiveType.QUADS,8,4,null);

            Material.EmissiveMap = Right;
            Material.Apply(Shader, r);
            r.SetMaterial(Material);
			//r.BindTexture(Right.Id,0);
			r.Draw(vb,PrimitiveType.QUADS,12,4,null);

            Material.EmissiveMap = Top;
            Material.Apply(Shader, r);
            r.SetMaterial(Material);
			//r.BindTexture(Top.Id,0);
			r.Draw(vb,PrimitiveType.QUADS,16,4,null);

            Material.EmissiveMap = Bottom;
            Material.Apply(Shader, r);
            r.SetMaterial(Material);
			//r.BindTexture(Bottom.Id,0);
			r.Draw(vb,PrimitiveType.QUADS,20,4,null);

			r.PopMatrix();
		}

		public Texture Left;
		public Texture Right;
		public Texture Top;
		public Texture Bottom;
		public Texture Front;
		public Texture Back;
		protected VertexBuffer vb;
		protected IndexBuffer ib;
		public float Tiles=1;
		public Material Material;
        public Shader Shader;

        #region ICloneable Members

        public object Clone()
        {
            return new SkyBox(Left, Right, Top, Bottom, Front, Back, Tiles);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

#endregion
    }
    /*
    public class DevIlImageDecoder : IImageDecoder
	{
		static DevIlImageDecoder()
		{
			Tao.DevIl.Il.ilInit();
		}

		public unsafe void Load(Stream stream)
		{
			byte[] buffer=new byte[stream.Length];
			int img;
			stream.Read(buffer,0,(int)stream.Length);
			Tao.DevIl.Il.ilGenImages(1,out img);
			Tao.DevIl.Il.ilBindImage(img);
			if(!Tao.DevIl.Il.ilLoadL(Tao.DevIl.Il.IL_TYPE_UNKNOWN,buffer,buffer.Length))
				throw new Exception("DevIL cant load image.");
			
			width=Tao.DevIl.Il.ilGetInteger(Tao.DevIl.Il.IL_IMAGE_WIDTH);
			height=Tao.DevIl.Il.ilGetInteger(Tao.DevIl.Il.IL_IMAGE_HEIGHT);
			//int format=Tao.DevIl.Il.ilGetInteger(Tao.DevIl.Il.IL_IMAGE_FORMAT);
			int bytesperpixel=Tao.DevIl.Il.ilGetInteger(Tao.DevIl.Il.IL_IMAGE_BPP);

			alpha=bytesperpixel>3;

			if(alpha)
			{
				if(!Tao.DevIl.Il.ilConvertImage(Tao.DevIl.Il.IL_RGBA,Tao.DevIl.Il.IL_BYTE))
					throw new Exception("DevIL cant convert image.");
			}
			else
			{
				if(!Tao.DevIl.Il.ilConvertImage(Tao.DevIl.Il.IL_RGB,Tao.DevIl.Il.IL_BYTE))
					throw new Exception("DevIL cant convert image.");
			}

			byte *ptr=(byte*)Tao.DevIl.Il.ilGetData();

			if(!alpha)
			{
				data=new byte[width*height*3];
				for(int y=0;y<height;++y)
				{
					byte *line=ptr+(width*3)*y;
					for(int x=0;x<width;++x)
					{
						byte *pixel=line+3*x;
						data[y*width*3+x*3]=pixel[0];
						data[y*width*3+x*3+1]=pixel[1];
						data[y*width*3+x*3+2]=pixel[2];
					}
				}
			}
			else
			{
				data=new byte[width*height*4];
				for(int y=0;y<height;++y)
				{
					byte *line=ptr+(width*4)*y;
					for(int x=0;x<width;++x)
					{
						byte *pixel=line+4*x;
						data[y*width*4+x*4]=pixel[0];
						data[y*width*4+x*4+1]=pixel[1];
						data[y*width*4+x*4+2]=pixel[2];
						data[y*width*4+x*4+3]=pixel[3];
					}
				}
			}
			Tao.DevIl.Il.ilDeleteImages(1,new int[]{img});
		}

		public byte[] getRGBA()
		{
			return data;
		}

		public int getWidth()
		{
			return width;
		}

		public int getHeight()
		{
			return height;
		}

		public bool hasAlpha()
		{
			return alpha;
		}

		//protected SDLImage image;
		protected byte[] data;
		protected int width;
		protected int height;
		protected bool alpha;
	}

	public class SDLImageDecoder : IImageDecoder
	{
		public unsafe void Load(Stream stream)
		{
			byte[] buffer=new byte[stream.Length];
			stream.Read(buffer,0,(int)stream.Length);
			image=new SDLImage(buffer);
			//Sdl.SDL_Load
			if(image==null)
				throw new Exception("cant create SDLImage.");
			
			width=image.Size.Width;
			height=image.Size.Height;

			byte *ptr=(byte*)image.Surface.Pixels;

			switch(image.Surface.BytesPerPixel)
			{
				case 3:
					data=new byte[width*height*3];
					alpha=false;
					for(int y=0;y<height;++y)
					{
						byte *line=ptr+image.Surface.Pitch*y;
						for(int x=0;x<width;++x)
						{
							byte *pixel=line+3*x;
							data[y*width*3+x*3]=pixel[0];
							data[y*width*3+x*3+1]=pixel[1];
							data[y*width*3+x*3+2]=pixel[2];
						}
					}
					break;
				case 4:
					data=new byte[width*height*4];
					alpha=true;
					for(int y=0;y<height;++y)
					{
						byte *line=ptr+image.Surface.Pitch*y;
						for(int x=0;x<width;++x)
						{
							byte *pixel=line+4*x;
							data[y*width*4+x*4]=pixel[0];
							data[y*width*4+x*4+1]=pixel[1];
							data[y*width*4+x*4+2]=pixel[2];
							data[y*width*4+x*4+3]=pixel[3];
						}
					}
					break;
				default:
					throw new Exception("image type not supported.");
			}
		}

		public byte[] getRGBA()
		{
			return data;
		}

		public int getWidth()
		{
			return width;
		}

		public int getHeight()
		{
			return height;
		}

		public bool hasAlpha()
		{
			return alpha;
		}

		protected SDLImage image;
		protected byte[] data;
		protected int width;
		protected int height;
		protected bool alpha;
	}
*/
	public class DotNetImageDecoder : IImageDecoder
	{
		public void Load(Stream stream)
		{
			Bitmap bm=new Bitmap(stream);
			
			width=bm.Width;
			height=bm.Height;
			data=new byte[width*height*3];

			for(int y=0;y<height;++y)
				for(int x=0;x<width;++x)
				{
					Color c=bm.GetPixel(x,y);
					data[y*width*3+x*3]=c.R;
					data[y*width*3+x*3+1]=c.G;
					data[y*width*3+x*3+2]=c.B;
					//data[y*width*4+x*4+3]=c.A;
				}

			bm.Dispose();
		}

		public byte[] getRGBA()
		{
			return data;
		}

		public int getWidth()
		{
			return width;
		}

		public int getHeight()
		{
			return height;
		}
		
		public bool hasAlpha()
		{
			return false;
		}

		protected byte[] data;
		protected int width;
		protected int height;
	}

	public interface IDrawable
	{
		void Draw(IRenderer r, Node n);
        bool IsWorldSpace
        {
            get;
        }
	}

	public enum PrimitiveType
	{
		TRIANGLES,TRIANGLESTRIP,QUADS,TRIANGLEFAN,LINES,LINESTRIP,POINTS
	}

	/*public enum VertexFormat
	{
		VF_POS2F_COLOR4F_TEX2F,VF_POS3F_NORMAL3F_TEX2F
	}*/


	public class IndexBuffer : IDisposable
	{
		public void Dispose()
		{
			buffer=null;
		}

		public int[] buffer;
	}


	public class Fog
	{
		public enum FogMode
		{
			LINEAR
		}

		public Fog()
		{
			Start=1.0f;
			End=10000;
			Density=0.5f;
			Color=new Color4f(0.4f,0.25f,0.23f,1);
			Mode=FogMode.LINEAR;
		}
		
		public Fog(float start,float end,float density,Color4f color)
		{
			Start=start;
			End=end;
			Density=density;
			Color=color;
			Mode=FogMode.LINEAR;
		}

		public float Start;
		public float End;
		public float Density;
		public Color4f Color;
		public FogMode Mode;
	}

	public class VertexFormat : ISerializable
	{
		public enum ElementType
		{
			Float,Int,Byte
		}
		public enum ElementName
		{
			None=0,Position,Normal,Binormal,Tangent,Color,Texture0,Texture1,Texture2,Texture3
		}

		public class Element : ISerializable
		{
			public Element(ElementName _name,int _count,ElementType _type)
			{
				Name=_name;
				Count=_count;
				Type=_type;
			}
			
			public Element(ElementName _name,int _count)
			{
				Name=_name;
				Count=_count;
				Type=ElementType.Float;
			}

            public Element(ElementName _name, string attrib,int _count)
            {
                Name = _name;
                Count = _count;
                Type = ElementType.Float;
                Attrib = attrib;
            }
            public Element(string attrib, int _count)
            {
                Name = ElementName.None;
                Count = _count;
                Type = ElementType.Float;
                Attrib = attrib;
            }

			public ElementName Name;
			public int Count;
			public ElementType Type;
            public string Attrib;

			public int Size
			{
				get
				{
					switch(Type)
					{
						case ElementType.Float:
							return 4*Count;
						case ElementType.Byte:
							return 1*Count;
						case ElementType.Int:
							return 4*Count;
						default:
							throw new Exception("VertexFormatElement.getSize: dont know format "+Type+".");
					}
				}
			}


			public void Serialize(SerializationContext context)
			{
                context.Write((int)Name);
                context.Write(Count);
                context.Write((int)Type);
            }

            public void DeSerialize(DeSerializationContext context)
            {
				Name=(ElementName)context.ReadInt32();
                Count = context.ReadInt32();
                Type = (ElementType)context.ReadInt32();
            }

            public Element(DeSerializationContext context)
            {
                DeSerialize(context);
            }
		}

		public VertexFormat(Element[] elements)
		{
			this.elements.AddRange(elements);
		}
		
		public int Count
		{
			get{return elements.Count;}
		}

		public int Size
		{
			get
			{
				int i=0;
				foreach(Element e in elements)
				{
					i+=e.Size;
				}
				return i;
			}
		}

		public void Serialize(SerializationContext context)
		{
			context.Write(elements.Count);
			foreach(Element e in elements)
			{
				e.Serialize(context);
			}
		}

        public VertexFormat(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public void DeSerialize(DeSerializationContext context)
        {
			int c=context.ReadInt32();
            elements.Clear();
			for(int i=0;i<c;++i)
			{
				elements.Add(new Element(context));
			}
		}

		public int Find(ElementName name)
		{
			for(int i=0;i<elements.Count;++i)
			{
				Element e = this[i];
				if (e.Name == name)
					return i;
			}
			throw new Exception("cant find element.");
		}

		public Element this[int index]
		{
			get{return (Element)elements[index];}
			set
			{
				elements[index]=value;
			}
		}

		/*public Element[] Elements
		{
			get{return elements.ToArray(;}
		}*/

		protected ArrayList elements=new ArrayList();

		public static readonly VertexFormat VF_P3T2N3=new VertexFormat(new Element[]{
																		   new Element(ElementName.Position,3),
																		   new Element(ElementName.Texture0,2),
																		   new Element(ElementName.Normal,3)
																	   });
		public static readonly VertexFormat VF_P3T2=new VertexFormat(new Element[]{
																						new Element(ElementName.Position,3),
																						new Element(ElementName.Texture0,2)
																					});

		public static readonly VertexFormat VF_P3C4=new VertexFormat(new Element[]{
																						new Element(ElementName.Position,3),
																						new Element(ElementName.Color,4)
																					});
		public static readonly VertexFormat VF_P3C3=new VertexFormat(new Element[]{
																					  new Element(ElementName.Position,3),
																					  new Element(ElementName.Color,3)
		});
		
		public static readonly VertexFormat VF_P2C4T2=new VertexFormat(new Element[]{
																					  new Element(ElementName.Position,2),
																					  new Element(ElementName.Color,4),
																						new Element(ElementName.Texture0,2)
		});
        public static readonly VertexFormat VF_P3 = new VertexFormat(new Element[]{
																					  new Element(ElementName.Position,3)
		});
		public static readonly VertexFormat VF_P3C4T2 = new VertexFormat(new Element[]{
																					  new Element(ElementName.Position,3),
																					  new Element(ElementName.Color,4),
																						new Element(ElementName.Texture0,2)
		});

    }

    public struct VertexP3C4T2
    {
        public VertexP3C4T2(float px, float py, float pz, float r, float g, float b,float a,float u,float v)
        {
            position.X = px;
            position.Y = py;
            position.Z = pz;
            color.r = r;
            color.g = g;
            color.b = b;
            color.a = a;
            texture0.X = u;
            texture0.Y = v;
        }
		
		public static float[] ToFloatArray(VertexP3C4T2[] vertices)
		{
			float[] f = new float[vertices.Length * 9];
			for(int i=0; i<vertices.Length; ++i)
			{
				f[i * 9 + 0] = vertices[i].position.X;
				f[i * 9 + 1] = vertices[i].position.Y;
				f[i * 9 + 2] = vertices[i].position.Z;
				f[i * 9 + 3] = vertices[i].color.r;
				f[i * 9 + 4] = vertices[i].color.g;
				f[i * 9 + 5] = vertices[i].color.b;
				f[i * 9 + 6] = vertices[i].color.a;
				f[i * 9 + 7] = vertices[i].texture0.X;
				f[i * 9 + 8] = vertices[i].texture0.Y;
			}
			return f;
		}
		
		public Vector3 position;
		public Color4f color;
		public Vector2 texture0;
	}

    public struct VertexBone
    {
        public Vector3 Position;
        public Vector3 Texture;
        public Vector3 Normal;
        public Vector3 Binormal;
        public Vector3 Tangent;

        public static readonly VertexFormat Format = new VertexFormat(new VertexFormat.Element[]{
																					  new VertexFormat.Element(VertexFormat.ElementName.Position,3),
																					  new VertexFormat.Element(VertexFormat.ElementName.Texture0,3),
																						new VertexFormat.Element(VertexFormat.ElementName.Normal,3),
																						new VertexFormat.Element(VertexFormat.ElementName.Binormal,3),
																						new VertexFormat.Element(VertexFormat.ElementName.Tangent,3)
		});
    }

    public struct VertexP3C3
	{
        public VertexP3C3(float px, float py, float pz, float r, float g, float b)
        {
            Position.X = px;
            Position.Y = py;
            Position.Z = pz;
            Color.r = r;
            Color.g = g;
            Color.b = b;
        }
		public Vector3 Position;
		public Color3f Color;
	}

    public struct VertexP3C4
    {
        public VertexP3C4(float px, float py, float pz, float r, float g, float b, float a)
        {
            Position.X = px;
            Position.Y = py;
            Position.Z = pz;
            Color.r = r;
            Color.g = g;
            Color.b = b;
            Color.a = a;
        }
        public Vector3 Position;
        public Color4f Color;
    }

    public struct VertexP2C4T2
    {
		public Vector2 position;
		public Color4f color;
		public Vector2 texture0;
	}

    public struct VertexP3T2
    {
        public Vector3 position;
        public Vector2 texture0;
    }
    

	public interface IVertexBuffer
	{
		/*void Lock(LockDelegate lockfunc,object context);
		UniversalVertexFormat getFormat();
		int getSize();
		void Unlock();*/
	}

	public class VertexBuffer
	{
		public VertexFormat Format;
		public int Size;
		public int Count
		{
			get{return Size/Format.Size;}
		}
		public float[] Buffer;
		public float[] GetAttribute(int vertexindex,VertexFormat.ElementName name)
		{
			int vertexstart = vertexindex * Format.Size/4;
			int attributestart = 0;
			int end = Format.Find(name);
			for (int i = 0; i < end; ++i)
				attributestart += Format[i].Count;
			attributestart += vertexstart;
			VertexFormat.Element e = Format[end];
			float[] attr=new float[e.Count];
			for (int i = 0; i < e.Count; ++i)
				attr[i] = Buffer[attributestart + i];
			return attr;
		}
	}

	public class DynamicVertexBuffer : VertexBuffer
	{
		public virtual IntPtr Lock(){return IntPtr.Zero;}
		public virtual void Unlock(){}
		public virtual unsafe void Update(object newdata,int size)
		{
			Array a=(Array)newdata;
			IntPtr source=Marshal.UnsafeAddrOfPinnedArrayElement(a,0);
			IntPtr dest=Lock();
			int *ps=(int*)source.ToPointer();
			int *pd=(int*)dest.ToPointer();
			if(size%4!=0)
				throw new Exception("wrong size.");
			int s=size/4;
			for(int i=0;i<s;++i)
				*pd++=*ps++;
			Unlock();
		}
	}

	public unsafe delegate void LockDelegate(void *ptr,VertexBuffer vb,object context);

/*
	public class ManagedVertexBuffer : IVertexBuffer
	{
		public ManagedVertexBuffer()
		{
		}

		public unsafe void Lock(LockDelegate lockfunc,object context)
		{
			fixed(byte* b=buffer)
			{
				lockfunc(b,this,context);
				//int i=sizeof(VF_Pos3f_Normal3f_Tex2f);
			}
		}
		
		public UniversalVertexFormat getFormat()
		{
			return format;
		}

		public int getSize()
		{
			return buffer.Length;
		}

		//public object buffer;
		public UniversalVertexFormat format;
		public byte[] buffer;
		//protected byte __pin *test;

	}
	
	public class UnmanagedVertexBuffer : IDisposable, IVertexBuffer
	{
		public UnmanagedVertexBuffer()
		{
		}

		public unsafe void Lock(LockDelegate lockfunc,object context)
		{
			lockfunc(buffer,this,context);
		}

		public void Dispose()
		{
		}
			
		public UniversalVertexFormat getFormat()
		{
			return format;
		}

		public int getSize()
		{
			return size;
		}

		public UniversalVertexFormat format;
		unsafe protected void *buffer;
		protected int size;
	}
*/
	public struct Color4b
	{
		public byte r,g,b,a;
	}


	public struct Color4f
	{
		public Color4f(float _r,float _g,float _b,float _a)
		{
			r=_r;
			g=_g;
			b=_b;
			a=_a;
		}
		
		public Color4f(float _r,float _g,float _b)
		{
			r=_r;
			g=_g;
			b=_b;
			a=1;
		}

		static public explicit operator float[](Color4f c)
		{
			return new float[4]{c.r,c.g,c.b,c.a};
		}
		public float r,g,b,a;
	}

	public struct Color3f
	{
		public Color3f(float _r,float _g,float _b)
		{
			r=_r;
			g=_g;
			b=_b;
		}

		static public explicit operator float[](Color3f c)
		{
			return new float[3]{c.r,c.g,c.b};
		}
		public float r,g,b;
	}

	public struct BasicMaterial
	{
		public Color4f diffuse;
		public Color4f specular;
		public Color4f ambient;
	}

	public class Material : IResource
	{
		public Color4f diffuse=new Color4f(1.0f,1.0f,1.0f,1);
		public Color4f specular=new Color4f(0.4f,0.4f,0.4f,1);
		public Color4f ambient=new Color4f(0.3f,0.3f,0.3f,1);
		public bool wire=false;
		public bool twosided=false;
        public bool DepthWrite = true;
        public bool DepthTest = true;
		public Texture diffusemap;
		public float shininess=1;
		public Texture DetailMap;
		public Texture EnvironmentMap;
		public Texture BumpMap;
        public Texture SpecularMap;
        public Texture ReflectionMap;
        public Texture HeightMap;
        public Texture EmissiveMap;
		public bool NoLighting=false;
        public bool Additive = false;
        public Shader Shader;
        public Dictionary<string, float[]> Uniforms = new Dictionary<string, float[]>();

        public void Apply(Shader s, IRenderer r)
        {
            if (s != null)
            {
                int stage = 0;

                if (diffusemap != null)
                {
                    int loc = s.GetUniformLocation("DiffuseMap");
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, new int[] { stage });
                        r.BindTexture(diffusemap.Id, stage++);
                    }
                    else
                    {
                        r.BindTexture(diffusemap.Id, stage++);
                    }
                }
                
                if (BumpMap != null)
                {
                    int loc = s.GetUniformLocation("BumpMap");
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, new int[] { stage });
                        r.BindTexture(BumpMap.Id, stage++);
                    }
                }


                if (DetailMap != null)
                {
                    int loc = s.GetUniformLocation("DetailMap");
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, new int[] { stage });
                        r.BindTexture(DetailMap.Id, stage++);
                    }
                }

                if (HeightMap != null)
                {
                    int loc = s.GetUniformLocation("HeightMap");
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, new int[] { stage });
                        r.BindTexture(HeightMap.Id, stage++);
                    }
                }
                if (SpecularMap != null)
                {
                    int loc = s.GetUniformLocation("SpecularMap");
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, new int[] { stage });
                        r.BindTexture(SpecularMap.Id, stage++);
                    }
                }
                if (EmissiveMap != null)
                {
                    int loc = s.GetUniformLocation("EmissiveMap");
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, new int[] { stage });
                        r.BindTexture(EmissiveMap.Id, stage++);
                    }
                }
                if (EnvironmentMap != null)
                {
                    int loc = s.GetUniformLocation("EnvironmentMap");
                    if (loc >= 0)
                    {
                        //System.Console.WriteLine(stage);
                        r.SetUniform(loc, new int[] { stage });
                        r.BindTexture(EnvironmentMap.Id, stage++);
                    }
                }
                if (ReflectionMap != null)
                {
                    int loc = s.GetUniformLocation("ReflectionMap");
                    if (loc >= 0)
                    {
                        //System.Console.WriteLine(stage);
                        r.SetUniform(loc, new int[] { stage });
                        r.BindTexture(ReflectionMap.Id, stage++);
                    }
                }
                {
                    int loc = s.GetUniformLocation("Time");
                    if (loc >= 0)
                    {
                        //System.Console.WriteLine(stage);
                        r.SetUniform(loc, new float[] { Root.Instance.Time });
                    }

                }
                {
                    int loc = s.GetUniformLocation("Aspect");
                    if (loc >= 0)
                    {
                        //System.Console.WriteLine(stage);
                        r.SetUniform(loc, new float[] { (float)Root.Instance.UserInterface.Renderer.Size.X / (float)Root.Instance.UserInterface.Renderer.Size.Y });
                    }

                }
                {
                    int loc = s.GetUniformLocation("Position2D");
                    if (loc >= 0)
                    {
                        //System.Console.WriteLine(stage);
                        float[] f=r.GetRasterPosition(new float[] { 0, 0, 0 });
                        //System.Console.WriteLine(f[0].ToString() + ", " + f[1].ToString());
                        float sx = r.Size.X/2;
                        float sy = r.Size.Y/2;
                        float[] f1=new float[]{(f[0]/sx)-1.0f,(f[1]/sy)-1.0f};
                        r.SetUniform(loc, f1);
                    }

                }
                {
                    int loc = s.GetUniformLocation("Color");
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, new float[] { diffuse.r, diffuse.g, diffuse.b, diffuse.a });
                    }

                }

                foreach (KeyValuePair<string, float[]> kv in Uniforms)
                {
                    int loc = s.GetUniformLocation(kv.Key);
                    if (loc >= 0)
                    {
                        r.SetUniform(loc, kv.Value);
                    }
                }
            }
        }

        public void Dispose()
        {
        }
        public static Material CreateSimpleMaterial(Texture t)
		{
			Material mat=new Material();
			mat.NoLighting=true;
			mat.diffusemap=t;
			mat.twosided=true;
			return mat;
		}
	}

	public class Light : Node, IResource
	{
		public Light()
		{
		}


        public Light(DeSerializationContext context)
        {
			DeSerialize(context);
		}
		
		public override void Serialize(SerializationContext context)
		{
			base.Serialize(context);
            context.Write((byte)(diffuse.r * 255)); context.Write((byte)(diffuse.g * 255)); context.Write((byte)(diffuse.b * 255)); context.Write((byte)(diffuse.a * 255));
            context.Write((byte)(ambient.r * 255)); context.Write((byte)(ambient.g * 255)); context.Write((byte)(ambient.b * 255)); context.Write((byte)(ambient.a * 255));
            context.Write((byte)(specular.r * 255)); context.Write((byte)(specular.g * 255)); context.Write((byte)(specular.b * 255)); context.Write((byte)(specular.a * 255));
            context.Write((short)(attenuation.X * 100)); context.Write((short)(attenuation.Y * 100)); context.Write((short)(attenuation.Z * 100));
            context.Write(directional);
        }
        public override void DeSerialize(DeSerializationContext context)
        {
			base.DeSerialize(context);
            diffuse.r = (float)context.ReadByte() / 255.0f; diffuse.g = (float)context.ReadByte() / 255.0f; diffuse.b = (float)context.ReadByte() / 255.0f; diffuse.a = (float)context.ReadByte() / 255.0f;
            ambient.r = (float)context.ReadByte() / 255.0f; ambient.g = (float)context.ReadByte() / 255.0f; ambient.b = (float)context.ReadByte() / 255.0f; ambient.a = (float)context.ReadByte() / 255.0f;
            specular.r = (float)context.ReadByte() / 255.0f; specular.g = (float)context.ReadByte() / 255.0f; specular.b = (float)context.ReadByte() / 255.0f; specular.a = (float)context.ReadByte() / 255.0f;
            attenuation.X = (float)context.ReadInt16() / 100.0f; attenuation.Y = (float)context.ReadInt16() / 100.0f; attenuation.Z = (float)context.ReadInt16() / 100.0f;
            directional = context.ReadBoolean();
        }

        public float Range
        {
            get
            {
                return range;
            }
            set
            {
                attenuation.Z = range = value;
            }
        }

		public Color4f diffuse=new Color4f(1.0f,1.0f,1.0f,1);
		public Color4f ambient=new Color4f(0.4f,0.4f,0.4f,1);
		public Color4f specular=new Color4f(0.5f,0.5f,0.5f,1);
		//public Vector3 attenuation=new Vector3(1,0.01f,0);
        public Vector3 attenuation = new Vector3(1, 0, 0);
        public bool directional = false;
        public float range = 0;
    }
}
