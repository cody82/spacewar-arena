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
using System.Diagnostics;

namespace Lidgren.Library.Network
{
	/// <summary>
	/// A network connection between two endpoints
	/// </summary>
	public class NetConnection
	{
		private NetConnectionStatus m_status;
		private NetBase m_parent;
		private IPEndPoint m_remoteEndpoint;
		private ushort[] m_nextSequenceNumber;
		private NetSequenceHandler m_sequenceHandler;
		private NetMessage m_reusedAckMessage;
		private int m_remoteClockOffset;
		private float m_throttleDebt;

		internal double m_lastHeardFromRemote;
		internal NetPing m_ping;
		internal Queue<NetMessage> m_unsentMessages;
		internal Queue<NetMessage> m_receivedMessages;
		internal List<NetMessage> m_withheldMessages;
		internal Queue<NetMessage> m_unsentAcknowledges;
		internal List<NetMessage>[] m_savedReliableMessages;
		internal double m_forceExplicitAckTime;
		internal NetStringTable m_stringTable;
		internal NetEncryption m_encryption;

		/// <summary>
		/// Status for this connection
		/// </summary>
		public NetConnectionStatus Status { get { return m_status; } }

		/// <summary>
		/// Remote endpoint of this connection
		/// </summary>
		public IPEndPoint RemoteEndpoint { get { return m_remoteEndpoint; } }
		
		/// <summary>
		/// Gets which NetClient/NetServer holds this connection
		/// </summary>
		public NetBase Parent { get { return m_parent; } }

		/// <summary>
		/// Configuration for this connection
		/// </summary>
		public NetConnectionConfiguration Configuration;
		
		public NetStatistics Statistics;
		
		/// <summary>
		/// Attached application data
		/// </summary>
		public object Tag;

		// Event fired when the network configuration is automatically optimized
		public event EventHandler<EventArgs> ConfigurationOptimized;

		// Event fired when a message is resent
		public event EventHandler<NetMessageEventArgs> MessageResent;

		// Event fired when a message is acknowledged
		public event EventHandler<NetMessageEventArgs> MessageAcknowledged;

		/// <summary>
		/// Local + Offset = Remote
		/// </summary>
		public int RemoteClockOffset
		{
			get { return m_remoteClockOffset; }
			internal set { m_remoteClockOffset = value; }
		}

		internal double m_firstSentHandshake, m_lastSentHandshake;

		internal void SetStatus(NetConnectionStatus status, string reason)
		{
			if (m_status == status)
			{
				m_parent.Log.Warning("Redundant status change: " + status);
				return;
			}
			NetConnectionStatus wasStatus = m_status;
			m_status = status;
			m_parent.NotifyStatusChange(this, wasStatus, reason);

			if (m_status == NetConnectionStatus.Connected)
				m_savedConnectCustomData = null; // no need to hang on to this now

			if (m_status == NetConnectionStatus.Disconnected)
			{
				NetServer server = m_parent as NetServer;
				if (server != null)
					server.RemoveConnection(this);
			}
		}

		/// <summary>
		/// Average roundtrip time to remote host
		/// </summary>
		public float AverageRoundtripTime { get { return m_ping.AverageRoundtrip; } }

		internal NetConnection(NetBase parent, IPEndPoint remote)
		{
			m_parent = parent;
			m_remoteEndpoint = remote;
			m_nextSequenceNumber = new ushort[NetConstants.NumSequenceChannels];
			m_sequenceHandler = new NetSequenceHandler();
			m_unsentMessages = new Queue<NetMessage>(10);
			m_receivedMessages = new Queue<NetMessage>(10);
			m_withheldMessages = new List<NetMessage>();
			m_unsentAcknowledges = new Queue<NetMessage>(10);
			m_lastHeardFromRemote = (float)NetTime.Now;
			m_reusedAckMessage = new NetMessage(NetMessageType.Acknowledge, null);
			m_savedReliableMessages = new List<NetMessage>[NetConstants.NumSequenceChannels];
			for (int i = 0; i < m_savedReliableMessages.Length; i++)
				m_savedReliableMessages[i] = new List<NetMessage>();
			Configuration = new NetConnectionConfiguration(this);
			m_ping = new NetPing(this);
			Statistics = new NetStatistics(this);
			m_stringTable = new NetStringTable();
			m_encryption = new NetEncryption();
			if (parent.Configuration.UsesEncryption)
				m_encryption.SetRSAKey(parent.Configuration.ServerPublicKeyBytes, parent.Configuration.ServerPrivateKeyBytes);
		}

