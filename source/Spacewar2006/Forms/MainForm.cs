using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Collections;
using System.IO;

using Cheetah;
using Cheetah.Graphics;

using SpaceWar2006.Rules;
using SpaceWar2006.GameObjects;

namespace Spacewar2006.Forms
{
    public partial class MainForm : Form
    {
        ServerFinder InternetScanner;
        
        public MainForm()
        {
            Application.EnableVisualStyles();
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text = Root.Instance.Mod.GameString;

            Console.Lines = Cheetah.Console.History.ToArray();

            loading = true;

            LoadMaps(HostMap);
            LoadRules(HostRule);

            //AboutWeb.Url = new Uri(Root.Instance.FileSystem.Get("web/about.html").info.FullName);
            //AboutWeb.AllowNavigation = false;

            LoadConfig();

            HostWebAdmin_CheckedChanged(null,null);
            HostQuery_CheckedChanged(null, null);
            HostIrcReporter_CheckedChanged(null, null);
            DemoRecord_CheckedChanged(null, null);
            HostSinglePlayer_CheckedChanged(null, null);
            LanButton_CheckedChanged(null, null);

            loading = false;
            LoadDemos();

            LoadModels();

            AboutRtf.LoadFile("misc/about.rtf");
            HelpRtf.LoadFile("misc/help.rtf");

            /*
            WebClient client = new WebClient();
            try
            {
                Stream strm = client.OpenRead("http://fch.selfkill.com/cody/spacewar-news.rtf");
                MemoryStream ms = new MemoryStream();
                StreamReader sr = new StreamReader(strm);
                string data=sr.ReadToEnd();
                strm.Close();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write(data);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                NewsRtf.LoadFile(ms, RichTextBoxStreamType.RichText);
            }
            catch (Exception e2)
            {
                NewsRtf.LoadFile("misc/news-error.rtf");
            }*/
        }

        bool loading = false;
        void LoadConfig()
        {
            loading = true;
            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");
            string tmp;

            Resolution.Text = c.GetString("video.width") + "x" + c.GetString("video.height");
            WindowMode.Checked = !c.GetBool("video.fullscreen");

            tmp = c.GetString("gameserver.map");
            if (tmp.Length > 0)
            {
                try
                {
                    HostMap.SelectedItem = Root.Instance.Factory.GetType(tmp);
                }
                catch (Factory.CantFindTypeException)
                {
                    HostMap.SelectedItem = tmp;
                }
            }
            tmp = c.GetString("gameserver.rule");
            if (tmp.Length > 0)
                HostRule.SelectedItem = Root.Instance.Factory.GetType(tmp);

            HostBots.Value = c.GetInteger("server.bots");

            PlayerName.Text = c.GetString("player.name");

            WebAdminName.Text = c.GetString("web.name");
            WebAdminPassword.Text = c.GetString("web.password");
            HostWebAdmin.Checked = c.GetBool("web.enable");

            HostQueryPort.Value = c.GetInteger("server.queryport");
            HostQuery.Checked = c.GetBool("server.queryport.enable");

            HostIrcReporter.Checked = c.GetBool("irc.enable");
            HostIrcChannel.Text = c.GetString("irc.channels");
            HostIrcNick.Text = c.GetString("irc.nick");
            HostIrcPort.Value = c.GetInteger("irc.port");
            HostIrcServer.Text = c.GetString("irc.host");

            //Sound.SelectedText = c.GetBool("audio.enable") ? "Fmod SoundSystem" : "Off";
            Sound.Checked = c.GetBool("audio.enable");

            bool bloom = true;
            if (c.Table.ContainsKey("postprocess.bloom"))
            {
                bloom = c.GetBool("postprocess.bloom");
            }
            Bloom.Checked = bloom;

            loading = false;
        }

