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

namespace Lidgren.Library.Network
{
	internal sealed class NetPing
	{
		private NetConnection m_connection;
		private float m_averageRoundtrip;
		private float[] m_samples;
		private int m_samplePtr;

		private byte m_pingNrInProgress;

		private double m_lastSentPing;

		public float AverageRoundtrip { get { return m_averageRoundtrip; } }

		public NetPing(NetConnection forConnection)
		{
			m_connection = forConnection;
			m_samples = new float[6];
			Initialize(0.05f); // 50 ms

			m_lastSentPing = NetTime.Now;
		}

		internal void Initialize(float seconds)
		{
			NetBase.CurrentContext.Log.Verbose("Initializing average ping: " + (int)(seconds * 1000.0) + " ms");
			for (int i = 0; i < m_samples.Length; i++)
				m_samples[i] = seconds;
			m_samplePtr = 0;
			m_averageRoundtrip = seconds;
			m_connection.Configuration.OptimizeSettings(seconds, true);

			if (m_connection.Parent is NetServer)
				SendOptimizeInfo(m_averageRoundtrip);
		}
		
		public void AddRoundtrip(float seconds)
		{
			m_samples[m_samplePtr++] = seconds;
			if (m_samplePtr >= m_samples.Length)
				m_samplePtr = 0;

			// drop highest value, average rest
			int high = 0;
			for (int i = 1; i < m_samples.Length; i++)
				if (m_samples[i] > m_samples[high])
					high = i;

			m_averageRoundtrip = 0.0f;
			for (int i = 0; i < m_samples.Length; i++)
				if (i != high)
					m_averageRoundtrip += m_samples[i];
			m_averageRoundtrip *= 0.2f;

			//m_connection.Parent.Log.Debug("Average roundtrip time: " + (int)(m_averageRoundtrip * 1000.0f) + " ms");
		}

		public void Heartbeat(double now)
		{
			// time to send?
			if (now - m_lastSentPing > m_connection.Configuration.PingFrequency)
				SendPing(now);
		}

		internal void SendPing(double now)
		{
			m_pingNrInProgress++;
			if (m_pingNrInProgress > 63)
				m_pingNrInProgress = 0;
			NetMessage ping = new NetMessage(NetMessageType.PingPong, 1);
			ping.Write(false); // means ping
			ping.Write(false); // means NOT optimize info
			ping.Write((byte)m_pingNrInProgress, 6);

			m_connection.Parent.SendSingleMessageAtOnce(ping, m_connection, m_connection.RemoteEndpoint);
			m_lastSentPing = now;
		}

		internal void SendOptimizeInfo(float roundtrip)
		{
			NetMessage ping = new NetMessage(NetMessageType.PingPong, 3);
			ping.Write(false); // meaningless
			ping.Write(true); // means optimize info
			ping.Write7BitEncodedUInt((uint)(roundtrip * 1000.0f));
			m_connection.SendMessage(ping, NetChannel.Unreliable);
		}

		internal static void ReplyPong(NetMessage pingMessage, NetConnection connection)
		{
			byte nr = pingMessage.ReadByte(7);

			NetMessage pong = new NetMessage(NetMessageType.PingPong, 3);
			pong.Write(true); // means pong
			pong.Write(false); // means NOT optimize info
			pong.Write(nr, 6);
			pong.WriteSendStamp();
			connection.Parent.SendSingleMessageAtOnce(pong, connection, connection.RemoteEndpoint);
		}

		internal void HandleOptimizeInfo(double now, NetMessage pongMessage)
		{
			float optimizeMillis = (float)pongMessage.Read7BitEncodedUInt();
			m_connection.Configuration.OptimizeSettings(optimizeMillis / 1000.0f);
		}

		internal void HandlePong(double now, NetMessage pongMessage)
		{
			int nr = pongMessage.ReadInt(6);
			if (nr != m_pingNrInProgress)
			{
				m_connection.Parent.Log.Warning("Pong received for wrong ping");
				return;
			}
			double roundtrip = now - m_lastSentPing;

			// spike, or can we use this roundtrip?
			double avgRT = pongMessage.Sender.AverageRoundtripTime;
			if (roundtrip > avgRT * 2.0)
			{
				// must be spike
				m_connection.Parent.Log.Verbose("Not adjusting clock due to lag spike... (rt " + NetUtil.SecToMil(roundtrip) + " avg " + NetUtil.SecToMil(avgRT) + ")");
			}
			else
			{
				ushort val = pongMessage.ReadUInt16();
				int curOffset = m_connection.RemoteClockOffset;
				int foundOffset = NetTime.CalculateOffset(now, val, roundtrip);

				int res = NetTime.MergeOffsets(curOffset, foundOffset);

				int rtMillis = (int)(roundtrip * 1000.0);
				if (res != curOffset)
				{
					//m_connection.Parent.Log.Verbose("Ping: " + rtMillis + " Local: " + NetTime.Encoded(now) + " CurOffset: " + curOffset + " Remote: " + val + " NewOffset: " + res + " (adjusted " + (res - curOffset) + ")");
					m_connection.Parent.Log.Verbose("Roundtrip: " + rtMillis + " ms; Ajusted remote offset " + (res - curOffset) + " ms");
					m_connection.RemoteClockOffset = res;
				}
			}

			AddRoundtrip((float)roundtrip);

			if (m_connection.Parent is NetServer)
			{
				// server is authorative on settings optimizing roundtrip numbers
				m_connection.Configuration.OptimizeSettings(m_averageRoundtrip);
				SendOptimizeInfo(m_averageRoundtrip);
			}

			return;
		}

	}
}