		private double m_lastHeartbeat;
		private double m_frameLength;
		internal byte[] m_savedConnectCustomData;

		// temporary storage
		private List<NetMessage> m_tmpRemoveList = new List<NetMessage>(10);

		/// <summary>
		/// Connection heartbeat, normally called from NetClient.Heartbeat() or NetServer.Heartbeat()
		/// Calling it manually may interfere with proper throttling
		/// </summary>
		public void Heartbeat(double now)
		{
			m_frameLength = now - m_lastHeartbeat;
			m_lastHeartbeat = now;

			// resend timed out saved packets
			for (int i = 0; i < m_savedReliableMessages.Length; i++)
			{
				if (m_savedReliableMessages[i].Count > 0)
				{
					foreach (NetMessage msg in m_savedReliableMessages[i])
					{
						if (now > msg.m_nextResendTime)
						{
							// it might have been lost; resend it
							if (msg.m_numResends >= Configuration.NumResendsBeforeFailing)
							{
								m_parent.Log.Error("Dropping outgoing packet " + msg + "; maximum number of resends reached!");
							}
							else
							{
								if (MessageResent != null)
									MessageResent(this, new NetMessageEventArgs(msg));
								//m_parent.Log.Verbose("Resending " + msg + " @ " + now);
								m_unsentMessages.Enqueue(msg);

								float nextResend = (float)now + (msg.m_numResends < 1 ? Configuration.ResendFirstUnacknowledgedDelay : Configuration.ResendSubsequentUnacknowledgedDelay);
								msg.m_nextResendTime = nextResend;

								msg.m_numResends++;
								Statistics.MessagesResent++;
							}
							m_tmpRemoveList.Add(msg); // to be removed
						}
					}

					if (m_tmpRemoveList.Count > 0)
					{
						foreach (NetMessage rm in m_tmpRemoveList)
							m_savedReliableMessages[i].Remove(rm); // remove it; it will be readded upon sending
						m_tmpRemoveList.Clear();
					}
				}
			}

			// send all messages (possibly forcing acks to be sent)
			SendUnsentMessages((m_forceExplicitAckTime != 0.0 && now > m_forceExplicitAckTime),
				(m_frameLength > 0.1 ? 0.1 : m_frameLength)); // throttling don't like unlikely long frames

			if (now - m_lastHeardFromRemote > Configuration.ConnectionTimeOut)
			{
				NetHandshake.SendDisconnected(m_parent, "Connection timed out", this);
				SetStatus(NetConnectionStatus.Disconnected, "Connection timed out");
				return;
			}

			if (m_status == NetConnectionStatus.Connecting)
			{
				// keep sending connect packets
				if (now - m_lastSentHandshake > NetConstants.SendHandshakeFrequency)
				{
					if (now - m_firstSentHandshake > NetConstants.ConnectAttemptTimeout)
					{
						m_parent.Log.Info("Failed to connect; no answer from remote host!");
						SetStatus(NetConnectionStatus.Disconnected, "Connect attempt timeout");
					}
					else
					{
						NetClient client = m_parent as NetClient;
						if (client != null)
						{
							int bytesSent = NetHandshake.SendConnect(client, m_remoteEndpoint, m_savedConnectCustomData);
							client.ServerConnection.Statistics.PacketsSent++;
							client.ServerConnection.Statistics.MessagesSent++;
							client.ServerConnection.Statistics.BytesSent += bytesSent;
							m_lastSentHandshake = now;
						}
						else
						{
							NetHandshake.SendConnectResponse(m_parent, this, m_remoteEndpoint);
							m_lastSentHandshake = now;
						}
					}
				}
				return;
			}

			m_ping.Heartbeat(now);
		}

