using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using csogg;
using System.IO;
using oggPacket = csogg.Packet;
using System.Runtime.InteropServices;
using size_t=System.UInt32;
using ogg_int64_t=System.Int64;
using ogg_uint32_t=System.UInt32;
using System.Threading;
using System.Security;

namespace Cheetah.Graphics
{
    public unsafe class Theora
    {
		public enum th_colorspace
		{
			TH_CS_UNSPECIFIED,
			TH_CS_ITU_REC_470M,
			TH_CS_ITU_REC_470BG,
			TH_CS_NSPACES
		}

		public enum th_pixel_fmt
		{
			TH_PF_420,
			TH_PF_RSVD,
			TH_PF_422,
			TH_PF_444,
			TH_PF_NFORMATS
		}

        public struct th_info
        {
			public byte version_major;
			public byte version_minor;
			public byte version_subminor;
			public ogg_uint32_t  frame_width;
			public ogg_uint32_t  frame_height;
			public ogg_uint32_t  pic_width;
			public ogg_uint32_t  pic_height;
			public ogg_uint32_t  pic_x;
			public ogg_uint32_t  pic_y;
			public ogg_uint32_t  fps_numerator;
			public ogg_uint32_t  fps_denominator;
			public ogg_uint32_t  aspect_numerator;
			public ogg_uint32_t  aspect_denominator;
			public th_colorspace colorspace;
			public th_pixel_fmt  pixel_fmt;
			
			public int           target_bitrate;
			public int           quality;
			public int           keyframe_granule_shift;
        }

		
        public struct th_comment
        {
			public char **user_comments;
			public int   *comment_lengths;
			public int    comments;
			public char  *vendor;
        }

        public struct th_setup_info
        {
        }

        public struct ogg_packet
        {
			public IntPtr packet;
			
			//these are integers, size depends on architecture
			public IntPtr  bytes;
			public IntPtr  b_o_s;
			public IntPtr  e_o_s;
			
			public ogg_int64_t  granulepos;
  
			public ogg_int64_t  packetno;
		}

        public struct th_dec_ctx
        {
        }

		public struct th_img_plane
        {
			public int width;
			public int height;
			public int stride;
			public IntPtr data;
			
			
        }
        public struct th_ycbcr_buffer
        {
			public th_img_plane plane0;
			public th_img_plane plane1;
			public th_img_plane plane2;
		
        }
		
		
        //const string LIB = "libtheoradec.so.1";
        public const string LIB = "libtheora.dll";

		
		[DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity] 
		extern static public IntPtr th_version_string ();
			
        [DllImport(LIB,CallingConvention=CallingConvention.Winapi)]
        [SuppressUnmanagedCodeSecurity]
        extern static public void th_info_init(th_info* _info);
		
