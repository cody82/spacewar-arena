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
using SpaceWar2006.Flows;
using SpaceWar2006.Rules;

using Cheetah;

namespace SpaceWar2006.Web
{
    public class PlayerInfoPage : WebPage
    {

        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");
            IList<Player> l = Root.Instance.Scene.FindEntitiesByType<Player>();
            HtmlTable t = new HtmlTable();
            //t.Class = "main";
            t.Rows.Add(new object[] { "Name", "Frags", "Deaths", "RTT" });
            foreach (Player p in l)
            {
                t.Rows.Add(new object[] { p.Name, p.Frags, p.Deaths, p.RTT });
            }
            Text.Write(t.ToString());

            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/playerinfo";
    }

    public class SceneInfoPage : WebPage
    {

        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");

            Text.WriteLine("<tt>" + Root.Instance.Scene.ToString().Replace("\n", "<br>") + "</tt>");

            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/sceneinfo";
    }
    public class ConsolePage : WebPage
    {

        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");
            Text.WriteLine("<form>");

            if (Parameters.ContainsKey("cmd"))
            {
                if (Parameters["cmd"] == "cmd")
                {
                    string cmd = Parameters["commandline"];
                    Text.WriteLine("<br>Command executed: " + cmd + "<br>");
                    Root.Instance.Script.Execute(cmd);
                }
                else if (Parameters["cmd"] == "reset")
                {
                    Text.WriteLine("Restarting Server...");
                    ((GameServer)Root.Instance.CurrentFlow).Restart();
                    Text.WriteLine("done.<br>");
                }
            }

            Text.WriteLine("<textarea cols=120 rows=30>");
            string[] log = Cheetah.Console.History.ToArray();
            for (int i = 0; i < log.Length; ++i)
            {
                Text.WriteLine(log[i]);
                //Text.WriteLine("<br>");

            }
            Text.WriteLine("</textarea><br>");

            Text.WriteLine("<input type=text name=commandline>");
            Text.WriteLine("<input type=submit name=cmd value=cmd>");
            Text.WriteLine("<input type=submit name=cmd value=reset>");


            Text.WriteLine("</form>");
            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/console";
    }


    public class MapPage : WebPage
    {
        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");
            Text.WriteLine("<form>");

            if (Parameters.ContainsKey("submit"))
            {
                Text.WriteLine("New Settings will be applied on server restart.<br>");

                GameServer gs = (GameServer)Root.Instance.CurrentFlow;
                gs.NextMap = (Map)Activator.CreateInstance(Root.Instance.Factory.GetType(Parameters["map"]));

            }

            Text.WriteLine("Maps:<br>");

            //HtmlTable t = new HtmlTable();
            //t.Rows.Add(new object[] { "Map", "<input type=text name=map>" });

            Text.WriteLine("<select name=map>");

            Type[] maps = Root.Instance.Factory.FindTypes(null, typeof(Map));
            foreach (Type t in maps)
            {
                Text.WriteLine("<option value=\"" + t.FullName + "\">" + t.Name + "</option>");
            }
            Text.WriteLine("</select>");

            Text.WriteLine("<input type=submit name=submit value=submit>");

            Text.WriteLine("</form>");
            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/map";
    }

    public class DeathMatchSettingsPage : WebPage
    {
        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");
            Text.WriteLine("<form>");

            if (Parameters.ContainsKey("submit"))
            {
                Text.WriteLine("New Settings will be applied on server restart.<br>");

                GameServer gs = (GameServer)Root.Instance.CurrentFlow;
                gs.NextRule = new DeathMatch(int.Parse(Parameters["fraglimit"]), float.Parse(Parameters["timelimit"]));
            }

            Text.WriteLine("DeathMatch Settings:<br>");

            HtmlTable t = new HtmlTable();
            t.Rows.Add(new object[] { "FragLimit", "<input type=text name=fraglimit>" });
            t.Rows.Add(new object[] { "TimeLimit", "<input type=text name=timelimit>" });

            Text.WriteLine(t.ToString());

            Text.WriteLine("<input type=submit name=submit value=submit>");

