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

namespace Cheetah
{
    public unsafe class Theora
    {
        public struct th_info
        {
        }

        public struct th_comment
        {
        }

        public struct th_setup_info
        {
        }

        public struct ogg_packet
        {
        }

        public struct th_dec_ctx
        {
        }

        public struct th_ycbcr_buffer
        {
        }
        
        const string LIB = "libtheora";

        [DllImport(LIB)]
        extern static public int th_decode_headerin(th_info* _info, th_comment* _tc, th_setup_info** _setup, ogg_packet* _op);


        [DllImport(LIB)]
        extern static public th_dec_ctx* th_decode_alloc(th_info* _info, th_setup_info* _setup);

        [DllImport(LIB)]
        extern static public void th_setup_free(th_setup_info* _setup);

        [DllImport(LIB)]
        extern static public int th_decode_ctl(th_dec_ctx* _dec, int _req, void* _buf, size_t _buf_sz);

        [DllImport(LIB)]
        extern static public int th_decode_packetin(th_dec_ctx* _dec, ogg_packet* _op, ogg_int64_t* _granpos);

        [DllImport(LIB)]
        extern static public int th_decode_ycbcr_out(th_dec_ctx* _dec, th_ycbcr_buffer _ycbcr);

        [DllImport(LIB)]
        extern static public void th_decode_free(th_dec_ctx* _dec);
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

        #region IOggDecoder Member

        public int PacketIn(csogg.Packet pp)
        {
            if (!HeadersRead)
            {
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
            
            oy.init(); // Now we can read pages
            int r;

            int bytes = 0;
            int index = 0;// oy.buffer(BUFFERSIZE);


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
                                    Console.WriteLine("headertype: " + headertype.ToString());
                            }
                        }

                        if (ss.Decoder != null)
                        {
                            ss.Decoder.PacketIn(op);
                        }

                        Console.WriteLine("stream " + serial + ": " + packetno.ToString() + ", " + op.bytes + " bytes");
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
