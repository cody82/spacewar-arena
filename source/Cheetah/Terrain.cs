using System;
using System.Drawing;
using System.Collections;
using System.IO;
//using OpenDe;
using Cheetah.Graphics;

namespace Cheetah.Graphics
{
	public class QuadTree
	{
		public QuadTree(Vector3 pos,Vector3 size,int levels,Point units)
		{
			Level=levels-1;
			Center=pos;
			Size=size;
			Radius=(size/2).GetMagnitude();
			Children=new QuadTree[4];
			Units=units;
			for(int i=0;i<4;++i)
			{
				Children[i]=new QuadTree(this,levels-2,i);
			}
		}

		protected QuadTree(QuadTree parent,int level,int n)
		{
			Level=level;
			Units=new Point(parent.Units.X/2,parent.Units.Y/2);
			switch(n)
			{
				case 0:
					Center=parent.Center-parent.Size/4;
					Position.X=parent.Position.X;
					Position.Y=parent.Position.Y;
					break;
				case 1:
					Center.X=parent.Center.X+parent.Size.X/4;
					Center.Z=parent.Center.Z-parent.Size.Z/4;
					Position.X=parent.Position.X+Units.X;
					Position.Y=parent.Position.Y;
					break;
				case 2:
					Center.X=parent.Center.X-parent.Size.X/4;
					Center.Z=parent.Center.Z+parent.Size.Z/4;
					Position.X=parent.Position.X;
					Position.Y=parent.Position.Y+Units.Y;
					break;
				case 3:
					Center=parent.Center+parent.Size/4;
					Position.X=parent.Position.X+Units.X;
					Position.Y=parent.Position.Y+Units.Y;
					break;
			}
			Center.Y=parent.Center.Y;
			Size=parent.Size/2;
			Size.Y=parent.Size.Y;
            Radius = (Size / 2).GetMagnitude();

            if(level>0)
			{
				Children=new QuadTree[4];
				for(int i=0;i<4;++i)
				{
					Children[i]=new QuadTree(this,level-1,i);
				}
			}
		}

		protected int Test(Camera c)
		{
            //HACK
            if (c.Frustum==null || c.Frustum.SphereInFrustum(Center.Z, Center.Y, Center.X, Radius))
			//if(c.Frustum.CubeInFrustum(
			return 1;
			else return 0;
		}

		public void GetVisibleLeafs(Camera c,ArrayList list)
		{
			if(Test(c)>0)//this is in camera frustum
			{
				if(IsLeaf)
				{
					list.Add(this);
				}
				else
				{
					for(int i=0;i<4;++i)
					{
						Children[i].GetVisibleLeafs(c,list);
					}
				}
			}
		}

		public bool IsLeaf
		{
			get{return Level==0;}
		}

		public QuadTree[] Children;
		public Vector3 Center;
		public Vector3 Size;
		public float Radius;
		public int Level;
		public Point Units;
		public Point Position;
		//public Point PatchSize;
		//public Point PatchPosition;
	}

	public interface IHeightMap : IResource
	{
		int GetHeight(int x,int y);
		Point Size
		{
			get;
		}
	}

	public class HeightMapImage : IHeightMap, IResource
	{
        public HeightMapImage(Cheetah.Graphics.Image _img)
		{
			size=new Point(_img.Width,_img.Width);
			img=_img;
		}

		public void Dispose()
		{
		}

		public int GetHeight(int x,int y)
		{
			byte[] data=img.Data;
			return (int)data[y*size.X*3+x*3];
		}

		public Point Size
		{
			get{return size;}
		}

		Cheetah.Graphics.Image img;
		Point size;
	}

/*	public class HeightMap16Bit : IHeightMap
	{
		public HeightMap16Bit(int w,int h)
		{
			data=new ushort[w*h];
			size.X=w;
			size.Y=h;
		}
		
		public HeightMap16Bit(Stream s)
		{
			size.X=size.Y=(int)Math.Sqrt(s.Length/2);
			BinaryReader br=new BinaryReader(s);
			
			data=new ushort[size.X*size.Y];
			for(int i=0;i<size.X*size.Y;++i)
				data[i]=br.ReadUInt16();
		}

		public int GetHeight(int x,int y)
		{
			if(x>=size.X||y>=size.Y)
				return 0;
			return (int)data[y*size.X+x];
		}
		
		public void SetHeight(int x,int y,ushort h)
		{
			data[y*size.X+x]=h;
		}

		public Point Size
		{
			get{return size;}
		}

		protected Point size;
		protected ushort[] data;
	}
	
	public class HeightMapMemory : IHeightMap
	{
		public HeightMapMemory(int w,int h)
		{
			data=new byte[w*h];
		}

		public int GetHeight(int x,int y)
		{
			return (int)data[y*size.X+x];
		}
		
		public void SetHeight(int x,int y,byte h)
		{
			data[y*size.X+x]=h;
		}

		public Point Size
		{
			get{return size;}
		}
		protected Point size;
		protected byte[] data;
	}

	public class HeightMapFractal : HeightMapMemory
	{
		public HeightMapFractal(int w,int h) : base(w,h)
		{
		}

		public void FaultFormation(int iterations)
		{
		}

		public void Erosion(int iterations)
		{
		}

		public void DiamondSquare()
		{
		}

		public void ParticleDesposition()
		{
		}
	}
*/
    public class TerrainLoader : IResourceLoader
    {
        IResource IResourceLoader.Load(FileSystemNode n)
        {
            StreamReader r = new StreamReader(n.getStream());
            string line;

            string header = r.ReadLine().Trim();
            if (header != "TERRAINTEXT")
                return null;

            IHeightMap hm = null;
            Texture color = null;
            Texture detail = null;
            float size = 1;
            float heightscale = 1;
            int patchsize = 33;

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
                    case "colormap":
                        color = Root.Instance.ResourceManager.LoadTexture(val);
                        break;
                    case "heightmap":
                        hm = Root.Instance.ResourceManager.LoadHeightMap(val);
                        break;
                    case "detailmap":
                        detail = Root.Instance.ResourceManager.LoadTexture(val);
                        break;
                    case "patchsize":
                        patchsize = int.Parse(val);
                        break;
                    case "heightscale":
                        heightscale = float.Parse(val);
                        break;
                    case "size":
                        size = float.Parse(val);
                        break;
                }


            }
            