        void LoadModels()
        {
            ModelList.Items.Clear();

            FileSystemNode models = Root.Instance.FileSystem.Get("models");

            foreach (DictionaryEntry de in models)
            {
                string name = (string)de.Key;
                FileSystemNode n = (FileSystemNode)de.Value;
                foreach (DictionaryEntry de2 in n)
                {
                    string mesh = (string)de2.Key;
                    FileSystemNode n2 = (FileSystemNode)de2.Value;
                    if (mesh.EndsWith(".mesh"))
                    {
                        ListViewItem lvi = new ListViewItem(new string[] { name, mesh});
                        lvi.Tag = n2;
                        ModelList.Items.Add(lvi);
                    }
                }
            }

            if (Root.Instance.FileSystem.ContainsKey("units"))
            {
                FileSystemNode units = Root.Instance.FileSystem.Get("units");
                foreach (DictionaryEntry de in units)
                {
                    string name = (string)de.Key;
                    FileSystemNode n = (FileSystemNode)de.Value;
                    ListViewItem lvi = new ListViewItem(new string[] { "units", name });
                    lvi.Tag = n;
                    ModelList.Items.Add(lvi);
                }
            }

        }

        void SaveConfig()
        {
            if (loading)
                return;

            Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");

            string[] res=Resolution.Text.Split('x');

            c.Set("video.width", res[0]);
            c.Set("video.height", res[1]);
            c.Set("video.fullscreen", WindowMode.Checked ? "false" : "true");

            //c.Set("gameserver.map", ((Type)HostMap.SelectedItem).FullName);
            c.Set("gameserver.map", HostMap.SelectedItem.ToString());
            c.Set("gameserver.rule", ((Type)HostRule.SelectedItem).FullName);
            c.Set("server.bots", ((int)HostBots.Value).ToString());

            c.Set("player.name", PlayerName.Text);

            c.Set("web.name", WebAdminName.Text);
            c.Set("web.password", WebAdminPassword.Text);
            c.Set("web.enable", HostWebAdmin.Checked?"true":"false");

            c.Set("server.queryport", ((int)HostQueryPort.Value).ToString());
            c.Set("server.queryport.enable", HostQuery.Checked ? "true" : "false");

            c.Set("irc.enable", HostIrcReporter.Checked ? "true" : "false");
            c.Set("irc.channels", HostIrcChannel.Text);
            c.Set("irc.nick", HostIrcNick.Text);
            c.Set("irc.port", ((int)HostIrcPort.Value).ToString());
            c.Set("irc.host", HostIrcServer.Text);

            c.Set("audio.enable", Sound.Checked);
            c.Set("postprocess.bloom", Bloom.Checked);
        }

        void LoadDemos()
        {
            DemoList.Items.Clear();

            FileSystemNode demos = Root.Instance.FileSystem.Get("demos");

            foreach (DictionaryEntry de in demos)
            {
                string name = (string)de.Key;
                if (name.EndsWith(".demo"))
                {
                    FileSystemNode n = (FileSystemNode)de.Value;
                    DemoPlayer demo = new DemoPlayer(n);
                    ListViewItem lvi = new ListViewItem(new string[] { name, demo.Length.ToString(), n.info.Length.ToString(), demo.FrameCount.ToString() });
                    lvi.Tag = n;
                    DemoList.Items.Add(lvi);
                }
            }
        }

        void LoadMaps(ComboBox cb)
        {
            Type[] maps = Root.Instance.Factory.FindTypes(null, typeof(SpaceWar2006.GameObjects.Map), false);
            cb.Items.AddRange(maps);

            //xml maps
            foreach (DictionaryEntry de in Root.Instance.FileSystem.Get("maps"))
            {
                if (((string)de.Key).ToLower().EndsWith(".xml"))
                {
                    cb.Items.Add(((FileSystemNode)de.Value).GetFullPath());
                }
            }

            if(cb.Items.Count>0)
                cb.SelectedIndex = 0;
        }

        void LoadRules(ComboBox cb)
        {
            Type[] rules = Root.Instance.Factory.FindTypes(null, typeof(SpaceWar2006.Rules.GameRule), false);
            cb.Items.AddRange(rules);
            if (cb.Items.Count > 0)
                cb.SelectedIndex = 0;
        }

        void ConsoleOutput(string text)
        {
            Console.Text += text + "\r\n";
        }