		[DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        extern static public void th_info_clear(th_info* _info);
			
		[DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        extern static public void th_comment_clear(th_comment* _tc);
		
		[DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        extern static public void th_comment_init(ref th_comment _tc);
			
        [DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        extern static public int th_decode_headerin(ref th_info _info, ref th_comment _tc, ref IntPtr _setup, ref ogg_packet _op);
        //extern static public int th_decode_headerin(ref th_info _info, ref th_comment _tc, th_setup_info** _setup, ogg_packet* _op);


        [DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        //extern static public th_dec_ctx* th_decode_alloc(th_info* _info, th_setup_info* _setup);
        extern static public IntPtr th_decode_alloc(ref th_info _info, IntPtr _setup);

        [DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        extern static public void th_setup_free(th_setup_info* _setup);

        [DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        extern static public int th_decode_ctl(th_dec_ctx* _dec, int _req, void* _buf, size_t _buf_sz);

        [DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        //extern static public int th_decode_packetin(th_dec_ctx* _dec, ogg_packet* _op, ogg_int64_t* _granpos);
        extern static public int th_decode_packetin(IntPtr _dec, ref ogg_packet _op, ref ogg_int64_t _granpos);

        [DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        //extern static public int th_decode_ycbcr_out(th_dec_ctx* _dec, th_ycbcr_buffer _ycbcr);
		extern static public int th_decode_ycbcr_out(IntPtr _dec, ref th_ycbcr_buffer _ycbcr);

        [DllImport(LIB)]
        [SuppressUnmanagedCodeSecurity]
        extern static public void th_decode_free(th_dec_ctx* _dec);
		
		
		public static string Version()
		{
			return Marshal.PtrToStringAnsi(th_version_string());
		}
    }

    public interface IOggDecoder
    {
        int PacketIn(oggPacket p);
    }

    public class VorbisDecoder : IOggDecoder
    {

		
        #region IOggDecoder Member

        public int PacketIn(csogg.Packet p)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class TheoraDecoder : IOggDecoder
    {
        bool HeadersRead = false;
		Theora.th_info info=new Theora.th_info();
		Theora.th_comment comment=new Theora.th_comment();
		IntPtr setup;
		IntPtr decode;
		int frame=0;
		int width=0;
		int height=0;
		public byte[] Data;
		public bool ConvertToRGB=false;
		
		public int FrameNumber
		{
			get{return frame;}
		}
		public int Width
		{
			get{return width;}
		}
		public int Height
		{
			get{return height;}
		}
		/*public byte[] Data
		{
			get{return data;}
		}*/
		
		public unsafe TheoraDecoder()
		{
			Console.WriteLine(Theora.Version());

            fixed (Theora.th_info* p = &info)
            {
                Theora.th_info_init(p);
            }
			Theora.th_comment_init(ref comment);
		
			
		}
		
		    public static T Clamp<T>(T value, T max, T min)
         where T : System.IComparable<T> {     
        T result = value;
        if (value.CompareTo(max) > 0)
            result = max;
        if (value.CompareTo(min) < 0)
            result = min;
        return result;
    } 
		
		
		
        #region IOggDecoder Member

        public int PacketIn(csogg.Packet pp)
        {
			Theora.ogg_packet op=new Theora.ogg_packet();
			op.bytes=new IntPtr(pp.bytes);
			op.b_o_s=new IntPtr(pp.b_o_s);
			op.e_o_s=new IntPtr(pp.e_o_s);
			op.granulepos=pp.granulepos;
			op.packet=Marshal.UnsafeAddrOfPinnedArrayElement(pp.packet_base,pp.packet);
			op.packetno=pp.packetno;

            if (!HeadersRead)
            {
                int r=Theora.th_decode_headerin(ref info,ref comment,ref setup, ref op);
				if(r==0)
				{
					Console.WriteLine("header complete");
					width=(int)info.frame_width;
					height=(int)info.frame_height;
					Data=new byte[width*height*3];
					Console.WriteLine(width.ToString()+"x"+height.ToString());
					HeadersRead=true;
					decode=Theora.th_decode_alloc(ref info,setup);
					if(decode==IntPtr.Zero)
						throw new Exception("decode");
				}
				else
					return 0;
            }

			ogg_int64_t granpos=0;
			int i;
			if(Theora.th_decode_packetin(decode,ref op,ref granpos)==0)
			{
				//Console.WriteLine("frame complete"+granpos.ToString());
				Theora.th_ycbcr_buffer picture=new Theora.th_ycbcr_buffer();
				i=Theora.th_decode_ycbcr_out(decode,ref picture);
				if(i!=0)
					throw new Exception("picture");
				float xfactor1=(float)picture.plane1.width/(float)picture.plane0.width;
				float yfactor1=(float)picture.plane1.height/(float)picture.plane0.height;
				float xfactor2=(float)picture.plane2.width/(float)picture.plane0.width;
				float yfactor2=(float)picture.plane2.height/(float)picture.plane0.height;
			
				if(ConvertToRGB)
				for(int y=0;y<picture.plane0.height;++y)
				{
					for(int x=0;x<picture.plane0.width;++x)
					{
						float Y=Marshal.ReadByte(picture.plane0.data,y*picture.plane0.stride+x);
						float Cb=Marshal.ReadByte(picture.plane1.data,(int)((float)y*yfactor1*(float)picture.plane1.stride+(float)x*xfactor1));
						float Cr=Marshal.ReadByte(picture.plane2.data,(int)((float)y*yfactor2*(float)picture.plane2.stride+(float)x*xfactor2));
						
						int R=(int)(Y + 1.402f *(Cr-128.0f));
						int G=(int)(Y - 0.34414f *(Cb-128.0f) - 0.71414f *(Cr-128.0f));
						int B=(int)(Y + 1.772f *(Cb-128.0f));
						
						R=Clamp(R,255,0);
						G=Clamp(G,255,0);
						B=Clamp(B,255,0);
						
						Data[y*width*3+x*3]=(byte)B;
						Data[y*width*3+x*3+1]=(byte)G;
						Data[y*width*3+x*3+2]=(byte)R;
						
						//w.WriteLine(R.ToString()+" "+G.ToString()+" "+B.ToString());
					}
				}
				else
				for(int y=0;y<picture.plane0.height;++y)
				{
					for(int x=0;x<picture.plane0.width;++x)
					{
						byte Y=Marshal.ReadByte(picture.plane0.data,y*picture.plane0.stride+x);
						byte Cb=Marshal.ReadByte(picture.plane1.data,(int)((float)y*yfactor1*(float)picture.plane1.stride+(float)x*xfactor1));
						byte Cr=Marshal.ReadByte(picture.plane2.data,(int)((float)y*yfactor2*(float)picture.plane2.stride+(float)x*xfactor2));
						
						
						Data[y*width*3+x*3]=(byte)Y;
						Data[y*width*3+x*3+1]=(byte)Cb;
						Data[y*width*3+x*3+2]=(byte)Cr;
						
						//w.WriteLine(R.ToString()+" "+G.ToString()+" "+B.ToString());
					}
				}
			
				//w.Close();
				//f.Close();
				frame++;
				return 1;
			}
			
            return 0;
        }

        #endregion
    }

    public class OggFile : IResource
    {
        public class StreamMap
        {
            public StreamState State=new StreamState();
            public string Type;
            public int Serial;
            public Stream Output;
            public IOggDecoder Decoder;
        }

        Stream stream;

        SyncState oy = new SyncState(); // sync and verify incoming physical bitstream
        StreamState os = new StreamState(); // take physical pages, weld into a logical stream of packets
        Page og = new Page(); // one Ogg bitstream page.  Vorbis packets are inside
        Dictionary<int, StreamMap> streams = new Dictionary<int, StreamMap>(); // one raw packet of data for decode
        oggPacket op=new oggPacket();
		public TheoraDecoder VideoDecoder;
		
        readonly int BUFFERSIZE = 8 * 1024;

        public OggFile(Stream s)
        {
            stream = s;
            
            oy.init();

			ReadNextTheoraFrame();
        }

		public void ReadNextTheoraFrame()
		{
			int bytes = 0;
            int index = 0;


            while (true)
            {
                while (oy.pageout(og) == 1)
                {
                    int serial=og.serialno();
                    StreamMap ss = null;
                    if (streams.ContainsKey(serial))
                    {
                        ss = streams[serial];
                    }
                    else
                    {
                        ss = new StreamMap();
                        ss.State.init(serial);
                        ss.Serial = serial;
                        streams[serial] = ss;
                        //ss.Output = new FileStream(ss.Serial.ToString(), FileMode.Create);
                        Console.WriteLine("new ogg stream: "+serial.ToString());

                    }
                    if (ss.State.pagein(og) == -1)
                        throw new Exception("pagein");
                    while (ss.State.packetout(op) == 1)
                    {
                        long packetno = op.packetno;
                        byte[] data = new byte[op.bytes];
                        Array.Copy(op.packet_base, op.packet, data, 0, op.bytes);

                        if (packetno == 0)
                        {
                            string type = Encoding.ASCII.GetString(data, 1, 6);
                            Console.WriteLine("new ogg stream: " + type);
                            ss.Type = type;
                            if (type == "theora")
                            {
                                ss.Decoder = VideoDecoder = new TheoraDecoder();
                            }
                        }

                        if (op.bytes > 0)
                        {
                            byte headertype = data[0];

                            if ((headertype & 0x80) != 0)
                            {
                                //this is a header packet
                                if (ss.Type == "theora")
                                {
                                    switch (headertype)
                                    {
                                        case 0x80:
                                            Console.WriteLine("theora ident header");
                                            break;
                                        case 0x81:
                                            Console.WriteLine("theora comment header");
                                            break;
                                        case 0x82:
                                            Console.WriteLine("theora setup header");
                                            break;
                                    }
                                }
                                else
								{
                                    //Console.WriteLine("headertype: " + headertype.ToString());
								}
                            }
                        }

                        if (op.e_o_s != 0)
                            Console.WriteLine("eos: " + op.e_o_s);
                        if(ss.Output!=null)
                            ss.Output.Write(data, 0, data.Length);
						
						if (ss.Decoder != null)
                        {
                            if(ss.Decoder.PacketIn(op)==1)
								return;
                        }

                        //Console.WriteLine("stream " + serial + ": " + packetno.ToString() + ", " + op.bytes + " bytes");

                    }

                }
                index = oy.buffer(BUFFERSIZE);
                bytes = stream.Read(oy.data, index, BUFFERSIZE);
                if (bytes <= 0)
				{
					Console.WriteLine("ogg stream end");
					stream.Seek(0,SeekOrigin.Begin);
               		bytes = stream.Read(oy.data, index, BUFFERSIZE);
 				}
				oy.wrote(bytes);
            }

		}
		
        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }

    public class OggLoader : IResourceLoader
    {
        #region IResourceLoader Member

        public IResource Load(FileSystemNode n)
        {
            return new OggFile(n.getStream());
        }

        public Type LoadType
        {
            get { return typeof(OggFile); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info.Name.ToLower().EndsWith(".ogg") || n.info.Name.ToLower().EndsWith(".ogv");
        }

        #endregion
    }
	
	public class TheoraTextureLoader : IResourceLoader
    {
        #region IResourceLoader Member

        public IResource Load(FileSystemNode n)
        {
            return new TheoraTexture(n.getStream());
        }

        public Type LoadType
        {
            get { return typeof(Texture); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            return n.info.Name.ToLower().EndsWith(".ogg") || n.info.Name.ToLower().EndsWith(".ogv");
        }

        #endregion
    }
	
	
	public class TheoraTexture : DynamicTexture,ITickable
	{
		OggFile ogg;
		public TheoraTexture(Stream s)
		{
            if (Root.Instance.UserInterface==null)
                return;

			try
			{
				ogg=new OggFile(s);
			}
			catch(Exception e)
			{
				Console.WriteLine("cant load ogg");
				return;
			}
			
			int size=ogg.VideoDecoder.Width*ogg.VideoDecoder.Height*3;
			ActiveSurface=new byte[size];
			DecodingSurface=new byte[size];

            Id=Root.Instance.UserInterface.Renderer.CreateTexture(ActiveSurface,ogg.VideoDecoder.Width,ogg.VideoDecoder.Height,false);
            DecodedFrame = LoadedFrame = 0;
            UpdateThread=new Thread(new ThreadStart(UpdateFunc));
			UpdateThread.Start();
		}

		public override void Dispose()
		{
			if (UpdateThread!=null && !ExitThread)
			{
                StopThread();
			}
		}

        protected void StopThread()
        {
            if (UpdateThread == null)
                return;

            ExitThread = true;
            if (Idle)
                UpdateThread.Resume();
            System.Console.WriteLine("stopping video update thread...");
            UpdateThread.Join();
            System.Console.WriteLine("video update thread ended.");
        }


		protected void UpdateSurface()
		{
			if(LoadedFrame!=DecodedFrame)
			{
				SwapMutex.WaitOne();
				Root.Instance.UserInterface.Renderer.UpdateTexture(Id,ActiveSurface);
				LoadedFrame=DecodedFrame;
				SwapMutex.ReleaseMutex();
			}
		}

		protected void Swap()
		{
			SwapMutex.WaitOne();
			byte[] tmp=ActiveSurface;
			ActiveSurface=DecodingSurface;
			DecodingSurface=tmp;
			SwapMutex.ReleaseMutex();
		}

		public void Tick(float dtime)
		{
			if(ogg==null)
				return;
			
            if (Root.Instance.Time - Id.LastBind > 5)
            {
                if (!Idle)
                {
                    Cheetah.Console.WriteLine("suspending video thread.");
                    UpdateThread.Suspend();
                    Idle = true;
                }
            }
            else
            {
                if (Idle)
                {
                    Cheetah.Console.WriteLine("resuming video thread.");
                    UpdateThread.Resume();
                    Idle = false;
                }
                Time += dtime;
                UpdateSurface();
            }
		}

		protected void UpdateFunc()
		{
			while(!ExitThread)
			{
                int wantframe = (int)(25 * Time);
                bool newframe=false;
                //while (wantframe > DecodedFrame)
                if (wantframe > DecodedFrame)
                {
					ogg.VideoDecoder.Data=DecodingSurface;
					ogg.ReadNextTheoraFrame();
					//Console.WriteLine("next"+DecodedFrame.ToString()+" "+LoadedFrame.ToString()+" "+ogg.VideoDecoder.FrameNumber.ToString());
                    newframe=true;
                    //XviD.Decode(Frame, l, DecodingSurface);
                    DecodedFrame++;
                }
                if(newframe)
                    Swap();
                Thread.Sleep(0);
			}
		}

        private bool Idle=false;
		private Thread UpdateThread;
		//private XviD.XviD XviD;
		//private AviLib.AviFile Avi;
		private byte[] Frame;
		private byte[] ActiveSurface;
		private byte[] DecodingSurface;
		private Mutex SwapMutex=new Mutex();
		private int LoadedFrame;
		private int DecodedFrame;
		private float Time;
		private bool ExitThread=false;
	}
}
