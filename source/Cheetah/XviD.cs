using System;
using System.Runtime.InteropServices;

namespace XviD
{
	/* XVID_GBL_INIT param1 */
	public struct xvid_gbl_init_t
	{
		public int version;
		public int cpu_flags; /* [in:opt] zero = autodetect cpu; XVID_CPU_FORCE|{cpu features} = force cpu features */
		public int debug;     /* [in:opt] debug level */
	} ;
	
	/* XVID_DEC_CREATE param 1
		image width & height may be specified here when the dimensions are
		known in advance. */
    public struct xvid_dec_create_t
	{
		public int version;
		public int width;     /* [in:opt] image width */
		public int height;    /* [in:opt] image width */
		public IntPtr handle; /* [out]	   decore context handle */
	} ;


	/* xvid_image_t
		for non-planar colorspaces use only plane[0] and stride[0]
		four plane reserved for alpha*/
    public struct xvid_image_t
	{
		public int csp;				/* [in] colorspace; or with XVID_CSP_VFLIP to perform vertical flip */
		public IntPtr plane1;		/* [in] image plane ptrs */
        public IntPtr plane2;		/* [in] image plane ptrs */
        public IntPtr plane3;		/* [in] image plane ptrs */
        public IntPtr plane4;		/* [in] image plane ptrs */
		public int stride1;			/* [in] image stride; "bytes per row"*/
		public int stride2;			/* [in] image stride; "bytes per row"*/
		public int stride3;			/* [in] image stride; "bytes per row"*/
		public int stride4;			/* [in] image stride; "bytes per row"*/
	} ;


    public struct xvid_dec_frame_t
	{
		public int version;
		public int general;         /* [in:opt] general flags */
        public IntPtr bitstream;     /* [in]     bitstream (read from)*/
		public int length;          /* [in]     bitstream length */
		public xvid_image_t output; /* [in]     output image (written to) */
	};

	public class XviDException : Exception
	{
		public XviDException(string text):base(text)
		{
		}
	}


	/// <summary>
	/// Summary description for XviD.
	/// </summary>
	public class XviD
	{
#if LINUX
		const string XVIDLIB="libxvidcore.so";
#else
        const string XVIDLIB = "xvidcore.dll";
#endif

        const int XVID_ERR_FAIL=-1;		/* general fault */
		const int XVID_ERR_MEMORY=-2;		/* memory allocation error */
		const int XVID_ERR_FORMAT=-3;		/* file format error */
		const int XVID_ERR_VERSION=-4;		/* structure version not supported */
		const int XVID_ERR_END=-5;		/* encoder only; end of stream reached */
		
		const int XVID_CPU_FORCE=(1<<31); /* force passed cpu flags */

		const int XVID_GBL_INIT    =0; /* initialize xvidcore; must be called before using xvid_decore, or xvid_encore) */
		const int XVID_GBL_INFO    =1; /* return some info about xvidcore, and the host computer */
		const int XVID_GBL_CONVERT =2; /* colorspace conversion utility */
		
		const int XVID_DEC_CREATE  =0; /* create decore instance; return 0 on success */
		const int XVID_DEC_DESTROY =1; /* destroy decore instance: return 0 on success */
		const int XVID_DEC_DECODE  =2; /* decode a frame: returns number of bytes consumed >= 0 */

		const int XVID_CSP_PLANAR  = (1<< 0); /* 4:2:0 planar (==I420, except for pointers/strides) */
		const int XVID_CSP_USER	  =XVID_CSP_PLANAR;
		const int XVID_CSP_I420    = (1<< 1); /* 4:2:0 planar */
		const int XVID_CSP_YV12    = (1<< 2); /* 4:2:0 planar */
		const int XVID_CSP_YUY2    = (1<< 3); /* 4:2:2 packed */
		const int XVID_CSP_UYVY    = (1<< 4); /* 4:2:2 packed */
		const int XVID_CSP_YVYU    = (1<< 5); /* 4:2:2 packed */
		const int XVID_CSP_BGRA    = (1<< 6); /* 32-bit bgra packed */
		const int XVID_CSP_ABGR    = (1<< 7); /* 32-bit abgr packed */
		const int XVID_CSP_RGBA    = (1<< 8); /* 32-bit rgba packed */
		const int XVID_CSP_ARGB    = (1<<15); /* 32-bit argb packed */
		const int XVID_CSP_BGR     = (1<< 9); /* 24-bit bgr packed */
		const int XVID_CSP_RGB555  = (1<<10); /* 16-bit rgb555 packed */
		const int XVID_CSP_RGB565  = (1<<11); /* 16-bit rgb565 packed */
		const int XVID_CSP_SLICE   = (1<<12); /* decoder only: 4:2:0 planar, per slice rendering */
		const int XVID_CSP_INTERNAL= (1<<13); /* decoder only: 4:2:0 planar, returns ptrs to internal buffers */
		const int XVID_CSP_NULL    = (1<<14); /* decoder only: dont output anything */
		const int XVID_CSP_VFLIP   = (1<<31); /* vertical flip mask */

