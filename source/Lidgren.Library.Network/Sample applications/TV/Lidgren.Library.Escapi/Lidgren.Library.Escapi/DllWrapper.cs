using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Lidgren.Library.Escapi
{
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public unsafe struct SimpleCapParams
	{
		public int* Buffer;
		public int Width;
		public int Height;
	}

	internal unsafe static class DllWrapper
	{
		[DllImport("escapi.dll")]
		public static extern int countCaptureDevices();
		
		[DllImport("escapi.dll")]
		public static extern void getCaptureDeviceName(uint deviceId, byte[] buffer, int buflen);

		[DllImport("escapi.dll")]
		public static extern bool initCapture(uint deviceId, SimpleCapParams *info);
		
		[DllImport("escapi.dll")]
		public static extern void doCapture(uint deviceId);

		[DllImport("escapi.dll")]
		public static extern bool isCaptureDone(uint deviceId);

		[DllImport("escapi.dll")]
		public static extern void deinitCapture(uint deviceId);
	}
}
