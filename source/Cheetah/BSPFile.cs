using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Cheetah.Graphics;
using OpenTK;


namespace Cheetah.Quake
{
	/// <summary>
	/// Summary description for BSPFile.
	/// </summary>
	
	#region BSP Classes

	public enum FaceType
	{
		Polygon = 1,
		Patch = 2,
		MeshFace = 3,
		Billboard = 4
	}

	public class BSPHeader
	{
		public String ID = "";
		public int Version = 0;

		public BSPHeader()
		{
			//Do nothing
		}		
	}

	public class BSPLump
	{
		public int Offset = 0;
		public int Length = 0;

		public BSPLump()
		{
			//Do nothing
		}
	}

	public class Vector3i
	{
		public int X = 0, Y = 0, Z = 0;

		public Vector3i()
		{
			//Do nothing
		}

		public Vector3i(int X, int Y, int Z)
		{
			//Copy values
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}
	}

	/*
	public class BSPVertex
	{
		public Vector3 Position = new Vector3();
		public Vector2 TextureCoord = new Vector2();
		public Vector2 LightmapCoord = new Vector2();
		public Vector3 Normal = new Vector3();
		public byte[] Color = new byte[4];

		public static readonly int SizeInBytes = 44;

		public BSPVertex()
		{
			//Do nothing
		}
	}
	*/

	public class BSPFace
	{
		public int TextureID = -1;
		public int Effect = 0;
		public int Type = 0;
		public int StartVertexIndex = -1;
		public int NumVertices = 0;
		public int MeshVertexIndex = -1;
		public int NumMeshVertices = 0;
		public int LightmapID = -1;
		public int[] MapCorner = new int[2];
		public int[] MapSize = new int[2];
		public Vector3 MapPosition = new Vector3();
		public Vector3[] MapVectors = new Vector3[2];
		public Vector3 Normal = new Vector3();
		public int[] Size = new int[2];

		public static readonly int SizeInBytes = 104;

		public BSPFace()
		{
			MapVectors[0] = new Vector3();
			MapVectors[1] = new Vector3();
		}
	}

	public class BSPTexture
	{
		public String Name = "";
		public int Flags = 0;
		public int Contents = 0;

		public static readonly int SizeInBytes = 72;

		public BSPTexture()
		{
			//Do nothing
		}
	}

	public class BSPLightmap
	{
		public byte[] ImageBytes = new byte[128 * 128 * 3];

		public static readonly int SizeInBytes = 49152;

		public BSPLightmap()
		{
			//Do nothing
		}
	}

	public class BSPNode
	{
		public int Plane = 0;
		public int Front = 0;
		public int Back = 0;
		public Vector3i Min = new Vector3i();
		public Vector3i Max = new Vector3i();

		public static readonly int SizeInBytes = 36;

		public BSPNode()
		{
			//Do nothing
		}
	}

	public class BSPLeaf
	{
		public int Cluster = 0;
		public int Area = 0;
		public Vector3i Min = new Vector3i();
		public Vector3i Max = new Vector3i();
		public int LeafFace = 0;
		public int NumLeafFaces = 0;
		public int LeafBrush = 0;
		public int NumLeafBrushes = 0;

		public static readonly int SizeInBytes = 48;

		public BSPLeaf()
		{
			//Do nothing
		}
	}

	public class BSPPlane
	{
		public Vector3 Normal = new Vector3();
		public float Distance = 0.0f;

		public static readonly int SizeInBytes = 16;

		public BSPPlane()
		{
			//Do nothing
		}
	}

	public class BSPModel
	{
		public float[] Mins = new float[3];
		public float[] Maxes = new float[3];
		public int FirstFace = 0;
		public int NumFaces = 0;
        public int FirstBrush = 0;
		public int NumBrushes = 0;

		public static readonly int SizeInBytes = 40;

		public BSPModel()
		{
			//Do nothing
		}
	}

	public class BSPBrush
	{
		public int FirstSide = 0;
		public int NumSides = 0;
		public int TextureIndex = 0;

		public static readonly int SizeInBytes = 12;

		public BSPBrush()
		{
			//Do nothing
		}
	}

	public class BSPBrushSide
	{
		public int Plane = 0;
		public int Texture = 0;

		public static readonly int SizeInBytes = 8;

		public BSPBrushSide()
		{
			//Do nothing
		}
	}

	public class BSPShader
	{
		public String Name = "";
        public int BrushIndex = 0;
		public int ContentFlags = 0;

		public static readonly int SizeInBytes = 72;

		public BSPShader()
		{
			//Do nothing
		}
	}

	public class BSPVisData
	{
		public int NumClusters = 0;
		public int BytesPerCluster = 0;
		public byte[] BitSets = null;

		public BSPVisData()
		{
			//Do nothing
		}
	}

	public enum LumpType
	{
		Entities = 0,
		Textures,
		Planes,
		Nodes,
		Leaves,
		LeafFaces,
		LeafBrushes,
		Models,
		Brushes,
		BrushSides,
		Vertices,
		MeshIndices,
		Shaders,
		Faces,
		Lightmaps,
		LightVols,
		VisData
	}

	#endregion

	public class Trigger
	{
		public int ModelIndex = -1;
		public String Name = "";

		public Trigger()
		{
			//Do nothing
		}

		public Trigger(String Name, int ModelIndex)
		{
			//Copy valies
			this.Name = Name;
			this.ModelIndex = ModelIndex;
		}
	}

	public class CollisionInformation
	{
		public float Fraction = 1.0f;
		public Vector3 EndPoint = new Vector3();
		public Vector3 Normal = new Vector3();
		public bool StartsOut = true;
		public bool AllSolid = false;
		public float PlaneDistance = 0.0f;

		public CollisionInformation()
		{
			//Do nothing
		}
	}

	public enum CollisionTypes
	{
		Ray, Sphere, Box
	}

	public class BSPFile
	{
		#region Class Variables

		//Constants
		private static readonly int MaxLumps = 17;
		private static readonly int LeafFaceSizeInBytes = 4;
		private static readonly int MeshIndexSizeInBytes = 4;
		private static readonly int LeafBrushSizeInBytes = 4;
		private static readonly int VertexSizeInBytes = 44;
		private static readonly float QuakeEpsilon = 0.03125f;

		//OpenGL
		//private IntPtr glClientActiveTextureARBPtr;
		//private IntPtr glActiveTextureARBPtr;

		//Skybox
		private int NoDrawTextureIndex = -1;
		private Texture[] SkyBoxTextures = null;

		//Collision detection
		public CollisionInformation CollisionInfo = new CollisionInformation();
		private CollisionTypes CollisionType = CollisionTypes.Ray;
		private float CollisionOffset = 0.0f;
		private Vector3 CollisionStart = new Vector3();
		private Vector3 CollisionEnd = new Vector3();
		private Vector3 CollisionExtents = new Vector3();
		private Vector3 CollisionMin = new Vector3();
		private Vector3 CollisionMax = new Vector3();

		//Triggers
		private Trigger[] Triggers = null;
		private Hashtable IndexTriggerHash = new Hashtable();
		
		//Header
		private BSPHeader Header = new BSPHeader();
		private BSPLump[] Lumps = new BSPLump[MaxLumps];

		//Vertices
		private int NumVertices = 0;
		private float[] Vertices = null;
		private float[] TextureCoords = null;
		private float[] LightmapCoords = null;
		
		//Faces
		private int NumFaces = 0;
		private BSPFace[] Faces = null;
		private BitArray FacesDrawn;
		
