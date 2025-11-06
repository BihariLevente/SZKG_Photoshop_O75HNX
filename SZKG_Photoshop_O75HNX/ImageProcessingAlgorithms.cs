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
			int imgWidthPix = sourceImage.Width;
			int imgHeightPix = sourceImage.Height;

			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			// teljes sorhossz (stride) = nWidth (hasznos bájtok száma) + nOffset (igazítás 4 byte-ra)
			int stride = bmData.Stride;
			IntPtr Scan0 = bmData.Scan0;

			unsafe
			{
				byte* pBase = (byte*)(void*)Scan0;
				int rowBytes = imgWidthPix * 3;
				//int nOffset = stride - nWidth;

				System.Threading.Tasks.Parallel.For(0, imgHeightPix, x =>
				{
					byte* p = pBase + x * stride; // új sor

					for (int y = 0; y < rowBytes; ++y)
					{
						p[0] = (byte)(255 - p[0]);
						++p;
					}
				});
			}

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ApplyGammaCorrection(Bitmap sourceImage, double gamma = 1)
		{
			int imgWidth = sourceImage.Width;
			int imgHeight = sourceImage.Height;

			// Gamma LUT (look up table)
			byte[] gammaLUT = new byte[256];

			for (int i = 0; i < 256; i++)
			{
				gammaLUT[i] = (byte)Math.Min(255, (int)(255.0 * Math.Pow(i / 255.0, gamma))); // g(x,y) = 255 * (f(x,y) / 255)^gamma
			}

			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, imgWidth, imgHeight), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int stride = bmData.Stride;
			IntPtr scan0 = bmData.Scan0;

			unsafe
			{
				byte* pBase = (byte*)scan0;
				int rowBytes = imgWidth * 3;

				System.Threading.Tasks.Parallel.For(0, imgHeight, y =>
				{
					byte* p = pBase + y * stride;

					for (int x = 0; x < rowBytes; x++)
					{
						p[x] = gammaLUT[p[x]];
					}
				});
			}

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

		public static Bitmap ApplyLogTransform(Bitmap sourceImage, double c = 1)
		{
			int imgWidth = sourceImage.Width;
			int imgHeight = sourceImage.Height;

			// Gamma LUT (look up table)
			byte[] logLUT = new byte[256];

			for (int i = 0; i < 256; i++)
			{
				logLUT[i] = (byte)Math.Min(255, (c * Math.Log(1 + i))); // g(x,y) = c * log(1 + f(x,y))
			}

			BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, imgWidth, imgHeight),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int stride = bmData.Stride;
			IntPtr scan0 = bmData.Scan0;

			unsafe
			{
				byte* pBase = (byte*)scan0;
				int rowBytes = imgWidth * 3;

				System.Threading.Tasks.Parallel.For(0, imgHeight, y =>
				{
					byte* p = pBase + y * stride;

					for (int x = 0; x < rowBytes; x++)
					{
						p[x] = logLUT[p[x]];
					}
				});
			}

			sourceImage.UnlockBits(bmData);
			return sourceImage;
		}

        public static Bitmap ConvertToGrayscale(Bitmap sourceImage)
        {
            int imgWidth = sourceImage.Width;
            int imgHeight = sourceImage.Height;

            BitmapData bmData = sourceImage.LockBits(new Rectangle(0, 0, imgWidth, imgHeight),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            IntPtr scan0 = bmData.Scan0;

            unsafe
            {
                byte* pBase = (byte*)scan0;
                int rowBytes = imgWidth * 3;

                System.Threading.Tasks.Parallel.For(0, imgHeight, y =>
                {
                    byte* p = pBase + y * stride;

                    for (int x = 0; x < rowBytes; x += 3)
                    {
                        byte blue = p[x];
                        byte green = p[x + 1];
                        byte red = p[x + 2];

                        byte gray = (byte)(0.299 * red + 0.587 * green + 0.114 * blue);

                        p[x] = p[x + 1] = p[x + 2] = gray;
                    }
                });
            }

            sourceImage.UnlockBits(bmData);
            return sourceImage;
        }

        public static Bitmap ComputeHistogram(Bitmap sourceImage)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;

            int[] histR = new int[256];
            int[] histG = new int[256];
            int[] histB = new int[256];

            BitmapData bmData = sourceImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            IntPtr scan0 = bmData.Scan0;

            unsafe
            {
                byte* pBase = (byte*)scan0;
                int rowBytes = width * 3;

                int processorCount = Environment.ProcessorCount;
                int[][] localR = new int[processorCount][];
                int[][] localG = new int[processorCount][];
                int[][] localB = new int[processorCount][];

                for (int i = 0; i < processorCount; i++)
                {
                    localR[i] = new int[256];
                    localG[i] = new int[256];
                    localB[i] = new int[256];
                }

                System.Threading.Tasks.Parallel.For(0, height, new ParallelOptions { MaxDegreeOfParallelism = processorCount }, y =>
                {
                    int threadIndex = Thread.GetCurrentProcessorId() % processorCount;
                    byte* p = pBase + y * stride;

                    for (int x = 0; x < rowBytes; x += 3)
                    {
                        byte b = p[x];
                        byte g = p[x + 1];
                        byte r = p[x + 2];

                        localR[threadIndex][r]++;
                        localG[threadIndex][g]++;
                        localB[threadIndex][b]++;
                    }
                });

                for (int i = 0; i < processorCount; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        histR[j] += localR[i][j];
                        histG[j] += localG[i][j];
                        histB[j] += localB[i][j];
                    }
                }
            }

            sourceImage.UnlockBits(bmData);

            int panelHeight = 150;
            int histBitmapHeight = panelHeight * 3;
            Bitmap histBitmap = new Bitmap(256, histBitmapHeight);

            int globalMax = Math.Max(histR.Max(), Math.Max(histG.Max(), histB.Max()));

            using (Graphics g = Graphics.FromImage(histBitmap))
            {
                g.Clear(Color.Black);

                for (int i = 0; i < 256; i++)
                {
                    int rHeight = histR[i] * panelHeight / globalMax;
                    g.DrawLine(Pens.Red, i, panelHeight, i, panelHeight - rHeight);
                }

                for (int i = 0; i < 256; i++)
                {
                    int gHeight = histG[i] * panelHeight / globalMax;
                    g.DrawLine(Pens.Lime, i, panelHeight * 2, i, panelHeight * 2 - gHeight);
                }

                for (int i = 0; i < 256; i++)
                {
                    int bHeight = histB[i] * panelHeight / globalMax;
                    g.DrawLine(Pens.Blue, i, panelHeight * 3, i, panelHeight * 3 - bHeight);
                }
            }

            return histBitmap;
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
