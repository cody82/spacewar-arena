using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using Lidgren.Library.Network;

namespace Cheetah
{
	public class Datagram
	{
		public Datagram(byte[] data,int i,int l,IPEndPoint ep)
		{
			Data=data;
			Sender=ep;
            Index = i;
            Length = l;
		}

        public Stream GetStream()
        {
            return new MemoryStream(Data, Index, Length);
        }

        public byte[] GetData()
        {
            return Data;
        }

		protected byte[] Data;
        public int Index;
        public int Length;
		public IPEndPoint Sender;
        public bool Connected=true;
	}

	public abstract class Packet : ISerializable
	{
        public virtual void DeSerialize(DeSerializationContext context)
        {
			//ClientNumber=r.ReadInt16();
			//SequenceNumber=r.ReadInt16();
		}

        public virtual void Serialize(SerializationContext context)
        {
			//w.Write(ClientNumber);
			//w.Write(SequenceNumber);
            //context.Write(Time);
		}

		//public short ClientNumber;
		//public short SequenceNumber;
        //public float Time;
	}

    public class Command : ISerializable
    {
        public Command(string cmd)
        {
            Cmd = cmd;
        }

        public Command(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public virtual void DeSerialize(DeSerializationContext context)
        {
            Cmd = context.ReadString();
        }

        public virtual void Serialize(SerializationContext context)
        {
            context.Write(Cmd);
        }

        public void Execute()
        {
            Console.WriteLine("excuting " + Cmd);
            Root.Instance.Script.Execute(Cmd);
        }

        public string Cmd;
    }

	public class Handshake : ISerializable
	{
		public enum Phase
		{
			Request,Accept,Deny,Quit
		}

		public Handshake(Phase t,int m,short n,string i,string password)
		{
			Type=t;
			Magic=m;
			ClientNumber=n;
			Info=i;
            if (password != null)
                Password = password;
            else
			    Password="";
            Name = "";
            Version = ThisVersion;
        }
        public Handshake(Phase t, int m, short n, string i)
            :this(t,m,n,i,"")
        {
        }

		public Handshake(DeSerializationContext context)
		{
			DeSerialize(context);
		}

        public void DeSerialize(DeSerializationContext context)
        {
            Magic = context.ReadInt32();
            Type = (Phase)context.ReadInt32();
            ClientNumber = context.ReadInt16();
            Info = context.ReadString();
            Password = context.ReadString();
            Name = context.ReadString();
            Version = context.ReadInt32();
        }

        public void Serialize(SerializationContext context)
        {
			context.Write(Magic);
            context.Write((int)Type);
            context.Write(ClientNumber);
            context.Write(Info);
            context.Write(Password);
            context.Write(Name);
            context.Write(Version);
        }

		public int Magic;
		public Phase Type;
		public short ClientNumber;
		public string Info;
		public string Password;
        public string Name;
        public int Version;
        public static readonly int MagicDefault=1337;
        public static readonly int ThisVersion = 7;
	}

    public interface IQuery
    {
        byte[][] Answer(byte[] packet, int length);
    }

    public class QueryServer : ITickable, IDisposable
    {
        public QueryServer(int port,IQuery[] q)
        {
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Sock.Bind(new IPEndPoint(IPAddress.Any,port));
            queries = q;
            Console.WriteLine("queryserver on port " + port + ".");
            //Sock.Blocking = false;
        }

        Socket Sock;

        #region ITickable Members

        public void Tick(float dtime)
        {
            byte[] buffer=new byte[8192];
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			EndPoint tempRemoteEP = (EndPoint)sender;
            int l;
            //try
            //{
            while (Sock.Poll(0, SelectMode.SelectRead))
            {
                if ((l = Sock.ReceiveFrom(buffer, ref tempRemoteEP)) > 0)
                {
                    //Console.WriteLine("received packet: " + Encoding.ASCII.GetString(buffer, 0, l));
                    foreach (IQuery q in queries)
                    {
                        byte[][] answer = q.Answer(buffer, l);
                        if (answer != null)
                        {
                            foreach (byte[] p in answer)
                            {
                                Console.WriteLine(tempRemoteEP.ToString());
                                Sock.SendTo(p, tempRemoteEP);
                            }
                        }
                    }
                }
            }
            //}
            //{
            //}
        }

        #endregion

        IQuery[] queries;// = new IQuery[] { new OldGameSpyQuery() };

        void Close()
        {
            if (Sock != null)
            {
                Sock.Close();
                Sock = null;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }

	public class QueryPacket : ISerializable
	{
		public enum Phase
		{
			Request,Reply
		}
		
		public QueryPacket()
		{
		}

		public QueryPacket(DeSerializationContext context)
		{
            DeSerialize(context);
        }

        public void DeSerialize(DeSerializationContext context)
        {
			Type=(Phase)context.ReadInt32();
            Info = context.ReadString();
            Name = context.ReadString();
            MaxPlayers = context.ReadInt32();
        }

        public void Serialize(SerializationContext context)
        {
            context.Write((int)Type);
            context.Write(Info);
            context.Write(Name);
            context.Write(MaxPlayers);
        }

		public Phase Type;
		public string Info="";
        public string Name = "";
        public int MaxPlayers;
	}

    public class InternetScanner : ITickable
    {
        public InternetScanner(AnswerDelegate answer)
        {
            Answer = answer;
            string url = @"http://fch.selfkill.com/cody/spacewar2006-servers.php";

            {
                WebClient wclient = new WebClient();
                try
                {
                    Stream strm = wclient.OpenRead(url);
                    MemoryStream ms = new MemoryStream();
                    StreamReader sr = new StreamReader(strm);

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] s = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(s[0]), int.Parse(s[1]));
                        Servers.Add(ep, null);
                        if (Answer != null)
                            Answer(null, ep);
                    }
                }
                catch (Exception e2)
                {
                }
            }
            Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//#if WINDOWS
            Sock.EnableBroadcast = true;
//#endif
            Sock.Blocking = false;

            //client = new UdpClient(Sock);

            Connected = true;
        }


        private void SendQuery()
        {
            System.Console.WriteLine("sending internet");

            QueryPacket q = new QueryPacket();
            q.Type = QueryPacket.Phase.Request;

            try
            {
                foreach(KeyValuePair<IPEndPoint,ISerializable> kv in Servers)
                {
                    //client.SetRemoteHost(kv.Key.Address, kv.Key.Port);
                    //client.Send(q);
                }
            }
            catch (SocketException)
            {
            }
        }