		//Textures
		private int NumTextures = 0;
		private Texture[] Textures = null;
		private BSPTexture[] LoadTextures = null;

		//Lightmaps
		private int NumLightmaps = 0;
		private Texture[] Lightmaps = null;
		private float Gamma = 10.0f;

		//Nodes
		private int NumNodes = 0;
		private BSPNode[] Nodes = null;

		//Leaves
		private int NumLeaves = 0;
		private BSPLeaf[] Leaves = null;

		//Planes
		private int NumPlanes = 0;
		private BSPPlane[] Planes = null;

		//Leaf faces
		private int NumLeafFaces = 0;
		private int[] LeafFaces = null;

		//Mesh indcies
		private int NumMeshIndices = 0;
		private uint[] MeshIndices = null;

		//Models
		private int NumModels = 0;
		private BSPModel[] Models = null;

		//Brushes
		private int NumBrushes = 0;
		private BSPBrush[] Brushes = null;
		
		//Brush sides
		private int NumBrushSides = 0;
		private BSPBrushSide[] BrushSides = null;

		//Leaf faces
		private int NumLeafBrushes = 0;
		private int[] LeafBrushes = null;

		//Shaders
		private int NumShaders = 0;
		private BSPShader[] Shaders = null;

		//Visibility Data
		private BSPVisData Clusters = null;

		//Entities
		private int EntityStringLength = 0;
		private String EntityString = "";
		public BSPEntityCollection Entities;

		#endregion

		public BSPFile(String FileName)
		{
				
			LoadBSP(FileName);
            BuildBuffers();
		}

