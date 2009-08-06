/*
Copyright (c) 2007 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;

namespace Lidgren.Library.Network
{
	[Flags]
	public enum NetLogEntryTypes : int
	{
		None = 0,
		Verbose = 1 << 0,
		Debug = 1 << 1,
		Info = 1 << 2,
		Warning = 1 << 3,
		Error = 1 << 4,
		Break = 1 << 5,
		Html = 1 << 6,
	}
	
	/// <summary>
	/// An entry in the network log
	/// </summary>
	public sealed class NetLogEntry
	{
		public NetLogEntryTypes Type;
		public long TimeMillis;
		public string Where;
		public string What;
	}

	/// <summary>
	/// A class for capturing log feedback from the library
	/// </summary>
	public sealed class NetLog
	{
		private static Dictionary<NetLogEntryTypes, string> LogEntryTypeName;

		private static ConsoleColor[] LogEntryTypeColors = {
			ConsoleColor.DarkGray,
			ConsoleColor.DarkGray,
			ConsoleColor.Gray,
			ConsoleColor.White,
			ConsoleColor.Yellow,
			ConsoleColor.Red,
			ConsoleColor.Red,
			ConsoleColor.Blue,
			ConsoleColor.White,
		};

		private Queue<NetLogEntry> m_entries;
		private Thread m_mainThread;
		private string m_file, m_fileToUse;

		/// <summary>
		/// Specifies a filename to save log output to
		/// </summary>
		public string OutputFileName
		{
			get { return m_file; }
			set { m_fileToUse = value; }
		}

		public event EventHandler<NetLogEventArgs> LogEvent;

		public NetLogEntryTypes IgnoreTypes = NetLogEntryTypes.Verbose;

		public NetLog()
		{
			m_entries = new Queue<NetLogEntry>();
			m_mainThread = Thread.CurrentThread;
			LogEntryTypeName = new Dictionary<NetLogEntryTypes, string>();
			LogEntryTypeName[NetLogEntryTypes.None] = "nnn";
			LogEntryTypeName[NetLogEntryTypes.Verbose] = "vrb";
			LogEntryTypeName[NetLogEntryTypes.Debug] = "dbg";
			LogEntryTypeName[NetLogEntryTypes.Info] = "inf";
			LogEntryTypeName[NetLogEntryTypes.Warning] = "wrn";
			LogEntryTypeName[NetLogEntryTypes.Error] = "err";
			LogEntryTypeName[NetLogEntryTypes.Break] = "brk";
			LogEntryTypeName[NetLogEntryTypes.Html] = "htm";
		}

		private void MakeEntry(NetLogEntryTypes type, string str)
		{
			if ((type & IgnoreTypes) == type)
				return;

			if (type != NetLogEntryTypes.Break)
			{
				ConsoleColor wasColor = Console.ForegroundColor;
				// TODO:
				//Console.ForegroundColor = LogEntryTypeColors[(int)type];
				Console.WriteLine(str);
				Console.ForegroundColor = wasColor;
			}

			NetLogEntry entry = new NetLogEntry();
			entry.TimeMillis = (long)(NetTime.Now * 1000.0);
			entry.Type = type;
#if DEBUG
			// get call stack
			StackTrace trace = new StackTrace(2, true);
			StackFrame frame = trace.GetFrame(0);
			MethodBase method = frame.GetMethod();
			entry.Where = method.Name + "() in " + method.DeclaringType.Name;
#else
			entry.Where = "";
#endif
			entry.What = str;

			m_entries.Enqueue(entry);

			if (LogEvent != null)
			{
				NetLogEventArgs args = new NetLogEventArgs();
				args.Entry = entry;
				LogEvent(this, args);
			}

			if (Thread.CurrentThread == m_mainThread)
				Flush();
		}

		[Conditional("DEBUG")]
		public void Verbose(string str) { MakeEntry(NetLogEntryTypes.Verbose, str); }
		[Conditional("DEBUG")]
		public void Debug(string str) { MakeEntry(NetLogEntryTypes.Debug, str); }
		public void Info(string str) { MakeEntry(NetLogEntryTypes.Info, str); }
		public void Warning(string str) { MakeEntry(NetLogEntryTypes.Warning, str); }
		public void Error(string str) { MakeEntry(NetLogEntryTypes.Error, str); }
		public void Break(string str) { MakeEntry(NetLogEntryTypes.Break, str); }
		public void Html(string str) { MakeEntry(NetLogEntryTypes.Html, str); }

		public void Flush()
		{
			if (m_fileToUse != m_file)
			{
				m_fileToUse = Path.GetFullPath(m_fileToUse);
				m_file = m_fileToUse;
				StreamWriter writer = new StreamWriter(m_file);
				writer.Write(@"
<!DOCTYPE HTML PUBLIC \'-//W3C//DTD HTML 4.01 Transitional//EN\'>
<HTML>
	<HEAD>
		<TITLE>Log</TITLE>
		<STYLE>
TD {
	font-family: verdana;
	font-size: 10px;
}
BODY {
	font-family: verdana;
	font-size: 10px;
	line-height: 16px;
	vertical-align: top;
}
H1 {
	font-family: verdana;
	font-size: 22px;
	margin-left: 20px;
	font-weight: bold;
}
.inf { color: #000000; border-bottom: 1px solid #CCCCCC; vertical-align: top; }
.suc { color: #008800; border-bottom: 1px solid #CCCCCC; vertical-align: top; }
.fai { color: #AA2200; border-bottom: 1px solid #CCCCCC; vertical-align: top; }
.dbg { color: #008800; border-bottom: 1px solid #CCCCCC; vertical-align: top; }
.vrb { color: #999999; border-bottom: 1px solid #CCCCCC; vertical-align: top; }
.wrn { font-weight: bold; color: #DD8800; border-bottom: 1px solid #CCCCCC; vertical-align: top; }
.err { font-weight: bold; color: #FF0000; border-bottom: 1px solid #CCCCCC; vertical-align: top; }
.brk { color: #FF0000; border-bottom: 1px solid #000000; vertical-align: top; }
.lbl { padding: 14px 4px 2px 22px; font-family: arial; font-size: 14px; font-weight: bold; color: #000000; background-color: #EEEEEE; border-top: 1px solid #000000; border-bottom: 1px solid #000000; }

U { text-align: right; text-decoration: none; width: 75px; margin-right: 6px; vertical-align: top; padding-right: 2px; }
Q { text-decoration: none; width: 275px; margin-left: 8px; margin-right: 6px; vertical-align: top; }
		</STYLE>
	</HEAD>
	<BODY>
");
				try
				{
					writer.WriteLine("\t\t<DIV CLASS=\"inf\">{0}</DIV>", System.Environment.CommandLine);
					writer.WriteLine("\t\t<DIV CLASS=\"inf\">{0}\\{1} @ {2} ({3})</DIV>", System.Environment.UserDomainName, System.Environment.UserName, System.Environment.MachineName, System.Environment.OSVersion);
					writer.WriteLine("\t\t<DIV CLASS=\"inf\">.NET Framework version: {0}</DIV>", System.Environment.Version);
					writer.WriteLine("\t\t<DIV CLASS=\"inf\">Log started {0}</DIV>", DateTime.Now);
					writer.WriteLine("\t\t<DIV CLASS=\"brk\"></DIV>");
				}
				finally { }

				writer.Close();
			}

			while (m_entries.Count > 0)
			{
				NetLogEntry entry = m_entries.Dequeue();
				if (m_file != null)
				{

					StringBuilder htmlBuilder = new StringBuilder(63);

					htmlBuilder.AppendFormat("<DIV CLASS=\"{0}\">{1}", LogEntryTypeName[entry.Type], System.Environment.NewLine);
					htmlBuilder.AppendFormat("<U>{0}</U>", entry.TimeMillis);

					if (entry.Where == null)
					{
						htmlBuilder.AppendLine("<Q>-</Q>");
					}
					else
					{
						try
						{
							htmlBuilder.AppendFormat("<Q>{0}</Q>", entry.Where);
						}
						catch (Exception ex)
						{
							htmlBuilder.AppendFormat("<Q>{0}</Q>", ex.ToString());
						}
					}

					htmlBuilder.Append(entry.What);

					htmlBuilder.AppendLine("</DIV>");

					File.AppendAllText(m_file, htmlBuilder.ToString(), Encoding.ASCII);
				}
			}
		}
	}

	public class NetLogEventArgs : System.EventArgs
	{
		public NetLogEntry Entry;

		public override string ToString()
		{
			return Entry.What;
		}
	
	}
}
