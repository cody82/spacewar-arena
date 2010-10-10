using System;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Collections.Generic;

using Cheetah;
using Cheetah.Graphics;

namespace SpaceWar2006
{
    public class Program
    {
        static void ServerMain(string[] args)
        {
            Root r = new Root(args, true);
            r.ServerServer(args);

            r.NextIndex += 10;

            while (!Root.Instance.Quit)
            {
                Flow f = new SpaceWar2006.Flows.GameServer();
                r.CurrentFlow = f;
                f.Start();
                r.ServerRun(true);
                f.Stop();
            }

            r.ServerStop();
            r.Dispose();
        }

        static Dictionary<IPEndPoint, GameSystem.GameServerInfo> servers = new Dictionary<IPEndPoint, SpaceWar2006.GameSystem.GameServerInfo>();

        public static void Answer(Lidgren.Network.NetIncomingMessage msg)
        {
            GameSystem.GameServerInfo info = new SpaceWar2006.GameSystem.GameServerInfo(msg.ReadString());
            servers[msg.SenderEndpoint] = info;
        }

        public static void ConsoleMain(string[] args)
        {
            Root r = new Root(args, false);
            SpaceWar2006.GameSystem.Mod.Instance.Init();

            ServerFinder scanner = new ServerFinder(Answer,true,true);

            for (int i = 0; i < 50; ++i)
            {
                scanner.Tick(0.1f);
                Thread.Sleep(100);
            }

            System.Console.WriteLine("---------SERVERS----------");

            List<IPEndPoint> list = new List<IPEndPoint>();
            int j = 0;
            foreach (KeyValuePair<IPEndPoint, GameSystem.GameServerInfo> kv in servers)
            {
                list.Add(kv.Key);
                j++;
                System.Console.Write("["+ j.ToString()+"] "+kv.Key.ToString()+": ");
                System.Console.WriteLine(kv.Value.ToString());
            }

            System.Console.WriteLine("--------------------------");
            System.Console.Write("select server: ");
            int select = int.Parse(System.Console.ReadLine()) - 1;

            IPEndPoint connect = list[select];

            System.Console.WriteLine("Connecting to "+connect.ToString()+"...");

            args = new string[] { "client", "-connect", connect.ToString() };
            Root.Instance.Args = args;
            r.ClientClient(args);

            Flow f = new SpaceWar2006.Flows.ClientStart();

            r.CurrentFlow = f;

            f.Start();

            r.ClientLoop();

            r.Dispose();
        }

        public static void ClientMain(string[] args)
        {
            Root r = new Root(args, false);
            r.ClientClient(args);
            IUserInterface ui = r.UserInterface;

            Flow f = new SpaceWar2006.Flows.ClientStart();

            r.CurrentFlow = f;

            f.Start();

            r.ClientLoop();

            r.Dispose();
        }
        static void Convert(string[] args)
        {
            Root r = new Root(args, true);

            SubMesh sm = (SubMesh)r.ResourceManager.Load(args[0], typeof(SubMesh));
            SubMeshSaver sms = new SubMeshSaver();
            sms.Save(sm, new FileStream(args[1], FileMode.CreateNew));
        }

        static void ConvertModel(string[] args)
        {
            Root r = new Root(args, true);

            FileSystemNode models=r.FileSystem.Get("models");

            FileSystemNode model = (FileSystemNode)models[args[0]];

            Hashtable ht = new Hashtable();


            //collect submeshes to convert
            foreach (DictionaryEntry de in model)
            {
                string name = (string)de.Key;

                if (name.EndsWith(".submesh") && !model.ContainsKey(name+".bin"))
                {
                    ht.Add(de.Key, de.Value);

                }

            }

            //convert all files in ht
            foreach (DictionaryEntry de in ht)
            {
                string name = (string)de.Key;

                FileSystemNode submeshnode = (FileSystemNode)de.Value;

                SubMesh sm = (SubMesh)r.ResourceManager.Load(submeshnode, typeof(SubMesh));

                FileSystemNode newnode = model.CreateFile(name + ".bin");

                SubMeshSaver sms = new SubMeshSaver();
                sms.Save(sm, newnode.getStream());
            }
        }

