using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Linq.Expressions;
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

						pRow[0] = (currPixelValue & 0xFF000000u) | ((uint)gammaLUT[(byte)(currPixelValue >> 16)] << 16) | ((uint)gammaLUT[(byte)(currPixelValue >> 8)] << 8) | gammaLUT[(byte)currPixelValue]; // >> biteltolás balra, Look Up Table alkalmazása

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
				byte* pSrcBase = (byte*)srcBmData.Scan0;

				Parallel.For(0, imgHeightPix, y =>
				{
					uint* pRow = (uint*)(pSrcBase + y * stride);

					for (int x = 0; x < imgWidthPix; x++)
					{
						uint currPixelValue = pRow[0];

						pRow[0] = (currPixelValue & 0xFF000000u) | ((uint)logLUT[(byte)(currPixelValue >> 16)] << 16) | ((uint)logLUT[(byte)(currPixelValue >> 8)] << 8) | logLUT[(byte)(currPixelValue)]; // >> biteltolás balra, Look Up Table alkalmazása

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
				byte* pSrcBase = (byte*)srcBmData.Scan0;

				Parallel.For(0, imgHeightPix, y =>
				{
					uint* pRow = (uint*)(pSrcBase + y * stride);

					for (int x = 0; x < imgWidthPix; x++)
					{
						uint currPixelValue = pRow[0];

						byte gray = (byte)((114 * (byte)(currPixelValue) + 587 * (byte)(currPixelValue >> 8) + 299 * (byte)(currPixelValue >> 16)) / 1000);

						pRow[0] = (currPixelValue & 0xFF000000u) | ((uint)gray << 16) | ((uint)gray << 8) | gray;

						pRow++;
					}
				});
			}

			srcImage.UnlockBits(srcBmData);

			return srcImage;
		}

		//public static Bitmap ConvertToGrayscale8bpp(Bitmap srcImage)
  //      {
  //          int imgWidthPix = srcImage.Width;
  //          int imgHeightPix = srcImage.Height;

		//	Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format8bppIndexed);

		//	BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
  //              ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

		//	ColorPalette pal = dstImage.Palette;
		//	for (int i = 0; i < 256; i++)
  //          {
		//		pal.Entries[i] = Color.FromArgb(i, i, i);
		//	}
		//	dstImage.Palette = pal;

		//	BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
		//		ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

		//	int sStride = srcBmData.Stride;
		//	int dStride = dstBmData.Stride;

		//	unsafe
  //          {
		//		byte* pSrcBase = (byte*)srcBmData.Scan0;
		//		byte* pDstBase = (byte*)dstBmData.Scan0;

		//		Parallel.For(0, imgHeightPix, y =>
  //              {
		//			uint* pSrcRow = (uint*)(pSrcBase + y * sStride);
		//			byte* pDstRow = pDstBase + y * dStride;

		//			for (int x = 0; x < imgWidthPix; x++)
  //                  {
		//				uint currPixelValue = pSrcRow[0];

		//				pDstRow[0] = (byte)((114 * (byte)(currPixelValue) + 587 * (byte)(currPixelValue >> 8) + 299 * (byte)(currPixelValue >> 16)) / 1000);

		//				pSrcRow++;
		//				// 8bpp!
  //                      pDstRow++;
		//			}
  //              });
  //          }

  //          srcImage.UnlockBits(srcBmData);
		//	dstImage.UnlockBits(dstBmData);

		//	return dstImage;
  //      }

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
                byte* pSrcBase = (byte*)srcBmData.Scan0;

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
                        uint* pRow = (uint*)(pSrcBase + y * stride);

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

			int L = histB.Length;

			int sumPixels = 0;
			for (int i = 0; i < L; i++)
			{
				sumPixels += histB[i];
			}

			int[] eqHistB = new int[L];
			int[] eqHistG = new int[L];
			int[] eqHistR = new int[L];

			int cumB = 0, cumG = 0, cumR = 0;

			for (int i = 0; i < L; i++)
			{
				cumB += histB[i];
				cumG += histG[i];
				cumR += histR[i];

				double cdfB = (double)cumB / sumPixels;
				double cdfG = (double)cumG / sumPixels;
				double cdfR = (double)cumR / sumPixels;

				int mapB = (int)Math.Round((L - 1) * cdfB);
				int mapG = (int)Math.Round((L - 1) * cdfG);
				int mapR = (int)Math.Round((L - 1) * cdfR);

				eqHistB[mapB] += histB[i];
				eqHistG[mapG] += histG[i];
				eqHistR[mapR] += histR[i];
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

				// Margók
				int marginLeft = 30;
				int marginRight = 30;
				int drawableWidth = width - marginLeft - marginRight;

				float bgrWidth = (float)drawableWidth / 256f;
				float barWidth = bgrWidth / 3f;
				float barSpacing = 1f;

				int bgrMax = Math.Max(histB.Max(), Math.Max(histG.Max(), histR.Max()));

				int heightScale = height - 60;

				for (int i = 0; i < 256; i++)
				{
					float xStart = marginLeft + i * bgrWidth;

					// Blue
					int bHeight = (int)((float)histB[i] / bgrMax * heightScale);
					int bY = height - bHeight - 15;
					g.FillRectangle(Brushes.Blue, xStart + 2 * (barWidth + barSpacing), bY, barWidth, bHeight);

					// Green
					int gHeight = (int)((float)histG[i] / bgrMax * heightScale);
					int gY = height - gHeight - 15;
					g.FillRectangle(Brushes.Green, xStart + barWidth + barSpacing, gY, barWidth, gHeight);

					// Red
					int rHeight = (int)((float)histR[i] / bgrMax * heightScale);
					int rY = height - rHeight - 15;
					g.FillRectangle(Brushes.Red, xStart, rY, barWidth, rHeight);
				}

				// Max érték kiírása viszonyítási alapként
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
						uint alpha = dstPixel[0] & 0xFF000000u;

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
					// Képlet: g(x,y) = e^-((x^2+y^2)/(2sigma^2))
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
                        uint alpha = dstPixel[0] & 0xFF000000u;

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

		//private static readonly int[,] sobelXKernel = new int[,]
        //{
		//	{ -1, 0,  1 },
		//	{ -2, 0,  2 },
		//	{ -1, 0,  1 }
        //};

		//private static readonly int[,] sobelYKernel = new int[,]
		//{
		//	{ -1, -2, -1 },
		//	{  0,  0,  0 },
		//	{  1,  2,  1 }
		//};

        public static (Bitmap, float[,], float[,]) ApplySobelEdgeDetection(Bitmap srcImage)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            float[,] Ix = new float[imgHeightPix - 2, imgWidthPix - 2];
            float[,] Iy = new float[imgHeightPix - 2, imgWidthPix - 2];

            Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format32bppArgb);

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int stride = srcBmData.Stride;

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

						int x0 = x - 1;

						// Sor y-1
						uint* pPreviousRow = (uint*)(pSrcBase + (y - 1) * stride + x0 * 4);
						byte g00 = (byte)pPreviousRow[0];
						byte g01 = (byte)pPreviousRow[1];
						byte g02 = (byte)pPreviousRow[2];

						// Sor y
						uint* pCurrentRow = (uint*)(pSrcBase + y * stride + x0 * 4);
						byte g10 = (byte)pCurrentRow[0];
						//byte g11 = (byte)pCurrentRow[1];
						byte g12 = (byte)pCurrentRow[2];

						// Sor y+1
						uint* pNextRow = (uint*)(pSrcBase + (y + 1) * stride + x0 * 4);
						byte g20 = (byte)pNextRow[0];
						byte g21 = (byte)pNextRow[1];
						byte g22 = (byte)pNextRow[2];

						// sobelY = [-1 -2 -1; 0 0 0; 1 2 1]
						//Gy += - g00 - 2 * g01 - g02 + g20 + 2 * g21 + g22;
						Gy += -g00 - (g01 << 1) - g02 + g20 + (g21 << 1) + g22;

						// sobelX = [-1 0 1; -2 0 2; -1 0 1]
						//Gx += - g00 + g02 - 2 * g10 + 2 * g12 - g20 + g22;
						Gx += -g00 + g02 - (g10 << 1) + (g12 << 1) - g20 + g22;

						Ix[y-1, x-1] = Gx;
                        Iy[y-1, x-1] = Gy;

                        // G = gyök(Gx^2 + Gy^2)
                        // int G = (int)Math.Sqrt(Gx * Gx + Gy * Gy);

                        // G ~ |Gx| + |Gy|
                        int amp = Math.Abs(Gx) + Math.Abs(Gy);
						amp = Math.Min(amp, 255);

						uint* dstPixel = (uint*)(pDstBase + y * stride + x * 4);
						dstPixel[0] = 0xFF000000u | ((uint)amp << 16) | ((uint)amp << 8) | (uint)amp;
                    }
                });
            }

            srcImage.UnlockBits(srcBmData);
            dstImage.UnlockBits(dstBmData);

            return (dstImage, Ix, Iy);
        }

		//private static readonly int[,] neighbors4LaplaceKernel = new int[,]
		//{
		//	{ 0,  1,  0 },
		//	{ 1, -4,  1 },
		//	{ 0,  1,  0 }
		//};

		//private static readonly int[,] neighbors8LaplaceKernel = new int[,]
		//{
		//	{ 1,  1,  1 },
		//	{ 1, -8,  1 },
		//	{ 1,  1,  1 }
		//};

		public static Bitmap ApplyLaplacianEdgeDetection(Bitmap srcImage, int neighbors = 4)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

            Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format32bppArgb);

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			int stride = srcBmData.Stride;

			unsafe
			{
				byte* pSrcBase = (byte*)srcBmData.Scan0;
				byte* pDstBase = (byte*)dstBmData.Scan0;

				Parallel.For(1, imgHeightPix - 1, y =>
				{
					for (int x = 1; x < imgWidthPix - 1; x++)
					{
						int x0 = x - 1;

						// Sor y-1
						uint* pPreviousRow = (uint*)(pSrcBase + (y - 1) * stride + x0 * 4);
						byte g00 = (byte)pPreviousRow[0];
						byte g01 = (byte)pPreviousRow[1];
						byte g02 = (byte)pPreviousRow[2];

						// Sor y
						uint* pCurrentRow = (uint*)(pSrcBase + y * stride + x0 * 4);
						byte g10 = (byte)pCurrentRow[0];
						byte center = (byte)pCurrentRow[1];
						byte g12 = (byte)pCurrentRow[2];

						// Sor y+1
						uint* pNextRow = (uint*)(pSrcBase + (y + 1) * stride + x0 * 4);
						byte g20 = (byte)pNextRow[0];
						byte g21 = (byte)pNextRow[1];
						byte g22 = (byte)pNextRow[2];

						int L;
						if (neighbors == 8)
						{
							// 8-neighbour: [1 1 1; 1 -8 1; 1 1 1]
							L = g00 + g01 + g02 + g10 - (center << 3) + g12 + g20 + g21 + g22; // -8 * g11
						}
						else
						{
							// 4-neighbour: [0 1 0; 1 -4 1; 0 1 0]
							L = g01 + g10 - (center << 2) + g12 + g21; // -4 * g11
						}

						int amp = Math.Abs(L);

						uint* dstPixel = (uint*)(pDstBase + y * stride + x * 4);
						dstPixel[0] = 0xFF000000u | ((uint)amp << 16) | ((uint)amp << 8) | (uint)amp;
					}
				});
			}

			srcImage.UnlockBits(srcBmData);
			dstImage.UnlockBits(dstBmData);

			return dstImage;
		}

        public static Bitmap DrawPoints(Bitmap srcImage, IEnumerable<Point> points, int pointSize = 3)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int stride = srcBmData.Stride;
            int radius = pointSize / 2;

            unsafe
            {
                byte* pSrcBase = (byte*)srcBmData.Scan0;

                foreach (var p in points)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int y = p.Y + dy;
                        if (y < 0 || y >= imgHeightPix) continue;

                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int x = p.X + dx;
                            if (x < 0 || x >= imgWidthPix) continue;

                            uint* pixel = (uint*)(pSrcBase + y * stride + x * 4);

							pixel[0] = 0xFFFF0000; // A, R = 255 és G, B = 0
                        }
                    }
                }
            }

            srcImage.UnlockBits(srcBmData);
            return srcImage;
        }

        public static ConcurrentBag<Point> DetectKeypoints(float[,] Ix, float[,] Iy, int threshold = 1000)
		{
            int height = Ix.GetLength(0);
            int width = Ix.GetLength(1);

            ConcurrentBag<Point> keyPoints = new ConcurrentBag<Point>();

            unsafe
            {
				Parallel.For(1, height - 1, y =>
				{
					for (int x = 1; x < width - 1; x++)
					{
                        float Ix00 = Ix[y - 1, x - 1], Ix01 = Ix[y - 1, x], Ix02 = Ix[y - 1, x + 1];
                        float Ix10 = Ix[y, x - 1], Ix11 = Ix[y, x], Ix12 = Ix[y, x + 1];
                        float Ix20 = Ix[y + 1, x - 1], Ix21 = Ix[y + 1, x], Ix22 = Ix[y + 1, x + 1];

                        float Iy00 = Iy[y - 1, x - 1], Iy01 = Iy[y - 1, x], Iy02 = Iy[y - 1, x + 1];
                        float Iy10 = Iy[y, x - 1], Iy11 = Iy[y, x], Iy12 = Iy[y, x + 1];
                        float Iy20 = Iy[y + 1, x - 1], Iy21 = Iy[y + 1, x], Iy22 = Iy[y + 1, x + 1];

                        float Ix2 = (Ix00 * Ix00 + Ix01 * Ix01 + Ix02 * Ix02 +
                                    Ix10 * Ix10 + Ix11 * Ix11 + Ix12 * Ix12 +
                                    Ix20 * Ix20 + Ix21 * Ix21 + Ix22 * Ix22) / 9f;

                        float Iy2 = (Iy00 * Iy00 + Iy01 * Iy01 + Iy02 * Iy02 +
                                    Iy10 * Iy10 + Iy11 * Iy11 + Iy12 * Iy12 +
                                    Iy20 * Iy20 + Iy21 * Iy21 + Iy22 * Iy22) / 9f;

                        float Ixy = (Ix00 * Iy00 + Ix01 * Iy01 + Ix02 * Iy02 +
                                    Ix10 * Iy10 + Ix11 * Iy11 + Ix12 * Iy12 +
                                    Ix20 * Iy20 + Ix21 * Iy21 + Ix22 * Iy22) / 9f;

                        float det = Ix2 * Iy2 - Ixy * Ixy;
                        float trace = Ix2 + Iy2;

                        float f = det / trace;

						if (f > threshold)
						{
							keyPoints.Add(new Point(x, y));
                        }
					}
				});
            }

            return (keyPoints);
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

						uint alpha = pSrcRow[0] & 0xFF000000u;
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
