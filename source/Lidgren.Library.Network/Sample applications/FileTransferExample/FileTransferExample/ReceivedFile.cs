using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Library.Network;

namespace FileTransferExample
{
	public class ReceivedFile
	{
		public byte[] Data;
		public int NumTotalPieces;
		public int NumPiecesReceived;
		public double FirstPieceReceived;

		public ReceivedFile(NetMessage msg)
		{
			FirstPieceReceived = NetTime.Now;

			int totalLen = msg.ReadInt32();			// write total length of data
			ushort pieceLen = msg.ReadUInt16();		// length of each piece (except probably the last piece)
			ushort pieceNr = msg.ReadUInt16();		// piece id
			int thisPieceLen = msg.Length - 8;

			NumTotalPieces = totalLen / pieceLen;
			if (NumTotalPieces * pieceLen < totalLen)
				NumTotalPieces++;

			Data = new byte[totalLen];

			// add this piece data
			int offset = pieceLen * pieceNr;
			byte[] pieceData = msg.ReadBytes(thisPieceLen);
			Array.Copy(pieceData, 0, Data, offset, pieceData.Length);
			NumPiecesReceived++;
		}

		public void AddPiece(NetMessage msg)
		{
			int totalLen = msg.ReadInt32();			// write total length of data
			ushort pieceLen = msg.ReadUInt16();		// length of each piece (except probably the last piece)
			ushort pieceNr = msg.ReadUInt16();		// piece id
			int thisPieceLen = msg.Length - 8;

			// add this piece data
			int offset = pieceLen * pieceNr;
			byte[] pieceData = msg.ReadBytes(thisPieceLen);
			Array.Copy(pieceData, 0, Data, 0, pieceData.Length);
			NumPiecesReceived++;
		}
	}
}
