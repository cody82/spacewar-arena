using System;
using System.Collections.Generic;
using System.Text;

namespace Lidgren.Library.Network
{
	internal sealed class XTEA
	{
		private const int m_blockSize = 8;
		private const int m_keySize = 16;
		private const int m_delta = unchecked((int)0x9E3779B9);
		private const int m_dSum = unchecked((int)0xC6EF3720); // sum on decrypt

		private byte[] m_keyBytes;
		private int[] m_key;
		private int m_rounds;

		public byte[] Key { get { return m_keyBytes; } }

		/// <summary>
		/// 16 byte key
		/// </summary>
		public XTEA(byte[] key, int rounds)
		{
			m_keyBytes = key;
			m_key = new int[4];
			m_key[0] = BitConverter.ToInt32(key, 0);
			m_key[1] = BitConverter.ToInt32(key, 4);
			m_key[2] = BitConverter.ToInt32(key, 8);
			m_key[3] = BitConverter.ToInt32(key, 12);
			m_rounds = rounds;
		}

		public void EncryptBlock(
			byte[] inBytes,
			int inOff,
			byte[] outBytes,
			int outOff)
		{
			// Pack bytes into integers
			int v0 = BytesToInt(inBytes, inOff);
			int v1 = BytesToInt(inBytes, inOff + 4);

			int sum = 0;

			for (int i = 0; i != m_rounds; i++)
			{
				v0 += ((v1 << 4 ^ (int)((uint)v1 >> 5)) + v1) ^ (sum + m_key[sum & 3]);
				sum += m_delta;
				v1 += ((v0 << 4 ^ (int)((uint)v0 >> 5)) + v0) ^ (sum + m_key[(int)((uint)sum >> 11) & 3]);
			}

			UnpackInt(v0, outBytes, outOff);
			UnpackInt(v1, outBytes, outOff + 4);

			return;
		}

		public void DecryptBlock(
			byte[] inBytes,
			int inOff,
			byte[] outBytes,
			int outOff)
		{
			// Pack bytes into integers
			int v0 = BytesToInt(inBytes, inOff);
			int v1 = BytesToInt(inBytes, inOff + 4);

			int sum = m_dSum;

			for (int i = 0; i != m_rounds; i++)
			{
				v1 -= ((v0 << 4 ^ (int)((uint)v0 >> 5)) + v0) ^ (sum + m_key[(int)((uint)sum >> 11) & 3]);
				sum -= m_delta;
				v0 -= ((v1 << 4 ^ (int)((uint)v1 >> 5)) + v1) ^ (sum + m_key[sum & 3]);
			}

			UnpackInt(v0, outBytes, outOff);
			UnpackInt(v1, outBytes, outOff + 4);

			return;
		}

		private static int BytesToInt(byte[] b, int inOff)
		{
			//return BitConverter.ToInt32(b, inOff);
			return ((b[inOff++]) << 24) |
					((b[inOff++] & 255) << 16) |
					((b[inOff++] & 255) << 8) |
					((b[inOff] & 255));
		}

		private static void UnpackInt(
			int v,
			byte[] b,
			int outOff)
		{
			uint uv = (uint)v;
			b[outOff++] = (byte)(uv >> 24);
			b[outOff++] = (byte)(uv >> 16);
			b[outOff++] = (byte)(uv >> 8);
			b[outOff] = (byte)uv;
		}
	}
}