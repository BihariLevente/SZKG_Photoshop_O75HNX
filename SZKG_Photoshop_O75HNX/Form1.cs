using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SZKG_Photoshop_O75HNX
{
	public partial class Form1 : Form
	{
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
			};

			string imagePath = Path.Combine(Application.StartupPath, "defaultimg.jpg");

			if (File.Exists(imagePath))
			{
				pictureBox1.Image = Image.FromFile(imagePath);
			}
		}

		private void AdjustGroupBoxAndButton(GroupBox groupBox, Button button)
		{
			groupBox.Height = flowLayoutPanel1.Height - 3;
			groupBox.Width = (flowLayoutPanel1.Width - 65) / 12;
			button.Left = (groupBox.ClientSize.Width - button.Width) / 2;
			button.Top = (int)((groupBox.ClientSize.Height - button.Height) * 0.95);
		}

		private void RunImageProcessingWithTimer(Action action, string methodName)
		{
			Stopwatch sw = Stopwatch.StartNew();
			action();
			sw.Stop();
			MessageBox.Show($"{methodName} execution time: {sw.ElapsedMilliseconds} ms",
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
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.InvertImage(new Bitmap(pictureBox1.Image));
				}, "InvertImage");
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyGammaCorrection(new Bitmap(pictureBox1.Image), gammaValue);
				}, "ApplyGammaCorrection");
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyLogTransform(new Bitmap(pictureBox1.Image), cValue);
				}, "ApplyLogTransform");
			}
		}

		private void button5_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ConvertToGrayscale(new Bitmap(pictureBox1.Image));
				}, "ConvertToGrayscale");
			}
		}

		private void button6_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ComputeHistogram(new Bitmap(pictureBox1.Image));
				}, "ComputeHistogram");
			}
		}

		private void button7_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.EqualizeHistogram(new Bitmap(pictureBox1.Image));
				}, "EqualizeHistogram");
			}
		}

		private void button8_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyBoxFilter(new Bitmap(pictureBox1.Image));
				}, "ApplyBoxFilter");
			}
		}

		private void button9_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyGaussianFilter(new Bitmap(pictureBox1.Image));
				}, "ApplyGaussianFilter");
			}
		}

		private void button10_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplySobelEdgeDetection(new Bitmap(pictureBox1.Image));
				}, "ApplySobelEdgeDetection");
			}
		}

		private void button11_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.ApplyLaplacianEdgeDetection(new Bitmap(pictureBox1.Image));
				}, "ApplyLaplacianEdgeDetection");
			}
		}

		private void button12_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				RunImageProcessingWithTimer(() =>
				{
					pictureBox2.Image = ImageProcessingAlgorithms.DetectKeypoints(new Bitmap(pictureBox1.Image));
				}, "DetectKeypoints");
			}
		}
	}
}
