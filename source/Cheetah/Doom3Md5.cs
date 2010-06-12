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

namespace Cheetah.Doom3
{
	public class Md5MeshLoader : IResourceLoader
	{
		public IResource Load(FileSystemNode n)
		{
			reader = new StreamReader(n.getStream());
            string[] line;
			model = new Md5Model();

			while ((line = ReadLineTokens()) != null)
			{
				switch (line[0])
				{
					case "numJoints":
						model.Joints = new Md5Joint[int.Parse(line[1])];
						break;
					case "numMeshes":
						model.Meshes = new Md5Mesh[int.Parse(line[1])];
						break;
					case "joints":
						LoadJoints();
						break;
					case "mesh":
						LoadMesh();
						break;
					case "MD5Version":
						model.Header.Version = int.Parse(line[1]);
						if (model.Header.Version != 10)
							Console.WriteLine("md5 warning: version is not 10.");
						break;
					case "commandline":
						//HACK
						break;
				}
			}

			return model;
		}

		void LoadJoints()
		{
            string[] line;
			int i=0;
			while ((line = ReadLineTokens()) != null&&line[0]!="}")
			{
				model.Joints[i].Name = line[0];
				model.Joints[i].Parent = int.Parse(line[1]);
				if(Flip)
					model.Joints[i].Position = new Vector3(float.Parse(line[2]), float.Parse(line[4]), float.Parse(line[3]));
				else
					model.Joints[i].Position = new Vector3(float.Parse(line[2]), float.Parse(line[3]), float.Parse(line[4]));
				float x = float.Parse(line[5]);
				float y = float.Parse(line[6]);
				float z = float.Parse(line[7]);
				if(Flip)
					model.Joints[i].Orientation = new Quaternion(x, z, y, (float)Math.Sqrt(1.0f - ((x * x) + (y * y) + (z * z))));
				else
					model.Joints[i].Orientation = new Quaternion(x, y, z, (float)Math.Sqrt(1.0f - ((x * x) + (y * y) + (z * z))));
				i++;
			}
		}

		void LoadMesh()
		{
			string[] line;
			mesh = new Md5Mesh();
			int i;

			while ((line = ReadLineTokens()) != null && line[0] != "}")
			{
				switch (line[0])
				{
					case "shader":
						mesh.Shader = line[1];
						break;
					case "numverts":
						mesh.Vertices = new Md5Vertex[int.Parse(line[1])];
						break;
					case "vert":
						i=int.Parse(line[1]);
						mesh.Vertices[i].TextureU = float.Parse(line[2]);
						mesh.Vertices[i].TextureV = float.Parse(line[3]);
						mesh.Vertices[i].WeightIndex = int.Parse(line[4]);
						mesh.Vertices[i].WeightCount = int.Parse(line[5]);
						break;
					case "numtris":
						mesh.Triangles = new Md5Triangle[int.Parse(line[1])];
						break;
					case "tri":
						i = int.Parse(line[1]);
						mesh.Triangles[i][0] = int.Parse(line[2]);
						mesh.Triangles[i][1] = int.Parse(line[3]);
						mesh.Triangles[i][2] = int.Parse(line[4]);
						break;
					case "numweights":
						mesh.Weights = new Md5Weight[int.Parse(line[1])];
						break;
					case "weight":
						i = int.Parse(line[1]);
						mesh.Weights[i].JointIndex = int.Parse(line[2]);
						mesh.Weights[i].Value = float.Parse(line[3]);
						if(Flip)
							mesh.Weights[i].Position = new Vector3(float.Parse(line[4]), float.Parse(line[6]), float.Parse(line[5]));
						else
							mesh.Weights[i].Position = new Vector3(float.Parse(line[4]), float.Parse(line[5]), float.Parse(line[6]));
						break;
					case "//":
						try
						{
							mesh.Name = line[2];
						}
						catch (Exception)
						{
						}
						break;
				}
			}
			for(i=0;;++i)
			{
				if (model.Meshes[i] == null)
				{
					model.Meshes[i] = mesh;
					break;
				}
			}
		}

		string[] ReadLineTokens()
		{
			string line;

			while ((line = reader.ReadLine()) != null)
			{
				line = line.Trim();
				if (line.Length == 0)// || line.StartsWith("//"))
					continue;

				string[] split = line.Split(new char[] { ' ', '(', ')','\t' },StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < split.Length; ++i)
				{
					split[i] = split[i].Trim('\"');
				}

				return split;
			}
			return null;
		}