        public void Tick(float dtime)
        {
            //byte[] buf=new byte[1024*16];
            NetMessage dg;
            //EndPoint ep=new IPEndPoint(IPAddress.Any,0);
            int n;

            //try
            {
                while ((dg = client.Receive()) != null)
                {
                    Console.WriteLine("answer received.");
                    //QueryPacket p=(QueryPacket)Root.Instance.Factory.DeSerialize(new MemoryStream(buf,4,buf.Length-4));
                    //ISerializable p = (ISerializable)Root.Instance.Factory.DeSerialize(dg.GetStream());

                    //Servers[dg.Sender] = p;
                    //if (Answer != null)
                    //    Answer(p, dg.Sender);
                }
            }
            //catch (SocketException)
            {
            }

            Timer += dtime;
            if (Timer > QueryInterval)
            {
                Timer = 0;
                SendQuery();
            }
        }
        public delegate void AnswerDelegate(ISerializable answer, IPEndPoint ep);
        public event AnswerDelegate Answer;

        UdpClient client;
        public float QueryInterval = 5;
        float Timer = 0;

        public Dictionary<IPEndPoint, ISerializable> Servers = new Dictionary<IPEndPoint, ISerializable>();
        public bool Connected = false;
    }

    public class NewLanScanner : ITickable
    {
        public NewLanScanner()
        {
            LowPort = ((Config)Root.Instance.ResourceManager.Load("config/global.config")).GetInteger("lanscanner.scanstart");
            HighPort = ((Config)Root.Instance.ResourceManager.Load("config/global.config")).GetInteger("lanscanner.scanend");

            string hostlist = ((Config)Root.Instance.ResourceManager.Load("config/global.config")).GetString("lanscanner.search");
            //Hosts = hostlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Hosts = hostlist.Split(new char[] { ',' });
            for (int i = 0; i < Hosts.Length; ++i)
            {
                Hosts[i] = Hosts[i].Trim();
            }

            UdpClient udp = new UdpClient();
            client = udp.client;
            client.ServerDiscovered += new EventHandler<NetServerDiscoveredEventArgs>(OnServerDiscovered);

            SendQuery();
        }

        void OnServerDiscovered(object sender, NetServerDiscoveredEventArgs e)
        {
            // We found a server located at e.ServerInformation.RemoteEndpoint

            if (Answer != null)
            {
                Answer(Encoding.UTF8.GetBytes(
                    e.ServerInformation.ServerName+"/"+
                    e.ServerInformation.NumConnected.ToString()+"/"+
                    e.ServerInformation.MaxConnections.ToString()),
                    e.ServerInformation.RemoteEndpoint);
            }
        }

        bool internet = false;
        private void SendQuery()
        {
            System.Console.WriteLine("sending");

            int from = LowPort;
            int to = HighPort;

            for (int i = LowPort; i < HighPort; ++i)
            {
                client.DiscoverLocalServers(i);
            }


            if (internet)
            {
                foreach (string host in Hosts)
                {
                    //client.DiscoverKnownServer(host,
                }
            }
            //int n=Sock.SendTo(buf, new IPEndPoint(IPAddress.Parse("192.168.0.10"), 1337));
        }

        public void Tick(float dtime)
        {
            //byte[] buf=new byte[1024*16];
            //Datagram dg;
            //EndPoint ep=new IPEndPoint(IPAddress.Any,0);
            //int n;

            //try
            {
                //  while ((dg = client.Receive())!=null)
                {
                    //Console.WriteLine("answer received.");
                    //QueryPacket p=(QueryPacket)Root.Instance.Factory.DeSerialize(new MemoryStream(buf,4,buf.Length-4));
                    //ISerializable p = (ISerializable)Root.Instance.Factory.DeSerialize(dg.GetStream());

                    //Servers[dg.Sender] = p;
                    //if (Answer!=null)
                    //    Answer(p, dg.Sender);
                }
            }
            //catch (SocketException)
            {
            }

            Timer += dtime;
            if (Timer > QueryInterval)
            {
                Timer = 0;
                SendQuery();
            }
            client.Heartbeat();
        }

        public delegate void AnswerDelegate(byte[] answer, IPEndPoint ep);
        public event AnswerDelegate Answer;

        public Hashtable Servers = new Hashtable();
        NetClient client;
        public float QueryInterval = 5;
        float Timer = 0;

        int LowPort;
        int HighPort;

        string[] Hosts;
    }

    public class LanScanner : ITickable
    {
        public LanScanner()
        {
            LowPort = ((Config)Root.Instance.ResourceManager.Load("config/global.config")).GetInteger("lanscanner.scanstart");
            HighPort = ((Config)Root.Instance.ResourceManager.Load("config/global.config")).GetInteger("lanscanner.scanend");

            string hostlist = ((Config)Root.Instance.ResourceManager.Load("config/global.config")).GetString("lanscanner.search");
            //Hosts = hostlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Hosts = hostlist.Split(new char[] { ',' });
            for (int i = 0; i < Hosts.Length; ++i)
            {
                Hosts[i] = Hosts[i].Trim();
            }

            Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//#if WINDOWS
            Sock.EnableBroadcast = true;
//#endif
            Sock.Blocking = false;

            //client = new UdpClient(Sock);
            //Sock.Bind(new IPEndPoint(IPAddress.Any, 0));

            SendQuery();
        }

        bool internet = false;
        private void SendQuery()
        {
            System.Console.WriteLine("sending");

            int from = LowPort;
            int to = HighPort;
            QueryPacket q = new QueryPacket();
            q.Type = QueryPacket.Phase.Request;

            try
            {
                for (int i = from; i <= to; ++i)
                {
                    //Sock.SendTo(buf, new IPEndPoint(IPAddress.Broadcast, i));
                    //client.SetRemoteHost(IPAddress.Broadcast, i);
                    //client.Send(q);
                }
            }
            catch (SocketException)
            {
            }

            if (internet)
            {
                foreach (string host in Hosts)
                {
                    try
                    {
                        IPAddress ip = Dns.GetHostEntry(host).AddressList[0];
                        for (int i = from; i <= to; ++i)
                        {
                            //Sock.SendTo(buf, new IPEndPoint(ip, i));
                            //client.SetRemoteHost(ip, i);
                            //client.Send(q);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        internet = false;
                    }
                }
            }
            //int n=Sock.SendTo(buf, new IPEndPoint(IPAddress.Parse("192.168.0.10"), 1337));
        }

        public void Tick(float dtime)
        {
            //byte[] buf=new byte[1024*16];
            //Datagram dg;
            //EndPoint ep=new IPEndPoint(IPAddress.Any,0);
            int n;
            
            //try
            {
              //  while ((dg = client.Receive())!=null)
                {
                    Console.WriteLine("answer received.");
                    //QueryPacket p=(QueryPacket)Root.Instance.Factory.DeSerialize(new MemoryStream(buf,4,buf.Length-4));
                    //ISerializable p = (ISerializable)Root.Instance.Factory.DeSerialize(dg.GetStream());

                    //Servers[dg.Sender] = p;
                    //if (Answer!=null)
                    //    Answer(p, dg.Sender);
                }
            }
            //catch (SocketException)
            {
            }

            Timer += dtime;
            if (Timer > QueryInterval)
            {
                Timer = 0;
                SendQuery();
            }
        }

        public delegate void AnswerDelegate(ISerializable answer,IPEndPoint ep);
        public event AnswerDelegate Answer;

        public Hashtable Servers=new Hashtable();
        UdpClient client;
        public float QueryInterval=5;
        float Timer=0;

        int LowPort;
        int HighPort;

        string[] Hosts;
    }

