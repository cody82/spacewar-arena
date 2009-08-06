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
	/// <summary>
	/// A network message
	/// </summary>
	public sealed partial class NetMessage
	{
		private NetChannel m_sequenceChannel;
		private ushort m_sequenceNumber;

		internal NetBuffer m_buffer;
		internal NetMessageType m_type;

		internal float m_nextResendTime;
		internal int m_numResends;

		/// <summary>
		/// For received messages; this holds from which connection the message was sent
		/// </summary>
		public NetConnection Sender;

		/// <summary>
		/// Create a net message
		/// </summary>
		public NetMessage()
		{
			m_type = NetMessageType.User;
			System.Diagnostics.Debug.Assert(NetBase.CurrentContext != null, "No network context; create a NetClient or NetServer first");
			m_buffer = new NetBuffer(NetBase.CurrentContext.Configuration.DefaultNetMessageBufferSize);
		}

		internal NetMessage(bool setBufferLater)
		{
			// set m_buffer at later stage
		}

		internal NetMessage(NetMessageType tp, int initialCapacity)
		{
			m_type = tp;
			m_buffer = new NetBuffer(initialCapacity);
		}

		internal NetMessage(NetMessageType tp, byte[] payload)
		{
			m_type = tp;
			m_buffer = new NetBuffer(payload);
		}

		/// <summary>
		/// Create a message with specified initial capacity in bytes
		/// </summary>
		public NetMessage(int initialCapacity)
		{
			m_type = NetMessageType.User;
			m_buffer = new NetBuffer(initialCapacity);
		}

		internal NetMessage(NetBuffer reuseBuffer)
		{
			m_buffer = reuseBuffer;
		}

		/// <summary>
		/// NetChannel the message was sent over
		/// </summary>
		public NetChannel SequenceChannel
		{
			get { return m_sequenceChannel; }
			set { m_sequenceChannel = value; }
		}

		/// <summary>
		/// Sequence number (within the channel) of the message
		/// </summary>
		public ushort SequenceNumber
		{
			get { return m_sequenceNumber; }
			internal set
			{
				if (value < 0 || value > NetConstants.NumSequenceNumbers)
					throw new InvalidOperationException("SequenceNumber must be between 0 and " + (NetConstants.NumSequenceNumbers - 1));
				m_sequenceNumber = value;
			}
		}

		internal void Encode(NetConnection connection, NetBuffer intoBuffer)
		{
			if (m_type == NetMessageType.None)
				m_type = NetMessageType.User;
			intoBuffer.Write((byte)m_type, 3);
			switch(m_type)
			{
				case NetMessageType.UserFragmented:
				case NetMessageType.User:
				case NetMessageType.Acknowledge:
				case NetMessageType.AcknowledgeBitField:
					intoBuffer.Write((byte)m_sequenceChannel, 5); // encode channel
					intoBuffer.Write(m_sequenceNumber, 12); // encode sequence number
					break;
			}

			int byteLen = (m_buffer == null ? 0 : m_buffer.LengthBytes);

			if (m_type == NetMessageType.UserFragmented || m_type == NetMessageType.User || m_type == NetMessageType.Discovery || m_type == NetMessageType.Handshake)
				intoBuffer.Write(byteLen, 12); // encode length

			// encode payload
			if (byteLen > 0)
				intoBuffer.Write(m_buffer.Data, 0, byteLen);

			return;
		}

		internal int EstimateEncodedLength()
		{
			return 4 + (m_buffer == null ? 0 : m_buffer.LengthBytes);
		}

		internal static NetMessage Decode(NetBuffer buffer)
		{
			try
			{
				NetMessage retval = new NetMessage(true);

				NetMessageType type = (NetMessageType)buffer.ReadByte(3);
				retval.m_type = type;

				switch (type)
				{
					case NetMessageType.None:
						// packet padding (due to encryption); we've reached the end
						return null;
					case NetMessageType.User:
					case NetMessageType.UserFragmented:
					case NetMessageType.Acknowledge:
					case NetMessageType.AcknowledgeBitField:
						retval.m_sequenceChannel = (NetChannel)buffer.ReadByte(5);
						retval.m_sequenceNumber = (ushort)buffer.ReadUInt32(12);
						break;
				}

				int msgLen = 0;
				switch (type)
				{
					case NetMessageType.User:
					case NetMessageType.UserFragmented:
						msgLen = (int)buffer.ReadUInt32(12);
						break;
					case NetMessageType.Acknowledge:
						msgLen = 0;
						break;
					case NetMessageType.AcknowledgeBitField:
						msgLen = 4;
						break;
					case NetMessageType.Discovery:
						msgLen = (int)buffer.ReadUInt32(12);
						break;
					case NetMessageType.Handshake:
						msgLen = (int)buffer.ReadUInt32(12);
						break;
					case NetMessageType.PingPong:
						msgLen = 3;
						break;
				}

				byte[] payload = null;
				if (msgLen > 0)
					payload = buffer.ReadBytes(msgLen);
				retval.m_buffer = new NetBuffer(payload);

				return retval;
			}
			catch
			{
				NetBase.CurrentContext.Log.Warning("Failed to decode NetMessage from buffer!");
				return null;
			}
		}

		// for sending
		internal static NetMessage CreateReferenceClone(NetMessage msg)
		{
			NetMessage retval = new NetMessage(msg.m_buffer);
			retval.m_sequenceChannel = msg.m_sequenceChannel;
			retval.m_sequenceNumber = msg.m_sequenceNumber;
			retval.m_type = msg.m_type;
			return retval;
		}

		public override string ToString()
		{
			return "[NetMessage " + m_type + " " + m_sequenceChannel + "|" + m_sequenceNumber + " " + (m_buffer == null ? 0 : m_buffer.LengthBytes) + " bytes]";
		}
	}
}
