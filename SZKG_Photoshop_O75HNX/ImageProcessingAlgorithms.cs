using System.Drawing.Imaging;

namespace SZKG_Photoshop_O75HNX
{
    public class ImageProcessingAlgorithms
	{
		public static Bitmap InvertImage(Bitmap srcImage)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

            // BGR!!!
			BitmapData bmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			// teljes sorhossz (stride) = (hasznos bájtok száma) + (igazítás 4 byte-ra)
			// [B][G][R][B][G][R]...[offset]
			int stride = bmData.Stride;

            unsafe
			{
                byte* pBase = (byte*)bmData.Scan0;

				Parallel.For(0, imgHeightPix, y =>
				{
					byte* p = pBase + y * stride; // új sor

					for (int x = 0; x < imgWidthPix; x++)
					{
						p[0] = (byte)(255 - p[0]);
						p[1] = (byte)(255 - p[1]);
						p[2] = (byte)(255 - p[2]);
						p += 3;
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
				
				Parallel.For(0, imgHeightPix, y =>
				{
					byte* p = pBase + y * stride;

					for (int x = 0; x < imgWidthPix; x++)
					{
						p[0] = gammaLUT[p[0]];
						p[1] = gammaLUT[p[1]];
						p[2] = gammaLUT[p[2]];
						p += 3;
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

				Parallel.For(0, imgHeightPix, y =>
				{
					byte* p = pBase + y * stride;

					for (int x = 0; x < imgWidthPix; x++)
					{
						p[0] = logLUT[p[0]];
						p[1] = logLUT[p[1]];
						p[2] = logLUT[p[2]];
						p += 3;
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

                Parallel.For(0, imgHeightPix, y =>
                {
                    byte* p = pBase + y * stride;

                    for (int x = 0; x < imgWidthPix; x++)
                    {
						p[0] = p[1] = p[2] = (byte)(0.299 * p[0] + 0.587 * p[1] + 0.114 * p[2]); //BGR
                        p += 3;
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
                    int startY = i * imgHeightPix / processorCount;
                    int endY = (i + 1) * imgHeightPix / processorCount;

                    int[] lr = localR[i];
                    int[] lg = localG[i];
                    int[] lb = localB[i];

                    for (int y = startY; y < endY; y++)
                    {
                        byte* p = pBase + y * stride;

                        for (int x = 0; x < imgWidthPix; x++)
                        {
                            lb[p[0]]++;
                            lg[p[1]]++;
                            lr[p[2]]++;
                            p += 3;
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

                int rgbMax = Math.Max(histR.Max(), Math.Max(histG.Max(), histB.Max()));
                int rgbWidth = width / histR.Length;
                int barWidth = rgbWidth / 3;
                int barSpacing = 1;

                for (int i = 0; i < 256; i++)
                {
                    int xStart = i * rgbWidth + 60;
                    int heightScale = height - 60;

                    // Red
                    int rHeight = (int)((double)histR[i] / rgbMax * heightScale);
                    int rY = height - rHeight - 15;
                    g.FillRectangle(Brushes.Red, xStart, rY, barWidth, rHeight);

                    // Green
                    int gHeight = (int)((double)histG[i] / rgbMax * heightScale);
                    int gY = height - gHeight - 15;
                    g.FillRectangle(Brushes.Green, xStart + barWidth + barSpacing, gY, barWidth, gHeight);

                    // Blue
                    int bHeight = (int)((double)histB[i] / rgbMax * heightScale);
                    int bY = height - bHeight - 15;
                    g.FillRectangle(Brushes.Blue, xStart + 2 * (barWidth + barSpacing), bY, barWidth, bHeight);
                }

                Font font = new Font("Arial", 10, FontStyle.Bold);
                string maxText = $"Max value: {rgbMax}";
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

        public static Bitmap ApplyGaussianFilter(Bitmap srcImage, int kernelSize = 3)
		{
			double sigma = 0.3 * ((kernelSize - 1) * 0.5 - 1) + 0.8;

			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			Bitmap dstImage = new Bitmap(imgWidthPix, imgHeightPix, PixelFormat.Format24bppRgb);

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			BitmapData dstBmData = dstImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

			//TODO: 

			srcImage.UnlockBits(srcBmData);
			dstImage.UnlockBits(dstBmData);

			return dstImage;
		}

		public static Bitmap ApplySobelEdgeDetection(Bitmap srcImage)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb); ;

			//TODO: 

			srcImage.UnlockBits(srcBmData);
			return srcImage;
		}

		public static Bitmap ApplyLaplacianEdgeDetection(Bitmap srcImage)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb); ;

			//TODO: 

			srcImage.UnlockBits(srcBmData);
			return srcImage;
		}

		public static Bitmap DetectKeypoints(Bitmap srcImage)
		{
			int imgWidthPix = srcImage.Width;
			int imgHeightPix = srcImage.Height;

			BitmapData srcBmData = srcImage.LockBits(new Rectangle(0, 0, imgWidthPix, imgHeightPix),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb); ;

			//TODO: 

			srcImage.UnlockBits(srcBmData);
			return srcImage;
		}
	}
}
