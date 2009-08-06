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

namespace Lidgren.Library.Network
{
	/// <summary>
	/// A network server
	/// </summary>
	public class NetServer : NetBase
	{
		/// <summary>
		/// List of all connections to this server; may contain null entries and
		/// entries may have status disconnected
		/// </summary>
		public NetConnection[] Connections;
		private Dictionary<int, NetConnection> m_connectionsLookUpTable;

		/// <summary>
		/// Event fired just before a client is connected; allows the application to reject unwanted connections
		/// </summary>
		public event EventHandler<NetConnectRequestEventArgs> ConnectionRequest;

		/// <summary>
		/// Gets the number of client that are connected (or connecting!)
		/// </summary>
		public int NumConnected
		{
			get
			{
				int retval = 0;
				for (int i = 0; i < Connections.Length; i++)
					if (Connections[i] != null && Connections[i].Status != NetConnectionStatus.Disconnected)
						retval++;
				return retval;
			}
		}

		/// <summary>
		/// Constructor for a server instance
		/// </summary>
		public NetServer(NetAppConfiguration config, NetLog log)
		{
			NetBase.CurrentContext = this;
			InitBase(config, log);
			Connections = new NetConnection[config.MaximumConnections];
			m_connectionsLookUpTable = new Dictionary<int, NetConnection>();
		}

		/// <summary>
		/// Read messages from network and sends unsent messages, resends etc
		/// This method should be called as often as possible
		/// </summary>
		public void Heartbeat()
		{
			double now = NetTime.Now;

			NetBase.CurrentContext = this;

			// read all packets
			while(ReadPacket());

			// connection heartbeats
			for (int i = 0; i < Connections.Length; i++)
			{
				NetConnection connection = Connections[i];
				if (connection != null)
				{
					NetConnectionStatus status = connection.Status;
					if (status == NetConnectionStatus.Connected || status == NetConnectionStatus.Connecting || status == NetConnectionStatus.Reconnecting)
						connection.Heartbeat(now);
				}
			}
		}

		public NetConnection FindConnection(IPEndPoint endpoint)
		{
			NetConnection retval;
			if (m_connectionsLookUpTable.TryGetValue(endpoint.GetHashCode(), out retval))
				return retval;
			return null;

			/*
			for (int i = 0; i < Connections.Length; i++)
			{
				if (Connections[i] != null && Connections[i].Status != NetConnectionStatus.Disconnected && Connections[i].RemoteEndpoint.Equals(endpoint))
					return Connections[i];
			}
			return null;
			*/
		}