		public void LoadBSP(String FileName)
		{
			FileStream MapFile = new FileStream(FileName, FileMode.Open);
			byte[] MapData = new byte[MapFile.Length];

			//Read in data
			MapFile.Read(MapData, 0, (int)MapFile.Length);
						
			//Read from in memory data
			BinaryReader InputFile = new BinaryReader( new MemoryStream(MapData) );

			//Read in header
			Header.ID = Encoding.ASCII.GetString(InputFile.ReadBytes(4), 0, 4);
			Header.Version = InputFile.ReadInt32();

			for (int i=0; i<MaxLumps; i++)
			{
				Lumps[i] = new BSPLump();
				Lumps[i].Offset = InputFile.ReadInt32();
				Lumps[i].Length = InputFile.ReadInt32();
			}

			if ( (Header.ID != "IBSP") && (Header.Version != 0x2E) )
			{
				throw new System.Exception("Wrong file type or version");
			}

			//Allocate vertices
			NumVertices = Lumps[(int)LumpType.Vertices].Length / VertexSizeInBytes;
			Vertices = new float[NumVertices * 3];
			TextureCoords = new float[NumVertices * 2];
			LightmapCoords = new float[NumVertices * 2];

			//Allocate faces
			NumFaces = Lumps[(int)LumpType.Faces].Length / BSPFace.SizeInBytes;
			Faces = new BSPFace[NumFaces];
									
			//Allocate textures
			NumTextures = Lumps[(int)LumpType.Textures].Length / BSPTexture.SizeInBytes;
			LoadTextures = new BSPTexture[NumTextures];
			Textures = new Texture[NumTextures];
			
			//Allocate lightmaps
			NumLightmaps = Lumps[(int)LumpType.Lightmaps].Length / BSPLightmap.SizeInBytes;
			Lightmaps = new Texture[NumLightmaps];

			//Allocate nodes
			NumNodes = Lumps[(int)LumpType.Nodes].Length / BSPNode.SizeInBytes;
            Nodes = new BSPNode[NumNodes];
			
			//Allocate leaves
			NumLeaves = Lumps[(int)LumpType.Leaves].Length / BSPLeaf.SizeInBytes;
			Leaves = new BSPLeaf[NumLeaves];

			//Allocate leaf faces
			NumLeafFaces = Lumps[(int)LumpType.LeafFaces].Length / LeafFaceSizeInBytes;
			LeafFaces = new int[NumLeafFaces];

			//Allocate planes
			NumPlanes = Lumps[(int)LumpType.Planes].Length / BSPPlane.SizeInBytes;
			Planes = new BSPPlane[NumPlanes];

			//Allocate mesh indices
			NumMeshIndices = Lumps[(int)LumpType.MeshIndices].Length / MeshIndexSizeInBytes;
			MeshIndices = new uint[NumMeshIndices];

			//Allocate models
			NumModels = Lumps[(int)LumpType.Models].Length / BSPModel.SizeInBytes;
			Models = new BSPModel[NumModels];

			//Allocate brushes
			NumBrushes = Lumps[(int)LumpType.Brushes].Length / BSPBrush.SizeInBytes;
			Brushes = new BSPBrush[NumBrushes];

			//Allocate brush sides
			NumBrushSides = Lumps[(int)LumpType.BrushSides].Length / BSPBrushSide.SizeInBytes;
			BrushSides = new BSPBrushSide[NumBrushSides];

			//Allocate leaf brushes
			NumLeafBrushes = Lumps[(int)LumpType.LeafBrushes].Length / LeafBrushSizeInBytes;
			LeafBrushes = new int[NumLeafBrushes];

			//Allocate shaders
			NumShaders = Lumps[(int)LumpType.Shaders].Length / BSPShader.SizeInBytes;
			Shaders = new BSPShader[NumShaders];

			//Allocate visibility data
			Clusters = new BSPVisData();

			//Calculate entity string length
			EntityStringLength = Lumps[(int)LumpType.Entities].Length;

			//=============
			//Entity String
			//=============
			InputFile.BaseStream.Seek(Lumps[(int)LumpType.Entities].Offset, SeekOrigin.Begin);
			byte[] EntityData = InputFile.ReadBytes(EntityStringLength);
			foreach (byte EntityByte in EntityData)
			{
				char EntityChar = Convert.ToChar(EntityByte);

				if (EntityChar != '\0')
				{
					EntityString += EntityChar;
				}			
			}

			//Generate entity collection
			Entities = new BSPEntityCollection(EntityString);

			//Seek "ambient" value of "worldspawn" entity
			String Ambient = Entities.SeekFirstEntityValue("worldspawn", "ambient");

			try
			{
				Gamma = float.Parse(Ambient);
				Gamma /= 17.0f;
			}
			catch
			{
				//Do nothing
			}

			//Seek "trigger_multiple" entities
			BSPEntity[] BSPTriggers = Entities.SeekEntitiesByClassname("trigger_multiple");

			if (BSPTriggers.Length > 0)
			{
				//Create list
				Triggers = new Trigger[BSPTriggers.Length];

				int TriggerIndex = 0;
				foreach(BSPEntity CurrentTrigger in BSPTriggers)
				{
					try
					{
						Trigger NewTrigger = new Trigger();
						
						//Parse name
						NewTrigger.Name = CurrentTrigger.SeekFirstValue("trigger_name");

						String Model = CurrentTrigger.SeekFirstValue("model");
						Model = Model.Replace("*", String.Empty);

						NewTrigger.ModelIndex = int.Parse(Model);

						//Add to hash table
						IndexTriggerHash[ NewTrigger.ModelIndex ] = NewTrigger;

						//Add to list
						Triggers[TriggerIndex] = NewTrigger;

						//Next trigger
						TriggerIndex++;
					}
					catch
					{
						//Do nothing
					}
				}
			}
						
			//========
			//Vertices
			//========
			int CurrentVertex = 0;
			int CurrentTextureCoord = 0;

			int VerticesOffset = Lumps[(int)LumpType.Vertices].Offset;
			for (int i=0; i<NumVertices; i++)
			{
				InputFile.BaseStream.Seek(VerticesOffset + (i * VertexSizeInBytes), SeekOrigin.Begin);			
				
				//Swap Y and Z values; negate Z
				Vertices[CurrentVertex] = InputFile.ReadSingle();
				Vertices[CurrentVertex + 2] = -InputFile.ReadSingle();
				Vertices[CurrentVertex + 1] = InputFile.ReadSingle();

				TextureCoords[CurrentTextureCoord] = InputFile.ReadSingle();

				//Negate V texture coordinate
				TextureCoords[CurrentTextureCoord + 1] = -InputFile.ReadSingle();

				LightmapCoords[CurrentTextureCoord] = InputFile.ReadSingle();

				//Negate V texture coordinate
				LightmapCoords[CurrentTextureCoord + 1] = -InputFile.ReadSingle();

				//Shift
				CurrentVertex += 3;
				CurrentTextureCoord += 2;
			}
			
			//=====
			//Faces
			//=====
			int FacesOffset = Lumps[(int)LumpType.Faces].Offset;
			for (int i=0; i<NumFaces; i++)
			{
				InputFile.BaseStream.Seek(FacesOffset + (i * BSPFace.SizeInBytes), SeekOrigin.Begin);

				Faces[i] = new BSPFace();
				Faces[i].TextureID = InputFile.ReadInt32();
				Faces[i].Effect = InputFile.ReadInt32();
				Faces[i].Type = InputFile.ReadInt32();
				Faces[i].StartVertexIndex = InputFile.ReadInt32();
				Faces[i].NumVertices = InputFile.ReadInt32();
				Faces[i].MeshVertexIndex = InputFile.ReadInt32();
				Faces[i].NumMeshVertices = InputFile.ReadInt32();
				Faces[i].LightmapID = InputFile.ReadInt32();

				Faces[i].MapCorner[0] = InputFile.ReadInt32();
				Faces[i].MapCorner[1] = InputFile.ReadInt32();

				Faces[i].MapSize[0] = InputFile.ReadInt32();
				Faces[i].MapSize[1] = InputFile.ReadInt32();

				Faces[i].MapPosition.X = InputFile.ReadSingle();
				Faces[i].MapPosition.Y = InputFile.ReadSingle();
				Faces[i].MapPosition.Z = InputFile.ReadSingle();

				Faces[i].MapVectors[0].X = InputFile.ReadSingle();
				Faces[i].MapVectors[0].Y = InputFile.ReadSingle();
				Faces[i].MapVectors[0].Z = InputFile.ReadSingle();

				Faces[i].MapVectors[1].X = InputFile.ReadSingle();
				Faces[i].MapVectors[1].Y = InputFile.ReadSingle();
				Faces[i].MapVectors[1].Z = InputFile.ReadSingle();

				Faces[i].Normal.X = InputFile.ReadSingle();
				Faces[i].Normal.Y = InputFile.ReadSingle();
				Faces[i].Normal.Z = InputFile.ReadSingle();

				Faces[i].Size[0] = InputFile.ReadInt32();
				Faces[i].Size[1] = InputFile.ReadInt32();
			}
			
			//========
			//Textures
			//========
			InputFile.BaseStream.Seek(Lumps[(int)LumpType.Textures].Offset, SeekOrigin.Begin);
			for(int i=0; i<NumTextures; i++)
			{
				LoadTextures[i] = new BSPTexture();

				byte[] NameBytes = InputFile.ReadBytes(64);
				for (int NameByteIndex=0; NameByteIndex<64; NameByteIndex++)
				{
					if (NameBytes[NameByteIndex] != '\0')
					{
						LoadTextures[i].Name += Convert.ToChar(NameBytes[NameByteIndex]);
					}
				}
				
				LoadTextures[i].Flags = InputFile.ReadInt32();
				LoadTextures[i].Contents = InputFile.ReadInt32();

				//Check for skybox texture
				if (LoadTextures[i].Name.IndexOf("bookstore/no_draw") != -1)
				{
					NoDrawTextureIndex = i;
				}
			}

			//Load textures
			for (int i=0; i<NumTextures; i++)
			{
				String JpgFile = LoadTextures[i].Name + ".jpg";
				String TgaFile = LoadTextures[i].Name + ".tga";
				
				//try
				{
					if (File.Exists(JpgFile))
					{
						Textures[i] = Root.Instance.ResourceManager.LoadTexture(JpgFile);
					}
					else if (File.Exists(TgaFile))
					{
                        Textures[i] = Root.Instance.ResourceManager.LoadTexture(TgaFile);
					}
					else
					{
						//Mark as not loaded
                        Console.WriteLine("cant find " + JpgFile + " or " + TgaFile);
						Textures[i] = null;
					}
				}
				//catch
				{
					//Mark as not loaded
					//Textures[i] = null;
				}
			}

			//Load skybox textures
			SkyBoxTextures = new Texture[6];

			//Load depending on the time
			String SkyDir = "day";
			int Hour = DateTime.Now.Hour;
			
			if ( (Hour > 6) && (Hour < 8) )
			{
				SkyDir = "dawn";
			}
			else if ( (Hour >= 8) && (Hour < 18) )
			{
				SkyDir = "day";
			}
			else if ( (Hour >= 18) && (Hour < 21) )
			{
				SkyDir = "dusk";
			}
			else
			{
				SkyDir = "night";
			}
            /*
            SkyBoxTextures[0] = Root.Instance.ResourceManager.LoadTexture(@"textures\bookstore\skies\" + SkyDir + @"\negx.jpg", true);
            SkyBoxTextures[1] = Root.Instance.ResourceManager.LoadTexture(@"textures\bookstore\skies\" + SkyDir + @"\negy.jpg", true);
            SkyBoxTextures[2] = Root.Instance.ResourceManager.LoadTexture(@"textures\bookstore\skies\" + SkyDir + @"\negz.jpg", true);
            SkyBoxTextures[3] = Root.Instance.ResourceManager.LoadTexture(@"textures\bookstore\skies\" + SkyDir + @"\posx.jpg", true);
            SkyBoxTextures[4] = Root.Instance.ResourceManager.LoadTexture(@"textures\bookstore\skies\" + SkyDir + @"\posy.jpg", true);
            SkyBoxTextures[5] = Root.Instance.ResourceManager.LoadTexture(@"textures\bookstore\skies\" + SkyDir + @"\posz.jpg", true);		
            */
			//=========
			//Lightmaps
			//=========
			InputFile.BaseStream.Seek(Lumps[(int)LumpType.Lightmaps].Offset, SeekOrigin.Begin);
			for (int i=0; i<NumLightmaps; i++)
			{
				byte[] InputData = InputFile.ReadBytes(BSPLightmap.SizeInBytes);
				/*Bitmap ImageData = new Bitmap(128, 128);

				int ByteIndex = 0;
				for (int y=0; y<128; y++)
				{
					for (int x=0; x<128; x++)
					{
						byte R = InputData[ByteIndex];
						byte G = InputData[ByteIndex + 1];
						byte B = InputData[ByteIndex + 2];

						ImageData.SetPixel( x, y, Color.FromArgb(R, G, B) );
						ByteIndex += 3;
					}
				}

				//Alter gamma
				ChangeGamma(ref ImageData, Gamma);*/
				
				//Copy back
                Lightmaps[i] = new Texture(Root.Instance.UserInterface.Renderer.CreateTexture(InputData,128,128,false));
			}

			//=====
			//Nodes
			//=====
			int NodesOffset = Lumps[(int)LumpType.Nodes].Offset;
			for (int i=0; i<NumNodes; i++)
			{
				InputFile.BaseStream.Seek(NodesOffset + (i * BSPNode.SizeInBytes), SeekOrigin.Begin);
				
				Nodes[i] = new BSPNode();

				Nodes[i].Plane = InputFile.ReadInt32();
				Nodes[i].Front = InputFile.ReadInt32();
				Nodes[i].Back = InputFile.ReadInt32();

				//Swap Y and Z; invert Z
				Nodes[i].Min.X = InputFile.ReadInt32();
				Nodes[i].Min.Z = -InputFile.ReadInt32();
				Nodes[i].Min.Y = InputFile.ReadInt32();

				//Swap Y and Z; invert Z
				Nodes[i].Max.X = InputFile.ReadInt32();
				Nodes[i].Max.Z = -InputFile.ReadInt32();
				Nodes[i].Max.Y = InputFile.ReadInt32();
			}

			//======
			//Leaves
			//======
			int LeavesOffset = Lumps[(int)LumpType.Leaves].Offset;
			for (int i=0; i<NumLeaves; i++)
			{
				InputFile.BaseStream.Seek(LeavesOffset + (i * BSPLeaf.SizeInBytes), SeekOrigin.Begin);
				
				Leaves[i] = new BSPLeaf();

				Leaves[i].Cluster = InputFile.ReadInt32();
				Leaves[i].Area = InputFile.ReadInt32();

				//Swap Y and Z; invert Z
				Leaves[i].Min.X = InputFile.ReadInt32();
				Leaves[i].Min.Z = -InputFile.ReadInt32();
				Leaves[i].Min.Y = InputFile.ReadInt32();

				//Swap Y and Z; invert Z
				Leaves[i].Max.X = InputFile.ReadInt32();
				Leaves[i].Max.Z = -InputFile.ReadInt32();
				Leaves[i].Max.Y = InputFile.ReadInt32();

				Leaves[i].LeafFace = InputFile.ReadInt32();
				Leaves[i].NumLeafFaces = InputFile.ReadInt32();
				Leaves[i].LeafBrush = InputFile.ReadInt32();
				Leaves[i].NumLeafBrushes = InputFile.ReadInt32();
			}

			//==========
			//Leaf Faces
			//==========
			InputFile.BaseStream.Seek(Lumps[(int)LumpType.LeafFaces].Offset, SeekOrigin.Begin);
			for (int i=0; i<NumLeafFaces; i++)
			{
				LeafFaces[i] = InputFile.ReadInt32();
			}

			//======
			//Planes
			//======
			int PlanesOffset = Lumps[(int)LumpType.Planes].Offset;
			for (int i=0; i<NumPlanes; i++)
			{
				InputFile.BaseStream.Seek(PlanesOffset + (i * BSPPlane.SizeInBytes), SeekOrigin.Begin);
				
				Planes[i] = new BSPPlane();

				//Swap Y and Z; invert Z
				Planes[i].Normal.X = InputFile.ReadSingle();
				Planes[i].Normal.Z = -InputFile.ReadSingle();
				Planes[i].Normal.Y = InputFile.ReadSingle();
				Planes[i].Distance = InputFile.ReadSingle();
			}

			//============
			//Mesh Indices
			//============
			InputFile.BaseStream.Seek(Lumps[(int)LumpType.MeshIndices].Offset, SeekOrigin.Begin);
			for (int i=0; i<NumMeshIndices; i++)
			{
				MeshIndices[i] = InputFile.ReadUInt32();
			}

			//======
			//Models
			//======
			int ModelsOffset = Lumps[(int)LumpType.Models].Offset;
			for (int i=0; i<NumModels; i++)
			{
				InputFile.BaseStream.Seek(ModelsOffset + (i * BSPModel.SizeInBytes), SeekOrigin.Begin);
				
				Models[i] = new BSPModel();

				//Swap Y and Z; negate Y
				Models[i].Mins[0] = InputFile.ReadSingle();
				Models[i].Maxes[2] = -InputFile.ReadSingle();
				Models[i].Mins[1] = InputFile.ReadSingle();

				//Swap Y and Z; negate Y
				Models[i].Maxes[0] = InputFile.ReadSingle();
				Models[i].Mins[2] = -InputFile.ReadSingle();
				Models[i].Maxes[1] = InputFile.ReadSingle();

				Models[i].FirstFace = InputFile.ReadInt32();
				Models[i].NumFaces = InputFile.ReadInt32();
				Models[i].FirstBrush = InputFile.ReadInt32();
				Models[i].NumBrushes = InputFile.ReadInt32();
			}

			//=======
			//Brushes
			//=======
			int BrushesOffset = Lumps[(int)LumpType.Brushes].Offset;
			for (int i=0; i<NumBrushes; i++)
			{
				InputFile.BaseStream.Seek(BrushesOffset + (i * BSPBrush.SizeInBytes), SeekOrigin.Begin);
				
				Brushes[i] = new BSPBrush();

				Brushes[i].FirstSide = InputFile.ReadInt32();
				Brushes[i].NumSides = InputFile.ReadInt32();
				Brushes[i].TextureIndex = InputFile.ReadInt32();
			}

			//===========
			//Brush Sides
			//===========
			int BrushSidesOffset = Lumps[(int)LumpType.BrushSides].Offset;
			for (int i=0; i<NumBrushSides; i++)
			{
				InputFile.BaseStream.Seek(BrushSidesOffset + (i * BSPBrushSide.SizeInBytes), SeekOrigin.Begin);
				
				BrushSides[i] = new BSPBrushSide();

				BrushSides[i].Plane = InputFile.ReadInt32();
				BrushSides[i].Texture = InputFile.ReadInt32();
			}

			//============
			//Leaf Brushes
			//============
			InputFile.BaseStream.Seek(Lumps[(int)LumpType.LeafBrushes].Offset, SeekOrigin.Begin);
			for (int i=0; i<NumLeafBrushes; i++)
			{
				LeafBrushes[i] = InputFile.ReadInt32();
			}

			//=======
			//Shaders
			//=======
			int ShadersOffset = Lumps[(int)LumpType.Shaders].Offset;
			for (int i=0; i<NumShaders; i++)
			{
				InputFile.BaseStream.Seek(ShadersOffset + (i * BSPShader.SizeInBytes), SeekOrigin.Begin);
				
				Shaders[i] = new BSPShader();

				byte[] NameBytes = InputFile.ReadBytes(64);
				for (int NameByteIndex=0; NameByteIndex<64; NameByteIndex++)
				{
					if (NameBytes[NameByteIndex] != '\0')
					{
						Shaders[i].Name += Convert.ToChar(NameBytes[NameByteIndex]);
					}
				}

				Shaders[i].BrushIndex = InputFile.ReadInt32();
				Shaders[i].ContentFlags = InputFile.ReadInt32();
			}

			//===============
			//Visibility Data
			//===============
			InputFile.BaseStream.Seek(Lumps[(int)LumpType.VisData].Offset, SeekOrigin.Begin);

			if (Lumps[(int)LumpType.VisData].Length > 0)
			{
				Clusters.NumClusters = InputFile.ReadInt32();
				Clusters.BytesPerCluster = InputFile.ReadInt32();

				int ClusterSize = Clusters.NumClusters * Clusters.BytesPerCluster;
				Clusters.BitSets = InputFile.ReadBytes(ClusterSize);
			}
						
			//Finished
			InputFile.Close();
			
			//=============
			//No draw faces
			//=============

			//Eliminate no draw faces
			if (NoDrawTextureIndex != -1)
			{
				for (int i=0; i<Faces.Length; i++)
				{
					if (Faces[i].TextureID == NoDrawTextureIndex)
					{
						Faces[i] = null;
					}
				}
			}

			//Create bit array
			FacesDrawn = new BitArray(NumFaces, false);
		}

