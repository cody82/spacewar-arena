using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Lidgren.Library.Network.Xna
{
	public static class XnaSerialization
	{
		/// <summary>
		/// Writes a Vector2
		/// </summary>
		public static void Write(NetMessage msg, Vector2 vector)
		{
			msg.Write(vector.X);
			msg.Write(vector.Y);
		}

		/// <summary>
		/// Reads a Vector2
		/// </summary>
		public static Vector2 ReadVector2(NetMessage msg)
		{
			Vector2 retval;
			retval.X = msg.ReadSingle();
			retval.Y = msg.ReadSingle();
			return retval;
		}

		/// <summary>
		/// Writes a Vector3
		/// </summary>
		public static void Write(NetMessage msg, Vector3 vector)
		{
			msg.Write(vector.X);
			msg.Write(vector.Y);
			msg.Write(vector.Z);
		}

		/// <summary>
		/// Reads a Vector3
		/// </summary>
		public static Vector3 ReadVector3(NetMessage msg)
		{
			Vector3 retval;
			retval.X = msg.ReadSingle();
			retval.Y = msg.ReadSingle();
			retval.Z = msg.ReadSingle();
			return retval;
		}

		/// <summary>
		/// Writes a Vector4
		/// </summary>
		public static void Write(NetMessage msg, Vector4 vector)
		{
			msg.Write(vector.X);
			msg.Write(vector.Y);
			msg.Write(vector.Z);
			msg.Write(vector.W);
		}

		/// <summary>
		/// Reads a Vector4
		/// </summary>
		public static Vector4 ReadVector4(NetMessage msg)
		{
			Vector4 retval;
			retval.X = msg.ReadSingle();
			retval.Y = msg.ReadSingle();
			retval.Z = msg.ReadSingle();
			retval.W = msg.ReadSingle();
			return retval;
		}


		/// <summary>
		/// Writes a unit vector (ie. a vector of length 1.0, for example a surface normal) 
		/// using specified number of bits
		/// </summary>
		public static void WriteUnitVector3(NetMessage msg, Vector3 unitVector, int numberOfBits)
		{
			float x = unitVector.X;
			float y = unitVector.Y;
			float z = unitVector.Z;
			double invPi = 1.0 / Math.PI;
			float phi = (float)(Math.Atan2(x, y) * invPi);
			float theta = (float)(Math.Atan2(z, Math.Sqrt(x*x + y*y)) * (invPi * 2));

			int halfBits = numberOfBits / 2;
			msg.WriteSignedSingle(phi, halfBits);
			msg.WriteSignedSingle(theta, numberOfBits - halfBits);
		}

		/// <summary>
		/// Reads a unit vector written using WriteUnitVector3(numberOfBits)
		/// </summary>
		public static Vector3 ReadUnitVector3(NetMessage msg, int numberOfBits)
		{
			int halfBits = numberOfBits / 2;
			float phi = msg.ReadSignedSingle(halfBits) * (float)Math.PI;
			float theta = msg.ReadSignedSingle(numberOfBits - halfBits) * (float)(Math.PI * 0.5);

			Vector3 retval;
			retval.X = (float)(Math.Sin(phi) * Math.Cos(theta));
			retval.Y = (float)(Math.Cos(phi) * Math.Cos(theta));
			retval.Z = (float)Math.Sin(theta);

			return retval;
		}

		/// <summary>
		/// Writes a unit quaternion using the specified number of bits per element
		/// for a total of 4 x bitsPerElements bits. Suggested value is 8 to 24 bits.
		/// </summary>
		public static void WriteRotation(NetMessage msg, Quaternion quaternion, int bitsPerElement)
		{
			msg.WriteSignedSingle(quaternion.X, bitsPerElement);
			msg.WriteSignedSingle(quaternion.Y, bitsPerElement);
			msg.WriteSignedSingle(quaternion.Z, bitsPerElement);
			msg.WriteSignedSingle(quaternion.W, bitsPerElement);
		}

		/// <summary>
		/// Reads a unit quaternion written using WriteRotation(... ,bitsPerElement)
		/// </summary>
		public static Quaternion ReadRotation(NetMessage msg, int bitsPerElement)
		{
			Quaternion retval;
			retval.X = msg.ReadSignedSingle(bitsPerElement);
			retval.Y = msg.ReadSignedSingle(bitsPerElement);
			retval.Z = msg.ReadSignedSingle(bitsPerElement);
			retval.W = msg.ReadSignedSingle(bitsPerElement);
			return retval;
		}

		/// <summary>
		/// Writes an orthonormal matrix (rotation, translation but not scaling or projection)
		/// </summary>
		public static void WriteMatrix(NetMessage msg, ref Matrix matrix)
		{
			Quaternion rot = Quaternion.CreateFromRotationMatrix(matrix);
			WriteRotation(msg, rot, 24);
			msg.Write(matrix.M41);
			msg.Write(matrix.M42);
			msg.Write(matrix.M43);
		}

		/// <summary>
		/// Writes an orthonormal matrix (rotation, translation but no scaling or projection)
		/// </summary>
		public static void WriteMatrix(NetMessage msg, Matrix matrix)
		{
			Quaternion rot = Quaternion.CreateFromRotationMatrix(matrix);
			WriteRotation(msg, rot, 24);
			msg.Write(matrix.M41);
			msg.Write(matrix.M42);
			msg.Write(matrix.M43);
		}

		/// <summary>
		/// Reads a matrix written using WriteMatrix()
		/// </summary>
		public static Matrix ReadMatrix(NetMessage msg)
		{
			Quaternion rot = ReadRotation(msg, 24);
			Matrix retval = Matrix.CreateFromQuaternion(rot);
			retval.M41 = msg.ReadSingle();
			retval.M42 = msg.ReadSingle();
			retval.M43 = msg.ReadSingle();
			return retval;
		}

		/// <summary>
		/// Reads a matrix written using WriteMatrix()
		/// </summary>
		public static void ReadMatrix(NetMessage msg, ref Matrix destination)
		{
			Quaternion rot = ReadRotation(msg, 24);
			destination = Matrix.CreateFromQuaternion(rot);
			destination.M41 = msg.ReadSingle();
			destination.M42 = msg.ReadSingle();
			destination.M43 = msg.ReadSingle();
		}

		/// <summary>
		/// Writes a bounding sphere
		/// </summary>
		public static void Write(NetMessage msg, BoundingSphere bounds)
		{
			msg.Write(bounds.Center.X);
			msg.Write(bounds.Center.Y);
			msg.Write(bounds.Center.Z);
			msg.Write(bounds.Radius);
		}

		/// <summary>
		/// Reads a bounding sphere written using Write(msg, BoundingSphere)
		/// </summary>
		public static BoundingSphere ReadBoundingSphere(NetMessage msg)
		{
			BoundingSphere retval;
			retval.Center.X = msg.ReadSingle();
			retval.Center.Y = msg.ReadSingle();
			retval.Center.Z = msg.ReadSingle();
			retval.Radius = msg.ReadSingle();
			return retval;
		}
	}
}
