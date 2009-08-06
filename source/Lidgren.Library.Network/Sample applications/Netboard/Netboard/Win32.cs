using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Netboard
{
	public static class Win32
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
		public const int WM_VSCROLL = 277; // Vertical scroll
		public const int SB_BOTTOM = 7; // Scroll to bottom 

		[StructLayout(LayoutKind.Sequential)]
		public struct PeekMsg
		{
			public IntPtr hWnd;
			public Message msg;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public System.Drawing.Point p;
		}

		[System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern bool PeekMessage(out PeekMsg msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

		public static bool AppStillIdle
		{
			get
			{
				PeekMsg msg;
				return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
			}
		}

		public static void ScrollRichTextBox(RichTextBox box)
		{
			SendMessage(box.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
		}


		[Flags]
		private enum SoundFlags : int
		{
			SND_SYNC = 0x0000,  // play synchronously (default) 
			SND_ASYNC = 0x0001,  // play asynchronously 
			SND_NODEFAULT = 0x0002,  // silence (!default) if sound not found 
			SND_MEMORY = 0x0004,  // pszSound points to a memory file
			SND_LOOP = 0x0008,  // loop the sound until next sndPlaySound 
			SND_NOSTOP = 0x0010,  // don't stop any currently playing sound 
			SND_NOWAIT = 0x00002000, // don't wait if the driver is busy 
			SND_ALIAS = 0x00010000, // name is a registry alias 
			SND_ALIAS_ID = 0x00110000, // alias is a predefined ID
			SND_FILENAME = 0x00020000, // name is file name 
			SND_RESOURCE = 0x00040004  // name is resource name or atom 
		}

		[System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		static extern bool PlaySound(string pszSound, IntPtr hMod, SoundFlags sf);

		public static void Play(string filename)
		{
			PlaySound(filename, IntPtr.Zero, SoundFlags.SND_ASYNC | SoundFlags.SND_FILENAME);
		}
	}

	public class MCISoundPlayer
	{
		[DllImport("winmm.dll")]
		private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

		private string Pcommand;
		private bool isOpen;

		public void Close()
		{
			Pcommand = "close MediaFile";
			mciSendString(Pcommand, null, 0, IntPtr.Zero);
			isOpen = false;
		}


		public void Open(string sFileName)
		{
			Pcommand = "open \"" + sFileName + "\" type mpegvideo alias MediaFile";
			mciSendString(Pcommand, null, 0, IntPtr.Zero);
			isOpen = true;
		}

		public void Play(bool loop)
		{
			if (isOpen)
			{
				Pcommand = "play MediaFile";
				if (loop)
					Pcommand += " REPEAT";
				mciSendString(Pcommand, null, 0, IntPtr.Zero);
			}
		}
	}

}
