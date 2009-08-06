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
	internal static class NetConstants
	{
		internal const string DefaultApplicationIdentifier = "Default";
		
		/// <summary>
		/// Default server name if none is set in NetAppConfiguration
		/// </summary>
		public const string DefaultServerName = "No name";

		internal const int NumSequenceChannels = 32;
		internal const int NumSequenceNumbers = 4096;
		internal const int NumKeptDuplicateNumbers = 2048; // per channel

		/// <summary>
		/// Maximum sequence difference for messages to be classified as Early
		/// </summary>
		public const int EarlyArrivalWindowSize = NumSequenceNumbers / 3;

		public const float SendHandshakeFrequency = 4.0f;
		public const float ConnectAttemptTimeout = 20.0f;
	}
}
