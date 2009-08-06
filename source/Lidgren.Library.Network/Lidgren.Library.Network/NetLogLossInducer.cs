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
using System.Net;
using System.Net.Sockets;
using System.Globalization;

namespace Lidgren.Library.Network
{
	internal class NetLogLossInducer
	{
		internal class DelayedPacket
		{
			public double DelayAmount;
			public double DelayedUntil;
			public byte[] Data;
			public IPEndPoint RemoteEP;

			public override string ToString()
			{
				return "[DelayedPacket delayed " + DelayAmount + " until " + DelayedUntil + ", now is " + NetTime.Now + "]";
			}
		}

		private NetLog m_log;

		internal List<DelayedPacket> m_delayed;

		public NetLogLossInducer(NetLog log)
		{
			m_log = log;
			m_delayed = new List<DelayedPacket>();
		}

		private List<DelayedPacket> m_delaySent = new List<DelayedPacket>(5);
		public void SendDelayedPackets(NetBase netBase, double now)
		{
			foreach(DelayedPacket pk in m_delayed)
			{
				if (now >= pk.DelayedUntil)
				{
					// send it!
					m_delaySent.Add(pk);
					try
					{
						//m_log.Debug("(sending delayed packet after " + (int)(pk.DelayAmount * 1000.0f) + " ms)");
						netBase.m_socket.SendTo(pk.Data, 0, pk.Data.Length, SocketFlags.None, pk.RemoteEP);
					}
					catch (SocketException sex)
					{
						if (sex.SocketErrorCode == SocketError.ConnectionReset ||
							sex.SocketErrorCode == SocketError.ConnectionRefused ||
							sex.SocketErrorCode == SocketError.ConnectionAborted)
						{
							m_log.Warning("Remote socket forcefully closed: " + sex.SocketErrorCode);
							//if (connection != null)
							//	connection.Disconnect("Socket forcefully closed: " + sex.SocketErrorCode);
							continue;
						}

						m_log.Warning("Execute SocketException: " + sex.SocketErrorCode);
						continue;
					}
				}
			}

			if (m_delaySent.Count > 0)
			{
				foreach (DelayedPacket pk in m_delaySent)
					m_delayed.Remove(pk);
				m_delaySent.Clear();
			}
		}

		public int ExecuteSend(NetBase netBase, NetBuffer buffer, NetConnection connection, IPEndPoint remoteEP)
		{
			int len = buffer.LengthBytes;
#if DEBUG
			if (connection != null)
			{
				NetConnectionConfiguration config = connection.Configuration;

				if (config.LossChance > 0.0f && NetRandom.Default.Chance(config.LossChance))
				{
					//m_log.Debug("(simulating loss of sent packet)");
					return len;
				}
				
				if (config.LagDelayChance > 0.0f && NetRandom.Default.Chance(config.LagDelayChance))
				{
					float delayAmount = config.LagDelayMinimum + (NetRandom.Default.NextFloat() * config.LagDelayVariance);

					DelayedPacket pk = new DelayedPacket();
					pk.Data = new byte[len];
					Array.Copy(buffer.Data, pk.Data, buffer.LengthBytes);
					pk.DelayAmount = delayAmount;
					pk.DelayedUntil = NetTime.Now + delayAmount;
					pk.RemoteEP = remoteEP;
					m_delayed.Add(pk);

					//m_log.Debug("(queueing packet for " + (int)(pk.DelayAmount * 1000.0f) + " ms)");

					return len;
				}
			}
#endif

			try
			{
				int bytesSent = netBase.m_socket.SendTo(buffer.Data, 0, len, SocketFlags.None, remoteEP);
				m_log.Verbose(string.Format(CultureInfo.InvariantCulture, "Sent {0} bytes to {1}", bytesSent, remoteEP));

#if DEBUG
				if (connection != null)
				{
					NetConnectionConfiguration config = connection.Configuration;
					if (NetRandom.Default.Chance(config.DuplicatedPacketChance))
					{
						m_log.Debug("(simulating send packet duplication)");
						netBase.m_socket.SendTo(buffer.Data, 0, buffer.LengthBytes, SocketFlags.None, remoteEP);
					}
				}
#endif

				return bytesSent;
			}
			catch (SocketException sex)
			{
				if (sex.SocketErrorCode == SocketError.ConnectionReset ||
					sex.SocketErrorCode == SocketError.ConnectionRefused ||
					sex.SocketErrorCode == SocketError.ConnectionAborted)
				{
					m_log.Warning("Remote socket forcefully closed: " + sex.SocketErrorCode);
					if (connection != null)
						connection.Disconnect("Socket forcefully closed: " + sex.SocketErrorCode);
					return 0;
				}

				m_log.Warning("Execute SocketException: " + sex.SocketErrorCode);
				return 0;
			}
		}
	}
}
