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
using System.Net.NetworkInformation;

namespace Lidgren.Library.Network
{
	/// <summary>
	/// Statistics for a connection
	/// </summary>
	public class NetStatistics
	{
		private NetConnection m_forConnection;

		public double Start, End;

		public int MessagesSent;
		public int UserMessagesSent;
		public int AckMessagesSent;
		public int MessagesReceived;
		public int UserMessagesReceived;
		public int AckMessagesReceived;

		public int PacketsSent;
		public int PacketsReceived;

		public int BytesSent;
		public int BytesReceived;

		private long m_sysReceived;
		private long m_sysSent;

		public int MessagesResent;

		public NetStatistics(NetConnection forConnection)
		{
			m_forConnection = forConnection;
			Reset();
		}

		/// <summary>
		/// Reset statistics counters
		/// </summary>
		public void Reset()
		{
			Start = NetTime.Now;

			MessagesSent = 0;
			UserMessagesSent = 0;
			AckMessagesSent = 0;
			MessagesReceived = 0;
			UserMessagesReceived = 0;
			AckMessagesReceived = 0;

			PacketsSent = 0;
			PacketsReceived = 0;

			BytesSent = 0;
			BytesReceived = 0;

			MessagesResent = 0;

			IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();
			UdpStatistics udpStats = ipProps.GetUdpIPv4Statistics();
			m_sysReceived = udpStats.DatagramsReceived;
			m_sysSent = udpStats.DatagramsSent;
		}

		/// <summary>
		/// Dump all statistics to log
		/// </summary>
		public void DumpToLog(NetLog log)
		{
			if (log == null)
				return;

			double timespan;
			if (End == 0.0f)
				timespan = NetTime.Now - Start;
			else
				timespan = End - Start;

			log.Debug("--- Statistics for " + m_forConnection + " @ " + NetTime.Now + "--------------");
			log.Debug("Timespan: " + timespan + " seconds");
			log.Debug("Sent " + PacketsSent + " packets (" + FPS(PacketsSent, timespan) + " per second)");
			log.Debug("Sent " + BytesSent + " bytes (" + FPS(BytesSent, timespan) + " per second, " + FPS(BytesSent, (double)PacketsSent) + " per packet, " + FPS(BytesSent, (double)MessagesSent) + " per message)");
			log.Debug("Sent " + MessagesSent + " messages (" + FPS(MessagesSent, timespan) + " per second, " + FPS(MessagesSent, (double)PacketsSent) + " per packet)");

			int libMessagesSent = MessagesSent - (UserMessagesSent + AckMessagesSent);
			log.Debug(" ... of which " + UserMessagesSent + " were user messages (" + FPS(UserMessagesSent, timespan) + " per second)");
			log.Debug(" ... of which " + AckMessagesSent + " were acknowledge messages (" + FPS(AckMessagesSent, timespan) + " per second)");
			log.Debug(" ... of which " + libMessagesSent + " were other system messages (" + FPS(libMessagesSent, timespan) + " per second)");

			log.Debug("Received " + PacketsReceived + " packets (" + FPS(PacketsReceived, timespan) + " per second)");
			log.Debug("Received " + BytesReceived + " bytes (" + FPS(BytesReceived, timespan) + " per second, " + FPS(BytesReceived, (double)PacketsReceived) + " per packet, " + FPS(BytesReceived, (double)MessagesReceived) + " per message)");
			log.Debug("Received " + MessagesReceived + " messages (" + FPS(MessagesReceived, timespan) + " per second, " + FPS(MessagesReceived, (double)PacketsReceived) + " per packet)");

			int libMessagesReceived = MessagesReceived - (UserMessagesReceived + AckMessagesReceived);
			log.Debug(" ... of which " + UserMessagesReceived + " were user messages (" + FPS(UserMessagesReceived, timespan) + " per second)");
			log.Debug(" ... of which " + AckMessagesReceived + " were acknowledge messages (" + FPS(AckMessagesReceived, timespan) + " per second)");
			log.Debug(" ... of which " + libMessagesReceived + " were other system messages (" + FPS(libMessagesReceived, timespan) + " per second)");
			log.Debug("Resent " + MessagesResent + " messages (" + FPS(MessagesResent, timespan) + " per second)");

			IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();
			// log.Debug("Domain name: " + ipProps.DomainName);

			UdpStatistics udpStats = ipProps.GetUdpIPv4Statistics();

			log.Debug("System wide datagrams sent: " + (udpStats.DatagramsSent - m_sysSent));
			log.Debug("System wide datagrams received: " + (udpStats.DatagramsReceived - m_sysReceived));
		}

		private static string FPS(int num, double divisor)
		{
			double fps = num / divisor;
			return fps.ToString("N1", System.Globalization.NumberFormatInfo.InvariantInfo);
		}
	}
}
