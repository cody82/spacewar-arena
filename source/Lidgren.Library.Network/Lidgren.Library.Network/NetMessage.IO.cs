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
	public sealed partial class NetMessage
	{
		/// <summary>
		/// Total length in bytes
		/// </summary>
		public int Length { get { return m_buffer.LengthBytes; } }

		/// <summary>
		/// Reset the read pointer of the message to the beginning
		/// </summary>
		public void ResetReadPointer() { m_buffer.ResetReadPointer(); }

		public void Write(bool val) { m_buffer.Write(val); }
		public void Write(short val) { m_buffer.Write(val); }
		public void Write(ushort val) { m_buffer.Write(val); }
		public void Write(uint val) { m_buffer.Write(val); }
		public void Write(int val) { m_buffer.Write(val); }
		public void Write(byte val) { m_buffer.Write(val); }
		public void Write(byte[] val) { m_buffer.Write(val); }
		public void Write(byte[] val, int offset, int numberOfBytes) { m_buffer.Write(val, offset, numberOfBytes); }
		public void Write(string val) { m_buffer.Write(val); }
		public void Write(float val) { m_buffer.Write(val); }
		public void Write(ulong val) { m_buffer.Write(val); }
		public void Write(long val) { m_buffer.Write(val); }

		/// <summary>
		/// Write an unsigned integer using 1 - 32 number of bits
		/// </summary>
		public void Write(uint val, int numberOfBits) { m_buffer.Write(val, numberOfBits); }

		/// <summary>
		/// Write an signed integer using 1 - 32 number of bits; using one of the bits as sign
		/// </summary>
		public void Write(int val, int numberOfBits) { m_buffer.Write(val, numberOfBits); }

		/// <summary>
		/// Write a byte using 1 - 8 number of bits
		/// </summary>
		public void Write(byte val, int numberOfBits) { m_buffer.Write(val, numberOfBits); }

		/// <summary>
		/// Write a float in the range of -1 .. 1 using 1 - 32 bits
		/// </summary>
		public void WriteSignedSingle(float val, int numberOfBits) { m_buffer.WriteSignedSingle(val, numberOfBits); }

		/// <summary>
		/// Write a float in the range of 0 .. 1 using 1 - 32 bits
		/// </summary>
		public void WriteUnitSingle(float val, int numberOfBits) { m_buffer.WriteUnitSingle(val, numberOfBits); }

		/// <summary>
		/// Compress a float within a specified range using a certain number of bits
		/// </summary>
		public void WriteRangedSingle(float val, float min, float max, int numberOfBits)
		{
			m_buffer.WriteRangedSingle(val, min, max, numberOfBits);
		}

		/// <summary>
		/// Reads a float written using WriteRangedSingle() using the same MIN and MAX values
		/// </summary>
		public float ReadRangedSingle(float min, float max, int numberOfBits)
		{
			return m_buffer.ReadRangedSingle(min, max, numberOfBits);
		}
		
		public void WriteStringTable(NetConnection connection, string val)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");
			connection.m_stringTable.Write(this, val);
		}

		/// <summary>
		/// Writes a unit vector using the supplied number of bits
		/// </summary>
		public void WriteNormal(float x, float y, float z, int numberOfBits)
		{
			double invPi = 1.0 / Math.PI;
			float phi = (float)(Math.Atan2(x, y) * invPi);
			float theta = (float)(Math.Atan2(z, Math.Sqrt(x*x + y*y)) * (invPi * 2));

			int halfBits = numberOfBits / 2;
			m_buffer.WriteSignedSingle(phi, halfBits);
			m_buffer.WriteSignedSingle(theta, numberOfBits - halfBits);
		}

		/// <summary>
		/// Write an unsigned integer using as few bytes as possible
		/// </summary>
		public void Write7BitEncodedUInt(UInt32 val)
		{
			m_buffer.Write7BitEncodedUInt(val);
		}

		/// <summary>
		/// Read a compressed unit vector
		/// </summary>
		public void ReadNormal(int numberOfBits, out float x, out float y, out float z)
		{
			int halfBits = numberOfBits / 2;
			float phi = m_buffer.ReadSignedSingle(halfBits) * (float)Math.PI;
			float theta = m_buffer.ReadSignedSingle(numberOfBits - halfBits) * (float)(Math.PI * 0.5);

			x = (float)(Math.Sin(phi) * Math.Cos(theta));
			y = (float)(Math.Cos(phi) * Math.Cos(theta));
			z = (float)Math.Sin(theta);
		}

		/// <summary>
		/// Write the current time
		/// </summary>
		public void WriteSendStamp()
		{
			m_buffer.Write(NetTime.NowEncoded);
		}

		/// <summary>
		/// Reads the next byte without incrementing the read pointer of the message
		/// </summary>
		public byte PeekByte() { return m_buffer.PeekByte(); }

		/// <summary>
		/// Reads the next 1-8 bits without incrementing the read pointer of the message
		/// </summary>
		public byte PeekByte(int numberOfBits) { return m_buffer.PeekByte(numberOfBits); }

		/// <summary>
		/// Reads the next unsigned integer without incrementing the read pointer of the message
		/// </summary>
		public uint PeekUInt32() { return m_buffer.PeekUInt32(); }

		/// <summary>
		/// Reads the next 1-32 bits without incrementing the read pointer of the message
		/// </summary>
		public uint PeekUInt32(int numberOfBits) { return m_buffer.PeekUInt32(numberOfBits); }
		
		/// <summary>
		/// Reads the next unsigned short without incrementing the read pointer of the message
		/// </summary>
		public ushort PeekUInt16() { return m_buffer.PeekUInt16(); }

		/// <summary>
		/// Reads the next unsigned short without incrementing the read pointer of the message
		/// </summary>
		public ushort PeekUInt16(int numberOfBits) { return m_buffer.PeekUInt16(numberOfBits); }

		// 1
		public bool ReadBoolean() { return m_buffer.ReadBoolean(); }

		// 1-8
		public byte ReadByte() { return m_buffer.ReadByte(); }
		public byte ReadByte(int numberOfBits) { return m_buffer.ReadByte(numberOfBits); }

		// 1-16
		public Int16 ReadInt16() { return m_buffer.ReadInt16(); }
		public UInt16 ReadUInt16() { return m_buffer.ReadUInt16(); }

		// 1-32
		public UInt32 ReadUInt32() { return m_buffer.ReadUInt32(); }
		public UInt32 ReadUInt32(int numberOfBits) { return m_buffer.ReadUInt32(numberOfBits); }
		public int ReadInt() { return m_buffer.ReadInt32(); }
		public int ReadInt32() { return m_buffer.ReadInt32(); }
		public int ReadInt(int numberOfBits) { return m_buffer.ReadInt32(numberOfBits); }
		public int ReadInt32(int numberOfBits) { return m_buffer.ReadInt32(numberOfBits); }

		// 1-64
		public UInt64 ReadUInt64() { return m_buffer.ReadUInt64(); }
		public UInt64 ReadUInt64(int numberOfBits) { return m_buffer.ReadUInt64(numberOfBits); }
		public Int64 ReadInt64() { return m_buffer.ReadInt64(); }
		public Int64 ReadInt64(int numberOfBits) { return m_buffer.ReadInt64(numberOfBits); }

		public float ReadSingle() { return m_buffer.ReadSingle(); }

		// 1-x
		public string ReadString() { return m_buffer.ReadString(); }
		public byte[] ReadBytes(int numberOfBytes) { return m_buffer.ReadBytes(numberOfBytes); }

		/// <summary>
		/// Reads a float in the range -1 to 1 written using WriteSignedSingle()
		/// </summary>
		public float ReadSignedSingle(int numberOfBits) { return m_buffer.ReadSignedSingle(numberOfBits); }

		/// <summary>
		/// Reads a float in the range 0 to 1 written using WriteUnitSingle()
		/// </summary>
		public float ReadUnitSingle(int numberOfBits) { return m_buffer.ReadUnitSingle(numberOfBits); }

		public string ReadStringTable(NetConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");
			return connection.m_stringTable.Read(this);
		}

		/// <summary>
		/// Reads an unsigned integer written using Write7BitEncodedUInt
		/// </summary>
		public UInt32 Read7BitEncodedUInt()
		{
			return m_buffer.Read7BitEncodedUInt();
		}

		/// <summary>
		/// Reads a timestamp written using WriteSendStamp()
		/// </summary>
		public double ReadSentStamp(NetConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");
			ushort val = m_buffer.ReadUInt16();
			int adjustRemoteMillis;
			float retval = NetTime.FromEncoded((float)NetTime.Now, connection.RemoteClockOffset, val, out adjustRemoteMillis);
			//if (adjustRemoteMillis != 0)
			//	connection.RemoteClockOffset = (connection.RemoteClockOffset + adjustRemoteMillis) % ushort.MaxValue;
			return retval;
		}

		/// <summary>
		/// Returns a copy of all bytes held in message payload
		/// </summary>
		public byte[] ToArray()
		{
			return m_buffer.ToArray();
		}
	}
}
