using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SZKG_Photoshop_O75HNX
{
	public class ImageProcessingAlgorithms
	{
		public static Bitmap InvertImage(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int stride = bmData.Stride;
			System.IntPtr Scan0 = bmData.Scan0;
			unsafe
			{
				byte* p = (byte*)(void*)Scan0;
				int nOffset = stride - bmData.Width * 3;
				int nWidth = bmData.Width * 3;

				for (int y = 0; y < bmData.Height; ++y)
				{
					for (int x = 0; x < nWidth; ++x)
					{
						p[0] = (byte)(255 - p[0]);
						++p;
					}
					p += nOffset;
				}
			}

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ApplyGammaCorrection(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ApplyLogTransform(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ConvertToGrayscale(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int stride = bmData.Stride;
			System.IntPtr Scan0 = bmData.Scan0;
			unsafe
			{
				byte* p = (byte*)(void*)Scan0;
				int nOffset = stride - bmData.Width * 3;
				byte red, green, blue;

				for (int y = 0; y < bmData.Height; ++y)
				{
					for (int x = 0; x < bmData.Width; ++x)
					{
						blue = p[0];
						green = p[1];
						red = p[2];

						p[0] = p[1] = p[2] = (byte)(0.299 * red + 0.587 * green + 0.114 * blue);
						p += 3;
					}
					p += nOffset;
				}
			}

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ComputeHistogram(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap EqualizeHistogram(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ApplyBoxFilter(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ApplyGaussianFilter(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ApplySobelEdgeDetection(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ApplyLaplacianEdgeDetection(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap DetectKeypoints(Bitmap sourceImage)
		{
			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			//TODO: 

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}
	}
}