		private NetMessage[] m_incompleteMessages = new NetMessage[256];
		private int[] m_incompleteFragmentsReceived = new int[256];
		private int[] m_incompleteSplitSize = new int[256];
		private int[] m_incompleteTotalBytes = new int[256];

		internal void ReceiveMessage(double now, NetMessage msg)
		{
			// reject duplicates and late sequenced messages
			bool rejectMessage, ackMessage, withholdMessage;
			m_sequenceHandler.AddSequence(msg, out rejectMessage, out ackMessage, out withholdMessage);

			if (ackMessage)
			{
				if (m_unsentAcknowledges.Count <= 0)
					m_forceExplicitAckTime = m_lastHeardFromRemote + Configuration.ForceExplicitAckDelay;
				m_unsentAcknowledges.Enqueue(msg);
			}

			if (rejectMessage)
			{
				m_parent.Log.Debug("Message rejected: " + msg);
				return;
			}

			if (withholdMessage)
			{
				//m_parent.Log.Debug("Message withheld: " + msg);
				m_withheldMessages.Add(msg);
				return;
			}

			ReleaseMessageToApplication(msg);

			// enqueue any currently withheld messages?
			int cnt = m_withheldMessages.Count;
			if (cnt > 0)
			{
			WithheldChecking:
				for (int i = 0; i < cnt; i++)
				{
					NetMessage withheld = m_withheldMessages[i];
					if (withheld.SequenceChannel == msg.SequenceChannel)
					{
						if (m_sequenceHandler.RelateToExpected(withheld) == 0)
						{
							m_withheldMessages.Remove(withheld);
							
							//m_receivedMessages.Enqueue(withheld);
							ReleaseMessageToApplication(withheld);

							// advance expected
							m_sequenceHandler.AdvanceExpected(withheld.SequenceChannel, 1);

							cnt = m_withheldMessages.Count;
							goto WithheldChecking; // restart to check entire list
						}
					}
				}
			}
		}

		private void ReleaseMessageToApplication(NetMessage msg)
		{
			//m_parent.Log.Debug("Releasing message: " + msg);
			if (msg.m_type == NetMessageType.UserFragmented)
			{
				// fragmented; check id, check ftor completeness, withhold incomplete

				int fragDataLen = msg.Length - 5;
				byte fid = msg.m_buffer.Data[0];
				int totalFragments = msg.m_buffer.Data[1] + (msg.m_buffer.Data[2] << 8);
				int thisFragment = msg.m_buffer.Data[3] + (msg.m_buffer.Data[4] << 8);

				if (totalFragments <= 0 || thisFragment >= totalFragments)
				{
					// just drop it
					m_parent.Log.Warning("Bad message fragment; " + thisFragment + " of " + totalFragments);
					return;
				}

				//m_parent.Log.Debug("Received fragment id " + fid + "; fragment " + thisFragment + " of " + totalFragments + " size: " + fragDataLen);

				if (m_incompleteMessages[fid] == null)
				{
					//
					// new fragments assembly detected
					//

					if (thisFragment == totalFragments - 1)
					{
						// Ouch! We received the LAST fragment FIRST, so we are unable
						// to determine a correct splitsize...
						return; // TODO FIX THIS
					}

					m_incompleteMessages[fid] = new NetMessage(NetMessageType.User, totalFragments * fragDataLen);
					m_incompleteSplitSize[fid] = fragDataLen;

					// clear next slot; we don't want any broken (by loss) fragmented unreliable message mess
					// up the next assembly
					int nextFid = (fid + 1) % 256;
					m_incompleteMessages[nextFid] = null;
					m_incompleteFragmentsReceived[nextFid] = 0;
					m_incompleteTotalBytes[nextFid] = 0;
				}

				Buffer.BlockCopy(msg.m_buffer.Data, 5, m_incompleteMessages[fid].m_buffer.Data, thisFragment * m_incompleteSplitSize[fid], fragDataLen);
				m_incompleteFragmentsReceived[fid]++;
				m_incompleteTotalBytes[fid] += fragDataLen;

				if (m_incompleteFragmentsReceived[fid] == totalFragments)
				{
					// Whee, it's complete! Pass it on to the application
					msg = m_incompleteMessages[fid];
					msg.m_buffer.LengthBits = m_incompleteTotalBytes[fid] * 8;
					m_incompleteMessages[fid] = null;
					m_incompleteFragmentsReceived[fid] = 0;
					m_incompleteTotalBytes[fid] = 0;
					//m_parent.Log.Debug("Fragmented message complete!");
				}
				else
				{
					// not done yet
					return;
				}
			}

			m_receivedMessages.Enqueue(msg);
		}

