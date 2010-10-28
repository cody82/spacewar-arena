using System;
using Npgsql;
using System.Collections.Generic;
using Lidgren.Network;
using Cheetah;
using System.Threading;

namespace PgQueryTool
{
	class MainClass
	{
		static Dictionary<string, SpaceWar2006.GameSystem.GameServerInfo> infos = new Dictionary<string, SpaceWar2006.GameSystem.GameServerInfo> ();

		protected static void Answer (NetIncomingMessage msg)
		{
			SpaceWar2006.GameSystem.GameServerInfo info;
			{
				string p = msg.ReadString ();
				info = new SpaceWar2006.GameSystem.GameServerInfo (p);
				infos.Add (msg.SenderEndpoint.Address.ToString () + ":" + msg.SenderEndpoint.Port, info);
			}
		}

		public static void Main (string[] args)
		{
			NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder ();
			builder.Host = "localhost";
			builder.Password = "";
			builder.Pooling = false;
			builder.UserName = "postgres";
			builder.Database = "game";
			
			int i = Array.IndexOf<string> (args, "--password");
			if (i != -1)
			{
				builder.Password = args[i + 1];
			}
			
			i = Array.IndexOf<string> (args, "--host");
			if (i != -1)
			{
				builder.Host = args[i + 1];
			}
			
			i = Array.IndexOf<string> (args, "--database");
			if (i != -1)
			{
				builder.Database = args[i + 1];
			}
			
			i = Array.IndexOf<string> (args, "--username");
			if (i != -1)
			{
				builder.UserName = args[i + 1];
			}
			
			string cstring = builder.ToString ();
			System.Console.WriteLine (cstring);
			NpgsqlConnection c = new NpgsqlConnection (cstring);
			c.Open ();
			
			NpgsqlCommand cmd = c.CreateCommand ();
			cmd.CommandText = "SELECT * FROM gameserver";
			NpgsqlDataReader r = cmd.ExecuteReader ();
			List<ServerFinder.Server> list = new List<ServerFinder.Server> ();
			while (r.Read ())
			{
				string host = (string)r["host"];
				int port = (int)r["port"];
				ServerFinder.Server s = new ServerFinder.Server (host, port);
				list.Add (s);
			}
			
			ServerFinder finder = new ServerFinder (new ServerFinder.AnswerDelegate (Answer), false, list);
			
			while (true)
			{
				for (i = 0; i < 10; ++i)
				{
					finder.Tick (0.1f);
					Thread.Sleep (100);
				}
				
				foreach (KeyValuePair<string, SpaceWar2006.GameSystem.GameServerInfo> info in infos)
				{
					string host = info.Key.Split (':')[0];
					int port = int.Parse (info.Key.Split (':')[1]);
					
					string map = info.Value.Map;
					int numplayers = info.Value.NumPlayers;
					int maxplayers = info.Value.MaxPlayers;
					
					string sql = string.Format (@"INSERT INTO gameinfo(time,map,numplayers,maxplayers,gameserver_id) VALUES (NOW(),'{2}',{3},{4},(SELECT id FROM gameserver WHERE host='{0}' AND port={1}));", host, port, map, numplayers, maxplayers);
					System.Console.WriteLine (sql);
					cmd = c.CreateCommand ();
					cmd.CommandText = sql;
					cmd.ExecuteNonQuery ();
				}
				infos.Clear();
			}
		}
	}
}