    public class Events : Packet
    {
        public Events(ICollection events)
        {
            this.EventList = events;
        }

        public Events(DeSerializationContext context)
        {
			DeSerialize(context);
		}

        public void RaiseAll()
        {
            foreach (EventReplicationInfo e in EventList)
            {
                e.Raise();
            }
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            List<EventReplicationInfo> events = new List<EventReplicationInfo>();
            int n=(int)context.ReadByte();
            for (int i = 0; i < n; ++i)
            {
                events.Add(new EventReplicationInfo(context));
            }

            EventList = events;
        }

        public override void Serialize(SerializationContext context)
        {
            context.Write((byte)EventList.Count);
            foreach (EventReplicationInfo e in EventList)
            {
                e.Serialize(context);
            }
        }

        public override string ToString()
        {
            string s = "";
            foreach (EventReplicationInfo e in EventList)
            {
                s += e.ToString()+"\n";
            }
            return s;
        }

        ICollection EventList;
    }

    public class State : Packet
	{
		public State(Scene s)
		{
			Scene=s;
		}

        public State(DeSerializationContext context)
        {
			DeSerialize(context);
		}

		public override void DeSerialize(DeSerializationContext context)
		{
			//Root.Instance.OnStateDataReceive(s,r);
		}

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
			Scene.Serialize(context);
		}

		public override string ToString()
		{
			return Scene.ToString();
		}

		public Scene Scene;
	}

	public struct ConnectionStatistics
	{
		public int PacketsIn;
		public int BytesIn;
		public int PacketsOut;
		public int BytesOut;
		public int UncompressedBytesOut;
		public int UncompressedBytesIn;

        /*public int KBpsIn;
        public int KBpsOut;
        public int PpsIn;
        public int PpsOut;
*/

        public ConnectionStatistics(int pi,int bi,int po,int bo,int ubi,int ubo)
		{
			PacketsIn=pi;
			BytesIn=bi;
			PacketsOut=po;
			BytesOut=bo;
			UncompressedBytesOut=ubo;
			UncompressedBytesIn=ubi;
		}

        public static ConnectionStatistics operator - (ConnectionStatistics s1,ConnectionStatistics s2)
        {
            return new ConnectionStatistics(
                s1.PacketsIn - s2.PacketsIn,
                s1.BytesIn - s2.BytesIn,
                s1.PacketsOut - s2.PacketsOut,
                s1.BytesOut - s2.BytesOut,
                s1.UncompressedBytesIn - s2.UncompressedBytesIn,
                s1.UncompressedBytesOut - s2.UncompressedBytesOut
                );

        }

		public float CompressionRatioIn
		{
			get{return (float)UncompressedBytesIn/(float)BytesIn;}
		}
		public float CompressionRatioOut
		{
			get{return (float)UncompressedBytesOut/(float)BytesOut;}
		}
		public float CompressionRatio
		{
			get{return (float)UncompressedBytes/(float)Bytes;}
		}
		public int Bytes
		{
			get{return BytesIn+BytesOut;}
		}
		public int UncompressedBytes
		{
			get{return UncompressedBytesIn+UncompressedBytesOut;}
		}
		public int Packets
		{
			get{return PacketsOut+PacketsIn;}
		}

        public int AvgPacketSizeOut
        {
            get { return BytesOut / (PacketsOut != 0 ? PacketsOut : 1); }
        }

        public int AvgPacketSizeIn
        {
            get { return BytesIn / (PacketsIn != 0 ? PacketsIn : 1); }
        }
        
        public int AvgPacketSize
        {
            get { return Bytes / (Packets!=0?Packets:1); }
        }

        public override string ToString()
		{
            string format = "#0.0";
            return
                "out: " + PacketsOut + "/" + ((float)BytesOut / 1000.0f).ToString(format) + "k/" + AvgPacketSizeOut + "/" + CompressionRatioOut.ToString(format) +
                ", in: " + PacketsIn + "/" + ((float)BytesIn / 1000.0f).ToString(format) + "k/" + AvgPacketSizeIn + "/" + CompressionRatioIn.ToString(format);
            //", comp: "+string.Format(format,CompressionRatioOut)+"/"+string.Format(format,CompressionRatioIn)+" out/in";
		}
	}

	public interface IConnection
	{
		void Send(NetMessage m);
        NetMessage Receive();
        ConnectionStatistics Statistics
		{
			get;
		}
        void Disconnect();
	}




    [Flags]
    public enum EntityFlags
    {
        None=0,Override,Empty,ServerNoOverride
    }