        ListViewItem FindServer(IPEndPoint ep)
        {
            foreach (ListViewItem i in ServerList.Items)
            {
                if (i.Tag.ToString() == ep.ToString())
                    return i;
            }
            return null;
        }
        protected void OnServerAnswer(Lidgren.Network.NetIncomingMessage msg)
        {
            //SpaceWar2006.GameSystem.GameServerInfo info = p as SpaceWar2006.GameSystem.GameServerInfo;
            SpaceWar2006.GameSystem.GameServerInfo info;
            //if (info == null)
            {
                string p = msg.ReadString();
                info = new SpaceWar2006.GameSystem.GameServerInfo(p);
            }
            ListViewItem lvi=FindServer(msg.SenderEndpoint);
            bool add = false;

            string[] subitems = new string[6];
            if (lvi == null)
            {
                lvi = new ListViewItem();
                lvi.Tag = msg.SenderEndpoint.ToString();
                add = true;
            }


            subitems[1] = msg.SenderEndpoint.ToString();

            if (info != null)
            {
                subitems[0] = info.ServerName;
                subitems[2] = info.Map;
                subitems[3] = info.GameType;
                subitems[4] = info.NumPlayers.ToString() + "/" + info.MaxPlayers.ToString();
                subitems[5] = info.Password.ToString();
            }
            else
            {
                subitems[0] = "???";
            }

            string tmp="";
            if (Root.Instance.IsWindows)
            {
                tmp = subitems[0];
                for (int i = 0; i < subitems.Length - 1; ++i)
                    subitems[i] = subitems[i + 1];
            }

            lvi.SubItems.Clear();
            lvi.SubItems.AddRange(subitems);
            if (Root.Instance.IsWindows)
                lvi.Text = tmp;

            if(add)
                ServerList.Items.Add(lvi);
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            GameRunning = true;
            string[] args = new string[] { };

            Root r = Root.Instance;
            r.Authoritive = false;
            r.ClientClient(args);
            IUserInterface ui = r.UserInterface;

            //Cheetah.Root.Instance.LocalObjects.Add(new Helper());

            Cheetah.Console.ConsoleEvent += ConsoleOutput;


            Flow f = new SpaceWar2006.Flows.ClientStart();

            r.CurrentFlow = f;



            f.Start();

            r.ClientLoop();

            //Cheetah.Root.Instance.LocalObjects.Clear();

            r.Gui.windows.Clear();
            r.Gui = null;
            GC.Collect();
            r.ResourceManager.UnloadAll();
            r.UserInterface.Dispose();
            r.UserInterface = null;

            Cheetah.Console.ConsoleEvent -= ConsoleOutput;
            GameRunning = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //HACK
            if (InternetScanner != null && !GameRunning && tabControl1.SelectedTab.Text == "Join")
            {
                InternetScanner.Tick(0.5f);
            }
        }

        bool GameRunning = false;

        private void JoinButton_Click(object sender, EventArgs e)
        {
            if (IpAddress.Text.Length == 0)
            {
                MessageBox.Show(this, "Select a server first.", "<°)))><", MessageBoxButtons.OK);
                return;
            }

            if (GameRunning)
            {
                MessageBox.Show(this, "Stop the current game first.", "<°)))><", MessageBoxButtons.OK);
                return;
            }
            
            GameRunning = true;

            string address = IpAddress.Text;
            string[] args;
            if(string.IsNullOrEmpty(JoinPassword.Text))
                args = new string[] { "-connect", address};
            else
                args = new string[] { "-connect", address, "-password", JoinPassword.Text };

            Root r = Root.Instance;
            r.Args = args;

            r.Authoritive = false;
            r.ClientClient(args);
            IUserInterface ui = r.UserInterface;

            Flow f = new SpaceWar2006.Flows.ClientStart();

            r.CurrentFlow = f;

            f.Start();

            r.ClientLoop();

            r.ResourceManager.UnloadAll();
            r.UserInterface.Dispose();
            r.UserInterface = null;

            GameRunning = false;
        }

