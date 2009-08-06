using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lidgren.Library.Network;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace TVReceiver
{
	static class Program
	{
		public static Form1 MainForm;
		public static NetClient Client;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new Form1();
			MainForm.pictureBox1.Image = new Bitmap(160, 120, PixelFormat.Format24bppRgb);

			NetAppConfiguration config = new NetAppConfiguration("TV", 14141);
			config.ReceiveBufferSize = 256000;
			NetConnectionConfiguration.DefaultOptimization = NetOptimization.EmphasizeBandwidth;
			NetLog log = new NetLog();
			Client = new NetClient(config, log);

			Application.Idle += new EventHandler(AppLoop);
			Application.Run(MainForm);
		}

		public static void AppLoop(object sender, EventArgs e)
		{
			while (Win32.AppStillIdle)
			{
				Client.Heartbeat();

				if (Client.Status != NetConnectionStatus.Disconnected)
				{
					NetMessage msg;
					while ((msg = Client.ReadMessage()) != null)
						HandleMessage(msg);
				}
			}
		}

		private static uint m_receivingFrame = 99999;
		private static byte[] m_receiveBuffer = new byte[16000];
		private static int m_receivePtr = 0;

		private static void HandleMessage(NetMessage msg)
		{
			uint framenr = msg.ReadUInt32();

			if (m_receivingFrame != framenr)
			{
				// new frame, display just received
				DecodeAndDisplayFrame();
				m_receivePtr = 0;
				m_receivingFrame = framenr;
			}

			// copy this piece into the buffer
			byte[] data = msg.ReadBytes(msg.Length - 4);
			Array.Copy(data, 0, m_receiveBuffer, m_receivePtr, data.Length);
			m_receivePtr += data.Length;
		}

		private static void DecodeAndDisplayFrame()
		{
			if (m_receivePtr == 0)
				return;

			try
			{
				// create image from received bytes
				MemoryStream ms = new MemoryStream(m_receiveBuffer, 0, m_receivePtr);
				Bitmap bm = (Bitmap)Bitmap.FromStream(ms);

				// display it
				MainForm.pictureBox1.Image = bm;
			}
			catch
			{
				// failed; might be incomplete first frame, or packet dropped creating
				// a bad frame. Ah well, hopefully next frame will work better
			}

			// give some time to other applications
			Thread.Sleep(0);
		}

		public static void Connect(string str)
		{
			Client.Connect(str, 14242);
		}
	}
}