		public Type LoadType
		{
			get { return typeof(Md5Model); }
		}

		public bool CanLoad(FileSystemNode n)
		{
			return n.info != null && (n.info.Extension.ToLower() == ".md5mesh");
		}

		StreamReader reader;
		Md5Model model;
		Md5Mesh mesh;
		bool Flip = false;
	}

	public class Md5Mesh : IDrawable
	{
		public string Name;
		public string Shader;
		public Md5Vertex[] Vertices;
		public Md5Triangle[] Triangles;
		public Md5Weight[] Weights;

		public VertexBuffer vb;
		public IndexBuffer ib;
		public VertexP3C4T2[] v;

        public bool IsWorldSpace
        {
            get{return false;}
        }
		public void CreateBuffers(Md5Joint[] joints,Md5Anim anim, float frame)
		{
			if (v == null)
				v=new VertexP3C4T2[Vertices.Length];
			for(int i=0;i<Vertices.Length;++i)
			{
				if(anim==null)
					v[i].position = CalcVertexPosition(i,joints);
				else
					//v[i].position=CalcVertexPosition(i,anim,frame)*0.12f;
					v[i].position = CalcInterpolatedVertexPosition(i, anim, frame);
				v[i].color.r = 1;
				v[i].color.g=1;
				v[i].color.b=1;
				v[i].color.a=1;
				v[i].texture0.x=Vertices[i].TextureU;
				v[i].texture0.y = Vertices[i].TextureV;
			}
			if(vb==null)
			{
				vb = Root.Instance.UserInterface.Renderer.CreateDynamicVertexBuffer(Vertices.Length * (3 + 4 + 2) * 4);
				vb.Format = VertexFormat.VF_P3C4T2;
			}
			((DynamicVertexBuffer)vb).Update(v, Vertices.Length * (3 + 4 + 2) * 4);

			if (ib == null)
			{
				ib = new IndexBuffer();
				ib.buffer = new int[Triangles.Length * 3];
				for (int i = 0; i < Triangles.Length; ++i)
				{
					ib.buffer[i * 3] = Triangles[i][0];
					ib.buffer[i * 3 + 1] = Triangles[i][1];
					ib.buffer[i * 3 + 2] = Triangles[i][2];
				}
			}
		}
        public void Draw(IRenderer r, Node n)
		{
			r.Draw(vb, PrimitiveType.TRIANGLES, 0, Triangles.Length * 3, ib);
		}

		public Vector3 CalcVertexPosition(int index, Md5Joint[] joints)
		{
			Md5Vertex v = Vertices[index];
			return CalcVertexPosition(v, joints);
		}
		public Vector3 CalcVertexPosition(int index, Md5Anim anim, int frame)
		{
			Md5Vertex v = Vertices[index];
			return CalcVertexPosition(v, anim, frame);
		}
		public Vector3 CalcInterpolatedVertexPosition(int index, Md5Anim anim, float frame)
		{
			Md5Vertex v = Vertices[index];
			return CalcInterpolatedVertexPosition(v, anim, frame);
		}

		protected Matrix3 GetJointMatrix(Md5Joint j)
		{
			return Matrix3.FromTranslation(j.Position) * Matrix3.FromQuaternion(j.Orientation);
		}

		protected Vector3 Transform(Vector3 v, Md5Joint j)
		{
			return GetJointMatrix(j).Transform(v);
		}

		public Vector3 CalcVertexPosition(Md5Vertex v, Md5Joint[] joints)
		{
			Vector3 pos=Vector3.Zero;

			for (int i = v.WeightIndex; i < v.WeightIndex + v.WeightCount; ++i)
			{
				Md5Weight w = Weights[i];
				Md5Joint j = joints[w.JointIndex];

				pos += w.Value * Transform(w.Position, j);
			}

			return pos;
		}