        Map CreateMap()
        {
            if (HostMap.SelectedItem is Type)
            {
                return (Map)Root.Instance.Factory.CreateInstance((Type)HostMap.SelectedItem);
            }
            else if (HostMap.SelectedItem is string)
            {
                return new XmlMap((string)HostMap.SelectedItem);

            }

            throw new Exception("unknown map selected.");
        }

        private void SingleStart()
        {
            if (GameRunning)
            {
                MessageBox.Show(this, "Stop the current game first.", "<°)))><", MessageBoxButtons.AbortRetryIgnore);
                return;
            }

            GameRunning = true;

            Root r = Root.Instance;

            r.Authoritive = true;
            r.ClientClient(new string[] {});


            Flow f = new SpaceWar2006.Flows.Game(
                ((IRuleCreator)CurrentGameSettings).CreateRule(),
                CreateMap(),
                (int)HostBots.Value,
                false
                );

            r.CurrentFlow = f;



            f.Start();

            r.ClientLoop();


            r.ResourceManager.UnloadAll();
            r.UserInterface.Dispose();
            r.UserInterface = null;
            r.ClientPostProcessor = null;

            GameRunning = false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void addServerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ServerListContextMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        private void DemoPlay_Click(object sender, EventArgs e)
        {
            if (GameRunning)
            {
                MessageBox.Show(this, "Stop the current game first.", "<°)))><", MessageBoxButtons.AbortRetryIgnore);
                return;
            }

            string name = "demos/" + DemoList.SelectedItems[0].SubItems[0].Text;
            
            GameRunning = true;

            Root r = Root.Instance;

            r.Authoritive = true;
            r.ClientClient(new string[] { });

            SpaceWar2006.Flows.Game g = new SpaceWar2006.Flows.Game(null, name, true, false);
            Root.Instance.CurrentFlow = g;

            if (DemoRecord.Checked)
            {
                r.LockTimeDelta = 1.0f / (float)DemoRecordFps.Value;
                r.ClientRecordVideo = 1;
            }

            g.Start();
            r.ClientLoop();

            r.LockTimeDelta = -1;
            r.ClientRecordVideo = -1;

            r.ResourceManager.UnloadAll();
            r.UserInterface.Dispose();
            r.UserInterface = null;

            GameRunning = false;
        }

        private void ServerStart_Click(object sender, EventArgs e)
        {
            if (HostSinglePlayer.Checked)
            {
                SingleStart();
                return;
            }

            if (GameRunning)
            {
                Root.Instance.Quit = true;
                return;
            }

            //start non-dedicated or dedicated server

            GameRunning = true;

            ServerStart.Text = "Stop";

            Helper h=null;

            if(HostDedicated.Checked)
            {
                //SimpleUserInterface sui = new SimpleUserInterface();

                Cheetah.Root.Instance.LocalObjects.Add(h = new Helper());
            }

            Root r = Root.Instance;

            if(HostNonDedicated.Checked)
            {
                //bring up the userinterface
                //ViewerForm vf = new ViewerForm();
                //vf.Show();
                //r.ClientClient(new string[] { }, vf.glControl1);
                r.ClientClient(new string[]{});
            }

            r.ServerServer(new string[] { });
            r.Authoritive = true;
            r.Scene = new Scene();
            if (HostRecordDemo.Checked)
            {
                string name = string.Format("{2}{3,02}{4,2}{5,2}{6,2}-{0}-{1}.demo",
                    ((Type)HostRule.SelectedItem).Name,
                    ((Type)HostMap.SelectedItem).Name,
                    DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                name=name.Replace(' ','0');
                FileSystemNode demo=Root.Instance.FileSystem.Get("demos");
                demo=demo.CreateFile(name);
                Root.Instance.Recorder = new DemoRecorder(demo.getStream(), true, 20);
            }

            Flow f;

            if (CurrentGameSettings is IRuleCreator)
            {
                if (HostDedicated.Checked)
                {
                    f = new SpaceWar2006.Flows.GameServer(
                        ((IRuleCreator)CurrentGameSettings).CreateRule(),
                        CreateMap(),
                        (int)HostBots.Value
                        );
                }
                else
                {
                    f = new SpaceWar2006.Flows.Game(
                      ((IRuleCreator)CurrentGameSettings).CreateRule(),
                      CreateMap(),
                      (int)HostBots.Value,
                      false
                      );
                }
            }
            else
            {
                if (HostNonDedicated.Checked)
                    throw new Exception();

                f = new SpaceWar2006.Flows.GameServer();
            }

            Root.Instance.ResourceManager.LoadConfig("config/global.config").Set("server.password", this.HostPassword.Text);

            r.CurrentFlow = f;
            f.Start();
            r.ServerRun(HostDedicated.Checked);
            f.Stop();
            r.ServerStop();

            r.ResourceManager.UnloadAll();

            if (r.UserInterface != null)
            {
                r.Dispose();
                r.UserInterface = null;
            }

            if(h!=null)
                Cheetah.Root.Instance.LocalObjects.Remove(h);

            Root.Instance.Recorder = null;
            GameRunning = false;

            if (HostRecordDemo.Checked)
            {
                LoadDemos();
            }

            ServerStart.Text = "Start";
            //StatusBar.Text = "Dedicated Server stopped. Ready.";
        }

        private void ServerList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            JoinButton_Click(null, null);
        }

        private void HostWebAdmin_CheckedChanged(object sender, EventArgs e)
        {
            WebAdminPassword.Enabled = WebAdminName.Enabled = HostWebAdminPort.Enabled = HostWebAdmin.Checked;
            SaveConfig();
        }

        private void HostQuery_CheckedChanged(object sender, EventArgs e)
        {
            HostQueryPort.Enabled = HostQuery.Checked;
            SaveConfig();
        }

        private void HostIrcReporter_CheckedChanged(object sender, EventArgs e)
        {
            HostIrcServer.Enabled = HostIrcNick.Enabled = HostIrcChannel.Enabled = HostIrcPort.Enabled = HostIrcChannel.Enabled = HostIrcReporter.Checked;
            SaveConfig();
        }

        private void Resolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void Resolution_TextChanged(object sender, EventArgs e)
        {
            string[] res = Resolution.Text.Split('x');
            if (res.Length == 2 && res[0].Length>0 && res[1].Length>0)
            {
                SaveConfig();
            }
        }

        private void WindowMode_CheckedChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void HostMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        UserControl CurrentGameSettings;
        private void HostRule_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveConfig();
            if (CurrentGameSettings != null)
            {
                GameSettingsGroup.Controls.Remove(CurrentGameSettings);
            }
            if (HostRule.SelectedItem == typeof(DeathMatch))
                CurrentGameSettings = new DeathMatchForm();
            else if (HostRule.SelectedItem == typeof(CaptureTheFlag))
                CurrentGameSettings = new CaptureTheFlagForm();
            else if (HostRule.SelectedItem == typeof(TeamDeathMatch))
                CurrentGameSettings = new TeamDeathMatchForm();
            else if (HostRule.SelectedItem == typeof(Race))
                CurrentGameSettings = new RaceForm();
            else if (HostRule.SelectedItem == typeof(Domination))
                CurrentGameSettings = new DominationForm();
            else if (HostRule.SelectedItem == typeof(KingOfTheHill))
                CurrentGameSettings = new KingOfTheHillForm();
            else
                return;
            //CurrentGameSettings.Location = GameSettingsGroup.Location;
            GameSettingsGroup.Controls.Add(CurrentGameSettings);
            //CurrentGameSettings.Show();
            //CurrentGameSettings.BringToFront();
        }