		#region Rendering Procedures

		public void RenderLevel(Vector3 CameraPosition, Frustum GameFrustrum)
		{					
			//Enable states
			//Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
			//Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
	
			FacesDrawn.SetAll(false);

			//Draw skybox			
			//RenderSkyBox(CameraPosition);

			//Get current leaf index
			int LeafIndex = FindLeaf(CameraPosition);

			//Get current cluster
			int ClusterIndex = Leaves[LeafIndex].Cluster;

			int i = NumLeaves;
			
			//Loop through all leaves and check visibility
			while (i > 0)
			{
				i--;

				BSPLeaf CurrentLeaf = Leaves[i];

				if ( IsClusterVisible(ClusterIndex, CurrentLeaf.Cluster) )
				{
                    if (GameFrustrum==null||GameFrustrum.BoxInFrustrum(CurrentLeaf.Min.X, CurrentLeaf.Min.Y, CurrentLeaf.Min.Z,
						CurrentLeaf.Max.X, CurrentLeaf.Max.Y, CurrentLeaf.Max.Z) )
					{
						int FaceCount = CurrentLeaf.NumLeafFaces;

						while (FaceCount > 0)
						{
							FaceCount--;
							int FaceIndex = LeafFaces[ CurrentLeaf.LeafFace + FaceCount ];

							if (Faces[FaceIndex] != null)
							{
								if (!FacesDrawn.Get(FaceIndex))
								{
									FacesDrawn.Set(FaceIndex, true);
									RenderFace(Faces[FaceIndex].Type, FaceIndex);
								}
							}
						}
					}
				}
			}

			//Disable blending
			//Gl.glDisable(Gl.GL_BLEND);

			//Disable states
			//Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
			//Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
		}
		/*
		private void RenderSkyBox(Vector3 CameraPosition)
		{
			//Setup
			Gl.glEnable(Gl.GL_TEXTURE_2D);
			Gl.glMatrixMode(Gl.GL_TEXTURE);
			Gl.glLoadIdentity();
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
			Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
			Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);
			Gl.glDisable(Gl.GL_BLEND);
			Gl.glDisable(Gl.GL_ALPHA_TEST);
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			Gl.glCullFace(Gl.GL_BACK);
			Gl.glDepthMask(Gl.GL_FALSE);

			//Save state
			Gl.glPushMatrix();
			
			//Set texture environment
			Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);

			//Draw in the following order: front, right, back, left, top, bottom
            
			//Front
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, SkyBoxTextures[2].TextureID);
			Gl.glBegin(Gl.GL_QUADS);
				Gl.glTexCoord2f(0, 0);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z - 10.0f);

				Gl.glTexCoord2f(1, 0);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z - 10.0f);
				
				Gl.glTexCoord2f(1, 1);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z - 10.0f);
					    
				Gl.glTexCoord2f(0, 1);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z - 10.0f);
			Gl.glEnd();

			//Right
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, SkyBoxTextures[0].TextureID);
				Gl.glBegin(Gl.GL_QUADS);
				Gl.glTexCoord2f(0, 0);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z - 10.0f);

				Gl.glTexCoord2f(1, 0);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z + 10.0f);
					
				Gl.glTexCoord2f(1, 1);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z + 10.0f);
						    
				Gl.glTexCoord2f(0, 1);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z - 10.0f);
			Gl.glEnd();

			//Back
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, SkyBoxTextures[5].TextureID);
			Gl.glBegin(Gl.GL_QUADS);
				Gl.glTexCoord2f(0, 0);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z + 10.0f);

				Gl.glTexCoord2f(1, 0);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z + 10.0f);
						
				Gl.glTexCoord2f(1, 1);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z + 10.0f);
							    
				Gl.glTexCoord2f(0, 1);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z + 10.0f);
			Gl.glEnd();

			//Left
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, SkyBoxTextures[3].TextureID);
			Gl.glBegin(Gl.GL_QUADS);
				Gl.glTexCoord2f(0, 0);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z + 10.0f);

				Gl.glTexCoord2f(1, 0);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z - 10.0f);
						
				Gl.glTexCoord2f(1, 1);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z - 10.0f);
							    
				Gl.glTexCoord2f(0, 1);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z + 10.0f);
			Gl.glEnd();

			//Top
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, SkyBoxTextures[4].TextureID);
			Gl.glBegin(Gl.GL_QUADS);
				Gl.glTexCoord2f(0, 0);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z - 10.0f);

				Gl.glTexCoord2f(1, 0);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z - 10.0f);
							
				Gl.glTexCoord2f(1, 1);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z + 10.0f);
								    
				Gl.glTexCoord2f(0, 1);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y + 10.0f,
					CameraPosition.Z + 10.0f);
			Gl.glEnd();

			//Bottom
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, SkyBoxTextures[1].TextureID);
			Gl.glBegin(Gl.GL_QUADS);
				Gl.glTexCoord2f(0, 0);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z + 10.0f);

				Gl.glTexCoord2f(1, 0);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z + 10.0f);
							
				Gl.glTexCoord2f(1, 1);
				Gl.glVertex3f(CameraPosition.X + 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z - 10.0f);
								    
				Gl.glTexCoord2f(0, 1);
				Gl.glVertex3f(CameraPosition.X - 10.0f, CameraPosition.Y - 10.0f,
					CameraPosition.Z - 10.0f);
			Gl.glEnd();
			
			//Restore state
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glDepthMask(Gl.GL_TRUE);    
			Gl.glCullFace(Gl.GL_FRONT);
			Gl.glPopMatrix();
		}
        */
		private void RenderFace(int BSPFaceType, int FaceIndex)
		{
			switch(BSPFaceType)
			{
				case (int)FaceType.Polygon:
				case (int)FaceType.MeshFace:
					RenderPolygonFace(FaceIndex);
					break;
			}
		}

