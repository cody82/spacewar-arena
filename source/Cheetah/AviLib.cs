using System;
using System.IO;
using System.Text;

namespace AviLib
{
	public class AviException : Exception
	{
		public AviException(string text):base(text)
		{
		}
	}

	struct video_index_entry
	{
		public long pos;
		public long len;
	}

	struct audio_index_entry
	{
		public long pos;
		public long len;
		public long tot;
	}

	/// <summary>
	/// Summary description for AviLib.
	/// </summary>
	public class AviFile
	{
		private const int HEADERBYTES=2048;
		private const int AVI_MAX_LEN=2000000000;

		//PAD_EVEN(x) ( ((x)+1) & ~1 )
		private static int pad_even(int x)
		{
			return ( ((x)+1) & ~1 );
		}
		private static uint pad_even(uint x)
		{
			return (uint)( ((x)+1) & ~1 );
		}

/* Copy n into dst as a 4 byte, little endian number.
   Should also work on big endian machines */
		private static void long2str(byte[] dst, int index,int n)
		{
			dst[index+0] = (byte)((n    )&0xff);
			dst[index+1] = (byte)((n>> 8)&0xff);
			dst[index+2] = (byte)((n>>16)&0xff);
			dst[index+3] = (byte)((n>>24)&0xff);
		}
		private static void long2str(byte[] dst, int index,uint n)
		{
			dst[index+0] = (byte)((n    )&0xff);
			dst[index+1] = (byte)((n>> 8)&0xff);
			dst[index+2] = (byte)((n>>16)&0xff);
			dst[index+3] = (byte)((n>>24)&0xff);
		}
		
		/* Calculate audio sample size from number of bits and number of channels.
		   This may have to be adjusted for eg. 12 bits and stereo */
		private int avi_sampsize()
		{
			int s;
			s = (((int)a_bits+7)/8)*(int)a_chans;
			if(s==0) s=1; /* avoid possible zero divisions */
			return s;
		}

/* Convert a string of 4 or 2 bytes to a number,
   also working on big endian machines */
		private static int str2long(byte[] str,int index)
		{
			return ( (int)str[index+0] | ((int)str[index+1]<<8) | ((int)str[index+2]<<16) | ((int)str[index+3]<<24) );
		}
		private static uint str2ulong(byte[] str,int index)
		{
			return (uint)( str[index+0] | (str[index+1]<<8) | (str[index+2]<<16) | (str[index+3]<<24) );
		}

		static uint str2ushort(byte[] str,int index)
		{
			return (uint)( str[index+0] | (str[index+1]<<8) );
		}
		static int str2short(byte[] str,int index)
		{
			return ( (int)str[index+0] | ((int)str[index+1]<<8) );
		}

		public AviFile(Stream source)
		{
			Open(source);
		}

		public AviFile(string filename)
		{
			Open(new FileStream(filename,FileMode.Open));
		}

