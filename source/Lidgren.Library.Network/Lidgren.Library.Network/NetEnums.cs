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
	/// Status for a connection
	/// </summary>
	public enum NetConnectionStatus
	{
		Disconnected,
		Connecting,
		Connected,
		Reconnecting,
		Disconnecting,
	}

	// 3 bits
	internal enum NetMessageType : int
	{
		None = 0, // No message; packet padding due to encryption
		User = 1, // Application message
		UserFragmented = 2, // Application message
		Acknowledge = 3,
		AcknowledgeBitField = 4, // Currently not implemented
		Handshake = 5, // (connect, connectresponse, connectestablished, disconnected (incl. server full))
		PingPong = 6, // (ping, pong, optimizeinfo)
		Discovery = 7, // (request, response)
	}

	internal enum NetHandshakeType : byte
	{
		Connect,
		ConnectResponse,
		ConnectionEstablished,
		Disconnected
	}

	/// <summary>
	/// Specifies if a connection should emphasize response or bandwidth
	/// </summary>
	public enum NetOptimization
	{
		EmphasizeResponse,
		Neutral,
		EmphasizeBandwidth
	}


	/// <summary>
	/// Specifies what guarantees a message send is given
	/// </summary>
	public enum NetChannel : byte
	{
		/// <summary>
		/// Messages are not guaranteed to arrive
		/// </summary>
		Unreliable = 0,

		/// <summary>
		/// Messages are not guaranteed to arrive, but out-of-order message, ie. late messages are dropped
		/// </summary>
		Sequenced1 = 1,
		Sequenced2 = 2,
		Sequenced3 = 3,
		Sequenced4 = 4,
		Sequenced5 = 5,
		Sequenced6 = 6,
		Sequenced7 = 7,
		Sequenced8 = 8,
		Sequenced9 = 9,
		Sequenced10 = 10,
		Sequenced11 = 11,
		Sequenced12 = 12,
		Sequenced13 = 13,
		Sequenced14 = 14,
		Sequenced15 = 15,

		/// <summary>
		/// Messages are guaranteed to arrive, but not necessarily in the same order as sent
		/// </summary>
		ReliableUnordered = 16,

		/// <summary>
		/// Messages are guaranteed to arrive, in the same order as they were sent
		/// </summary>
		Ordered1 = 17,
		Ordered2 = 18,
		Ordered3 = 19,
		Ordered4 = 20,
		Ordered5 = 21,
		Ordered6 = 22,
		Ordered7 = 23,
		Ordered8 = 24,
		Ordered9 = 25,
		Ordered10 = 26,
		Ordered11 = 27,
		Ordered12 = 28,
		Ordered13 = 29,
		Ordered14 = 30,
		Ordered15 = 31,
	}

}