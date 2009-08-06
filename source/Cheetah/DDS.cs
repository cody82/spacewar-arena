using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace DDS
{
    public enum TextureFormat
    {
        DXT1, DXT2, DXT3, DXT4, DXT5, RGB, RGBA
    }

    public class DdsHeader
    {
        public byte[] Signature;

        public int Size1;				// size of the structure (minus MagicNum)
        public int Flags1; 			// determines what fields are valid
        public int Height; 			// height of surface to be created
        public int Width;				// width of input surface
        public int LinearSize; 		// Formless late-allocated optimized surface size
        public int Depth;				// Depth if a volume texture
        public int MipMapCount;		// number of mip-map levels requested
        public int AlphaBitDepth;		// depth of alpha buffer requested

        //int	NotUsed[10];

        public int Size2;				// size of structure
        public int Flags2;				// pixel format flags
        public int FourCC;				// (FOURCC code)
        public int RGBBitCount;		// how many bits per pixel
        public int RBitMask;			// mask for red bit
        public int GBitMask;			// mask for green bits
        public int BBitMask;			// mask for blue bits
        public int RGBAlphaBitMask;	// mask for alpha channel

        public int ddsCaps1, ddsCaps2, ddsCaps3, ddsCaps4; // direct draw surface capabilities
        public int TextureStage;

        public bool IsCubeMap
        {
            get
            {
                if ((ddsCaps2 & DdsFile.DDS_CUBEMAP) > 0)
                {
                    if (
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_NEGATIVEX) == 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_NEGATIVEY) == 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_NEGATIVEZ) == 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_POSITIVEX) == 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_POSITIVEY) == 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_POSITIVEZ) == 0)
                    )
                    {
                        throw new Exception("cubemap doesnt contain all sides.");
                    }
                    //System.Console.WriteLine(ddsCaps2.ToString());
                    //System.Console.WriteLine("true" + DdsFile.DDS_CUBEMAP.ToString());
                    return true;
                }
                else
                {
                    if (
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_NEGATIVEX) != 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_NEGATIVEY) != 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_NEGATIVEZ) != 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_POSITIVEX) != 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_POSITIVEY) != 0) ||
                        ((ddsCaps2 & DdsFile.DDS_CUBEMAP_POSITIVEZ) != 0)
                    )
                    {
                        throw new Exception("cubemap fucked up.");
                    }
                    return false;
                }
            }
        }
    }

    public class DdsFile
    {

        public const int DDS_CAPS = 0x00000001;
        public const int DDS_HEIGHT = 0x00000002;
        public const int DDS_WIDTH = 0x00000004;
        public const int DDS_PIXELFORMAT = 0x00001000;

        public const int DDS_ALPHAPIXELS = 0x00000001;
        public const int DDS_ALPHA = 0x00000002;
        public const int DDS_FOURCC = 0x00000004;
        public const int DDS_PITCH = 0x00000008;
        public const int DDS_COMPLEX = 0x00000008;
        public const int DDS_TEXTURE = 0x00001000;
        public const int DDS_MIPMAPCOUNT = 0x00020000;
        public const int DDS_LINEARSIZE = 0x00080000;
        public const int DDS_VOLUME = 0x00200000;
        public const int DDS_MIPMAP = 0x00400000;
        public const int DDS_DEPTH = 0x00800000;

        public const int DDS_CUBEMAP = 0x00000200;
        public const int DDS_CUBEMAP_POSITIVEX = 0x00000400;
        public const int DDS_CUBEMAP_NEGATIVEX = 0x00000800;
        public const int DDS_CUBEMAP_POSITIVEY = 0x00001000;
        public const int DDS_CUBEMAP_NEGATIVEY = 0x00002000;
        public const int DDS_CUBEMAP_POSITIVEZ = 0x00004000;
        public const int DDS_CUBEMAP_NEGATIVEZ = 0x00008000;

        public DdsHeader Header;
        Stream Input;
        BinaryReader Reader;

        int BlockSize;
        public TextureFormat Format;
        public byte[][] MipMaps;
        public byte[][] CubeMaps;

        public DdsFile(Stream s)
        {
            Input = s;
            Reader = new BinaryReader(s);
            Header = ReadHeader();

            if (Header.IsCubeMap)
                ReadCubeMaps();
            else
                ReadMipmaps();
        }

        public static int MakeFourCC(char c0, char c1, char c2, char c3)
        {
            byte[] c = Encoding.ASCII.GetBytes(new char[] { c0, c1, c2, c3 });
            int x = ((int)c[0]) + (((int)c[1]) << 8) + (((int)c[2]) << 16) + (((int)c[3]) << 24);
            return x;
        }

        byte[] ReadData(int Width, int Height, int Depth)
        {
            int Bps;
            int y, z;
            //ILubyte* Temp;
            int Bpp;
            int CompSize;
            int CompLineSize;

            if (Format == TextureFormat.RGB)
                Bpp = 3;
            else
                Bpp = 4;

            byte[] data;

            if ((Header.Flags1 & DDS_LINEARSIZE) > 0 && Header.LinearSize>0)
            {
                //Head.LinearSize = Head.LinearSize * Depth;

                data = Reader.ReadBytes(Header.LinearSize);
            }
            else
            {
                Bps = Width * Header.RGBBitCount / 8;
                CompSize = Bps * Height * Depth;
                CompLineSize = Bps;

                //data = new byte[CompSize];
                data = Reader.ReadBytes(Depth * Height * Bps);
                /*CompData = (ILubyte*)ialloc(CompSize);

                Temp = CompData;
                for (z = 0; z < Depth; z++)
                {
                    for (y = 0; y < Height; y++)
                    {
                        if (iread(Temp, 1, Bps) != Bps)
                        {
                            ifree(CompData);
                            return IL_FALSE;
                        }
                        Temp += Bps;
                    }
                }*/
            }

            return data;
        }

        void ReadCubeMaps()
        {
            int i, CompFactor = 0;
            byte Bpp;
            int LastLinear;
            int minW, minH;

            if (Format == TextureFormat.RGB)
                Bpp = 3;
            else
                Bpp = 4;

            if ((Header.Flags1 & DDS_LINEARSIZE) > 0)
            {
                CompFactor = (Header.Width * Header.Height * Header.Depth * Bpp) / Header.LinearSize;
            }

            int Width = Header.Width;
            int Height = Header.Height;
            int Depth = Header.Depth;

            LastLinear = Header.LinearSize;
            CubeMaps = new byte[6][];

            for (i = 0; i < 6; i++)
            {

                if (Depth == 0)
                    Depth = 1;
                if (Width == 0)
                    Width = 1;
                if (Height == 0)
                    Height = 1;


                if ((Header.Flags1 & DDS_LINEARSIZE) > 0)
                {
                    minW = Width;
                    minH = Height;
                    if ((Format != TextureFormat.RGB) && (Format != TextureFormat.RGBA))
                    {
                        minW = Math.Max(4, Width);
                        minH = Math.Max(4, Height);
                        Header.LinearSize = (minW * minH * Depth * Bpp) / CompFactor;
                    }
                    else
                    {
                        Header.LinearSize = Width * Height * Depth * (Header.RGBBitCount >> 3);
                    }
                }
                else
                {
                    Header.LinearSize >>= 1;
                }

                CubeMaps[i] = ReadData(Width, Height, Depth);

                //Depth = Depth / 2;
                //Width = Width / 2;
                //Height = Height / 2;
            }

            Header.LinearSize = LastLinear;
        }

        void ReadMipmaps()
        {
            int i, CompFactor = 0;
            byte Bpp;
            int LastLinear;
            int minW, minH;

            if (Format == TextureFormat.RGB)
                Bpp = 3;
            else
                Bpp = 4;

            if ((Header.Flags1 & DDS_LINEARSIZE) > 0 && Header.LinearSize > 0)
            {
                CompFactor = (Header.Width * Header.Height * Header.Depth * Bpp) / Header.LinearSize;
            }

            int Width = Header.Width;
            int Height = Header.Height;
            int Depth = Header.Depth;

            LastLinear = Header.LinearSize;
            MipMaps = new byte[Header.MipMapCount][];

            for (i = 0; i < Header.MipMapCount; i++)
            {

                if (Depth == 0)
                    Depth = 1;
                if (Width == 0)
                    Width = 1;
                if (Height == 0)
                    Height = 1;


                if ((Header.Flags1 & DDS_LINEARSIZE) > 0)
                {
                    minW = Width;
                    minH = Height;
                    if ((Format != TextureFormat.RGB) && (Format != TextureFormat.RGBA))
                    {
                        minW = Math.Max(4, Width);
                        minH = Math.Max(4, Height);
                        Header.LinearSize = (minW * minH * Depth * Bpp) / CompFactor;
                    }
                    else
                    {
                        Header.LinearSize = Width * Height * Depth * (Header.RGBBitCount >> 3);
                    }
                }
                else
                {
                    Header.LinearSize >>= 1;
                }

                MipMaps[i] = ReadData(Width, Height, Depth);

                Depth = Depth / 2;
                Width = Width / 2;
                Height = Height / 2;
            }

            Header.LinearSize = LastLinear;
        }
        protected DdsHeader ReadHeader()
        {
            DdsHeader Header = new DdsHeader();
            Header.Signature = Reader.ReadBytes(4);

            Header.Size1 = Reader.ReadInt32();
            Header.Flags1 = Reader.ReadInt32();
            Header.Height = Reader.ReadInt32();
            Header.Width = Reader.ReadInt32();
            Header.LinearSize = Reader.ReadInt32();
            Header.Depth = Reader.ReadInt32();
            Header.MipMapCount = Reader.ReadInt32();
            Header.AlphaBitDepth = Reader.ReadInt32();

            Reader.ReadBytes(4 * 10);

            Header.Size2 = Reader.ReadInt32();
            Header.Flags2 = Reader.ReadInt32();
            Header.FourCC = Reader.ReadInt32();
            Header.RGBBitCount = Reader.ReadInt32();
            Header.RBitMask = Reader.ReadInt32();
            Header.GBitMask = Reader.ReadInt32();
            Header.BBitMask = Reader.ReadInt32();
            Header.RGBAlphaBitMask = Reader.ReadInt32();

            Header.ddsCaps1 = Reader.ReadInt32();
            Header.ddsCaps2 = Reader.ReadInt32();
            Header.ddsCaps3 = Reader.ReadInt32();
            Header.ddsCaps4 = Reader.ReadInt32();

            Header.TextureStage = Reader.ReadInt32();

            //System.Console.WriteLine("width: " + Header.Width + " height: " + Header.Height + " depth: " + Header.Depth + " mips: " + Header.MipMapCount);
            if (Header.Signature[0] != 'D' || Header.Signature[1] != 'D' || Header.Signature[2] != 'S' || Header.Signature[3] != ' ')
                throw new Exception();

            if (Header.Size1 != 124)
                throw new Exception();
            if (Header.Size2 != 32)
                throw new Exception();
            if (Header.Width == 0 || Header.Height == 0)
                throw new Exception();

            if (Header.Depth == 0)
                Header.Depth = 1;

            // Microsoft bug, they're not following their own documentation.
            if ((Header.Flags1 & (DDS_LINEARSIZE | DDS_PITCH)) == 0)
            {
                Header.Flags1 |= DDS_LINEARSIZE;
                //Header.LinearSize = BlockSize;
            }
            if ((Header.Flags1 & DDS_MIPMAPCOUNT) == 0 || Header.MipMapCount == 0)
            {
                //some .dds-files have their mipmap flag set,
                //but a mipmapcount of 0. Because mipMapCount is an uint, 0 - 1 gives
                //overflow - don't let this happen:
                Header.MipMapCount = 1;
            }
            if ((Header.Flags2 & DDS_FOURCC) > 0)
            {
                BlockSize = ((Header.Width + 3) / 4) * ((Header.Height + 3) / 4) * ((Header.Depth + 3) / 4);
                if (Header.FourCC == MakeFourCC('D', 'X', 'T', '1'))
                {
                    //CompFormat = PF_DXT1;
                    Format = TextureFormat.DXT1;
                    BlockSize *= 8;
                }
                else if (Header.FourCC == MakeFourCC('D', 'X', 'T', '2'))
                {
                    Format = TextureFormat.DXT2;
                    BlockSize *= 16;
                }
                else if (Header.FourCC == MakeFourCC('D', 'X', 'T', '3'))
                {
                    Format = TextureFormat.DXT3;
                    BlockSize *= 16;
                }
                else if (Header.FourCC == MakeFourCC('D', 'X', 'T', '4'))
                {
                    Format = TextureFormat.DXT4;
                    BlockSize *= 16;
                }
                else if (Header.FourCC == MakeFourCC('D', 'X', 'T', '5'))
                {
                    Format = TextureFormat.DXT5;
                    BlockSize *= 16;
                    if (Header.LinearSize == 0)
                    {
                        System.Console.WriteLine("dds: no linearsize?!");
                        Header.LinearSize = Header.Width * Header.Height;
                    }
                }
                else
                {
                    Format = TextureFormat.RGB;
                    BlockSize *= 16;
                }

                System.Console.WriteLine("dds format: " + Format.ToString());
            }
            else
            {
                // This dds texture isn't compressed so write out ARGB format
                if ((Header.Flags2 & DDS_ALPHAPIXELS) > 0)
                {
                    Format = TextureFormat.RGBA;
                }
                else
                {
                    Format = TextureFormat.RGB;
                }
                BlockSize = (Header.Width * Header.Height * Header.Depth * (Header.RGBBitCount >> 3));
            }

            System.Console.WriteLine("mipmaps: " + Header.MipMapCount);
            return Header;
        }
    }
}