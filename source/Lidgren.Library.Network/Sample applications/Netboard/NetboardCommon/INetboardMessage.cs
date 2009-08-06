using System;
using System.Collections.Generic;
using System.Text;

using Lidgren.Library.Network;

namespace NetboardCommon
{
	public interface INetboardMessage
	{
		NetBoardTypes MessageType { get; }
		void Encode(NetMessage msg);
		void Decode(NetMessage msg);
	}
}