        Cheetah.Graphics.VertexBuffer VertexBuffer;
        Cheetah.Graphics.IndexBuffer IndexBuffer;
        private void BuildBuffers()
        {
            if (Vertices.Length/3 != TextureCoords.Length/2)
                throw new Exception("Vertices.Length!=TextureCoords.Length");
            int c = Vertices.Length / 3;
            VertexP3T2[] data = new VertexP3T2[Vertices.Length];
            for (int i = 0; i < c; ++i)
            {
                data[i].position = new Vector3(Vertices[i*3],Vertices[i*3+1],Vertices[i*3+2]);
                data[i].texture0 = new Vector2(TextureCoords[i*2],TextureCoords[i*2+1]);
            }
            VertexBuffer = Root.Instance.UserInterface.Renderer.CreateStaticVertexBuffer(data, Vertices.Length * VertexFormat.VF_P3T2.Size);
            VertexBuffer.Format = VertexFormat.VF_P3T2;

            IndexBuffer = new IndexBuffer();
            IndexBuffer.buffer = new int[MeshIndices.Length];
            for (int i = 0; i < MeshIndices.Length; ++i)
                IndexBuffer.buffer[i] = (int)MeshIndices[i];

        }
        private unsafe void RenderPolygonFace(int FaceIndex)
        {
            BSPFace CurrentFace = Faces[FaceIndex];

            //if (Textures[CurrentFace.TextureID] != null)
            {
                //Vertices
                /*fixed (void* VertexPtr = &Vertices[CurrentFace.StartVertexIndex * 3])
                {
                    Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, new IntPtr(VertexPtr));
                }
                
				if (CurrentFace.LightmapID >= 0)
				{
					if (Lightmaps[CurrentFace.LightmapID] != null)
					{
						//Lightmap
						//Gl.glBindTexture(Gl.GL_TEXTURE_2D, Lightmaps[CurrentFace.LightmapID].TextureID);
                        Root.Instance.UserInterface.Renderer.BindTexture(Lightmaps[CurrentFace.LightmapID].Id);
						
						//Setup blending
						Gl.glDisable(Gl.GL_BLEND);
						Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);
						
						//Set texture coordinates
						fixed (void* LightmapCoordPtr = &LightmapCoords[CurrentFace.StartVertexIndex * 2])
						{
							Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, new IntPtr(LightmapCoordPtr));
						}
					
						//Draw face
						fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
						{
							Gl.glDrawElements(Gl.GL_TRIANGLES, CurrentFace.NumMeshVertices,
								Gl.GL_UNSIGNED_INT, new IntPtr(IndexPtr));
						}
					}
				}
                */
                //Decal texture				
                //Gl.glBindTexture(Gl.GL_TEXTURE_2D, Textures[CurrentFace.TextureID].TextureID);
                if (Textures[CurrentFace.TextureID] != null)
                    Root.Instance.UserInterface.Renderer.BindTexture(Textures[CurrentFace.TextureID].Id);
                else
                    Root.Instance.UserInterface.Renderer.BindTexture(null);

                //Setup blending
                //Gl.glEnable(Gl.GL_BLEND);
                //Gl.glBlendFunc(Gl.GL_DST_COLOR, Gl.GL_SRC_COLOR);
                //Gl.glBlendFunc(Gl.GL_ONE, Gl.GL_ONE);
                //Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);
                
                //Set texture coordinates
                /*fixed (void* TexCoordPtr = &TextureCoords[CurrentFace.StartVertexIndex * 2])
                {
                    Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, new IntPtr(TexCoordPtr));
                }

                //Draw face
                fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
                {
                    Gl.glDrawElements(Gl.GL_TRIANGLES, CurrentFace.NumMeshVertices,
                        Gl.GL_UNSIGNED_INT, new IntPtr(IndexPtr));
                }*/

                Root.Instance.UserInterface.Renderer.Draw(VertexBuffer, PrimitiveType.TRIANGLES, CurrentFace.StartVertexIndex, CurrentFace.NumMeshVertices, IndexBuffer, CurrentFace.MeshVertexIndex);
            }
        }