		protected void Open(Stream source)
		{
			byte[] buffer=new byte[256];
			int n;
			byte[] hdrl_data=null;

			AviStream=source;

			if(AviStream.Read(buffer,0,12)!=12)
				throw new AviException("Cant read first 12 bytes from Stream.");

			string riff=Encoding.ASCII.GetString(buffer,0,4);
			string avi=Encoding.ASCII.GetString(buffer,8,4);
			if(riff!="RIFF"||avi!="AVI ")
				throw new AviException("Not an Avi File.");

			while(true)
			{
				if(AviStream.Read(buffer,0,8)!=8)
					break;

				n=str2long(buffer,4);
				n=pad_even(n);

				string label=Encoding.ASCII.GetString(buffer,0,4);
				//Console.WriteLine("found label: "+label);
				label=label.ToUpper();
				switch(label)
				{
					case "LIST":
					{
						if(AviStream.Read(buffer,0,4)!=4)
							throw new AviException("Read Error.");
						n-=4;
						label=Encoding.ASCII.GetString(buffer,0,4);
						//Console.WriteLine("found label2: "+label);
						label=label.ToUpper();
						switch(label)
						{
							case "HDRL":
								hdrl_data=new byte[n];
								if(AviStream.Read(hdrl_data,0,n)!=n)
									throw new AviException("Read Error.");
								break;
							case "MOVI":
								movi_start=(int)AviStream.Position;
								AviStream.Seek(n,SeekOrigin.Current);
								break;
							default:
								AviStream.Seek(n,SeekOrigin.Current);
								break;
						}
						break;
					}
					case "IDX1":
					{
						n_idx = max_idx = n/16;
						idx=new byte[n];
						if(AviStream.Read(idx,0,n)!=n)
							throw new AviException("Read Error.");
						break;
					}
					default:
						AviStream.Seek(n,SeekOrigin.Current);
						break;
				}
			}

			if(hdrl_data==null)
				throw new AviException("No HDRL.");
			if(movi_start==0)
				throw new AviException("No MOVI.");

			int lasttag=0;
			int num_stream=0;
			bool vids_strh_seen = false;
			bool vids_strf_seen = false;
			bool auds_strh_seen = false;
			//bool auds_strf_seen = false;

			for(int i=0;i<hdrl_data.Length;)
			{
				string label=Encoding.ASCII.GetString(hdrl_data,i,4).ToUpper();
				//Console.WriteLine("hdrl label: "+label);
				if(label=="LIST")
				{
					i+=12;
					continue;
				}
				n=str2long(hdrl_data,i+4);
				n=pad_even(n);

				if(label=="STRH")
				{
					i+=8;
					label=Encoding.ASCII.GetString(hdrl_data,i,4).ToUpper();
					if(label=="VIDS"&& !vids_strh_seen)
					{
						compressor=Encoding.ASCII.GetString(hdrl_data,i+4,4);
						int scale = str2long(hdrl_data,i+20);
						int rate  = str2long(hdrl_data,i+24);
						if(scale!=0)fps = (double)rate/(double)scale;
						video_frames = str2long(hdrl_data,i+32);
						video_strn = num_stream;
						vids_strh_seen = true;
						lasttag = 1; /* vids */
					}
					else if(label=="AUDS"&& ! auds_strh_seen)
					{
						audio_bytes = str2long(hdrl_data,i+32)*avi_sampsize();
						audio_strn = num_stream;
						auds_strh_seen = true;
						lasttag = 2; /* auds */
					}
					else
						lasttag=0;
					num_stream++;
				}
				else if(label=="STRF")
				{
					i+=8;
					if(lasttag==1)
					{
						width  = str2long(hdrl_data,i+4);
						height = str2long(hdrl_data,i+8);
						vids_strf_seen = true;
					}
					else if(lasttag==2)
					{
						a_fmt   = str2short(hdrl_data,i  );
						a_chans = str2short(hdrl_data,i+2);
						a_rate  = str2long (hdrl_data,i+4);
						a_bits  = str2short(hdrl_data,i+14);
						//auds_strf_seen = true;
					}
					lasttag=0;
				}
				else
				{
					i += 8;
					lasttag = 0;
				}

				i += n;

			}

			if(!vids_strh_seen || !vids_strf_seen || video_frames==0)
				throw new AviException("No Video.");
			
			video_tag=Encoding.ASCII.GetString(new byte[]
				{
					(byte)(video_strn/10 + '0'),
					(byte)(video_strn%10 + '0'),
					(byte)'d',
					(byte)'b'
				});

			/* Audio tag is set to "99wb" if no audio present */
			if(a_chans==0) audio_strn = 99;

			audio_tag=Encoding.ASCII.GetString(new byte[]
				{
					(byte)(audio_strn/10 + '0'),
					(byte)(audio_strn%10 + '0'),
					(byte)'w',
					(byte)'b'
				});



			int idx_type=0;
			long nvi=0, nai=0, ioff=0;
			long tot=0;

			if(idx!=null)
			{
				int pos;
				int len;
				
				/* Search the first videoframe in the idx1 and look where
					it is in the file */
				int i;
				for(i=0;i<n_idx;i++)
				{
					string s=Encoding.ASCII.GetString(idx,i,3);
					if(s==video_tag.Substring(0,3))
						break;
				}

				if(i>=n_idx)
					throw new AviException("No Vids.");
				
				pos = str2long(idx,i+ 8);
				len = str2long(idx,i+12);
      
				AviStream.Seek(pos,SeekOrigin.Begin);
				if(AviStream.Read(buffer,0,8)!=8)
					throw new AviException("Read Error.");
				string s1=Encoding.ASCII.GetString(idx,i,4);
				string s2=Encoding.ASCII.GetString(buffer,0,4);
				if( s1==s2 && str2long(buffer,4)==len )
				{
					idx_type = 1; /* Index from start of file */
				}
				else
				{
 					AviStream.Seek(pos+movi_start-4,SeekOrigin.Begin);
					if(AviStream.Read(buffer,0,8)!=8)
						throw new AviException("Read Error.");
					s1=Encoding.ASCII.GetString(idx,i,4);
					s2=Encoding.ASCII.GetString(buffer,0,4);
					if( s1==s2 && str2long(buffer,4)==len )
					{
						idx_type = 2; /* Index from start of movi list */
					}
				}
				/* idx_type remains 0 if neither of the two tests above succeeds */

			}
			
			if(idx_type == 0)
			{
				/* we must search through the file to get the index */
				throw new AviException("NYI.");
			}
			
			for(int i=0;i<n_idx;i++)
			{
				string s1=Encoding.ASCII.GetString(idx,i*16,3);
				if(video_tag.Substring(0,3)==s1)
					nvi++;
				s1=Encoding.ASCII.GetString(idx,i*16,4);
				if(audio_tag==s1)nai++;
			}
			
			video_frames = nvi;
			audio_chunks = nai;

			if(video_frames==0)
				throw new AviException("No Vids.");

			video_index = new video_index_entry[nvi];

			if(audio_chunks>0)
			{
				audio_index = new audio_index_entry[nai];
 			}
			
			nvi = 0;
			nai = 0;
			tot = 0;
			ioff = idx_type == 1 ? 8 : movi_start+4;

			for(int i=0;i<n_idx;i++)
			{
				int j=i*16;
				string s1=Encoding.ASCII.GetString(idx,j,3);
				string s2=Encoding.ASCII.GetString(idx,j,4);
				//if(video_tag.Substring(0,3)==s1)nvi++;
				//s1=Encoding.ASCII.GetString(idx,i,4);

				if(s1==video_tag.Substring(0,3))
				{
					video_index[nvi].pos = str2long(idx,j+ 8)+ioff;
					video_index[nvi].len = str2long(idx,j+12);
					nvi++;
				}
				else if(s2==audio_tag)
				{
					audio_index[nai].pos = str2long(idx,j+ 8)+ioff;
					audio_index[nai].len = str2long(idx,j+12);
					audio_index[nai].tot = tot;
					tot += audio_index[nai].len;
					nai++;
				}
				else throw new AviException("Unknown tag found: "+s2+".");
			}

			audio_bytes = tot;

			/* Reposition the file */

			AviStream.Seek(movi_start,SeekOrigin.Begin);
			video_pos = 0;

		}