		internal void ReceiveAcknowledge(NetMessage msg)
		{
			// todo: optimize
			int seqChan = (int)msg.SequenceChannel;
			ushort seqNr = msg.SequenceNumber;
			List<NetMessage> list = m_savedReliableMessages[seqChan];
			foreach(NetMessage fm in list)
			{
				if (fm.SequenceNumber == seqNr)
				{
					if (MessageAcknowledged != null)
						MessageAcknowledged(this, new NetMessageEventArgs(fm));

					//m_parent.Log.Debug("Stored message acked and removed: " + msg.SequenceChannel + "|" + msg.SequenceNumber);
					m_savedReliableMessages[seqChan].Remove(fm);

					return;
				}
			}

			m_parent.Log.Verbose("Failed to find " + msg.SequenceChannel + "|" + msg.SequenceNumber + " in saved list; probably double acked");
		}

		public void AssignSequenceNumber(NetMessage msg, NetChannel channel)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");
			int chanNr = (int)channel;
			lock (m_nextSequenceNumber)
			{
				msg.SequenceChannel = channel;
				msg.SequenceNumber = m_nextSequenceNumber[chanNr]++;
				if (m_nextSequenceNumber[chanNr] >= NetConstants.NumSequenceNumbers)
					m_nextSequenceNumber[chanNr] = 0;
			}
			return;
		}

		private byte m_nextFragmentAssemblyId = 42;

		internal void SendMessage(NetMessage msg, NetChannel channel)
		{
			if (msg.Length > m_parent.Configuration.MaximumTransmissionUnit)
			{
				// fragment large message
				int totlen = msg.Length;
				int splitSize = m_parent.Configuration.MaximumTransmissionUnit - 13; // todo: research this number
				int numFragments = totlen / splitSize;
				if (numFragments * splitSize < totlen)
					numFragments++;
				
				byte hdr1 = (byte)numFragments;
				byte hdr2 = (byte)(numFragments >> 8);

				int ptr = 0;
				for (int i = 0; i < numFragments; i++)
				{
					int numBytes = msg.Length - ptr;
					if (numBytes > splitSize)
						numBytes = splitSize;
					byte[] fragmentData = new byte[5 + numBytes];
					fragmentData[0] = m_nextFragmentAssemblyId;
					fragmentData[1] = hdr1;
					fragmentData[2] = hdr2;
					fragmentData[3] = (byte)i;
					fragmentData[4] = (byte)(i >> 8);
					Buffer.BlockCopy(msg.m_buffer.Data, ptr, fragmentData, 5, numBytes);

					// send fragment
					NetMessage fragment = new NetMessage(NetMessageType.UserFragmented, fragmentData);
					SendMessage(fragment, channel);

					ptr += numBytes;
				}
				//m_parent.Log.Debug("Sent " + numFragments + " fragments, " + splitSize + " bytes per fragment = " + totlen);
				m_nextFragmentAssemblyId++;
				return;
			}

			msg.SequenceChannel = channel;
			AssignSequenceNumber(msg, channel);
			m_unsentMessages.Enqueue(msg);
		}

