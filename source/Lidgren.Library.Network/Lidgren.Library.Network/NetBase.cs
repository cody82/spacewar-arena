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
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lidgren.Library.Network
{
	/// <summary>
	/// Base class for NetClient and NetServer
	/// </summary>
	public abstract class NetBase
	{
		internal Socket m_socket;
		internal NetBuffer m_sendBuffer;
		private NetBuffer m_receiveBuffer;
		private EndPoint m_senderRemote;
		internal NetLogLossInducer m_lagLoss;

		/// <summary>
		/// Configuration for this client/server
		/// </summary>
		public NetAppConfiguration Configuration;
		public NetLog Log;

		public static NetBase CurrentContext;

		/// <summary>
		/// Event fired every time the status of any connection associated with this network changes
		/// </summary>
		public event EventHandler<NetStatusEventArgs> StatusChanged;

		protected void InitBase(NetAppConfiguration config, NetLog log)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			if (log == null)
				throw new ArgumentNullException("log");

			Configuration = config;
			Log = log;

			Configuration.m_isLocked = true; // prevent changes

			// validate config
			if (config.ApplicationIdentifier == NetConstants.DefaultApplicationIdentifier)
				log.Error("Warning! ApplicationIdentifier not set in configuration!");

			if (this is NetServer)
			{
				if (config.MaximumConnections == -1)
					throw new ArgumentException("MaximumConnections must be set in configuration!");
				if (config.ServerName == NetConstants.DefaultServerName)
					log.Warning("Warning! Server name not set!");
			}

			// create buffers
			m_sendBuffer = new NetBuffer(config.SendBufferSize);
			m_receiveBuffer = new NetBuffer(config.ReceiveBufferSize);

			// Bind to port
			try
			{
				IPEndPoint iep = new IPEndPoint(IPAddress.Any, config.Port);
				EndPoint ep = (EndPoint)iep;

				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				m_socket.Blocking = false;
				m_socket.Bind(ep);
				if (iep.Port != 0)
					Log.Info("Bound to port " + iep.Port);
			}
			catch (SocketException sex)
			{
				if (sex.SocketErrorCode != SocketError.AddressAlreadyInUse)
					throw new NetException("Failed to bind to port " + config.Port + " - Address already in use!", sex);
			}
			catch (Exception ex)
			{
				throw new NetException("Failed to bind to port " + config.Port, ex);
			}
			
			m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, config.ReceiveBufferSize);
			m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, config.SendBufferSize);

			m_senderRemote = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
#if DEBUG
			m_lagLoss = new NetLogLossInducer(log);
