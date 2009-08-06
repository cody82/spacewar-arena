using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lidgren.Library.Escapi;
using System.Drawing.Imaging;
using System.IO;
using Lidgren.Library.Network;
using System.Threading;

namespace TVStation
{
	static class Program
	{
		public static Form1 MainForm;
		public static VideoCapture Capture;
		public static NetServer Server;

		private static ImageCodecInfo m_useCodec;
		private static EncoderParameters m_codecParams;

		private static byte[] m_saveBuffer;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new Form1();

			// create a server
			NetAppConfiguration config = new NetAppConfiguration("TV", 14242);
			config.SendBufferSize = 256000;
			config.MaximumConnections = 8; // lets not strain ourselves :-)
			NetConnectionConfiguration.DefaultOptimization = NetOptimization.EmphasizeBandwidth;
			NetLog log = new NetLog();
			Server = new NetServer(config, log);

			// get jpeg codec and parameters
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
			foreach (ImageCodecInfo codec in codecs)
				if (codec.FormatDescription == "JPEG")
					m_useCodec = codec;
			m_codecParams = new EncoderParameters(1);
			m_codecParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)20); // low quality!

			// create save buffer
			m_saveBuffer = new byte[16000]; // should suffice

			// Create capture device; no error checking, might fail hard :-)
			Capture = new VideoCapture(0, 160, 120, 15, 15);
			Capture.ImageCaptured += new EventHandler<EventArgs>(OnImageCaptured);

			Application.Idle += new EventHandler(AppLoop);
			Application.Run(MainForm);

			Capture.StopCapturing();
			Server.Shutdown("Transmission ending");
		}

		static void AppLoop(object sender, EventArgs e)
		{
			while (Win32.AppStillIdle)
			{
				Server.Heartbeat();

				// read any message and drop them instantly; we are lidgren of borg
				Server.ReadMessage();

				// don't hog cpu
				Thread.Sleep(0);
			}
		}

		public static void StartCapturing()
		{
			Capture.StartCapturing();
		}

		public static void StopCapturing()
		{
			Capture.StopCapturing();
		}

		private static Random m_random = new Random();
		private static uint m_frameNumber;
		public static void OnImageCaptured(object sender, EventArgs e)
		{
			lock (m_saveBuffer)
			{
				// compress as jpeg to memorystream
				MemoryStream ms = new MemoryStream(m_saveBuffer);
				Capture.Bitmap.Save(ms, m_useCodec, m_codecParams);

				// ok; now send it 
				int len = (int)ms.Position;
				int maxPerPacket = Server.Configuration.MaximumTransmissionUnits - 16;
				int numPackets = (len / maxPerPacket) + 1;
				int left = len;
				for (int i = 0; i < numPackets; i++)
				{
					// create packet
					int plen = (left > maxPerPacket ? maxPerPacket : left);
					NetMessage msg = new NetMessage(4 + plen);
					msg.Write(m_frameNumber);
					msg.Write(m_saveBuffer, len - left, plen);

					//Console.WriteLine("Frame " + m_frameNumber + "; pkt " + i + "/" + numPackets + " - " + plen + " bytes");

					// broadcast it
					Server.Broadcast(msg, NetChannel.Ordered1);

					left -= plen;
				}

				// also display it
				MainForm.pictureBox1.Image = Capture.Bitmap;
				MainForm.pictureBox1.Invalidate();

				m_frameNumber++;
			}
		}

	}
}