        #region Multi-Texture Code

        /*
		public void RenderLevel(Vector3 CameraPosition, Frustrum GameFrustrum)
		{						
			//Enable states
			Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
			Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

			Gl.glClientActiveTextureARB(glClientActiveTextureARBPtr, Gl.GL_TEXTURE1_ARB);
			Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
			
			FacesDrawn.SetAll(false);

			//Draw skybox
			RenderSkyBox(CameraPosition);

			//Get current leaf index
			int LeafIndex = FindLeaf(CameraPosition);

			//Get current cluster
			int ClusterIndex = Leaves[LeafIndex].Cluster;

			int i = NumLeaves;
			
			//Loop through all leaves and check visibility
			while (i > 0)
			{
				i--;

				BSPLeaf CurrentLeaf = Leaves[i];

				if ( IsClusterVisible(ClusterIndex, CurrentLeaf.Cluster) )
				{
					if ( GameFrustrum.BoxInFrustrum(CurrentLeaf.Min.X, CurrentLeaf.Min.Y, CurrentLeaf.Min.Z,
						CurrentLeaf.Max.X, CurrentLeaf.Max.Y, CurrentLeaf.Max.Z) )
					{
						int FaceCount = CurrentLeaf.NumLeafFaces;

						while (FaceCount > 0)
						{
							FaceCount--;
							int FaceIndex = LeafFaces[ CurrentLeaf.LeafFace + FaceCount ];

							if (Faces[FaceIndex] != null)
							{
								if (!FacesDrawn.Get(FaceIndex))
								{
									FacesDrawn.Set(FaceIndex, true);
									RenderFace(Faces[FaceIndex].Type, FaceIndex);
								}
							}
						}
					}
				}
			}

			//Disable blending
			Gl.glDisable(Gl.GL_BLEND);

			//Disable states
			Gl.glClientActiveTextureARB(glClientActiveTextureARBPtr, Gl.GL_TEXTURE1_ARB);
			Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

			Gl.glActiveTextureARB(glActiveTextureARBPtr, Gl.GL_TEXTURE1_ARB);
			Gl.glDisable(Gl.GL_TEXTURE_2D);

			Gl.glClientActiveTextureARB(glClientActiveTextureARBPtr, Gl.GL_TEXTURE0_ARB);			
			Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
			Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);

			Gl.glActiveTextureARB(glActiveTextureARBPtr, Gl.GL_TEXTURE0_ARB);
		}
		
		private unsafe void RenderPolygonFace(int FaceIndex)
		{
			BSPFace CurrentFace = Faces[FaceIndex];

			if (Textures[ CurrentFace.TextureID ] != null)
			{	
				//Vertices
				fixed (void* VertexPtr = &Vertices[CurrentFace.StartVertexIndex * 3])
				{						
					Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, VertexPtr);
				}

				//Decal texture				
				Gl.glActiveTextureARB(glActiveTextureARBPtr, Gl.GL_TEXTURE0_ARB);
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, Textures[CurrentFace.TextureID].TextureID);
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);

				//Set texture coordinates
				Gl.glClientActiveTextureARB(glClientActiveTextureARBPtr, Gl.GL_TEXTURE0_ARB);
				fixed (void* TexCoordPtr = &TextureCoords[CurrentFace.StartVertexIndex * 2])
				{
					Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, TexCoordPtr);
				}

				if (CurrentFace.LightmapID >= 0)
				{
					if (Lightmaps[CurrentFace.LightmapID] != null)
					{
						//Lightmap
						Gl.glActiveTextureARB(glActiveTextureARBPtr, Gl.GL_TEXTURE1_ARB);
						Gl.glEnable(Gl.GL_TEXTURE_2D);
						Gl.glBindTexture(Gl.GL_TEXTURE_2D, Lightmaps[CurrentFace.LightmapID].TextureID);
						
						//Setup blending
						Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
						
						//Set texture coordinates
						Gl.glClientActiveTextureARB(glClientActiveTextureARBPtr, Gl.GL_TEXTURE1_ARB);
						fixed (void* LightmapCoordPtr = &LightmapCoords[CurrentFace.StartVertexIndex * 2])
						{
							Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, LightmapCoordPtr);
						}					
					}
				}

				//Draw face
				fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
				{
					Gl.glDrawElements(Gl.GL_TRIANGLES, CurrentFace.NumMeshVertices,
						Gl.GL_UNSIGNED_INT, IndexPtr);
				}
			}
		}
		*/

