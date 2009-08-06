using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Lidgren.Library.Network
{
	internal partial class NetBuffer
	{
		//
		// 7 bit length encoding
		//
	
		/// <summary>
		/// Write 7-bit encoded UInt32
		/// </summary>
		/// <returns>number of bytes written</returns>
		public int Write7BitEncodedUInt(uint value)
		{
			int retval = 1;
			uint num1 = (uint)value;
			while (num1 >= 0x80)
			{
				this.Write((byte)(num1 | 0x80));
				num1 = num1 >> 7;
				retval++;
			}
			this.Write((byte)num1);
			return retval;
		}

		/// <summary>
		/// Reads a UInt32 written using Write7BitEncodedUInt()
		/// </summary>
		public uint Read7BitEncodedUInt()
		{
			int num1 = 0;
			int num2 = 0;
			while (true)
			{
				if (num2 == 0x23)
					throw new FormatException("Bad 7-bit encoded integer");

				byte num3 = this.ReadByte();
				num1 |= (num3 & 0x7f) << (num2 & 0x1f);
				num2 += 7;
				if ((num3 & 0x80) == 0)
					return (uint)num1;
			}
		}

		/// <summary>
		/// Compress (lossy) a float in the range -1..1 using numberOfBits bits
		/// </summary>
		public void WriteSignedSingle(float val, int numberOfBits)
		{
			Debug.Assert(((val >= -1.0) && (val <= 1.0)), " WriteSignedSingle() must be passed a float in the range -1 to 1; val is " + val);

			float unit = (val + 1.0f) * 0.5f;
			int maxVal = (1 << numberOfBits) - 1;
			uint writeVal = (uint)(unit * (float)maxVal);

			Write(writeVal, numberOfBits);
		}

		/// <summary>
		/// Reads a float written using WriteSignedSingle()
		/// </summary>
		public float ReadSignedSingle(int numberOfBits)
		{
			uint encodedVal = ReadUInt32(numberOfBits);
			int maxVal = (1 << numberOfBits) - 1;
			return ((float)(encodedVal + 1) / (float)(maxVal + 1) - 0.5f) * 2.0f;
		}
	
		/// <summary>
		/// Compress (lossy) a float in the range 0..1 using numberOfBits bits
		/// </summary>
		public void WriteUnitSingle(float val, int numberOfBits)
		{
			Debug.Assert(((val >= 0.0) && (val <= 1.0)), " WriteUnitSingle() must be passed a float in the range 0 to 1; val is " + val);

			int maxVal = (1 << numberOfBits) - 1;
			uint writeVal = (uint)(val * (float)maxVal);

			Write(writeVal, numberOfBits);
		}

		/// <summary>
		/// Reads a float written using WriteUnitSingle()
		/// </summary>
		public float ReadUnitSingle(int numberOfBits)
		{
			uint encodedVal = ReadUInt32(numberOfBits);
			int maxVal = (1 << numberOfBits) - 1;
			return (float)(encodedVal + 1) / (float)(maxVal + 1);
		}

		/// <summary>
		/// Compress a float within a specified range using a certain number of bits
		/// </summary>
		public void WriteRangedSingle(float val, float min, float max, int numberOfBits)
		{
			Debug.Assert(((val >= min) && (val <= max)), " WriteRangedSingle() must be passed a float in the range MIN to MAX; val is " + val);

			float range = max - min;
			float unit = ((val - min) / range);
			int maxVal = (1 << numberOfBits) - 1;
			Write((uint)((float)maxVal * unit), numberOfBits);
		}

		/// <summary>
		/// Reads a float written using WriteRangedSingle() using the same MIN and MAX values
		/// </summary>
		public float ReadRangedSingle(float min, float max, int numberOfBits)
		{
			float range = max - min;
			int maxVal = (1 << numberOfBits) - 1;
			float encodedVal = (float)ReadUInt32(numberOfBits);
			float unit = encodedVal / (float)maxVal;
			return min + (unit * range);
		}

	}
}
