using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using csogg;
using System.IO;
using oggPacket = csogg.Packet;

namespace Cheetah
{
    public class Theora
    {
    }

    public class OggFile : IResource
    {
        public class StreamMap
        {
            public StreamState State=new StreamState();
            public string Type;
            public int Serial;
            public Stream Output;
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