		public Vector3 CalcVertexPosition(Md5Vertex v, Md5Anim a, int frame)
		{
			Vector3 pos = Vector3.Zero;

			for (int i = v.WeightIndex; i < v.WeightIndex + v.WeightCount; ++i)
			{
				Md5Weight w = Weights[i];
				Md5JointValue j = a.Frames[frame].Values[w.JointIndex];
				
				Matrix3 m = Matrix3.FromTranslation(j.Position) * Matrix3.FromQuaternion(j.Orientation);
				pos += w.Value * m.Transform(w.Position);
			}

			return pos;
		}
		public Vector3 CalcInterpolatedVertexPosition(Md5Vertex v, Md5Anim a, float frame)
		{
			int frame1 = (int)frame;
			int frame2 = (int)(frame + 1);
			float alpha = frame - (float)frame1;

			return CalcVertexPosition(v, a, frame1) * (1 - alpha) + CalcVertexPosition(v, a, frame2) * alpha;
		}
	}

	public class Md5Model : IResource, IDrawable, ITickable
	{
		public Md5Joint[] Joints;
		public Md5Mesh[] Meshes;
		public Md5Header Header;
		public Dictionary<string, Md5Anim> Animations;
		public Md5Anim CurrentAnimation;
		public float CurrentFrame;

		public void Dispose()
		{
		}
        public bool IsWorldSpace
        {
            get { return false; }
        }
        protected void CreateBuffers()
		{
			foreach (Md5Mesh m in Meshes)
			{
				m.CreateBuffers(Joints,CurrentAnimation,CurrentFrame);
			}
		}

        public void Draw(IRenderer r, Node n)
		{
			CreateBuffers();
			Material mq = Material.CreateSimpleMaterial(Root.Instance.ResourceManager.LoadTexture("revenant.tga"));
			mq.twosided = true;
			mq.wire = true;
			r.SetMode(RenderMode.Draw3D);
			r.SetMaterial(mq);
			IEffect e = (IEffect)Root.Instance.ResourceManager.Load("shaders/simple.cgfx", typeof(IEffect));
			float[] modelview = new float[16];
			float[] projection = new float[16];
			r.GetMatrix(modelview, projection);
			Matrix3 m1=new Matrix3(modelview);
			Matrix3 m2=new Matrix3(projection);
			Matrix3 m3 = m2*m1;
			e.SetParameter(e.GetParameter("mvp"), (float[])m3);
			e.SetParameter(e.GetParameter("mv"), modelview);
			e.SetParameter(e.GetParameter("Color"), new float[] {1,0,0,1 });
			e.BeginPass(0);
			foreach (Md5Mesh m in Meshes)
			{
				m.Draw(r,n);
			}
			e.EndPass(0);
		}

		public void Tick(float dtime)
		{
			if (CurrentAnimation != null)
			{
				CurrentFrame += dtime * (float)CurrentAnimation.FrameRate;
				if (CurrentFrame >= CurrentAnimation.Frames.Length - 1)
					CurrentFrame = 0;
			}
		}
	}