    /*
	public class UdpClient3 : IConnection
	{
        bool compress;

		public UdpClient3(string host,int port, string name,string password)
		{
            compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
			Connect(host,port,name,password);
		}

        public UdpClient3(string host, int port)
        {
            compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
            SetRemoteHost(host, port);
        }
        public UdpClient3()
        {
            compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
        }
        public UdpClient3(Socket s)
        {
            compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
            Sock = s;
            Sock.Blocking = true;
        }
		public virtual void Send(byte[] data, int index, int length)
		{
			if(EP!=null)
			{
                byte[] data2 = new byte[data.Length + 4];
                BinaryWriter w = new BinaryWriter(new MemoryStream(data2));
                w.Write(LastServerTimeStamp);
                w.Write(data, index, length);
                w.Flush();
                w.BaseStream.Flush();
                w.Close();
         
                byte[] compressed;
                int compressedlength;
                if (compress)
                {
                    compressed = new byte[8192];
                    //compressed = new byte[data2.Length];
                    compressedlength = ZipCompressor.Instance.Compress(data2, data2.Length, compressed);
                }
                else
                {
                    compressed = data2;
                    compressedlength = data2.Length;
                }
				stats.PacketsOut++;
                stats.BytesOut += compressedlength;
				stats.UncompressedBytesOut+=data2.Length;
				Sock.SendTo(compressed,0,compressedlength,SocketFlags.None,EP);
			}
			else
				throw new Exception();
		}
		
		public virtual void Send(ISerializable obj)
		{
			MemoryStream m=new MemoryStream();
			//Root.Instance.Factory.Serialize(m,obj);
            byte[] buf = m.GetBuffer();
            //Console.WriteLine(buf[0].ToString() +" "+ buf[1].ToString());
			Send(buf,0,(int)m.Position);
		}

		public virtual Datagram Receive()
		{
			return Receive(false);
		}

		protected Datagram Receive(bool block)
		{
			Sock.Blocking=block;
			
			//if(EP==null)
			//	throw new Exception();

			byte[] buffer=new byte[8192];
			try
			{
				IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				EndPoint tempRemoteEP = (EndPoint)sender;
				int i=Sock.ReceiveFrom(buffer,ref tempRemoteEP);
				stats.PacketsIn++;
				stats.BytesIn+=i;

                byte[] decompressed;
                int decompressedlength;
                if (compress)
                {
                    decompressed = new byte[8192];
                    decompressedlength = ZipCompressor.Instance.Decompress(buffer, i, decompressed);
                }
                else
                {
                    decompressedlength = i;
                    decompressed = buffer;
                }
                if (decompressedlength < i)
                    Console.WriteLine("compressor bug?!");

				//Array.Copy(buffer,b,i);
				//byte[] decompressed=Compressor.DeCompress(b);
                stats.UncompressedBytesIn += decompressedlength;

                //byte[] data2 = new byte[decompressed.Length - 4];
                BinaryReader r = new BinaryReader(new MemoryStream(decompressed));
                float f = r.ReadSingle();
                LastServerTimeStamp = f;
                //Array.Copy(decompressed, 4, data2, 0, data2.Length);

                //Console.WriteLine("rcvd " + i +" "+decompressedlength);
                //HACK
                return new Datagram(decompressed, 4, decompressedlength - 4, (IPEndPoint)tempRemoteEP);
			}
			catch(Exception)
			{
				return null;
			}
		}

		protected void CreateSocket()
		{
			Sock=new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
			Sock.Blocking=true;
		}

        public void SetRemoteHost(IPEndPoint ep)
        {
            if (Sock == null)
                CreateSocket();

            EP = ep;
        }
        public void SetRemoteHost(IPAddress ip, int port)
        {
            if (Sock == null)
                CreateSocket();

            EP = new IPEndPoint(ip, port);
        }
        public void SetRemoteHost(string host, int port)
		{
			if(Sock==null)
				CreateSocket();

            IPAddress ip=Dns.Resolve(host).AddressList[0];
            EP=new IPEndPoint(ip,port);
		}

		public QueryPacket Query(string host,int port)
		{
			SetRemoteHost(host,port);

			QueryPacket p=new QueryPacket();
			p.Type=QueryPacket.Phase.Request;
			Send(p);
			Datagram d=Receive(true);
			return (QueryPacket)Root.Instance.Factory.DeSerialize(d.GetStream());
		}

		public virtual void Connect(string host,int port,string name,string password)
		{
			SetRemoteHost(host,port);

            Stream m=null;
            Datagram dg=null;
            for (int i = 0; i < 10; ++i)
			{
				//request
                Handshake hs=new Handshake(Handshake.Phase.Request, Handshake.MagicDefault, 0, "Join Request",password);
                hs.Name = name;
                Send(hs);
			
				//check answer from server
				dg=Receive(true);
				if(dg!=null)
				{
					m=dg.GetStream();
					break;
				}
				Thread.Sleep(5000);
			}
			if(m==null)
				throw new Exception("cant connect.");

            Handshake h;
            try
            {
                h = (Handshake)Root.Instance.Factory.DeSerialize(m);
            }
            catch (Exception e)
            {
                Console.WriteLine("exception raised while receiving handshake. packet dumped.");
                FileStream f = new FileStream("packet.dump", FileMode.Create, FileAccess.Write);
                byte[] data = dg.GetData();
                f.Write(data, dg.Index, dg.Length);
                f.Close();
                throw e;

            }
            if (h.Type != Handshake.Phase.Accept)
                throw new Exception("request denied");

			Sock.Blocking=false;
			//IPEndPoint LocalEP=(IPEndPoint)Sock.LocalEndPoint;
			Console.WriteLine("connected to server: clientnumber: "+h.ClientNumber+" \""+h.Info+"\"");
			ClientNumber=h.ClientNumber;
			//Address=address;
		}

		public virtual void Disconnect()
		{
			Send(new Handshake(Handshake.Phase.Quit,Handshake.MagicDefault,ClientNumber,"Quit"));
			Sock.Close();
			Sock=null;
		}

		public ConnectionStatistics Statistics
		{
			get{return stats;}
		}

		protected Socket Sock;
		protected IPEndPoint EP;
		public short ClientNumber;
        public float LastServerTimeStamp;

		protected ConnectionStatistics stats;
		//public string Address;
	}
     * 
    /*
	public class UdpServer3 : IConnection, ITickable
	{
		public class Slot
		{
			public Slot(IPEndPoint ep,short n,string name)
			{
				ClientEndPoint=ep;
				ClientNumber=n;
                Name = name;
            }

			public override string ToString()
			{
				return "#"+ClientNumber+":\t\""+Name+"\"\t["+ClientEndPoint.ToString()+"] RTT: "+(int)(AvgRTT*1000)+"ms";
			}

            public void OnMeasureRTT(float rtt)
            {
                NumRTT++;
                RTT = rtt;
                SumRTT += rtt;
            }

            public void UpdateRTT()
            {
                if (NumRTT != 0)
                    AvgRTT = SumRTT / (float)NumRTT;
                else
                    AvgRTT = RTT;
                ResetRTT();
            }

            public void ResetRTT()
            {
                NumRTT = 0;
                SumRTT = 0;
                RTT = 0;
            }

			public IPEndPoint ClientEndPoint;
			public short ClientNumber;
            public float LastPacketTime;
            public string Name;
            public float RTT;
            public int NumRTT;
            public float SumRTT;
            public float AvgRTT;
        }

        bool compress;
		public UdpServer3(int port,int maxclients,string password)
		{
            compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
            Password = password;
			Clients=new Slot[maxclients];
			Listen(port);
		}

		public override string ToString()
		{
			string s="Slots: "+Clients.Length+"\r\n";
			for(int i=0;i<Clients.Length;++i)
			{
				if(Clients[i]!=null)
				s+=i+":\t"+Clients[i].ToString()+"\r\n";
			}
			return s;
		}

		protected void AddClient(IPEndPoint ep,short n,string name)
		{
			for(int i=0;i<Clients.Length;++i)
			{
				if(Clients[i]==null)
				{
					Clients[i]=new Slot(ep,n,name);
                    NumClients++;
					return;
				}
			}
		}

        protected Slot GetClient(IPEndPoint ep)
        {
            for (int i = 0; i < Clients.Length; ++i)
            {
                if (Clients[i] != null && Clients[i].ClientEndPoint.ToString() == ep.ToString())
                {
                    return Clients[i];
                }
            }
            return null;
        }

        public short GetClientNumber(IPEndPoint ep)
		{
			for(int i=0;i<Clients.Length;++i)
			{
				if(Clients[i]!=null&&Clients[i].ClientEndPoint.ToString()==ep.ToString())
				{
					return Clients[i].ClientNumber;
				}
			}
			return -1;
		}

        public Slot GetClient(short id)
        {
            for (int i = 0; i < Clients.Length; ++i)
            {
                if (Clients[i] != null && Clients[i].ClientNumber == id)
                {
                    return Clients[i];
                }
            }
            return null;
        }

		public bool ClientExists(short id)
		{
			for(int i=0;i<Clients.Length;++i)
			{
				if(Clients[i]!=null&&Clients[i].ClientNumber==id)
				{
					return true;
				}
			}
			return false;
		}

		public bool ClientExists(IPEndPoint ep)
		{
			for(int i=0;i<Clients.Length;++i)
			{
				if(Clients[i]!=null&&Clients[i].ClientEndPoint.ToString()==ep.ToString())
				{
					return true;
				}
			}
			return false;
		}

        public void RemoveAllClients()
        {
            for (int i = 0; i < Clients.Length; ++i)
            {
                if (Clients[i] != null)
                {
                    Clients[i] = null;
                    NumClients--;
                    return;
                }
            }
        }

        public void Disconnect()
        {
            RemoveAllClients();
            Sock.Close();
            Sock = null;
        }

		protected void RemoveClient(IPEndPoint ep)
		{
			for(int i=0;i<Clients.Length;++i)
			{
				if(Clients[i]!=null&&Clients[i].ClientEndPoint.ToString()==ep.ToString())
				{
					Clients[i]=null;
                    NumClients--;
					return;
				}
			}
		}

		protected int GenerateNumber()
		{
			return Root.Instance.NextIndex++;
		}

		public virtual void Listen(int port)
		{
			Sock=new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
			Sock.Blocking=false;
			Sock.Bind(new IPEndPoint(IPAddress.Any,port));
			Console.WriteLine("listening on port "+port);
            Port = port;
		}

		public void Tick(float dtime)
		{
            //return;

			Datagram ri;
            Slot s1;
            while((ri=Receive(false))!=null)
			{
				BinaryReader r=new BinaryReader(ri.GetStream());
				//string s=r.ReadString();
                short typeid = -1;// r.ReadInt16();
                Type type = null;
                try
                {
                    //float t = r.ReadSingle();
                    typeid = r.ReadInt16();
                    type = Root.Instance.Factory.GetType(typeid);
                }
                catch (Exception)
                {
                    Console.WriteLine("received packet with wrong id: "+typeid);
                    continue;
                }

                r.BaseStream.Position=0;
				if(type==typeof(Handshake))
				{
					Handshake h=null;
                    //try
                    //{
                        h=(Handshake)Root.Instance.Factory.DeSerialize(ri.GetStream());//r.BaseStream);
                    //}
                    //catch(EndOfStreamException)
                    //{
                    //}

                    if (h.Magic == Handshake.MagicDefault)
                    {
                        if (h.Type == Handshake.Phase.Request && !ClientExists(ri.Sender))
                        {
                            if (h.Version != Handshake.ThisVersion)
                            {
                                //wrong version
                                Console.WriteLine("client denied(wrong version): " + h.Name + " \"" + h.Info + "\"" + " [" + ri.Sender.ToString() + "]");
                                Send(new Handshake(Handshake.Phase.Deny, Handshake.MagicDefault, -1, "Wrong Version"), ri.Sender);
                            }
                            else if (Password == null || Password == "" || Password == h.Password)
                            {
                                short n = (short)GenerateNumber();
                                AddClient(ri.Sender, n, h.Name);
                                GetClient(ri.Sender).LastPacketTime = Root.Instance.Time;
                                Console.WriteLine("client connected: " + n + ": " + h.Name + " \"" + h.Info + "\"" + " [" + ri.Sender.ToString() + "]");
                                Send(new Handshake(Handshake.Phase.Accept, Handshake.MagicDefault, n, "Join Accept"), ri.Sender);
                                Root.Instance.ServerOnConnect(n, h.Name, ri.Sender);
                            }
                            else
                            {
                                //password wrong
                                Console.WriteLine("client denied(wrong password): " + h.Name + " \"" + h.Info + "\"" + " [" + ri.Sender.ToString() + "]");
                                Send(new Handshake(Handshake.Phase.Deny, Handshake.MagicDefault, -1, "Wrong Password"), ri.Sender);
                            }
                        }
                        else if (h.Type == Handshake.Phase.Quit && ClientExists(ri.Sender))
                        {
                            Slot slot = GetClient(ri.Sender);
                            Root.Instance.ServerOnDisconnect(slot.ClientNumber, slot.Name, ri.Sender);
                            RemoveClient(ri.Sender);
                            Console.WriteLine("client disconnected: " + h.ClientNumber + " \"" + h.Info + "\"" + " [" + ri.Sender.ToString() + "]");
                        }
                        else
                        {
                            //packet makes no sense - drop!
                        }
                    }
                    else
                    {
                        //number wrong, ignore
                    }
				}
                else if ((s1 = GetClient(ri.Sender)) != null)
                {
                    s1.LastPacketTime = Root.Instance.Time;
                    RecvQueue.Enqueue(ri);
                }
                else//unconnected client
                {
                    RecvQueue.Enqueue(ri);
                }
            }

            for (int i = 0; i < Clients.Length; ++i)
            {
                if (Clients[i] != null)
                {
                    if (Root.Instance.Time - Clients[i].LastPacketTime > 30)
                    {
                        Slot slot = Clients[i];
                        Console.WriteLine("client timeout: " + Clients[i].ClientNumber + " [" + Clients[i].ClientEndPoint.ToString() + "]");
                        Root.Instance.ServerOnDisconnect(slot.ClientNumber, slot.Name, slot.ClientEndPoint);
                        RemoveClient(Clients[i].ClientEndPoint);
                    }
                }
            }
            
            UpdateRTT += dtime;
            if (UpdateRTT >= 3)
            {
                UpdateRTT = 0;
                for (int i = 0; i < Clients.Length; ++i)
                {
                    if (Clients[i] != null)
                    {
                        Clients[i].UpdateRTT();
                    }
                }
            }
        }

        public virtual void Send(ISerializable obj)
		{
            if (NumClients == 0 && Root.Instance.Recorder==null)
                return;

            byte[] buf = new byte[8192];
            MemoryStream m = new MemoryStream(buf);

            Root.Instance.Factory.Serialize(m, obj);

            if (Root.Instance.Recorder != null)
            {
                Root.Instance.Recorder.WritePacket((int)(Root.Instance.Time * 1000), buf, (int)m.Position);
            }

            foreach (Slot s in Clients)
            {
				if(s!=null)
					Send(buf,0,(int)m.Position,s.ClientEndPoint);
			}
		}
		
		public virtual void Send(ISerializable obj,IPEndPoint ep)
		{
            MemoryStream m = new MemoryStream();
			Root.Instance.Factory.Serialize(m,obj);
            byte[] buf = m.GetBuffer();
			//m.Seek(0,SeekOrigin.Begin);
			//m.Read(buf,0,(int)m.Length);
			Send(buf,0,(int)m.Position,ep);
		}
        public virtual void SendNot(ISerializable obj, IPEndPoint ep)
        {
            if (NumClients == 0)
                return;
            MemoryStream m = new MemoryStream();
            Root.Instance.Factory.Serialize(m, obj);
            byte[] buf = m.GetBuffer();
            //m.Seek(0, SeekOrigin.Begin);
            //m.Read(buf, 0, (int)m.Length);

            SendNot(buf, 0, (int)m.Position,ep);
        }

		public virtual void Send(byte[] data, int index, int length)
		{
            foreach (Slot s in Clients)
			{
                if(s!=null)
				    Send(data,index,length,s.ClientEndPoint);
			}
		}

        public virtual void SendNot(byte[] data, int index, int length, IPEndPoint ep)
        {
            if (NumClients == 0 && Root.Instance.Recorder==null)
                return;
            if (ep == null)
                throw new Exception("ep==null");
            if (Root.Instance.Recorder != null)
            {
                Root.Instance.Recorder.WritePacket((int)(Root.Instance.Time * 1000), data, length);
            }
            foreach (Slot s in Clients)
            {
                if (s != null)
                {
                    if (ep.ToString() != s.ClientEndPoint.ToString())
                        Send(data, index, length, s.ClientEndPoint);
                    //else System.Console.WriteLine("sening not to " + ep.ToString());
                }
            }
        }

		public virtual void Send(byte[] data,int index,int length,IPEndPoint ep)
		{
            //return;
            byte[] data2 = new byte[length + 4];
            BinaryWriter w = new BinaryWriter(new MemoryStream(data2));
            w.Write(Root.Instance.Time);
            w.Write(data, index, length);
            w.Flush();
            w.BaseStream.Flush();
            w.Close();

            stats.PacketsOut++;
            stats.UncompressedBytesOut += length - index;

            byte[] compressed;
            int compressedlength;
            if (compress)
            {
                //compressed = new byte[data2.Length];
                compressed = new byte[8192];
                compressedlength = ZipCompressor.Instance.Compress(data2, data2.Length, compressed);
            }
            else
            {
                compressed = data2;
                compressedlength = data2.Length;
            }

            stats.BytesOut += compressedlength;

           // Console.WriteLine("send " + compressedlength +" "+data2.Length);

            //int i=Root.Instance.TickCount();
            Sock.SendTo(compressed, 0, compressedlength, SocketFlags.None, ep);
        }

		public Datagram Receive()
		{
			if(RecvQueue.Count>0)
			{
				Datagram ri=(Datagram)RecvQueue.Dequeue();
				return ri;
			}
			else
				return null;
		}

        //byte[] buffer2 = new byte[8192];
        byte[] buffer = new byte[8192];
        protected Datagram Receive(bool block)
		{
			Sock.Blocking=block;

			try
			{
                //byte[] buffer = new byte[8192];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				EndPoint tempRemoteEP = (EndPoint)sender;
				int i=Sock.ReceiveFrom(buffer,ref tempRemoteEP);

                stats.PacketsIn++;
                stats.BytesIn += i;

                byte[] decompressed;
                int decompressedlength;
                if (compress)
                {
                    decompressed = new byte[8192];
                    decompressedlength = ZipCompressor.Instance.Decompress(buffer, i, decompressed);
                }
                else
                {
                    decompressedlength = i;
                    decompressed = buffer;
                }
                stats.UncompressedBytesIn += decompressedlength;

                BinaryReader r = new BinaryReader(new MemoryStream(decompressed));
                float f = r.ReadSingle();
                //Array.Copy(decompressed, 4, data2, 0, data2.Length);
                sender=(IPEndPoint)tempRemoteEP;
                Slot s=GetClient(sender);
                if (s != null)
                {
                    s.OnMeasureRTT(Root.Instance.Time - f);
                }
                Datagram dg = new Datagram(decompressed, 4, decompressedlength - 4, (IPEndPoint)tempRemoteEP);
                dg.Connected=s!=null;
                return dg;
            }
			catch(Exception)
			{
				return null;
			}
		}
		
		public ConnectionStatistics Statistics
		{
			get{return stats;}
		}

        public int MaxClients
        {
            get
            {
                return Clients.Length;
            }
        }

        public int ConnectedClients
        {
            get
            {
                return NumClients;
            }
        }

		protected Socket Sock;
		public Slot[] Clients;
		protected Queue RecvQueue=new Queue();
        public string Password;
		protected ConnectionStatistics stats;
        public int NumClients=0;
        float UpdateRTT;
        public int Port;
	}

    */