        #endregion

        #endregion

        #region Collision Detection Procedures

        public void DetectCollisionRay(Vector3 Start, Vector3 End)
        {
            CollisionType = CollisionTypes.Ray;
            CollisionOffset = 0.0f;
            DetectCollision(Start, End);
        }

        public void DetectCollisionSphere(Vector3 Start, Vector3 End, float Radius)
        {
            CollisionType = CollisionTypes.Sphere;
            CollisionOffset = Radius;
            DetectCollision(Start, End);
        }

        public void DetectCollisionBox(Vector3 Start, Vector3 End, Vector3 Min, Vector3 Max)
        {
            CollisionType = CollisionTypes.Box;
            CollisionMin = Min;
            CollisionMax = Max;

            CollisionExtents.X = (-Min.X > Max.X) ? -Min.X : Max.X;
            CollisionExtents.Y = (-Min.Y > Max.Y) ? -Min.Y : Max.Y;
            CollisionExtents.Z = (-Min.Z > Max.Z) ? -Min.Z : Max.Z;

            DetectCollision(Start, End);
        }

        private void DetectCollision(Vector3 Start, Vector3 End)
        {
            CollisionInfo = new CollisionInformation();

            //Set global values
            CollisionStart = new Vector3(Start.X, Start.Y, Start.Z);
            CollisionEnd = new Vector3(End.X, End.Y, End.Z);

            CheckNode(0, 0.0f, 1.0f, Start, End);

            if (CollisionInfo.Fraction == 1.0f)
            {
                CollisionInfo.EndPoint = CollisionEnd;
            }
            else
            {
                CollisionInfo.EndPoint = CollisionStart +
                    ((CollisionEnd - CollisionStart) * CollisionInfo.Fraction);
            }
        }

        private void CheckNode(int NodeIndex, float StartFraction, float EndFraction,
			Vector3 Start, Vector3 End)
        {
            if (CollisionInfo.Fraction <= StartFraction)
            {
                return;
            }

            //Check if the node is a leaf
            if (NodeIndex < 0)
            {
                BSPLeaf CurrentLeaf = Leaves[~NodeIndex];

                for (int i = 0; i < CurrentLeaf.NumLeafBrushes; i++)
                {
                    BSPBrush CurrentBrush = Brushes[LeafBrushes[CurrentLeaf.LeafBrush + i]];

                    if ((CurrentBrush.NumSides > 0) && ((LoadTextures[CurrentBrush.TextureIndex].Contents & 1) > 0))
                    {
                        CheckBrush(CurrentBrush);
                    }
                }

                return;
            }

            BSPNode CurrentNode = Nodes[NodeIndex];
            BSPPlane CurrentPlane = Planes[CurrentNode.Plane];

            //Get distance from each point to the node's plane
            float StartDistance = (CurrentPlane.Normal.X * Start.X) + (CurrentPlane.Normal.Y * Start.Y)
                + (CurrentPlane.Normal.Z * Start.Z) - CurrentPlane.Distance;

            float EndDistance = (CurrentPlane.Normal.X * End.X) + (CurrentPlane.Normal.Y * End.Y)
                + (CurrentPlane.Normal.Z * End.Z) - CurrentPlane.Distance;

            //Calculate offest
            if (CollisionType == CollisionTypes.Box)
            {
                CollisionOffset = Math.Abs(CollisionExtents.X * CurrentPlane.Normal.X) +
                    Math.Abs(CollisionExtents.Y * CurrentPlane.Normal.Y) +
                    Math.Abs(CollisionExtents.Z * CurrentPlane.Normal.Z);
            }

            if ((StartDistance >= CollisionOffset) && (EndDistance >= CollisionOffset))
            {
                //Points are in front of plane
                CheckNode(CurrentNode.Front, StartFraction, EndFraction, Start, End);
            }
            else if ((StartDistance < -CollisionOffset) && (EndDistance < -CollisionOffset))
            {
                //Points are behind plane
                CheckNode(CurrentNode.Back, StartFraction, EndFraction, Start, End);
            }
            else
            {
                int Side1 = -1, Side2 = -1;
                float Fraction1 = 0.0f, Fraction2 = 0.0f;
                Vector3 Middle = new Vector3();

                //Split segment
                if (StartDistance < EndDistance)
                {
                    Side1 = CurrentNode.Back;
                    Side2 = CurrentNode.Front;
                    float InverseDistance = 1.0f / (StartDistance - EndDistance);
                    Fraction1 = (StartDistance - QuakeEpsilon - CollisionOffset) * InverseDistance;
                    Fraction2 = (StartDistance + QuakeEpsilon + CollisionOffset) * InverseDistance;
                }
                else if (EndDistance < StartDistance)
                {
                    Side1 = CurrentNode.Front;
                    Side2 = CurrentNode.Back;
                    float InverseDistance = 1.0f / (StartDistance - EndDistance);
                    Fraction1 = (StartDistance + QuakeEpsilon + CollisionOffset) * InverseDistance;
                    Fraction2 = (StartDistance - QuakeEpsilon - CollisionOffset) * InverseDistance;
                }
                else
                {
                    Side1 = CurrentNode.Front;
                    Side2 = CurrentNode.Back;
                    Fraction1 = 1.0f;
                    Fraction2 = 0.0f;
                }

                //Validate numbers
                if (Fraction1 < 0.0f)
                {
                    Fraction1 = 0.0f;
                }
                else if (Fraction1 > 1.0f)
                {
                    Fraction1 = 1.0f;
                }

                if (Fraction2 < 0.0f)
                {
                    Fraction2 = 0.0f;
                }
                else if (Fraction2 > 1.0f)
                {
                    Fraction2 = 1.0f;
                }

                //Do first side
                Middle = Start + ((End - Start) * Fraction1);
                float MiddleFraction = StartFraction + ((EndFraction - StartFraction) * Fraction1);
                CheckNode(Side1, StartFraction, MiddleFraction, Start, Middle);

                //Do second side
                Middle = Start + ((End - Start) * Fraction2);
                MiddleFraction = StartFraction + ((EndFraction - StartFraction) * Fraction2);
                CheckNode(Side2, MiddleFraction, EndFraction, Middle, End);
            }
        }

