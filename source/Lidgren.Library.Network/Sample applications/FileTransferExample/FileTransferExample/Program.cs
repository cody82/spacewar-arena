using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lidgren.Library.Network;
using System.IO;

namespace FileTransferExample
{
	static class Program
	{
		private static Form1 m_form;
		private static NetServer m_server;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			m_form = new Form1();

			NetAppConfiguration config = new NetAppConfiguration("filetransfer", 14242);
			config.MaximumConnections = 8;
			config.ServerName = "My server";
			config.ReceiveBufferSize = 500000; // half a megabyte to handle high bandwidth
			config.SendBufferSize = 128000;

			NetLog log = new NetLog();
			log.OutputFileName = "receiver-log.html";
			log.IgnoreTypes = NetLogEntryTypes.Verbose;
			log.LogEvent += new EventHandler<NetLogEventArgs>(OnLogEvent);
			m_server = new NetServer(config, log);

			Application.Idle += new EventHandler(ApplicationLoop);
			Application.Run(m_form);
		}

		public static void AddText(string txt)
		{
			try
			{
				m_form.richTextBox1.AppendText(txt + Environment.NewLine);
				Win32.ScrollRichTextBox(m_form.richTextBox1);
			}
			catch { } // ignore failure
		}

		static void OnLogEvent(object sender, NetLogEventArgs e)
		{
			AddText(e.Entry.What);
		}

		static void ApplicationLoop(object sender, EventArgs e)
		{
			while (Win32.AppStillIdle)
			{
				m_server.Heartbeat();

				NetMessage msg;
				while ((msg = m_server.ReadMessage()) != null)
					HandleMessage(msg);
			}
		}

		private static double m_startTimestamp;
		private static void HandleMessage(NetMessage msg)
		{
			NetConnection sender = msg.Sender;

			if (msg.Length == 0)
			{
				// starting file download
				m_startTimestamp = NetTime.Now;
				return;
			}

			double endTimestamp = NetTime.Now;
			double duration = endTimestamp - m_startTimestamp;

			// write it to file
			byte[] data = msg.ToArray();

			float bytesPerSecond = (float)((double)data.Length / duration);

			File.WriteAllBytes("received.data", data);

			AddText("File received; " + msg.Length + " bytes (" + bytesPerSecond + " bytes/second); saved to 'received.data'");
		}
	}
}