    public class UdpClient : IConnection
    {
        bool compress;

        public UdpClient(string host, int port, string name, string password)
            :this()
        {
            
            Connect(host, port, name, password);
        }

        public UdpClient()
        {
            compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
            log = new NetLog();
            client = new NetClient(new NetAppConfiguration("spacewar2006-1"), log);
        }

        public virtual void Send(NetMessage m)
        {
            //if (EP != null)
            /*{
                byte[] data2 = new byte[data.Length + 4];
                BinaryWriter w = new BinaryWriter(new MemoryStream(data2));
                w.Write(LastServerTimeStamp);
                w.Write(data, index, length);
                w.Flush();
                w.BaseStream.Flush();
                w.Close();

                byte[] compressed;
                int compressedlength;
                if (compress)
                {
                    compressed = new byte[8192];
                    //compressed = new byte[data2.Length];
                    compressedlength = ZipCompressor.Instance.Compress(data2, data2.Length, compressed);
                }
                else
                {
                    compressed = data2;
                    compressedlength = data2.Length;
                }
                stats.PacketsOut++;
                stats.BytesOut += compressedlength;
                stats.UncompressedBytesOut += data2.Length;

                NetMessage m = new NetMessage(compressedlength);
                m.Write(compressed,0,compressedlength);
                client.SendMessage(m,NetChannel.Unreliable);
                //System.Console.WriteLine("send: " + m.Length);
                //Sock.SendTo(compressed, 0, compressedlength, SocketFlags.None, EP);
            }
            //else
            //    throw new Exception();*/

            client.SendMessage(m, NetChannel.Unreliable);
            client.Heartbeat();
        }