		public int FrameSize(int frame)
		{
			//if(AVI->mode==AVI_MODE_WRITE) { AVI_errno = AVI_ERR_NOT_PERM; return -1; }
			//if(!AVI->video_index)         { AVI_errno = AVI_ERR_NO_IDX;   return -1; }

			//if(frame < 0 || frame >= AVI->video_frames) return 0;
			int n=(int)(video_index[frame].len);
			if(n==0)
				throw new AviException("Assert: FrameSize>0.");
			return n;

		}
		
		private void avi_add_chunk(byte[] tag, byte[] data, int length)
		{
		byte[] c=new byte[8];

		/* Copy tag and length int c, so that we need only 1 write system call
		   for these two values */

		Array.Copy(tag,c,4);
		long2str(c,4,length);

		/* Output tag, length and data, restore previous position
		   if the write fails */

		length = pad_even(length);

		AviStream.Write(c,0,8);
		AviStream.Write(data,0,length);

	/* Update file position */

	pos += 8 + length;

	//return 0;
}

	private void avi_add_index_entry(byte[] tag, int flags, int pos, int len)
	{
		byte[] ptr;

		if(n_idx>=max_idx)
		{
			ptr=new byte[(max_idx+4096)*16];
			Array.Copy(idx,ptr,idx.Length);
			max_idx += 4096;
		}

   /* Add index entry */

		Array.Copy(tag,0,idx,n_idx*16,4);

		long2str(idx,n_idx*16+ 4,flags);
		long2str(idx,n_idx*16+ 8,pos);
		long2str(idx,n_idx*16+ 12,len);

   /* Update counter */

n_idx++;

//return 0;
}
		public int ReadFrame(byte[] buffer,int frame)
		{
			int n = (int)video_index[frame].len;

			AviStream.Seek(video_index[frame].pos,SeekOrigin.Begin);
			if (AviStream.Read(buffer,0,n) != n)
			{
				throw new AviException("Read Error.");
			}

			return n;
		}

