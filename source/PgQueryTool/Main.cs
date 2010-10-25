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
		static List<SpaceWar2006.GameSystem.GameServerInfo> infos = new List<SpaceWar2006.GameSystem.GameServerInfo>();

        static protected void Answer(NetIncomingMessage msg)
        {
            SpaceWar2006.GameSystem.GameServerInfo info;
            {
                string p = msg.ReadString();
                info = new SpaceWar2006.GameSystem.GameServerInfo(p);
                infos.Add(info);
            }
        }
		
		public static void Main (string[] args)
		{
			NpgsqlConnectionStringBuilder builder=new NpgsqlConnectionStringBuilder();
			builder.Host="localhost";
			builder.Password="";
			builder.Pooling=false;
			builder.UserName="postgres";
			builder.Database="game";
			string cstring=builder.ToString();
			System.Console.WriteLine(cstring);
			NpgsqlConnection c=new NpgsqlConnection(cstring);
			c.Open();
			
			NpgsqlCommand cmd=c.CreateCommand();
			cmd.CommandText="SELECT * FROM gameserver";
			NpgsqlDataReader r=cmd.ExecuteReader();
			List<ServerFinder.Server> list=new List<ServerFinder.Server>();
			while(r.Read())
			{
				string host=(string)r["host"];
				int port=(int)r["port"];
				ServerFinder.Server s=new ServerFinder.Server(host,port);
				list.Add(s);
			}
			
			ServerFinder finder=new ServerFinder(new ServerFinder.AnswerDelegate(Answer),false,list);
			
			for (int i = 0; i < 5; ++i)
            {
                finder.Tick(0.5f);
                Thread.Sleep(500);
            }
			
			foreach(SpaceWar2006.GameSystem.GameServerInfo info in infos)
			{
				System.Console.WriteLine(info.ToString());
			}
			
		}
	}
}

