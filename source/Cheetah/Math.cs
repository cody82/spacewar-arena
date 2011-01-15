using System;
using OpenTK;

namespace Cheetah
{
    public struct Plane
    {
        public Vector3 Normal;
        public float Dist;

        public Plane(float nx, float ny, float nz, float d)
        {
            Normal = new Vector3(nx, ny, nz);
            Dist = d;
        }
        public Plane(float[] values)
            :this(values[0],values[1],values[2],values[3])
        {
            
        }

        public float A
        {
            get
            {
                return Normal.X;
            }
        }
        public float B
        {
            get
            {
                return Normal.Y;
            }
        }
        public float C
        {
            get
            {
                return Normal.Z;
            }
        }
        public float D
        {
            get
            {
                return Dist;
            }
        }

        public static Plane FromPointAndNormal(Vector3 p, Vector3 n)
        {
            return new Plane(n.X, n.Y, n.Z, Vector3.Dot(n, p));
        }

        public float Intersect(Vector3 p0, Vector3 p1)
        {
            Vector3 Direction = p1 - p0;
            float denominator = Vector3.Dot(Normal, Direction);
            if (denominator == 0)
            {
                throw new DivideByZeroException("Can not get the intersection of a plane with a line when they are parallel to each other.");
            }
            float t = (Dist - Vector3.Dot(Normal, p0)) / denominator;
            return t;
        }

        public Vector3 GetIntersection(Vector3 p0, Vector3 p1)
        {

            Vector3 Direction = p1 - p0;
            float denominator = Vector3.Dot(Normal, Direction);
            if (denominator == 0)
            {
                throw new DivideByZeroException("Can not get the intersection of a plane with a line when they are parallel to each other.");
            }
            float t = (Dist - Vector3.Dot(Normal, p0)) / denominator;
            return p0 + Direction * t;
        }
    }

    public struct Sphere
    {
        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
        public Vector3 Center;
        public float Radius;
    }

    public struct Ray
    {
        public Vector3 Start;
        public Vector3 End;
        public Vector3 Direction
        {
            get
            {
                Vector3 n=End-Start;
                n.Normalize();
                return n;
            }
            set
            {
                End = Start + Direction;
            }
        }
        public float Length
        {
            get
            {
                return (Start-End).Length;
            }
            set
            {
                End = Start + Direction * value;
            }
        }

        public bool Intersect(Sphere s)
        {
            Plane p = Plane.FromPointAndNormal(s.Center, Direction);
            float f=p.Intersect(Start, End);
            if (f < 0) f = 0;
            else if (f > 1) f = 1;
            Vector3 nearest = Start + f * (End - Start);
            float dist = (s.Center - nearest).Length;
            return dist<=s.Radius;
        }

        public Ray(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }
    }

