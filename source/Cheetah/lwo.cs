using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace Cheetah.Lightwave
{
    public class LWO
    {
        public LWO(string file)
            :this(new FileStream(file,FileMode.Open,FileAccess.Read))
        {
        }

        public LWO(Stream s)
        {
            Stream = s;
            Reader = new BinaryReader(s);

            string form = ReadString(4);
            if (form != "FORM")
                throw new Exception();

            byte[] ka = Reader.ReadBytes(4);

            string lwob = ReadString(4);
            if (!lwob.StartsWith("LWO2"))
                throw new Exception();

            while (true)
            {
                string chunk;
                int size;
                try
                {
                    chunk = ReadString(4);
                    size = ReadInt32();
                }
                catch (Exception)
                {
                    break;
                }
                
                Console.WriteLine(chunk + " " + size.ToString());
                long pos = Stream.Position;

                switch (chunk)
                {
                    case "PNTS":
                        PNTS(size);
                        break;
                    case "POLS":
                        POLS(size);
                        break;
                    case "VMAP":
                        VMAP(size);
                        break;
                    case "PTAG":
                        PTAG(size);
                        break;
                    case "TAGS":
                        TAGS(size);
                        break;
                    case "BBOX":
                        BBOX(size);
                        break;
                    case "LAYR":
                        LAYR(size);
                        break;
                    case "SURF":
                        SURF(size);
                        break;
                    case "CLIP":
                        CLIP(size);
                        break;
                }

                try
                {

                    Stream.Seek(pos+(long)size+(long)(size%2), SeekOrigin.Begin);
                }
                catch (Exception)
                {
                    break;
                }
            }

            s.Close();

            for (int i = 0; i < Layers.Count; ++i)
            {
                Console.WriteLine(Layers[i].ToString());
            }

            foreach (Part p in GetParts())
            {
                Console.WriteLine(p.ToString());
            }
        }

        public List<Part> GetParts()
        {
            List<Part> parts = new List<Part>();

            foreach (Layer l in Layers)
            {
                parts.AddRange(l.Parts);
            }
            return parts;
        }

        void BBOX(int size)
        {
            float[] min = new float[3];
            float[] max = new float[3];

            for (int i = 0; i < 3; ++i)
                min[i] = ReadSingle();
            for (int i = 0; i < 3; ++i)
                max[i] = ReadSingle();
        }

        void TMAP(int pos, int size)
        {
            string chunk;
            int csize;
            while (Stream.Position - pos < size)
            {
                chunk = ReadString(4);
                csize = ReadInt16();

                //Console.WriteLine("lwo: TMAP: " + chunk + " " + csize + ".");
                long pos2 = Stream.Position;

                Stream.Seek(pos2 + (long)csize + (long)(csize % 2), SeekOrigin.Begin);
            }
        }
        string IMAP(int pos, int size)
        {
            string chunk;
            int csize;

            ReadStringZero();
            string chan=null;
            while (Stream.Position - pos < size)
            {
                chunk = ReadString(4);
                csize = ReadInt16();

                //Console.WriteLine("lwo: IMAP: " + chunk + " " + csize + ".");
                long pos2 = Stream.Position;

                switch (chunk)
                {
                    case "CHAN":
                        chan = ReadString(4);
                        Console.WriteLine("lwo: CHAN: " + chan + ".");
                        switch (chan)
                        {
                            case "DIFF":
                                break;
                            case "LUMI":
                                break;
                            case "COLR":
                                break;
                            case "SPEC":
                                break;
                        }
                        break;
                    case "OPAC":
                        break;
                    case "ENAB":
                        break;
                    case "AXIS":
                        break;
                }
                Stream.Seek(pos2 + (long)csize + (long)(csize % 2), SeekOrigin.Begin);
            }

            return chan;
        }
        void BLOK(int pos, int size)
        {
            string chunk;
            int csize;

            string chan=null;

            while (Stream.Position - pos < size)
            {
                chunk = ReadString(4);
                csize = ReadInt16();

                //Console.WriteLine("lwo: BLOK: " + chunk + " " + csize + ".");
                long pos2 = Stream.Position;

                switch (chunk)
                {
                    case "IMAG":
                        int index = ReadVX() -1;
                        //Console.WriteLine(index.ToString());
                        Console.WriteLine("lwo: IMAG: " + index + "->" + Clips[index].Filename);
                        switch (chan)
                        {
                            case "DIFF":
                                CurrentSurface.DiffuseTexture = Clips[index].Filename;
                                break;
                            case "LUMI":
                                CurrentSurface.LumiTexture = Clips[index].Filename;
                                break;
                            case "COLR":
                                CurrentSurface.ColorTexture = Clips[index].Filename;
                                break;
                            case "SPEC":
                                CurrentSurface.SpecularTexture = Clips[index].Filename;
                                break;
                            default:
                                CurrentSurface.Texture = Clips[index].Filename;
                                break;
                        }

                        break;
                    case "PROC":
                    case "GRAD":
                    case "SHDR":
                        //Console.WriteLine("lwo: " + ReadStringZero()+".");
                        break;
                    case "PROJ":
                        int mode = ReadInt16();
                        Console.WriteLine("lwo: " + mode + ".");
                        break;
                    case "TMAP":
                        TMAP((int)pos2, csize);
                        break;
                    case "AXIS":
                        break;
                    case "WRAP":
                        break;
                    case "VMAP":
                        string uvmap = ReadStringZero();
                        Console.WriteLine("lwo: VMAP: " + uvmap + ".");
                        break;
                    case "IMAP":
                        chan=IMAP((int)pos2, csize);
                        break;


                }

                Stream.Seek(pos2 + (long)csize + (long)(csize % 2), SeekOrigin.Begin);
            }
        }

        void SURF_sub(int pos,int size)
        {
            string chunk;
            int csize;
            while (Stream.Position - pos < size)
            {
                //Reader.ReadByte();
                chunk = ReadString(4);
                csize = ReadInt16();

                //Console.WriteLine("lwo: surface-sub: " + chunk + " "+csize+".");
                long pos2 = Stream.Position;
                switch (chunk)
                {
                    case "COLR":
                        {
                            float[] color = new float[] { ReadSingle(), ReadSingle(), ReadSingle() };
                            int envelope = ReadVX();
                            break;
                        }
                    case "DIFF":
                        {
                            float[] intensity = new float[] { ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle() };
                            int envelope = ReadVX();
                            break;
                        }
                    case "LUMI":
                        {
                            float[] intensity = new float[] { ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle() };
                            int envelope = ReadVX();
                            break;
                        }
                    case "SPEC":
                        {
                            float[] intensity = new float[] { ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle() };
                            int envelope = ReadVX();
                            break;
                        }
                    case "REFL":
                        {
                            float[] intensity = new float[] { ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle() };
                            int envelope = ReadVX();
                            break;
                        }
                    case "TRAN":
                        {
                            float[] intensity = new float[] { ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle() };
                            int envelope = ReadVX();
                            break;
                        }
                    case "TRNL":
                        {
                            float[] intensity = new float[] { ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle() };
                            int envelope = ReadVX();
                            break;
                        }
                    case "GLOS":
                        {
                            float[] intensity = new float[] { ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle() };
                            int envelope = ReadVX();
                            break;
                        }
                    case "SMAN":
                        {
                            float angle = ReadSingle();
                            break;
                        }
                    case "BLOK":
                        {
                            BLOK((int)pos2,csize);
                            break;
                        }
                }


                Stream.Seek(pos2 + (long)csize + (long)(csize % 2), SeekOrigin.Begin);

            }
        }

        public List<Surface> Surfaces = new List<Surface>();
        Surface CurrentSurface;

        void SURF(int size)
        {
            int start = (int)Stream.Position;

            string name = ReadStringZero();
            string source = ReadStringZero();

            Console.WriteLine("lwo: surface: " + name + " " + source + ".");
            CurrentSurface = new Surface();
            Surfaces.Add(CurrentSurface);

            SURF_sub(start,size);
        }

        public class Clip
        {
            public string Filename;
        }
        public List<Clip> Clips = new List<Clip>();

        void CLIP(int size)
        {
            int pos = (int)Stream.Position;

            int index = ReadInt32();

            string chunk;
            int csize;
            while (Stream.Position - pos < size)
            {
                chunk = ReadString(4);
                csize = ReadInt16();

                Console.WriteLine("lwo: CLIP: " + chunk + " " + csize + ".");
                long pos2 = Stream.Position;

                switch (chunk)
                {
                    case "STIL":
                        string filename = ReadStringZero();
                        Console.WriteLine("lwo: filename: " + filename + ".");
                        Clip c = new Clip();
                        Clips.Add(c);
                        c.Filename = filename;
                        break;
                    case "XREF":
                        break;
                    case "ISEQ":
                        break;
                    case "ANIM":
                        break;
                    case "STCC":
                        break;
                }

                Stream.Seek(pos2 + (long)csize + (long)(csize % 2), SeekOrigin.Begin);
            }
            //throw new Exception();
        }
        void LAYR(int size)
        {
            int number = ReadInt16();
            int flags = ReadInt16();
            float[] pivot = new float[] { ReadSingle(), ReadSingle(), ReadSingle() };
            string name = ReadStringZero();
            int parent = ReadInt16();
            Console.WriteLine("lwo: layer: " + number + " " + name + " " + parent + ".");
            CurrentLayer = new Layer();
            CurrentLayer.Number = number;
            CurrentLayer.Name = name;
            Layers.Add(CurrentLayer);
        }


        void PTAG(int size)
        {
            int pos = (int)Stream.Position;
            string type = ReadString(4);

            if (type == "SURF")
            {
                while (Stream.Position - pos < size)
                {
                    int poly = ReadVX();
                    int surf = ReadInt16();
                    CurrentLayer.Polygons[poly].Surface = surf;
                }
            }
            else if (type == "SMGP")
            {
                while (Stream.Position - pos < size)
                {
                    int poly = ReadVX();
                    int group = ReadInt16();
                    CurrentLayer.Polygons[poly].Group = group;
                }
            }
        }

        void TAGS(int size)
        {
            int pos = (int)Stream.Position;

            while (Stream.Position - pos < size)
            {
                string tag = ReadStringZero();
                Console.WriteLine("low: tag " + tag + ".");
            }
        }

        void VMAP(int size)
        {
            int pos = (int)Stream.Position;
            string type = ReadString(4);
            int dim = ReadInt16();
            string name = ReadStringZero();
            List<float> values = new List<float>();
            List<int> indices = new List<int>();


            if (type == "TXUV")
            {
                while (Stream.Position - pos < size)
                {
                    indices.Add(ReadVX());
                    for (int i = 0; i < dim; ++i)
                    {
                        float f = ReadSingle();
                        //if (f < 0 || f > 1)
                        //    Console.WriteLine(f.ToString());
                        values.Add(f);
                    }
                }

                if (indices.Count * dim != values.Count)
                    throw new Exception();
                Console.WriteLine("lwo: " + indices.Count + " uv values, " + values.Count + " floats.");

                float[] uv=new float[values.Count];
                for (int i = 0; i < indices.Count; ++i)
                {
                    uv[indices[i] * 2 + 0] = values[i*2+0];
                    uv[indices[i] * 2 + 1] = values[i*2+1];
                }
                CurrentLayer.TextureUV=uv;
            }
        }

        string ReadStringZero()
        {
            byte[] b;
            string s = "";
            while ((b = Reader.ReadBytes(1))[0] != 0)
            {
                s += Encoding.ASCII.GetString(b );
            }
            if ((s.Length + 1) % 2 != 0)
                if (Reader.ReadByte() != 0)
                    throw new Exception();

            return s;
        }

        int[] PolygonToTriangles(int[] polygon)
        {
            int numtriangles = polygon.Length - 2;
            int[] triangles = new int[numtriangles * 3];

            for (int i = 0; i < numtriangles; ++i)
            {
                triangles[i * 3 + 0] = polygon[0];
                triangles[i * 3 + 1] = polygon[i + 1];
                triangles[i * 3 + 2] = polygon[i + 2];
            }
            return triangles;
        }

        public class Surface
        {
            public string Texture;
            public string DiffuseTexture;
            public string LumiTexture;
            public string ColorTexture;
            public string SpecularTexture;
        }

        public class Part
        {
            public List<int> Triangles=new List<int>();
            public int Surface;
            public float[] Points;
            public float[] TextureUV;

            public override string ToString()
            {
                return Surface.ToString() + ": " + Triangles.Count/3;
            }
        }

        public class Polygon
        {
            public Polygon(int[] t)
            {
                Triangles = t;
            }
            public int TriangleCount
            {
                get
                {
                    return Triangles.Length / 3;
                }
            }
            public int[] Triangles;
            public int Surface;
            public int Group;
        }

        public class Layer
        {
            public List<Polygon> Polygons=new List<Polygon>();
            public float[] Points;
            public float[] TextureUV;
            public string Name;
            public int Number;
            public List<Part> Parts;

            public int TriangleCount
            {
                get
                {
                    int c = 0;
                    foreach (Polygon p in Polygons)
                        c += p.TriangleCount;
                    return c;
                }
            }

            void CreateParts()
            {
                if (Parts != null)
                    return;

                SortPolygons();
                Part current = null;
                Parts = new List<Part>();
                for (int i = 0; i < Polygons.Count; ++i)
                {
                    Polygon p = Polygons[i];
                    if (current == null || p.Surface != current.Surface)
                    {
                        if (current != null)
                            Parts.Add(current);
                        current = new Part();
                        current.Surface = p.Surface;
                        current.Points = Points;
                        current.TextureUV = TextureUV;
                    }
                    current.Triangles.AddRange(p.Triangles);
                }
                Parts.Add(current);

            }

            int GetSurfaceCount()
            {
                CreateParts();
                return Parts.Count;
            }

            void SortPolygons()
            {
                Polygons.Sort(new Comparison<Polygon>(delegate(Polygon p1, Polygon p2)
                {
                    if (p1.Surface > p2.Surface)
                        return 1;
                    else if (p1.Surface < p2.Surface)
                        return -1;
                    else
                        return 0;
                }));
                //return parts;
            }

            public override string ToString()
            {
                return "layer " + Name + ": " + Polygons.Count + " polys, " + TriangleCount + " triangles, " + (Points.Length / 3) + " points, " + (TextureUV.Length / 2) + " UVs, " + GetSurfaceCount() +" surfaces.";
            }
        }

        //public List<Polygon> Polygons;

        void POLS(int size)
        {
            int pos = (int)Stream.Position;
            string type = ReadString(4);
            if (type != "FACE")
                return;
            //List<int> indices = new List<int>();

            CurrentLayer.Polygons = new List<Polygon>();
            while (Stream.Position-pos<size)
            {
                int count = (int)ReadInt16() & 1023;

                int[] vertices = new int[count];
                for (int i = 0; i < count; ++i)
                {
                    vertices[i] = ReadVX();
                }
                int[] triangles = PolygonToTriangles(vertices);
                //indices.AddRange(triangles);
                CurrentLayer.Polygons.Add(new Polygon(triangles));
            }
            //if (indices.Count % 3 != 0)
            //     throw new Exception();

            //Console.WriteLine("lwo: " + indices.Count + " indices, " + indices.Count/3 + " triangles.");

            //Triangles.Add(indices.ToArray());
        }

        int ReadVX()
        {
            int b = Reader.PeekChar();
            if (b == 255)
            {
                return ReadInt32() & 0x00FFFFFF;
            }
            else
            {
                return ReadInt16();
            }
        }

        //public List<float[]> Points=new List<float[]>();
        //public List<float[]> TextureUV = new List<float[]>();
        //public List<int[]> Triangles=new List<int[]>();
        public List<Layer> Layers = new List<Layer>();
        public Layer CurrentLayer;

        void PNTS(int size)
        {
            int count = size / 12;
            int v = 0;
            float[] points = new float[count * 3];

            for (int i = 0; i < count; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    points[v++] = ReadSingle();
                }
            }
            if (v != count * 3)
                throw new Exception();

            Console.WriteLine(count + " points.");
            //Points.Add(points);
            CurrentLayer.Points = points;
        }

        float ReadSingle()
        {
            byte[] c = Reader.ReadBytes(4);
            if (c.Length != 4)
                throw new Exception();
            byte b;
            b = c[0];
            c[0] = c[3];
            c[3] = b;
            b = c[1];
            c[1] = c[2];
            c[2] = b;
            BinaryReader r = new BinaryReader(new MemoryStream(c));
            return r.ReadSingle();
        }

        int ReadInt16()
        {
            byte[] c = Reader.ReadBytes(2);
            if (c.Length != 2)
                throw new Exception();
            return (((int)c[0]) << 8) + (int)c[1];
        }

        int ReadInt32()
        {
            byte[] c = Reader.ReadBytes(4);
            if (c.Length != 4)
                throw new Exception();
            return (((int)c[0]) << 24) + (((int)c[1]) << 16) + (((int)c[2]) << 8) + (int)c[3];
        }

        Stream Stream;
        BinaryReader Reader;

        string ReadString(int length)
        {
            byte[] c = Reader.ReadBytes(length);
            return Encoding.ASCII.GetString(c);
        }
    }


}