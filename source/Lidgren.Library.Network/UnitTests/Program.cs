using System;
using System.Collections.Generic;
using System.Text;

using Lidgren.Library.Network;
using System.Net;

//
// very simple unit tests
//
namespace UnitTests
{
	class Program
	{
		static void Main(string[] args)
		{
			// need netbase context
			NetAppConfiguration config = new NetAppConfiguration("unittest", 12345);
			NetLog log = new NetLog();
			NetClient client = new NetClient(config, log);

			NetMessage msg = new NetMessage();

			msg.Write((short)short.MaxValue);
			msg.Write((short)short.MinValue);
			msg.Write((short)-42);

			msg.Write(421);
			msg.Write((byte)7);
			msg.Write(-42.8f);
			msg.Write("duke of earl");
			msg.Write((uint)9991);

			// byte boundary kept until here

			msg.Write(true);
			msg.Write((uint)3, 5);
			msg.Write(8.111f);
			msg.Write("again");
			byte[] arr = new byte[] { 1, 6, 12, 24 };
			msg.Write(arr);
			msg.Write((byte)7, 7);
			msg.Write(Int32.MinValue);
			msg.Write(UInt32.MaxValue);
			msg.WriteRangedSingle(21.0f, -10, 50, 12);

			// test reduced bit signed writing
			msg.Write(15, 5);
			msg.Write(2, 5);
			msg.Write(0, 5);
			msg.Write(-1, 5);
			msg.Write(-2, 5);
			msg.Write(-15, 5);

			msg.Write(UInt64.MaxValue);
			msg.Write(Int64.MaxValue);
			msg.Write(Int64.MinValue);

			msg.Write(42);

			// verify
			msg.ResetReadPointer();

			short a = msg.ReadInt16();
			short b = msg.ReadInt16();
			short c = msg.ReadInt16();

			if (a != short.MaxValue || b != short.MinValue || c != -42)
				throw new Exception("Ack thpth short failed");

			if (msg.ReadInt32() != 421)
				throw new Exception("Ack thphth 1");
			if (msg.ReadByte() != (byte)7)
				throw new Exception("Ack thphth 2");
			if (msg.ReadSingle() != -42.8f)
				throw new Exception("Ack thphth 3");
			if (msg.ReadString() != "duke of earl")
				throw new Exception("Ack thphth 4");

			if (msg.ReadUInt32() != 9991)
				throw new Exception("Ack thphth 4.5");

			if (msg.ReadBoolean() != true)
				throw new Exception("Ack thphth 5");
			if (msg.ReadUInt32(5) != (uint)3)
				throw new Exception("Ack thphth 6");
			if (msg.ReadSingle() != 8.111f)
				throw new Exception("Ack thphth 7");
			if (msg.ReadString() != "again")
				throw new Exception("Ack thphth 8");
			byte[] rrr = msg.ReadBytes(4);
			if (rrr[0] != arr[0] || rrr[1] != arr[1] || rrr[2] != arr[2] || rrr[3] != arr[3])
				throw new Exception("Ack thphth 9");
			if (msg.ReadByte(7) != 7)
				throw new Exception("Ack thphth 10");
			if (msg.ReadInt32() != Int32.MinValue)
				throw new Exception("Ack thphth 11");
			if (msg.ReadUInt32() != UInt32.MaxValue)
				throw new Exception("Ack thphth 12");

			float v = msg.ReadRangedSingle(-10, 50, 12);
			// v should be close to, but not necessarily exactly, 21.0f
			if ((float)Math.Abs(21.0f - v) > 0.1f)
				throw new Exception("Ack thphth *RangedSingle() failed");

			if (msg.ReadInt32(5) != 15)
				throw new Exception("Ack thphth signed reduced bit 1");
			if (msg.ReadInt32(5) != 2)
				throw new Exception("Ack thphth signed reduced bit 2");
			if (msg.ReadInt32(5) != 0)
				throw new Exception("Ack thphth signed reduced bit 3");
			if (msg.ReadInt32(5) != -1)
				throw new Exception("Ack thphth signed reduced bit 4");
			if (msg.ReadInt32(5) != -2)
				throw new Exception("Ack thphth signed reduced bit 5");
			if (msg.ReadInt32(5) != -15)
				throw new Exception("Ack thphth signed reduced bit 6");

			UInt64 longVal = msg.ReadUInt64();
			if (longVal != UInt64.MaxValue)
				throw new Exception("Ack thphth UInt64");
			if (msg.ReadInt64() != Int64.MaxValue)
				throw new Exception("Ack thphth Int64");
			if (msg.ReadInt64() != Int64.MinValue)
				throw new Exception("Ack thphth Int64");

			if (msg.ReadInt32() != 42)
				throw new Exception("Ack thphth end");

			Console.WriteLine("All's well");
			Console.ReadKey();
		}
	}
}
