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
using System.Net;
using System.Net.Sockets;
using System.Globalization;

namespace Lidgren.Library.Network
{
	/// <summary>
	/// A network client
	/// </summary>
	/// <example><code>
	/// NetConfiguration config = new NetConfiguration("MyApp", 12345);
	/// NetLog log = new NetLog();
	///
	/// NetClient myNet = new NetClient(config, log);
	///
	/// myNet.Connect("localhost", 12345);
	/// </code></example>
	public class NetClient : NetBase
	{
		private NetConnection m_serverConnection;

		/// <summary>
		/// Event fired when server is found after using DiscoverLocal() to discover servers
		/// </summary>
		public event EventHandler<NetServerDiscoveredEventArgs> ServerDiscovered;

		/// <summary>
		/// Status of the server connection
		/// </summary>
		public NetConnectionStatus Status { get { return (m_serverConnection == null ? NetConnectionStatus.Disconnected : m_serverConnection.Status); } }

		/// <summary>
		/// The connection to the server; will be null until a call to Connect() has been made.
		/// </summary>
		public NetConnection ServerConnection { get { return m_serverConnection; } }

		public NetClient(NetAppConfiguration config, NetLog log)
		{
			NetBase.CurrentContext = this;
			InitBase(config, log);
		}

		/// <summary>
		/// Reads messages from network and sends unsent messages, resends etc
		/// This method should be called as often as possible
		/// </summary>
		public void Heartbeat()
		{
			double now = NetTime.Now;
			base.Heartbeat(now);

			NetBase.CurrentContext = this;

			// read all packets
			while (ReadPacket()) ;

			if (m_serverConnection == null)
				return;

			if (m_serverConnection.Status != NetConnectionStatus.Disconnected)
				m_serverConnection.Heartbeat(now);
		}

