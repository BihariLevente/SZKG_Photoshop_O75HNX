using System.Drawing.Imaging;
using System.Runtime.Intrinsics.Arm;
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
			// [B][G][R][A][B][G][R][A]...[offset]
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
						byte b = (byte)pRow[0];
						byte g = (byte)(pRow[0] >> 8); // >> biteltolás jobbra
						byte r = (byte)(pRow[0] >> 16);
						byte a = (byte)(pRow[0] >> 24);

						pRow[0] = ((uint)a << 24) | ((uint)gammaLUT[r] << 16) | ((uint)gammaLUT[g] << 8) | gammaLUT[b]; // >> biteltolás balra, Look Up Table alkalmazása

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
						byte b = (byte)(pRow[0]);
						byte g = (byte)(pRow[0] >> 8); // >> biteltolás jobbra
						byte r = (byte)(pRow[0] >> 16);
						byte a = (byte)(pRow[0] >> 24);

						pRow[0] = ((uint)a << 24) | ((uint)logLUT[r] << 16) | ((uint)logLUT[g] << 8) | logLUT[b]; // >> biteltolás balra, Look Up Table alkalmazása

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
						int b = (byte)(pRow[0]);
						int g = (byte)(pRow[0] >> 8);
						int r = (byte)(pRow[0] >> 16);
						int a = (byte)(pRow[0] >> 24);

						byte gray = (byte)((114 * b + 587 * g + 299 * r) / 1000);

						pRow[0] = ((uint)a << 24) | ((uint)gray << 16) | ((uint)gray << 8) | gray;

						pRow++;
					}
                });
            }

            srcImage.UnlockBits(srcBmData);

            return srcImage;
        }

        public static (int[] histR, int[] histG, int[] histB) ComputeHistogram(Bitmap srcImage)
        {
            int imgWidthPix = srcImage.Width;
            int imgHeightPix = srcImage.Height;

            int[] histR = new int[256];
            int[] histG = new int[256];
            int[] histB = new int[256];

            BitmapData bmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;

            unsafe
            {
                byte* pBase = (byte*)bmData.Scan0;

                int processorCount = Environment.ProcessorCount;
                var localR = new int[processorCount][];
                var localG = new int[processorCount][];
                var localB = new int[processorCount][];

                for (int i = 0; i < processorCount; i++)
                {
                    localR[i] = new int[256];
                    localG[i] = new int[256];
                    localB[i] = new int[256];
                }
				Parallel.For(0, processorCount, i =>
                {
					//Split the image rows into 'processorCount' segments
					int startY = i * imgHeightPix / processorCount;
                    int endY = (i + 1) * imgHeightPix / processorCount;

                    int[] lr = localR[i];
                    int[] lg = localG[i];
                    int[] lb = localB[i];

                    for (int y = startY; y < endY; y++)
                    {
                        byte* pRow = pBase + y * stride;

                        for (int x = 0; x < imgWidthPix; x++)
                        {
                            lb[pRow[0]]++;
                            lg[pRow[1]]++;
                            lr[pRow[2]]++;
                            pRow += 3;
                        }
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

            srcImage.UnlockBits(bmData);

            return (histR, histG, histB);
        }

        public static (int[], int[], int[]) EqualizeHistogram(int[] histR, int[] histG, int[] histB)
		{
			//Képlet: s = T(r) = (L - 1)*Σp_A(i) <--- (i 0-tól r-ig)

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

        public static void ShowHistogram(int[] histR, int[] histG, int[] histB, string titleText)
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

                int bgrMax = Math.Max(histR.Max(), Math.Max(histG.Max(), histB.Max()));
                int bgrWidth = width / histR.Length;
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

            Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format24bppRgb);

            BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

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

						int x0 = x - radius;   // ablak bal széle
						int y0 = y - radius;   // ablak felső sora

						for (int ky = 0; ky < kernelSize; ky++)
						{
							byte* pWindow = pSrcBase + (y0 + ky) * stride + x0 * 3;

							for (int kx = 0; kx < kernelSize; kx++)
							{
								sumB += pWindow[0];
								sumG += pWindow[1];
								sumR += pWindow[2];
								pWindow += 3;
							}
						}

						byte* dstPix = pDstBase + y * stride + x * 3;
						dstPix[0] = (byte)(sumB / count);
						dstPix[1] = (byte)(sumG / count);
						dstPix[2] = (byte)(sumR / count);
					}
				});
            }

            srcImage.UnlockBits(srcBmData);
            dstImage.UnlockBits(dstBmData);

            return dstImage;
        }

        public static Bitmap ApplyGaussianFilter(Bitmap srcImage, int kernelSize = 3)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format24bppRgb);

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

			int stride = srcBmData.Stride;
			int radius = kernelSize / 2;

			double[,] kernel = CalculateGaussKernel(kernelSize, radius);

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

						int x0 = x - radius;   // ablak bal széle
						int y0 = y - radius;   // ablak felső sora

						for (int ky = 0; ky < kernelSize; ky++)
						{
							byte* pWindow = pSrcBase + (y0 + ky) * stride + x0 * 3;

							for (int kx = 0; kx < kernelSize; kx++)
							{
								double weight = kernel[ky, kx];

								sumB += pWindow[0] * weight;
								sumG += pWindow[1] * weight;
								sumR += pWindow[2] * weight;

								pWindow += 3;
							}
						}

						byte* dstPix = pDstBase + y * stride + x * 3;

						dstPix[0] = (byte)sumB;
						dstPix[1] = (byte)sumG;
						dstPix[2] = (byte)sumR;
					}
				});
			}

			srcImage.UnlockBits(srcBmData);
			dstImage.UnlockBits(dstBmData);

			return dstImage;
		}

		private static double[,] CalculateGaussKernel(int kernelSize, int radius)
		{
			// ökölszabály sigma számítására
			double sigma = 0.3 * ((kernelSize - 1) * 0.5 - 1) + 0.8;
			double[,] kernel = new double[kernelSize, kernelSize];
			double twoSigma2 = 2 * sigma * sigma;
			double sum = 0.0;

			// 2D Gauss-kernel előállítása
			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					double exponent = -(x * x + y * y) / twoSigma2;
					double value = Math.Exp(exponent) / (Math.PI * twoSigma2);
					kernel[y + radius, x + radius] = value;
					sum += value;
				}
			}

			// 2D Gauss-kernel (összeg 1)
			for (int i = 0; i < kernelSize; i++)
				for (int j = 0; j < kernelSize; j++)
					kernel[i, j] /= sum;

			return kernel;
		}

		public static bool IsGrayscale(Bitmap srcImage, int tolerance = 0)
		{
			int bpp = Image.GetPixelFormatSize(srcImage.PixelFormat) / 8;
			if (srcImage.PixelFormat == PixelFormat.Format8bppIndexed) return true;
			if (bpp != 3 && bpp != 4) return false;

			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, srcImage.PixelFormat);

			int stride = srcBmData.Stride;

			try
			{
				unsafe
				{
					byte* pBase = (byte*)srcBmData.Scan0;

					int stepY = Math.Max(1, srcImage.Height / 64);
					int stepX = Math.Max(1, srcImage.Width / 64);

					for (int y = 0; y < srcImage.Height; y += stepY)
					{
						byte* pRow = pBase + y * stride;

						for (int x = 0; x < srcImage.Width; x += stepX)
						{
							byte b = pRow[x * bpp + 0];
							byte g = pRow[x * bpp + 1];
							byte r = pRow[x * bpp + 2];
							int maxc = Math.Max(r, Math.Max(g, b));
							int minc = Math.Min(r, Math.Min(g, b));
							if (maxc - minc > tolerance) return false;
						}
					}
				}
				return true;
			}
			finally { srcImage.UnlockBits(srcBmData); }
		}

		public static Bitmap ApplySobelEdgeDetection(Bitmap srcImage)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			//TODO: 

			srcImage.UnlockBits(srcBmData);
			return srcImage;
		}

		public static Bitmap ApplyLaplacianEdgeDetection(Bitmap srcImage)
		{
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
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			//TODO: 

			srcImage.UnlockBits(srcBmData);
			return srcImage;
		}
	}
}