            Text.WriteLine("</form>");
            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/deathmatchsettings";
    }


    public class KingOfTheHillSettingsPage : WebPage
    {
        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");
            Text.WriteLine("<form>");

            if (Parameters.ContainsKey("submit"))
            {
                Text.WriteLine("New Settings will be applied on server restart.<br>");
                GameServer gs = (GameServer)Root.Instance.CurrentFlow;
                gs.NextRule = new KingOfTheHill();

            }

            Text.WriteLine("King of the hill Settings:<br>");

            HtmlTable t = new HtmlTable();
            t.Rows.Add(new object[] { "TimeLimit", "<input type=text name=timelimit>" });

            Text.WriteLine(t.ToString());

            Text.WriteLine("<input type=submit name=submit value=submit>");

            Text.WriteLine("</form>");
            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/kingofthehillsettings";
    }

    public class RaceSettingsPage : WebPage
    {
        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");
            Text.WriteLine("<form>");

            if (Parameters.ContainsKey("submit"))
            {
                Text.WriteLine("New Settings will be applied on server restart.<br>");

                GameServer gs = (GameServer)Root.Instance.CurrentFlow;
                gs.NextRule = new Race(int.Parse(Parameters["laps"]));

            }

            Text.WriteLine("TeamDeathMatch Settings:<br>");

            HtmlTable t = new HtmlTable();
            t.Rows.Add(new object[] { "Laps", "<input type=text name=laps>" });
            t.Rows.Add(new object[] { "TimeLimit", "<input type=text name=timelimit>" });

            Text.WriteLine(t.ToString());

            Text.WriteLine("<input type=submit name=submit value=submit>");

            Text.WriteLine("</form>");
            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/racesettings";
    }

    public class TeamDeathMatchSettingsPage : WebPage
    {
        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");
            Text.WriteLine("<form>");

            if (Parameters.ContainsKey("submit"))
            {
                Text.WriteLine("New Settings will be applied on server restart.<br>");

                GameServer gs = (GameServer)Root.Instance.CurrentFlow;
                gs.NextRule = new TeamDeathMatch(new Team[]{new Team(0,"Red"),new Team(1,"Green")},int.Parse(Parameters["teamscorelimit"]), float.Parse(Parameters["timelimit"]));

            }

            Text.WriteLine("TeamDeathMatch Settings:<br>");

            HtmlTable t = new HtmlTable();
            t.Rows.Add(new object[] { "TeamScoreLimit", "<input type=text name=teamscorelimit>" });
            t.Rows.Add(new object[] { "TimeLimit", "<input type=text name=timelimit>" });

            Text.WriteLine(t.ToString());

            Text.WriteLine("<input type=submit name=submit value=submit>");

            Text.WriteLine("</form>");
            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/teamdeathmatchsettings";
    }


    public class ConfigPage : WebPage
    {

        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");

            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");

            HtmlTable t = new HtmlTable();

            foreach (DictionaryEntry kv in c.Table)
            {
                t.Rows.Add(new object[] { kv.Key.ToString(), kv.Value.ToString() });
            }
            Text.Write(t.ToString());

            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/config";
    }
    public class GameInfoPage : WebPage
    {

        public override void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
            Text.WriteLine("<html><head>");
            Text.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">");
            Text.WriteLine("</head><body class=\"main\">");

            GameRule r = Root.Instance.Scene.FindEntityByType<GameRule>();
            if (r is TeamDeathMatch)
            {
                TeamDeathMatch tdm = (TeamDeathMatch)r;
                HtmlTable t = new HtmlTable();
                t.Rows.Add(new object[] { "TeamDeathMatch", "" });
                t.Rows.Add(new object[] { "FragLimit", tdm.FragLimit });
                t.Rows.Add(new object[] { "TimeLimit", tdm.TimeLimit });
                t.Rows.Add(new object[] { "TimeElapsed", tdm.TimeElapsed });

                Text.Write(t.ToString());
            }

            Text.WriteLine("</body></html>");
        }

        public static readonly string Url = "/gameinfo";
    }
}