		public int ReadFrame(byte[] buffer)
		{
			int n = ReadFrame(buffer,(int)video_pos);

			video_pos++;
			if(Loop&&video_pos>=video_frames)
				video_pos=0;

			return n;
		}

		public float Duration
		{
			get
			{
				return (float)video_frames/(float)fps;
			}
		}

		public int Width
		{
			get
			{
				return (int)width;
			}
		}
		
		public int Height
		{
			get
			{
				return (int)height;
			}
		}

		public float FrameTime
		{
			get
			{
				return 1/(float)fps;
			}
		}

		public float Fps
		{
			get
			{
				return (float)fps;
			}
		}

		public string Compressor
		{
			get
			{
				return compressor;
			}
		}

		public int AvgFrameSize
		{
			get
			{
				int avg=0;
				for(int i=0;i<FrameCount;++i)
				{
					avg+=FrameSize(i);
				}
				avg/=FrameCount;
				return avg;
			}
		}
		
		public int MaxFrameSize
		{
			get
			{
				int max=0;
				for(int i=0;i<FrameCount;++i)
				{
					max=Math.Max(max,FrameSize(i));
				}
				return max;
			}
		}

		public int FrameCount
		{
			get
			{
				return (int)video_frames;
			}
		}

		public bool Loop
		{
			get
			{
				return loop;
			}
			set
			{
				loop=value;
			}
		}

		protected Stream AviStream;
		protected int movi_start;
		protected int n_idx;
		protected int max_idx;
		protected byte[] idx;
		protected bool loop=true;

		protected long   width;             /* Width  of a video frame */
		protected long   height;            /* Height of a video frame */
		protected double fps;               /* Frames per second */
		protected string compressor;     /* Type of compressor, 4 bytes + padding for 0 byte */
		protected long   video_strn;        /* Video stream number */
		protected long   video_frames;      /* Number of video frames */
		protected string video_tag;      /* Tag of video data */
		protected long   video_pos;         /* Number of next frame to be read
                                (if index present) */

		protected long   a_fmt;             /* Audio format, see #defines below */
		protected long   a_chans;           /* Audio channels, 0 for no audio */
		protected long   a_rate;            /* Rate in Hz */
		protected long   a_bits;            /* bits per audio sample */
		protected long   audio_strn;        /* Audio stream number */
		protected long   audio_bytes;       /* Total number of bytes of audio data */
		protected long   audio_chunks;      /* Chunks of audio data in the file */
		protected string audio_tag;      /* Tag of audio data */
		protected long   audio_posc;        /* Audio position: chunk */
		protected long   audio_posb;        /* Audio position: byte within chunk */
	
		private video_index_entry[] video_index;
		private audio_index_entry[] audio_index;
		protected long   last_pos;          /* Position of last frame written */
		protected long   last_len;          /* Length of last frame written */
		protected int    must_use_index;    /* Flag if frames are duplicated */
		protected long   pos;               /* position in file */

	}
}
