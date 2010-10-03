using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tao.FFmpeg;

namespace Cheetah
{
    public class FFmpegFile
    {
        static FFmpegFile()
        {
            FFmpeg.av_register_all();
        }

        IntPtr context;

        IntPtr OpenFile(string filename)
        {
            IntPtr context;
            if (FFmpeg.av_open_input_file(out context, filename, IntPtr.Zero, 0, IntPtr.Zero) != 0)
            {
                throw new Exception("FFmpeg: cant open file " + filename);
            }
            return context;
        }

        public FFmpegFile(string filename)
        {
            context = OpenFile(filename);
        }

        public FFmpegFile()
        {
        }
    }
}
