using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lidgren.Library.Escapi;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace TestApp
{
	static class Program
	{
		public static Form1 MainForm;
		public static VideoCapture Capture;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new Form1();

			Capture = new VideoCapture(0, 320, 240, 15, 15);
			Capture.ImageCaptured += new EventHandler<EventArgs>(capture_ImageCaptured);

			Application.Run(MainForm);

			Capture.StopCapturing();
		}

		internal static void Toggle()
		{
			if (Capture.IsCapturing)
				Capture.StopCapturing();
			else
				Capture.StartCapturing();
		}
		
		private static int framenr = 0;
		private static object updating = new object();

		static void capture_ImageCaptured(object sender, EventArgs e)
		{
			lock (updating)
			{
				MainForm.pictureBox1.Image = Capture.Bitmap;
				MainForm.pictureBox1.Invalidate();
				framenr++;
			}
		}
	}
}