        static void RegisterProtocol()
        {
#if !LINUX
            Microsoft.Win32.RegistryKey classes;
            classes = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes", true);

            Microsoft.Win32.RegistryKey spacewar;
            spacewar = classes.CreateSubKey("spacewar");

            Microsoft.Win32.RegistryKey shell;
            shell = spacewar.CreateSubKey("shell");

            Microsoft.Win32.RegistryKey open;
            open = shell.CreateSubKey("open");

            Microsoft.Win32.RegistryKey command;
            command = open.CreateSubKey("command");

            Microsoft.Win32.RegistryKey icon;
            icon = spacewar.CreateSubKey("Defaulticon");

            spacewar.SetValue("URL Protocol", "");

            string cmd = Assembly.GetEntryAssembly().Location + " client -connect \"%1\"";
            command.SetValue("", cmd);

            command.Close();
            icon.Close();
            open.Close();
            shell.Close();
            spacewar.Close();
            classes.Close();
#endif
        }

        static void ViewerMain()
        {
        }


        [STAThread]
        static void Main(string[] args)
        {
            FFmpegFile f = new FFmpegFile(@"D:\Code\spacewar2006\dist\textures\0190.ogv");
            return;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");

                Spacewar2006.Forms.LoadForm load = null;
                /*if (args.Length == 0)
                {
                    load = new Spacewar2006.Forms.LoadForm();
                    load.Show();
                    System.Windows.Forms.Application.DoEvents();
                }*/

                string dir = Directory.GetCurrentDirectory();
                Assembly a = Assembly.GetEntryAssembly();
                System.Console.WriteLine("assembly path:" + a.Location);

                int i = Array.IndexOf<string>(args, "-root");
                if (i != -1)
                {
                    string rootdir = args[i + 1];
                    Directory.SetCurrentDirectory(rootdir);
                    System.Console.WriteLine("root directory: " + rootdir);
                }
                else
                {
                    DirectoryInfo current = new FileInfo(a.Location).Directory;
                    while (current.GetFiles("cheetah_root").Length == 0)
                    {
                        if ((current = current.Parent) == null)
                        {
                            throw new Exception("Can't find game root directory. Use -root $directory.");
                        }
                    }
                    Directory.SetCurrentDirectory(current.FullName);
                    System.Console.WriteLine("root directory: " + current.FullName);
                }



                if (Array.IndexOf<string>(args, "server") != -1)
                {
                    ServerMain(args);
                }
                else if (Array.IndexOf<string>(args, "client") != -1)
                {
                    ClientMain(args);
                }
                else if (Array.IndexOf<string>(args, "clientserver") != -1)
                {
                    System.Console.WriteLine("client started. launching server...");
                    Process server = Process.Start("Game.exe", "server");
                    Thread.Sleep(1000);
                    System.Console.WriteLine("done.");

                    ClientMain(args);

                    server.Kill();
                }
                else if (Array.IndexOf<string>(args, "viewer") != -1)
                {
                    ViewerMain();
                }
                else if (Array.IndexOf<string>(args, "register") != -1)
                {
                    RegisterProtocol();
                }
                else if (Array.IndexOf<string>(args, "convert") != -1)
                {
                    Convert(args);
                }
                else if (Array.IndexOf<string>(args, "convertmodel") != -1)
                {
                    ConvertModel(args);
                }
                else if (Array.IndexOf<string>(args, "search") != -1)
                {
                    ConsoleMain(args);
                }
                else
                {
                    Root r = new Root(args, false);

                    System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

                    SpaceWar2006.GameSystem.Mod.Instance.Init();
                    load = null;
                    new Spacewar2006.Forms.MainForm().ShowDialog();

                }
            }
        }

        static bool mailing_done = false;
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (!mailing_done)
            {
                Cheetah.Bugs.BugReport.Instance.Send(e.Exception);
                mailing_done = true;
            }
            throw e.Exception;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!mailing_done)
            {
                Cheetah.Bugs.BugReport.Instance.Send((Exception)e.ExceptionObject);
                mailing_done = true;
            }
        }
    }
}