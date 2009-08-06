using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Library.Network;
using Lidgren.Library.Network.Xna;
using Microsoft.Xna.Framework;

namespace UnitTests
{
	class Program
	{
		static void Main(string[] args)
		{
			// needed for network context
			NetClient unused = new NetClient(new NetAppConfiguration("test", 0), new NetLog());

			NetMessage msg = new NetMessage();

			Matrix mat = Matrix.CreateRotationX(MathHelper.ToRadians(36.0f));
			mat *= Matrix.CreateRotationY(MathHelper.ToRadians(-12.0f));
			mat *= Matrix.CreateRotationZ(MathHelper.ToRadians(192.0f));
			mat.Translation = new Vector3(1.0f, 2.0f, 3.0f);

			Quaternion rot = Quaternion.CreateFromRotationMatrix(mat);

			Vector3 vec = new Vector3(42.0f, 43.0f, 44.0f);

			Vector3 nrm = new Vector3(12, 34, 56);
			nrm.Normalize();

			BoundingSphere sphere = new BoundingSphere(vec, float.PositiveInfinity);

			XnaSerialization.Write(msg, vec);
			XnaSerialization.WriteRotation(msg, rot, 16);
			XnaSerialization.WriteMatrix(msg, mat);
			XnaSerialization.WriteUnitVector3(msg, nrm, 24);
			XnaSerialization.Write(msg, sphere);

			// verify
			msg.ResetReadPointer();
			Vector3 rVec = XnaSerialization.ReadVector3(msg);
			Quaternion rRot = XnaSerialization.ReadRotation(msg, 16);
			Matrix rMat = XnaSerialization.ReadMatrix(msg);
			Vector3 rNrm = XnaSerialization.ReadUnitVector3(msg, 24);
			BoundingSphere rSphere = XnaSerialization.ReadBoundingSphere(msg);

			// Compare here
			System.Diagnostics.Debugger.Break();
		}
	}
}