#endif
			return;
		}

		internal protected void Heartbeat(double now)
		{
#if DEBUG
			if (m_lagLoss != null)
				m_lagLoss.SendDelayedPackets(this, now);
#endif
		}

		internal abstract void HandlePacket(NetBuffer buffer, int bytesReceived, IPEndPoint sender);

		protected bool ReadPacket()
		{
			try
			{
				if (m_socket == null || m_socket.Available < 1)
					return false;

				int bytesReceived = m_socket.ReceiveFrom(m_receiveBuffer.Data, 0, m_receiveBuffer.Data.Length, SocketFlags.None, ref m_senderRemote);
				IPEndPoint ipsender = (IPEndPoint)m_senderRemote;
				//Log.Verbose("Read " + bytesReceived + " bytes from " + ipsender);
				m_receiveBuffer.SetDataLength(bytesReceived);

				if (bytesReceived < 0)
					return true;

				m_receiveBuffer.ResetReadPointer();
				HandlePacket(m_receiveBuffer, bytesReceived, ipsender);
				return true;
			}
			catch (SocketException sex)
			{
				if (sex.SocketErrorCode == SocketError.ConnectionReset)
				{
					// who disconnected?
					HandleConnectionReset((IPEndPoint)m_senderRemote);
					return false;
				}
				Log.Warning("ReadPacket socket exception: " + sex.SocketErrorCode);
				return false;
			}
			catch (Exception ex)
			{
				throw new NetException("ReadPacket() exception", ex);
			}
		}

		// unsequenced packets only, returns number of bytes sent
		internal int SendSingleMessageAtOnce(NetMessage msg, NetConnection connection, IPEndPoint endpoint)
		{
			m_sendBuffer.ResetWritePointer();
			msg.Encode(connection, m_sendBuffer);
			if (connection != null)
				return ExecuteSend(m_sendBuffer, connection, connection.RemoteEndpoint);
			else
				return ExecuteSend(m_sendBuffer, null, endpoint);
		}

		internal int ExecuteSend(NetBuffer buffer, NetConnection connection, IPEndPoint remoteEP)
		{
			if (buffer.LengthBytes < 0)
			{
				Log.Warning("ExecuteSend passed 0 bytes to send");
				return 0;
			}

#if DEBUG
			if (connection != null)
			{
				if (connection.Configuration.SimulateLagLoss && m_lagLoss != null)
					return m_lagLoss.ExecuteSend(this, buffer, connection, remoteEP);
			}
#endif
			// encrypt
			if (connection != null && connection.m_encryption.SymmetricEncryptionKeyBytes != null)
			{
				// Log.Debug("SEND: Encrypting packet using key: " + Convert.ToBase64String(connection.SymmetricEncryptionKey));
				connection.m_encryption.EncryptSymmetric(Log, buffer);
			}

			try
			{
				int bytesSent = m_socket.SendTo(buffer.Data, 0, buffer.LengthBytes, SocketFlags.None, remoteEP);

				Debug.Assert(bytesSent == buffer.LengthBytes, "Ouch, sent partial UDP message?!");

				//Log.Verbose(string.Format(CultureInfo.InvariantCulture, "Sent {0} bytes to {1}", bytesSent, remoteEP));
				return bytesSent;
			}
			catch (SocketException sex)
			{
				if (sex.SocketErrorCode == SocketError.WouldBlock)
				{
					// send buffer overflow?
#if DEBUG
					Log.Error("SocketException.WouldBlock thrown during sending; send buffer overflow? Increase buffer using NetAppConfiguration.SendBufferSize");
					throw new NetException("SocketException.WouldBlock thrown during sending; send buffer overflow? Increase buffer using NetAppConfiguration.SendBufferSize", sex);
#else
					// let reliability handle it, but log warning
					Log.Warning("Network send buffer overflow");
#endif
				}

				if (sex.SocketErrorCode == SocketError.ConnectionReset ||
					sex.SocketErrorCode == SocketError.ConnectionRefused ||
					sex.SocketErrorCode == SocketError.ConnectionAborted)
				{
					Log.Warning("Remote socket forcefully closed: " + sex.SocketErrorCode);
					if (connection != null)
						connection.Disconnect("Socket forcefully closed: " + sex.SocketErrorCode);
					return 0;
				}

				Log.Warning("Execute SocketException: " + sex.SocketErrorCode);
				return 0;
			}
		}

		internal abstract void HandleConnectionReset(IPEndPoint remote);

		private object m_shutdownLockObject = new object();
		
		public virtual void Shutdown(string reason)
		{
			lock (m_shutdownLockObject)
			{
				Log.Info("Closing socket");
				try
				{
					if (m_socket != null)
					{
						m_socket.Shutdown(SocketShutdown.Both);
						m_socket.Close();
						m_socket = null;
					}
				}
				catch (Exception ex)
				{
					Log.Warning("NetBase shutdown exception: " + ex.Message);
				}
				finally
				{
					m_socket = null;
				}
			}
		}

		internal void NotifyStatusChange(NetConnection conn, NetConnectionStatus previousStatus, string reason)
		{
			if (StatusChanged != null)
			{
				NetStatusEventArgs e = new NetStatusEventArgs();
				e.PreviousStatus = previousStatus;
				e.Connection = conn;
				e.Reason = reason;

				StatusChanged(this, e);
			}
		}
	}
}
