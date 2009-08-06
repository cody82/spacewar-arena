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
using System.Diagnostics;

namespace Lidgren.Library.Network
{
	internal partial class NetBuffer
	{
		public byte[] Data;

		private int m_bitLength;
		private int m_readBitPtr;

		public int LengthBytes { get { return (m_bitLength >> 3) + ((m_bitLength & 7) > 0 ? 1 : 0); } }
		public int LengthBits { get { return m_bitLength; } set { m_bitLength = value; } }

		public void ResetReadPointer() { m_readBitPtr = 0; }
		public void ResetWritePointer() { m_bitLength = 0; }

		public int ReadBitsLeft { get { return m_bitLength - m_readBitPtr; } }

		internal NetBuffer()
		{
			// Data to be set later
		}

		internal void SetDataLength(int numBytes)
		{
			m_bitLength = numBytes * 8;
		}

		public NetBuffer(int initialSizeInBytes)
		{
			Data = new byte[initialSizeInBytes];
		}

		public NetBuffer(byte[] fromBuffer)
		{
			Data = fromBuffer;
			m_bitLength = (fromBuffer == null ? 0 : fromBuffer.Length * 8);
		}

		private void EnsureSizeWrite(int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
		}

		public void EnsureBufferSize(int numberOfBits)
		{
			int byteLen = (numberOfBits >> 3) + ((numberOfBits & 7) > 0 ? 1 : 0);
			if (Data == null)
			{
				Data = new byte[byteLen + 4]; // overallocate 4 bytes
				return;
			}
			if (Data.Length < byteLen)
				Array.Resize<byte>(ref Data, byteLen + 4); // overallocate 4 bytes
			return;
		}

		internal byte[] ToArray()
		{
			int len = LengthBytes;
			byte[] copy = new byte[len];
			Array.Copy(Data, copy, copy.Length);
			return copy;
		}

		#region Write Methods

		public void Write(byte source)
		{
			EnsureSizeWrite(8);
			NetBitStreamUtil.Write(source, 8, Data, m_bitLength);
			m_bitLength += 8;
		}

		public void Write(byte[] source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			EnsureSizeWrite(source.Length * 8);

			NetBitStreamUtil.Write(source, 0, source.Length, Data, m_bitLength);
			m_bitLength += (source.Length * 8);
		}

