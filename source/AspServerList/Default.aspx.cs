using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Cheetah;
using Lidgren.Network;
using System.Threading;

using Npgsql;
using System.Data;

namespace AspServerList
{
    public partial class _Default : System.Web.UI.Page
    {
        /*List<SpaceWar2006.GameSystem.GameServerInfo> infos = new List<SpaceWar2006.GameSystem.GameServerInfo>();

        protected void Answer(NetIncomingMessage msg)
        {
            SpaceWar2006.GameSystem.GameServerInfo info;
            {
                string p = msg.ReadString();
                info = new SpaceWar2006.GameSystem.GameServerInfo(p);
                infos.Add(info);
            }
        }*/

        protected void Page_Load(object sender, EventArgs e)
        {
            /*ServerFinder finder = new ServerFinder(new ServerFinder.AnswerDelegate(Answer), false, true);

            GridView1.AutoGenerateColumns = true;

            for (int i = 0; i < 5; ++i)
            {
                finder.Tick(0.5f);
                Thread.Sleep(500);
            }

            GridView1.DataSource = infos;
            GridView1.DataBind();*/
			
			NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder ();
			builder.Host = "localhost";
			builder.Password = "";
			builder.Pooling = false;
			builder.UserName = "postgres";
			builder.Database = "game";
			
			NpgsqlConnection c=new NpgsqlConnection(builder.ToString());
			c.Open();
			
			NpgsqlCommand cmd = c.CreateCommand ();
			cmd.CommandText = @"SELECT s2.id,s2.name,s2.host,s2.port,i2.time,i2.map,i2.numplayers,i2.maxplayers FROM
(SELECT s.id AS id,MAX(i.time) AS maxtime FROM gameserver s 
LEFT JOIN gameinfo i ON s.id=i.gameserver_id
GROUP BY s.id) AS times
LEFT JOIN gameserver s2 ON times.id=s2.id
LEFT JOIN gameinfo i2 ON times.id=i2.gameserver_id AND times.maxtime=i2.time";
			//NpgsqlDataReader r = cmd.ExecuteReader();
			NpgsqlDataAdapter a=new NpgsqlDataAdapter(cmd);
			DataTable dt=new DataTable();
			a.Fill(dt);
			GridView1.DataSource=dt;
			GridView1.DataBind();
        }
    }
}
