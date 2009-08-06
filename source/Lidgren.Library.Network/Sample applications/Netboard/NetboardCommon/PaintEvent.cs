using System;
using System.Collections.Generic;
using System.Text;

using Lidgren.Library.Network;

namespace NetboardCommon
{
	/// <summary>
	/// Message passed from server to clients
	/// </summary>
	public class PaintEvent
	{
		public NetBoardTypes MessageType { get { return NetBoardTypes.PaintEvent; } }

		public byte FromWho;
		public uint FromX, FromY;
		public uint ToX, ToY;

		public void Encode(NetMessage msg)
		{
			msg.Write((byte)MessageType);
			msg.Write((byte)FromWho); // allowing up to 255 players
			msg.Write(FromX, 12); // 12 bits, allowing for 0 - 4095
			msg.Write(FromY, 12); // 12 bits, allowing for 0 - 4095
			msg.Write(ToX, 12); // 12 bits, allowing for 0 - 4095
			msg.Write(ToY, 12); // 12 bits, allowing for 0 - 4095
		}

		public void Decode(NetMessage msg)
		{
			// we assume we've stepped past the NetBoardTypes byte here
			FromWho = msg.ReadByte();
			FromX = msg.ReadUInt32(12);
			FromY = msg.ReadUInt32(12);
			ToX = msg.ReadUInt32(12);
			ToY = msg.ReadUInt32(12);
		}
	}
}