		public void Write(byte source, int numberOfBits)
		{
			Debug.Assert((numberOfBits > 0 && numberOfBits <= 8), "Write(byte, numberOfBits) can only write between 1 and 8 bits");
			EnsureSizeWrite(numberOfBits);
			NetBitStreamUtil.Write(source, numberOfBits, Data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		public void Write(byte[] source, int offset, int numberOfBytes)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			EnsureSizeWrite(numberOfBytes * 8);

			NetBitStreamUtil.Write(source, offset, numberOfBytes, Data, m_bitLength);
			m_bitLength += (numberOfBytes * 8);
		}

		public void Write(ushort source)
		{
			EnsureSizeWrite(16);
			NetBitStreamUtil.Write((uint)source, 16, Data, m_bitLength);
			m_bitLength += 16;
		}

		public void Write(short source)
		{
			EnsureSizeWrite(16);
			NetBitStreamUtil.Write((uint)source, 16, Data, m_bitLength);
			m_bitLength += 16;
		}

		public void Write(bool y)
		{
			EnsureSizeWrite(1);
			NetBitStreamUtil.Write((y ? (byte)1 : (byte)0), 1, Data, m_bitLength);
			m_bitLength += 1;
		}

		public unsafe void Write(Int32 source)
		{
			EnsureSizeWrite(32);
			
			// can write fast?
			if (m_bitLength % 8 == 0)
			{
				fixed (byte* numRef = &Data[m_bitLength / 8])
				{
					*((int*)numRef) = source;
				}
			}
			else
			{
				NetBitStreamUtil.Write((UInt32)source, 32, Data, m_bitLength);
			}
			m_bitLength += 32;
		}

		public unsafe void Write(UInt32 source)
		{
			EnsureSizeWrite(32);

			// can write fast?
			if (m_bitLength % 8 == 0)
			{
				fixed (byte* numRef = &Data[m_bitLength / 8])
				{
					*((uint*)numRef) = source;
				}
			}
			else
			{
				NetBitStreamUtil.Write(source, 32, Data, m_bitLength);
			}
	
			m_bitLength += 32;
		}

		public unsafe void Write(double source)
		{
			Write(*((ulong*)&source));
		}

		public unsafe void Write(float source)
		{
			Write(*((int*)&source));
		}

		public void Write(ulong source)
		{
			EnsureSizeWrite(64);
			NetBitStreamUtil.Write(source, 64, Data, m_bitLength);
			m_bitLength += 64;
		}

		public void Write(Int64 source)
		{
			EnsureSizeWrite(64);
			ulong usource = (ulong)source;
			NetBitStreamUtil.Write(usource, 64, Data, m_bitLength);
			m_bitLength += 64;
		}

		public void Write(ushort source, int numberOfBits)
		{
			Debug.Assert((numberOfBits > 0 && numberOfBits <= 16), "Write(ushort, numberOfBits) can only write between 1 and 16 bits");
			EnsureSizeWrite(numberOfBits);
			NetBitStreamUtil.Write((uint)source, numberOfBits, Data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		public void Write(uint source, int numberOfBits)
		{
			Debug.Assert((numberOfBits > 0 && numberOfBits <= 32), "Write(uint, numberOfBits) can only write between 1 and 32 bits");
			EnsureSizeWrite(numberOfBits);
			NetBitStreamUtil.Write(source, numberOfBits, Data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		public void Write(int source, int numberOfBits)
		{
			Debug.Assert((numberOfBits > 0 && numberOfBits <= 32), "Write(int, numberOfBits) can only write between 1 and 32 bits");
			EnsureSizeWrite(numberOfBits);

			if (numberOfBits != 32)
			{
				// make first bit sign
				int signBit = 1 << (numberOfBits - 1);
				if (source < 0)
					source = (-source - 1) | signBit;
				else
					source &= (~signBit);
			}

			NetBitStreamUtil.Write((uint)source, numberOfBits, Data, m_bitLength);

			m_bitLength += numberOfBits;
		}

		public void Write(string str)
		{
			NetAppConfiguration config = NetBase.CurrentContext.Configuration;

			if (string.IsNullOrEmpty(str))
			{
				Write7BitEncodedUInt(0);
				return;
			}

			byte[] bytes = config.StringEncoding.GetBytes(str);

			Write7BitEncodedUInt((uint)bytes.Length);
			Write(bytes);
		}

		#endregion

		#region Read Methods
	
		public byte ReadByte()
		{
			//Debug.Assert(m_bitLength - m_readBitPtr >= 8, "tried to read past buffer size");
			byte retval = NetBitStreamUtil.Read(Data, 8, m_readBitPtr);
			m_readBitPtr += 8;
			return retval;
		}

		public bool ReadBoolean()
		{
			//Debug.Assert(m_bitLength - m_readBitPtr >= 1, "tried to read past buffer size");
			byte retval = NetBitStreamUtil.Read(Data, 1, m_readBitPtr);
			m_readBitPtr += 1;
			return (retval > 0 ? true : false);
		}

		public byte ReadByte(int numberOfBits)
		{
			Debug.Assert((numberOfBits > 0 && numberOfBits <= 8), "ReadByte() can only read between 1 and 8 bits");
			//Debug.Assert(m_bitLength - m_readBitPtr >= numberOfBits, "tried to read past buffer size");

			byte retval = NetBitStreamUtil.Read(Data, numberOfBits, m_readBitPtr);
			m_readBitPtr += numberOfBits;
			return retval;
		}

		public byte[] ReadBytes(int numberOfBytes)
		{
			byte[] retval = new byte[numberOfBytes];
			NetBitStreamUtil.ReadBytes(Data, numberOfBytes, m_readBitPtr, retval, 0);
			m_readBitPtr += (8 * numberOfBytes);
			return retval;
		}

		public short ReadInt16()
		{
			Debug.Assert(m_bitLength - m_readBitPtr >= 16, "tried to read past buffer size");
			uint retval = NetBitStreamUtil.ReadUInt32(Data, 16, m_readBitPtr);
			m_readBitPtr += 16;
			return (short)retval;
		}
		
		public ushort ReadUInt16()
		{
			Debug.Assert(m_bitLength - m_readBitPtr >= 16, "tried to read past buffer size");
			uint retval = NetBitStreamUtil.ReadUInt32(Data, 16, m_readBitPtr);
			m_readBitPtr += 16;
			return (ushort)retval;
		}

		public Int32 ReadInt32()
		{
			Debug.Assert(m_bitLength - m_readBitPtr >= 32, "tried to read past buffer size");
			uint retval = NetBitStreamUtil.ReadUInt32(Data, 32, m_readBitPtr);
			m_readBitPtr += 32;
			return (Int32)retval;
		}

		public int ReadInt32(int numberOfBits)
		{
			Debug.Assert((numberOfBits > 0 && numberOfBits <= 32), "ReadInt() can only read between 1 and 32 bits");
			Debug.Assert(m_bitLength - m_readBitPtr >= numberOfBits, "tried to read past buffer size");

			uint retval = NetBitStreamUtil.ReadUInt32(Data, numberOfBits, m_readBitPtr);
			m_readBitPtr += numberOfBits;

			if (numberOfBits == 32)
				return (int)retval;

			int signBit = 1 << (numberOfBits - 1);
			if ((retval & signBit) == 0)
				return (int)retval; // positive

			// negative
			unchecked
			{
				uint mask = ((uint)-1) >> (33 - numberOfBits);
				uint tmp = (retval & mask) + 1;
				return -((int)tmp);
			}
		}

		public UInt32 ReadUInt32()
		{
			uint retval = NetBitStreamUtil.ReadUInt32(Data, 32, m_readBitPtr);
			m_readBitPtr += 32;
			return retval;
		}

		public uint ReadUInt32(int numberOfBits)
		{
			Debug.Assert((numberOfBits > 0 && numberOfBits <= 32), "ReadUInt() can only read between 1 and 32 bits");
			//Debug.Assert(m_bitLength - m_readBitPtr >= numberOfBits, "tried to read past buffer size");

			uint retval = NetBitStreamUtil.ReadUInt32(Data, numberOfBits, m_readBitPtr);
			m_readBitPtr += numberOfBits;
			return retval;
		}

		public float ReadSingle()
		{
			return ReadFloat();
		}

		public float ReadFloat()
		{
			Debug.Assert(m_bitLength - m_readBitPtr >= (4 * 8), "tried to read past buffer size");
			byte[] bytes = ReadBytes(4);
			return BitConverter.ToSingle(bytes, 0);
		}

		public UInt64 ReadUInt64()
		{
			Debug.Assert(m_bitLength - m_readBitPtr >= 64, "tried to read past buffer size");

			ulong low = NetBitStreamUtil.ReadUInt32(Data, 32, m_readBitPtr);
			m_readBitPtr += 32;
			ulong high = NetBitStreamUtil.ReadUInt32(Data, 32, m_readBitPtr);

			ulong retval = low + (high << 32);

			m_readBitPtr += 32;
			return retval;
		}

		public Int64 ReadInt64()
		{
			Debug.Assert(m_bitLength - m_readBitPtr >= 64, "tried to read past buffer size");
			unchecked
			{
				ulong retval = ReadUInt64();
				long longRetval = (long)retval;
				return longRetval;
			}
		}

		public UInt64 ReadUInt64(int numberOfBits)
		{
			Debug.Assert((numberOfBits > 0 && numberOfBits <= 64), "ReadUInt() can only read between 1 and 64 bits");
			Debug.Assert(m_bitLength - m_readBitPtr >= numberOfBits, "tried to read past buffer size");

			ulong retval;
			if (numberOfBits <= 32)
			{
				retval = (ulong)NetBitStreamUtil.ReadUInt32(Data, numberOfBits, m_readBitPtr);
			}
			else
			{
				retval = NetBitStreamUtil.ReadUInt32(Data, 32, m_readBitPtr);
				retval |= NetBitStreamUtil.ReadUInt32(Data, numberOfBits - 32, m_readBitPtr) << 32;
			}
			m_readBitPtr += numberOfBits;
			return retval;
		}

		public Int64 ReadInt64(int numberOfBits)
		{
			Debug.Assert(((numberOfBits > 0) && (numberOfBits < 65)), "ReadInt64(bits) can only read between 1 and 64 bits");
			return (long)ReadUInt64(numberOfBits);
		}

		public string ReadString()
		{
			NetAppConfiguration config=null;
            if(NetBase.CurrentContext!=null)
                config = NetBase.CurrentContext.Configuration;

			int byteLen = (int)Read7BitEncodedUInt();

            Encoding enc = Encoding.ASCII;
            if(config!=null)
                enc = config.StringEncoding;

			// verify we have enough data
			if (m_readBitPtr + (byteLen * 8) > this.LengthBits)
			{
				int rem = (this.LengthBits - m_readBitPtr) / 8;
				throw new IndexOutOfRangeException("ReadString() tried to read " + byteLen + " bytes; but remainder of message only has " + rem + " bytes left");
			}

			byte[] bytes = ReadBytes(byteLen);

			return enc.GetString(bytes, 0, bytes.Length);
		}

		#endregion

		#region Peek Methods
		
		public byte PeekByte()
		{
			return NetBitStreamUtil.Read(Data, 8, m_readBitPtr);
		}

		public byte PeekByte(int numberOfBits)
		{
			Debug.Assert(((numberOfBits > 0) && (numberOfBits < 9)), "PeekByte(bits) can only read between 1 and 8 bits");
			return NetBitStreamUtil.Read(Data, numberOfBits, m_readBitPtr);
		}

		public uint PeekUInt32()
		{
			return NetBitStreamUtil.ReadUInt32(Data, 32, m_readBitPtr);
		}

		public uint PeekUInt32(int numberOfBits)
		{
			Debug.Assert(((numberOfBits > 0) && (numberOfBits < 33)), "PeekUInt32(bits) can only read between 1 and 32 bits");
			return NetBitStreamUtil.ReadUInt32(Data, numberOfBits, m_readBitPtr);
		}

		public ushort PeekUInt16()
		{
			return (ushort)NetBitStreamUtil.ReadUInt32(Data, 16, m_readBitPtr);
		}

		public ushort PeekUInt16(int numberOfBits)
		{
			Debug.Assert(((numberOfBits > 0) && (numberOfBits < 17)), "PeekUInt32(bits) can only read between 1 and 16 bits");
			return (ushort)NetBitStreamUtil.ReadUInt32(Data, numberOfBits, m_readBitPtr);
		}

		#endregion
	}
}
