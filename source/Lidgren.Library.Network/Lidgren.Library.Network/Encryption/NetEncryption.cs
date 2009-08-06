using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Lidgren.Library.Network
{
	/// <summary>
	/// Helper class for encryption
	/// </summary>
	public sealed class NetEncryption
	{
		private byte[] m_symEncKeyBytes;
		private RSACryptoServiceProvider m_rsa;
		private XTEA m_xtea;
		private int[] m_symmetricKey;

		internal byte[] SymmetricEncryptionKeyBytes { get { return m_symEncKeyBytes; } }

		/// <summary>
		/// Generate an RSA keypair, divided into public and private parts
		/// </summary>
		public static void GenerateRandomKeyPair(out byte[] publicKey, out byte[] privateKey)
		{
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
			RSAParameters prm = rsa.ExportParameters(true);

			List<byte> pubKey = new List<byte>(131);
			pubKey.AddRange(prm.Exponent);
			pubKey.AddRange(prm.Modulus);
			
			List<byte> privKey = new List<byte>(448);
			privKey.AddRange(prm.D); // 128
			privKey.AddRange(prm.DP); // 64
			privKey.AddRange(prm.DQ); // 64
			privKey.AddRange(prm.InverseQ); // 64
			privKey.AddRange(prm.P); // 64
			privKey.AddRange(prm.Q); // 64

			publicKey = pubKey.ToArray();
			privateKey = privKey.ToArray();
		}

		public NetEncryption()
		{
		}

		internal void SetSymmetricKey(byte[] xteakey)
		{
			m_symEncKeyBytes = xteakey;
			m_symmetricKey = new int[4];
			m_symmetricKey[0] = BitConverter.ToInt32(xteakey, 0);
			m_symmetricKey[1] = BitConverter.ToInt32(xteakey, 4);
			m_symmetricKey[2] = BitConverter.ToInt32(xteakey, 8);
			m_symmetricKey[3] = BitConverter.ToInt32(xteakey, 12);
			m_xtea = new XTEA(xteakey, 32);
		}

		/// <summary>
		/// for clients; pass null as privateKey
		/// </summary>
		internal void SetRSAKey(byte[] publicKey, byte[] privateKey)
		{
			m_rsa = new RSACryptoServiceProvider();

			RSAParameters prm = new RSAParameters();
			prm.Exponent = Extract(publicKey, 0, 3);
			prm.Modulus = Extract(publicKey, 3, 128);

			if (privateKey != null)
			{
				int ptr = 0;
				prm.D = Extract(privateKey, ptr, 128); ptr += 128;
				prm.DP = Extract(privateKey, ptr, 64); ptr += 64;
				prm.DQ = Extract(privateKey, ptr, 64); ptr += 64;
				prm.InverseQ = Extract(privateKey, ptr, 64); ptr += 64;
				prm.P = Extract(privateKey, ptr, 64); ptr += 64;
				prm.Q = Extract(privateKey, ptr, 64); ptr += 64;
			}

			m_rsa.ImportParameters(prm);

			// also generate random symmetric key
			byte[] newKey = new byte[16];
			NetRandom.Default.NextBytes(newKey);
			SetSymmetricKey(newKey);
		}

		private static byte[] Extract(byte[] buf, int start, int len)
		{
			byte[] retval = new byte[len];
			Array.Copy(buf, start, retval, 0, len);
			return retval;
		}

		/// <summary>
		/// Encrypt data using a public RSA key
		/// </summary>
		internal byte[] EncryptRSA(byte[] plainData)
		{
			return m_rsa.Encrypt(plainData, false);
		}

		/// <summary>
		/// Decrypt data using the public and private RSA key
		/// </summary>
		internal byte[] DecryptRSA(NetLog log, byte[] encryptedData)
		{
			try
			{
				return m_rsa.Decrypt(encryptedData, false);
			}
			catch (Exception ex)
			{
				log.Warning("Failed to Decrypt RSA: " + ex);
				return null;
			}
		}

		/// <summary>
		/// Append a CRC checksum and encrypt data in place using XTEA
		/// </summary>
		internal void EncryptSymmetric(NetLog log, NetBuffer buffer)
		{
			//string plain = Convert.ToBase64String(buffer.Data, 0, buffer.LengthBytes);

			// pad to even byte
			int padBits = 8 - (buffer.LengthBits % 8);
			buffer.Write((uint)0, padBits);

			// make room for crc
			buffer.Write((ushort)0);

			// pad to 8-byte boundary
			int len = buffer.LengthBytes;
			int totalLen = len + (8 - (len % 8));
			int zeroPads = totalLen - len;
			buffer.EnsureBufferSize(totalLen * 8);
			for (int i = 0; i < zeroPads; i++)
				buffer.Write((byte)0);

			// replace crc
			ushort crc = Checksum.Adler16(buffer.Data, 0, buffer.LengthBytes);
			buffer.Data[totalLen - 2] = (byte)(crc >> 8);
			buffer.Data[totalLen - 1] = (byte)crc;

			// encrypt in place
			int ptr = 0;
			while (ptr < totalLen)
			{
				m_xtea.EncryptBlock(buffer.Data, ptr, buffer.Data, ptr);
				ptr += 8;
			}

			//log.Debug("Encrypting using key: " + Convert.ToBase64String(m_xtea.Key));
			//log.Debug("Plain: " + plain);
			//log.Debug("Result (len " + buffer.LengthBytes + "): " + Convert.ToBase64String(buffer.Data, 0, buffer.LengthBytes) + " CRC: " + crc);

			return;
		}
		
		/// <summary> 
		/// Decrypt using XTEA algo and verify CRC
		/// </summary>
		/// <returns>true for success, false for failure</returns>
		internal bool DecryptSymmetric(NetLog log, NetBuffer buffer)
		{
			int bufLen = buffer.LengthBytes;

			if (bufLen % 8 != 0)
			{
				log.Info("Bad buffer size in DecryptSymmetricInPlace()");
				return false;
			}

			//log.Debug("Decrypting using key: " + Convert.ToBase64String(m_xtea.Key));
			//log.Debug("Input (len " + bufLen + "): " + Convert.ToBase64String(buffer.Data, 0, bufLen));

			// decrypt
			for (int i = 0; i < bufLen; i += 8)
				m_xtea.DecryptBlock(buffer.Data, i, buffer.Data, i);
			
			ushort statedCrc = (ushort)(buffer.Data[bufLen - 2] << 8 | buffer.Data[bufLen-1]);

			// zap crc to be able to compare
			buffer.Data[bufLen - 2] = 0;
			buffer.Data[bufLen - 1] = 0;
			ushort dataCrc = Checksum.Adler16(buffer.Data, 0, bufLen);

			//log.Debug("Plain (len " + bufLen + "): " + Convert.ToBase64String(buffer.Data, 0, bufLen) + " Stated CRC: " + statedCrc + " Calc: " + realCrc);
			if (statedCrc != dataCrc)
			{
				log.Warning("CRC failure; expected " + dataCrc + " found " + statedCrc + " dropping packet!");
				return false;
			}

			// remove crc
			buffer.LengthBits -= 16;

			return true;
		}
	}
}