		static int XVID_MAKE_VERSION(int a,int b,int c)
		{
			return ((((a)&0xff)<<16) | (((b)&0xff)<<8) | ((c)&0xff));
		}
		static byte XVID_VERSION_MAJOR(int a)
		{
			return ((byte)(((a)>>16) & 0xff));
		}
		static byte XVID_VERSION_MINOR(int a)
		{
			return ((byte)(((a)>> 8) & 0xff));
		}
		static byte XVID_VERSION_PATCH(int a)
		{
			return ((byte)(((a)>> 0) & 0xff));
		}
		static int XVID_MAKE_API(int a,int b)
		{
			return ((((a)&0xff)<<16) | (((b)&0xff)<<0));
		}
		static int XVID_API_MAJOR(int a)
		{
			return (((a)>>16) & 0xff);
		}
		static int XVID_API_MINOR(int a)
		{
			return (((a)>> 0) & 0xff);
		}
		
		static int XVID_VERSION=XVID_MAKE_VERSION(1,0,-123);
		static int XVID_API=XVID_MAKE_API(4, 0);

		[DllImport(XVIDLIB)]
		private static extern int xvid_decore(IntPtr handle, int opt, ref xvid_dec_create_t param1, IntPtr param2);
		[DllImport(XVIDLIB)]
		private static extern int xvid_decore(IntPtr handle, int opt, ref xvid_dec_frame_t param1, IntPtr param2);
		//[DllImport(XVIDLIB)]
		//private static extern int xvid_decore(IntPtr handle, int opt, void *param1, void *param2);
		
		[DllImport(XVIDLIB)]
		private static extern int xvid_global(IntPtr handle, int opt, ref xvid_gbl_init_t param1, IntPtr param2);

		public XviD(int w,int h)
		{
			xvid_gbl_init_t   xvid_gbl_init=new xvid_gbl_init_t();
			xvid_dec_create_t xvid_dec_create=new xvid_dec_create_t();

			xvid_gbl_init.version = XVID_VERSION;
			xvid_gbl_init.cpu_flags = XVID_CPU_FORCE;
			
			xvid_global(IntPtr.Zero, 0, ref xvid_gbl_init, IntPtr.Zero);
			
			xvid_dec_create.version = XVID_VERSION;

			/*
			 * Image dimensions -- set to 0, xvidcore will resize when ever it is
			 * needed
			 */
			xvid_dec_create.width = Width=w;
			xvid_dec_create.height = Height=h;

			int ret = xvid_decore(IntPtr.Zero, XVID_DEC_CREATE, ref xvid_dec_create, IntPtr.Zero);

			if(ret!=0)
				throw new XviDException("XviD Decore Init Error.");

			dec_handle = xvid_dec_create.handle;

		}

		public void Decode(byte[] inbuf,int insize,byte[] outbuf)
		{
			int ret;

			xvid_dec_frame_t xvid_dec_frame=new xvid_dec_frame_t();

			/* Set version */
			xvid_dec_frame.version = XVID_VERSION;
			//xvid_dec_stats->version = XVID_VERSION;

			/* No general flags to set */
			xvid_dec_frame.general          = 0;

            /*
			fixed(void *inptr=inbuf,outptr=outbuf)
			{
				xvid_dec_frame.bitstream        = inptr;
				xvid_dec_frame.length           = insize;

				xvid_dec_frame.output.plane1  = outptr;
				xvid_dec_frame.output.stride1 = Width*3;
				xvid_dec_frame.output.csp       = XVID_CSP_BGR;

				ret = xvid_decore(dec_handle, XVID_DEC_DECODE, ref xvid_dec_frame, IntPtr.Zero);
			}
            */

            GCHandle h0 = GCHandle.Alloc(inbuf, GCHandleType.Pinned);
            GCHandle h1 = GCHandle.Alloc(outbuf, GCHandleType.Pinned);
            xvid_dec_frame.bitstream = h0.AddrOfPinnedObject();
            xvid_dec_frame.length = insize;

            xvid_dec_frame.output.plane1 = h1.AddrOfPinnedObject();
            xvid_dec_frame.output.stride1 = Width * 3;
            xvid_dec_frame.output.csp = XVID_CSP_BGR;

            ret = xvid_decore(dec_handle, XVID_DEC_DECODE, ref xvid_dec_frame, IntPtr.Zero);

            h0.Free();
            h1.Free();
		}

		private IntPtr dec_handle;
		public int Width,Height;
	}
}
