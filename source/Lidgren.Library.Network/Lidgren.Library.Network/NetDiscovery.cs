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

namespace Lidgren.Library.Network
{
	/// <summary>
	/// Information about a running server
	/// </summary>
	public sealed class NetServerInfo
	{
		private IPEndPoint m_remoteEndpoint;
		private string m_serverName;
		private int m_numConnected;
		private int m_maxConnections;

		/// <summary>
		/// Remote endpoint of the server
		/// </summary>
		public IPEndPoint RemoteEndpoint
		{
			get { return m_remoteEndpoint; }
			set { m_remoteEndpoint = value; }
		}

		/// <summary>
		/// Name of the server
		/// </summary>
		public string ServerName
		{
			get { return m_serverName; }
			set { m_serverName = value; }
		}

		/// <summary>
		/// Number of clients connected (or connecting) to the server
		/// </summary>
		public int NumConnected
		{
			get { return m_numConnected; }
			set { m_numConnected = value; }
		}

		/// <summary>
		/// Maximum number of connections allowed for the server
		/// </summary>
		public int MaxConnections
		{
			get { return m_maxConnections; }
			set { m_maxConnections = value; }
		}

		public override string ToString()
		{
			return string.Format("{0} ({1}/{2})", m_serverName, m_numConnected, m_maxConnections);
		}
	}

	/// <summary>
	/// Discovery of local servers
	/// </summary>
	internal static class NetDiscovery
	{
		public static NetMessage EncodeRequest(NetClient client)
		{
			if (client == null)
				throw new ArgumentNullException("client");
			NetMessage msg = new NetMessage();
			msg.m_type = NetMessageType.Discovery;
			msg.Write(client.Configuration.ApplicationIdentifier);
			return msg;
		}

		public static NetMessage EncodeResponse(NetServer server)
		{
			if (server == null)
				throw new ArgumentNullException("server");
			NetMessage msg = new NetMessage(server.Configuration.ServerName.Length + 4);
			msg.m_type = NetMessageType.Discovery;
			msg.Write((ushort)server.NumConnected);
			msg.Write((ushort)server.Configuration.MaximumConnections);
			msg.Write((string)server.Configuration.ServerName);
			return msg;
		}

		public static NetServerInfo DecodeResponse(NetMessage msg, IPEndPoint ep)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");
			NetServerInfo info = new NetServerInfo();
			info.NumConnected = msg.ReadUInt16();
			info.MaxConnections = msg.ReadUInt16();
			info.ServerName = msg.ReadString();
			info.RemoteEndpoint = ep;
			return info;
		}
	}

	public class NetServerDiscoveredEventArgs : EventArgs
	{
		private NetServerInfo m_serverInformation;
		public NetServerInfo ServerInformation { get { return m_serverInformation; } set { m_serverInformation = value; } }
		public NetServerDiscoveredEventArgs(NetServerInfo info)
		{
			m_serverInformation = info;
		}
	} 
}