		internal override void HandlePacket(NetBuffer buffer, int bytesReceived, IPEndPoint senderEndpoint)
		{
			double now = NetTime.Now;
			NetConnection sender = FindConnection(senderEndpoint);
			if (sender != null)
			{
				sender.m_lastHeardFromRemote = now;

				if (sender.m_encryption.SymmetricEncryptionKeyBytes != null)
				{
					bool ok = sender.m_encryption.DecryptSymmetric(Log, buffer);
					if (!ok)
					{
						Log.Warning("Failed to decrypt packet from client " + sender);
						return;
					}
				}
			}

			try
			{
				NetMessage response;
				int messagesReceived = 0;
				int usrMessagesReceived = 0;
				int ackMessagesReceived = 0;
				while (buffer.ReadBitsLeft > 7)
				{
					NetMessage msg = NetMessage.Decode(buffer);
					if (msg == null)
						break; // done
				
					messagesReceived++;
					msg.Sender = sender;
					switch (msg.m_type)
					{
						case NetMessageType.Acknowledge:
						case NetMessageType.AcknowledgeBitField:
							if (sender == null)
							{
								Log.Warning("Received Ack from not-connected source!");
							}
							else
							{
								//Log.Debug("Received ack " + msg.SequenceChannel + "|" + msg.SequenceNumber);
								sender.ReceiveAcknowledge(msg);
								ackMessagesReceived++;
							}
							break;
						case NetMessageType.Handshake:
							NetHandshakeType tp = (NetHandshakeType)msg.ReadByte();
							if (tp == NetHandshakeType.ConnectResponse)
							{
								Log.Warning("Received ConnectResponse?!");
							}
							else if (tp == NetHandshakeType.Connect)
							{
								if (sender == null)
								{
									NetHandshake.HandleConnect(msg, this, senderEndpoint);
								}
								else
								{
									// resend response
									NetHandshake.SendConnectResponse(this, sender, senderEndpoint);
									Log.Verbose("Redundant Connect received; resending response");
								}
							}
							else if (tp == NetHandshakeType.ConnectionEstablished)
							{
								if (sender == null)
								{
									Log.Warning("Received ConnectionEstablished, but no sender connection?!");
								}
								else
								{
									float rt = (float)(now - sender.m_firstSentHandshake);
									sender.m_ping.Initialize(rt);

									ushort remoteValue = msg.ReadUInt16();
									sender.RemoteClockOffset = NetTime.CalculateOffset(now, remoteValue, rt);
									Log.Verbose("Reinitializing remote clock offset to " + sender.RemoteClockOffset + " ms (roundtrip " + NetUtil.SecToMil(rt) + " ms)");

									if (sender.Status == NetConnectionStatus.Connected)
									{
										Log.Verbose("Redundant ConnectionEstablished received");
									}
									else
									{
										sender.SetStatus(NetConnectionStatus.Connected, "Connected by established");
									}
								}
							}
							else
							{
								// disconnected
								if (sender == null)
								{
									Log.Warning("Disconnected received from unconnected source: " + senderEndpoint);
									return;
								}
								string reason = msg.ReadString();
								sender.Disconnected(reason);
							}
							break;
						case NetMessageType.Discovery:
							Log.Debug("Answering discovery response from " + senderEndpoint);
							response = NetDiscovery.EncodeResponse(this);
							SendSingleMessageAtOnce(response, null, senderEndpoint);
							break;
						case NetMessageType.PingPong:
							
							if (sender == null)
								return;

							bool isPong = msg.ReadBoolean();
							bool isOptimizeInfo = msg.ReadBoolean();
							if (isOptimizeInfo)
							{
								// DON'T handle optimizeinfo... only clients should adapt to server ping info
							} else if (isPong)
							{
								if (sender.Status == NetConnectionStatus.Connected)
									sender.m_ping.HandlePong(now, msg);
							}
							else
							{
								NetPing.ReplyPong(msg, sender); // send pong
							}
							break;
						case NetMessageType.User:
						case NetMessageType.UserFragmented:
							usrMessagesReceived++;
							if (sender == null)
							{
								Log.Warning("User message received from unconnected source: " + senderEndpoint);
								return; // don't handle user messages from unconnected sources
							}
							if (sender.Status == NetConnectionStatus.Connecting)
								sender.SetStatus(NetConnectionStatus.Connected, "Connected by user message");
							sender.ReceiveMessage(now, msg);
							break;
						default:
							Log.Warning("Bad message type: " + msg);
							break;
					}
				}

				if (sender != null)
				{
					NetStatistics stats = sender.Statistics;
					stats.PacketsReceived++;
					stats.MessagesReceived += messagesReceived;
					stats.UserMessagesReceived += usrMessagesReceived;
					stats.AckMessagesReceived += ackMessagesReceived;
					stats.BytesReceived += bytesReceived;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Failed to parse packet correctly; read/write mismatch? " + ex);
			}
		}

		/// <summary>
		/// Reads a received message from any connection to the server
		/// </summary>
		public NetMessage ReadMessage()
		{
			NetBase.CurrentContext = this;

			for (int i = 0; i < Connections.Length; i++)
			{
				if (Connections[i] != null && Connections[i].Status != NetConnectionStatus.Disconnected)
				{
					if (Connections[i].m_receivedMessages.Count > 0)
						return Connections[i].m_receivedMessages.Dequeue();
				}
			}
			return null;
		}

		internal override void HandleConnectionReset(IPEndPoint ep)
		{
			NetConnection conn = FindConnection(ep);
			if (conn == null)
				return; // gulp

			if (conn.Status == NetConnectionStatus.Disconnected)
			{
				Log.Verbose("ConnectionReset from already disconnected connection " + conn);
				return;
			}

			// oi! disconnect 
			conn.SetStatus(NetConnectionStatus.Disconnected, "Connection Reset");
		}

		internal NetConnection AddConnection(IPEndPoint remoteEndpoint, int remoteClockOffset)
		{
			// find empty slot
			for (int i = 0; i < Connections.Length; i++)
			{
				if (Connections[i] == null)
				{
					NetConnection conn = new NetConnection(this, remoteEndpoint);
					conn.RemoteClockOffset = remoteClockOffset;
					Log.Verbose("Initializing remote clock offset to " + remoteClockOffset + " ms");
					conn.m_firstSentHandshake = NetTime.Now;
					conn.m_lastSentHandshake = conn.m_firstSentHandshake;

					int hash = remoteEndpoint.GetHashCode();
					NetConnection existingConn;
					if (m_connectionsLookUpTable.TryGetValue(hash, out existingConn))
					{
						if (existingConn.Status != NetConnectionStatus.Disconnected)
							throw new NetException("Ack thphth; Connections lookup hash value taken!");

						// disconnected; just remove it
						RemoveConnection(existingConn);
					}

					Connections[i] = conn;

					m_connectionsLookUpTable[hash] = conn;

					conn.SetStatus(NetConnectionStatus.Connecting, "Connecting from " + remoteEndpoint);
					return conn;
				}
			}

			Log.Warning("Failed to add new connection!");
			return null;
		}

		/// <summary>
		/// Sends a message to a certain connection using the channel specified
		/// </summary>
		public bool SendMessage(NetMessage msg, NetConnection connection, NetChannel channel)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");

			if (connection.Status == NetConnectionStatus.Disconnected)
			{
				Log.Warning("SendMessage failed - Connection is Disconnected!");
				return false;
			}

			connection.SendMessage(msg, channel);
			return true;
		}

