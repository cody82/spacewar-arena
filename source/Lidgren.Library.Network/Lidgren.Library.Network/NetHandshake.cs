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
	internal static class NetHandshake
	{
		internal static void HandleConnect(NetMessage connectMsg, NetServer server, IPEndPoint senderEndpoint)
		{
			NetMessage response;
			if (server.NumConnected >= server.Configuration.MaximumConnections)
			{
				// server full
				response = new NetMessage(NetMessageType.Handshake, "Server full".Length + 1);
				response.Write((byte)NetHandshakeType.Disconnected);
				response.Write("Server full");
				server.SendSingleMessageAtOnce(response, null, senderEndpoint);
				return;
			}

			ushort remoteNow = connectMsg.ReadUInt16();
			//server.Log.Debug("Setting remote clock based on guess of 50 ms lag...");
			int remoteClockOffset = NetTime.CalculateOffset(NetTime.Now, remoteNow, 0.05); // assume 50ms...
			
			// read symmetric key
			int encSymKeyLen = connectMsg.ReadUInt16();
			byte[] encSymKey = null;
			if (encSymKeyLen > 0)
				encSymKey = connectMsg.ReadBytes(encSymKeyLen);

			// read custom data
			int cdLen = (int)connectMsg.Read7BitEncodedUInt();
			byte[] customData = null;
			if (cdLen > 0)
				customData = connectMsg.ReadBytes(cdLen);

			string failReason = null;
			bool ok = server.ApproveConnection(senderEndpoint, customData, out failReason);
			if (!ok)
			{
				if (!string.IsNullOrEmpty(failReason))
				{
					// send disconnect reason; unencrypted, client can handle it because status is connecting
					response = new NetMessage(NetMessageType.Handshake, failReason.Length + 3);
					response.Write((byte)NetHandshakeType.Disconnected);
					response.Write(failReason);
					server.SendSingleMessageAtOnce(response, null, senderEndpoint);
				}

				// connection not approved
				return;
			}

			NetConnection connection = server.AddConnection(senderEndpoint, remoteClockOffset);
			if (connection == null)
				return; // uh oh 

			if (encSymKeyLen > 0)
			{
				byte[] symKey = connection.m_encryption.DecryptRSA(server.Log, encSymKey);
				if (symKey == null)
				{
					// send disconnect unencrypted, client can handle it because status is connecting
					string bye = "RSA failed; are you using correct public key?";
					response = new NetMessage(NetMessageType.Handshake, bye.Length + 3);
					response.Write((byte)NetHandshakeType.Disconnected);
					response.Write(bye);
					server.SendSingleMessageAtOnce(response, null, senderEndpoint);

					server.Log.Warning("Failed to decrypt RSA encrypted symmetric key from " + senderEndpoint);
					return;
				}
				connection.m_encryption.SetSymmetricKey(symKey);

				server.Log.Debug("Received Connect containing key: " + Convert.ToBase64String(symKey));
			}
			else
			{
				if (server.Configuration.UsesEncryption)
				{
					server.Log.Warning("Client tried to connect without encryption from " + senderEndpoint);

					// denied
					response = new NetMessage(NetMessageType.Handshake, "Encryption required".Length + 1);
					response.Write((byte)NetHandshakeType.Disconnected);
					response.Write("Encryption required");
					server.SendSingleMessageAtOnce(response, null, senderEndpoint);
					return;
				}
				server.Log.Debug("Received Connect - using unencrypted connection!");
			}

			// send connect response
			int bytesSent = SendConnectResponse(server, connection, senderEndpoint);

			if (connection != null)
			{
				// account for connectresponse
				connection.Statistics.PacketsSent++;
				connection.Statistics.MessagesSent++;
				connection.Statistics.BytesSent += bytesSent;
			}
		}

		internal static int SendConnect(NetClient client, IPEndPoint remoteEndpoint, byte[] customData)
		{
			if (client.Configuration.UsesEncryption)
				client.Log.Debug("Sending Connect containing key: " + Convert.ToBase64String(client.ServerConnection.m_encryption.SymmetricEncryptionKeyBytes));
			else
				client.Log.Debug("Sending Connect - Unencrypted connection!");

			NetMessage msg = new NetMessage(NetMessageType.Handshake, 3);
			msg.Write((byte)NetHandshakeType.Connect);
			msg.WriteSendStamp();

			// encrypt symmetric key using server public key
			if (client.Configuration.UsesEncryption)
			{
				byte[] encryptedKey = client.ServerConnection.m_encryption.EncryptRSA(client.ServerConnection.m_encryption.SymmetricEncryptionKeyBytes);
				msg.Write((ushort)encryptedKey.Length);
				msg.Write(encryptedKey);
			}
			else
			{
				// request no encryption
				msg.Write((ushort)0);
			}

			if (customData == null || customData.Length < 1)
			{
				msg.Write7BitEncodedUInt(0);
			}
			else
			{
				msg.Write7BitEncodedUInt((uint)customData.Length);
				msg.Write(customData);
			}

			return client.SendSingleMessageAtOnce(msg, null, remoteEndpoint);
		}

		internal static int SendConnectResponse(NetBase netBase, NetConnection clientConnection, IPEndPoint remoteEndpoint)
		{
			netBase.Log.Debug("Sending ConnectResponse");
			double now = NetTime.Now;
			ushort nowEnc = NetTime.Encoded(now);
			NetMessage response = new NetMessage(NetMessageType.Handshake, 3);
			response.Write((byte)NetHandshakeType.ConnectResponse);
			response.Write(nowEnc);
			clientConnection.m_firstSentHandshake = now;
			clientConnection.m_lastSentHandshake = now;
			return netBase.SendSingleMessageAtOnce(response, clientConnection, remoteEndpoint);
		}

		internal static int SendDisconnected(NetBase netBase, string reason, NetConnection connection)
		{
			NetMessage bye = new NetMessage(NetMessageType.Handshake, reason.Length + 3);
			bye.Write((byte)NetHandshakeType.Disconnected);
			bye.Write(reason);
			return netBase.SendSingleMessageAtOnce(bye, connection, connection.RemoteEndpoint);
		}
	}
}
