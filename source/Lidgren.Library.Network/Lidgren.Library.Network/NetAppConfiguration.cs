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
	/// Configuration for a networked app
	/// </summary>
	public sealed class NetAppConfiguration
	{
		//
		// application settings
		//
		internal bool m_isLocked;

		// lockable fields
		private int m_sendBufferSize;
		private int m_receiveBufferSize;
		private string m_applicationIdentifier;
		private int m_maxConnections;
		private int m_port;

		/// <summary>
		/// Size of the send buffer; default 65536
		/// </summary>
		public int SendBufferSize
		{
			get { return m_sendBufferSize; }
			set
			{
				if (m_isLocked)
					throw new NetException("Can't change SendBufferSize after creating NetClient/NetServer");
				m_sendBufferSize = value;
			}
		}

		/// <summary>
		/// Size of the receive buffer; default 65536
		/// </summary>
		public int ReceiveBufferSize
		{
			get { return m_receiveBufferSize; }
			set
			{
				if (m_isLocked)
					throw new NetException("Can't change ReceiveBufferSize after creating NetClient/NetServer");
				m_receiveBufferSize = value;
			}
		}

		/// <summary>
		/// Identifier for this application; differentiating it from other Lidgren.Library.Network apps
		/// </summary>
		public string ApplicationIdentifier
		{
			get { return m_applicationIdentifier; }
			set { m_applicationIdentifier = value; }
		}

		/// <summary>
		/// Local port to bind to
		/// </summary>
		public int Port
		{
			get { return m_port; }
			set
			{
				if (m_isLocked)
					throw new NetException("Can't change Port after creating NetClient/NetServer");
				m_port = value;
			}
		}

		/// <summary>
		/// Encoding used by NetMessage.Write(string)
		/// </summary>
		public Encoding StringEncoding;

		/// <summary>
		/// Maximum number of bytes to send in a single packet
		/// </summary>
		public int MaximumTransmissionUnit;

		/// <summary>
		/// How many bytes to allocate in new NetMessages by default
		/// </summary>
		public int DefaultNetMessageBufferSize;

		/// <summary>
		/// Server only: Maximum number of connections allowed
		/// </summary>
		public int MaximumConnections
		{
			get { return m_maxConnections; }
			set
			{
				if (m_isLocked)
					throw new NetException("Can't change MaximumConnections after creating NetClient/NetServer");
				m_maxConnections = value;
			}
		}

		/// <summary>
		/// Server name reported by local server discovery
		/// </summary>
		public string ServerName;

		private bool m_usesEncryption;
		private string m_serverPublicKey;
		private byte[] m_pubKeyBytes;
		private string m_serverPrivateKey;
		private byte[] m_privKeyBytes;

		public string ServerPublicKey { get { return m_serverPublicKey; } }
		public byte[] ServerPublicKeyBytes { get { return m_pubKeyBytes; } }
		public string ServerPrivateKey { get { return m_serverPrivateKey; } }
		public byte[] ServerPrivateKeyBytes { get { return m_privKeyBytes; } }

		/// <summary>
		/// Gets whether encryption is used or not
		/// </summary>
		public bool UsesEncryption { get { return m_usesEncryption; } }

		public void EnableEncryption(string publicKey, string privateKey)
		{
			m_usesEncryption = true;
			m_serverPublicKey = publicKey;
			m_serverPrivateKey = privateKey;
			m_pubKeyBytes = Convert.FromBase64String(m_serverPublicKey);
			if (m_serverPrivateKey != null)
				m_privKeyBytes = Convert.FromBase64String(m_serverPrivateKey);
		}

		/// <summary>
		/// Application-wide network configuration
		/// </summary>
		public NetAppConfiguration(string applicationIdentifier, int port)
		{
			Init(applicationIdentifier, port);
		}

		/// <summary>
		/// Application-wide network configuration
		/// </summary>
		public NetAppConfiguration(string applicationIdentifier)
		{
			Init(applicationIdentifier, 0);
		}

		private void Init(string applicationIdentifier, int port)
		{
			//
			// default settings
			//
			m_isLocked = false;

			SendBufferSize = 65536;
			ReceiveBufferSize = 65536;
			ApplicationIdentifier = applicationIdentifier;
			Port = port;
			DefaultNetMessageBufferSize = 16;

			StringEncoding = Encoding.ASCII;
			MaximumTransmissionUnit = 1459;

			// server only
			MaximumConnections = -1;
			ServerName = NetConstants.DefaultServerName;
		}
	}
}
