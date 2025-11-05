using System.Windows.Forms.VisualStyles;
using static System.Net.Mime.MediaTypeNames;

namespace SZKG_Photoshop_O75HNX
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			tableLayoutPanel1 = new TableLayoutPanel();
			tableLayoutPanel2 = new TableLayoutPanel();
			pictureBox1 = new PictureBox();
			pictureBox2 = new PictureBox();
			flowLayoutPanel1 = new FlowLayoutPanel();

			button1 = CreateButton("Load", button1_Click);
			groupBox1 = CreateGroupBox("Load", button1);

			button2 = CreateButton("Invert", button2_Click);
			groupBox2 = CreateGroupBox("Invert", button2);

			string gammaValuePrefix = "Gamma value: ";
			button3 = CreateButton("GammaCorrect", button3_Click);
			groupBox3 = CreateGroupBox("GammaCorrect", button3);
			var gammaLabel = CreateLabel(gammaValuePrefix, gammaValue);
			var gammaTrackBar = CreateParameterTrackBar(value => gammaValue = value, gammaLabel, 0.1, 10, 0.1, 1.0, gammaValuePrefix);
			groupBox3.Controls.Add(gammaLabel);
			groupBox3.Controls.Add(gammaTrackBar);

			string cValuePrefix = "C value: ";
			button4 = CreateButton("LogTransform", button4_Click);
			groupBox4 = CreateGroupBox("LogTransform", button4);
			var cLabel = CreateLabel(cValuePrefix, cValue);
			var cTrackBar = CreateParameterTrackBar(value => cValue = value, cLabel, 5, 100, 1, 46.0, cValuePrefix);
			groupBox4.Controls.Add(cLabel);
			groupBox4.Controls.Add(cTrackBar);

			button5 = CreateButton("GrayScale", button5_Click);
			groupBox5 = CreateGroupBox("GrayScale", button5);

			button6 = CreateButton("ComputeHist", button6_Click);
			groupBox6 = CreateGroupBox("ComputeHist", button6);

			button7 = CreateButton("EqualizeHist", button7_Click);
			groupBox7 = CreateGroupBox("EqualizeHist", button7);

			button8 = CreateButton("BoxFilter", button8_Click);
			groupBox8 = CreateGroupBox("BoxFilter", button8);

			button9 = CreateButton("GaussFilter", button9_Click);
			groupBox9 = CreateGroupBox("GaussFilter", button9);

			button10 = CreateButton("SobelEdge", button10_Click);
			groupBox10 = CreateGroupBox("SobelEdge", button10);

			button11 = CreateButton("LaplacianEdge", button11_Click);
			groupBox11 = CreateGroupBox("LaplacianEdge", button11);

			button12 = CreateButton("KeyPoints", button12_Click);
			groupBox12 = CreateGroupBox("KeyPoints", button12);

			tableLayoutPanel1.SuspendLayout();
			tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
			flowLayoutPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.ColumnCount = 1;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
			tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
			tableLayoutPanel1.Dock = DockStyle.Fill;
			tableLayoutPanel1.Location = new Point(0, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 2;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			tableLayoutPanel1.Size = new Size(1902, 1033);
			tableLayoutPanel1.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			tableLayoutPanel2.ColumnCount = 2;
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel2.Controls.Add(pictureBox1, 0, 0);
			tableLayoutPanel2.Controls.Add(pictureBox2, 1, 0);
			tableLayoutPanel2.Dock = DockStyle.Fill;
			tableLayoutPanel2.Location = new Point(3, 3);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			tableLayoutPanel2.RowCount = 1;
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel2.Size = new Size(1896, 923);
			tableLayoutPanel2.TabIndex = 0;
			// 
			// pictureBox1
			// 
			pictureBox1.BorderStyle = BorderStyle.Fixed3D;
			pictureBox1.Dock = DockStyle.Fill;
			pictureBox1.Location = new Point(3, 3);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(942, 917);
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.TabIndex = 0;
			pictureBox1.TabStop = false;
			// 
			// pictureBox2
			// 
			pictureBox2.BorderStyle = BorderStyle.Fixed3D;
			pictureBox2.Dock = DockStyle.Fill;
			pictureBox2.Location = new Point(951, 3);
			pictureBox2.Name = "pictureBox2";
			pictureBox2.Size = new Size(942, 917);
			pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox2.TabIndex = 1;
			pictureBox2.TabStop = false;
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.AutoScroll = true;
			flowLayoutPanel1.Controls.Add(groupBox1);
			flowLayoutPanel1.Controls.Add(groupBox2);
			flowLayoutPanel1.Controls.Add(groupBox3);
			flowLayoutPanel1.Controls.Add(groupBox4);
			flowLayoutPanel1.Controls.Add(groupBox5);
			flowLayoutPanel1.Controls.Add(groupBox6);
			flowLayoutPanel1.Controls.Add(groupBox7);
			flowLayoutPanel1.Controls.Add(groupBox8);
			flowLayoutPanel1.Controls.Add(groupBox9);
			flowLayoutPanel1.Controls.Add(groupBox10);
			flowLayoutPanel1.Controls.Add(groupBox11);
			flowLayoutPanel1.Controls.Add(groupBox12);
			flowLayoutPanel1.Dock = DockStyle.Fill;
			flowLayoutPanel1.Location = new Point(0, 929);
			flowLayoutPanel1.Margin = new Padding(0);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new Size(1902, 104);
			flowLayoutPanel1.TabIndex = 1;
			flowLayoutPanel1.WrapContents = false;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1902, 1033);
			Controls.Add(tableLayoutPanel1);
			Name = "Form1";
			Text = "Form1";
			WindowState = FormWindowState.Maximized;
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
			flowLayoutPanel1.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private GroupBox CreateGroupBox(string gpName, Button button)
		{
			var groupBox = new GroupBox
			{
				Location = new Point(3, 3),
				Margin = new Padding(3, 0, 3, 3),
				Padding = new Padding(3, 0, 3, 3),
				Size = new Size(130, 155),
				TabIndex = 0,
				TabStop = false,
				Name = gpName
			};

			groupBox.Controls.Add(button);
			return groupBox;
		}

		private Button CreateButton(string buttonName, EventHandler onClick)
		{
			var button = new Button
			{
				Dock = DockStyle.Fill,
				Location = new Point(3, 23),
				Size = new Size(114, 129),
				TabIndex = 1,
				Name = buttonName,
				Text = buttonName
			};

			button.Click += onClick;
			return button;
		}

		private TrackBar CreateParameterTrackBar(Action<double> onValueChanged, Label label, double min, double max, double step, double defaultValue, string labelPrefix)
		{
			int scale = (int)(1 / step); // pl. 0.1 → 10

			var trackBar = new TrackBar
			{
				Minimum = (int)(min * scale),
				Maximum = (int)(max * scale),
				TickFrequency = (int)(scale * (max - min) / 10),
				Value = (int)(defaultValue * scale),
				SmallChange = 1,
				LargeChange = 5,
				Dock = DockStyle.Top,
				Width = 120,
			};

			trackBar.Scroll += (s, e) =>
			{
				double value = trackBar.Value / (double)scale;
				onValueChanged(value);
				label.Text = $"{labelPrefix}: {value:F1}";
			};

			return trackBar;
		}

		private Label CreateLabel(string labelPrefix = "", double defaultValue = 1.0)
		{
			var label = new Label
			{
				Text = $"{labelPrefix}: {defaultValue:F1}",
				AutoSize = true
			};

			return label;
		}

		public double gammaValue = 1.0;
		public double cValue = 46.0;

		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel2;
		private PictureBox pictureBox1;
		private PictureBox pictureBox2;
		private FlowLayoutPanel flowLayoutPanel1;
		private GroupBox groupBox1;
		private Button button1;
		private GroupBox groupBox2;
		private Button button2;
		private GroupBox groupBox3;
		private Button button3;
		private GroupBox groupBox4;
		private Button button4;
		private GroupBox groupBox5;
		private Button button5;
		private GroupBox groupBox6;
		private Button button6;
		private GroupBox groupBox7;
		private Button button7;
		private GroupBox groupBox8;
		private Button button8;
		private GroupBox groupBox9;
		private Button button9;
		private GroupBox groupBox10;
		private Button button10;
		private GroupBox groupBox11;
		private Button button11;
		private GroupBox groupBox12;
		private Button button12;
	}
}
