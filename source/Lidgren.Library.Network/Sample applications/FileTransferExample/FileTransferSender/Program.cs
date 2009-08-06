using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lidgren.Library.Network;
using System.IO;

namespace FileTransferSender
{
	static class Program
	{
		private static Form1 m_form;
		private static NetClient m_client;
		private static int m_numResends;

		[STAThread]
		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			m_form = new Form1();

			// app identifier must match servers
			NetAppConfiguration config = new NetAppConfiguration("filetransfer");
			config.MaximumTransmissionUnit = 1350;
			config.DefaultNetMessageBufferSize = config.MaximumTransmissionUnit; // we'll be sending big messages
			config.ReceiveBufferSize = 128000;
			config.SendBufferSize = 500000; // half a megabyte to handle high bandwidth

			// some special configuration for file transfers
			NetConnectionConfiguration.DefaultOptimization = NetOptimization.EmphasizeBandwidth;
			NetConnectionConfiguration.DefaultThrottleBytesPerSecond = (int)CommonBandwidths.TwoMbps;

			NetLog log = new NetLog();
			//log.IgnoreTypes = NetLogEntryType.Verbose;
			log.OutputFileName = "sender-log.html";
			log.LogEvent += new EventHandler<NetLogEventArgs>(OnLogEvent);

			m_client = new NetClient(config, log);
			m_client.StatusChanged += new EventHandler<NetStatusEventArgs>(OnStatusChange);

			Application.Idle += new EventHandler(ApplicationLoop);
			Application.Run(m_form);
		}

		static void OnStatusChange(object sender, NetStatusEventArgs e)
		{
			if (e.Connection.Status == NetConnectionStatus.Connected)
			{
				e.Connection.MessageResent += new EventHandler<NetMessageEventArgs>(OnMessageResent);
				e.Connection.ConfigurationOptimized += new EventHandler<EventArgs>(OnConnectionOptimized);

				e.Connection.Configuration.SimulateLagLoss = true;
				e.Connection.Configuration.LagDelayMinimum = 0.05f;
				e.Connection.Configuration.LagDelayVariance = 0.1f;
				e.Connection.Configuration.LossChance = 0.0f;
			}
		}

		static void OnConnectionOptimized(object sender, EventArgs e)
		{
			// completely override optimization with our own settings
			// very long resend times; lets make sure its really a dropped packet
			NetConnection conn = sender as NetConnection;
			float rt = conn.AverageRoundtripTime;
			conn.Configuration.ResendFirstUnacknowledgedDelay = 0.5f + (rt * 4.0f);
			conn.Configuration.ResendSubsequentUnacknowledgedDelay = 1.0f + (rt * 8.0f);
		}

		static void OnMessageResent(object sender, EventArgs e)
		{
			m_numResends++;
		}

		private static int m_wasNumResends = 0;
		static void ApplicationLoop(object sender, EventArgs e)
		{
			while (Win32.AppStillIdle)
			{
				m_client.Heartbeat();

				NetMessage msg;
				while ((msg = m_client.ReadMessage()) != null)
					HandleMessage(msg);

				// update title showing number of resends
				if (m_numResends != m_wasNumResends)
				{
					m_form.Text = m_numResends + " resends";
					m_wasNumResends = m_numResends;
				}
			}
		}

		static void OnLogEvent(object sender, NetLogEventArgs e)
		{
			AddText(e.Entry.What);
		}

		public static void AddText(string txt)
		{
			try
			{
				if (m_form != null && !m_form.Disposing && !m_form.IsDisposed)
				{
					m_form.richTextBox1.AppendText(txt + Environment.NewLine);
					Win32.ScrollRichTextBox(m_form.richTextBox1);
				}
			}
			catch { } // ignore failure
		}

		public static void Connect()
		{
			m_client.Connect("localhost", 14242);
		}

		public static void SendFile()
		{
			m_numResends = 0;

			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Open file to send...";
			DialogResult res = dlg.ShowDialog();
			if (res != DialogResult.OK)
				return;

			AddText("Opening file " + dlg.FileName + "...");

			byte[] all = File.ReadAllBytes(dlg.FileName);

			AddText("Sending " + all.Length + " bytes; throttled to " + m_client.ServerConnection.Configuration.ThrottleBytesPerSecond + " bytes per second");

			// send zero-size message to be able to measure time taken
			NetMessage msg = new NetMessage(0);
			m_client.SendMessage(msg, NetChannel.ReliableUnordered);

			// send all data using fragmentation and throttling
			msg = new NetMessage(all.Length);
			msg.Write(all);
			m_client.SendMessage(msg, NetChannel.Ordered1);

			// done
			AddText("Done sending");
		}

		private static void HandleMessage(NetMessage msg)
		{
			// shouldn't happen... server isn't sending any application level messages.
		}
	}
}