	public struct Md5Triangle
	{
		public int this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return A;
					case 1: return B;
					case 2: return C;
					default: throw new Exception("");
				}
			}
			set
			{
				switch (index)
				{
					case 0: A = value; break;
					case 1: B = value; break;
					case 2: C = value; break;
					default: throw new Exception("");
				}
			}
		}

		int A;
		int B;
		int C;
	}

	public struct Md5Joint
	{
		public string Name;
		public int Parent;
		public Vector3 Position;
		public Quaternion Orientation;
	}

	public struct Md5Weight
	{
		public int JointIndex;
		public float Value;
		public Vector3 Position;
	}

	public struct Md5JointAnim
	{
		public static readonly int Tx = 1;
		public static readonly int Ty = 2;
		public static readonly int Tz = 4;
		public static readonly int Qx = 8;
		public static readonly int Qy = 16;
		public static readonly int Qz = 32;

		public string BoneName;
		public int ParentIndex;
		public int Components;
		public int FrameIndex;
	}

	public struct Md5JointValue
	{
		public Vector3 Position;
		public Quaternion Orientation;
	}

	public class Md5Frame
	{
		public Md5JointValue[] Values;
	}

	public class Md5AnimLoader : IResourceLoader
	{
		public IResource Load(FileSystemNode n)
		{
			reader = new StreamReader(n.getStream());
			string[] line;
			anim = new Md5Anim();

			while ((line = ReadLineTokens()) != null)
			{
				switch (line[0])
				{
					case "numFrames":
						anim.Frames = new Md5Frame[int.Parse(line[1])];
						break;
					case "numJoints":
						anim.NumJoints = int.Parse(line[1]);
						break;
					case "frameRate":
						anim.FrameRate = int.Parse(line[1]);
						break;
					case "numAnimatedComponents":
						anim.NumAnimatedComponents = int.Parse(line[1]);
						break;
					case "hierarchy":
						LoadHierarchy();
						break;
					case "baseframe":
						anim.BaseFrame = LoadFrame(true);
						break;
					case "frame":
						anim.Frames[int.Parse(line[1])]=LoadFrame(false);
						break;
					case "MD5Version":
						anim.Header.Version = int.Parse(line[1]);
						if (anim.Header.Version != 10)
							Console.WriteLine("md5 warning: version is not 10.");
						break;
					case "commandline":
						//HACK
						break;
				}
			}

			return anim;
		}

		void LoadHierarchy()
		{
			string[] line;
			int i = 0;
			anim.Hierarchy = new Md5JointAnim[anim.NumJoints];

			while ((line = ReadLineTokens()) != null && line[0] != "}")
			{
				anim.Hierarchy[i].BoneName = line[0];
				anim.Hierarchy[i].ParentIndex = int.Parse(line[1]);
				anim.Hierarchy[i].Components = int.Parse(line[2]);
				anim.Hierarchy[i].FrameIndex = int.Parse(line[3]);
				i++;
			}
		}

		Md5Frame LoadFrame(bool baseframe)
		{
			string[] line;
			int i = 0;
			float[] values;
			if(baseframe)
				values = new float[anim.NumJoints*6];
			else
				values=new float[anim.NumAnimatedComponents];

			while ((line = ReadLineTokens()) != null && line[0] != "}")
			{
				foreach (string s in line)
				{
					values[i++] = float.Parse(s);
				}
			}
			if (i != values.Length)
				throw new Exception("");
	
			Md5Frame frame=new Md5Frame();
			Md5Frame baseframe1 = new Md5Frame();

			frame.Values = new Md5JointValue[anim.NumJoints];
			baseframe1.Values = new Md5JointValue[anim.NumJoints];

			if (baseframe)
			{
				for (i = 0; i < anim.NumJoints; ++i)
				{
					frame.Values[i] = GetBaseFrameJointValue(i, values);
				}
				for (i = 0; i < anim.NumJoints; ++i)
				{
					baseframe1.Values[i] = GetBaseFrameJointValue(i, values);
				}

				for (i = 0; i < anim.NumJoints; ++i)
				{
					frame.Values[i] = GetBaseJointValue(i, frame, frame);
				}
			}
			else
			{
				for (i = 0; i < anim.NumJoints; ++i)
				{
					frame.Values[i] = GetJointValue(values, i, frame, anim.BaseFrame);
				}
			}

			if (baseframe)
				return baseframe1;
			else return frame;
		}

		public Md5JointValue GetBaseFrameJointValue(int joint, float[] values)
		{
			Md5JointValue v = new Md5JointValue();
			v.Position.X = values[joint * 6 + 0];
			if (Flip)
			{
				v.Position.Y = values[joint * 6 + 2];
				v.Position.Z = values[joint * 6 + 1];
			}
			else
			{
				v.Position.Y = values[joint * 6 + 1];
				v.Position.Z = values[joint * 6 + 2];
			}
			v.Orientation.X = values[joint * 6 + 3];
			if (Flip)
			{
				v.Orientation.Y = values[joint * 6 + 5];
				v.Orientation.Z = values[joint * 6 + 4];
			}
			else
			{
				v.Orientation.Y = values[joint * 6 + 4];
				v.Orientation.Z = values[joint * 6 + 5];
			}
			v.Orientation.W = (float)Math.Sqrt(1.0f - ((v.Orientation.X * v.Orientation.X) + (v.Orientation.Y * v.Orientation.Y) + (v.Orientation.Z * v.Orientation.Z)));
			return v;
		}
		public Md5JointValue GetBaseJointValue(int joint,Md5Frame baseframe,Md5Frame thisframe)
		{
			Md5JointValue v;

			
			int parentjoint=anim.Hierarchy[joint].ParentIndex;
			if ( parentjoint< 0)
			{
				v = baseframe.Values[joint];
			}
			else
			{
				if (parentjoint >= joint)
					throw new Exception("");

				Md5JointValue parent = thisframe.Values[parentjoint];
				Vector3 p = Matrix3.FromQuaternion(parent.Orientation) * baseframe.Values[joint].Position;
				p += parent.Position;
				Quaternion q = baseframe.Values[joint].Orientation * parent.Orientation;
				q.Normalize();
				v.Orientation = q;
				v.Position = p;
			}

			return v;
		}
/*
		public float[] GetJointValues(float[] values, int joint)
		{
			float[] r = new float[6];
			int i = 0;
			int c = 1;
			for (int k = 0; k < 6; ++k)
			{
				//if ((Hierarchy[joint].Components & c) != 0)
				if (false)
				{
					//component is animated
					r[k] = values[anim.Hierarchy[joint].FrameIndex + i];
					i++;
				}
				else
				{
					//not animated, get from baseframe
					//r[k] = values[joint*6+k];
				}
				c *= 2;
			}
			return r;
		}
*/
		public Md5JointValue GetJointValue(float[] values, int joint, Md5Frame thisframe, Md5Frame baseframe)
		{
			Md5JointValue v = baseframe.Values[joint];

			int i = anim.Hierarchy[joint].FrameIndex;
			int c = anim.Hierarchy[joint].Components;

			if (Flip)
			{
				if ((c & Md5JointAnim.Tx) != 0) v.Position.X = values[i++];
				if ((c & Md5JointAnim.Ty) != 0) v.Position.Z = values[i++];
				if ((c & Md5JointAnim.Tz) != 0) v.Position.Y = values[i++];
				if ((c & Md5JointAnim.Qx) != 0) v.Orientation.X = values[i++];
				if ((c & Md5JointAnim.Qy) != 0) v.Orientation.Z = values[i++];
				if ((c & Md5JointAnim.Qz) != 0) v.Orientation.Y = values[i++];
			}
			else
			{
				if ((c & Md5JointAnim.Tx) != 0) v.Position.X = values[i++];
				if ((c & Md5JointAnim.Ty) != 0) v.Position.Y = values[i++];
				if ((c & Md5JointAnim.Tz) != 0) v.Position.Z = values[i++];
				if ((c & Md5JointAnim.Qx) != 0) v.Orientation.X = values[i++];
				if ((c & Md5JointAnim.Qy) != 0) v.Orientation.Y = values[i++];
				if ((c & Md5JointAnim.Qz) != 0) v.Orientation.Z = values[i++];
			}

			float term = 1.0f - ((v.Orientation.X * v.Orientation.X) + (v.Orientation.Y * v.Orientation.Y) + (v.Orientation.Z * v.Orientation.Z));
			if (term < 0.0f)
				v.Orientation.W = 0.0f;
			else
				v.Orientation.W = (float)Math.Sqrt(term); 



			//v.Orientation.W = (float)Math.Sqrt(1.0f - );

			int parentjoint = anim.Hierarchy[joint].ParentIndex;
			if (parentjoint < 0)
			{
			}
			else
			{
				if (parentjoint >= joint)
					throw new Exception("");

				Md5JointValue parent = thisframe.Values[parentjoint];
				Vector3 p = Matrix3.FromQuaternion(parent.Orientation) * v.Position;
				p += parent.Position;
				Quaternion q = v.Orientation * parent.Orientation;
				q.Normalize();
				v.Orientation = q;
				v.Position = p;
			}

			return v;
		}

		string[] ReadLineTokens()
		{
			string line;

			while ((line = reader.ReadLine()) != null)
			{
				line = line.Trim();
				if (line.Length == 0)// || line.StartsWith("//"))
					continue;

                string[] split = line.Split(new char[] { ' ', '(', ')', '\t' }, StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < split.Length; ++i)
				{
					split[i] = split[i].Trim('\"');
				}

				return split;
			}
			return null;
		}

		public Type LoadType
		{
			get { return typeof(Md5Anim); }
		}

		public bool CanLoad(FileSystemNode n)
		{
			return n.info != null && (n.info.Extension.ToLower() == ".md5anim");
		}

		StreamReader reader;
		Md5Anim anim;
		bool Flip = false;
	}

	public class Md5Anim : IResource
	{
		public Md5Header Header;
		public Md5Frame[] Frames;
		public Md5JointAnim[] Hierarchy;
		public Md5Frame BaseFrame;
		public int FrameRate;
		public int NumJoints;
		public int NumAnimatedComponents;






		public void Dispose()
		{
		}
	}

	public struct Md5Vertex
	{
		public float TextureU;
		public float TextureV;
		public int WeightIndex;
		public int WeightCount;
	}

	public struct Md5Header
	{
		public int Version;
		public string CommandLine;
	}
}