		internal override void HandlePacket(NetBuffer buffer, int bytesReceived, IPEndPoint sender)
		{
			double now = NetTime.Now;

			try
			{
				NetMessage msg;
				if (m_serverConnection == null || m_serverConnection.Status == NetConnectionStatus.Disconnected)
				{
					//
					// unconnected packet
					//
					msg = NetMessage.Decode(buffer);
					if (msg == null)
					{
						Log.Warning("Malformed NetMessage?");
						return; // malformed?
					}

					Log.Verbose("Received unconnected message: " + msg);

					// discovery response?
					if (msg.m_type == NetMessageType.Discovery)
					{
						NetServerInfo info = NetDiscovery.DecodeResponse(msg, sender);
						if (ServerDiscovered != null)
							ServerDiscovered(this, new NetServerDiscoveredEventArgs(info));
						return;
					}

					if (m_serverConnection != null && sender.Equals(m_serverConnection.RemoteEndpoint))
					{
						// we have m_serverConnection, but status is disconnected
						Log.Warning("Received " + buffer.LengthBytes + " from server; but we're disconnected!");
						return;
					}

					Log.Warning("Received " + buffer.LengthBytes + " bytes from non-server source: " + sender + "(server is " + m_serverConnection.RemoteEndpoint + ")");
					return;
				}

				// decrypt in place
				if (m_serverConnection.m_encryption.SymmetricEncryptionKeyBytes != null)
				{
					byte[] savedBuffer = null;
					if (m_serverConnection.Status == NetConnectionStatus.Connecting)
					{
						// special case; save buffer in case we get unencrypted response
						savedBuffer = new byte[buffer.LengthBytes];
						Array.Copy(buffer.Data, 0, savedBuffer, 0, buffer.LengthBytes);
					}

					bool ok = m_serverConnection.m_encryption.DecryptSymmetric(Log, buffer);
					if (!ok) 
					{
						// failed to decrypt; drop this packet UNLESS we're in a connecting state,
						// in which case the server may want to tell us something
						if (m_serverConnection.Status == NetConnectionStatus.Connecting)
						{
							// ok let this one thru unencrypted
							Array.Copy(savedBuffer, buffer.Data, savedBuffer.Length);
						}
						else
						{
							Log.Warning("Failed to decrypt packet from server!");
							return;
						}
					}
				}

				m_serverConnection.m_lastHeardFromRemote = now;

				int messagesReceived = 0;
				int usrMessagesReceived = 0;
				int ackMessagesReceived = 0;
				while (buffer.ReadBitsLeft > 7)
				{
					msg = NetMessage.Decode(buffer);

					if (msg == null)
						break; // done
					messagesReceived++;
					msg.Sender = m_serverConnection;
					switch (msg.m_type)
					{
						case NetMessageType.Handshake:
							NetHandshakeType tp = (NetHandshakeType)msg.ReadByte();
							if (tp == NetHandshakeType.Connect || tp == NetHandshakeType.ConnectionEstablished)
							{
								Log.Warning("Client received " + tp + "?!");
							}
							else if (tp == NetHandshakeType.ConnectResponse)
							{
								if (m_serverConnection.Status != NetConnectionStatus.Connecting)
								{
									Log.Verbose("Received redundant ConnectResponse!");
									break;
								}
								// Log.Debug("ConnectResponse received");
								m_serverConnection.SetStatus(NetConnectionStatus.Connected, "Connected");
								Log.Info("Connected to " + m_serverConnection.RemoteEndpoint);

								// initialize ping to now - m_firstSentConnect
								float initRoundtrip = (float)(now - m_serverConnection.m_firstSentHandshake);
								m_serverConnection.m_ping.Initialize(initRoundtrip);

								ushort remoteValue = msg.ReadUInt16();
								m_serverConnection.RemoteClockOffset = NetTime.CalculateOffset(now, remoteValue, initRoundtrip);
								Log.Verbose("Initializing remote clock offset to " + m_serverConnection.RemoteClockOffset + " ms (roundtrip " + NetUtil.SecToMil(initRoundtrip) + " ms)");

								NetMessage established = new NetMessage(NetMessageType.Handshake, 3);
								established.Write((byte)NetHandshakeType.ConnectionEstablished);
								established.WriteSendStamp();
								SendSingleMessageAtOnce(established, m_serverConnection, m_serverConnection.RemoteEndpoint);
							}
							else
							{ // Disconnected
								string reason = msg.ReadString();
								m_serverConnection.SetStatus(NetConnectionStatus.Disconnected, reason);
							}
							break;
						case NetMessageType.Acknowledge:
							//Log.Debug("Received ack " + msg.SequenceChannel + "|" + msg.SequenceNumber);
							m_serverConnection.ReceiveAcknowledge(msg);
							ackMessagesReceived++;
							break;

						case NetMessageType.PingPong:
							bool isPong = msg.ReadBoolean();
							bool isOptimizeInfo = msg.ReadBoolean();
							if (isOptimizeInfo)
							{
								m_serverConnection.m_ping.HandleOptimizeInfo(now, msg);
							} else if (isPong)
							{
								if (m_serverConnection.Status == NetConnectionStatus.Connected)
									m_serverConnection.m_ping.HandlePong(now, msg);
							}
							else
							{
								NetPing.ReplyPong(msg, m_serverConnection);
							}
							break;
						case NetMessageType.User:
						case NetMessageType.UserFragmented:
							//Log.Debug("User message received; " + msg.m_buffer.LengthBytes + " bytes");
							m_serverConnection.ReceiveMessage(now, msg);
							usrMessagesReceived++;
							break;
						case NetMessageType.Discovery:
							NetServerInfo info = NetDiscovery.DecodeResponse(msg, sender);
							if (ServerDiscovered != null)
								ServerDiscovered(this, new NetServerDiscoveredEventArgs(info));
							break;
						default:
							Log.Warning("Client received " + msg.m_type + "?!");
							break;
					}
				}

				// add statistics
				NetStatistics stats = m_serverConnection.Statistics;
				stats.PacketsReceived++;
				stats.MessagesReceived += messagesReceived;
				stats.UserMessagesReceived += usrMessagesReceived;
				stats.AckMessagesReceived += ackMessagesReceived;
				stats.BytesReceived += bytesReceived;
			}
			catch (Exception ex)
			{
				Log.Error("Failed to parse packet correctly; read/write mismatch? " + ex);
			}
		}