		/// <summary>
		/// Sends a message to several connections using the channel specified; should NOT be
		/// used for messages which uses the string table read/write methods!
		/// </summary>
		/// <returns>number of messages sent</returns>
		public int SendMessage(NetMessage msg, IEnumerable<NetConnection> connections, NetChannel channel)
		{
			if (connections == null)
				throw new ArgumentNullException("connection");

			int numSentTo = 0;
			bool originalSent = false;
			foreach (NetConnection connection in connections)
			{
				if (connection == null || (connection.Status != NetConnectionStatus.Connected && connection.Status != NetConnectionStatus.Reconnecting))
					continue;

				if (!originalSent)
				{
					connection.SendMessage(msg, channel);
					originalSent = true;
				}
				else
				{
					// send refcloned message
					NetMessage clone = NetMessage.CreateReferenceClone(msg);
					connection.SendMessage(clone, channel);
				}
				numSentTo++;
			}

			// Log.Debug("Broadcast to " + numSentTo + " connections...");

			return numSentTo;
		}

		/// <summary>
		/// Broadcasts a message to all connections except specified; should NOT be used for
		/// messages which uses the string table read/write methods!
		/// </summary>
		public int Broadcast(NetMessage msg, NetChannel channel, NetConnection except)
		{
			int numSentTo = 0;
			bool originalSent = false;
			foreach (NetConnection conn in Connections)
			{
				if (conn == null || conn == except || (conn.Status != NetConnectionStatus.Connected && conn.Status != NetConnectionStatus.Reconnecting))
					continue;

				numSentTo++;

				if (!originalSent)
				{
					// send original
					conn.SendMessage(msg, channel);
					originalSent = true;
				}
				else
				{
					// send refcloned message
					NetMessage clone = NetMessage.CreateReferenceClone(msg);
					conn.SendMessage(clone, channel);
					numSentTo++;
				}
			}

			return numSentTo;
		}

		/// <summary>
		/// Broadcasts a message to all connections; should NOT be used for messages which
		/// uses the string table read/write methods!
		/// </summary>
		public int Broadcast(NetMessage msg, NetChannel channel)
		{
			return SendMessage(msg, Connections, channel);
		}

		/// <summary>
		/// Sends all unsent messages for all connections; may interfere with proper throttling!
		/// </summary>
		public void FlushMessages()
		{
			for (int i = 0; i < Connections.Length; i++)
			{
				NetConnection connection = Connections[i];
				if (connection != null && connection.Status != NetConnectionStatus.Disconnected)
					connection.SendUnsentMessages(true, 0.01f);
			}
		}

		/// <summary>
		/// Sends disconnect to all connections and stops listening for new connections
		/// </summary>
		public override void Shutdown(string reason)
		{
			for (int i = 0; i < Connections.Length; i++)
			{
				if (Connections[i] != null)
				{
					Log.Debug("Statistics for " + Connections[i]);
					Connections[i].DumpStatisticsToLog(Log);
					if (Connections[i].Status != NetConnectionStatus.Disconnected)
						Connections[i].Disconnect(reason);
					Connections[i] = null;
				}
			}
			if (m_lagLoss != null)
			{
				Log.Debug("Artificially delayed packets still in queue: " + m_lagLoss.m_delayed.Count);
				foreach (NetLogLossInducer.DelayedPacket dm in m_lagLoss.m_delayed)
					Log.Debug("... " + dm);
			}
			base.Shutdown(reason);
		}

		internal bool ApproveConnection(IPEndPoint senderEndpoint, byte[] customData, out string failReason)
		{
			if (ConnectionRequest != null)
			{
				NetConnectRequestEventArgs ea = new NetConnectRequestEventArgs();
				ea.EndPoint = senderEndpoint;
				ea.CustomData = customData;
				ea.MayConnect = true;
				ea.DenialReason = null;

				// ask application
				ConnectionRequest(this, ea);

				failReason = ea.DenialReason;
				return ea.MayConnect;
			}

			failReason = null;
			return true;
		}

		/// <summary>
		/// Remove connection from Connections list(s)
		/// </summary>
		internal void RemoveConnection(NetConnection conn)
		{
			if (conn == null)
				return;

			Log.Debug("Removing connection " + conn);
			for(int i=0;i<Connections.Length;i++)
				if (Connections[i] == conn)
					Connections[i] = null;

			// also remove lookup entry
			int hash = conn.RemoteEndpoint.GetHashCode();
			if (m_connectionsLookUpTable.ContainsKey(hash))
				m_connectionsLookUpTable.Remove(hash);
		}

		public override string ToString()
		{
			return "[NetServer " + this.NumConnected + " of " + this.Configuration.MaximumConnections + " connected]";
		}
	}
}