        public virtual void Send(ISerializable obj)
        {
            SerializationContext c = new SerializationContext();
            Root.Instance.Factory.Serialize(c, obj);
            byte[] buf = c.ToArray();
            //Console.WriteLine(buf[0].ToString() +" "+ buf[1].ToString());
            Send(c.GetMessage());
        }

        public virtual NetMessage Receive()
        {
            return Receive(false);
        }

        protected NetMessage Receive(bool block)
        {
            client.Heartbeat();
            return client.ReadMessage();
            //Sock.Blocking = block;

            //if(EP==null)
            //	throw new Exception();

            //byte[] buffer = new byte[8192];
            //try
            /*{
                //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                //EndPoint tempRemoteEP = (EndPoint)sender;
                //int i = Sock.ReceiveFrom(buffer, ref tempRemoteEP);
                NetMessage m = client.ReadMessage();
                if (m == null)
                    return null;
                byte[] buffer = m.ReadBytes(m.Length);
                int i = m.Length;

                stats.PacketsIn++;
                stats.BytesIn += m.Length;

                byte[] decompressed;
                int decompressedlength;
                if (compress)
                {
                    decompressed = new byte[8192];
                    decompressedlength = ZipCompressor.Instance.Decompress(buffer, i, decompressed);
                }
                else
                {
                    decompressedlength = i;
                    decompressed = buffer;
                }
                if (decompressedlength < i)
                    Console.WriteLine("compressor bug?!");

                //Array.Copy(buffer,b,i);
                //byte[] decompressed=Compressor.DeCompress(b);
                stats.UncompressedBytesIn += decompressedlength;

                //byte[] data2 = new byte[decompressed.Length - 4];
                BinaryReader r = new BinaryReader(new MemoryStream(decompressed));
                float f = r.ReadSingle();
                LastServerTimeStamp = f;
                //Array.Copy(decompressed, 4, data2, 0, data2.Length);

                //Console.WriteLine("rcvd " + i +" "+decompressedlength);
                //HACK
                return new Datagram(decompressed, 4, decompressedlength - 4, m.Sender.RemoteEndpoint);
            }*/
            //catch (Exception)
            //{
            //    return null;
            //}
        }
        