        private void CheckBrush(BSPBrush CurrentBrush)
        {
            float StartFraction = -1.0f;
            float EndFraction = 1.0f;
            bool StartsOut = false;
            bool EndsOut = false;
            float PlaneDistance = 0.0f;

            Vector3 CanidateNormal = new Vector3();

            //Loop through brush sides
            for (int i = 0; i < CurrentBrush.NumSides; i++)
            {
                BSPBrushSide CurrentSide = BrushSides[CurrentBrush.FirstSide + i];
                BSPPlane CurrentPlane = Planes[CurrentSide.Plane];

                //Compute distances from intitial vectors
                float StartDistance = 0.0f, EndDistance = 0.0f;

                if (CollisionType == CollisionTypes.Box)
                {
                    Vector3 VOffset = new Vector3();

                    if (CurrentPlane.Normal.X < 0)
                    {
                        VOffset.X = CollisionMax.X;
                    }
                    else
                    {
                        VOffset.X = CollisionMin.X;
                    }

                    if (CurrentPlane.Normal.Y < 0)
                    {
                        VOffset.Y = CollisionMax.Y;
                    }
                    else
                    {
                        VOffset.Y = CollisionMin.Y;
                    }


                    if (CurrentPlane.Normal.Z < 0)
                    {
                        VOffset.Z = CollisionMax.Z;
                    }
                    else
                    {
                        VOffset.Z = CollisionMin.Z;
                    }

                    StartDistance = (((CollisionStart.X + VOffset.X) * CurrentPlane.Normal.X) +
                        ((CollisionStart.Y + VOffset.Y) * CurrentPlane.Normal.Y) +
                        ((CollisionStart.Z + VOffset.Z) * CurrentPlane.Normal.Z)) - CurrentPlane.Distance;

                    EndDistance = (((CollisionEnd.X + VOffset.X) * CurrentPlane.Normal.X) +
                        ((CollisionEnd.Y + VOffset.Y) * CurrentPlane.Normal.Y) +
                        ((CollisionEnd.Z + VOffset.Z) * CurrentPlane.Normal.Z)) - CurrentPlane.Distance;
                }
                else
                {
                    //Ray and sphere
                    StartDistance = (CurrentPlane.Normal.X * CollisionStart.X) + (CurrentPlane.Normal.Y * CollisionStart.Y)
                        + (CurrentPlane.Normal.Z * CollisionStart.Z) - CurrentPlane.Distance - CollisionOffset;

                    EndDistance = (CurrentPlane.Normal.X * CollisionEnd.X) + (CurrentPlane.Normal.Y * CollisionEnd.Y)
                        + (CurrentPlane.Normal.Z * CollisionEnd.Z) - CurrentPlane.Distance - CollisionOffset;
                }

                if (StartDistance > 0)
                {
                    StartsOut = true;
                }

                if (EndDistance > 0)
                {
                    EndsOut = true;
                }

                //Outside of brush
                if ((StartDistance > 0) && (EndDistance > 0))
                {
                    return;
                }

                //Will be clipped by another side
                if ((StartDistance <= 0) && (EndDistance <= 0))
                {
                    continue;
                }

                if (StartDistance > EndDistance)
                {
                    float Fraction = (StartDistance - QuakeEpsilon) / (StartDistance - EndDistance);
                    if (Fraction > StartFraction)
                    {
                        StartFraction = Fraction;
                        CanidateNormal = CurrentPlane.Normal;
                        PlaneDistance = CurrentPlane.Distance;
                    }
                }
                else
                {
                    float Fraction = (StartDistance + QuakeEpsilon) / (StartDistance - EndDistance);
                    if (Fraction < EndFraction)
                    {
                        EndFraction = Fraction;
                    }
                }
            }

            //Done checking sides
            if (StartsOut == false)
            {
                CollisionInfo.StartsOut = false;

                if (EndsOut == false)
                {
                    CollisionInfo.AllSolid = true;
                }

                return;
            }

            if (StartFraction < EndFraction)
            {
                if ((StartFraction > -1) && (StartFraction < CollisionInfo.Fraction))
                {
                    if (StartFraction < 0)
                    {
                        StartFraction = 0;
                    }

                    CollisionInfo.Fraction = StartFraction;
                    CollisionInfo.Normal = CanidateNormal;
                    CollisionInfo.PlaneDistance = PlaneDistance;
                }
            }
        }

        public Trigger DetectTriggerCollisions(Vector3 Position)
        {
            Trigger ReturnTrigger = null;

            for (int i = 1; i < Models.Length; i++)
            {
                BSPModel CurrentModel = Models[i];

                if (PointInBox(Position, CurrentModel.Mins, CurrentModel.Maxes))
                {
                    try
                    {
                        ReturnTrigger = (Trigger)IndexTriggerHash[i];
                        break;
                    }
                    catch
                    {
                        //Do nothing
                    }
                }
            }

            return (ReturnTrigger);
        }

        private bool PointInBox(Vector3 Position, float[] Mins, float[] Maxes)
        {
            bool ReturnStatus = false;

            if ((Position.X >= Mins[0]) && (Position.X <= Maxes[0]) &&
                (Position.Y >= Mins[1]) && (Position.Y <= Maxes[1]) &&
                (Position.Z >= Mins[2]) && (Position.Z <= Maxes[2]))
            {
                ReturnStatus = true;
            }

            return (ReturnStatus);
        }

        private bool BoxesCollide(float[] Mins, float[] Maxes,
			float[] Mins2, float[] Maxes2)
        {
            bool ReturnStatus = false;

            if ((Maxes2[0] > Mins[0]) && (Mins2[0] < Maxes[0]) &&
                (Maxes2[1] > Mins[1]) && (Mins2[1] < Maxes[1]) &&
                (Maxes2[2] > Mins[2]) && (Mins2[2] < Maxes[2]))
            {
                ReturnStatus = true;
            }

            return (ReturnStatus);
        }

        #endregion

        #region Auxiliary Procedures

        private bool IsClusterVisible(int CurrentCluster, int TestCluster)
        {
            bool ReturnStatus = true;

            if ((Clusters.BitSets != null) && (CurrentCluster >= 0) && (TestCluster >= 0))
            {
                byte VisSet = Clusters.BitSets[(CurrentCluster * Clusters.BytesPerCluster) + (TestCluster / 8)];
                int Result = VisSet & (1 << ((TestCluster) & 7));

                if (Result <= 0)
                {
                    ReturnStatus = false;
                }
            }

            return (ReturnStatus);
        }

        private int FindLeaf(Vector3 CameraPosition)
        {
            int NodeIndex = 0;
            float Distance = 0.0f;

            while (NodeIndex >= 0)
            {
                BSPNode CurrentNode = Nodes[NodeIndex];
                BSPPlane CurrentPlane = Planes[CurrentNode.Plane];

                //Calcuate distance
                Distance = (((CurrentPlane.Normal.X * CameraPosition.X) +
                    (CurrentPlane.Normal.Y * CameraPosition.Y) +
                    (CurrentPlane.Normal.Z * CameraPosition.Z)) - CurrentPlane.Distance);

                if (Distance >= 0)
                {
                    NodeIndex = CurrentNode.Front;
                }
                else
                {
                    NodeIndex = CurrentNode.Back;
                }
            }

            return (~NodeIndex);
        }

        public static void ChangeGamma(ref Bitmap ImageBmp, float Factor)
        {
            for (int x = 0; x < ImageBmp.Width; x++)
            {
                for (int y = 0; y < ImageBmp.Height; y++)
                {
                    float Scale = 1.0f;
                    float TempFloat = 0.0f;

                    Color Pixel = ImageBmp.GetPixel(x, y);
                    float R = (float)Pixel.R;
                    float G = (float)Pixel.G;
                    float B = (float)Pixel.B;

                    R = R * Factor / 255.0f;
                    G = G * Factor / 255.0f;
                    B = B * Factor / 255.0f;

                    if (R > 1.0f)
                    {
                        TempFloat = 1.0f / R;

                        if (TempFloat < Scale)
                        {
                            Scale = TempFloat;
                        }
                    }

                    if (G > 1.0f)
                    {
                        TempFloat = 1.0f / G;

                        if (TempFloat < Scale)
                        {
                            Scale = TempFloat;
                        }
                    }

                    if (B > 1.0f)
                    {
                        TempFloat = 1.0f / B;

                        if (TempFloat < Scale)
                        {
                            Scale = TempFloat;
                        }
                    }

                    Scale *= 255.0f;
                    R *= Scale;
                    G *= Scale;
                    B *= Scale;

                    ImageBmp.SetPixel(x, y, Color.FromArgb((int)R, (int)G, (int)B));
                }
            }
        }

        #endregion
    }
}
