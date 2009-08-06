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
	public sealed class NetConnectRequestEventArgs : EventArgs
	{
		/// <summary>
		/// Remote endpoint of client wishing to connect
		/// </summary>
		public IPEndPoint EndPoint;

		/// <summary>
		/// Custom data sent by the client wishing to connect
		/// </summary>
		public byte[] CustomData;

		/// <summary>
		/// Set this to false to disallow this connection
		/// </summary>
		public bool MayConnect = true;

		/// <summary>
		/// Set this to a string if you want to supply a reason for denial
		/// If no reason is set, no response will be sent and the connection attempt
		/// silently dropped
		/// </summary>
		public string DenialReason;
	}
}