        private void HostBots_ValueChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void PlayerName_TextChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void WebAdminName_TextChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void WebAdminPassword_TextChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void HostWebAdminPort_ValueChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        void ShowDemoDetails(FileSystemNode demo)
        {
            DemoFile.Text = demo.info.FullName;
        }

        private void DemoList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //DemoPlayer demo = DemoList.SelectedItems[0].Tag;
            if (DemoList.SelectedItems.Count > 0)
            {
                FileSystemNode n = (FileSystemNode)DemoList.SelectedItems[0].Tag;
                ShowDemoDetails(n);
            }
        }

        private void DemoRecord_CheckedChanged(object sender, EventArgs e)
        {
            DemoRecordFps.Enabled = DemoRecord.Checked;
        }

        private void HostNonDedicated_CheckedChanged(object sender, EventArgs e)
        {
            NetworkGroup.Enabled = !HostSinglePlayer.Checked;
        }

        private void HostDedicated_CheckedChanged(object sender, EventArgs e)
        {
            NetworkGroup.Enabled = !HostSinglePlayer.Checked;

        }

        private void HostSinglePlayer_CheckedChanged(object sender, EventArgs e)
        {
            NetworkGroup.Enabled = !HostSinglePlayer.Checked;

        }

        private void ModelViewerStart_Click(object sender, EventArgs e)
        {
            if (GameRunning)
            {
                if (Root.Instance.CurrentFlow is SpaceWar2006.Flows.Viewer)
                {
                    FileSystemNode n1 = (FileSystemNode)ModelList.SelectedItems[0].Tag;
                    ((SpaceWar2006.Flows.Viewer)Root.Instance.CurrentFlow).ChangeMesh(n1.GetFullPath());
                }
                else
                {
                    MessageBox.Show(this, "Stop the current game first.", "<°)))><", MessageBoxButtons.AbortRetryIgnore);
                }
                return;
            }

            GameRunning = true;

            Root r = Root.Instance;

            r.Authoritive = true;

            ViewerForm vf = new ViewerForm();
            vf.Show();
            r.ClientClient(new string[] { }, vf.glControl1);

            //r.ClientClient(new string[] { });
            IUserInterface ui = r.UserInterface;

            FileSystemNode n = (FileSystemNode)ModelList.SelectedItems[0].Tag;

            SpaceWar2006.Flows.Viewer v = new SpaceWar2006.Flows.Viewer(n.GetFullPath());
            Root.Instance.CurrentFlow = v;
            v.Start();
            r.CurrentFlow = v;



            v.Start();

            r.ClientLoop();

            //Cheetah.Root.Instance.LocalObjects.Remove(h);

            r.ResourceManager.UnloadAll();
            r.UserInterface.Dispose();
            r.UserInterface = null;
            vf.Close();

            GameRunning = false;
        }