	public class Frustum
	{
		public Frustum()
		{
			planes=new float[6][];
			for(int i=0;i<6;++i)
				planes[i]=new float[4];
		}
		public bool SphereInFrustum(float x,float y,float z,float r)
		{
			////////////////////////////////////////////////////////////////////////
			// Return true when the sphere is inside the view frustum. This is
			// the case when it is never further behind a any plane than its radius
			////////////////////////////////////////////////////////////////////////

			int iCurPlane;

			for (iCurPlane = 0; iCurPlane<6; iCurPlane++)
			{
				if (planes[iCurPlane][0] * x + planes[iCurPlane][1] * 
					y + planes[iCurPlane][2] * z + planes[iCurPlane][3] <= -r)
				{
					return false;
				}
			}

			return true;
		}
        public bool SphereInFrustum(Vector3 v, float r)
        {
            ////////////////////////////////////////////////////////////////////////
            // Return true when the sphere is inside the view frustum. This is
            // the case when it is never further behind a any plane than its radius
            ////////////////////////////////////////////////////////////////////////

            int iCurPlane;

            for (iCurPlane = 0; iCurPlane < 6; iCurPlane++)
            {
                if (planes[iCurPlane][0] * v.X + planes[iCurPlane][1] *
                    v.Y + planes[iCurPlane][2] * v.Z + planes[iCurPlane][3] <= -r)
                {
                    return false;
                }
            }

            return true;
        }
        public bool BoxInFrustrum(float X, float Y, float Z, float X2, float Y2, float Z2)
        {
            bool ReturnStatus = true;

            for (int i = 0; i < 6; i++)
            {
                Plane CP = new Plane(planes[i]);

                if (((CP.A * X) + (CP.B * Y) + (CP.C * Z) + CP.D) > 0) continue;
                if (((CP.A * X2) + (CP.B * Y) + (CP.C * Z) + CP.D) > 0) continue;
                if (((CP.A * X) + (CP.B * Y2) + (CP.C * Z) + CP.D) > 0) continue;
                if (((CP.A * X2) + (CP.B * Y2) + (CP.C * Z) + CP.D) > 0) continue;
                if (((CP.A * X) + (CP.B * Y) + (CP.C * Z2) + CP.D) > 0) continue;
                if (((CP.A * X2) + (CP.B * Y) + (CP.C * Z2) + CP.D) > 0) continue;
                if (((CP.A * X) + (CP.B * Y2) + (CP.C * Z2) + CP.D) > 0) continue;
                if (((CP.A * X2) + (CP.B * Y2) + (CP.C * Z2) + CP.D) > 0) continue;

                ReturnStatus = false;
            }

            return (ReturnStatus);
        }
		public bool CubeInFrustum(float x,float y,float z, float fSize)
		{
			////////////////////////////////////////////////////////////////////////
			// Return true when the cube intersects with the view frustum. The
			// parameter fSize has to be the width of the cube divided by two
			////////////////////////////////////////////////////////////////////////

			int iCurPlane;

			for (iCurPlane = 0; iCurPlane<6; iCurPlane++)
			{
				if (planes[iCurPlane][0] * (x - fSize) + planes[iCurPlane][1] *
					(y - fSize) + planes[iCurPlane][2] * (z - fSize) + 
					planes[iCurPlane][3] > 0)
					continue;
				if (planes[iCurPlane][0] * (x + fSize) + planes[iCurPlane][1] * 
					(y - fSize) + planes[iCurPlane][2] * (z - fSize) + 
					planes[iCurPlane][3] > 0)
					continue;
				if (planes[iCurPlane][0] * (x - fSize) + planes[iCurPlane][1] * 
					(y + fSize) + planes[iCurPlane][2] * (z - fSize) + 
					planes[iCurPlane][3] > 0)
					continue;
				if (planes[iCurPlane][0] * (x + fSize) + planes[iCurPlane][1] * 
					(y + fSize) + planes[iCurPlane][2] * (z - fSize) + 
					planes[iCurPlane][3] > 0)
					continue;
				if (planes[iCurPlane][0] * (x - fSize) + planes[iCurPlane][1] * 
					(y - fSize) + planes[iCurPlane][2] * (z + fSize) + 
					planes[iCurPlane][3] > 0)
					continue;
				if (planes[iCurPlane][0] * (x + fSize) + planes[iCurPlane][1] * 
					(y - fSize) + planes[iCurPlane][2] * (z + fSize) + 
					planes[iCurPlane][3] > 0)
					continue;
				if (planes[iCurPlane][0] * (x - fSize) + planes[iCurPlane][1] * 
					(y + fSize) + planes[iCurPlane][2] * (z + fSize) + 
					planes[iCurPlane][3] > 0)
					continue;
				if (planes[iCurPlane][0] * (x + fSize) + planes[iCurPlane][1] * 
					(y + fSize) + planes[iCurPlane][2] * (z + fSize) + 
					planes[iCurPlane][3] > 0)
					continue;

				return false;
			}

			return true;
		}