		/// <summary>
		/// Reads a message, if available
		/// </summary>
		/// <returns>NetMessage, or null if none are available</returns>
		/// <example><code>
		/// bool keepGoing = true;
		/// while (keepGoing)
		/// {
		///		myClient.Heartbeat();
 		/// 
		///		NetMessage msg;
		///		while ((msg = myNet.ReadMessage()) != null)
		///		{
		///			// handle msg
		///		}
		/// }
		/// </code></example>
		public NetMessage ReadMessage()
		{
			if (m_serverConnection == null)
				return null;

			NetBase.CurrentContext = this;

			if (m_serverConnection.m_receivedMessages.Count < 1)
				return null;

			NetMessage msg = m_serverConnection.m_receivedMessages.Dequeue();
			return msg;
		}

		internal override void HandleConnectionReset(IPEndPoint remote)
		{
			if (m_serverConnection == null)
				return;

			if (m_serverConnection.Status == NetConnectionStatus.Connecting)
			{
				Log.Info("Failed to connect");
				m_serverConnection.SetStatus(NetConnectionStatus.Disconnected, "No server listening!");
			}
			else
			{
				m_serverConnection.SetStatus(NetConnectionStatus.Disconnected, "Connection reset");
			}
		}
		
		/// <summary>
		/// Disconnects from the server, providing specified reason
		/// </summary>
		public void Disconnect(string reason)
		{
			if (m_serverConnection != null)
			{
				if (m_serverConnection.Status == NetConnectionStatus.Disconnected)
				{
					Log.Warning("Disconnect() called altho server connection is disconnected");
				}
				else
				{
					m_serverConnection.Disconnect(reason);
				}
			}
			else
			{
				Log.Warning("Disconnect() called altho no server connection");
			}
		}

		/// <summary>
		/// Send server discovery request to a remote host, not necessarily on
		/// the local network
		/// </summary>
		public void DiscoverKnownServer(string ipOrHostName, int serverPort)
		{
			IPAddress ip = NetUtil.Resolve(this.Log, ipOrHostName);
			NetMessage msg = NetDiscovery.EncodeRequest(this);
			SendSingleMessageAtOnce(msg, null, new IPEndPoint(ip, serverPort));
		}

		public void DiscoverLocalServers(int serverPort)
		{
			IPEndPoint broadcast = new IPEndPoint(IPAddress.Broadcast, serverPort);
			NetMessage msg = NetDiscovery.EncodeRequest(this);

			Log.Info("Broadcasting server discovery ping...");

			// special send style; can't use simulated lag due to socket options
			try
			{
				m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

				m_sendBuffer.ResetWritePointer();
				msg.Encode(null, m_sendBuffer);

				int bytesSent = m_socket.SendTo(m_sendBuffer.Data, 0, m_sendBuffer.LengthBytes, SocketFlags.None, broadcast);
				Log.Verbose(string.Format(CultureInfo.InvariantCulture, "Sent {0} bytes to {1}", bytesSent, broadcast));
				return;
			}
			catch (SocketException sex)
			{
				if (sex.SocketErrorCode == SocketError.ConnectionReset ||
					sex.SocketErrorCode == SocketError.ConnectionRefused ||
					sex.SocketErrorCode == SocketError.ConnectionAborted)
				{
					Log.Warning("Remote socket forcefully closed: " + sex.SocketErrorCode);
					return;
				}

				Log.Warning("Execute SocketException: " + sex.SocketErrorCode);
				return;
			}
			finally
			{
				m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
			}
		}

		/// <summary>
		/// Connects to a server
		/// </summary>
		public bool Connect(IPAddress address, int port)
		{
			return Connect(address, port, null);
		}

		/// <summary>
		/// Connects to a server
		/// </summary>
		public bool Connect(string host, int port)
		{
			IPAddress ip = NetUtil.Resolve(Log, host);
			if (ip == null)
				return false;
			return Connect(ip, port, null);
		}

