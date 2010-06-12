using System;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Rules;

using Cheetah;
using Cheetah.Graphics;

namespace SpaceWar2006.GameSystem
{



    public class IrcReporter : IrcBot
    {
        public IrcReporter(string host, int port, string nick, string realname, string[] channels)
            : base(host, port, nick, realname, channels)
        {
            chans = channels;
        }

        public void Announce(string text)
        {
            foreach (string c in chans)
                Say(c, text);
        }

        public void Say(string[] lines)
        {
            for (int i = 0; i < lines.Length; ++i)
                Announce(lines[i]);
        }
        protected override void OnChannelMessage(string channel, string nick, string message)
        {
            if (message == "!stats")
            {
                Say(CreateStats());
            }
            else if (message.StartsWith("!cmd"))
            {
                Root.Instance.Script.Execute(message.Substring(5));
            }
        }
        public string[] CreateStats()
        {
            Scene s = Root.Instance.Scene;
            GameRule r = s.FindEntityByType<GameRule>();
            IList<Player> p = s.FindEntitiesByType<Player>();
            Map m = s.FindEntityByType<Map>();
            List<string> lines = new List<string>();
            int width = 10;

            lines.Add(r.ToString());
            lines.Add(m.ToString());
            lines.Add("name".PadRight(width) + "|" + "frags".PadRight(width) + "|" + "deaths".PadRight(width) + "|" + "team".PadRight(width) + "|" + "rtt");
            foreach (Player p1 in p)
            {
                lines.Add(p1.Name.PadRight(width) + "|" + p1.Frags.ToString().PadRight(width) + "|" + p1.Deaths.ToString().PadRight(width) + "|" + (p1.Team >= 0 ? Team.ColorNames[p1.Team].PadRight(width) : "none".PadRight(width)) + "|" + p1.RTT.ToString().PadRight(width));
            }
            return lines.ToArray();
        }

        #region ITickable Members

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            Time += dtime;
            while (Time >= 1)
            {
                Time -= 1;
                Seconds++;

                if (Seconds % 60 == 0)
                {
                    Say(CreateStats());
                }
            }


        }

        #endregion