		public bool PointInFrustum(float x,float y,float z)
		{
			////////////////////////////////////////////////////////////////////////
			// Return true when the given point is inside the view frustum. This is
			// the case when its distance is positive to all the frustum planes
			////////////////////////////////////////////////////////////////////////

			int iCurPlane;

			for (iCurPlane = 0; iCurPlane<6; iCurPlane++)
			{
				if (planes[iCurPlane][0] * x + planes[iCurPlane][1] * 
					y + planes[iCurPlane][2] * z + planes[iCurPlane][3] <= 0)
				{
					return false;
				}
			}

			return true;
		}
        public bool PointInFrustum(Vector3 v)
        {
            ////////////////////////////////////////////////////////////////////////
            // Return true when the given point is inside the view frustum. This is
            // the case when its distance is positive to all the frustum planes
            ////////////////////////////////////////////////////////////////////////

            int iCurPlane;

            for (iCurPlane = 0; iCurPlane < 6; iCurPlane++)
            {
                if (planes[iCurPlane][0] * v.X + planes[iCurPlane][1] *
                    v.Y + planes[iCurPlane][2] * v.Z + planes[iCurPlane][3] <= 0)
                {
                    return false;
                }
            }

            return true;
        }
		public bool GetFrustum(IRenderer r)
		{
			////////////////////////////////////////////////////////////////////////
			// Fetch the current projection and modelview matrices from OpenGL
			// and recalculate the 6 frustum planes from it
			////////////////////////////////////////////////////////////////////////

			float[] fProj=new float[16];	// For grabbing the projection matrix
			float[] fView=new float[16];	// For grabbing the modelview matrix
			float[] fClip=new float[16];	// Holds the result of projection * modelview
			r.GetMatrix(fView,fProj);
			float fT;

			// Get the current projection matrix from OpenGL
			//glGetFloatv(GL_PROJECTION_MATRIX, fProj);

			// Get the current modelview matrix from OpenGL
			//Gl.glGetFloatv(GL_MODELVIEW_MATRIX, fView);

			// Concenate the two matrices
			fClip[ 0] = fView[ 0] * fProj[ 0] + fView[ 1] * fProj[ 4] + fView[ 2] * fProj[ 8] + fView[ 3] * fProj[12];
			fClip[ 1] = fView[ 0] * fProj[ 1] + fView[ 1] * fProj[ 5] + fView[ 2] * fProj[ 9] + fView[ 3] * fProj[13];
			fClip[ 2] = fView[ 0] * fProj[ 2] + fView[ 1] * fProj[ 6] + fView[ 2] * fProj[10] + fView[ 3] * fProj[14];
			fClip[ 3] = fView[ 0] * fProj[ 3] + fView[ 1] * fProj[ 7] + fView[ 2] * fProj[11] + fView[ 3] * fProj[15];

			fClip[ 4] = fView[ 4] * fProj[ 0] + fView[ 5] * fProj[ 4] + fView[ 6] * fProj[ 8] + fView[ 7] * fProj[12];
			fClip[ 5] = fView[ 4] * fProj[ 1] + fView[ 5] * fProj[ 5] + fView[ 6] * fProj[ 9] + fView[ 7] * fProj[13];
			fClip[ 6] = fView[ 4] * fProj[ 2] + fView[ 5] * fProj[ 6] + fView[ 6] * fProj[10] + fView[ 7] * fProj[14];
			fClip[ 7] = fView[ 4] * fProj[ 3] + fView[ 5] * fProj[ 7] + fView[ 6] * fProj[11] + fView[ 7] * fProj[15];

			fClip[ 8] = fView[ 8] * fProj[ 0] + fView[ 9] * fProj[ 4] + fView[10] * fProj[ 8] + fView[11] * fProj[12];
			fClip[ 9] = fView[ 8] * fProj[ 1] + fView[ 9] * fProj[ 5] + fView[10] * fProj[ 9] + fView[11] * fProj[13];
			fClip[10] = fView[ 8] * fProj[ 2] + fView[ 9] * fProj[ 6] + fView[10] * fProj[10] + fView[11] * fProj[14];
			fClip[11] = fView[ 8] * fProj[ 3] + fView[ 9] * fProj[ 7] + fView[10] * fProj[11] + fView[11] * fProj[15];

			fClip[12] = fView[12] * fProj[ 0] + fView[13] * fProj[ 4] + fView[14] * fProj[ 8] + fView[15] * fProj[12];
			fClip[13] = fView[12] * fProj[ 1] + fView[13] * fProj[ 5] + fView[14] * fProj[ 9] + fView[15] * fProj[13];
			fClip[14] = fView[12] * fProj[ 2] + fView[13] * fProj[ 6] + fView[14] * fProj[10] + fView[15] * fProj[14];
			fClip[15] = fView[12] * fProj[ 3] + fView[13] * fProj[ 7] + fView[14] * fProj[11] + fView[15] * fProj[15];

			// Extract the right plane
			planes[0][0] = fClip[ 3] - fClip[ 0];
			planes[0][1] = fClip[ 7] - fClip[ 4];
			planes[0][2] = fClip[11] - fClip[ 8];
			planes[0][3] = fClip[15] - fClip[12];

			// Normalize the result
			fT = (float) Math.Sqrt(planes[0][0] * planes[0][0] + planes[0][1] * planes[0][1] + 
				planes[0][2] * planes[0][2]);
			planes[0][0] /= fT;
			planes[0][1] /= fT;
			planes[0][2] /= fT;
			planes[0][3] /= fT;

			// Extract the left plane
			planes[1][0] = fClip[ 3] + fClip[ 0];
			planes[1][1] = fClip[ 7] + fClip[ 4];
			planes[1][2] = fClip[11] + fClip[ 8];
			planes[1][3] = fClip[15] + fClip[12];

			// Normalize the result
			fT = (float) Math.Sqrt(planes[1][0] * planes[1][0] + planes[1][1] * planes[1][1] + 
				planes[1][2] * planes[1][2]);
			planes[1][0] /= fT;
			planes[1][1] /= fT;
			planes[1][2] /= fT;
			planes[1][3] /= fT;

			// Extract the bottom plane
			planes[2][0] = fClip[ 3] + fClip[ 1];
			planes[2][1] = fClip[ 7] + fClip[ 5];
			planes[2][2] = fClip[11] + fClip[ 9];
			planes[2][3] = fClip[15] + fClip[13];

			// Normalize the result
			fT = (float) Math.Sqrt(planes[2][0] * planes[2][0] + planes[2][1] * planes[2][1] + 
				planes[2][2] * planes[2][2]);
			planes[2][0] /= fT;
			planes[2][1] /= fT;
			planes[2][2] /= fT;
			planes[2][3] /= fT;

			// Extract the top plane
			planes[3][0] = fClip[ 3] - fClip[ 1];
			planes[3][1] = fClip[ 7] - fClip[ 5];
			planes[3][2] = fClip[11] - fClip[ 9];
			planes[3][3] = fClip[15] - fClip[13];

			// Normalize the result
			fT = (float) Math.Sqrt(planes[3][0] * planes[3][0] + planes[3][1] * planes[3][1] + 
				planes[3][2] * planes[3][2]);
			planes[3][0] /= fT;
			planes[3][1] /= fT;
			planes[3][2] /= fT;
			planes[3][3] /= fT;

			// Extract the far plane
			planes[4][0] = fClip[ 3] - fClip[ 2];
			planes[4][1] = fClip[ 7] - fClip[ 6];
			planes[4][2] = fClip[11] - fClip[10];
			planes[4][3] = fClip[15] - fClip[14];

			// Normalize the result
			fT = (float) Math.Sqrt(planes[4][0] * planes[4][0] + planes[4][1] * planes[4][1] + 
				planes[4][2] * planes[4][2]);
			planes[4][0] /= fT;
			planes[4][1] /= fT;
			planes[4][2] /= fT;
			planes[4][3] /= fT;

			// Extract the near plane
			planes[5][0] = fClip[ 3] + fClip[ 2];
			planes[5][1] = fClip[ 7] + fClip[ 6];
			planes[5][2] = fClip[11] + fClip[10];
			planes[5][3] = fClip[15] + fClip[14];

			// Normalize the result
			fT = (float) Math.Sqrt(planes[5][0] * planes[5][0] + planes[5][1] * planes[5][1] + 
				planes[5][2] * planes[5][2]);
			planes[5][0] /= fT;
			planes[5][1] /= fT;
			planes[5][2] /= fT;
			planes[5][3] /= fT;


			return true;
		}
		
		float[][] planes;
	}
}