		/// <summary>
		/// Connects to a server
		/// </summary>
		public bool Connect(string host, int port, byte[] customData)
		{
			IPAddress ip = NetUtil.Resolve(Log, host);
			if (ip == null)
				return false;
			return Connect(ip, port, customData);
		}

		/// <summary>
		/// Connects to a server
		/// </summary>
		public bool Connect(IPAddress address, int port, byte[] customData)
		{
			if (m_serverConnection != null && m_serverConnection.Status != NetConnectionStatus.Disconnected)
				m_serverConnection.Disconnect("Reconnecting");

			IPEndPoint remoteIP = new IPEndPoint(address, port);
			m_serverConnection = new NetConnection(this, remoteIP);

			string remoteHost = remoteIP.ToString();
			Log.Info("Connecting to " + remoteHost);
			m_serverConnection.SetStatus(NetConnectionStatus.Connecting, "Connecting");

			m_serverConnection.m_firstSentHandshake = NetTime.Now;
			m_serverConnection.m_lastSentHandshake = m_serverConnection.m_firstSentHandshake;

			// save custom data for repeat connect requests
			if (customData != null)
			{
				m_serverConnection.m_savedConnectCustomData = new byte[customData.Length];
				Array.Copy(customData, m_serverConnection.m_savedConnectCustomData, customData.Length);
			}

			int bytesSent = NetHandshake.SendConnect(this, remoteIP, customData);

			// account for connect packet
			m_serverConnection.Statistics.PacketsSent++;
			m_serverConnection.Statistics.MessagesSent++;
			m_serverConnection.Statistics.BytesSent += bytesSent;

			return true;
		}

		/// <summary>
		/// Sends a message to the server using a specified channel
		/// </summary>
		public bool SendMessage(NetMessage msg, NetChannel channel)
		{
			if (m_serverConnection == null || m_serverConnection.Status == NetConnectionStatus.Disconnected)
			{
				Log.Warning("SendMessage failed - Not connected!");
				return false;
			}

			m_serverConnection.SendMessage(msg, channel);
			return true;
		}

		/// <summary>
		/// Sends all unsent messages; may interfere with proper throttling
		/// </summary>
		public void FlushMessages()
		{
			m_serverConnection.SendUnsentMessages(true, 0.01f);
		}

		/// <summary>
		/// Sends disconnect to server and close the connection
		/// </summary>
		public override void Shutdown(string reason)
		{
			Log.Info("Client shutdown: " + reason);
			if (m_serverConnection != null)
			{
				/*
				// some simple statistics
				for (int i = 0; i < m_serverConnection.m_savedReliableMessages.Length; i++)
				{
					if (m_serverConnection.m_savedReliableMessages[i].Count > 0)
						Log.Info("Saved reliable messages (" + ((NetChannel)i) + "): " + m_serverConnection.m_savedReliableMessages[i].Count);
				}
				Log.Debug(" ");
				Log.Debug("Unsent acks left: " + m_serverConnection.m_unsentAcknowledges.Count);
				Log.Debug("Unsent messages left: " + m_serverConnection.m_unsentMessages.Count);
				Log.Debug("Withheld messages left: " + m_serverConnection.m_withheldMessages.Count);
				Log.Debug("Received messages left: " + m_serverConnection.m_receivedMessages.Count);
				Log.Debug("Average RTT left: " + m_serverConnection.AverageRoundtripTime);
				*/

				if (m_serverConnection.Status != NetConnectionStatus.Disconnected)
					m_serverConnection.Disconnect(reason);

				m_serverConnection.DumpStatisticsToLog(Log);
				if (m_lagLoss != null)
				{
					Log.Debug("Artificially delayed packets still in queue: " + m_lagLoss.m_delayed.Count);
					foreach (NetLogLossInducer.DelayedPacket dm in m_lagLoss.m_delayed)
						Log.Debug("... " + dm);
				}
			}

			base.Shutdown(reason);
		}

		public override string ToString()
		{
			return "[NetClient to " + this.Status + "]";
		}
	}
}
