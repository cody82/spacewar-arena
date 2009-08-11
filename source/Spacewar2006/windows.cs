using System;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using SpaceWar2006.Rules;
using SpaceWar2006.GameObjects;
using SpaceWar2006.Flows;
using SpaceWar2006.Weapons;
using SpaceWar2006.Effects;
using SpaceWar2006.Planets;
using Cheetah;

namespace SpaceWar2006.Windows
{

    public class Gallery : Window
    {
        public Gallery()
            : base(new Layout(1, 1))
        {
            Pictures = new ImageListBox(FindPictures());
            Transparent = true;
            //Pictures.SelectionChangedEvent += new ListBox.SelectionChangedDelegate(Pictures_SelectionChangedEvent);
            Add(Pictures, 0, 0);
        }

        ListBoxItem[] FindPictures()
        {
            FileSystemNode n = Root.Instance.FileSystem.Get("textures/porn");
            List<ListBoxItem> demos = new List<ListBoxItem>();
            foreach (DictionaryEntry de in n)
            {
                string name = (string)de.Key;
                if (name.EndsWith(".dds"))
                {
                    demos.Add(new ListBoxItem(null, Root.Instance.ResourceManager.LoadTexture((FileSystemNode)de.Value), ((FileSystemNode)de.Value).GetFullPath()));
                }
            }
            return demos.ToArray();
        }

        ImageListBox Pictures;
    }

    public class DemoControl : Window
    {
        public DemoControl()
            : base(new Layout(6, 1))
        {
            Transparent = true;

            Layout.Widths[0] = Layout.Widths[1] = Layout.Widths[4] = Layout.Widths[5] = 0.5f;

            Forward = new Button(ForwardClick, "+5");
            Add(Forward, 4, 0);

            Backward = new Button(BackwardClick, "-5");
            Add(Backward, 1, 0);

            FastForward = new Button(FastForwardClick, "+30");
            Add(FastForward, 5, 0);

            FastBackward = new Button(FastBackwardClick, "-30");
            Add(FastBackward, 0, 0);

            Time = new Button("--/--");
            Add(Time, 3, 0);

            Progress = new ProgressBar();
            Add(Progress, 2, 0);

            Layout.Update(Size);
        }

