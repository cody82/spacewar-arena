//
// Wrapper for the excellent video capture library ESCAPI by Jari Komppa
// See http://sol.gfxile.net/code.html
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Lidgren.Library.Escapi
{
	public class VideoCapture
	{
		public static string[] GetDevices()
		{
			int num = DllWrapper.countCaptureDevices();
			if (num == 0)
				return new string[0];

			string[] retval = new string[num];
			for (int i = 0; i < num; i++)
			{
				try
				{
					byte[] arr = new byte[256];
					DllWrapper.getCaptureDeviceName(0, arr, arr.Length);
					string str = Encoding.ASCII.GetString(arr);

					if (string.IsNullOrEmpty(str))
						retval[i] = "(Unknown)";
					else
						retval[i] = str.Trim();
				}
				catch (Exception ex)
				{
					retval[i] = "(Exception)";
				}
			}
			return retval;
		}

		private uint m_deviceId;
		private int m_width, m_height;
		private Thread m_captureThread;
		private float m_fps;

		private bool m_isCapturing;
		public bool IsCapturing
		{
			get { return m_isCapturing; } 
		}

		public event EventHandler<EventArgs> ImageCaptured;

		public VideoCapture(uint deviceid, int width, int height, int fps, int numBuffers)
		{
			m_deviceId = deviceid;
			m_width = width;
			m_height = height;
			m_fps = (float)fps;

			m_bitmaps = new Bitmap[numBuffers];
			for(int i=0;i<numBuffers;i++)
				m_bitmaps[i] = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			m_currentBitmap = 0;

			m_bounds = new Rectangle(0, 0, width, height);
			m_rgbData = new byte[width * height * 3];
		}

		public void StartCapturing()
		{
			m_captureThread = new Thread(new ThreadStart(Run));
			m_captureThread.Start();
		}

		public void StopCapturing()
		{
			m_isCapturing = false;
		}

		~VideoCapture()
		{
			StopCapturing();
			Thread.Sleep(1);
		}

		private Bitmap[] m_bitmaps;
		private int m_currentBitmap;
		public Bitmap Bitmap
		{
			get { return m_bitmaps[m_currentBitmap]; }
		}

		private Rectangle m_bounds;
		private byte[] m_rgbData;

		private unsafe void Run()
		{
			// This runs in its own thread
			Stopwatch clock = new Stopwatch();
			try
			{
				int[] data = new int[m_width * m_height];
				fixed (int* ptr = data)
				{
					SimpleCapParams info = new SimpleCapParams();
					info.Width = m_width;
					info.Height = m_height;
					info.Buffer = ptr;

					bool ok = DllWrapper.initCapture(m_deviceId, &info);
					if (!ok)
						return; // exit

					// capture
					m_isCapturing = true;
					clock.Start();

					long millisPerCapture = (long)(1000.0f / m_fps);
					long usedMillis = 0;
					while (m_isCapturing)
					{
						DllWrapper.doCapture(m_deviceId);
						while (!DllWrapper.isCaptureDone(m_deviceId))
							Thread.Sleep(0);

						// done, handle data here
						int nextBitmap = m_currentBitmap + 1;
						if (nextBitmap >= m_bitmaps.Length)
							nextBitmap = 0;
						Bitmap useBitmap = m_bitmaps[nextBitmap];
						lock (useBitmap)
						{
							// copy data to bitmap
							int tries = 0;
							BitmapData bmpData = null;
							do
							{
								try
								{
									bmpData = useBitmap.LockBits(m_bounds, ImageLockMode.WriteOnly, useBitmap.PixelFormat);
								}
								catch (Exception ex)
								{
									bmpData = null;
									Thread.Sleep(1);
								}
							} while (bmpData == null && tries < 5);

							// Get the address of the first line.
							IntPtr bmDataPtr = bmpData.Scan0;

							// copy image to array
							int rgbPtr = 0;
							for (int i = 0; i < data.Length; i++)
							{
								m_rgbData[rgbPtr++] = (byte)(data[i] & 255);
								m_rgbData[rgbPtr++] = (byte)((data[i] >> 8) & 255);
								m_rgbData[rgbPtr++] = (byte)((data[i] >> 16) & 255);
							}

							// Copy the RGB values back to the bitmap
							Marshal.Copy(m_rgbData, 0, bmDataPtr, m_rgbData.Length);

							// Unlock the bits.
							useBitmap.UnlockBits(bmpData);

							// swap bitmaps
							m_currentBitmap = nextBitmap;
						}
						
						// Advertise a new image is available
						if (ImageCaptured != null)
							ImageCaptured(this, null);

						// pause until time to capture again
						usedMillis += millisPerCapture;
						while (usedMillis > clock.ElapsedMilliseconds)
							Thread.Sleep(2);
					}

					DllWrapper.deinitCapture(m_deviceId);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Ex: " + ex);
				m_isCapturing = false;
				try
				{
					DllWrapper.deinitCapture(m_deviceId);
				}
				catch { }
			}
		}
	}
}