        public QueryPacket Query(string host, int port)
        {
            return null;
        }
        
        public virtual void Connect(string host, int port, string name, string password)
        {
            if (!client.Connect(host, port))
                throw new Exception("cant connect");

            //read client number
            int i = 0;
            short n=-1;
            while(i++<10)
            {
                client.Heartbeat();
                Thread.Sleep(100);
                NetMessage m = client.ReadMessage();
                if (m != null && m.SequenceChannel==NetChannel.Ordered1)
                {
                    n = m.ReadInt16();
                    System.Console.WriteLine(n);
                    break;
                }
            }
            if (n == -1)
                throw new Exception();

            Console.WriteLine("connected to server: clientnumber: " + n + " \"" + "???" + "\"");
            ClientNumber = n;
        }

        public virtual void Disconnect()
        {
            client.Disconnect("");
            client = null;
            ClientNumber = -1;

        }

        public static ConnectionStatistics Convert(NetStatistics n)
        {
            ConnectionStatistics c = new ConnectionStatistics(
                n.PacketsReceived, n.BytesReceived,
                n.PacketsSent, n.BytesSent,
                n.BytesReceived, n.BytesSent);
            return c;
        }

        public ConnectionStatistics Statistics
        {
            get { return Convert(client.ServerConnection.Statistics); }
        }

        //protected Socket Sock;
        public NetClient client;
        public NetLog log;
        protected IPEndPoint EP;
        public short ClientNumber;
        public float LastServerTimeStamp;

        protected ConnectionStatistics stats;
        //public string Address;
    }




    public class UdpServer : IConnection, ITickable
    {

        bool compress;
        NetLog log;

        public UdpServer(int port, int maxclients, string password)
        {
            compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
            Password = password;
            log=new NetLog();
            NetAppConfiguration c = new NetAppConfiguration("spacewar2006-1", 31400);
            c.MaximumConnections = 8;
            server = new NetServer(c, log);
            server.StatusChanged += new EventHandler<NetStatusEventArgs>(server_StatusChanged);
        }

        void server_StatusChanged(object sender, NetStatusEventArgs e)
        {
            if (e.Connection.Status == NetConnectionStatus.Connected)
            {
                NetMessage m = new NetMessage(2);
                int n=Array.IndexOf<NetConnection>(server.Connections, e.Connection) + 1;
                m.Write((short)n);
                server.SendMessage(m, e.Connection, NetChannel.Ordered1);
                //server.FlushMessages();
                System.Console.WriteLine("sending client number "+n);
            }
        }

        public void RemoveAllClients()
        {
        }

        public int NumClients
        {
            get
            {
                return server.NumConnected;
            }
        }
        public NetConnection[] Clients
        {
            get
            {
                return server.Connections;
            }
        }
        public override string ToString()
        {
            string s = "Slots: " + Clients.Length + "\r\n";
            for (int i = 0; i < Clients.Length; ++i)
            {
                if (Clients[i] != null)
                    s += i + ":\t" + Clients[i].ToString() + "\r\n";
            }
            return s;
        }


        public short GetClientNumber(IPEndPoint ep)
        {
            NetConnection c = server.FindConnection(ep);
            if (c == null)
                return -1;

            return (short)(Array.IndexOf<NetConnection>(server.Connections, c) + 1);
        }

        public bool ClientExists(short id)
        {
            return server.Connections[id - 1] != null;
        }

        public bool ClientExists(IPEndPoint ep)
        {
            return server.FindConnection(ep) != null;
        }


        public void Disconnect()
        {
            server.Shutdown("");
        }


        protected int GenerateNumber()
        {
            return Root.Instance.NextIndex++;
        }


