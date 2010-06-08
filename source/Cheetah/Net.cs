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
using Lidgren.Network;

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

    public class InternetScanner : ITickable
    {
        struct Server
        {
            public Server(string host, int port)
            {
                Host = host;
                Port = port;
            }
            public string Host;
            public int Port;
        }
        public InternetScanner(AnswerDelegate answer)
        {
            Answer = answer;
            string url = @"http://spacewar-arena.com/spacewar-arena-servers.xml";

            WebClient wclient = new WebClient();
            XmlDocument doc = new XmlDocument();
            Stream strm = wclient.OpenRead(url);
            doc.Load(strm);

            XmlNode list = doc.GetElementsByTagName("serverlist")[0];
            foreach (XmlNode server in list.ChildNodes)
            {
                if (server.Name == "server")
                {
                    string host = server.Attributes["host"].Value;
                    string port = server.Attributes["port"].Value;

                    servers.Add(new Server(host, int.Parse(port)));
                }
            }

            LowPort = ((Config)Root.Instance.ResourceManager.Load("config/global.config")).GetInteger("lanscanner.scanstart");
            HighPort = ((Config)Root.Instance.ResourceManager.Load("config/global.config")).GetInteger("lanscanner.scanend");

            client = new UdpClient();

            SendQuery();
        }

        int LowPort;
        int HighPort;

        private void SendQuery()
        {
            Console.WriteLine("sending");
            foreach (Server ep in servers)
                client.client.DiscoverKnownPeer(ep.Host, ep.Port);

            for (int i = LowPort; i < HighPort; ++i)
            {
                client.client.DiscoverLocalPeers(i);
            }

        }

        public void Tick(float dtime)
        {
            Timer += dtime;
            if (Timer > QueryInterval)
            {
                Timer = 0;
                SendQuery();
            }

            NetIncomingMessage msg;
            while ((msg = client.client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(msg.ReadString());
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        {
                            Console.WriteLine("Found server at " + msg.SenderEndpoint);// + " name: " + msg.ReadString());

                            if (Answer != null)
                            {
                                Answer(Encoding.UTF8.GetBytes(msg.ReadString()),
                                    msg.SenderEndpoint);
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Unhandled type: " + msg.MessageType);
                        break;
                }
                client.client.Recycle(msg);
            }
        }

        public delegate void AnswerDelegate(byte[] answer, IPEndPoint ep);
        public event AnswerDelegate Answer;
        public float QueryInterval = 5;
        float Timer = 0;

        UdpClient client;
        List<Server> servers = new List<Server>();
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
            //client.ServerDiscovered += new EventHandler<NetServerDiscoveredEventArgs>(OnServerDiscovered);

            SendQuery();
        }


        bool internet = false;
        private void SendQuery()
        {
            System.Console.WriteLine("sending");

            int from = LowPort;
            int to = HighPort;

            for (int i = LowPort; i < HighPort; ++i)
            {
                client.DiscoverLocalPeers(i);
            }


            if (internet)
            {
                foreach (string host in Hosts)
                {
                    //client.DiscoverKnownServer(host,
                }
            }
        }

        public void Tick(float dtime)
        {
            Timer += dtime;
            if (Timer > QueryInterval)
            {
                Timer = 0;
                SendQuery();
            }

            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(msg.ReadString());
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        {
                            Console.WriteLine("Found server at " + msg.SenderEndpoint);// + " name: " + msg.ReadString());

                            if (Answer != null)
                            {
                                Answer(Encoding.UTF8.GetBytes(msg.ReadString()),
                                    msg.SenderEndpoint);
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Unhandled type: " + msg.MessageType);
                        break;
                }
                client.Recycle(msg);
            }

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

        public float RoundTripTime;

        public ConnectionStatistics(int pi,int bi,int po,int bo,int ubi,int ubo)
		{
			PacketsIn=pi;
			BytesIn=bi;
			PacketsOut=po;
			BytesOut=bo;
			UncompressedBytesOut=ubo;
			UncompressedBytesIn=ubi;
            RoundTripTime = 0;
		}
        public ConnectionStatistics(long pi, long bi, long po, long bo, long ubi, long ubo)
        {
            PacketsIn = (int)pi;
            BytesIn = (int)bi;
            PacketsOut = (int)po;
            BytesOut = (int)bo;
            UncompressedBytesOut = (int)ubo;
            UncompressedBytesIn = (int)ubi;
            RoundTripTime = 0;
        }
        public static ConnectionStatistics operator - (ConnectionStatistics s1,ConnectionStatistics s2)
        {
            ConnectionStatistics c=new ConnectionStatistics(
                s1.PacketsIn - s2.PacketsIn,
                s1.BytesIn - s2.BytesIn,
                s1.PacketsOut - s2.PacketsOut,
                s1.BytesOut - s2.BytesOut,
                s1.UncompressedBytesIn - s2.UncompressedBytesIn,
                s1.UncompressedBytesOut - s2.UncompressedBytesOut
                );
            c.RoundTripTime = s1.RoundTripTime;
            return c;
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
                ", in: " + PacketsIn + "/" + ((float)BytesIn / 1000.0f).ToString(format) + "k/" + AvgPacketSizeIn + "/" + CompressionRatioIn.ToString(format) +
                ", rtt: " + (int)(RoundTripTime*1000.0f+0.5f) + "ms";
            //", comp: "+string.Format(format,CompressionRatioOut)+"/"+string.Format(format,CompressionRatioIn)+" out/in";
		}
	}

	public interface IConnection
	{
		void Send(NetOutgoingMessage m);
        NetIncomingMessage Receive(out IPEndPoint sender);
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


    public static class NetCompressor
    {
        public static NetOutgoingMessage Compress(NetOutgoingMessage msg)
        {
            msg.WritePadBits();
            byte[] input = msg.PeekDataBuffer();
            byte[] output = new byte[input.Length*2];
            int outlength = ZipCompressor.Instance.Compress(input, msg.LengthBytes, output);
            msg.EnsureBufferSize(outlength*8);
            input = msg.PeekDataBuffer();
            Array.Copy(output, input, outlength);
            msg.LengthBytes = outlength;
            msg.LengthBits = outlength * 8;
            return msg;
        }

        public static NetIncomingMessage Decompress(NetIncomingMessage msg)
        {
            byte[] input = msg.PeekDataBuffer();
            byte[] output=new byte[msg.LengthBytes*10];
            int outlength = ZipCompressor.Instance.Decompress(input, msg.LengthBytes, output);
            return new NetIncomingMessage(output, outlength);
        }
    }

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
            //compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
            NetPeerConfiguration config = new NetPeerConfiguration("spacewar2006-1");
            config.EnableMessageType(NetIncomingMessageType.Data);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.Error);
            config.EnableMessageType(NetIncomingMessageType.Receipt);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);

            client = new NetClient(config);
            client.Start();
        }

        public virtual void Send(NetOutgoingMessage m)
        {
            client.SendMessage(compress?NetCompressor.Compress(m):m, NetDeliveryMethod.Unreliable);
        }

        public virtual void Send(ISerializable obj)
        {
            SerializationContext c = new SerializationContext(Root.Instance.Factory,client.CreateMessage());
            Root.Instance.Factory.Serialize(c, obj);
            byte[] buf = c.ToArray();
            Send(c.GetMessage());
        }


        public NetIncomingMessage Receive(out IPEndPoint sender)
        {
            NetConnection c;

            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(msg.ReadString());
                        break;
                    case NetIncomingMessageType.Data:
                    //case NetIncomingMessageType.DiscoveryResponse:
                        sender = msg.SenderEndpoint;
                        return compress?NetCompressor.Decompress(msg):msg;
                    default:
                        Console.WriteLine("Unhandled type: " + msg.MessageType);
                        break;
                }
                client.Recycle(msg);
            }


            sender = null;
            return null;
        }

        protected NetOutgoingMessage CreateApprovalRequest(string password)
        {
            NetOutgoingMessage msg = client.CreateMessage();

            msg.Write(Root.Instance.Version);
            msg.Write(Root.Instance.Mod.Version);
            msg.Write(password);

            return msg;
        }

        public virtual void Connect(string host, int port, string name, string password)
        {
            client.Connect(host, port, CreateApprovalRequest(password));

            //read client number
            int i = 0;
            short n=-1;
            byte[] classes = null;

            while(i++<100)
            {
                Thread.Sleep(100);

                NetIncomingMessage m;
                while ((m=client.ReadMessage())!=null)
                {
                    switch (m.MessageType)
                    {
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Console.WriteLine(m.ReadString());
                            break;
                        case NetIncomingMessageType.Data:
                            {
                                if (m.LengthBytes == 2)
                                {
                                    n = m.ReadInt16();
                                    System.Console.WriteLine(n);
                                    break;
                                }
                                else if (m.PeekUInt32() == 0xFFFFFFFF)
                                {
                                    m.ReadUInt32();
                                    classes = m.ReadBytes(m.LengthBytes - 4);
                                    System.Console.WriteLine("received class dictionary: " + classes.Length);
                                    break;
                                }
                                else if (m.PeekUInt32() == 0xFFFFFFFE)
                                {
                                    m.ReadUInt32();
                                    compress = m.ReadBoolean();
                                    Console.WriteLine("compression: " + compress);
                                }

                                Console.WriteLine("skip: " + m.LengthBytes);
                            }
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)m.ReadByte();
                            Console.WriteLine("status: " + status.ToString());
                            switch (status)
                            {
                                case NetConnectionStatus.Connecting:
                                    break;
                                case NetConnectionStatus.Connected:
                                    break;
                            }
                            break;
                        default:
                            Console.WriteLine("Unhandled type: " + m.MessageType);
                            break;
                    }
                    client.Recycle(m);

                    /*else if (m.SequenceChannel == NetChannel.ReliableUnordered)
                    {
                        classes = m.ReadBytes(m.Length);
                        System.Console.WriteLine("received class dictionary: " + classes.Length);
                    }*/
                }
                if (classes != null && n != -1)
                    break;
            }
            if (n == -1)
                throw new Exception();
            if (n > 16)
                throw new Exception();

            Root.Instance.Factory = new Factory(new MemoryStream(classes));

            Console.WriteLine("connected to server: clientnumber: " + n);
            ClientNumber = n;
        }

        public virtual void Disconnect()
        {
            client.Disconnect("");
            client = null;
            ClientNumber = -1;
        }

        public static ConnectionStatistics Convert(NetPeerStatistics n,float rtt)
        {
            if (n != null)
            {
                ConnectionStatistics c = new ConnectionStatistics(
                    n.ReceivedPackets, n.ReceivedBytes,
                    n.SentPackets, n.SentBytes,
                    n.ReceivedBytes, n.SentBytes);
                c.RoundTripTime = rtt;
                return c;
            }
            else
            {
                ConnectionStatistics c = new ConnectionStatistics();
                c.RoundTripTime = rtt;
                return c;
            }
        }

        public ConnectionStatistics Statistics
        {
            get { return Convert(client.Statistics,client.ServerConnection.AverageRoundtripTime); }
        }

        public NetClient client;
        protected IPEndPoint EP;
        public short ClientNumber;
        public float LastServerTimeStamp;

        protected ConnectionStatistics stats;
    }




    public class UdpServer : IConnection, ITickable
    {

        bool compress;

        public UdpServer(int port, int maxclients, string password)
        {
            compress = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetBool("net.compression");
            Password = password;
            connections = new NetConnection[maxclients];
            NetPeerConfiguration config = new NetPeerConfiguration("spacewar2006-1");
            config.EnableMessageType(NetIncomingMessageType.Data);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.Error);
            config.EnableMessageType(NetIncomingMessageType.Receipt);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            config.Port = port;
            config.MaximumConnections = maxclients;
            server = new NetServer(config);
            server.Start();
            //server.StatusChanged += new EventHandler<NetStatusEventArgs>(server_StatusChanged);
        }

        int FindEmptySlot()
        {
            for (int i = 0; i < connections.Length; ++i)
            {
                if (connections[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }
        int FindSlot(NetConnection c)
        {
            for (int i = 0; i < connections.Length; ++i)
            {
                if (connections[i] == c)
                {
                    return i;
                }
            }
            return -1;
        }

        void OnConnect(NetConnection sender)
        {
                NetOutgoingMessage m = server.CreateMessage(2);
                int n = FindEmptySlot();
                connections[n] = sender;
                m.Write((short)(n+1));
                server.SendMessage(m, sender, NetDeliveryMethod.ReliableOrdered);

                m = server.CreateMessage(5);
                m.Write((uint)0xFFFFFFFE);
                m.Write(compress);
                server.SendMessage(m, sender, NetDeliveryMethod.ReliableOrdered);

                //server.FlushMessages();

                MemoryStream s = new MemoryStream();
                Root.Instance.Factory.SaveClassIds(s);
                byte[] buf = s.ToArray();
                m = server.CreateMessage(4+buf.Length);
                m.Write((uint)0xFFFFFFFF);
                m.Write(buf);
                server.SendMessage(m, sender, NetDeliveryMethod.ReliableOrdered);
                System.Console.WriteLine("sending client number " + (n+1));
        }

        void OnDisconnect(NetConnection sender)
        {
            System.Console.WriteLine("disconnect: " + sender.RemoteEndpoint);
            int n = FindSlot(sender);
            connections[n] = null;
            System.Console.WriteLine("number: " + (n+1));
            Root.Instance.ServerOnDisconnect((short)(n+1), "", sender.RemoteEndpoint);
        }

        public void RemoveAllClients()
        {
        }

        public int NumClients
        {
            get
            {
                return server.ConnectionsCount;
            }
        }
        public NetConnection[] Clients
        {
            get
            {
                return connections;
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

        NetConnection FindConnection(IPEndPoint ep)
        {
            foreach (NetConnection c in server.Connections)
            {
                if (c.RemoteEndpoint.ToString() == ep.ToString())
                    return c;
            }
            return null;
        }

        public short GetClientNumber(IPEndPoint ep)
        {
            NetConnection c = FindConnection(ep);
            if (c == null)
                return -1;

            int n = FindSlot(c);
            if (n == -1)
                return -1;

            return (short)(n+1);
        }

        public bool ClientExists(short id)
        {
            if (id <= -1)
                return false;

            return server.Connections[id - 1] != null;
        }

        public bool ClientExists(IPEndPoint ep)
        {
            return FindSlot(FindConnection(ep)) != -1;
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
        }

        public virtual void Send(ISerializable obj)
        {
            if (server.ConnectionsCount == 0 && Root.Instance.Recorder == null)
                return;


            NetOutgoingMessage m = server.CreateMessage();
            SerializationContext c = new SerializationContext(Root.Instance.Factory,m);
            c.Serialize(obj);
            if (Root.Instance.Recorder != null)
            {
                byte[] buf = c.ToArray();
                Root.Instance.Recorder.WritePacket((int)(Root.Instance.Time * 1000), buf, buf.Length);
            }

            Send(m);
        }

        public virtual void Send(ISerializable obj, IPEndPoint ep)
        {
            NetOutgoingMessage m = server.CreateMessage();
            SerializationContext c = new SerializationContext(Root.Instance.Factory,m);
            Root.Instance.Factory.Serialize(c, obj);
            byte[] buf = c.ToArray();
            Send(c.GetMessage(), ep);
        }
        public virtual void SendNot(ISerializable obj, IPEndPoint ep)
        {
            if (server.ConnectionsCount == 0)
                return;
            NetOutgoingMessage m = server.CreateMessage();
            SerializationContext c = new SerializationContext(Root.Instance.Factory, m);
            Root.Instance.Factory.Serialize(c, obj);
            byte[] buf = c.ToArray();

            SendNot(c.GetMessage(), ep);
        }

        public virtual void Send(NetOutgoingMessage m)
        {
            server.SendMessage(compress?NetCompressor.Compress(m):m, server.Connections, NetDeliveryMethod.Unreliable,0);
        }

        public virtual void SendNot(NetOutgoingMessage m, IPEndPoint ep)
        {
            if (server.ConnectionsCount == 0 && Root.Instance.Recorder == null)
                return;
            if (ep == null)
                throw new Exception("ep==null");

            if (Root.Instance.Recorder != null)
            {
                byte[] buf = m.PeekDataBuffer();
                Root.Instance.Recorder.WritePacket((int)(Root.Instance.Time * 1000), buf, buf.Length);
            }

            List<NetConnection> list = new List<NetConnection>();
            foreach (NetConnection s in Clients)
            {
                if (s != null)
                {
                    if (ep.ToString() != s.RemoteEndpoint.ToString())
                        list.Add(s);
                }
            }
            server.SendMessage(compress?NetCompressor.Compress(m):m, list, NetDeliveryMethod.Unreliable, 0);
        }

        public virtual void Send(NetOutgoingMessage m, IPEndPoint ep)
        {
            NetConnection c = FindConnection(ep);
            if(c!=null && c.Status==NetConnectionStatus.Connected)
                server.SendMessage(compress?NetCompressor.Compress(m):m, c, NetDeliveryMethod.Unreliable);

        }

        public NetIncomingMessage Receive(out IPEndPoint sender)
        {
            NetIncomingMessage m;
            while ((m = server.ReadMessage()) != null)
            {
                switch (m.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(m.ReadString());
                        break;
                    case NetIncomingMessageType.Data:
                        sender = m.SenderEndpoint;
                        return compress?NetCompressor.Decompress(m):m;
                    case NetIncomingMessageType.DiscoveryRequest:
                        {
                            ISerializable info=Root.Instance.CurrentFlow.Query();
                            if (info != null)
                            {
                                NetOutgoingMessage m2 = server.CreateMessage();
                                m2.Write(info.ToString());
                                server.SendDiscoveryResponse(m2, m.SenderEndpoint);
                            }
                            break;
                        }
                    case NetIncomingMessageType.StatusChanged:
                        {
                            NetConnectionStatus newStatus = (NetConnectionStatus)m.ReadByte();
                            string statusMessage = m.ReadString();
                            Console.WriteLine(statusMessage);
                            switch (newStatus)
                            {
                                case NetConnectionStatus.Connected:
                                    OnConnect(m.SenderConnection);
                                    break;
                                case NetConnectionStatus.Disconnected:
                                    OnDisconnect(m.SenderConnection);
                                    break;
                            }
                        }
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        {
                            int root_version = m.ReadInt32();
                            if (root_version != Root.Instance.Version)
                            {
                                Cheetah.Console.WriteLine("Client rejected: Wrong root version.");
                                m.SenderConnection.Deny("Wrong Root Version. Need " + Root.Instance.Version + ", got " + root_version);
                                break;
                            }
                            int mod_version = m.ReadInt32();
                            if (mod_version != Root.Instance.Mod.Version)
                            {
                                Cheetah.Console.WriteLine("Client rejected: Wrong mod version.");
                                m.SenderConnection.Deny("Wrong Mod Version. Need " + Root.Instance.Mod.Version + ", got " + mod_version);
                                break;
                            }
                            string password = m.ReadString();
                            if (password != this.Password && !string.IsNullOrEmpty(Password))
                            {
                                Cheetah.Console.WriteLine("Client rejected: Wrong password.");
                                m.SenderConnection.Deny("Wrong Password.");
                                break;
                            }

                            m.SenderConnection.Approve();
                        }
                        break;
                    default:
                        Console.WriteLine("Unhandled type: " + m.MessageType);
                        break;
                }
                server.Recycle(m);
            }
            sender = null;
            return null;
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
                return server.ConnectionsCount;
            }
        }

        public NetServer server;
        NetConnection[] connections;
        public string Password;
        protected ConnectionStatistics stats;
        float UpdateRTT;
        public int Port;
    }





}
