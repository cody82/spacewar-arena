using System;
using System.Drawing;
using System.Drawing.Imaging;
using Tao.DevIl;
using System.Runtime.InteropServices;
using System.IO;

namespace Cheetah
{
	public class DevIlDdsDecode
	{
/// <summary>
/// Converts an in-memory image in DDS format to a System.Drawing.Bitmap
/// object for easy display in Windows forms.
/// </summary>
/// <param name="DDSData">Byte array containing DDS image data</param>
/// <returns>A Bitmap object that can be displayed</returns>
public static Bitmap DDSDataToBMP(byte[ ] DDSData) {
  // Create a DevIL image "name" (which is actually a number)
  int img_name;
  Il.ilGenImages(1, out img_name);
  Il.ilBindImage(img_name);

  // Load the DDS file into the bound DevIL image
  Il.ilLoadL(Il.IL_DDS, DDSData, DDSData.Length);

  // Set a few size variables that will simplify later code

  int ImgWidth = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
  int ImgHeight = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
  Rectangle rect = new Rectangle(0, 0, ImgWidth, ImgHeight);

  // Convert the DevIL image to a pixel byte array to copy into Bitmap
  Il.ilConvertImage(Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);

  // Create a Bitmap to copy the image into, and prepare it to get data
  Bitmap bmp = new Bitmap(ImgWidth, ImgHeight);
  BitmapData bmd =
    bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

  // Copy the pixel byte array from the DevIL image to the Bitmap
  Il.ilCopyPixels(0, 0, 0,
    Il.ilGetInteger(Il.IL_IMAGE_WIDTH),
    Il.ilGetInteger(Il.IL_IMAGE_HEIGHT),
    1, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE,
    bmd.Scan0);

  // Clean up and return Bitmap
  Il.ilDeleteImages(1, ref img_name);
  bmp.UnlockBits(bmd);
  return bmp;
}
		
	}
	
	public class DevIlDdsDecoder : Cheetah.Graphics.IImageDecoder
	{
		public void Load(Stream stream)
		{
			byte[] dds = new byte[stream.Length]; 
			stream.Read(dds,0,(int)stream.Length);
			data = DDSDataToRGBA(dds);
		}
		
		
		public byte[] DDSDataToRGBA(byte[ ] DDSData) {
  // Create a DevIL image "name" (which is actually a number)
  int img_name;
  Il.ilGenImages(1, out img_name);
  Il.ilBindImage(img_name);

  // Load the DDS file into the bound DevIL image
  Il.ilLoadL(Il.IL_DDS, DDSData, DDSData.Length);

  // Set a few size variables that will simplify later code

  width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
  height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);

  // Convert the DevIL image to a pixel byte array to copy into Bitmap
  Il.ilConvertImage(Il.IL_RGBA, Il.IL_UNSIGNED_BYTE);

	data=new byte[width*height*4];
			
	IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(data,0);
  // Copy the pixel byte array from the DevIL image to the Bitmap
  Il.ilCopyPixels(0, 0, 0,
    Il.ilGetInteger(Il.IL_IMAGE_WIDTH),
    Il.ilGetInteger(Il.IL_IMAGE_HEIGHT),
    1, Il.IL_RGBA, Il.IL_UNSIGNED_BYTE,
    p);
			
  Il.ilDeleteImages(1, ref img_name);
  return data;
}
		
		public byte[] getRGBA()
		{
			return data;
		}

		public int getWidth()
		{
			return width;
		}

		public int getHeight()
		{
			return height;
		}
		
		public bool hasAlpha()
		{
			return false;
		}

		protected byte[] data;
		protected int width;
		protected int height;
	}
}

