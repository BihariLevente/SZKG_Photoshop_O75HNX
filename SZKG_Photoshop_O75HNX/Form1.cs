using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SZKG_Photoshop_O75HNX
{
	public partial class Form1 : Form
	{
        int[] histR = new int[256];
        int[] histG = new int[256];
        int[] histB = new int[256];

        public Form1()
		{
			InitializeComponent();

			// resize GroupBoxes
			flowLayoutPanel1.Resize += (s, e) =>
			{
				AdjustGroupBoxAndButton(groupBox1, button1);
				AdjustGroupBoxAndButton(groupBox2, button2);
                AdjustGroupBoxAndButton(groupBox3, button3);
				AdjustGroupBoxAndButton(groupBox4, button4);
				AdjustGroupBoxAndButton(groupBox5, button5);
				AdjustGroupBoxAndButton(groupBox6, button6);
				AdjustGroupBoxAndButton(groupBox7, button7);
				AdjustGroupBoxAndButton(groupBox8, button8);
				AdjustGroupBoxAndButton(groupBox9, button9);
				AdjustGroupBoxAndButton(groupBox10, button10);
				AdjustGroupBoxAndButton(groupBox11, button11);
				AdjustGroupBoxAndButton(groupBox12, button12);
                AdjustGroupBoxAndButton(groupBox13, button13);
                AdjustGroupBoxAndButton(groupBox14, button14);
            };

			string basepath = AppContext.BaseDirectory;
			for (int i = 0; i < 4; i++)
			{
				basepath = Directory.GetParent(basepath)!.FullName;
			} 

			string imagePath = basepath + "\\images\\defaultimg.jpg";

			if (File.Exists(imagePath))
			{
				pictureBox1.Image = Image.FromFile(imagePath);
                imgSizeLabel.Text = $"Image size: ({pictureBox1.Image.Width}, {pictureBox1.Image.Height})";
            }
		}

		private void AdjustGroupBoxAndButton(GroupBox groupBox, Button button)
		{
            groupBox.Height = flowLayoutPanel1.Height - 3;
            groupBox.Width = (flowLayoutPanel1.Width - 75) / 14;
            //button.Left = (groupBox.ClientSize.Width - button.Width) / 2;
            //button.Top = (int)((groupBox.ClientSize.Height - button.Height) * 0.95);
        }

		private void RunImageProcessingWithTimer(Action action, string methodName)
		{
			Stopwatch sw = Stopwatch.StartNew();
			action();
			sw.Stop();
			MessageBox.Show($"{methodName} method execution time: {sw.ElapsedMilliseconds} ms\n\n{sw.ElapsedTicks} ticks (1ms = {Stopwatch.Frequency / 1000} ticks)",
				"Timing", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image|*.png;*.jpg;*.bmp;*.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Image img = Image.FromFile(openFileDialog.FileName);
                    pictureBox1.Image = img;
                    imgSizeLabel.Text = $"Image size: ({pictureBox1.Image.Width}, {pictureBox1.Image.Height})";
                }
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
                    string ext = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();

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
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.InvertImage(new Bitmap(pictureBox1.Image));
				}, "InvertImage");
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyGammaCorrection(new Bitmap(pictureBox1.Image), gammaValue);
				}, "ApplyGammaCorrection");
			}
		}

		private void button5_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyLogTransform(new Bitmap(pictureBox1.Image), cValue);
				}, "ApplyLogTransform");
			}
		}

		private void button6_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ConvertToGrayscale(new Bitmap(pictureBox1.Image));
				}, "ConvertToGrayscale");
			}
		}

		private void button7_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
                RunImageProcessingWithTimer(() =>
				{
                    (histR, histG, histB) = ImageProcessingAlgorithms.ComputeHistogram(new Bitmap(pictureBox1.Image));
                    pictureBox2.Image = ImageProcessingAlgorithms.ShowHistogram(histR, histG, histB, 150);
                }, "ComputeHistogram");
            }
		}

		private void button8_Click(object sender, EventArgs e)
		{
            if (!(histR.All(r => r == 0) && histG.All(g => g == 0) && histB.All(b => b == 0)))
            {
                RunImageProcessingWithTimer(() =>
                {
                    (histR, histG, histB) = ImageProcessingAlgorithms.EqualizeHistogram(histR, histG, histB);
                    pictureBox2.Image = ImageProcessingAlgorithms.ShowHistogram(histR, histG, histB, 150);
                }, "EqualizeHistogram");
            }
            else
            {
                MessageBox.Show("The image histogram is empty. Cannot perform histogram equalization.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

		private void button9_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyBoxFilter(new Bitmap(pictureBox1.Image), k1Value);
				}, "ApplyBoxFilter");
			}
		}

		private void button10_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyGaussianFilter(new Bitmap(pictureBox1.Image));
				}, "ApplyGaussianFilter");
			}
		}

		private void button11_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplySobelEdgeDetection(new Bitmap(pictureBox1.Image));
				}, "ApplySobelEdgeDetection");
			}
		}

		private void button12_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyLaplacianEdgeDetection(new Bitmap(pictureBox1.Image));
				}, "ApplyLaplacianEdgeDetection");
			}
		}

		private void button13_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.DetectKeypoints(new Bitmap(pictureBox1.Image));
				}, "DetectKeypoints");
			}
		}

        private void button14_Click(object sender, EventArgs e)
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
