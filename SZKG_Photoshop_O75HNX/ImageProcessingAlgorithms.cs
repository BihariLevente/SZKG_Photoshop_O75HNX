using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Intrinsics.Arm;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SZKG_Photoshop_O75HNX
{
    public class ImageProcessingAlgorithms
	{
		public static Bitmap InvertImage(Bitmap srcImage)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			// srcImage.PixelFormat = Format32bppArgb
			// BGRA!!!
			BitmapData bmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix), 
				ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

			// teljes sorhossz (stride) = (hasznos bájtok száma) + (igazítás 4 byte-ra), de esetemben nincs offset
			// [B][G][R][B][G][R]...[offset]
			int stride = bmData.Stride;

			unsafe
			{
                byte* pBase = (byte*)bmData.Scan0;

				Parallel.For(0, imgHeightPix, y =>
				{
					uint* pRow = (uint*)(pBase + y * stride);

					for (int x = 0; x < imgWidthPix; x++)
					{
						pRow[0] ^= 0x00FFFFFFu; // bitmaszk alsó 3 byte (BGR) intertálása XOR művelettel, A változatlan
						pRow++;
					}
				});
			}

			srcImage.UnlockBits(bmData);

			return srcImage;
		}

		public static Bitmap ApplyGammaTransform(Bitmap srcImage, double gamma = 1)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			// Gamma look up table
			byte[] gammaLUT = new byte[256];

			for (int i = 0; i < 256; i++)
			{
				gammaLUT[i] = (byte)Math.Min(255, (int)(255.0 * Math.Pow(i / 255.0, gamma))); // g(x,y) = 255 * (f(x,y) / 255)^gamma
			}

			BitmapData bmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix), 
				ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

			int stride = bmData.Stride;

            unsafe
            {
				byte* pBase = (byte*)bmData.Scan0;
				
				Parallel.For(0, imgHeightPix, y =>
				{
					uint* pRow = (uint*)(pBase + y * stride);

					for (int x = 0; x < imgWidthPix; x++)
					{
						uint currPixelValue = pRow[0];

						pRow[0] = (currPixelValue & 0xFF000000) | ((uint)gammaLUT[(byte)(currPixelValue >> 16)] << 16) | ((uint)gammaLUT[(byte)(currPixelValue >> 8)] << 8) | gammaLUT[(byte)currPixelValue]; // >> biteltolás balra, Look Up Table alkalmazása

						pRow++;
					}
				});
			}

			srcImage.UnlockBits(bmData);

			return srcImage;
		}

		public static Bitmap ApplyLogTransform(Bitmap srcImage, int c = 1)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			// Log look up table
			byte[] logLUT = new byte[256];

			for (int i = 0; i < 256; i++)
			{
				logLUT[i] = (byte)Math.Min(255, (c * Math.Log(1 + i))); // g(x,y) = c * log(1 + f(x,y))
			}

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

			int stride = srcBmData.Stride;

            unsafe
			{
				byte* pBase = (byte*)srcBmData.Scan0;

				Parallel.For(0, imgHeightPix, y =>
				{
					uint* pRow = (uint*)(pBase + y * stride);

					for (int x = 0; x < imgWidthPix; x++)
					{
						uint currPixelValue = pRow[0];

						pRow[0] = (currPixelValue & 0xFF000000) | ((uint)logLUT[(byte)(currPixelValue >> 16)] << 16) | ((uint)logLUT[(byte)(currPixelValue >> 8)] << 8) | logLUT[(byte)(currPixelValue)]; // >> biteltolás balra, Look Up Table alkalmazása

						pRow++;
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
				ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

			int stride = srcBmData.Stride;

			unsafe
			{
				byte* pBase = (byte*)srcBmData.Scan0;

				Parallel.For(0, imgHeightPix, y =>
				{
					uint* pRow = (uint*)(pBase + y * stride);

					for (int x = 0; x < imgWidthPix; x++)
					{
						uint currPixelValue = pRow[0];

						byte gray = (byte)((114 * (byte)(currPixelValue) + 587 * (byte)(currPixelValue >> 8) + 299 * (byte)(currPixelValue >> 16)) / 1000);

						pRow[0] = (currPixelValue & 0xFF000000) | ((uint)gray << 16) | ((uint)gray << 8) | gray;

						pRow++;
					}
				});
			}

			srcImage.UnlockBits(srcBmData);

			return srcImage;
		}

		public static Bitmap ConvertToGrayscale8bpp(Bitmap srcImage)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

			Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format8bppIndexed);

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			ColorPalette pal = dstImage.Palette;
			for (int i = 0; i < 256; i++)
            {
				pal.Entries[i] = Color.FromArgb(i, i, i);
			}
			dstImage.Palette = pal;

			BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

			int sStride = srcBmData.Stride;
			int dStride = dstBmData.Stride;

			unsafe
            {
				byte* pSrcBase = (byte*)srcBmData.Scan0;
				byte* pDstBase = (byte*)dstBmData.Scan0;

				Parallel.For(0, imgHeightPix, y =>
                {
					uint* pSrcRow = (uint*)(pSrcBase + y * sStride);
					byte* pDstRow = pDstBase + y * dStride;

					for (int x = 0; x < imgWidthPix; x++)
                    {
						uint currPixelValue = pSrcRow[0];

						pDstRow[0] = (byte)((114 * (byte)(currPixelValue) + 587 * (byte)(currPixelValue >> 8) + 299 * (byte)(currPixelValue >> 16)) / 1000);

						pSrcRow++;
						// 8bpp!
                        pDstRow++;
					}
                });
            }

            srcImage.UnlockBits(srcBmData);
			dstImage.UnlockBits(dstBmData);

			return dstImage;
        }

        public static (int[] histB, int[] histG, int[] histR) ComputeHistogram(Bitmap srcImage)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            int[] histB = new int[256];
            int[] histG = new int[256];
            int[] histR = new int[256];

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int stride = srcBmData.Stride;

            unsafe
            {
                byte* pBase = (byte*)srcBmData.Scan0;

                int processorCount = Environment.ProcessorCount;
                var localB = new int[processorCount][];
                var localG = new int[processorCount][];
                var localR = new int[processorCount][];

                for (int i = 0; i < processorCount; i++)
                {
                    localB[i] = new int[256];
                    localG[i] = new int[256];
                    localR[i] = new int[256];
                }
				Parallel.For(0, processorCount, i =>
                {
					//Split the image rows into 'processorCount' segments
					int startY = i * imgHeightPix / processorCount;
                    int endY = (i + 1) * imgHeightPix / processorCount;

                    int[] lb = localB[i];
                    int[] lg = localG[i];
                    int[] lr = localR[i];

                    for (int y = startY; y < endY; y++)
                    {
                        uint* pRow = (uint*)(pBase + y * stride);

                        for (int x = 0; x < imgWidthPix; x++)
                        {
							uint currPixelValue = pRow[0];

							lb[(byte)(currPixelValue)]++;
                            lg[(byte)(currPixelValue >> 8)]++;
                            lr[(byte)(currPixelValue >> 16)]++;

							pRow++;
                        }
                    }
                });

                for (int i = 0; i < processorCount; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        histB[j] += localB[i][j];
                        histG[j] += localG[i][j];
                        histR[j] += localR[i][j];
                    }
                }
            }

            srcImage.UnlockBits(srcBmData);

            return (histB, histG, histR);
        }

        public static (int[], int[], int[]) EqualizeHistogram(int[] histB, int[] histG, int[] histR)
		{
			//Képlet: s = T(r) = (L - 1)*Σp_A(i) <--- (i 0-tól r-ig)

			int L = histR.Count();

            int sumB = histB.Sum();
            int sumG = histG.Sum();
            int sumR = histR.Sum();

            double[] pB = new double[L];
            double[] pG = new double[L];
            double[] pR = new double[L];

            for (int i = 0; i < L; i++)
            {
                pB[i] = (double)histB[i] / sumB;
                pG[i] = (double)histG[i] / sumG;
                pR[i] = (double)histR[i] / sumR;
            }

            double[] cdfB = new double[L];
            double[] cdfG = new double[L];
            double[] cdfR = new double[L];

            cdfB[0] = pB[0];
            cdfG[0] = pG[0];
            cdfR[0] = pR[0];

            for (int i = 1; i < L; i++)
            {
                cdfB[i] = cdfB[i - 1] + pB[i];
                cdfG[i] = cdfG[i - 1] + pG[i];
                cdfR[i] = cdfR[i - 1] + pR[i];
            }

            int[] mapB = new int[L];
            int[] mapG = new int[L];
            int[] mapR = new int[L];

            for (int i = 0; i < L; i++)
            {
                mapB[i] = (int)Math.Round((L - 1) * cdfB[i]);
                mapG[i] = (int)Math.Round((L - 1) * cdfG[i]);
                mapR[i] = (int)Math.Round((L - 1) * cdfR[i]);
            }

            int[] eqHistB = new int[L];
            int[] eqHistG = new int[L];
            int[] eqHistR = new int[L];

            for (int i = 0; i < L; i++)
            {
                eqHistB[mapB[i]] += histB[i];
                eqHistG[mapG[i]] += histG[i];
                eqHistR[mapR[i]] += histR[i];
            }

            return (eqHistB, eqHistG, eqHistR);
        }

        public static void ShowHistogram(int[] histB, int[] histG, int[] histR, string titleText)
        {
            var histForm = new Form
            {
                Text = titleText,
                WindowState = FormWindowState.Maximized
            };

            histForm.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.Clear(Color.White);

                int width = histForm.ClientSize.Width;
                int height = histForm.ClientSize.Height;

                int bgrMax = Math.Max(histB.Max(), Math.Max(histG.Max(), histR.Max()));
                int bgrWidth = width / histB.Length;
                int barWidth = bgrWidth / 3;
                int barSpacing = 1;

                for (int i = 0; i < 256; i++)
                {
                    int xStart = i * bgrWidth + 60;
                    int heightScale = height - 60;

					// Blue
					int bHeight = (int)((double)histB[i] / bgrMax * heightScale);
					int bY = height - bHeight - 15;
					g.FillRectangle(Brushes.Blue, xStart + 2 * (barWidth + barSpacing), bY, barWidth, bHeight);

					// Green
					int gHeight = (int)((double)histG[i] / bgrMax * heightScale);
					int gY = height - gHeight - 15;
					g.FillRectangle(Brushes.Green, xStart + barWidth + barSpacing, gY, barWidth, gHeight);

					// Red
					int rHeight = (int)((double)histR[i] / bgrMax * heightScale);
                    int rY = height - rHeight - 15;
                    g.FillRectangle(Brushes.Red, xStart, rY, barWidth, rHeight);
                }

                Font font = new Font("Arial", 10, FontStyle.Bold);
                string maxText = $"Max value: {bgrMax}";
                SizeF textSize = g.MeasureString(maxText, font);
                g.DrawString(maxText, font, Brushes.Black, (width - textSize.Width) / 2, 10);
            };

            histForm.Show();
        }

		public static Bitmap ApplyBoxFilter(Bitmap srcImage, int kernelSize = 3)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format32bppArgb);

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

			int stride = srcBmData.Stride;
			int radius = kernelSize / 2;
			int count = kernelSize * kernelSize;

			unsafe
			{
				byte* pSrcBase = (byte*)srcBmData.Scan0;
				byte* pDstBase = (byte*)dstBmData.Scan0;

				for (int y = 0; y < imgHeightPix; y++)
				{
					byte* s = pSrcBase + y * stride;
					byte* d = pDstBase + y * stride;
					Buffer.MemoryCopy(s, d, stride, stride);
				}

				Parallel.For(radius, imgHeightPix - radius, y =>
				{
					for (int x = radius; x < imgWidthPix - radius; x++)
					{
						int sumB = 0, sumG = 0, sumR = 0;

						int y0 = y - radius;   // ablak felső sora
						int x0 = x - radius;   // ablak bal széle

						for (int ky = 0; ky < kernelSize; ky++)
						{
							uint* pWindow = (uint*)(pSrcBase + (y0 + ky) * stride + x0 * 4);

							for (int kx = 0; kx < kernelSize; kx++)
							{
								uint currPixelValue = pWindow[0];

								sumB += (byte)(currPixelValue);
								sumG += (byte)(currPixelValue >> 8);
								sumR += (byte)(currPixelValue >> 16);

								pWindow++;
							}
						}

						uint* dstPixel = (uint*)(pDstBase + y * stride + x * 4);
						uint alpha = dstPixel[0] & 0xFF000000;

						dstPixel[0] = alpha | ((uint)(sumR / count) << 16) | ((uint)(sumG / count) << 8) | (uint)(sumB / count);
					}
				});
			}

			srcImage.UnlockBits(srcBmData);
			dstImage.UnlockBits(dstBmData);

			return dstImage;
		}

		private static readonly Dictionary<int, double[,]> gaussKernelCache = new();

        public static void InitGaussKernels(int maxKernelSize = 5)
        {
            for (int size = 3; size <= maxKernelSize; size+=2)
            {
                int radius = size / 2;
                gaussKernelCache[size] = CalculateGaussKernel(size, radius);
            }
        }

        private static double[,] GetGaussKernelFromCache(int kernelSize)
        {
            if (gaussKernelCache.TryGetValue(kernelSize, out var kernel))
                return kernel;

            int radius = kernelSize / 2;
            kernel = CalculateGaussKernel(kernelSize, radius);
            gaussKernelCache[kernelSize] = kernel;
            return kernel;
        }

		private static double[,] CalculateGaussKernel(int kernelSize, int radius)
		{
			// ökölszabály sigma becslésére
			double sigma = 0.3 * ((kernelSize - 1) * 0.5 - 1) + 0.8;

			double[,] kernel = new double[kernelSize, kernelSize];

			double twoSigma2 = 2 * sigma * sigma;

			double sum = 0;
			// 2D Gauss-kernel előállítása
			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					double v = Math.Exp(-(x * x + y * y) / twoSigma2);
					kernel[y + radius, x + radius] = v;
					sum += v;
				}
			}

			// 2D Gauss-kernel normálása
			for (int i = 0; i < kernelSize; i++)
			{
				for (int j = 0; j < kernelSize; j++)
				{
					kernel[i, j] /= sum;
				}
			}

			return kernel;
		}

		public static Bitmap ApplyGaussianFilter(Bitmap srcImage, int kernelSize = 3)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format32bppArgb);

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int stride = srcBmData.Stride;
            int radius = kernelSize / 2;

            double[,] kernel = GetGaussKernelFromCache(kernelSize);

            unsafe
            {
                byte* pSrcBase = (byte*)srcBmData.Scan0;
                byte* pDstBase = (byte*)dstBmData.Scan0;

                for (int y = 0; y < imgHeightPix; y++)
                {
                    byte* s = pSrcBase + y * stride;
                    byte* d = pDstBase + y * stride;
                    Buffer.MemoryCopy(s, d, stride, stride);
                }

                Parallel.For(radius, imgHeightPix - radius, y =>
                {
                    for (int x = radius; x < imgWidthPix - radius; x++)
                    {
                        double sumB = 0, sumG = 0, sumR = 0;

                        int y0 = y - radius;   // ablak felső sora
                        int x0 = x - radius;   // ablak bal széle

                        for (int ky = 0; ky < kernelSize; ky++)
                        {
                            uint* pWindow = (uint*)(pSrcBase + (y0 + ky) * stride + x0 * 4);

                            for (int kx = 0; kx < kernelSize; kx++)
                            {
                                uint currPixelValue = pWindow[0];
                                double weight = kernel[ky, kx];

                                sumB += (byte)(currPixelValue) * weight;
                                sumG += (byte)(currPixelValue >> 8) * weight;
                                sumR += (byte)(currPixelValue >> 16) * weight;

                                pWindow++;
                            }
                        }

                        uint* dstPixel = (uint*)(pDstBase + y * stride + x * 4);
                        uint alpha = dstPixel[0] & 0xFF000000;

                        dstPixel[0] = alpha | ((uint)sumR << 16) | ((uint)sumG << 8) | (uint)sumB;
                    }
                });
            }

            srcImage.UnlockBits(srcBmData);
            dstImage.UnlockBits(dstBmData);

            return dstImage;
        }

		public static bool IsGrayscale(Bitmap srcImage, int tolerance = 0)
		{
			int bpp = Image.GetPixelFormatSize(srcImage.PixelFormat) / 8;

			if (srcImage.PixelFormat == PixelFormat.Format8bppIndexed) return true;
			if (bpp != 3 && bpp != 4) return false; // bpp nem 1, nem 3, nem 4 -> speciális formátum

			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, srcImage.PixelFormat);

			int stride = srcBmData.Stride;

			unsafe
			{
				byte* pBase = (byte*)srcBmData.Scan0;

				int stepY = Math.Max(1, imgHeightPix / 64);
				int stepX = Math.Max(1, imgWidthPix / 64);

				for (int y = 0; y < imgHeightPix; y += stepY)
				{
					byte* pRow = pBase + y * stride;

					for (int x = 0; x < imgWidthPix; x += stepX)
					{
						byte b = pRow[0];
						byte g = pRow[1];
						byte r = pRow[2];

						int maxc = Math.Max(r, Math.Max(g, b));
						int minc = Math.Min(r, Math.Min(g, b));

						if (maxc - minc > tolerance)
						{
							srcImage.UnlockBits(srcBmData);
							return false;
						}

						pRow += bpp;
					}
				}
			}

			srcImage.UnlockBits(srcBmData);

			return true;
		}

		// Sobel X kernel
		private static readonly int[,] sobelXKernel = new int[,]
        {
			{ -1, 0, 1 },
			{ -2, 0, 2 },
			{ -1, 0, 1 }
        };

        // Sobel Y kernel
        private static readonly int[,] sobelYKernel = new int[,]
        {
			{ -1, -2, -1 },
			{  0,  0,  0 },
			{  1,  2,  1 }
        };

        public static Bitmap ApplySobelEdgeDetection(Bitmap srcImage)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format32bppArgb);

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int stride = srcBmData.Stride;

            int[,] sobelX = sobelXKernel;
			int[,] sobelY = sobelYKernel;

			unsafe
            {
                byte* pSrcBase = (byte*)srcBmData.Scan0;
                byte* pDstBase = (byte*)dstBmData.Scan0;

                Parallel.For(1, imgHeightPix - 1, y =>
                {
                    for (int x = 1; x < imgWidthPix - 1; x++)
                    {
						int Gy = 0, Gx = 0;

						//int y0 = y - 1;   // ablak felső sora
						//int x0 = x - 1;   // ablak bal széle

						//for (int ky = 0; ky < 3; ky++)
						//{
						//	uint* pWindow = (uint*)(pSrcBase + (y0 + ky) * stride + x0 * 4);

						//	for (int kx = 0; kx < 3; kx++)
						//{
						//		uint currPixelValue = pWindow[0];
						//		byte gray = (byte)currPixelValue;

						//		sumY += gray * sobelY[ky, kx];
						//		sumX += gray * sobelX[ky, kx];

						//		pWindow++;
						//}
						//}

						int y0 = y - 1;   // ablak felső sora
						int x0 = x - 1;   // ablak bal széle

						// Sor -1
						uint* pPreviousRow = (uint*)(pSrcBase + (y0 + 0) * stride + x0 * 4);
						byte g00 = (byte)pPreviousRow[0];
						byte g01 = (byte)pPreviousRow[1];
						byte g02 = (byte)pPreviousRow[2];

						// Sor 0
						uint* pCurrentRow = (uint*)(pSrcBase + (y0 + 1) * stride + x0 * 4);
						byte g10 = (byte)pCurrentRow[0];
						byte g11 = (byte)pCurrentRow[1];
						byte g12 = (byte)pCurrentRow[2];

						// Sor 1
						uint* pNextRow = (uint*)(pSrcBase + (y0 + 2) * stride + x0 * 4);
						byte g20 = (byte)pNextRow[0];
						byte g21 = (byte)pNextRow[1];
						byte g22 = (byte)pNextRow[2];

						// Sobel Y
						Gy += g00 * sobelY[0, 0] + g01 * sobelY[0, 1] + g02 * sobelY[0, 2];
						Gy += g10 * sobelY[1, 0] + g11 * sobelY[1, 1] + g12 * sobelY[1, 2];
						Gy += g20 * sobelY[2, 0] + g21 * sobelY[2, 1] + g22 * sobelY[2, 2];

						// Sobel X
						Gx += g00 * sobelX[0, 0] + g01 * sobelX[0, 1] + g02 * sobelX[0, 2];
						Gx += g10 * sobelX[1, 0] + g11 * sobelX[1, 1] + g12 * sobelX[1, 2];
						Gx += g20 * sobelX[2, 0] + g21 * sobelX[2, 1] + g22 * sobelX[2, 2];

						//int G = (int)Math.Sqrt(Gx * Gx + Gy * Gy);

						// G ~ |Gx| + |Gy|
						int  G = Math.Abs(Gx) + Math.Abs(Gy);
						G = Math.Min(G, 255);

						uint* dstPixel = (uint*)(pDstBase + y * stride + x * 4);
						dstPixel[0] = 0xFF000000 | ((uint)G << 16) | ((uint)G << 8) | (uint)G;
                    }
                });
            }

            srcImage.UnlockBits(srcBmData);
            dstImage.UnlockBits(dstBmData);

            return dstImage;
        }

        public static Bitmap ApplyLaplacianEdgeDetection(Bitmap srcImage)
		{
			//TODO: to write the function with 32bpp

			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			//TODO: 

			srcImage.UnlockBits(srcBmData);
			return srcImage;
		}

		public static Bitmap DetectKeypoints(Bitmap srcImage)
		{
			//TODO: to write the function with 32bpp

			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			//TODO: 

			srcImage.UnlockBits(srcBmData);
			return srcImage;
		}

		public static Bitmap ThresholdImage(Bitmap srcImage, int threshold = 128)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format32bppArgb);

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

			BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			int srcStride = srcBmData.Stride;
			int dstStride = dstBmData.Stride;

			unsafe
			{
				byte* pSrcBase = (byte*)srcBmData.Scan0;
				byte* pDstBase = (byte*)dstBmData.Scan0;

				Parallel.For(0, imgHeightPix, y =>
				{
					uint* pSrcRow = (uint*)(pSrcBase + y * srcStride);
					uint* pDstRow = (uint*)(pDstBase + y * dstStride);

					for (int x = 0; x < imgWidthPix; x++)
					{
						uint currPixelValue = pSrcRow[0];
						// B = G = R
						byte gray = (byte)currPixelValue;
						byte binary = (byte)(gray > threshold ? 255 : 0);

						uint alpha = pSrcRow[0] & 0xFF000000;
						pDstRow[0] = alpha | ((uint)binary << 16) | ((uint)binary << 8) | (uint)binary;

						pSrcRow++;
						pDstRow++;
					}
				});
			}

			srcImage.UnlockBits(srcBmData);
			dstImage.UnlockBits(dstBmData);

			return dstImage;
		}
	}
}
