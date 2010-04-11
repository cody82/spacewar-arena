/* Copyright (c) 2010 Michael Lidgren

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

namespace Lidgren.Network
{
	public partial class NetConnection
	{
		//
		// Connection keepalive and latency calculation
		//
		internal float m_averageRoundtripTime = 0.05f;
		private byte m_lastSentPingNumber;
		private double m_lastPingSendTime;
		private double m_nextPing;
		private double m_nextKeepAlive;
		private double m_lastSendRespondedTo;

		// remote + diff = local
		// diff = local - remote
		// local - diff = remote
		internal double m_remoteToLocalNetTime = double.MinValue;

		public float AverageRoundtripTime { get { return m_averageRoundtripTime; } }

		internal void InitializeLatency(float rtt, float remoteNetTime)
		{
			m_averageRoundtripTime = Math.Max(0.005f, rtt - 0.005f); // TODO: find out why initial ping always overshoots
			m_nextPing = NetTime.Now + m_peerConfiguration.m_pingFrequency / 2.0f;

			double currentRemoteNetTime = remoteNetTime - (rtt * 0.5);
			m_remoteToLocalNetTime = (float)(NetTime.Now - currentRemoteNetTime);

			m_owner.LogDebug("Initialized average roundtrip time to: " + NetTime.ToReadable(m_averageRoundtripTime) + " remote time diff to " + NetTime.ToReadable(m_remoteToLocalNetTime));
		}

		internal void HandleIncomingPing(byte pingNumber)
		{
			// send pong
			NetOutgoingMessage pong = m_owner.CreateMessage(1);
			pong.m_type = NetMessageType.Library;
			pong.m_libType = NetMessageLibraryType.Pong;
			pong.Write((byte)pingNumber);
			pong.Write(NetTime.Now);

			// send immediately
			m_owner.SendImmediately(this, pong);
		}

		internal void HandleIncomingPong(double receiveNow, byte pingNumber, double remoteNetTime)
		{
			// verify it´s the correct ping number
			if (pingNumber != m_lastSentPingNumber)
			{
				m_owner.LogDebug("Received wrong pong number; got " + pingNumber + " expected " + m_lastSentPingNumber);
				return;
			}
			
			double now = NetTime.Now; // need exact number

			m_lastHeardFrom = now;
			m_lastSendRespondedTo = m_lastPingSendTime;

			m_nextKeepAlive = now + m_peerConfiguration.m_keepAliveDelay;

			double rtt = now - m_lastPingSendTime;

			// update clock sync
			double currentRemoteNetTime = remoteNetTime - (rtt * 0.5);
			double foundDiffRemoteToLocal = receiveNow - currentRemoteNetTime;

			if (m_remoteToLocalNetTime == double.MinValue)
				m_remoteToLocalNetTime = foundDiffRemoteToLocal;
			else
				m_remoteToLocalNetTime = ((m_remoteToLocalNetTime + foundDiffRemoteToLocal) * 0.5);

			m_averageRoundtripTime = (m_averageRoundtripTime * 0.7f) + (float)(rtt * 0.3f);
			m_owner.LogVerbose("Found rtt: " + NetTime.ToReadable(rtt) + ", new average: " + NetTime.ToReadable(m_averageRoundtripTime) + " new time diff: " + NetTime.ToReadable(m_remoteToLocalNetTime));
		}

		internal void KeepAliveHeartbeat(double now)
		{
			// do keepalive and latency pings
			if (m_status == NetConnectionStatus.Disconnected || m_status == NetConnectionStatus.None)
				return;

			if (now > m_nextKeepAlive || now > m_nextForceAckTime)
			{
				// send keepalive message
				m_owner.LogVerbose("Sending keepalive");

				NetOutgoingMessage keepalive = m_owner.CreateMessage(1);
				keepalive.m_type = NetMessageType.Library;
				keepalive.m_libType = NetMessageLibraryType.KeepAlive;
				EnqueueOutgoingMessage(keepalive);

				m_nextKeepAlive = now + m_peerConfiguration.KeepAliveDelay;
				m_nextForceAckTime = double.MaxValue;
			}

			// timeout
			if (now > m_lastSendRespondedTo + m_peerConfiguration.m_connectionTimeout)
				Disconnect("Timed out");

			// ping time?
			if (now > m_nextPing)
			{
				//
				// send ping
				//
				m_lastSentPingNumber++;

				NetOutgoingMessage ping = m_owner.CreateMessage(1);
				ping.m_type = NetMessageType.Library;
				ping.m_libType = NetMessageLibraryType.Ping;
				ping.Write((byte)m_lastSentPingNumber);

				m_owner.SendImmediately(this, ping);

				m_lastPingSendTime = NetTime.Now; // need exact number
				m_nextPing = now + m_owner.Configuration.m_pingFrequency;
			}
		}
	}
}