        public void Tick(float dtime)
        {
            server.Heartbeat();
        }

        public virtual void Send(ISerializable obj)
        {
            if (server.NumConnected == 0 && Root.Instance.Recorder == null)
                return;

            
            //MemoryStream m = new MemoryStream(buf);
            //NetMessage m = new NetMessage();
            SerializationContext c = new SerializationContext();
            c.Serialize(obj);
            byte[] buf = c.ToArray();
            if (Root.Instance.Recorder != null)
            {
                Root.Instance.Recorder.WritePacket((int)(Root.Instance.Time * 1000), buf, buf.Length);
            }

            foreach (NetConnection s in Clients)
            {
                if (s != null)
                    Send(c.GetMessage(), s.RemoteEndpoint);
            }
        }

        public virtual void Send(ISerializable obj, IPEndPoint ep)
        {
            //MemoryStream m = new MemoryStream();
            //NetMessage m = new NetMessage();
            SerializationContext c = new SerializationContext();
            Root.Instance.Factory.Serialize(c, obj);
            byte[] buf = c.ToArray();
            //m.Seek(0,SeekOrigin.Begin);
            //m.Read(buf,0,(int)m.Length);
            Send(c.GetMessage(), ep);
        }
        public virtual void SendNot(ISerializable obj, IPEndPoint ep)
        {
            if (server.NumConnected == 0)
                return;
            //NetMessage m = new NetMessage();
            SerializationContext c = new SerializationContext();
            Root.Instance.Factory.Serialize(c, obj);
            byte[] buf = c.ToArray();
            //m.Seek(0, SeekOrigin.Begin);
            //m.Read(buf, 0, (int)m.Length);

            SendNot(c.GetMessage(), ep);
        }

        public virtual void Send(NetMessage m)
        {
            foreach (NetConnection s in Clients)
            {
                if (s != null)
                    Send(m, s.RemoteEndpoint);
            }
        }

        public virtual void SendNot(NetMessage m, IPEndPoint ep)
        {
            if (server.NumConnected == 0 && Root.Instance.Recorder == null)
                return;
            if (ep == null)
                throw new Exception("ep==null");

            if (Root.Instance.Recorder != null)
            {
                byte[] buf = m.ToArray();
                Root.Instance.Recorder.WritePacket((int)(Root.Instance.Time * 1000), buf, buf.Length);
            }
            foreach (NetConnection s in Clients)
            {
                if (s != null)
                {
                    if (ep.ToString() != s.RemoteEndpoint.ToString())
                        Send(m, s.RemoteEndpoint);
                    //else System.Console.WriteLine("sening not to " + ep.ToString());
                }
            }
        }

        public virtual void Send(NetMessage m, IPEndPoint ep)
        {
            //return;
            /*byte[] data2 = new byte[length + 4];
            BinaryWriter w = new BinaryWriter(new MemoryStream(data2));
            w.Write(Root.Instance.Time);
            w.Write(data, index, length);
            w.Flush();
            w.BaseStream.Flush();
            w.Close();

            stats.PacketsOut++;
            stats.UncompressedBytesOut += length - index;

            byte[] compressed;
            int compressedlength;
            if (compress)
            {
                //compressed = new byte[data2.Length];
                compressed = new byte[8192];
                compressedlength = ZipCompressor.Instance.Compress(data2, data2.Length, compressed);
            }
            else
            {
                compressed = data2;
                compressedlength = data2.Length;
            }

            stats.BytesOut += compressedlength;

            // Console.WriteLine("send " + compressedlength +" "+data2.Length);

            //int i=Root.Instance.TickCount();
            //Sock.SendTo(compressed, 0, compressedlength, SocketFlags.None, ep);
            NetMessage m=new NetMessage(compressedlength);
            m.Write(compressed,0,compressedlength);
            server.SendMessage(m, server.FindConnection(ep), NetChannel.Unreliable);
            /*i=Root.Instance.TickCount() - i;
            if (i > 0)
                Console.WriteLine(i.ToString());*/
            server.SendMessage(m, server.FindConnection(ep), NetChannel.Unreliable);

        }

        public NetMessage Receive()
        {
            return Receive(false);
            //NetMessage m = server.ReadMessage();
            /*if (m!=null)
            {
                Datagram ri = new Datagram(m.ToArray(), 0, m.Length, m.Sender.RemoteEndpoint);
                return ri;
            }
            else
                return null;*/
        }

        //byte[] buffer2 = new byte[8192];
        protected NetMessage Receive(bool block)
        {
            return server.ReadMessage();

            //try
           /* {
                //byte[] buffer = new byte[8192];
                //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                //EndPoint tempRemoteEP = (EndPoint)sender;
                NetMessage m = server.ReadMessage();
                if (m == null)
                    return null;

                int i = m.Length;
                byte[] buffer = m.ReadBytes(i);

                //System.Console.WriteLine("rcvd "+i+" bytes.");
                stats.PacketsIn++;
                stats.BytesIn += i;

                byte[] decompressed;
                int decompressedlength;
                if (compress)
                {
                    decompressed = new byte[8192];
                    decompressedlength = ZipCompressor.Instance.Decompress(buffer, i, decompressed);
                }
                else
                {
                    decompressedlength = i;
                    decompressed = buffer;
                }
                stats.UncompressedBytesIn += decompressedlength;

                BinaryReader r = new BinaryReader(new MemoryStream(decompressed));
                float f = r.ReadSingle();
                //Array.Copy(decompressed, 4, data2, 0, data2.Length);
                //sender = m.Sender.RemoteEndpoint;
                /*Net s = GetClient(sender);
                if (s != null)
                {
                    s.OnMeasureRTT(Root.Instance.Time - f);
                }*/
                //Datagram dg = new Datagram(decompressed, 4, decompressedlength - 4, m.Sender.RemoteEndpoint);
                //Datagram dg = new Datagram(data2, 4, decompressedlength - 4, m.Sender.RemoteEndpoint);
                //dg.Connected = true;
                //return dg;
            //}
            //catch (Exception)
            //{
            //    return null;
            //}
        }

        public ConnectionStatistics Statistics
        {
            get { return new ConnectionStatistics(); }
        }

        public int MaxClients
        {
            get
            {
                return server.Configuration.MaximumConnections;
            }
        }

        public int ConnectedClients
        {
            get
            {
                return server.NumConnected;
            }
        }

        public NetServer server;
        //public Slot[] Clients;
        //protected Queue RecvQueue = new Queue();
        public string Password;
        protected ConnectionStatistics stats;
        //public int NumClients = 0;
        float UpdateRTT;
        public int Port;
    }





}
