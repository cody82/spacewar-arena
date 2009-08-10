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

namespace Cheetah
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
			public long  bytes;
			public long  b_o_s;
			public long  e_o_s;
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
		
		
        const string LIB = "libtheoradec.so.1";

		
		[DllImport(LIB)]
		extern static public IntPtr th_version_string ();
			
        [DllImport(LIB)]
        extern static public void th_info_init (ref th_info _info);
		
		[DllImport(LIB)]
		extern static public void th_info_clear (th_info *_info);
			
		[DllImport(LIB)]
		extern static public void th_comment_clear (th_comment *_tc);
		
		[DllImport(LIB)]
		extern static public void th_comment_init (ref th_comment _tc);
			
        [DllImport(LIB)]
        extern static public int th_decode_headerin(ref th_info _info, ref th_comment _tc, ref IntPtr _setup, ref ogg_packet _op);
        //extern static public int th_decode_headerin(ref th_info _info, ref th_comment _tc, th_setup_info** _setup, ogg_packet* _op);


        [DllImport(LIB)]
        //extern static public th_dec_ctx* th_decode_alloc(th_info* _info, th_setup_info* _setup);
        extern static public IntPtr th_decode_alloc(ref th_info _info, IntPtr _setup);

        [DllImport(LIB)]
        extern static public void th_setup_free(th_setup_info* _setup);

        [DllImport(LIB)]
        extern static public int th_decode_ctl(th_dec_ctx* _dec, int _req, void* _buf, size_t _buf_sz);

        [DllImport(LIB)]
        //extern static public int th_decode_packetin(th_dec_ctx* _dec, ogg_packet* _op, ogg_int64_t* _granpos);
        extern static public int th_decode_packetin(IntPtr _dec, ref ogg_packet _op, ref ogg_int64_t _granpos);

        [DllImport(LIB)]
        //extern static public int th_decode_ycbcr_out(th_dec_ctx* _dec, th_ycbcr_buffer _ycbcr);
		extern static public int th_decode_ycbcr_out(IntPtr _dec, ref th_ycbcr_buffer _ycbcr);

        [DllImport(LIB)]
        extern static public void th_decode_free(th_dec_ctx* _dec);
		
		
		public static string Version()
		{
			//byte *b=th_version_string();
			//Console.WriteLine(((int)b).ToString());
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
		public TheoraDecoder()
		{
			Console.WriteLine(Theora.Version());
			
			Theora.th_info_init(ref info);
			Theora.th_comment_init(ref comment);
		
			
		}
        #region IOggDecoder Member

        public int PacketIn(csogg.Packet pp)
        {
			Theora.ogg_packet op=new Theora.ogg_packet();
			op.bytes=pp.bytes;
			op.b_o_s=pp.b_o_s;
			op.e_o_s=pp.e_o_s;
			op.granulepos=pp.granulepos;
			op.packet=Marshal.UnsafeAddrOfPinnedArrayElement(pp.packet_base,pp.packet);
			op.packetno=pp.packetno;
			
            if (!HeadersRead)
            {
				if(Theora.th_decode_headerin(ref info,ref comment,ref setup, ref op)==0)
				{
					Console.WriteLine("header complete");
					HeadersRead=true;
					decode=Theora.th_decode_alloc(ref info,setup);
					if(decode==IntPtr.Zero)
						throw new Exception("decode");
				}
				else
					return 0;
            }

			ogg_int64_t granpos=0;
			int i=Theora.th_decode_packetin(decode,ref op,ref granpos);
			if(i==0)
			{
				Console.WriteLine("frame complete");
				Theora.th_ycbcr_buffer picture=new Theora.th_ycbcr_buffer();
				i=Theora.th_decode_ycbcr_out(decode,ref picture);
				if(i!=0)
					throw new Exception("picture");
				Console.WriteLine(picture.plane0.width.ToString());
				Console.WriteLine(picture.plane0.height.ToString());
				Console.WriteLine(picture.plane0.stride.ToString());
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

        readonly int BUFFERSIZE = 8 * 1024;

        public OggFile(Stream s)
        {
            stream = s;
            
            oy.init();
            int r;

            int bytes = 0;
            int index = 0;


            while (true)
            {
                index = oy.buffer(BUFFERSIZE);
                bytes = stream.Read(oy.data, index, BUFFERSIZE);
                if (bytes <= 0)
                    break;

                oy.wrote(bytes);
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
                                ss.Decoder = new TheoraDecoder();
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

                        if (ss.Decoder != null)
                        {
                            ss.Decoder.PacketIn(op);
                        }

                        //Console.WriteLine("stream " + serial + ": " + packetno.ToString() + ", " + op.bytes + " bytes");
                        if (op.e_o_s != 0)
                            Console.WriteLine("eos: " + op.e_o_s);
                        if(ss.Output!=null)
                            ss.Output.Write(data, 0, data.Length);
                    }

                }
            }
            foreach (StreamMap m in streams.Values)
            {
                if(m.Output!=null)
                    m.Output.Close();
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
}
