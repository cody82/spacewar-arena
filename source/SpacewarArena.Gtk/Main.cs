using System;
using Gtk;

using Cheetah;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Threading;

namespace SpacewarArena.Gtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");


            string dir = Directory.GetCurrentDirectory();
            Assembly a = Assembly.GetEntryAssembly();
            System.Console.WriteLine("assembly path:"+a.Location);

            int i=Array.IndexOf<string>(args,"-root");
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

			
			Application.Init ();
			
			
			                    Root r = new Root(args, false);

                    SpaceWar2006.GameSystem.Mod.Instance.Init();

			
			
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}
