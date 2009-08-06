using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lidgren.Library.Network;

using NetboardCommon;

namespace NetboardServer
{
	static class Program
	{
		public static Form1 MainForm;
		public static NetServer Server;
		public static NetLog Log;
		public static NetAppConfiguration Config;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new Form1();
			MainForm.Show();

			Log = new NetLog();
			Log.IgnoreTypes = NetLogEntryTypes.Verbose;//  | LogTypes.Debug;
			Log.LogEvent += new EventHandler<NetLogEventArgs>(Log_LogEvent);

			Config = new NetAppConfiguration("Netboard", 14242);
			Config.MaximumConnections = 64;
			Config.ServerName = "Netboard Server";

			// uncomment to enable encryption
			/*
			Config.EnableEncryption(
 "AQAB1nF0bzkPd+oG2lXk1lraVovWJHTt9+fuZKUZ4VoH6dmeO8GnuDDjWAk+KjeHh" +
 "u1Malaf1DJpdzehTTKXvh9JV47+GUSgyGLjeuASBQCcqi3RVTe0S0u6KhOkdclDjN" +
 "gr07oFlyR96UdTLhUtYUnp/E1xbzDgU//A9fdjAP/oBeU=",
 "gV5mwOaql0wfID7rVAnxaH7qDIpiOcm4/wy/ZT08Qu7fT+uPdEZCEQ1abHudjl/AV" +
 "vK1b32ONLbh38Gi27biNg2ywBFl8/0FGVFrbO7pBi2AHPkVBs7E+WUdvfoz9gGtd6" +
 "dPfNqrpyyEYxxQBoqVvc/3Npf4nQxavDj+IxFiZWG2m+FLsdtq2hDLqFoeGbgH9YC" +
 "/ekFrXPeCVAbBLV3aprsFFFSTJvRmU+h1Kwc5KeebxogMSqUXMKCPD80ftpA5PxND" +
 "DuAV4iNJu5HFepcugm8agZaDsn6J7VGPwZzA02X/ZA+5gM5mNL/W9KS0CIQxmIO9A" +
 "CutgFxVJ5GHby/CObFk5lGP9ItWRUZQs2f16JVcEK4G9FPJaFO5izRa0FIHQyAZXw" +
 "yCy2AwVFAFnY6B9sJfEU8SgW3MpFEhf2OZ6m3/ciXJ1XCI2s7zjwfqKfohL/J5006" +
 "u3nRMFOa1BS+My4UCeiryN3LgXRqpFTbffm+caM2lX6/AS9emecnFKMYd1uiJso5x" +
 "m/U+mp5CQeOBsRlCg78s4Ni7BGYD2UAyC0wLlGXPAMiwCoPIUj7avM42VkmDSvPRX" +
 "ZLpkqlpRV6UaQ==");
			*/

			Server = new NetServer(Config, Log);
			Server.StatusChanged += new EventHandler<NetStatusEventArgs>(Server_StatusChanged);

			Application.Idle += new EventHandler(ApplicationLoop);

			Application.Run(MainForm);
		}

		static void ApplicationLoop(object sender, EventArgs e)
		{
			while (Win32.AppStillIdle)
			{
				Server.Heartbeat();

				NetMessage msg;
				while ((msg = Server.ReadMessage()) != null)
					HandleMessage(msg);
			}
		}

		static void Server_StatusChanged(object sender, NetStatusEventArgs e)
		{
			OutputText(e.Connection.ToString() + " - " + e.Connection.Status + ": " + e.Reason);
		}

		static void Log_LogEvent(object sender, NetLogEventArgs e)
		{
			OutputText(e.Entry.Type + ": " + e.Entry.What);
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

		static void HandleMessage(NetMessage msg)
		{
			// someone painted; broadcast this information to everyone (including sender)
			NetBoardTypes tp = (NetBoardTypes)msg.ReadByte();

			if (tp != NetBoardTypes.PaintRequest)
			{
				Log.Warning("Uh oh; Server received something else than a PaintRequest!");
				return;
			}

			PaintRequest request = new PaintRequest();
			request.Decode(msg); // decode

			// we now have information about the paint request

			// construct a paintevent message to notify everyone
			PaintEvent response = new PaintEvent();
			response.FromX = request.FromX;
			response.FromY = request.FromY;
			response.ToX = request.ToX;
			response.ToY = request.ToY;

			NetMessage responseNetMsg = new NetMessage();
			response.Encode(responseNetMsg); // encode the response into the netmessage

			// send to everyone
			Server.Broadcast(responseNetMsg, NetChannel.Unreliable);
		}
	}
}