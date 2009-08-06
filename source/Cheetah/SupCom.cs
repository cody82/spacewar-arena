using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace SupCom
{
    // SScaFileHeader -- The header for .SCA files.
    //
    public struct SScaFileHeader
    {
        public static SScaFileHeader Read(BinaryReader br)
        {
            SScaFileHeader v;

            v.mMagic = br.ReadUInt32();
            v.mVersion = br.ReadUInt32();
            v.mNumFrames = br.ReadUInt32();
            v.mDuration = br.ReadUInt32();
            v.mNumBones = br.ReadUInt32();
            v.mBoneNamesOffset = br.ReadUInt32();
            v.mBoneLinksOffset = br.ReadUInt32();
            v.mFirstFrameOffset = br.ReadUInt32();
            v.mFrameSize = br.ReadUInt32();

            return v;
        }
        // The FOURCC 'ANIM'
        public uint mMagic;

        // The .SCA version number.
        public uint mVersion;

        // The number of frames in this animation.
        public uint mNumFrames;

        // The duration (in seconds) of this animation.  
        // The animation plays at (mNumFrames-1)/mDuration frames per second.
        public uint mDuration;

        // The number of bones in this animation.
        public uint mNumBones;

        // Offset of the bone names (SScaFileBoneNames[0])
        public uint mBoneNamesOffset;

        // Offset of the bone link info (SScaFileBoneLinks[0])
        public uint mBoneLinksOffset;

        // Offset of the actual animation data (SScaFileAnimData[0])
        public uint mFirstFrameOffset;

        // The number of bytes in one animation frame.
        public uint mFrameSize;
    }


    // SScaFileBoneNames -- The bone names used in this animation.
    //
    /*public struct SScaFileBoneNames
    {
        // Array of bone names.  There are header.mNumBones NUL terminated
        // strings concatenated together starting here.
        char[] mBoneNameData; //(array size is mNumBones)
    };

    // SScaFileBoneLinks -- The parent links for the bones used in this animation.
    //
    public struct SScaFileBoneLinks
    {
        // Array of bone indices. 
        public uint[] mParentBoneIndex; //(array size is mNumBones)
    };*/

    // SScaFileBoneKeyframe -- The data for single bone at a single key
    // frame.
    //
    public struct SScaFileBoneKeyframe
    {
        public static SScaFileBoneKeyframe Read(BinaryReader br)
        {
            SScaFileBoneKeyframe f;

            f.mPosition = Vector3.Read(br);
            f.mRotation = Vector4.Read(br);

            return f;
        }

        // Position relative to the parent bone.
        // Vector (x,y,z)
        public Vector3 mPosition;//3

        // Rotation relative to the parent bone.
        // Quaternion (w,x,y,z)
        public Vector4 mRotation;//4
    };

    // SScaFileKeyframe -- The data about a single key frame.
    //
    public struct SScaFileKeyframe
    {
        public static SScaFileKeyframe Read(BinaryReader br, int numbones)
        {
            SScaFileKeyframe f;

            f.mTime = br.ReadSingle();
            f.mFlags = br.ReadUInt32();

            f.mBones = new SScaFileBoneKeyframe[numbones];
            for (int i = 0; i < numbones; ++i)
            {
                f.mBones[i] = SScaFileBoneKeyframe.Read(br);
            }

            return f;
        }
        
        // The time (in seconds) of this keyframe.
        public float mTime;

        // Various flags.  None defined yet.
        public uint mFlags;

        // Array of keyframe data for each bone.
        public SScaFileBoneKeyframe[] mBones;  //(array size is mNumBones)
    };

    // SScaFileAnimData -- The actual animation data.
    //
    public struct SScaFileAnimData
    {
        public static SScaFileAnimData Read(BinaryReader br, int numbones, int numframes)
        {
            SScaFileAnimData f;

            f.mPositionDelta = Vector3.Read(br);
            f.mOrientDelta = Vector4.Read(br);

            f.mFrames = new SScaFileKeyframe[numframes];
            for (int i = 0; i < numframes; ++i)
            {
                f.mFrames[i] = SScaFileKeyframe.Read(br,numbones);
            }

            return f;
        }
        
        // The total position delta between the first frame and the last frame.
        // Vector (x,y,z)
        public Vector3 mPositionDelta;//3

        // The total orientation delta between the first frame and the last frame.
        // Quaternion (w,x,y,z)
        public Vector4 mOrientDelta;//4

        // The per-frame data.
        public SScaFileKeyframe[] mFrames;  //(array size is mNumFrames)
    };


    // SCM VERSION 5 DATA LAYOUT

    // Multi-byte data is writting in little-endian ("Intel") format 

    // There are 5 required and 2 optional sections in an SCM file, each indicated by a leading FOURCC code:

    // FOURCC  |  Contents
    // --------+------------------------
    // 'MODL'  | Header info
    // 'NAME'  | List of bone name strings
    // 'SKEL'  | Array of bone data
    // 'VTXL'  | Array of basic vertex data
    // 'TRIS'  | Array of triangle indices
    // 'VEXT'  | Array of extra vertex data (OPTIONAL SECTION)
    // 'INFO'  | List of null terminated information strings (OPTIONAL SECTION)

    // Section offsets in the file header point to the start of the data for that section (ie, the first byte AFTER
    // the section's identifying FOURCC) Padding characters are added to the end of each section to ensure that 
    // the next section is 16-byte aligned. Ommitted sections are indicated by an offset of 0. 

    // *** All offsets are relative to the start of the file ***

    //
    //
    public class ScmHeader
    {
        public ScmHeader(BinaryReader br)
        {
            Read(br);
        }
        public void Read(BinaryReader br)
        {
            mMagic = br.ReadUInt32();
            mVersion = br.ReadUInt32();
            mBoneOffset = br.ReadUInt32();
            mWeightedBoneCount = br.ReadUInt32();
            mVertexOffset = br.ReadUInt32();
            mVertexExtraOffset = br.ReadUInt32();
            mVertexCount = br.ReadUInt32();
            mIndexOffset = br.ReadUInt32();
            mIndexCount = br.ReadUInt32();
            mInfoOffset = br.ReadUInt32();
            mInfoCount = br.ReadUInt32();
            mTotalBoneCount = br.ReadUInt32();
        }

       // The FOURCC 'MODL'
        public uint mMagic;

        // The .SCM version number
        public uint mVersion;

        // Offset to SCM_BoneData[0]
        public uint mBoneOffset;

        // Number of elements in SCM_BoneData that actually influence verts (no reference points)
        public uint mWeightedBoneCount;

        // Offset of basic vertex data section (SCM_VertData[0])
        public uint mVertexOffset;

        // Offset of extra vertex data section (SCM_VertExtraData[0]) 
        // Contains additional per-vertex information. *** Currently unused (and omitted) in SupCom 1.0 ***
        public uint mVertexExtraOffset;

        // Number of elements in the SCM_VertData array
        // (and the SCM_VertExtraData array, if mVertexExtraOffset != 0)
        public uint mVertexCount;

        // Offset of the triangle index section (SCM_TriangleData[0])
        public uint mIndexOffset;

        // Number of elements in the SCM_TriangleData array
        public uint mIndexCount;

        // Offset of information section (SCM_InfoData[0])
        public uint mInfoOffset;

        // Number of elements in the SCM_InfoData list
        public uint mInfoCount;

        // Number of elements in the SCM_BoneData array (including 'reference point' bones)
        public uint mTotalBoneCount;
    };

    //
    //
    public struct ScmBoneData
    {
        public static ScmBoneData Read(BinaryReader br)
        {
            ScmBoneData v;

            v.mRestPoseInverse = new float[4, 4];
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    v.mRestPoseInverse[x, y] = br.ReadSingle();
                }
            }
            v.mPosition = Vector3.Read(br);
            v.mRotation = Vector4.Read(br);

            v.mNameOffset = br.ReadUInt32();
            {
                long pos = br.BaseStream.Position;
                br.BaseStream.Seek(v.mNameOffset, SeekOrigin.Begin);
                char c;
                v.BoneName = "";
                while ((c = br.ReadChar()) != '\0')
                {
                    v.BoneName += c;
                }
                br.BaseStream.Seek(pos, SeekOrigin.Begin);
            }

            v.mParentBoneIndex = br.ReadUInt32();


            v.RESERVED_0 = br.ReadUInt32();
            v.RESERVED_1 = br.ReadUInt32();

            return v;
        }
        // Inverse transform of the bone relative to the local origin of the mesh
        // 4x4 Matrix with row major (i.e. D3D default ordering)
        public float[,] mRestPoseInverse;//44

        // Position relative to the parent bone.
        // Vector (x,y,z)
        public Vector3 mPosition;//3

        // Rotation relative to the parent bone.
        // Quaternion (w,x,y,z)
        public Vector4 mRotation;//4

        // Offset of the bone's name string
        public uint mNameOffset;
        public string BoneName;

        // Index of the bone's parent in the SCM_BoneData array
        public uint mParentBoneIndex;
        public bool HasParent
        {
            get
            {
                return mParentBoneIndex != uint.MaxValue;
            }
        }

        public uint RESERVED_0;
        public uint RESERVED_1;
    };

    public struct Vector3
    {
        public static Vector3 Read(BinaryReader br)
        {
            Vector3 v;

            v.X = br.ReadSingle();
            v.Y = br.ReadSingle();
            v.Z = br.ReadSingle();

            return v;
        }

        public float X;
        public float Y;
        public float Z;
    }
    public struct Vector4
    {
        public static Vector4 Read(BinaryReader br)
        {
            Vector4 v;

            v.W = br.ReadSingle();
            v.X = br.ReadSingle();
            v.Y = br.ReadSingle();
            v.Z = br.ReadSingle();

            return v;
        }

        public float X;
        public float Y;
        public float Z;
        public float W;
    }

    public struct Vector2
    {
        public static Vector2 Read(BinaryReader br)
        {
            Vector2 v;

            v.X = br.ReadSingle();
            v.Y = br.ReadSingle();

            return v;
        }
        
        public float X;
        public float Y;
    }
    //
    //
    public struct ScmVertData
    {
        public static ScmVertData Read(BinaryReader br)
        {
            ScmVertData v;

            v.mPosition = Vector3.Read(br);
            v.mNormal = Vector3.Read(br);
            v.mTangent = Vector3.Read(br);
            v.mBinormal = Vector3.Read(br);
            v.mUV0 = Vector2.Read(br);
            v.mUV1 = Vector2.Read(br);
            v.mBoneIndex0 = br.ReadByte();
            v.mBoneIndex1 = br.ReadByte();
            v.mBoneIndex2 = br.ReadByte();
            v.mBoneIndex3 = br.ReadByte();

            return v;
        }

        // Position of the vertex relative to the local origin of the mesh
        public Vector3 mPosition;//3

        // 'Tangent Space' normal, tangent & binormal unit vectors
        public Vector3 mNormal;//3
        public Vector3 mTangent;//3
        public Vector3 mBinormal;//3

        // Two sets of UV coordinates 
        public Vector2 mUV0;//2
        public Vector2 mUV1;//2

        // Up to 4-bone skinning can be supported using additional 
        // indices (in conjunction with bone weights in the optional VertexExtra array)
        // Skinned meshes are not used in SupCom 1.0 so only mBoneIndex[0] is expected
        // to contain a valid index.
        public byte mBoneIndex0; //4
        public byte mBoneIndex1; //4
        public byte mBoneIndex2; //4
        public byte mBoneIndex3; //4
    };

    public struct ScmTriangleData
    {
        public static ScmTriangleData Read(BinaryReader br)
        {
            ScmTriangleData v;

            v.Index0 = br.ReadUInt16();
            v.Index1 = br.ReadUInt16();
            v.Index2 = br.ReadUInt16();

            return v;
        }
        
        public ushort Index0;//3
        public ushort Index1;//3
        public ushort Index2;//3
    };


    public class ScaFile
    {
        public ScaFile(Stream file)
        {
            Load(file);
        }

        public void Load(Stream file)
        {
            using (BinaryReader br = new BinaryReader(file, Encoding.ASCII))
            {
                Load(br);
            }
        }
        public void Load(BinaryReader br)
        {
            Header = SScaFileHeader.Read(br);
            BoneNames = ReadBoneNames(br);
            ReadBoneLinks(br);
            ReadAnimData(br);
        }

        void ReadBoneLinks(BinaryReader br)
        {
            br.BaseStream.Seek(Header.mBoneLinksOffset, SeekOrigin.Begin);
            BoneLinks = new uint[Header.mNumBones];

            for (int i = 0; i < BoneLinks.Length; ++i)
            {
                BoneLinks[i] = br.ReadUInt32();
            }
       }

        void ReadAnimData(BinaryReader br)
        {
            br.BaseStream.Seek(Header.mFirstFrameOffset, SeekOrigin.Begin);
            AnimData = SScaFileAnimData.Read(br, (int)Header.mNumBones, (int)Header.mNumFrames);
        }

        string[] ReadBoneNames(BinaryReader br)
        {
            br.BaseStream.Seek(Header.mBoneNamesOffset, SeekOrigin.Begin);
            int count = (int)Header.mNumBones;

            string[] names = new string[count];

            for (int i = 0; i < count; ++i)
            {
                char c;
                while ((c = br.ReadChar()) != '\0')
                {
                    names[i] += c;
                }

            }
            return names;
        }

        public SScaFileHeader Header;
        public string[] BoneNames;
        public uint[] BoneLinks;
        public SScaFileAnimData AnimData;
    }

    public class ScmFile
    {
        public ScmFile(Stream file)
        {
            Load(file);
        }

        public void Load(Stream file)
        {
            using (BinaryReader br = new BinaryReader(file,Encoding.ASCII))
            {
                Load(br);
            }
        }

        string ReadFourCC(BinaryReader br)
        {
            return Encoding.ASCII.GetString(br.ReadBytes(4));
        }

        string[] ReadStrings(BinaryReader br, int count)
        {
            string[] names = new string[count];

            for (int i = 0; i < count; ++i)
            {
                char c;
                while((c = br.ReadChar())!='\0')
                {
                    names[i] += c;
                }

            }
            return names;
        }

        ScmVertData[] ReadVertexData(BinaryReader br, int count)
        {
            ScmVertData[] v = new ScmVertData[count];
            for (int i = 0; i < count; ++i)
            {
                v[i] = ScmVertData.Read(br);
            }
            return v;
        }
        ScmTriangleData[] ReadTriangleData(BinaryReader br, int count)
        {
            ScmTriangleData[] v = new ScmTriangleData[count];
            for (int i = 0; i < count; ++i)
            {
                v[i] = ScmTriangleData.Read(br);
            }
            return v;
        }
        ScmBoneData[] ReadBoneData(BinaryReader br, int count)
        {
            ScmBoneData[] v = new ScmBoneData[count];
            for (int i = 0; i < count; ++i)
            {
                v[i] = ScmBoneData.Read(br);
            }
            return v;
        }
        public void Load(BinaryReader br)
        {
            Header = new ScmHeader(br);

            int padding = 32 - (int)br.BaseStream.Position % 32 - 4;
            br.BaseStream.Seek(padding, SeekOrigin.Current);

            if (ReadFourCC(br) != "NAME")
                throw new Exception("cant find bone names.");

            BoneNames = ReadStrings(br, (int)Header.mWeightedBoneCount);

            br.BaseStream.Seek(Header.mBoneOffset, SeekOrigin.Begin);
            BoneData = ReadBoneData(br, (int)Header.mWeightedBoneCount);

            br.BaseStream.Seek(Header.mVertexOffset, SeekOrigin.Begin);
            VertexData = ReadVertexData(br, (int)Header.mVertexCount);

            br.BaseStream.Seek(Header.mIndexOffset, SeekOrigin.Begin);
            TriangleData = ReadTriangleData(br, (int)Header.mIndexCount/3);


            br.BaseStream.Seek(Header.mInfoOffset, SeekOrigin.Begin);
            Infos = Encoding.ASCII.GetString(br.ReadBytes((int)Header.mInfoCount)).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public ScmHeader Header;
        public string[] BoneNames;
        public ScmVertData[] VertexData;
        public ScmTriangleData[] TriangleData;
        public string[] Infos;
        public ScmBoneData[] BoneData;
    }

    public class ScmapFile
    {
        public struct Layer
        {
            public string PathTexture;
            public string PathNormalmap;
            public float ScaleTexture;
            public float ScaleNormalmap;
        }

        public ScmapFile(Stream file)
        {
            Load(file);
        }

        public void Load(Stream file)
        {
            using (BinaryReader br = new BinaryReader(file,Encoding.ASCII))
            {
                Load(br);
            }
        }

        string ReadFourCC(BinaryReader br)
        {
            return Encoding.ASCII.GetString(br.ReadBytes(4));
        }

        public short[,] Heightmap;
        public float HeightScale;
        //int Width;
        //int Height;
        public byte[] TexturemapData;
        public byte[] PreviewImage;
        public Layer[] Layers;
        public byte[] NormalmapData;

        public void Load(BinaryReader br)
        {
            string magic = ReadFourCC(br);

            int FileVersionMajor = br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();

            float width = br.ReadSingle();
            float height = br.ReadSingle();
            br.ReadInt32();
            br.ReadInt16();

            int length = br.ReadInt32();
            PreviewImage = br.ReadBytes(length);

            int FileVersionMinor = br.ReadInt32();
            int w = br.ReadInt32();
            int h = br.ReadInt32();

            HeightScale = br.ReadSingle();
            Heightmap = new short[w + 1,h + 1];
            for (int x = 0; x < w+1; ++x)
                for (int y = 0; y < h + 1; ++y)
                    Heightmap[x,y] = br.ReadInt16();

            string TerrainShader = ReadString(br);

            string TexPathBackground = ReadString(br);
            string TexPathSkyCubemap = ReadString(br);
            string TexPathEnvCubemap = ReadString(br);

            float LightingMultiplier = br.ReadSingle();
            Vector3 SunDirection = Vector3.Read(br);
            Vector3 SunAmbience = Vector3.Read(br);
            Vector3 SunColor = Vector3.Read(br);
            Vector3 ShadowFillColor = Vector3.Read(br);
            Vector4 SpecularColor = Vector4.Read(br);
            float Bloom = br.ReadSingle();

            Vector3 FogColor = Vector3.Read(br);
            float FogStart = br.ReadSingle();
            float FogEnd = br.ReadSingle();

            bool HasWater = br.ReadBoolean();
            float WaterElevation = br.ReadSingle();
            float WaterElevationDeep = br.ReadSingle();
            float WaterElevationAbyss = br.ReadSingle();

            LoadWaterShaderProperties(br);



            int c = br.ReadInt32();
            for (int i = 0; i < c; ++i)
                LoadWaveGenerator(br);

            ReadString(br);

            c = br.ReadInt32();
            Layers = new Layer[c];
            for (int i = 0; i < c; ++i)
                Layers[i] = LoadLayer(br);

            br.ReadInt32();
            br.ReadInt32();

            c = br.ReadInt32();
            for (int i = 0; i < c; ++i)
                LoadDecal(br);

            c = br.ReadInt32();
            for (int i = 0; i < c; ++i)
                LoadDecalGroup(br);

            br.ReadInt32();//w
            br.ReadInt32();//h

            c = br.ReadInt32();//always 1?
            for (int i = 0; i < c; ++i)
            {
                int len = br.ReadInt32();
                if (i == 0)
                {
                    NormalmapData = br.ReadBytes(len);
                }
                else
                    br.ReadBytes(len);
            }
            br.ReadInt32();// 'always 1
            length = br.ReadInt32();
            TexturemapData = br.ReadBytes(length);

            br.ReadInt32();// 'always 1
            length = br.ReadInt32();
            byte[] WatermapData = br.ReadBytes(length);

            length = (w / 2) * (h / 2);
            byte[] WaterFoamMap = br.ReadBytes(length);// 'Obviously not used.. each byte is 00  
            byte[] WaterFlatnessMap = br.ReadBytes(length);// 'Obviously not used.. each byte is FF
            byte[] WaterDepthBiasMap = br.ReadBytes(length);// 'Obviously not used.. each byte is 7F

            byte[] TerrainTypeData = br.ReadBytes(w * h);

            if (FileVersionMinor <= 52) br.ReadInt16();// 'always 0

            c = br.ReadInt32();
            for (int i = 0; i < c; ++i)
                LoadProp(br);

        }

        private void LoadProp(BinaryReader br)
        {
            string BlueprintPath = ReadString(br);
            Vector3 Position = Vector3.Read(br);
            Vector3 Rotation_vX = Vector3.Read(br);
            Vector3 Rotation_vY = Vector3.Read(br);
            Vector3 Rotation_vZ = Vector3.Read(br);

            Vector3.Read(br);// ' unused scale
        }

        private void LoadDecalGroup(BinaryReader br)
        {
            int ID = br.ReadInt32();
            string Name = ReadString(br);
            int Length = br.ReadInt32();
            int[] Data = new int[Length];
            for (int i = 0; i < Length; ++i)
                Data[i] = br.ReadInt32();

        }

        private void LoadDecal(BinaryReader br)
        {
            int ID = br.ReadInt32();
            int Type = br.ReadInt32();

            int TextureCount = br.ReadInt32();
            for (int i = 0; i < TextureCount; ++i)
            {
                int Length = br.ReadInt32();
                string TexturePath = ReadString(br,Length);
            }

            Vector3 Scale = Vector3.Read(br);
            Vector3 Position = Vector3.Read(br);
            Vector3 Rotation = Vector3.Read(br);

            float CutOffLOD = br.ReadSingle();
            float NearCutOffLOD = br.ReadSingle();
            int OwnerArmy = br.ReadInt32();
        }

        private Layer LoadLayer(BinaryReader br)
        {
            Layer l;
            l.PathTexture = ReadString(br);
            l.PathNormalmap = ReadString(br);
            l.ScaleTexture = br.ReadSingle();
            l.ScaleNormalmap = br.ReadSingle();
            return l;
        }

        private void LoadWaveGenerator(BinaryReader br)
        {

            string TextureName = ReadString(br);
            string RampName = ReadString(br);

            Vector3 Position = Vector3.Read(br);
            float Rotation = br.ReadSingle();
            Vector3 Velocity = Vector3.Read(br);

            float LifetimeFirst = br.ReadSingle();
            float LifetimeSecond = br.ReadSingle();
            float PeriodFirst = br.ReadSingle();
            float PeriodSecond = br.ReadSingle();
            float ScaleFirst = br.ReadSingle();
            float ScaleSecond = br.ReadSingle();

            float FrameCount = br.ReadSingle();
            float FrameRateFirst = br.ReadSingle();
            float FrameRateSecond = br.ReadSingle();
            float StripCount = br.ReadSingle();
        }

        private void LoadWaterShaderProperties(BinaryReader br)
        {
            float[] ka = new float[20];
            for (int i = 0; i < ka.Length; ++i)
                ka[i] = br.ReadSingle();

            string TexPathWaterCubemap = ReadString(br);
            string TexPathWaterRamp = ReadString(br);

            float[] ka2 = new float[4];
            for (int i = 0; i < ka2.Length; ++i)
                ka2[i] = br.ReadSingle();

            for (int i = 0; i < 4; ++i)
                LoadWaveTexture(br);
        }

        private void LoadWaveTexture(BinaryReader br)
        {
            float ScaleX = br.ReadSingle();
            float ScaleY = br.ReadSingle();
            string TexturePath = ReadString(br);
        }

        private string ReadString(BinaryReader br)
        {
            char c;
            string s="";
            while ((c = br.ReadChar()) != '\0')
            {
                s += c;
            }
            return s;
        }
        private string ReadString(BinaryReader br, int length)
        {
            string s = "";
            for (int i = 0; i < length; ++i)
                s += br.ReadChar();
            return s;
        }
    }

}