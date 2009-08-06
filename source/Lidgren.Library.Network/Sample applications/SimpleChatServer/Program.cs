using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lidgren.Library.Network;
using System.Threading;

namespace SimpleChatServer
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

			// Create a configuration for the server
			// 'SimpleChat' is our application identifier, which distinguishes it from other
			// lidgren.library.network applications
			// 14242 is the port the server listens on; ie. the port clients should connect to
			Config = new NetAppConfiguration("SimpleChat", 14242);

			// set maximum number of clients to 128
			Config.MaximumConnections = 128;

			// set a server name (which shows up when client discovers local servers)
			Config.ServerName = "My server";

			// enable encryption; this key was generated using the 'GenerateEncryptionKeys' application
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

			// create a log
			Log = new NetLog();

			// ignore net events of type 'verbose'
			Log.IgnoreTypes = NetLogEntryTypes.None; //  Verbose; 

			// save log output to this file
			Log.OutputFileName = "serverlog.html";

			// catch log events
			Log.LogEvent += new EventHandler<NetLogEventArgs>(Log_LogEvent);

			// Now create the server!
			Server = new NetServer(Config, Log);

			// catch clients connecting
			Server.ConnectionRequest += new EventHandler<NetConnectRequestEventArgs>(Server_ConnectionRequest);

			// catch status changes; for example, when a client connects or disconnects
			Server.StatusChanged += new EventHandler<NetStatusEventArgs>(Server_StatusChanged);

			Application.Idle += new EventHandler(ApplicationLoop);
			Application.Run(MainForm);

			// shutdown; sends disconnect to all connected clients with this reason string
			Server.Shutdown("Application exiting");
		}

		static void Server_ConnectionRequest(object sender, NetConnectRequestEventArgs e)
		{
			// At this point we can approve or deny the incoming connection like this: 
			//
			// e.MayConnect = true;
			// e.DenialReason = "Sorry, no lamers!";
			//
			// ... possibly depending on the content of e.CustomData which the client
			// specifies when calling Connect()
		}

		static void ApplicationLoop(object sender, EventArgs e)
		{
			while (Win32.AppStillIdle)
			{
				// call heartbeat as often as possible; this sends queued packets,
				// keepalives and acknowledges etc.
				Server.Heartbeat();

				NetMessage msg;

				// read a packet if available
				while ((msg = Server.ReadMessage()) != null)
					HandleMessage(msg);

				Thread.Sleep(1); // don't hog the cpu
			}
		}

		private static void HandleMessage(NetMessage msg)
		{
			// we received a chat message!
			// pass it onto all clients
			string str = msg.ReadString();
			Log.Info("Broadcasting '" + str + "'");
			
			// crate new message to send to all clients
			NetMessage outMsg = new NetMessage();
			outMsg.Write(str);

			// use the ReliableUnordered channel; ie. it WILL arrive, but not necessarily in order
			Server.Broadcast(outMsg, NetChannel.ReliableUnordered);
		}

		static void Log_LogEvent(object sender, NetLogEventArgs e)
		{
			try
			{
				if (MainForm != null && !MainForm.Disposing && !MainForm.IsDisposed)
				{
					MainForm.richTextBox1.AppendText(e.Entry.What + Environment.NewLine);
					Win32.ScrollRichTextBox(MainForm.richTextBox1);
				}
			}
			catch { } // ignore disposal problems
		}

		static void Server_StatusChanged(object sender, NetStatusEventArgs e)
		{
			// display changes
			Log.Info(e.Connection + ": " + e.Connection.Status + " - " + e.Reason);
		}
	}
}