		internal void SendUnsentMessages(bool forceAcks, double frameLength)
		{
			if (m_unsentMessages.Count < 1 && !forceAcks)
			{
				if (m_status == NetConnectionStatus.Disconnecting)
					SetStatus(NetConnectionStatus.Disconnected, m_pendingDisconnectedReason);
				return; // nothing to send
			}

			//if (m_unsentMessages.Count < 1 && forceAcks)
			//	m_parent.Log.Debug("FORCING EXPLICIT ACK");

			float sendBytesAllowed = (float)frameLength * (float)Configuration.ThrottleBytesPerSecond;
			if (m_throttleDebt > sendBytesAllowed)
			{
				// NetBase.CurrentContext.Log.Debug("Working off debt: " + m_throttleDebt + " bytes by " + sendBytesAllowed");
				m_throttleDebt -= sendBytesAllowed;
				return;
			}
			else if (m_throttleDebt > 0)
			{
				sendBytesAllowed -= m_throttleDebt;
				m_throttleDebt = 0;
			}

			NetBuffer sendBuffer = m_parent.m_sendBuffer;
			sendBuffer.ResetWritePointer();

			int mtu = m_parent.Configuration.MaximumTransmissionUnit;

			float now = (float)NetTime.Now;

			int pktMsgAdded = 0;
			int ackMsgAdded = 0;
			int usrMsgAdded = 0;
			NetMessage msg;

			// TODO: make ack bitfield if possible

			while (m_unsentAcknowledges.Count > 0 && sendBytesAllowed > 0)
			{
				msg = m_unsentAcknowledges.Dequeue();
				m_reusedAckMessage.SequenceChannel = msg.SequenceChannel;
				m_reusedAckMessage.SequenceNumber = msg.SequenceNumber;

				if (sendBuffer.LengthBytes + 3 > mtu)
				{
					// send buffer
					m_parent.ExecuteSend(sendBuffer, this, m_remoteEndpoint);
					sendBytesAllowed -= sendBuffer.LengthBytes;

					Statistics.PacketsSent++;
					Statistics.MessagesSent += pktMsgAdded;
					Statistics.AckMessagesSent += ackMsgAdded;
					Statistics.BytesSent += sendBuffer.LengthBytes;

					sendBuffer.ResetWritePointer();
					pktMsgAdded = 0;
					ackMsgAdded = 0;
				}

				//m_parent.Log.Debug("Sending ack " + msg.SequenceChannel + "|" + msg.SequenceNumber);
				m_reusedAckMessage.Encode(this, sendBuffer);
				pktMsgAdded++;
				ackMsgAdded++;
			}

			// no unsent acks left!
			m_forceExplicitAckTime = 0.0;

			while (m_unsentMessages.Count > 0 && sendBytesAllowed > 0)
			{
				msg = m_unsentMessages.Dequeue();

				// make pessimistic estimate of message length
				int estMsgLen = msg.EstimateEncodedLength();

				if (sendBuffer.LengthBytes + estMsgLen > mtu)
				{
					if (pktMsgAdded < 1)
						throw new NetException("Message too large to send: " + estMsgLen + " bytes: " + msg);

					// send buffer
					m_parent.ExecuteSend(sendBuffer, this, m_remoteEndpoint);
					sendBytesAllowed -= sendBuffer.LengthBytes;

					Statistics.PacketsSent++;
					Statistics.MessagesSent += pktMsgAdded;
					Statistics.UserMessagesSent += usrMsgAdded;
					Statistics.AckMessagesSent += ackMsgAdded;
					Statistics.BytesSent += sendBuffer.LengthBytes;

					sendBuffer.ResetWritePointer();
					pktMsgAdded = 0;
					ackMsgAdded = 0;
				}

				msg.Encode(this, sendBuffer);
				if (msg.m_type == NetMessageType.User || msg.m_type == NetMessageType.UserFragmented)
					usrMsgAdded++;
				pktMsgAdded++;

				// store until acknowledged
				if (msg.SequenceChannel >= NetChannel.ReliableUnordered)
				{
					float nextResend = now + (msg.m_numResends < 1 ? Configuration.ResendFirstUnacknowledgedDelay : Configuration.ResendSubsequentUnacknowledgedDelay);
					msg.m_nextResendTime = nextResend;
					//m_parent.Log.Debug("Storing " + msg.SequenceChannel + "|" + msg.SequenceNumber + " @ " + now + " first resend: " + nextResend);
					m_savedReliableMessages[(int)msg.SequenceChannel].Add(msg);
				}
			}

			if (pktMsgAdded > 0)
			{
				m_parent.ExecuteSend(sendBuffer, this, m_remoteEndpoint);
				sendBytesAllowed -= sendBuffer.LengthBytes;
				Statistics.PacketsSent++;
				Statistics.MessagesSent += pktMsgAdded;
				Statistics.UserMessagesSent += usrMsgAdded;
				Statistics.AckMessagesSent += ackMsgAdded;
				Statistics.BytesSent += sendBuffer.LengthBytes;
			}

			Debug.Assert(m_throttleDebt == 0);
			if (sendBytesAllowed < 0)
				m_throttleDebt = -sendBytesAllowed;
		}