        private void Sound_CheckedChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void LanButton_CheckedChanged(object sender, EventArgs e)
        {
            if (LanButton.Checked)
            {
                ServerList.Items.Clear();
                InternetScanner = new ServerFinder(OnServerAnswer,true,false);
            }
        }

        private void InternetButton_CheckedChanged(object sender, EventArgs e)
        {
            if (InternetButton.Checked)
            {
                ServerList.Items.Clear();
                InternetScanner = new ServerFinder(OnServerAnswer, false, true);
            }
        }

        private void ServerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ServerList.SelectedItems.Count > 0)
                IpAddress.Text = (string)ServerList.SelectedItems[0].Tag;
            else IpAddress.Text = "";
        }

        private void EditorButton_Click(object sender, EventArgs e)
        {

            if (GameRunning)
            {
                    MessageBox.Show(this, "Stop the current game first.", "<°)))><", MessageBoxButtons.AbortRetryIgnore);
                return;
            }

            GameRunning = true;

            Root r = Root.Instance;

            r.Authoritive = true;

            Cheetah.Forms.EditorForm vf = new Cheetah.Forms.EditorForm();
            vf.Show();
            r.ClientClient(new string[] { }, vf.glControl1);

            //r.ClientClient(new string[] { });
            IUserInterface ui = r.UserInterface;

            Editor v = new Editor();
            Root.Instance.CurrentFlow = v;
            v.Start();
            r.CurrentFlow = v;



            v.Start();

            r.ClientLoop();

            //Cheetah.Root.Instance.LocalObjects.Remove(h);

            r.ResourceManager.UnloadAll();
            r.UserInterface.Dispose();
            r.UserInterface = null;
            vf.Close();

            GameRunning = false;
        }

        private void Bloom_CheckedChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

    }
}