            Terrain t = new Terrain(hm, color, detail,size,heightscale,patchsize);

            return t;
        }

        Type IResourceLoader.LoadType
        {
            get { return typeof(Terrain); }
        }


        bool IResourceLoader.CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Extension.ToLower() == ".terrain";
        }

    }

    public class Terrain : IDrawable, ISerializable, IResource
	{
        public bool IsWorldSpace
        {
            get { return false; }
        }
        public class Patch
		{
			public Patch(Terrain master,int x,int y)
			{
				PatchGridPosition=new Point(x,y);
				Master=master;
				ib=new IndexBuffer[master.Levels];
				ibfixed=new IndexBuffer();
			}

			public void SetNeighbors(Patch l,Patch r,Patch t,Patch b)
			{
				Left=l;
				Right=r;
				Top=t;
				Bottom=b;
			}
			
			public void Draw(IRenderer r, Node n)
			{
				r.SetLighting(false);
				r.Draw(vb,PrimitiveType.TRIANGLESTRIP,0,ibfixed.buffer.Length,ibfixed);
				//r.Draw(vb,PrimitiveType.POINTS,0,ibfixed.buffer.Length,ibfixed);
			}

			protected void ReIndex(int[] buffer,int from,int to)
			{
				int j=0;
				for(int i=0;i<buffer.Length;++i)
					if(buffer[i]==from){buffer[i]=to;j++;}
				if(j==0)
					throw new Exception("reindex");
			}

			public void FixCracks()
			{
				int l=ib[Level].buffer.Length;
				//ibfixed.buffer=ib[Level].buffer;
				ibfixed.buffer=new int[l];
				for(int i=0;i<l;++i)ibfixed.buffer[i]=ib[Level].buffer[i];

				int c=Master.PatchSize;
				int c2=Master.PatchSize;
				int step=1;
				for(int i=0;i<Level;++i){c/=2;step*=2;}
//step*=2;
				//if(step>1)
				//	return;

				//HACK: left=top,right=bottom??
				if(Left!=null&&Left.Level>Level&&Left.Visible)
				{
					if(Left.Level>Level+1)
						throw new Exception("cant solve crack.");
					/*for(int i=0;i<c2-step*2;i+=step*2)
					{
						ReIndex(ibfixed.buffer,(uint)((i+step)*c2),(uint)((i)*c2));
					}*/
					for(int i=0;i<c2-step*2;i+=step*2)
					{
						ReIndex(ibfixed.buffer,(i+step),i);
					}
				}
				if(Right!=null&&Right.Level>Level&&Right.Visible)
				{
					if(Right.Level>Level+1)
						throw new Exception("cant solve crack.");
					/*for(int i=0;i<c2-step*2;i+=step*2)
					{
						ReIndex(ibfixed.buffer,(uint)((i+step)*c2+c2-1),(uint)((i)*c2+c2-1));
					}*/
					int b=c2*(c2-1);
					for(int i=0;i<c2-step*2;i+=step*2)
					{
						ReIndex(ibfixed.buffer,b+i+step,b+i);
					}
				}
				if(Top!=null&&Top.Level>Level&&Top.Visible)
				{
					if(Top.Level>Level+1)
						throw new Exception("cant solve crack.");
					/*for(int i=0;i<c2-step*2;i+=step*2)
					{
						ReIndex(ibfixed.buffer,(uint)((i+step)),(uint)(i));
					}*/
					for(int i=0;i<c2-step*2;i+=step*2)
					{
						ReIndex(ibfixed.buffer,(i+step)*c2,i*c2);
					}
				}
				if(Bottom!=null&&Bottom.Level>Level&&Bottom.Visible)
				{
					if(Bottom.Level>Level+1)
						throw new Exception("cant solve crack.");
					/*int b=c2*(c2-1);
					for(int i=0;i<c2-step*2;i+=step*2)
					{
						ReIndex(ibfixed.buffer,(uint)((b+i+step)),(uint)(b+i));
					}*/
					for(int i=0;i<c2-step*2;i+=step*2)
					{
						ReIndex(ibfixed.buffer,(i+step)*c2+c2-1,i*c2+c2-1);
					}
				}
			}

			protected int[] GenerateIndex(int step)
			{
				int c=Master.PatchSize;
				int sc=(c-1)/step+1;
				int[] index=new int[(sc)*(sc-1)*2+(sc-2)*2];
				int i=0;
				for(int y=0;y<c-1;y+=step)
				{
					for(int x=0;x<c;x+=step)
					{
						index[i++]=x+y*c;
						if(x==0&&y>0)
						{
							index[i]=index[i-1];
							++i;
						}

						index[i++]=x+y*c+c*step;
					}
					if(y<c-2*step)
					{
						index[i]=index[i-1];
						++i;
					}
				}
				if(i!=index.Length)
					throw new Exception();

				return index;
			}

			public void Generate()
			{
				int c=Master.PatchSize;
				int i;
				//Index[0]=GenerateIndex(1);
				int step=1;
				for(i=0;i<ib.Length;++i)
				{
					ib[i]=new IndexBuffer();
					ib[i].buffer=GenerateIndex(step);
					step*=2;
				}

				//byte[] vertices=new byte[c*c*4*(3+4+2)];
				vertices = new VertexP3C4T2[c * c];
				//MemoryStream ms=new MemoryStream(vertices);
				//BinaryWriter bw=new BinaryWriter(ms);
				float s=Master.PatchScale;
				i=0;
				for(int y=0;y<c;++y)
				{
					for(int x=0;x<c;++x)
					{
						float px=(float)PatchGridPosition.X*s+((float)x/(float)(c-1))*s;
						float py=(float)PatchGridPosition.Y*s+((float)y/(float)(c-1))*s;
						px-=s*Master.PatchCount/2;
						py-=s*Master.PatchCount/2;
						float h=((float)Master.Height.GetHeight(
							PatchGridPosition.X*(Master.PatchSize-1)+x,
							PatchGridPosition.Y*(Master.PatchSize-1)+y)
							/255)*Master.HeightScale;

						/*bw.Write(px);bw.Write(h);bw.Write(py);
						bw.Write(1.0f);bw.Write(1.0f);bw.Write(1.0f);bw.Write(1.0f);
						bw.Write((float)(PatchGridPosition.X*(Master.PatchSize-1)+x)/(float)(Master.PatchCount*(Master.PatchSize-1)));
						bw.Write((float)(PatchGridPosition.Y*(Master.PatchSize-1)+y)/(float)(Master.PatchCount*(Master.PatchSize-1)));
						 * */
						vertices[i].position.X = px;
						vertices[i].position.Y = h;
						vertices[i].position.Z = py;
						vertices[i].color.r = vertices[i].color.g = vertices[i].color.b = vertices[i].color.a = 1.0f;
						vertices[i].texture0.x = (float)(PatchGridPosition.X * (Master.PatchSize - 1) + x) / (float)(Master.PatchCount * (Master.PatchSize - 1));
						vertices[i].texture0.y = (float)(PatchGridPosition.Y * (Master.PatchSize - 1) + y) / (float)(Master.PatchCount * (Master.PatchSize - 1));
						i++;
					}
				}

				if(Root.Instance.UserInterface!=null)
				{
					IRenderer r=Root.Instance.UserInterface.Renderer;
					vb=r.CreateStaticVertexBuffer(vertices,vertices.Length*(3+4+2)*4);
					
					/*VertexFormat format=new VertexFormat(new VertexFormat.Element[]{
																					   new VertexFormat.Element(VertexFormat.ElementName.Position,3),
																					   new VertexFormat.Element(VertexFormat.ElementName.Color,4),
																					   new VertexFormat.Element(VertexFormat.ElementName.Texture0,2)
																				   });*/
					vb.Format=VertexFormat.VF_P3C4T2;
				}

			}


			public bool Visible
			{
				get
				{
					return Root.Instance.frame==VisibleFrame;
				}
				set
				{
					if(value)
						VisibleFrame=Root.Instance.frame;
					else
						VisibleFrame=-1;
				}
			}

			public Patch Left,Right,Top,Bottom;
			public Terrain Master;
			public Point PatchGridPosition;
			public IndexBuffer[] ib;
			public IndexBuffer ibfixed;
			public VertexBuffer vb;
			public int Level;
			public int VisibleFrame;
			public VertexP3C4T2[] vertices;
		}


		~Terrain()
		{
			Dispose();
		}

		static protected int GetHighestBitValue(int i)
		{
			int h=0;
			int k=1;
			for(int j=1;j<32;++j)
			{
				if((i&k)!=0)h=k;
				k*=2;
			}
			return h;
		}

		static protected bool IsPowerOf2(int i)
		{
			return GetHighestBitValue(i)==i;
		}

		public void Dispose()
		{
		}
        public Terrain(DeSerializationContext context)
        {
			string hm=context.ReadString();
            string color = context.ReadString();
            string detail = context.ReadString();

            Init(
				Root.Instance.ResourceManager.LoadHeightMap(Root.Instance.FileSystem.Get(hm)),
				Root.Instance.ResourceManager.LoadTexture(Root.Instance.FileSystem.Get(color)),
				Root.Instance.ResourceManager.LoadTexture(Root.Instance.FileSystem.Get(detail))
				);

		}

		protected void Init(IHeightMap heightmap,Texture color,Texture detail)
		{
			Height=heightmap;
			if(Height.Size.X!=Height.Size.Y)
				throw new Exception("map.Size.X!=map.Size.Y");
			if(IsPowerOf2(Height.Size.X-1))
			{
				PatchCount=(Height.Size.X-1)/(PatchSize-1);
			}
			else if(IsPowerOf2(Height.Size.X))
			{
				PatchCount=(Height.Size.X)/(PatchSize-1);
			}
			else
				throw new Exception("!IsPowerOf2(map.Size.X-1)");
			if(Height.Size.X<PatchSize)
				throw new Exception("map.Size.X<patchvertexsize");
			Levels=(int)Math.Log(PatchSize-1,2)+1;


			Patches=new Patch[PatchCount][];
			for(int x=0;x<PatchCount;++x)
			{
				Patches[x]=new Patch[PatchCount];
				for(int y=0;y<PatchCount;++y)
				{
					Patch p=new Patch(this,x,y);
					Patches[x][y]=p;
				}
			}
			for(int x=0;x<PatchCount;++x)
			{
				for(int y=0;y<PatchCount;++y)
				{
					Patches[y][x].SetNeighbors(x>0 ? Patches[y][x-1] : null,
						x<PatchCount-1 ? Patches[y][x+1] : null,
						y>0 ? Patches[y-1][x] : null,
						y<PatchCount-1 ? Patches[y+1][x] : null);

					Patches[y][x].Generate();
				}
			}
			/*for(int x=0;x<PatchCount;++x)
			{
				for(int y=0;y<PatchCount;++y)
				{
					Patches[y][x].Generate();
				}
			}*/
			Material=new Material();
			Material.diffusemap=color;
			Material.DetailMap=detail;

            HighDetail = 1 * PatchScale;
            LowDetail = HighDetail + Levels * PatchScale * 1.2f;
            //Color=color;
			//Detail=detail;
			Tree=new QuadTree(new Vector3(0,HeightScale/2,0),new Vector3(PatchScale*(float)PatchCount,HeightScale,PatchScale*(float)PatchCount),(int)Math.Log(PatchCount,2)+1,new Point(PatchCount,PatchCount));

			//collision
			/*Ode.dVector3[] verts=new Ode.dVector3[heightmap.Size.X*heightmap.Size.Y];
			int[] inds=new int[(heightmap.Size.X-1)*(heightmap.Size.Y-1)*2*3];
			int i=0;
			int j=0;
			for(int y=0;y<heightmap.Size.Y;++y)
			{
				for(int x=0;x<heightmap.Size.X;++x)
				{
					float h=((float)Height.GetHeight(x,y)/255)*HeightScale;
					verts[i].X = (float)(x - heightmap.Size.X / 2) * PatchScale;
					verts[i].Y = h;
					verts[i].Z = (float)(y - heightmap.Size.Y / 2) * PatchScale;
					if (x < heightmap.Size.X - 1 && y < heightmap.Size.Y - 1)
					{
						inds[j++] = y * heightmap.Size.X + x;
						inds[j++] = y * heightmap.Size.X + x +1;
						inds[j++] = (y+1) * heightmap.Size.X + x;

						inds[j++] = (y + 1) * heightmap.Size.X + x;
						inds[j++] = y * heightmap.Size.X + x + 1;
						inds[j++] = (y + 1) * heightmap.Size.X + x +1;
					}
					i++;
				}
			}

			CollisionMesh = new OdeTriMeshData(verts,inds);*/
			//CollisionMesh.
		}

		public Terrain(IHeightMap heightmap,Texture color,Texture detail)
		{
			Init(heightmap,color,detail);
		}
        
        public Terrain(IHeightMap heightmap, Texture color, Texture detail,float size,float heightscale,int patchsize)
        {
            PatchSize = patchsize;
            PatchCount = (heightmap.Size.X) / (PatchSize - 1);
            HeightScale = heightscale;

            //PatchCount*PatchScale=size
            PatchScale = size / PatchCount;

            Init(heightmap, color, detail);
        }
        
        protected int CalcLevel(int x,int y,Camera c)
		{
            //HACK
            return 0;

			Patch p=Patches[y][x];
			Vector3 pos=new Vector3((float)y*PatchScale+0.5f*PatchScale,0,(float)x*PatchScale+0.5f*PatchScale);
			pos-=new Vector3((float)(PatchCount/2)*PatchScale,0,(float)(PatchCount/2)*PatchScale);
			Vector3 dist=pos-c.AbsolutePosition;
			float d=dist.GetMagnitude();
            if (d < HighDetail)
                return 0;
			else if(d>LowDetail)
				return Levels-1;
			else
			{
                d -= HighDetail;
                d /= LowDetail - HighDetail;
                return (int)(d*((float)Levels-1.0f)+0.5f);
			}
			//return y%2+x%2;
		}

		public virtual void Serialize(SerializationContext context)
		{
			string hm=Root.Instance.ResourceManager.Find(Height).GetFullPath();
			string color=Root.Instance.ResourceManager.Find(Material.diffusemap).GetFullPath();
			string detail=Root.Instance.ResourceManager.Find(Material.DetailMap).GetFullPath();

            context.Write(hm);
            context.Write(color);
            context.Write(detail);
        }

        public virtual void DeSerialize(DeSerializationContext context)
        {
            string hm = context.ReadString();
            string color = context.ReadString();
            string detail = context.ReadString();

        }

		public void DrawInfo(IRenderer r)
		{
			for(int y=0;y<PatchCount;++y)
			{
				string line="";
				for(int x=0;x<PatchCount;++x)
				{
					if(Patches[y][x].Visible)
						line+=Patches[y][x].Level.ToString();
					else
						line+="X";
				}
				//r.Draw(line,20,20+y*8,8,8,Root.getInstance().gui.defaultfont.texture);
				Root.Instance.Gui.DefaultFont.Draw(r,line,20,40+y*8);
			}
		}
        public SupCom.ScmapFile scamap;
        public void Draw(IRenderer r, Node n)
		{
			//r.SetMode(RenderMode.Draw3D);
			//r.SetMode(RenderMode.Draw3DPointSprite);
            r.UseShader(Root.Instance.ResourceManager.LoadShader("terrain.shader"));
            Material.NoLighting = false;
            r.SetMaterial(Material);
            Material.Apply(Root.Instance.ResourceManager.LoadShader("terrain.shader"), r);
            //r.BindTexture(Color,0);
            //r.BindTexture(Detail,1);

            if (scamap != null)
            {
                Shader s = Root.Instance.ResourceManager.LoadShader("terrain.supcom.shader");
                r.UseShader(s);
                
                r.SetUniform(s.GetUniformLocation("LowerAlbedo"), new int[] { 0 });
                r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scamap.Layers[0].PathTexture).Id, 0);

                r.SetUniform(s.GetUniformLocation("Albedo0"), new int[] { 1 });
                r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scamap.Layers[1].PathTexture).Id, 1);

                r.SetUniform(s.GetUniformLocation("Albedo1"), new int[] { 2 });
                r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scamap.Layers[2].PathTexture).Id, 2);

                r.SetUniform(s.GetUniformLocation("Albedo2"), new int[] { 3 });
                r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scamap.Layers[3].PathTexture).Id, 3);

                r.SetUniform(s.GetUniformLocation("Albedo3"), new int[] { 4 });
                r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scamap.Layers[4].PathTexture).Id, 4);

                r.SetUniform(s.GetUniformLocation("UpperAlbedo"), new int[] { 5 });
                r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scamap.Layers[5].PathTexture).Id, 5);

                r.SetUniform(s.GetUniformLocation("Mask"), new int[] { 6 });
                r.BindTexture(Material.diffusemap.Id, 6);
                r.SetUniform(s.GetUniformLocation("NormalMap"), new int[] { 7 });
                r.BindTexture(Material.DetailMap.Id, 7);

                r.SetUniform(s.GetUniformLocation("TerrainScale"), new float[] { 10.0f});
                r.SetUniform(s.GetUniformLocation("LowerAlbedoTile"), new float[] { scamap.Layers[0].ScaleTexture });
                r.SetUniform(s.GetUniformLocation("Albedo0Tile"), new float[] { scamap.Layers[1].ScaleTexture });
                r.SetUniform(s.GetUniformLocation("Albedo1Tile"), new float[] { scamap.Layers[2].ScaleTexture });
                r.SetUniform(s.GetUniformLocation("Albedo2Tile"), new float[] { scamap.Layers[3].ScaleTexture });
                r.SetUniform(s.GetUniformLocation("Albedo3Tile"), new float[] { scamap.Layers[4].ScaleTexture });
                r.SetUniform(s.GetUniformLocation("UpperAlbedoTile"), new float[] { scamap.Layers[5].ScaleTexture });
            }
            else
            {
                r.UseShader(Root.Instance.ResourceManager.LoadShader("terrain.shader"));
                r.SetMaterial(Material);
                Material.Apply(Root.Instance.ResourceManager.LoadShader("terrain.shader"), r);
            }
			/*for(int x=0;x<PatchCount;++x)
			{
				for(int y=0;y<PatchCount;++y)
				{
					Patches[y][x].Visible=false;
				}
			}*/

			ArrayList list=new ArrayList();
			Tree.GetVisibleLeafs(Root.Instance.Scene.camera,list);
			int frame=Root.Instance.frame;
			foreach(QuadTree q in list)
			{
				Patches[q.Position.Y][q.Position.X].VisibleFrame=frame;
			}
			int x,y;
			//for(int x=0;x<PatchCount;++x)
			{
				//for(int y=0;y<PatchCount;++y)
				foreach(QuadTree q in list)
				{
					x=q.Position.X;
					y=q.Position.Y;
					//if(Patches[y][x].Visible)
						Patches[y][x].Level=CalcLevel(x,y,Root.Instance.Scene.camera);
				}
			}
			//for(int x=0;x<PatchCount;++x)
			{
				//for(int y=0;y<PatchCount;++y)
				{
					//if(Patches[y][x].Visible)
					foreach(QuadTree q in list)
					{
						x=q.Position.X;
						y=q.Position.Y;
						Patches[y][x].FixCracks();
						Patches[y][x].Draw(r,n);
					}
				}
			}
			/*for(int x=0;x<PatchCount;++x)
			{
				for(int y=0;y<PatchCount;++y)
				{
					if(Patches[y][x].Visible)
				}
			}*/
			//r.SetMode(RenderMode.Draw2D);
			//DrawInfo(r);
		}

		Patch[][] Patches;
		int Levels;
		public IHeightMap Height;
		public int PatchCount;
		public int PatchSize=33;
		public float PatchScale=160;
		public Material Material;
		public float HeightScale=400/1.5f;
        public float LowDetail = 10000;
        public float HighDetail = 100;
		//public OdeTriMeshData CollisionMesh;
		QuadTree Tree;
	}

    public class SupComMap : GeoMipmap, IResource
    {
        public SupComMap(SupCom.ScmapFile map)
            : base(new SupComMapLoader.Heightmap(map),33,100,0.03f)
        {
            scmap = map;

            TextureLoader loader = new TextureLoader();

            Normal = loader.LoadDDS(new MemoryStream(scmap.NormalmapData));
            Mask = loader.LoadDDS(new MemoryStream(scmap.TexturemapData));


        }
        Texture Normal;
        Texture Mask;

        public override void Draw(IRenderer r, Node n)
        {
            Material m = new Material();
            m.wire = true;
            r.SetMaterial(m);

            Shader s = Root.Instance.ResourceManager.LoadShader("terrain.supcom.shader");
            r.UseShader(s);

            r.SetUniform(s.GetUniformLocation("LowerAlbedo"), new int[] { 0 });
            r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scmap.Layers[0].PathTexture).Id, 0);

            r.SetUniform(s.GetUniformLocation("Albedo0"), new int[] { 1 });
            r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scmap.Layers[1].PathTexture).Id, 1);

            r.SetUniform(s.GetUniformLocation("Albedo1"), new int[] { 2 });
            r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scmap.Layers[2].PathTexture).Id, 2);

            r.SetUniform(s.GetUniformLocation("Albedo2"), new int[] { 3 });
            r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scmap.Layers[3].PathTexture).Id, 3);

            r.SetUniform(s.GetUniformLocation("Albedo3"), new int[] { 4 });
            r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scmap.Layers[4].PathTexture).Id, 4);

            r.SetUniform(s.GetUniformLocation("UpperAlbedo"), new int[] { 5 });
            r.BindTexture(Root.Instance.ResourceManager.LoadTexture(scmap.Layers[5].PathTexture).Id, 5);

            r.SetUniform(s.GetUniformLocation("Mask"), new int[] { 6 });
            r.BindTexture(Mask.Id, 6);
            r.SetUniform(s.GetUniformLocation("NormalMap"), new int[] { 7 });
            r.BindTexture(Normal.Id, 7);

            r.SetUniform(s.GetUniformLocation("TerrainScale"), new float[] { 5.0f });
            r.SetUniform(s.GetUniformLocation("LowerAlbedoTile"), new float[] { scmap.Layers[0].ScaleTexture });
            r.SetUniform(s.GetUniformLocation("Albedo0Tile"), new float[] { scmap.Layers[1].ScaleTexture });
            r.SetUniform(s.GetUniformLocation("Albedo1Tile"), new float[] { scmap.Layers[2].ScaleTexture });
            r.SetUniform(s.GetUniformLocation("Albedo2Tile"), new float[] { scmap.Layers[3].ScaleTexture });
            r.SetUniform(s.GetUniformLocation("Albedo3Tile"), new float[] { scmap.Layers[4].ScaleTexture });
            r.SetUniform(s.GetUniformLocation("UpperAlbedoTile"), new float[] { scmap.Layers[5].ScaleTexture }); 
            
            base.Draw(r, n);
        }

        SupCom.ScmapFile scmap;

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

    public class GeoMipmap : IDrawable
    {
        public class Level
        {
            public Level(int level, int patchsize)
            {
                Indices=new int[16][];
                IndexBuffers = new IndexBuffer[16];

                Indices[0] = GenerateIndex(patchsize, level);
                IndexBuffers[0] = new IndexBuffer();
                IndexBuffers[0].buffer = Indices[0];

                for(int i=1;i<Indices.Length;++i)
                {
                    Indices[i] = FixCracks(Indices[0],level, patchsize, IsTopFixed(i), IsBottomFixed(i), IsLeftFixed(i), IsRightFixed(i));
                    IndexBuffers[i] = new IndexBuffer();
                    IndexBuffers[i].buffer = Indices[i];
                }
            }

            public static int GetFixedIndex(bool t, bool b, bool l, bool r)
            {
                return (t ? 1 : 0) * 1 + (b ? 1 : 0) * 2 + (l ? 1 : 0) * 4 + (r ? 1 : 0) * 8;
            }

            public static bool IsBottomFixed(int index)
            {
                return (index & 2) > 0;
            }
            public static bool IsTopFixed(int index)
            {
                return (index & 1) > 0;
            }
            public static bool IsLeftFixed(int index)
            {
                return (index & 4) > 0;
            }
            public static bool IsRightFixed(int index)
            {
                return (index & 8) > 0;
            }

            public static int Step(int level)
            {
                int step = 1;
                for (int i = 0; i < level; ++i) { step *= 2; }
                return step;
            }

            public static int[] GenerateIndex(int patchsize, int level)
            {
                int step = Step(level);
                int c = patchsize;
                int sc = (c - 1) / step + 1;
                int size = (sc) * (sc - 1) * 2 + (sc - 2) * 2;
                //allocate buffer

                int[] index = new int[size];
                int i = 0;
                //step through the grid and fill the index buffer
                //degenerated tringles are needed to render the patches as strips
                for (int y = 0; y < c - 1; y += step)
                {
                    for (int x = 0; x < c; x += step)
                    {
                        //add the index
                        index[i++] = (x + y * c);
                        if (x == 0 && y > 0)
                        {
                            //duplicate a vertex for a degenerated triangle
                            index[i] = index[i - 1];
                            ++i;
                        }

                        index[i++] = (x + y * c + c * step);
                    }
                    if (y < c - 2 * step)
                    {
                        //duplicate a vertex for a degenerated triangle before next row
                        index[i] = index[i - 1];
                        ++i;
                    }
                }
                if (i != index.Length)//should never happen
                    throw new Exception("bla");

                return index;
            }

            public static int[] FixCracks(int[] source, int level, int patchsize,bool top, bool bottom, bool left, bool right)
            {
                int i;

                int[] fixedbuffer = new int[source.Length];
                Array.Copy(source, fixedbuffer, source.Length);

                int c = patchsize;
                int c2 = patchsize;
                int step = 1;

                for (i = 0; i < level; ++i) { c /= 2; step *= 2; }

                if (left)
                {
                    //solve a crack to the left
                    for (i = 0; i < c2 - step * 2; i += step * 2)
                    {
                        ReIndex(fixedbuffer, ((i + step) * c2), ((i) * c2));
                    }
                }
                if (right)
                {
                    //solve a crack to the right
                    for (i = 0; i < c2 - step * 2; i += step * 2)
                    {
                        ReIndex(fixedbuffer, ((i + step) * c2 + c2 - 1), ((i) * c2 + c2 - 1));
                    }
                }
                if (top)
                {
                    //solve a crack to the top
                    for (i = 0; i < c2 - step * 2; i += step * 2)
                    {
                        ReIndex(fixedbuffer, ((i + step)), (i));
                    }
                }
                if (bottom)
                {
                    //solve a crack to the bottom
                    int b = c2 * (c2 - 1);
                    for (i = 0; i < c2 - step * 2; i += step * 2)
                    {
                        ReIndex(fixedbuffer, ((b + i + step)), (b + i));
                    }
                }
                return fixedbuffer;
            }

            public int[] GetIndices(bool top, bool bottom, bool left, bool right)
            {
                return Indices[GetFixedIndex(top, bottom, left, right)];
            }
            public IndexBuffer GetBuffer(bool top, bool bottom, bool left, bool right)
            {
                return IndexBuffers[GetFixedIndex(top, bottom, left, right)];
            }
            protected static void ReIndex(int[] buffer, int from, int to)
            {
                int j = 0;
                for (int i = 0; i < buffer.Length; ++i)
                    if (buffer[i] == from) { buffer[i] = to; j++; }
                if (j == 0)
                    throw new Exception("reindex");
            }

            int[][] Indices;
            IndexBuffer[] IndexBuffers;
        }

        public class Patch
        {
            public Patch(IHeightMap map, int patchsize, int gridx, int gridy, int patchcount,float scale,float heightscale)
            {
                int c = patchsize;
                float s = scale;

                VertexP3T2[] vtx = new VertexP3T2[c * c];

                int i = 0;
                for (int y = 0; y < c; ++y)
                {
                    for (int x = 0; x < c; ++x)
                    {
                        //x and y position
                        float px = (float)gridx * s + ((float)x / (float)(c - 1)) * s;
                        float py = (float)gridy * s + ((float)y / (float)(c - 1)) * s;
                        px -= s * patchcount / 2;
                        py -= s * patchcount / 2;

                        //z position (height)
                        float h = ((float)map.GetHeight(
                            gridx * (patchsize - 1) + x,
                            gridy * (patchsize - 1) + y)) * heightscale;

                        //add the vertex
                        vtx[i].position=new Vector3(px, h, py);
                        vtx[i++].texture0=new Vector2f(
                            (float)(gridx * (c - 1) + x) / (float)(patchcount * (c - 1)),
                            (float)(gridy * (c - 1) + y) / (float)(patchcount * (c - 1))
                            );
                    }
                }

                Vertices = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(vtx, Format.Size * vtx.Length);
                Vertices.Format = Format;
            }

            VertexFormat Format= VertexFormat.VF_P3T2;
            VertexP3T2[] vtx;
            public VertexBuffer Vertices;
        }

        public GeoMipmap(IHeightMap map, int patchsize, float scale,float heightscale)
        {
            int count;

            if (map.Size.X != map.Size.Y)
                throw new Exception("map.Size.X!=map.Size.Y");
            if (IsPowerOf2(map.Size.X - 1))
            {
                count = (map.Size.X - 1) / (patchsize - 1);
            }
            else if (IsPowerOf2(map.Size.X))
            {
                count = map.Size.X / (patchsize - 1);
            }
            else
                throw new Exception("!IsPowerOf2(map.Size.X-1)");
            if (map.Size.X < patchsize)
                throw new Exception("map.Size.X<patchvertexsize");

            int levels = (int)(Math.Log((double)(patchsize - 1)) / Math.Log(2.0)) + 1;

            Levels = GenerateLevels(levels,patchsize);
            Patches = GeneratePatches(map, patchsize, count, scale,heightscale);

            Size = scale * count;

            Lod = new int[count, count];
            System.Console.WriteLine("terrain: " + map.Size.X + "x" + map.Size.X + "+size=" + Size.ToString() + " patchcount=" + count.ToString());
        }

        public float OnePatchSize
        {
            get
            {
                return Size / (float)Patches.GetLength(0);
            }
        }

        public float Size;

        static protected int GetHighestBitValue(int i)
        {
            int h = 0;
            int k = 1;
            for (int j = 1; j < 32; ++j)
            {
                if ((i & k) != 0) h = k;
                k *= 2;
            }
            return h;
        }

        static protected bool IsPowerOf2(int i)
        {
            return GetHighestBitValue(i) == i;
        }

        public static Patch[,] GeneratePatches(IHeightMap map,int patchsize, int count, float scale, float heightscale)
        {
            Patch[,] patches = new Patch[count, count];
            for (int y = 0; y < count; ++y)
            {
                for (int x = 0; x < count; ++x)
                {
                    patches[x, y] = new Patch(map, patchsize, x, y, count,scale,heightscale);
                }
            }
            return patches;
        }

        public static Level[] GenerateLevels(int levels, int patchsize)
        {
            Level[] l = new Level[levels];
            for (int i = 0; i < levels; ++i)
            {
                l[i] = new Level(i, patchsize);
            }
            return l;
        }

        Level[] Levels;
        Patch[,] Patches;
        int[,] Lod;

        int CalcLod(Camera c,int x, int y)
        {
            Vector3 patch = new Vector3(x * OnePatchSize, 0, y * OnePatchSize) + new Vector3(OnePatchSize * 0.5f - Size * 0.5f, 0, OnePatchSize * 0.5f - Size * 0.5f);

            float distance = (c.AbsolutePosition - patch).GetMagnitude();

            float maxdetail = OnePatchSize * 3;
            float mindetail = maxdetail + Levels.Length * OnePatchSize * 2;

            if (distance <= maxdetail)
                return 0;
            else if (distance >= mindetail)
                return Levels.Length - 1;
            else
            {
                return (int)((distance - maxdetail) / (mindetail - maxdetail) * (float)(Levels.Length - 1));
            }
        }

        bool MustFixTop(int x, int y)
        {
            return y > 0 && Lod[x, y - 1] > Lod[x, y];
        }
        bool MustFixBottom(int x, int y)
        {
            return y < Patches.GetLength(0) - 1 && Lod[x, y + 1] > Lod[x, y];
        }
        bool MustFixLeft(int x, int y)
        {
            return x > 0 && Lod[x - 1, y] > Lod[x, y];
        }
        bool MustFixRight(int x, int y)
        {
            return x < Patches.GetLength(0) - 1 && Lod[x + 1, y] > Lod[x, y];
        }

        #region IDrawable Members

        public virtual void Draw(IRenderer r, Node n)
        {
            for (int y = 0; y < Patches.GetLength(0); ++y)
            {
                for (int x = 0; x < Patches.GetLength(0); ++x)
                {
                    Lod[x, y] = CalcLod(Root.Instance.Scene.camera,x, y);
                }
            }

            //r.UseShader(Root.Instance.ResourceManager.LoadShader("simple3d.shader"));
            for (int y = 0; y < Patches.GetLength(0); ++y)
            {
                for (int x = 0; x < Patches.GetLength(0); ++x)
                {
                    Patch p=Patches[x,y];

                    IndexBuffer i = Levels[Lod[x, y]].GetBuffer(MustFixTop(x, y), MustFixBottom(x, y), MustFixLeft(x, y), MustFixRight(x, y));
                    r.Draw(p.Vertices, PrimitiveType.TRIANGLESTRIP, 0, i.buffer.Length, i);
                }
            }
        }

        public bool IsWorldSpace
        {
            get { return false; }
        }

        #endregion
    }
}