        void ForwardClick(Button source,int button, float x, float y)
        {
            Root.Instance.Player.GoTo(Root.Instance.Player.CurrentTime + 5);
        }
        void BackwardClick(Button source,int button, float x, float y)
        {
            Root.Instance.Player.GoTo(Root.Instance.Player.CurrentTime - 5);
        }
        void FastForwardClick(Button source,int button, float x, float y)
        {
            Root.Instance.Player.GoTo(Root.Instance.Player.CurrentTime + 30);
        }
        void FastBackwardClick(Button source,int button, float x, float y)
        {
            Root.Instance.Player.GoTo(Root.Instance.Player.CurrentTime - 30);
        }
        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            Time.Caption = Root.Instance.Player.CurrentTime.ToString("F2") + "/" + Root.Instance.Player.Length.ToString("F2");
            Progress.Value = Root.Instance.Player.CurrentTime / Root.Instance.Player.Length;
        }
        Button Forward;
        Button Backward;
        Button FastForward;
        Button FastBackward;
        ProgressBar Progress;
        Button Time;
    }

    public class DemoMenu : Window
    {
        public DemoMenu()
            : base(new Layout(1, 2))
        {
            DemoList = new ListBox(FindDemos());
            Transparent = true;
            DemoList.SelectionChangedEvent += new ListBox.SelectionChangedDelegate(DemoList_SelectionChangedEvent);
            Add(DemoList, 0, 0);
            //Add(new ScrollButton("Up", new Window[] { DemoList }, new Vector2(0, -64)),1,0);
            //Add(new ScrollButton("Down", new Window[] { DemoList }, new Vector2(0, 64)), 1, 2);
            //Add(new ScrollBar(DemoList), 1, 1);
            Layout.Heights[1] = 0.1f;
            //Layout.Widths[1] = 0.05f;

            PlayButton = new Button(PlayButtonPressed, "Play");
            Add(PlayButton, 0, 1);
            //Add(new Label(, 0, 1);

        }

        void DemoList_SelectionChangedEvent(ListBox lb)
        {
            PlayButton.Caption = "Play " + lb.Selected.Text;
        }
        protected void PlayButtonPressed(Button source,int button, float x, float y)
        {
            Root.Instance.Gui.windows.Remove(this);
            Root.Instance.CurrentFlow.Stop();
            Root.Instance.CurrentFlow = new Game(null, DemoList.Selected.Text, true, false);
            Root.Instance.CurrentFlow.Start();
        }
        ListBoxItem[] FindDemos()
        {
            FileSystemNode n = Root.Instance.FileSystem.Get("demos");
            List<ListBoxItem> demos = new List<ListBoxItem>();
            foreach (DictionaryEntry de in n)
            {
                string name = (string)de.Key;
                if (name.EndsWith(".demo"))
                {
                    demos.Add(new ListBoxItem(null, null, ((FileSystemNode)de.Value).GetFullPath()));
                }
            }
            return demos.ToArray();
        }

        ListBox DemoList;
        Button PlayButton;
    }


    public class Chat : InGameConsole
    {
        public Chat(Player p)
            : base(Root.Instance.UserInterface.Renderer.Size.X - 300, Root.Instance.UserInterface.Renderer.Size.Y - 150, 300, 150)
        {
            player = p;
        }

        public override void Execute(string cmd)
        {
            player.Say(cmd);
        }

        public Player player;
    }

    public class ActorInfoWindow : Window
    {
        public ActorInfoWindow()
            : base(0, 0, 64, 24, new Layout(1, 4))
        {
            HitpointBar = new ProgressBar(new Color4f(0.2f, 0.2f, 0.2f, 0.25f), new Color4f(0, 1, 0, 0.8f), OrientationType.Horizontal);
            Add(HitpointBar, 0, 1);

            ShieldBar = new ProgressBar(new Color4f(0.2f, 0.2f, 0.2f, 0.25f), new Color4f(0, 0, 1, 0.8f), OrientationType.Horizontal);
            Add(ShieldBar, 0, 2);

            EneryBar = new ProgressBar(new Color4f(0.2f, 0.2f, 0.2f, 0.25f), new Color4f(1, 1, 0, 0.8f), OrientationType.Horizontal);
            EneryBar.Value = 1;
            Add(EneryBar, 0, 3);

            Name = new Button("");
            Add(Name, 0, 0);

            Color = new Color4f(0, 0, 0, 0);
            Transparent = true;

            Layout.Update(this.Size);
        }

        public override void Draw(IRenderer r, RectangleF rect)
        {
            base.Draw(r, rect);
        }
        public ProgressBar HitpointBar;
        public ProgressBar ShieldBar;
        public ProgressBar EneryBar;
        public Button Name;
    }

    public class Monitor : Window
    {
        public Monitor()
            : base(0, Root.Instance.UserInterface.Renderer.Size.Y - 150, 240, 150, new Layout(1, 1))
        {
            text = CreateTextBox();
            Add(text, 0, 0);
            //Add(new TextBox("Shield"), 0, 1);
            //Add(new TextBox("Hull"), 0, 2);
            //Add(new TextBox("Speed"), 0, 3);

            Layout.Update(this.Size);
        }

        public override void OnKeyDown(Key key)
        {
            base.OnKeyDown(key);


        }

        public override void OnChildClick(Window w, int button)
        {
            base.OnChildClick(w, button);

        }

        public override void Draw(IRenderer r, RectangleF rect)
        {
            Cheetah.Font f = Root.Instance.Gui.DefaultFont;
            Color4f c = f.color;
            f.color = new Color4f(0, 1, 0, 1);
            base.Draw(r, rect);
            f.color = c;
        }
        TextBox CreateTextBox()
        {
            TextBox tb = new TextBox(true);
            tb.NormalColor = new Color4f(0, 0, 0, 0);
            tb.FocusColor = new Color4f(0, 0, 0, 0);
            tb.Color = new Color4f(0, 0, 0, 0);
            tb.ReadOnly = true;
            return tb;
        }

        public void On(Computer c)
        {
            on = Visible = true;
            comp = c;
            Color = new Color4f(0.0f, 1.0f, 0.0f, 0.2f);
        }

        public void Off()
        {
            on = Visible = false;
            //Color = new Color4f(0.0f, 0.0f, 0.0f, 0.0f);
        }

        public void Toggle()
        {
            if (on)
                Off();
            else
                On(comp);
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (on && comp != null)
            {
                string[] lines = new string[5];
                int i = 0;

                lines[i++] = comp.Target.Name;
                lines[i++] = string.Format("dist {0} m", (int)(comp.Target.AbsolutePosition - comp.Owner.AbsolutePosition).GetMagnitude());
                lines[i++] = string.Format("speed {0} m/s", (int)comp.Target.Speed.GetMagnitude());
                if (comp.Target.Hull != null)
                    lines[i++] = "hull " + (int)comp.Target.Hull.CurrentHitpoints + "/" + (int)comp.Target.Hull.MaxHitpoints + " HP";
                else
                    lines[i++] = "hull n/a";
                if (comp.Target.Shield != null)
                    lines[i++] = "shield " + (int)comp.Target.Shield.CurrentCharge + "/" + (int)comp.Target.Shield.MaxEnergy + " GJ";
                else
                    lines[i++] = "shield n/a";

                text.Lines = lines;
            }
        }

        bool on = true;
        TextBox text;
        Computer comp;
    }

    public class InventoryDisplay : Window
    {
        public InventoryDisplay(Inventory i)
            : base(650, 100, 150, 300, new Layout(1, 1))
        {
            //Transparent = true;
            inventory = i;


            list = new ListBox();
            Add(list, 0, 0);

            Layout.Update(Size);
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            UpdateList();
        }


        public void UpdateList()
        {
            List<ListBoxItem> objects = new List<ListBoxItem>();
            foreach (KeyValuePair<Type, int> kv in inventory.Freight)
            {
                objects.Add(new ListBoxItem(null, null, kv.Value + " " + kv.Key.Name));
            }
            //list.SetContents(objects.ToArray());
            list.SetContents(new ListBoxItem[] { new ListBoxItem("bla", null, "bla") });
        }

        Inventory inventory;
        ListBox list;
    }

    public class ShipMenu : Window
    {
        public ShipMenu(Type ship)
            : base(new Layout(1, 1))
        {
            Transparent = true;
            Type[] tmp1 = Root.Instance.Factory.FindTypes(null, typeof(SpaceShip), false);
            List<ListBoxItem> tmp2 = new List<ListBoxItem>();
            foreach (Type t in tmp1)
            {
                if (t != typeof(SpaceShip))
                {
                    FieldInfo fi = t.GetField("Thumbnail", BindingFlags.Public | BindingFlags.Static);
                    string texname = (string)fi.GetValue(null);
                    Texture tex=Root.Instance.ResourceManager.LoadTexture(texname);
                    tmp2.Add(new ListBoxItem(t, tex, t.Name));
                }
            }

            Add(List = new ImageListBox(tmp2.ToArray()), 0, 0);

            //Add(Info = new TextBox("", true), 1, 0);
            List.SelectionChangedEvent += new ListBox.SelectionChangedDelegate(List_SelectionChangedEvent);
            SetShip(ship);
        }

        void List_SelectionChangedEvent(ListBox lb)
        {
            SetShip((Type)lb.Selected.Object);
        }

        void SetShip(Type t)
        {
            Ship = t;
            //Info.Clear();
            //Info.AppendLine(t.Name);
        }

        public Type Result
        {
            get
            {
                return Ship;
            }
        }
        ListBox List;
        //TextBox Info;
        Type Ship;
    }

    public class MainMenu : Window
    {
        public MainMenu()
            : base(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y, new Layout(6, 4))
        {
            Color = new Color4f(0, 0, 0, 0);

            float resy = Size.y;
            float resx = Size.y;
            float want = 50;
            Layout.Heights[0] = want;
            Layout.Heights[3] = want;
            Layout.Heights[1] = Layout.Heights[2] = (resy - 2 * want) / 2;
            Layout.Widths[0] = Layout.Widths[1] = Layout.Widths[2] = Layout.Widths[4] = Layout.Widths[5] = 75;
            Layout.Widths[3] = resx - 5 * 75;

            Window w = new Window();
            w.Shader = Root.Instance.ResourceManager.LoadShader("window.textured.shader");
            w.texture = Root.Instance.ResourceManager.LoadTexture("main2.avi");
            w.Color = new Color4f(1, 1, 1, 1);
            Layout.GetCell(0, 1).Span.X = 6;
            Layout.GetCell(0, 1).Span.Y = 2;
            Add(w, 0, 1);

            w = new Window();
            w.Shader = Root.Instance.ResourceManager.LoadShader("window.gradiant.shader");
            w.ShaderParams = new ShaderParams();
            w.ShaderParams[w.Shader.GetUniformLocation("Gradiant")] = new float[] { 0, -1, 1, 1 };
            w.Color = new Color4f(1, 1, 1, 1);
            Layout.GetCell(5, 1).Span.X = -6;
            //Layout.GetCell(4, 1).Span.Y = 2;
            Add(w, 5, 1);

            w = new Window();
            w.Shader = Root.Instance.ResourceManager.LoadShader("window.gradiant.shader");
            w.ShaderParams = new ShaderParams();
            w.ShaderParams[w.Shader.GetUniformLocation("Gradiant")] = new float[] { 0, 1, 1, 0 };
            w.Color = new Color4f(1, 1, 1, 1);
            Layout.GetCell(5, 2).Span.X = -6;
            Add(w, 5, 2);


            Button b = new Button(OnJoinButtonPressed, "Multiplayer");
            b.Color = b.NormalColor = new Color4f(0, 1, 0, 1);
            b.FocusColor = new Color4f(0.5f, 1, 0.5f, 1);
            b.Tooltip = OnTooltip;
            b.TooltipText = "Join LAN/Internet game.";
            //Root.Instance.Gui.windows.Add(b);
            Add(b, 0, 3);

            b = new Button(OnReplayButtonPressed, "Replay");
            b.Color = b.NormalColor = new Color4f(0, 0, 1, 1);
            b.FocusColor = new Color4f(0.5f, 0.5f, 1, 1);
            b.Tooltip = OnTooltip;
            b.TooltipText = "Play a demo.";
            Add(b, 1, 3);

            b = new Button(OnSingleButtonPressed, "Singleplayer");
            b.Color = b.NormalColor = new Color4f(0, 1, 1, 1);
            b.FocusColor = new Color4f(0.5f, 1, 1, 1);
            b.Tooltip = OnTooltip;
            b.TooltipText = "Play vs. Bots.";
            Add(b, 2, 3);

            b = new Button(OnConfigButtonPressed, "Config");
            b.Color = b.NormalColor = new Color4f(1, 1, 0, 1);
            b.FocusColor = new Color4f(1, 1, 0.5f, 1);
            b.Tooltip = OnTooltip;
            b.TooltipText = "Configure player settings and controls.";
            Add(b, 4, 3);
            
            //MeshWindow mw = new MeshWindow(Root.Instance.ResourceManager.LoadMesh("quad/quad.mesh"));
            //Add(mw, 4, 3);

            b = new Button(OnExitButtonPressed, "Exit");
            b.Color = b.NormalColor = new Color4f(1, 0, 0, 1);
            b.FocusColor = new Color4f(1, 0.5f, 0.5f, 1);
            b.Tooltip = OnTooltip;
            b.TooltipText = "Quit playing.   <°)))><";
            Add(b, 5, 3);

            TooltipButton = b = new Button(null, "Welcome to SpaceWar 2006[tm]!");
            b.Color = b.NormalColor = b.FocusColor = new Color4f(0, 0, 0, 0);
            Add(b, 3, 3);


            Layout.Update(Size);

        }

        //Channel music;

        void Show(Window w)
        {
            if (CurrentWindow != null)
            {
                CurrentWindow.Close();
                if (w.GetType() == CurrentWindow.GetType())
                {
                    CurrentWindow = null;
                    return;
                }
            }
            CurrentWindow = w;
            Add(w, 0, 1);
            Layout.Update(Size);
        }

        public void OnConfigButtonPressed(Button source,int button, float x, float y)
        {
            //Show(new ConfigMenu());
            Show(new TabWindow(new Type[]{typeof(ConfigMenu),typeof(JoystickConfigMenu)},0));
        }
        public void OnExitButtonPressed(Button source,int button, float x, float y)
        {
            Root.Instance.CurrentFlow.Finished = true;
        }
        public void OnSingleButtonPressed(Button source,int button, float x, float y)
        {
            Show(new StartGameMenu());
        }
        public void OnJoinButtonPressed(Button source,int button, float x, float y)
        {
            Show(new JoinGameMenu());
        }
        public void OnReplayButtonPressed(Button source,int button, float x, float y)
        {
            Show(new DemoMenu());
        }

        public void OnTooltip(string text)
        {
            TooltipButton.Caption = text;
        }

        Button TooltipButton;
        Window CurrentWindow;

    }

    public class ConfigListWindow : Window
    {
        public ConfigListWindow(KeyValuePair<string, string>[] kv)
        {
            Layout = new Layout(2, kv.Length);
            int i = 0;
            foreach (KeyValuePair<string, string> item in kv)
            {
                Add(new TextBox(item.Key, false), 0, i);

                TextBox v = new TextBox(item.Value, false);
                Add(Items[item.Key] = v, 1, i++);
            }

            Layout.Update(Size);
        }
        public ConfigListWindow(string[] keys, string[] values)
        {
            Layout = new Layout(2, keys.Length);
            int i = 0;
            foreach (string key in keys)
            {
                Add(new TextBox(key, false), 0, i);

                TextBox v = new TextBox(values[i], false);
                Add(Items[key] = v, 1, i++);
            }

            Layout.Update(Size);
        }
        public string GetValue(string key)
        {
            return Items[key].GetLine(0);
        }

        Dictionary<string, TextBox> Items = new Dictionary<string, TextBox>();
    }

    public class DeathMatchSettingsWindow : ConfigListWindow, IRuleCreator
    {
        public DeathMatchSettingsWindow()
            : base(new KeyValuePair<string, string>[]
                {new KeyValuePair<string,string>("FragLimit","10"),new KeyValuePair<string,string>("TimeLimit","60")
            })
        {

        }

        public GameRule CreateRule()
        {
            return new DeathMatch(int.Parse(GetValue("FragLimit")), float.Parse(GetValue("TimeLimit")));
        }
    }
    public class KingOfTheHillSettingsWindow : ConfigListWindow, IRuleCreator
    {
        public KingOfTheHillSettingsWindow()
            : base(new KeyValuePair<string, string>[]
                {new KeyValuePair<string,string>("FragLimit","10"),new KeyValuePair<string,string>("TimeLimit","60")
            })
        {

        }
        public GameRule CreateRule()
        {
            return new KingOfTheHill();
        }
    }
    public class TeamDeathMatchSettingsWindow : ConfigListWindow, IRuleCreator
    {
        public TeamDeathMatchSettingsWindow()
            : base(new KeyValuePair<string, string>[]
                {new KeyValuePair<string,string>("TeamScoreLimit","10"),new KeyValuePair<string,string>("TimeLimit","60")
            })
        {

        }

        Team[] GetTeams()
        {
            return new Team[] { new Team(0, "kdjgnhf"), new Team(1, "dgujfgh") };
        }

        public GameRule CreateRule()
        {
            return new TeamDeathMatch(GetTeams(), int.Parse(GetValue("TeamScoreLimit")), float.Parse(GetValue("TimeLimit")));
        }
    }


    public class MissionSettingsWindow : ConfigListWindow, IRuleCreator
    {
        public MissionSettingsWindow()
            : base(new KeyValuePair<string, string>[]
                {new KeyValuePair<string,string>("TeamScoreLimit","10"),new KeyValuePair<string,string>("TimeLimit","60")
            })
        {

        }


        Team[] GetTeams()
        {
            return new Team[] { new Team(0, "kdjgnhf"), new Team(1, "dgujfgh") };
        }

        public GameRule CreateRule()
        {
            return new Mission();
        }
    }

    public class CaptureTheFlagSettingsWindow : ConfigListWindow, IRuleCreator
    {
        public CaptureTheFlagSettingsWindow()
            : base(new KeyValuePair<string, string>[]
                {new KeyValuePair<string,string>("TeamScoreLimit","10"),new KeyValuePair<string,string>("TimeLimit","60")
            })
        {

        }

        CtfTeam[] GetTeams()
        {
            return new CtfTeam[] { new CtfTeam(0, "kdjgnhf"), new CtfTeam(1, "dgujfgh") };
        }

        public GameRule CreateRule()
        {
            return new CaptureTheFlag(GetTeams(), int.Parse(GetValue("TeamScoreLimit")), float.Parse(GetValue("TimeLimit")));
        }
    }

    public class RaceSettingsWindow : ConfigListWindow, IRuleCreator
    {
        public RaceSettingsWindow()
            : base(new KeyValuePair<string, string>[]
                {new KeyValuePair<string,string>("Laps","3")
            })
        {

        }
        public GameRule CreateRule()
        {
            return new Race(int.Parse(GetValue("Laps")));
        }
    }
    public class DominationSettingsWindow : ConfigListWindow, IRuleCreator
    {
        public DominationSettingsWindow()
            : base(new KeyValuePair<string, string>[]
                {new KeyValuePair<string,string>("TeamScoreLimit","10"),new KeyValuePair<string,string>("ScoreInterval","30")
            })
        {

        }
        public GameRule CreateRule()
        {
            return new Domination(int.Parse(GetValue("ScoreInterval")), int.Parse(GetValue("TeamScoreLimit")));
        }
    }
    public class StartGameMenu : Window
    {
        public StartGameMenu()
            : base(new Layout(2, 4))
        {
            Transparent = true;

            Type[] modes = Root.Instance.Factory.FindTypes(null, typeof(GameRule), false);
            ListBoxItem[] items = new ListBoxItem[modes.Length];
            for (int i = 0; i < items.Length; ++i)
            {
                items[i] = new ListBoxItem(modes[i], null, modes[i].Name);
            }
            GameModes = new ListBox(items);
            Add(GameModes, 0, 0);
            GameModes.SelectionChangedEvent += new ListBox.SelectionChangedDelegate(GameModes_SelectionChangedEvent);

            Add(new TextBox("Number of bots:", false), 0, 1);
            Add(BotCount = new TextBox("0", false), 1, 1);

            //AppDomain.CurrentDomain.AppendPrivatePath("bin\\windows");
            //AppDomain.CurrentDomain.AppendPrivatePath("mods\\spacewar2006\bin");

            //Root.Instance.Factory.UpdateTypeIds();

            Type[] maps = Root.Instance.Factory.FindTypes(null, typeof(Map), false);
            items = new ListBoxItem[maps.Length];
            for (int i = 0; i < items.Length; ++i)
            {
                items[i] = new ListBoxItem(maps[i], null, maps[i].Name);
            }

            Add(new TextBox("Map:", false), 0, 2);
            Add(Map = new ListBox(items), 1, 2);


            PlayButton = new Button(PlayButtonPressed, "Play");
            Add(PlayButton, 0, 3);
        }

        void GameModes_SelectionChangedEvent(ListBox lb)
        {
            if (ModeSettings != null)
            {
                ModeSettings.Close();
            }
            Type t = (Type)GameModes.Selected.Object;

            if (t == typeof(DeathMatch))
            {
                ModeSettings = new DeathMatchSettingsWindow();
            }
            else if (t == typeof(KingOfTheHill))
            {
                ModeSettings = new KingOfTheHillSettingsWindow();
            }
            else if (t == typeof(TeamDeathMatch))
            {
                ModeSettings = new TeamDeathMatchSettingsWindow();
            }
            else if (t == typeof(Race))
            {
                ModeSettings = new RaceSettingsWindow();
            }
            else if (t == typeof(Domination))
            {
                ModeSettings = new DominationSettingsWindow();
            }
            else if (t == typeof(CaptureTheFlag))
            {
                ModeSettings = new CaptureTheFlagSettingsWindow();
            }
            else if (t == typeof(Mission))
            {
                ModeSettings = new MissionSettingsWindow();
            }
            else throw new Exception("unknown gamerule.");

            Add(ModeSettings, 1, 0);
            Layout.Update(Size);
        }

        protected void PlayButtonPressed(Button source,int button, float x, float y)
        {
            if (ModeSettings == null)
            {
                PlayButton.Caption = "Play (you must select a gametype!)";
                return;
            }

            Root.Instance.Gui.windows.Remove(this);
            Root.Instance.CurrentFlow.Stop();
            Game g = new Game(null, null, false, true);
            g.Server.NextMap = (Map)Activator.CreateInstance((Type)Map.Selected.Object);
            //g.Server.NextMap = new TestSector();
            g.Server.NextBotCount = int.Parse(BotCount.GetLine(0));
            g.Server.NextRule = ((IRuleCreator)ModeSettings).CreateRule();
            Root.Instance.CurrentFlow = g;
            g.Start();
        }
        Button PlayButton;
        ListBox GameModes;
        Window ModeSettings;
        TextBox BotCount;
        ListBox Map;
    }

    public class JoystickConfigMenu : Window
    {
        public JoystickConfigMenu()
            : base(new Layout(2, 11))
        {
            Transparent = true;

            Add(new Button("Forward:"), 0, 0);
            Add(Forward = new Button(ButtonClicked), 1, 0);
            Add(new Button("Back:"), 0, 1);
            Add(Back = new Button(ButtonClicked), 1, 1);

            Add(new Button("Left:"), 0, 2);
            Add(Left = new Button(ButtonClicked), 1, 2);
            Add(new Button("Right:"), 0, 3);
            Add(Right = new Button(ButtonClicked), 1, 3);

            Add(new Button("StrafeLeft:"), 0, 4);
            Add(StrafeLeft = new Button(ButtonClicked), 1, 4);
            Add(new Button("StrafeRight:"), 0, 5);
            Add(StrafeRight = new Button(ButtonClicked), 1, 5);

            Add(new Button("Fire1:"), 0, 6);
            Add(Fire1 = new Button(ButtonClicked), 1, 6);
            Add(new Button("Fire2:"), 0, 7);
            Add(Fire2 = new Button(ButtonClicked), 1, 7);
            Add(new Button("Select:"), 0, 8);
            Add(Select = new Button(ButtonClicked), 1, 8);
            Add(new Button("Mouse Look:"), 0, 9);
            Add(MouseLook = new CheckBox(ButtonClicked), 1, 9);
            //Name.AppendLine(c.GetString("player.name"));

            Add(new Button(SaveButtonClicked, "Save changes"), 0, 10);
            Layout.GetCell(0, 10).Span.X = 2;

            Layout.Update(Size);
            //Control.Load();
            Update();
        }

        void SaveButtonClicked(Button source, int button, float x, float y)
        {
            Control.Save();
        }

        void ButtonClicked(Button source, int button, float x, float y)
        {
            if (source == MouseLook)
            {
                Control.MouseLook = MouseLook.Checked;
            }
            else
            {
                TimeToStartCheck = 0.5f;
                Selected = source;
            }
        }

        float TimeToStartCheck=-1;

        Button GetOtherButton(Button b)
        {
            if (b == Forward)
                return Back;
            if (b == Back)
                return Forward;

            if (b == Left)
                return Right;
            if (b == Right)
                return Left;

            if (b == StrafeLeft)
                return StrafeRight;
            if (b == StrafeRight)
                return StrafeLeft;

            return null;
        }

        ControlInfo GetControl(Button b)
        {
            if (b == Forward || b == Back)
                return Control.Thrust;
            if (b == Left || b == Right)
                return Control.Rotate;
            if (b == StrafeLeft || b == StrafeRight)
                return Control.Strafe;
            if (b == Fire1)
                return Control.Fire;
            if (b == Fire2)
                return Control.FireSecondary;
            if (b == Select)
                return Control.Select;
            return null;
        }

        bool IsFirstOfAxis(Button b)
        {
            return b == Forward || b == Left || b == StrafeLeft;
        }

        float[] rel = new float[] { Root.Instance.UserInterface.Mouse.GetPosition(0), Root.Instance.UserInterface.Mouse.GetPosition(1) };

        void CheckControls()
        {
            //System.Console.WriteLine(TimeToStartCheck);
            if (TimeToStartCheck == 0.0f && Selected != null)
            {
                bool needupdate = false;
                foreach (ControlID c in new ControlID[] { ControlID.Joystick0, ControlID.Keyboard, ControlID.Mouse })
                {
                    try
                    {
                        for (int i = 0; true; ++i)
                        {
                            float pos = Root.Instance.UserInterface.GetControl(c).GetPosition(i);
                            if (c == ControlID.Mouse)
                            {
                                pos = pos - rel[i];
                            }
                            if (Math.Abs(pos) > (c == ControlID.Mouse?2.0f:0.5f))
                            {
                                //Selected.Caption = "Axis " + i.ToString() + ((Math.Sign(pos)>0)?"+":"-");
                                //GetOtherButton(Selected).Caption = "Axis " + i.ToString() + ((Math.Sign(pos) > 0) ? "-" : "+");
                                System.Console.WriteLine(":" + pos);
                                ControlInfo ci = GetControl(Selected);
                                ci.Id = c;
                                ci.IsAxis = true;
                                ci.AxisButton1 = i;
                                ci.Invert = ((Math.Sign(pos) < 0.0f) && IsFirstOfAxis(Selected)) || ((Math.Sign(pos) > 0.0f) && !IsFirstOfAxis(Selected));
                                needupdate = true;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }

                    if (!needupdate)
                    {
                        try
                        {
                            for (int i = 0; true; ++i)
                            {
                                bool pos = Root.Instance.UserInterface.GetControl(c).GetButtonState(i);
                                if (pos)
                                {
                                    //Selected.Caption = "Button " + i.ToString();
                                    //if(GetOtherButton(Selected)!=null)
                                    //    GetOtherButton(Selected).Caption = "";
                                    ControlInfo ci = GetControl(Selected);
                                    ci.Id = c;
                                    ci.IsAxis = false;
                                    if (IsFirstOfAxis(Selected) || GetOtherButton(Selected) == null)
                                        ci.AxisButton1 = i;
                                    else
                                        ci.Button2 = i;
                                    needupdate = true;
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                    if (needupdate)
                    {
                        Selected = null;
                        TimeToStartCheck = -1;
                        break;
                    }
                }

                Control.MouseLook = MouseLook.Checked;
                if (needupdate)
                {
                    System.Console.WriteLine("update");
                    Update();
                }
            }
            IControl mouse = Root.Instance.UserInterface.Mouse;
            for (int i = 0; i < 2; ++i)
            {
                float pos = mouse.GetPosition(i);
                //float f = pos - rel[i];
                rel[i] = pos;
                //pos = f;
                //System.Console.WriteLine(pos);
            }
        }

        void Update()
        {
            foreach (Button b in new Button[] { Forward,Back,Left,Right,StrafeLeft,StrafeRight,Fire1,Fire2,Select})
            {
                ControlInfo ci = GetControl(b);

                //if (ci.Control == null)
                //    continue;

                if (ci.IsAxis)
                {
                    bool plus=(IsFirstOfAxis(b)&&!ci.Invert)||(!IsFirstOfAxis(b)&&ci.Invert);
                    b.Caption = ci.Id.ToString() + " Axis " + ci.AxisButton1 + (plus ? "+" : "-");
                }
                else if (GetOtherButton(b) != null)
                {
                    if (ci.Id == ControlID.Keyboard)
                    {
                        b.Caption = ci.Id.ToString() + " " + (IsFirstOfAxis(b) ? ((KeyCode)ci.AxisButton1).ToString() : ((KeyCode)ci.Button2).ToString());
                    }
                    else
                        b.Caption = ci.Id.ToString() + " " + (IsFirstOfAxis(b) ? ci.AxisButton1.ToString() : ci.Button2.ToString());                   
                }
                else
                {
                    if (ci.Id == ControlID.Keyboard)
                        b.Caption = ci.Id.ToString() + " " + ((KeyCode)ci.AxisButton1).ToString();
                    else
                        b.Caption = ci.Id.ToString() + " " + ci.AxisButton1.ToString();
                }
            }
            MouseLook.Checked = Control.MouseLook;
        }


        Controls.SpaceShipControl Control=new SpaceWar2006.Controls.SpaceShipControl();

        public override void Close()
        {
            base.Close();
            //XmlSerializer xml = new XmlSerializer(typeof(ControlInfo[]));
            //Control.Save();

        }
        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            if (TimeToStartCheck > 0.0f)
            {
                TimeToStartCheck -= dtime;
                if (TimeToStartCheck < 0.0f)
                    TimeToStartCheck = 0;
            }
            CheckControls();
        }
        Button Forward;
        Button Back;
        //ControlInfo Thrust = new ControlInfo();

        Button Left;
        Button Right;
        //ControlInfo Rotate = new ControlInfo();

        Button StrafeLeft;
        Button StrafeRight;
        //ControlInfo Strafe = new ControlInfo();

        Button Fire1;
        //ControlInfo Fire1Control = new ControlInfo();
        Button Fire2;
        //ControlInfo Fire2Control = new ControlInfo();
        Button Select;
        //ControlInfo SelectControl = new ControlInfo();
        CheckBox MouseLook;


        Button Selected;
    }

    public class ConfigMenu : Window
    {
        public ConfigMenu()
            : base(new Layout(2, 6))
        {
            Transparent = true;

            Add(new Button("Name:"), 0, 0);
            Name = new TextBox(c.GetString("player.name"), false);
            Add(Name, 1, 0);

            Add(new Button("Team:"), 0, 1);
            Team = new TextBox("0", false);
            Add(Team, 1, 1);

            Add(new Button(OkButtonPressed, "OK"), 1, 2);

            //Name.AppendLine(c.GetString("player.name"));
            Layout.Update(Size);
        }

        protected void OkButtonPressed(Button source,int button, float x, float y)
        {
            c.Set("player.name", Name.GetLine(0));
            c.Save();
            Close();
        }

        Config c = Root.Instance.ResourceManager.LoadConfig("config/global.config");
        TextBox Name;
        TextBox Team;
    }

    public class JoinGameMenu : Window
    {
        public JoinGameMenu()
            : base(100, 400, 420, 300, new Layout(3, 2))
        {
            Color = new Color4f(0.0f, 0.0f, 0.0f, 1.0f);
            Center();
            Add(Host = new TextBox("localhost", false), 0, 0);
            Add(new Button(new Button.ClickDelegate(ConnectButtonClicked), "Connect!"), 1, 0);
            //Add(new Button(new Button.ClickDelegate(BackButtonClicked), "Back"), 2, 0);

            ServerList = new ListBox();
            ServerList.SelectionChangedEvent += new ListBox.SelectionChangedDelegate(ServerList_SelectionChangedEvent);
            Add(ServerList, 0, 1);
            Layout.Widths[0] = 3;
            Layout.Heights[0] = 0.1f;
            Layout.Update(this.Size);
            Layout.GetCell(0, 1).Span.X = 3;

            Scanner = new LanScanner();
            Scanner.Answer += OnServerAnswer;
        }

        void ServerList_SelectionChangedEvent(ListBox lb)
        {
            Host.SetLine(0, lb.Selected.Object.ToString());
        }

        /*protected void BackButtonClicked(Button source,int button, float x, float y)
        {
            Root.Instance.Gui.windows.Remove(this);
            Root.Instance.Gui.windows.Add(new MainMenu());
        }*/
        protected void ConnectButtonClicked(Button source,int button, float x, float y)
        {
            Root.Instance.Gui.windows.Remove(this);
            Root.Instance.CurrentFlow.Stop();
            Root.Instance.CurrentFlow = new Game(Host.GetLine(0), null, false, false);
            Root.Instance.CurrentFlow.Start();
        }
        /*
        public override void OnChildClick(Window w, int button)
        {
            foreach (Button b in Servers)
            {
                if (b == w)
                {
                    if (b.UserData != null)
                        Host.SetLine(0, b.UserData.ToString());
                    break;
                }
            }
        }
    */
        protected void OnServerAnswer(Cheetah.ISerializable p, IPEndPoint ep)
        {
            throw new Exception("NYI");//HACK
            /*
            for (int i = 0; i < Servers.Count; ++i)
            {
                if (((IPEndPoint)Servers[i].Object).ToString() == ep.ToString())
                {
                    //Servers[i].Text = p.Name + "[" + ep.ToString() + "] (" + p.MaxPlayers + ")";
                    //ServerList.SetContents(Servers.ToArray());
                    return;
                }
            }
            Servers.Add(new ListBoxItem(ep, null, p.Name + "[" + ep.ToString() + "] (" + p.MaxPlayers + ")"));
            ServerList.SetContents(Servers.ToArray());*/
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            Scanner.Tick(dtime);
        }

        TextBox Host;
        LanScanner Scanner;
        ListBox ServerList;
        List<ListBoxItem> Servers = new List<ListBoxItem>();
        //int Count = 0;
    }


    public class TeamMenu : Window
    {
        public TeamMenu(int team)
            : base(new Layout(3, 1))
        {
            Transparent = true;
            Add(PrevTeam = new Button(PrevButtonClicked, "Prev Team"), 0, 0);
            Add(NextTeam = new Button(NextButtonClicked, "Next Team"), 2, 0);
            Add(CurrentTeam = new Button(""), 1, 0);
            SetTeam(team);
        }

        void SetTeam(int team)
        {
            Team = team;
            if (Team >= 0)
            {
                CurrentTeam.Caption = SpaceWar2006.GameObjects.Team.ColorNames[Team];
            }
            else
            {
                CurrentTeam.Caption = "-none-";
            }
        }

        void PrevButtonClicked(Button source,int button, float x, float y)
        {
            Team--;
            if (Team == -2)
                Team = 2;
            SetTeam(Team);
        }
        void NextButtonClicked(Button source,int button, float x, float y)
        {
            Team++;
            if (Team > 2)
                Team = -1;
            SetTeam(Team);
        }
        public int Result
        {
            get
            {
                return Team;
            }
        }

        int Team;
        Button NextTeam;
        Button PrevTeam;
        Button CurrentTeam;
    }

    public class GameMenu : Window
    {
        public GameMenu(int team, Type ship)
            : base(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y*0.9f)
        {
            Center();
            Transparent = true;

            Layout = new Layout(3, 3);

            Layout.Heights[0] = Layout.Heights[2] = 0.1f;

            Add(Team = new TeamMenu(team), 0, 0);
            Layout.GetCell(0, 0).Span.X = 2;

            Add(Ship = new ShipMenu(ship), 0, 1);
            Layout.GetCell(0, 1).Span.X = 2;

            Add(new Button(OkButtonClicked, "OK"), 0, 2);
            Add(new Button(QuitButtonClicked, "Quit"), 1, 2);


            Layout.GetCell(2, 0).Span.Y = 3;
            Layout.Widths[2] = 4;
            Add(new GameInfoDisplay(),2,0);

            Layout.Update(Size);
        }

        void OkButtonClicked(Button source,int button, float x, float y)
        {
            Select(Team.Result, Ship.Result);
            Close();
        }
        void QuitButtonClicked(Button source,int button, float x, float y)
        {
            Root.Instance.CurrentFlow.Stop();
        }
        TeamMenu Team;
        ShipMenu Ship;

        public delegate void SelectDelegate(int team, Type ship);
        public event SelectDelegate Select;
    }

    public class RadarDisplay : Window
    {
        public RadarDisplay()
            : base()
        {
            Shader = Root.Instance.ResourceManager.LoadShader("radar.shader");
            PointBuffer = Root.Instance.UserInterface.Renderer.CreateDynamicVertexBuffer(256 * 36);
            PointBuffer.Format = VertexFormat.VF_P3C4T2;

            Size = new Vector2(200,200);
            Position=new Vector2(Root.Instance.UserInterface.Renderer.Size.X-Size.y-8,8);
        }

        void UpdateBuffer()
        {
            IList<Node> list = Root.Instance.Scene.FindEntitiesByType<Node>();

            Vector3 offset = new Vector3(5000, 0, 5000);
            Vector3 scale = new Vector3(Size.x / 10000.0f, Size.y / 10000.0f, 0);

            int i = 0;
            foreach (Node n in list)
            {
                Color4f c;
                float a = 0.7f;
                float pointsize = 1;
                if (n is SpaceShip)
                {
                    c = new Color4f(1, 1, 0, a);
                    pointsize = 5;
                }
                else if (n is Projectile)
                    c = new Color4f(1, 0, 0, a);
                else if (n is Planet)
                {
                    pointsize = 7;
                    c = new Color4f(0, 0, 1, a);
                }
                else if (n is Actor)
                {
                    c = new Color4f(1, 1, 1, a);
                    pointsize = 3;
                }
                else if (n is BigExplosion)
                {
                    c = new Color4f(1, 1, 1, a);
                    pointsize = 11;
                }
                else if (n is SpawnPoint || n is PlayerStart)
                {
                    c = new Color4f(0, 1, 1, a);
                    pointsize = 11;
                }
                else if (n is Flag)
                {
                    c = new Color4f(0.5f, 1, 0, a);
                    pointsize = 11;
                }
                else
                    continue;

                Vector3 v = n.AbsolutePosition + offset;
                v = new Vector3(v.X * scale.X, v.Z * scale.Y, 0);
                //v.X += Position.x;
                //v.Y += Position.y;
                Vertices[i].position = v;
                Vertices[i].texture0.x = pointsize;
                c.a = pointsize;


                Vertices[i++].color = c;
            }
            Count = i;

            PointBuffer.Update(Vertices, Count * 36);
        }
        public override void DrawInternal(IRenderer r, RectangleF rect)
        {
            base.DrawInternal(r, rect);

            r.SetMode(RenderMode.Draw3DPointSprite);
            r.UseShader(Shader);
            r.Draw(PointBuffer, PrimitiveType.POINTS, 0, Count, null);
            r.SetMode(RenderMode.Draw2D);
        }
        public override void Draw(IRenderer r, RectangleF rect)
        {
            base.Draw(r, rect);

        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            UpdateBuffer();
        }
        VertexP3C4T2[] Vertices = new VertexP3C4T2[256];
        DynamicVertexBuffer PointBuffer;
        Shader Shader;
        int Count;
    }

    public class ControlDisplay : Window
    {
        public ControlDisplay(float w,SpaceShip s)
            : base(0, 0, Root.Instance.UserInterface.Renderer.Size.X, Root.Instance.UserInterface.Renderer.Size.Y, new Layout(2, 4))
        {
            Transparent = true;

            Layout.Widths[0] = Root.Instance.UserInterface.Renderer.Size.X-250;
            Layout.Widths[1] = 250;

            Layout.GetCell(0, 0).Span.Y = 4;

            Layout.Heights[0] = 0.5f;
            WeaponDisplay = new WeaponDisplay(s.Slots);
            Add(WeaponDisplay, 1, 0);

            WeaponManager = new WeaponManager(s);
            Add(WeaponManager, 1, 1);

            Comm = new Chat(s.GetPlayer());
            Add(Comm, 1, 2);

            RadarDisplay rd = new RadarDisplay();
            Add(rd, 1, 3);

            //InfoWindow = new GameInfoDisplay();
            //InfoWindow.Visible = false;
            //Add(InfoWindow,0,0);

            Layout.Update(Size);
        }

        public Chat Comm;
        public WeaponManager WeaponManager;
        WeaponDisplay WeaponDisplay;
        //GameInfoDisplay InfoWindow;

        public SpaceShip Ship
        {
            set
            {
                Comm.player = value.GetPlayer();
                WeaponManager.Ship = value;
                WeaponDisplay.Slots = value.Slots;
            }
        }
    }

    public class WeaponBar : Window
    {
        public WeaponBar(SpaceShip s, WeaponDisplay d)
        {
            ship = s;
            display = d;

            icons = new MeshWindow[SpaceShip.WeaponList.Length-1];
            for (int i = 0; i < SpaceShip.WeaponList.Length-1; ++i)
            {
                icons[i] = new MeshWindow(GetIcon(SpaceShip.WeaponList[i]));
                if(ship.CurrentWeapon==i)
                {
                    Style(icons[i], true);
                    currentweapon = i;
                }
                else
                {
                    Style(icons[i], false);
                }
            }

            float pixelsize = 48;

            Layout = new Layout(1, icons.Length);
            Size = new Vector2(pixelsize, icons.Length * pixelsize);
            Position = new Vector2(
                8,
                Root.Instance.UserInterface.Renderer.Size.Y / 2 - Size.y / 2
                );
            Transparent = true;

            for(int i=0;i<icons.Length;++i)
                Add(icons[i],0,i);

            Layout.Update(Size);

            UpdateDisplayPosition();
        }

        void UpdateStyles(int prev, int next)
        {
            Style(icons[prev], false);
            Style(icons[next], true);
        }

        public override void Tick(float dtime)
        {
            if (currentweapon != ship.CurrentWeapon)
            {
                UpdateStyles(currentweapon, ship.CurrentWeapon);
                currentweapon = ship.CurrentWeapon;
                UpdateDisplayPosition();
           }

            base.Tick(dtime);
        }


        public override void Draw(IRenderer r, RectangleF rect)
        {
            base.Draw(r, rect);

            for (int i = 0; i < SpaceShip.WeaponList.Length - 1; ++i)
            {
                Vector2 pos=icons[i].Position+Position;
                Root.Instance.Gui.DefaultFont.Draw(r, ship.Inventory.Count(SpaceShip.WeaponList[i]).ToString(), pos.x, pos.y,new Color4f(0,1,0,1));
            }
        }
        void UpdateDisplayPosition()
        {
            display.Position = Position + icons[currentweapon].Position + new Vector2(icons[currentweapon].Size.x, 0) + new Vector2(8, 0);
        }

        WeaponDisplay display;

        Mesh GetIcon(Type t)
        {
            string name;
            if (t == typeof(LaserCannon))
            {
                name = "plasma-icon/default.mesh";
            }
            else if (t == typeof(MineLayer))
            {
                name = "mine-icon/default.mesh";
            }
            else if (t == typeof(IonPulseCannon))
            {
                name = "ion-icon/default.mesh";
            }
            else if (t == typeof(HomingMissileLauncher))
            {
                name = "rocket-icon/default.mesh";
            }
            else if (t == typeof(PulseLaserCannon))
            {
                name = "laser-icon/default.mesh";
            }
            else if (t == typeof(RailGun))
            {
                name = "rail-icon/default.mesh";
            }
            else if (t == typeof(DisruptorCannon))
            {
                name = "disruptor-icon/default.mesh";
            }
            else
            {
                name = "laser-icon/default.mesh";
            }

            return Root.Instance.ResourceManager.LoadMesh(name);
        }

        MeshWindow[] icons;
        SpaceShip ship;
        int currentweapon;

        void Style(MeshWindow w, bool active)
        {
            if (active)
            {
                w.Color = new Color4f(1.0f, 1.0f, 1.0f, 0.5f);
                w.Scale = new Vector2(1.5f, 1.5f);
                w.Transparent = false;
            }
            else
            {
                w.Color = new Color4f(0.5f, 1.0f, 0.5f, 0.0f);
                w.Scale = new Vector2(1.0f, 1.0f);
                w.Transparent = true;
            }
        }
    }

    public class WeaponManager : Window
    {
        public WeaponManager(SpaceShip ship)
            :base(new Layout(ship.Slots.Length+1,SpaceShip.WeaponList.Length))
        {
            Ship = ship;

            Transparent = true;

            WeaponButtons = new Button[ship.Slots.Length, SpaceShip.WeaponList.Length - 1];

            //slot numbers
            for (int j = 0; j < ship.Slots.Length; ++j)
            {
                Button b2 = new Button(j.ToString());
                Add(b2, j + 1, 0);
            }

            //weapon matrix
            for (int i = 0; i < SpaceShip.WeaponList.Length; ++i)
            {
                Type t = SpaceShip.WeaponList[i];
                if (t == null)
                    break;

                //weaponbutton
                Button b = new Button(t.Name);
                Add(b, 0, i + 1);

                for (int j = 0; j < ship.Slots.Length; ++j)
                {
                    /*string text;
                    if (ship.Slots[j].Weapon != null && ship.Slots[j].Weapon.GetType() == t)
                        text = "X";
                    else if (ship.CanArmWeapon(SpaceShip.WeaponList[i]))
                        text = "+";
                    else
                        text = "-";*/
                    Button b2 = new Button(WeaponButtonClicked,"");
                    b2.UserData = new Point(j, i);
                    Add(b2, j + 1, i + 1);
                    WeaponButtons[j, i] = b2;
                }
            }

            Update();
            //Layout.Update

            //width = 500;
            Layout.Widths[0] = 4;
            Size = new Vector2(500,Root.Instance.Gui.DefaultFont.size * Layout.Height);
        }

        public override void OnResize()
        {
            base.OnResize();
        }

        protected void Update()
        {
            for (int i = 0; i < WeaponButtons.GetLength(0); ++i)
            {
                for (int j = 0; j < WeaponButtons.GetLength(1); ++j)
                {
                    Button b = WeaponButtons[i, j];
                    Type t = SpaceShip.WeaponList[j];
                    if (t == null)
                        break;

                    string text;
                    if (Ship.Slots[i].Weapon != null && Ship.Slots[i].Weapon.GetType() == t)
                        text = "X";
                    else if (Ship.CanArmWeapon(SpaceShip.WeaponList[j]))
                        text = "+";
                    else
                        text = "-";

                    b.Caption = text;
                }
            }

            //Layout.Update
        }

        protected void WeaponButtonClicked(Button source, int button, float x, float y)
        {
            Point p = (Point)source.UserData;
            if (Ship.CanArmWeapon(SpaceShip.WeaponList[p.Y]))
            {
                Ship.ArmWeapon(p.Y, p.X);
                Update();
            }
        }

        public SpaceShip Ship;
        Button[,] WeaponButtons;
    }

    public class WeaponDisplay : Window
    {
        public WeaponDisplay(Slot[] slots)
            : base(Root.Instance.UserInterface.Renderer.Size.X - 150, 0, 96, slots.Length * 16, new Layout(1, slots.Length))
        {
            Transparent = true;

            WeaponButtons = new Button[slots.Length];
            WeaponProgress = new ProgressBar[slots.Length];

            for (int i = 0; i < slots.Length; i += 1)
            {
                WeaponButtons[i] = new Button("-none-");
                WeaponButtons[i].Transparent = true;
                Add(WeaponButtons[i], 0, i);
                WeaponProgress[i] = new ProgressBar(new Color4f(0.3f, 0.3f, 0.3f, 0.5f), new Color4f(0, 1, 0, 0.5f), OrientationType.Horizontal);
                Add(WeaponProgress[i], 0, i);
            }

            Slots = slots;
            Layout.Update(Size);
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            UpdateButtons();
        }

        string GetName(Weapon w)
        {
            Type t = w.GetType();
            if(t==typeof(IonPulseCannon))
            {
                return "Ion";
            }
            else if(t==typeof(RailGun))
            {
                return "Rail";
            }
            else if (t == typeof(HomingMissileLauncher))
            {
                return "Rockets";
            }
            else if (t == typeof(DisruptorCannon))
            {
                return "Disruptor";
            }
            else if (t == typeof(LaserCannon))
            {
                return "Laser";
            }
            else if (t == typeof(PulseLaserCannon))
            {
                return "PulseLaser";
            }
            else if (t == typeof(MineLayer))
            {
                return "Mines";
            }
            else
            {
                return t.Name;
            }
        }

        public void UpdateButtons()
        {
            for (int i = 0; i < Slots.Length; ++i)
            {
                if (Slots[i].Weapon != null)
                {
                    float f = Slots[i].Weapon.CurrentReloadTime / Slots[i].Weapon.ReloadTime;
                    WeaponButtons[i].Caption = GetName(Slots[i].Weapon) + " " + GetAmmo(Slots[i].Weapon);
                    WeaponProgress[i].Value = f;
                    WeaponProgress[i].BarColor = new Color4f(1 - f, f, 0, 0.5f);
                }
                else
                {
                    WeaponButtons[i].Caption = "-none-";
                    WeaponProgress[i].Value = 0;
                }
            }
        }

        string GetAmmo(Weapon w)
        {
            if (w.ProjectileType != null && w.AmmoSource!=null)
                return w.AmmoSource.Count(w.ProjectileType).ToString();
            else
                return "";
        }

        Button[] WeaponButtons;
        ProgressBar[] WeaponProgress;
        public Slot[] Slots;
    }
    /*
    public class MissionDisplay : Window
    {
        public MissionDisplay()
            : base(new Layout(2, m.PrimaryObjectives.Length + m.SecondaryObjectives.Length + 2))
        {
            Add(new Button("Primary objectives"), 0, 0);

            int y = 1;

            for (int i = 0; i < m.PrimaryObjectives; ++i)
            {
                Add(new Button(""), 0, y);
                Add(new Button(""), 1, y);
                y++;
            }
            Add(new Button("Secondary objectives"), 0, y++);

            for (int i = 0; i < m.SecondaryObjectives; ++i)
            {
                Add(new Button(""), 0, y);
                Add(new Button(""), 1, y);
                y++;
            }
        }

        void Update()
        {
        }

        Mission Mission;
    }
    */
    public class GameInfoDisplay : Window
    {
        public GameInfoDisplay()
            : base(new Layout(4, 18))
        {

            System.Drawing.Point p = Root.Instance.UserInterface.Renderer.Size;
            Position = new Vector2(0.2f * p.X, 0.1f * p.Y);
            Size = new Vector2(0.6f * p.X, 0.80f * p.Y);

            //Color = new Color4f(0.2f, 0.5f, 0.3f, 0.6f);
            //Transparent = true;

            Layout.GetCell(0, 0).Span.X = 4;

            Add(Title = new Button(""), 0, 0);

            Add(new Button("Nick"), 0, 1);
            Add(new Button("Frags"), 1, 1);
            Add(new Button("Deaths"), 2, 1);
            Add(new Button("RTT"), 3, 1);

            for (int i = 0; i < 16; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    Add(new Button(""), j, 2 + i);
                }
            }

            foreach (Window w in Children)
                w.Transparent = true;

            Layout.Update(this.Size);

            //QueryPacket p=((UdpClient)Root.Instance.Connection).Query("localhost");
            //Cheetah.Console.WriteLine(p.Info);

            //Root.Instance.Timer1s.Function += Update;
        }
        Button Title;

        private void SetRow(int row, string name, string frags, string deaths, string rtt, Color4f color)
        {
            ((Button)Layout.GetCell(0, row + 2).Windows[0]).Caption = name;
            ((Button)Layout.GetCell(0, row + 2).Windows[0]).TextColor = color;

            ((Button)Layout.GetCell(1, row + 2).Windows[0]).Caption = frags;
            ((Button)Layout.GetCell(1, row + 2).Windows[0]).TextColor = color;

            ((Button)Layout.GetCell(2, row + 2).Windows[0]).Caption = deaths;
            ((Button)Layout.GetCell(2, row + 2).Windows[0]).TextColor = color;

            ((Button)Layout.GetCell(3, row + 2).Windows[0]).Caption = rtt;
            ((Button)Layout.GetCell(3, row + 2).Windows[0]).TextColor = color;

        }
        private void SetEmptyRow(int row)
        {
            SetRow(row, "", "", "", "", new Color4f(1, 1, 1, 1));
        }
        private void SetTeamRow(int row, int team, string name, int score, Color4f color)
        {
            SetRow(row, "Team:", "\"" + name + "\"", "Score:", score.ToString(), color);
        }

        private void Update()
        {

            if (!Visible)
                return;

            IList<Player> plist = Root.Instance.Scene.FindEntitiesByType<Player>();
            Player[] Players = new Player[plist.Count];
            plist.CopyTo(Players, 0);
            //p.
            GameRule Rule = Root.Instance.Scene.FindEntityByType<GameRule>();

            if (Rule != null)
                Title.Caption = Rule.ToString();
            else
                Title.Caption = "SpaceWar 2006";

//#if WINDOWS
            Array.Sort<Player>(Players, new Comparison<Player>(delegate(Player e1, Player e2)
            {
                int i1 = ((Player)e1).Team;
                int i2 = ((Player)e2).Team;

                if (i1 < i2) return -1;
                if (i1 > i2) return 1;

                int f1 = ((Player)e1).Frags;
                int f2 = ((Player)e2).Frags;

                if (f1 > f2) return -1;
                if (f1 < f2) return 1;

                return 0;
            }));
//#endif

            int i = 0;

            if (Rule != null && Rule is TeamDeathMatch)
            {
                TeamDeathMatch tdm = (TeamDeathMatch)Rule;
                int currentteam = -1;
                //foreach (Player p in Players)
                for (int j = 0; j < Players.Length; ++j)
                {
                    Player p = Players[j];
                    Color4f c = new Color4f(1, 1, 1, 1);
                    if (p.Team >= 0)
                        c = Team.Colors[p.Team];


                    if (p.Team != currentteam)
                    {
                        currentteam = p.Team;
                        SetTeamRow(i++, p.Team, tdm.Teams[p.Team].Name, (Rule is CaptureTheFlag) ? ((CtfTeam)tdm.Teams[p.Team]).Captures : tdm.Teams[p.Team].Score, c);
                    }

                    SetRow(i++, p.Name, p.Frags.ToString(), p.Deaths.ToString(), p.RTT.ToString(), c);
                }
            }
            else if (Rule != null && Rule is Domination)
            {
                Domination dom = (Domination)Rule;
                int currentteam = -1;
                //foreach (Player p in Players)
                for (int j = 0; j < Players.Length; ++j)
                {
                    Player p = Players[j];
                    Color4f c = new Color4f(1, 1, 1, 1);
                    if (p.Team >= 0)
                        c = Team.Colors[p.Team];


                    if (p.Team != currentteam)
                    {
                        currentteam = p.Team;
                        SetTeamRow(i++, p.Team, dom.Teams[p.Team].Name, dom.Teams[p.Team].Score, c);
                    }

                    SetRow(i++, p.Name, p.Frags.ToString(), p.Deaths.ToString(), p.RTT.ToString(), c);
                }
            }
            else if (Rule != null && Rule is Race)
            {
                Race race = (Race)Rule;

                for (int j = 0; j < Players.Length; ++j)
                {
                    RacePlayer p = (RacePlayer)Players[j];
                    SetRow(i++, p.Name, p.Frags.ToString(), p.Checks.ToString(), p.RTT.ToString(), new Color4f(1, 1, 1, 1));
                }
            }
            else if (Rule != null && Rule is Mission)
            {
                Mission m = (Mission)Rule;

                for (int j = 0; j < m.Missions.Length; ++j)
                {
                    SingleMission sm = m.Missions[j];
                    SetTeamRow(i++, j, m.Teams[j].Name, m.Teams[j].Score, Team.Colors[j]);

                    for (int k = 0; k < sm.PrimaryObjectives.Length; ++k)
                    {
                        Objective o = sm.PrimaryObjectives[k];
                        SetRow(i++, o.Text, "", o.CurrentStatus.ToString(), "", new Color4f(1,1,1,1));
                    }
                }
            }
            else
            {
                //foreach (Player p in Players)
                for (int j = 0; j < Players.Length; ++j)
                {
                    Player p = Players[j];
                    SetRow(i++, p.Name, p.Frags.ToString(), p.Deaths.ToString(), p.RTT.ToString(), new Color4f(1, 1, 1, 1));
                }
            }

            for (; i < 16; ++i)
                SetEmptyRow(i);
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            Update();
        }

        //GameStats Stats;
        //Button[] Buttons;
    }

}
