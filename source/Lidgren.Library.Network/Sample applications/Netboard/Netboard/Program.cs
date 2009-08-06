using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

using Lidgren.Library.Network;

using NetboardCommon;

namespace Netboard
{
	static class Program
	{
		public static NetClient Client;
		public static Form1 MainForm;
		public static NetAppConfiguration Config;
		public static NetLog Log;

		private static Pen m_blackPen = new Pen(new SolidBrush(Color.Black));
		private static bool m_isMouseDown;
		private static Point m_drawFrom;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new Form1();

			Log = new NetLog();
			Log.IgnoreTypes = NetLogEntryTypes.Verbose;
			Log.LogEvent += new EventHandler<NetLogEventArgs>(Log_LogEvent);

			Config = new NetAppConfiguration("Netboard");

			// uncomment to enable encryption
			/*
			Config.EnableEncryption(
 "AQAB1nF0bzkPd+oG2lXk1lraVovWJHTt9+fuZKUZ4VoH6dmeO8GnuDDjWAk+KjeHh" +
 "u1Malaf1DJpdzehTTKXvh9JV47+GUSgyGLjeuASBQCcqi3RVTe0S0u6KhOkdclDjN" +
 "gr07oFlyR96UdTLhUtYUnp/E1xbzDgU//A9fdjAP/oBeU=", null);
			*/

			Client = new NetClient(Config, Log);

			Client.ServerDiscovered += new EventHandler<NetServerDiscoveredEventArgs>(Client_ServerDiscovered);
			Client.StatusChanged += new EventHandler<NetStatusEventArgs>(Client_StatusChanged);

			// hook mouse
			MainForm.pictureBox1.MouseDown += new MouseEventHandler(pictureBox1_MouseDown);
			MainForm.pictureBox1.MouseUp += new MouseEventHandler(pictureBox1_MouseUp);
			MainForm.pictureBox1.MouseMove += new MouseEventHandler(pictureBox1_MouseMove);
			MainForm.pictureBox1.MouseLeave += new EventHandler(pictureBox1_MouseLeave);

			Application.Idle += new EventHandler(ApplicationLoop);

			Application.Run(MainForm);

			// shutdown
			Client.Shutdown("Application exiting");
		}

		static void ApplicationLoop(object sender, EventArgs e)
		{
			while (Win32.AppStillIdle)
			{
				Client.Heartbeat();

				NetMessage msg;
				while ((msg = Client.ReadMessage()) != null)
					HandleMessage(msg);
			}
		}

		static void HandleMessage(NetMessage msg)
		{
			// determine what type of message it is
			// (in this case we're actually pretty sure it's a PaintEvent, but...)
			NetBoardTypes tp = (NetBoardTypes)msg.ReadByte();
			switch (tp)
			{
				case NetBoardTypes.PaintRequest:
					Log.Warning("Client received PaintRequest! Something must be wrong...");
					break;
				case NetBoardTypes.PaintEvent:
					// Paint event! Update screen
					UpdateScreen(msg);
					break;
				default:
					Log.Warning("Unknown message type!");
					break;
			}

			return;
		}

		static void UpdateScreen(NetMessage msg)
		{
			PaintEvent paintMsg = new PaintEvent();
			paintMsg.Decode(msg); // decode information from net message

			if (MainForm.pictureBox1.Image == null)
				MainForm.pictureBox1.Image = new Bitmap(MainForm.pictureBox1.ClientSize.Width, MainForm.pictureBox1.ClientSize.Height);

			Graphics g = Graphics.FromImage(MainForm.pictureBox1.Image);
			g.DrawLine(m_blackPen,
				new Point((int)paintMsg.FromX, (int)paintMsg.FromY),
				new Point((int)paintMsg.ToX, (int)paintMsg.ToY));
			MainForm.pictureBox1.Refresh();
		}

		static void Log_LogEvent(object sender, NetLogEventArgs e)
		{
			// Console.WriteLine(tp.ToString() + ": " + message);
		}

		static void Client_ServerDiscovered(object sender, NetServerDiscoveredEventArgs e)
		{
			// We found a server!

			// Are we already connected/connecting?
			if (Client.Status != NetConnectionStatus.Disconnected)
				return;

			// No, so let's connect!
			Client.Connect(e.ServerInformation.RemoteEndpoint.Address, e.ServerInformation.RemoteEndpoint.Port);
		}

		static void Client_StatusChanged(object sender, NetStatusEventArgs e)
		{
			MainForm.label1.Text = e.Connection.Status + " - " + e.Reason;
		}

		static void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			m_isMouseDown = true;
			m_drawFrom = e.Location;

			if (Client.Status == NetConnectionStatus.Disconnected)
			{
				// We're disconnected; lets discover servers!
				MainForm.label1.Text = "Discovering local servers...";
				Client.DiscoverLocalServers(14242);
				return;
			}
		}

		static void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_isMouseDown)
			{
				if (e.Location == m_drawFrom)
					return; // no change

				int x = e.X;
				int y = e.Y;

				// clamp
				if (x < 0)
					x = 0;
				if (y < 0)
					y = 0;
				if (x >= MainForm.pictureBox1.ClientSize.Width)
					x = MainForm.pictureBox1.ClientSize.Width - 1;
				if (y >= MainForm.pictureBox1.ClientSize.Height)
					y = MainForm.pictureBox1.ClientSize.Height - 1;

				if (Client.Status == NetConnectionStatus.Disconnected)
				{
					// We're disconnected; lets discover servers!
					MainForm.label1.Text = "Discovering local servers...";
					Client.DiscoverLocalServers(14242);
					return;
				}

				// we're connected (or at least connecting...)
				// send coords of click
				PaintRequest paintMsg = new PaintRequest();
				paintMsg.FromX = (uint)m_drawFrom.X;
				paintMsg.FromY = (uint)m_drawFrom.Y;
				paintMsg.ToX = (uint)x;
				paintMsg.ToY = (uint)y;

				NetMessage netMsg = new NetMessage();
				paintMsg.Encode(netMsg); // encode paint request into a netmessage for sending

				// Send to server
				Client.SendMessage(netMsg, NetChannel.ReliableUnordered);

				m_drawFrom.X = x;
				m_drawFrom.Y = y;
			}
		}

		static void pictureBox1_MouseLeave(object sender, EventArgs e)
		{
			m_isMouseDown = false;
		}

		static void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			m_isMouseDown = false;
		}
		
	}
}