using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lidgren.Library.Network;
using System.Threading;
using System.Drawing;

namespace SimpleChatClient
{
	static class Program
	{
		public static Form1 MainForm;
		public static NetClient Client;
		public static NetLog Log;
		public static NetAppConfiguration Config;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new Form1();

			// Create a configuration for the client
			// 'SimpleChat' is the server application identifier we want to connect to
			Config = new NetAppConfiguration("SimpleChat");

			// enable encryption; this key was generated using the 'GenerateEncryptionKeys' application
			Config.EnableEncryption(
 "AQAB1nF0bzkPd+oG2lXk1lraVovWJHTt9+fuZKUZ4VoH6dmeO8GnuDDjWAk+KjeHh" +
 "u1Malaf1DJpdzehTTKXvh9JV47+GUSgyGLjeuASBQCcqi3RVTe0S0u6KhOkdclDjN" +
 "gr07oFlyR96UdTLhUtYUnp/E1xbzDgU//A9fdjAP/oBeU=", null);
			
			Log = new NetLog();
			Log.IgnoreTypes = NetLogEntryTypes.None; //  Verbose;

			Log.OutputFileName = "clientlog.html";

			// uncomment this if you want to start multiple instances of this process on the same computer
			//Log.OutputFileName = "clientlog" + System.Diagnostics.Process.GetCurrentProcess().Id + ".html";

			Log.LogEvent += new EventHandler<NetLogEventArgs>(Log_LogEvent);

			Client = new NetClient(Config, Log);
			Client.StatusChanged += new EventHandler<NetStatusEventArgs>(Client_StatusChanged);

			Log.Info("Enter ip or 'localhost' to connect...");

			Application.Idle += new EventHandler(ApplicationLoop);
			Application.Run(MainForm);

			Client.Shutdown("Application exiting");
		}

		static void ApplicationLoop(object sender, EventArgs e)
		{
			while (Win32.AppStillIdle)
			{
				// call heartbeat as often as possible; this sends queued packets,
				// keepalives and acknowledges etc.
				Client.Heartbeat();

				NetMessage msg;
				// read a packet if available
				while ((msg = Client.ReadMessage()) != null)
					HandleMessage(msg);

				Thread.Sleep(1); // don't hog the cpu
			}
		}

		private static void HandleMessage(NetMessage msg)
		{
			// we received a chat message!
			Log.Debug("msg: " + msg);
			try
			{
				// display it
				Font was = MainForm.richTextBox1.SelectionFont;
				MainForm.richTextBox1.SelectionFont = new Font("Verdana", 8, System.Drawing.FontStyle.Bold);
				MainForm.richTextBox1.AppendText("Received: " + msg.ReadString() + Environment.NewLine);
				MainForm.richTextBox1.SelectionFont = was;
				Win32.ScrollRichTextBox(MainForm.richTextBox1);
			}
			catch { } // ignore disposal problems
		}

		static void Log_LogEvent(object sender, NetLogEventArgs e)
		{
			OutputText(e.Entry.What);
		}

		static void OutputText(string str)
		{
			try
			{
				if (MainForm != null && !MainForm.Disposing && !MainForm.IsDisposed)
				{
					MainForm.richTextBox1.AppendText(str + Environment.NewLine);
					Win32.ScrollRichTextBox(MainForm.richTextBox1);
				}
			}
			catch { } // ignore disposal problems
		}

		static void Client_StatusChanged(object sender, NetStatusEventArgs e)
		{
			Log.Info(e.Connection + ": " + e.Connection.Status + " - " + e.Reason);
		}

		internal static void Entered(string cmd)
		{
			if (Client.Status == NetConnectionStatus.Disconnected)
			{
				// the first time we enter something; connect to that host
				Client.Connect(cmd, 14242);
			}
			else
			{
				// subsequent input; send chat message to server
				// create a message
				NetMessage msg = new NetMessage();
				msg.Write(cmd);

				// send use the ReliableUnordered channel; ie. it WILL arrive, but not necessarily in order
				Client.SendMessage(msg, NetChannel.ReliableUnordered);
			}
		}
	}
}