		private string m_pendingDisconnectedReason;

		/// <summary>
		/// Disconnects from remote host providing specified reason
		/// </summary>
		public void Disconnect(string reason)
		{
			if (m_status != NetConnectionStatus.Disconnected)
			{
				SendUnsentMessages(true, 1.0f); // flush

				NetHandshake.SendDisconnected(m_parent, reason, this);
				if (m_unsentMessages.Count < 1)
				{
					SetStatus(NetConnectionStatus.Disconnected, reason);
				}
				else
				{
					SetStatus(NetConnectionStatus.Disconnecting, reason);
					m_pendingDisconnectedReason = reason;
				}
			}
		}

		internal void Disconnected(string reason)
		{
			SendUnsentMessages(true, 1.0f); // flush

			if (m_unsentMessages.Count <= 0)
				SetStatus(NetConnectionStatus.Disconnected, reason);
			else
				m_pendingDisconnectedReason = reason;
		}

		/// <summary>
		/// Dump all statistics for this connection to the log specified
		/// </summary>
		public void DumpStatisticsToLog(NetLog log)
		{
			Statistics.DumpToLog(log);

			for(int i=0;i<NetConstants.NumSequenceChannels;i++)
			{
				NetChannel channel = (NetChannel)i;
				if (m_savedReliableMessages[i].Count > 0)
				{
					log.Debug("Saved reliable messages left in " + channel + ": " + m_savedReliableMessages[i].Count);
					foreach (NetMessage mmm in m_savedReliableMessages[i])
					{
						log.Debug("... " + mmm + " - next resend time: " + mmm.m_nextResendTime + " numresends: " + mmm.m_numResends);
					}
				}
			}
			log.Debug("Unsent acknowledges: " + m_unsentAcknowledges.Count);
			log.Debug("Withheld messages: " + m_withheldMessages.Count);
			log.Flush();
		}

		internal void NotifyOptimized()
		{
			if (ConfigurationOptimized != null)
				ConfigurationOptimized(this, EventArgs.Empty);

			/*
			m_parent.Log.Info("Resend first time:        " + (Configuration.ResendFirstUnacknowledgedDelay * 1000) + "ms");
			m_parent.Log.Info("Resend subseqent times:   " + (Configuration.ResendSubsequentUnacknowledgedDelay * 1000) + "ms");
			m_parent.Log.Info("#Resends before failing:  " + Configuration.NumResendsBeforeFailing);
			m_parent.Log.Info("Force explicit ack after: " + (Configuration.ForceExplicitAckDelay * 1000) + "ms");
			m_parent.Log.Info("Ping frequency:           " + (Configuration.PingFrequency * 1000) + "ms");
			*/
		}

		public override string ToString()
		{
			return "[NetConnection to " + m_remoteEndpoint + "]";
		}
	}
}