        float Time = 0;
        int Seconds = 0;
        string[] chans;
    }

    public class GameServerInfo : ISerializable
    {
        public GameServerInfo(string p)
        {
            string[] s = p.Split('/');
            ServerName = s[0];
            NumPlayers = int.Parse(s[1]);
            MaxPlayers = int.Parse(s[2]);
            Map = s[3];
            GameType = s[4];

            if(s.Length>5)
                Password = bool.Parse(s[5]);
        }
        public GameServerInfo()
        {
            UpdateInfos();
        }
        public GameServerInfo(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public override string ToString()
        {
            return ServerName + "/" + NumPlayers + "/" + MaxPlayers + "/" + Map.Replace('/','\\') + "/" + GameType + "/" + Password.ToString();
        }
        public void UpdateInfos()
        {
            Root serv = Root.Instance;
            if (serv == null)
                return;

            UdpServer udp = serv.Connection as UdpServer;
            if (udp == null)
                return;
            Map map = Root.Instance.Scene.FindEntityByType<Map>();
            GameRule rule = Root.Instance.Scene.FindEntityByType<GameRule>();


            Config c = serv.ResourceManager.LoadConfig("config/global.config");

            ServerName = serv.ServerName;
            GamePort = udp.Port;
            QueryPort = c.GetInteger("server.queryport");
            WebPort = serv.ServerWeb!=null?serv.ServerWeb.Port:0;
            Password = udp.Password != null;
            MaxPlayers = udp.MaxClients;
            NumPlayers = udp.NumClients;
            AdminName = serv.ServerAdmin;
            AdminMail = serv.ServerAdminMail;
            Map = GetMapName(map);
            Game = "spacewar2006";
            GameType = rule!=null?rule.GetType().Name:"none";
            Version = Root.Instance.Mod.Version;
            TickRate = (int)serv.ServerTickrate;
            UpdateRate = (int)(serv.ServerTickrate / (float)serv.ServerUpdateDivisor);

            Players = Root.Instance.Scene.FindEntitiesByType<Player>();
        }

        string GetMapName(Map m)
        {
            if(m != null)
            {
                return (m is XmlMap)?((XmlMap)m).Path:m.GetType().Name;
            }
            else
                return "none";
        }

        public string ServerName;
        public int GamePort;
        public int QueryPort;
        public int WebPort;
        public bool Password;
        public int MaxPlayers;
        public int NumPlayers;
        public string AdminName;
        public string AdminMail;
        public string Map;
        public string Game;
        public string GameType;
        public int Version;
        public int TickRate;
        public int UpdateRate;

        public IEnumerable<Player> Players;

        #region ISerializable Members

        public void Serialize(SerializationContext context)
        {
            context.Write(ServerName);
            context.Write(GamePort);
            context.Write(QueryPort);
            context.Write(WebPort);
            context.Write(Password);
            context.Write(MaxPlayers);
            context.Write(NumPlayers);
            context.Write(AdminName);
            context.Write(AdminMail);
            context.Write(Map);
            context.Write(Game);
            context.Write(GameType);
            context.Write(Version);
            context.Write(TickRate);
            context.Write(UpdateRate);
        }

        public void DeSerialize(DeSerializationContext context)
        {
            ServerName = context.ReadString();
            GamePort = context.ReadInt32();
            QueryPort = context.ReadInt32();
            WebPort = context.ReadInt32();
            Password = context.ReadBoolean();
            MaxPlayers = context.ReadInt32();
            NumPlayers = context.ReadInt32();
            AdminName = context.ReadString();
            AdminMail = context.ReadString();
            Map = context.ReadString();
            Game = context.ReadString();
            GameType = context.ReadString();
            Version = context.ReadInt32();
            TickRate = context.ReadInt32();
            UpdateRate = context.ReadInt32();
        }

        #endregion
    }

    public class OldGameSpyQuery : IQuery
    {
        int id = 10;
        #region IQuery Members

        public byte[][] Answer(byte[] packet, int length)
        {
            string query = Encoding.ASCII.GetString(packet, 0, length);
            query = query.Trim('\\');
            string[] s = query.Split('\\');
            List<byte[]> answer = new List<byte[]>();
            switch (s[0])
            {
                case "basic":
                    //Console.WriteLine("answer");
                    //answer.Add(Encoding.ASCII.GetBytes(@"\gamename\ut\gamever\440\minnetver\432\location\0\queryid\21.1\final\"));
                    //answer.Add(Encoding.ASCII.GetBytes(@"\hostname\bygames.com.Tactical.Ops.#1.[Favourite.4]\hostport\9000\maptitle\RapidWaters][\mapname\TO-RapidWaters\gametype\s_SWATGame\numplayers\16\maxplayers\16\gamemode\openplaying\gamever\440\minnetver\432\worldlog\false\wantworldlog\true\queryid\17.1\final\"));
                    //answer.Add(Encoding.ASCII.GetBytes(@"\player_0\HyBz\frags_0\0\ping_0\.53\team_0\0\mesh_0\Female.Soldier\skin_0\SGirlSkins.Army\face_0\SGirlSkins.Shyann\ngsecret_0\false\player_1\Gin\frags_1\1\ping_1\.56\team_1\1\mesh_1\Female.Soldier\skin_1\SGirlSkins.fwar\face_1\SGirlSkins.Lilith\ngsecret_1\true\player_2\|EZ|jazz_>=iii=<()\frags_2\0\ping_2\.113\team_2\255\mesh_2\Male.Commando\skin_2\CommandoSkins.goth\face_2\CommandoSkins.Grail\ngsecret_2\true\player_3\Mikkeeee\frags_3\-1\ping_3\.53\team_3\1\mesh_3\Male.Soldier\skin_3\SoldierSkins.sldr\face_3\SoldierSkins.Brock\ngsecret_3\false\queryid\53.1\final\"));
                    //answer.Add(Encoding.ASCII.GetBytes(@"\listenserver\False\password\False\timelimit\20\minplayers\0\changelevels\True\maxteams\2\balanceteams\True\playersbalanceteams\True\friendlyfire\0%\tournament\False\gamestyle\Hardcore\AdminName\Amok\AdminEMail\cstokes@blueyonder.co.uk\queryid\55.1\final\"));
                    //answer.Add(Encoding.ASCII.GetBytes(@"\gamename\spacewar\gamever\440\minnetver\432\location\0\hostname\First SpaceWar Server\hostport\9000\maptitle\Liandri.Central.Core\mapname\DM-Liandri\gametype\DeathMatchPlus\numplayers\3\maxplayers\12\gamemode\openplaying\worldlog\true\wantworldlog\true\listenserver\False\password\False\timelimit\15\fraglimit\40\minplayers\0\changelevels\True\tournament\False\gamestyle\Hardcore\AdminName\Amok\AdminEMail\cody@l33t.de\mutators\No.Redeemer\player_0\_6T3_Stringer\frags_0\28\ping_0\.57\team_0\1\mesh_0\Male.Commando\skin_0\CommandoSkins.goth\face_0\CommandoSkins.Necrotic\ngsecret_0\true\player_1\[HERD]:CyBeR-ShEeP:\frags_1\15\ping_1\.68\team_1\255\mesh_1\Nali.Cow\skin_1\TCowMeshSkins.atomiccow\face_1\\ngsecret_1\false\player_2\Mikkeeee\frags_2\12\ping_2\.48\team_2\1\mesh_2\Male.Soldier\skin_2\SoldierSkins.sldr\face_2\SoldierSkins.Brock\ngsecret_2\false\queryid\" + (id++) + @".1\final\"));
                    answer.Add(Encoding.ASCII.GetBytes(FormatGameServerInfo(new GameServerInfo())));
                    break;
                case "info":
                    break;
                case "rules":
                    break;
                case "status":
                    break;
                case "players":
                    break;
            }
            if (answer.Count == 0)
                return null;

            return answer.ToArray();
        }

        #endregion

        string FormatGameServerInfo(GameServerInfo gsi)
        {
            string s = string.Format(@"\gamename\{0}\gamever\{1}\tickrate\{12}\hostname\{2}\hostport\{3}\maptitle\{4}\mapname\{5}\gametype\{6}\numplayers\{7}\maxplayers\{8}\updaterate\{13}\password\{9}\timelimit\0\fraglimit\0\webport\{14}\AdminName\{10}\AdminEMail\{11}\uptime\{15}\",
                gsi.Game, gsi.Version, gsi.ServerName, gsi.GamePort, gsi.Map, gsi.Map, gsi.GameType, gsi.NumPlayers, gsi.MaxPlayers, gsi.Password, gsi.AdminName, gsi.AdminMail, gsi.TickRate, gsi.UpdateRate, gsi.WebPort, Root.Instance.Time);

            if (gsi.Players != null)
            {
                //for (int i = 0; i < gsi.Players.Length; ++i)
                int i = 0;
                foreach (Player player in gsi.Players)
                {
                    //if (gsi.Players[i] == null)
                    //     continue;
                    string p = string.Format(@"player_{0}\{1}\frags_{0}\{2}\ping_{0}\{3}\deaths_{0}\{4}\", i, player.Name, player.Frags, player.RTT, player.Deaths);
                    s += p;
                    ++i;
                }
            }
            s += @"queryid\" + (id++) + @".1\final\";
            //player_0\_6T3_Stringer\frags_0\28\ping_0\.57\team_0\1\mesh_0\Male.Commando\skin_0\CommandoSkins.goth\face_0\CommandoSkins.Necrotic\ngsecret_0\true\player_1\[HERD]:CyBeR-ShEeP:\frags_1\15\ping_1\.68\team_1\255\mesh_1\Nali.Cow\skin_1\TCowMeshSkins.atomiccow\face_1\\ngsecret_1\false\player_2\Mikkeeee\frags_2\12\ping_2\.48\team_2\1\mesh_2\Male.Soldier\skin_2\SoldierSkins.sldr\face_2\SoldierSkins.Brock\ngsecret_2\false\queryid\" + (id++) + @".1\final\")
            return s;
        }
    }

    public class Mod : Cheetah.Mod
    {
        public void Init()
        {
            if (initialized)
            {
                Cheetah.Console.WriteLine("mod already initialized. HACK");
                return;
            }

            Root.Instance.Mod = this;
            initialized = true;

            System.Console.WriteLine(GameString);

            foreach (DictionaryEntry de in Root.Instance.ResourceManager.SearchFileNode("maps"))
            {
                FileSystemNode n = ((FileSystemNode)de.Value);
                if (n.GetName().EndsWith(".dll"))
                {
                    AssemblyResource ar = Root.Instance.ResourceManager.LoadAssembly(n.GetFullPath());
                    Root.Instance.Factory.Add(ar.Assembly);
                    Root.Instance.Assemblies.Add(ar.Assembly);
                    Root.Instance.Script.Reference(ar.Assembly);
                }
            }

            //Root.Instance.Script.Execute(FileSystem.Get("mods/" + r.Mod + "/scripts/init.boo").getStream());
            //Root.Instance.Script.Execute(Root.Instance.FileSystem.Get("mods/" + Root.Instance.Mod + "/scripts/init.boo").getStream());
            Root.Instance.Script.Execute(Root.Instance.FileSystem.Get("scripts/spacewar2006.boo").getStream());
        }

        public override Version AssemblyVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public override int Version
        {
            get
            {
                return AssemblyVersion.Minor;
            }
        }

        public override string GameString
        {
            get
            {
                return "Spacewar Arena (net: " + Root.Instance.Version + "." + Root.Instance.Mod.Version + ", assembly: " + Root.Instance.AssemblyVersion.ToString() + ";" + Root.Instance.Mod.AssemblyVersion + ")";
            }
        }

        
        const int version = 1;
        bool initialized = false;
        public static Mod Instance = new Mod();
    }

}