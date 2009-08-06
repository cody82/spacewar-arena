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
	/// <summary>
	/// Helper class for Netmessage.Payload.cs
	/// </summary>
	internal static class NetBitStreamUtil
	{
		/// <summary>
		/// Read 1-8 bits from a buffer
		/// </summary>
		public static byte Read(byte[] fromBuffer, int numberOfBits, int readBitOffset)
		{
			Debug.Assert(((numberOfBits > 0) && (numberOfBits < 9)), "Read() can only read between 1 and 8 bits");

			int bytePtr = readBitOffset >> 3;
			int startReadAtIndex = (readBitOffset % 8);

			if (startReadAtIndex == 0 && numberOfBits == 8)
				return fromBuffer[bytePtr];

			// "mask" away unused bits lower than (right of) relevant bits in first byte
			byte returnValue = (byte)(fromBuffer[bytePtr] >> startReadAtIndex);

			int numberOfBitsInSecondByte = numberOfBits - (8 - startReadAtIndex);

			if (numberOfBitsInSecondByte < 1)
			{
				// we don't need to read from the second byte, but we DO need
				// to mask away unused bits higher than (left of) relevant bits
				returnValue &= (byte)(255 >> (8 - numberOfBits));
				return returnValue;
			}

			byte second = fromBuffer[bytePtr + 1];

			// mask away unused bits higher than (left of) relevant bits in second byte
			second &= (byte)(255 >> (8 - numberOfBitsInSecondByte));

			returnValue |= (byte)(second << (numberOfBits - numberOfBitsInSecondByte));

			return returnValue;
		}

		/// <summary>
		/// Read 1-32 bits from a buffer
		/// </summary>
		public unsafe static uint ReadUInt32(byte[] fromBuffer, int numberOfBits, int readBitOffset)
		{
			Debug.Assert(((numberOfBits > 0) && (numberOfBits <= 32)), "ReadUInt32() can only read between 1 and 32 bits");

			if (numberOfBits == 32 && ((readBitOffset % 8) == 0))
			{
				fixed (byte* ptr = &(fromBuffer[readBitOffset / 8]))
				{
					return *(((uint*)ptr));
				}
			}

			uint returnValue;
			if (numberOfBits <= 8)
			{
				returnValue = Read(fromBuffer, numberOfBits, readBitOffset);
				return returnValue;
			}
			returnValue = Read(fromBuffer, 8, readBitOffset);
			numberOfBits -= 8;
			readBitOffset += 8;

			if (numberOfBits <= 8)
			{
				returnValue |= (uint)(Read(fromBuffer, numberOfBits, readBitOffset) << 8);
				return returnValue;
			}
			returnValue |= (uint)(Read(fromBuffer, 8, readBitOffset) << 8);
			numberOfBits -= 8;
			readBitOffset += 8;

			if (numberOfBits <= 8)
			{
				uint r = Read(fromBuffer, numberOfBits, readBitOffset);
				r <<= 16;
				returnValue |= r;
				return returnValue;
			}
			returnValue |= (uint)(Read(fromBuffer, 8, readBitOffset) << 16);
			numberOfBits -= 8;
			readBitOffset += 8;

			returnValue |= (uint)(Read(fromBuffer, numberOfBits, readBitOffset) << 24);
			return returnValue;
		}

		/// <summary>
		/// Read several (whole) bytes from a buffer
		/// </summary>
		public static void ReadBytes(byte[] fromBuffer, int numberOfBytes, int readBitOffset, byte[] destination, int destinationByteOffset)
		{
			int firstPartLen = (readBitOffset % 8);
			int readPtr = readBitOffset >> 3;

			if (firstPartLen == 0)
			{
				for(int i=0;i<numberOfBytes;i++)
					destination[destinationByteOffset++] = fromBuffer[readPtr++];
				return;
			}

			int secondPartLen = 8 - firstPartLen;
			int secondMask = 255 >> secondPartLen;

			for(int i=0;i<numberOfBytes;i++)
			{
				// "mask" away unused bits lower than (right of) relevant bits in byte
				int b = fromBuffer[readPtr] >> firstPartLen;

				readPtr++;

				// mask away unused bits higher than (left of) relevant bits in second byte
				int second = fromBuffer[readPtr] & secondMask;

				// combine
				b |= second << secondPartLen;

				destination[destinationByteOffset++] = (byte)b;
			}

			return;
		}

		/// <summary>
		/// Write 1-8 bits to a buffer; assumes buffer is previously allocated
		/// </summary>
		public static void Write(byte source, int numberOfBits, byte[] destination, int destBitOffset)
		{
			Debug.Assert(((numberOfBits >= 1) && (numberOfBits <= 8)), "Must write between 1 and 8 bits!");

			// mask out unwanted bits in the source
			uint isrc = (uint)source & ((~(uint)0) >> (8 - numberOfBits));

			int bytePtr = destBitOffset >> 3;

			int localBitLen = (destBitOffset % 8);
			if (localBitLen == 0)
			{
				destination[bytePtr] = (byte)isrc;
				return;
			}

			destination[bytePtr] &= (byte)(255 >> (8 - localBitLen)); // clear before writing
			destination[bytePtr] |= (byte)(isrc << localBitLen); // write first half

			// need write into next byte?
			if (localBitLen + numberOfBits > 8)
			{
				destination[bytePtr + 1] &= (byte)(255 << localBitLen); // clear before writing
				destination[bytePtr + 1] |= (byte)(isrc >> (8 - localBitLen)); // write second half
			}

			return;
		}

		/// <summary>
		/// Write 1-32 bits to a buffer; assumes buffer is previously allocated
		/// </summary>
		public static int Write(uint source, int numberOfBits, byte[] destination, int destBitOffset)
		{
			int returnValue = destBitOffset + numberOfBits;
			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)source, numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)source, 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 8), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 8), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 16), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 16), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			NetBitStreamUtil.Write((byte)(source >> 24), numberOfBits, destination, destBitOffset);
			return returnValue;
		}

		/// <summary>
		/// Write 1-64 bits to a buffer; assumes buffer is previously allocated
		/// </summary>
		public static int Write(ulong source, int numberOfBits, byte[] destination, int destBitOffset)
		{
			int returnValue = destBitOffset + numberOfBits;
			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)source, numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)source, 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 8), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 8), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 16), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 16), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 24), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 24), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 32), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 32), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 40), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 40), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 48), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 48), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			if (numberOfBits <= 8)
			{
				NetBitStreamUtil.Write((byte)(source >> 56), numberOfBits, destination, destBitOffset);
				return returnValue;
			}
			NetBitStreamUtil.Write((byte)(source >> 56), 8, destination, destBitOffset);
			destBitOffset += 8;
			numberOfBits -= 8;

			return returnValue;
		}
		
		/// <summary>
		/// Write several (whole) bytes
		/// </summary>
		public static void Write(byte[] source, int sourceByteOffset, int numberOfBytes, byte[] destination, int destBitOffset)
		{
			int dstBytePtr = destBitOffset >> 3;
			int firstPartLen = (destBitOffset % 8);

			if (firstPartLen == 0)
			{
				// optimized; TODO: write 64 bit chunks if possible
				for (int i = 0; i < numberOfBytes; i++)
					destination[dstBytePtr++] = source[sourceByteOffset + i];
				return;
			}

			int lastPartLen = 8 - firstPartLen;

			for (int i = 0; i < numberOfBytes; i++)
			{
				byte src = source[sourceByteOffset + i];

				// write last part of this byte
				destination[dstBytePtr] &= (byte)(255 >> lastPartLen); // clear before writing
				destination[dstBytePtr] |= (byte)(src << firstPartLen); // write first half

				dstBytePtr++;

				// write first part of next byte
				destination[dstBytePtr] &= (byte)(255 << firstPartLen); // clear before writing
				destination[dstBytePtr] |= (byte)(src >> lastPartLen); // write second half
			}

			return;
		}

	}
}
