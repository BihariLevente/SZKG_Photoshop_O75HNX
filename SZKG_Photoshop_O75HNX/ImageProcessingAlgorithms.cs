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
		public static Bitmap InvertImage(Bitmap srcImage)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData bmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // teljes sorhossz (stride) = nWidth (hasznos bájtok száma) + nOffset (igazítás 4 byte-ra)
            int stride = bmData.Stride;

            unsafe
			{
                byte* pBase = (byte*)bmData.Scan0;
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

			srcImage.UnlockBits(bmData);

			return srcImage;
		}

		public static Bitmap ApplyGammaCorrection(Bitmap srcImage, double gamma = 1)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			// Gamma LUT (look up table)
			byte[] gammaLUT = new byte[256];

			for (int i = 0; i < 256; i++)
			{
				gammaLUT[i] = (byte)Math.Min(255, (int)(255.0 * Math.Pow(i / 255.0, gamma))); // g(x,y) = 255 * (f(x,y) / 255)^gamma
			}

			BitmapData bmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int stride = bmData.Stride;

			unsafe
			{
				byte* pBase = (byte*)bmData.Scan0;
				int rowBytes = imgWidthPix * 3;

				System.Threading.Tasks.Parallel.For(0, imgHeightPix, y =>
				{
					byte* p = pBase + y * stride;

					for (int x = 0; x < rowBytes; x++)
					{
						p[x] = gammaLUT[p[x]];
					}
				});
			}

			srcImage.UnlockBits(bmData);

			return srcImage;
		}

		public static Bitmap ApplyLogTransform(Bitmap srcImage, double c = 1)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			// Gamma LUT (look up table)
			byte[] logLUT = new byte[256];

			for (int i = 0; i < 256; i++)
			{
				logLUT[i] = (byte)Math.Min(255, (c * Math.Log(1 + i))); // g(x,y) = c * log(1 + f(x,y))
			}

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int stride = srcBmData.Stride;

			unsafe
			{
				byte* pBase = (byte*)srcBmData.Scan0;
				int rowBytes = imgWidthPix * 3;

				System.Threading.Tasks.Parallel.For(0, imgHeightPix, y =>
				{
					byte* p = pBase + y * stride;

					for (int x = 0; x < rowBytes; x++)
					{
						p[x] = logLUT[p[x]];
					}
				});
			}

			srcImage.UnlockBits(srcBmData);

			return srcImage;
		}

        public static Bitmap ConvertToGrayscale(Bitmap srcImage)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = srcBmData.Stride;

            unsafe
            {
                byte* pBase = (byte*)srcBmData.Scan0;
                int rowBytes = imgWidthPix * 3;

                System.Threading.Tasks.Parallel.For(0, imgHeightPix, y =>
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

            srcImage.UnlockBits(srcBmData);

            return srcImage;
        }

        public static (int[], int[], int[]) ComputeHistogram(Bitmap srcImage)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            int[] histR = new int[256];
            int[] histG = new int[256];
            int[] histB = new int[256];

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = srcBmData.Stride;

            unsafe
            {
                byte* pBase = (byte*)srcBmData.Scan0;
                int rowBytes = imgWidthPix * 3;

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

                System.Threading.Tasks.Parallel.For(0, imgHeightPix, new ParallelOptions { MaxDegreeOfParallelism = processorCount }, y =>
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

            srcImage.UnlockBits(srcBmData);

            return (histR, histG, histB);
        }

        public static (int[], int[], int[]) EqualizeHistogram(int[] histR, int[] histG, int[] histB)
		{
            int L = 256;
            int totalPixelsR = histR.Sum();
            int totalPixelsG = histG.Sum();
            int totalPixelsB = histB.Sum();

            double[] pR = new double[L];
            double[] pG = new double[L];
            double[] pB = new double[L];

            for (int i = 0; i < L; i++)
            {
                pR[i] = (double)histR[i] / totalPixelsR;
                pG[i] = (double)histG[i] / totalPixelsG;
                pB[i] = (double)histB[i] / totalPixelsB;
            }

            double[] cdfR = new double[L];
            double[] cdfG = new double[L];
            double[] cdfB = new double[L];

            cdfR[0] = pR[0];
            cdfG[0] = pG[0];
            cdfB[0] = pB[0];

            for (int i = 1; i < L; i++)
            {
                cdfR[i] = cdfR[i - 1] + pR[i];
                cdfG[i] = cdfG[i - 1] + pG[i];
                cdfB[i] = cdfB[i - 1] + pB[i];
            }

            int[] mapR = new int[L];
            int[] mapG = new int[L];
            int[] mapB = new int[L];

            for (int i = 0; i < L; i++)
            {
                mapR[i] = (int)Math.Round((L - 1) * cdfR[i]);
                mapG[i] = (int)Math.Round((L - 1) * cdfG[i]);
                mapB[i] = (int)Math.Round((L - 1) * cdfB[i]);
            }

            int[] eqHistR = new int[L];
            int[] eqHistG = new int[L];
            int[] eqHistB = new int[L];

            for (int i = 0; i < L; i++)
            {
                eqHistR[mapR[i]] += histR[i];
                eqHistG[mapG[i]] += histG[i];
                eqHistB[mapB[i]] += histB[i];
            }

            return (eqHistR, eqHistG, eqHistB);
        }

        public static Bitmap ShowHistogram(int[] histR, int[] histG, int[] histB, int panelHeight = 150)
        {
            int histBitmapHeight = panelHeight * 3;
            Bitmap histImage = new Bitmap(256, histBitmapHeight);

            int globalMax = Math.Max(histR.Max(), Math.Max(histG.Max(), histB.Max()));

            using (Graphics g = Graphics.FromImage(histImage))
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

            return histImage;
        }

        public static Bitmap ApplyBoxFilter(Bitmap srcImage, int kernelSize = 3)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format24bppRgb);

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcBmData.Stride;

            unsafe
            {
                byte* pSrcBase = (byte*)srcBmData.Scan0;
                byte* pDstBase = (byte*)dstBmData.Scan0;
                int offset = kernelSize / 2;

                Parallel.For(0, imgHeightPix, y =>
                {
                    for (int x = 0; x < imgWidthPix; x++)
                    {
                        int sumR = 0, sumG = 0, sumB = 0;
                        int count = 0;

                        for (int fy = -offset; fy <= offset; fy++)
                        {
                            int iy = y + fy;
                            if (iy < 0 || iy >= imgHeightPix) continue;

                            byte* pRow = pSrcBase + iy * stride;

                            for (int fx = -offset; fx <= offset; fx++)
                            {
                                int ix = x + fx;
                                if (ix < 0 || ix >= imgWidthPix) continue;

                                byte* pPixel = pRow + ix * 3;
                                sumB += pPixel[0];
                                sumG += pPixel[1];
                                sumR += pPixel[2];
                                count++;
                            }
                        }

                        byte* pDstPixel = pDstBase + y * stride + x * 3;
                        pDstPixel[0] = (byte)(sumB / count);
                        pDstPixel[1] = (byte)(sumG / count);
                        pDstPixel[2] = (byte)(sumR / count);
                    }
                });
            }

            srcImage.UnlockBits(srcBmData);
            dstImage.UnlockBits(dstBmData);

            return dstImage;
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
