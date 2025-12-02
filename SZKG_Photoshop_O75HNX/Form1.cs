using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace SZKG_Photoshop_O75HNX
{
	public partial class Form1 : Form
	{
        private int[]? histR;
        private int[]? histG;
        private int[]? histB;

		float[,]? Ix;
		float[,]? Iy;

        Bitmap? lastImage;
		ConcurrentBag<Point> keyPoints;

        public Form1()
		{
			InitializeComponent();
            this.Text = "Mini Photoshop - BL";

            // resize GroupBoxes
            flowLayoutPanel1.Resize += (s, e) =>
			{
				AdjustGroupBoxAndButton(tablePanel1, button1);
				AdjustGroupBoxAndButton(tablePanel2, button2);
                AdjustGroupBoxAndButton(tablePanel3, button3);
				AdjustGroupBoxAndButton(tablePanel4, button4);
				AdjustGroupBoxAndButton(tablePanel5, button5);
				AdjustGroupBoxAndButton(tablePanel6, button6);
				AdjustGroupBoxAndButton(tablePanel7, button7);
				AdjustGroupBoxAndButton(tablePanel8, button8);
				AdjustGroupBoxAndButton(tablePanel9, button9);
				AdjustGroupBoxAndButton(tablePanel10, button10);
				AdjustGroupBoxAndButton(tablePanel11, button11);
				AdjustGroupBoxAndButton(tablePanel12, button12);
                AdjustGroupBoxAndButton(tablePanel13, button13);
                AdjustGroupBoxAndButton(tablePanel14, button14);
				AdjustGroupBoxAndButton(tablePanel15, button15);
			};

			string basepath = AppContext.BaseDirectory;
			for (int i = 0; i < 4; i++)
			{
				basepath = Directory.GetParent(basepath)!.FullName;
			}

			string imagePath = Path.Combine(AppContext.BaseDirectory, "images", "defaultimg.jpg");

			if (File.Exists(imagePath))
			{
				pictureBox1.Image = Image.FromFile(imagePath);
                lastImage = new Bitmap(pictureBox1.Image);
                imgSizeLabel.Text = $"Image size:\n ({pictureBox1.Image.Width}, {pictureBox1.Image.Height})";
            }

			ImageProcessingAlgorithms.InitGaussKernels(k2TrackBar.Maximum * 2 + 3);
        }

		private void AdjustGroupBoxAndButton(TableLayoutPanel tablePanel, Button button)
		{
			tablePanel.Height = flowLayoutPanel1.Height;
			tablePanel.Width = flowLayoutPanel1.Width / 15;
		}

		private void RunImageProcessingWithTimer(Func<string> action, string methodName)
		{
			Stopwatch sw = Stopwatch.StartNew();
            string result = action();
            sw.Stop();
			MessageBox.Show($"{methodName} method execution time: {sw.ElapsedMilliseconds} ms\n\n{sw.ElapsedTicks} ticks (1ms = {Stopwatch.Frequency / 1000} ticks){result}",
				"Timing", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

        private void button0_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image|*.png;*.jpg;*.bmp;*.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                    lastImage = new Bitmap(pictureBox1.Image);
                    imgSizeLabel.Text = $"Image size: ({pictureBox1.Image.Width}, {pictureBox1.Image.Height})";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
			if (lastImage != null)
			{
				pictureBox1.Image = lastImage;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("No image on the right!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif";
                saveFileDialog.Title = "Save Image";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat format = ImageFormat.Png;
                    string ext = Path.GetExtension(saveFileDialog.FileName).ToLower();

                    switch (ext)
                    {
                        case ".jpg":
                        case ".jpeg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                        case ".gif":
                            format = ImageFormat.Gif;
                            break;
                    }

                    pictureBox2.Image.Save(saveFileDialog.FileName, format);
                    MessageBox.Show("Image saved successfully!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				// TANULSÁG: new Bitmap(Image) mindig 32 bpp-s képet hoz létre
				// viszont ha konvertálunk (Bitmap)-al, akkor az eredeti bpp megmarad, de ilyenkor nem az értékét, hanem a referenciáját adja át
				// ha clone-ozunk akkor viszont NAGYON lassul a képfeldolgozás 30-40 ms -> 400-500 ms
				//Bitmap bmImage = (Bitmap)pictureBox1.Image.Clone();
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.InvertImage(bmImage);
                    return "";
                }, "InvertImage");
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyGammaTransform(bmImage, gammaValue);
                    return "";
                }, "ApplyGammaCorrection");
			}
		}

		private void button5_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyLogTransform(bmImage, cValue);
                    return "";
                }, "ApplyLogTransform");
			}
		}

		private void button6_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ConvertToGrayscale(bmImage);
                    return "";
                }, "ConvertToGrayscale");
			}
		}

		private void button7_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				RunImageProcessingWithTimer(() =>
				{
                    (histB, histG, histR) = ImageProcessingAlgorithms.ComputeHistogram(bmImage);
                    return "";
                }, "ComputeHistogram");
                RunImageProcessingWithTimer(() =>
                {
                    ImageProcessingAlgorithms.ShowHistogram(histB!, histG!, histR!, "Histrogram");
                    return "";
                }, "ShowHistogram");
            }
		}

		private void button8_Click(object sender, EventArgs e)
		{
            if (histB != null && histB.All(b => b != 0)/*&& histG != null && histG.All(g => g != 0) && histR != null && histR.All(r => r != 0)*/)
            {
                RunImageProcessingWithTimer(() =>
                {
                    (histB, histG, histR) = ImageProcessingAlgorithms.EqualizeHistogram(histB, histG!, histR!);
                    return "";
                }, "ShowEqualizedHistogram");
				RunImageProcessingWithTimer(() =>
				{
					ImageProcessingAlgorithms.ShowHistogram(histB, histG!, histR!, "Histrogram");
                    return "";
                }, "ShowHistogram");
			}
            else
            {
                MessageBox.Show("The histogram arrays are empty. Cannot perform histogram equalization.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

		private void button9_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyBoxFilter(bmImage, k1Value);
                    return "";
                }, "ApplyBoxFilter");
			}
		}

		private void button10_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyGaussianFilter(bmImage, k2Value);
                    return "";
                }, "ApplyGaussianFilter");
			}
		}

        private void button11_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap bmImage = new Bitmap(pictureBox1.Image);

                if (ImageProcessingAlgorithms.IsGrayscale(bmImage))
                {
                    RunImageProcessingWithTimer(() =>
                    {
                        pictureBox2.Image = ImageProcessingAlgorithms.ApplyLaplacianEdgeDetection(bmImage, neighborsValue);
                        return "";
                    }, "ApplyLaplacianEdgeDetection");
                }
                else
                {
                    MessageBox.Show("This function can only run on grayscale images.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				if (ImageProcessingAlgorithms.IsGrayscale(bmImage))
				{
					RunImageProcessingWithTimer(() =>
					{
						(pictureBox2.Image, Ix, Iy) = ImageProcessingAlgorithms.ApplySobelEdgeDetection(bmImage);
                        return "";
                    }, "ApplySobelEdgeDetection");

				}
				else
				{
					MessageBox.Show("This function can only run on grayscale images.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}
		

		private void button13_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				if (ImageProcessingAlgorithms.IsGrayscale(bmImage) && Ix != null && Iy != null)
				{
					RunImageProcessingWithTimer(() =>
					{
						keyPoints = ImageProcessingAlgorithms.DetectKeypoints(Ix, Iy, thresholdValue);
                        pictureBox2.Image = ImageProcessingAlgorithms.DrawPoints(bmImage, keyPoints, pointSizeValue);
                        return "\n\nNumber of detected keypoints: " + keyPoints.Count();
                    }, "DetectKeypoints");
                }
				else
				{
					MessageBox.Show("This function can only run on grayscale images.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		private void button14_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				Bitmap bmImage = new Bitmap(pictureBox1.Image);

				if (ImageProcessingAlgorithms.IsGrayscale(bmImage))
				{
					RunImageProcessingWithTimer(() =>
					{
						pictureBox2.Image = ImageProcessingAlgorithms.ThresholdImage(bmImage, threshold2Value);
						return "";
					}, "ThresholdImage");
				}
				else
				{
					MessageBox.Show("This function can only run on grayscale images.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		private void button15_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                pictureBox1.Image = pictureBox2.Image;
                imgSizeLabel.Text = $"Image size: ({pictureBox2.Image.Width}, {pictureBox2.Image.Height})";
                pictureBox2.Image = null;
            }
        }
    }
}
