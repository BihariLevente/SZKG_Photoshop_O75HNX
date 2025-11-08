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

            string imgSizePrefix = "Image size:\n ";
            imgSizeLabel = CreateLabel(0, imgSizePrefix);
            button1 = CreateButton("Load", button1_Click);
            tablePanel1 = CreateTableLayoutPanel(imgSizeLabel, button1);

            button2 = CreateButton("Save", button2_Click);
			tablePanel2 = CreateTableLayoutPanel(button2);

            button3 = CreateButton("Invert", button3_Click);
			tablePanel3 = CreateTableLayoutPanel(button3);

			string gammaValuePrefix = "Gamma: ";
            button4 = CreateButton("GammaCorrect", button4_Click);
            var gammaLabel = CreateLabel(gammaValue, gammaValuePrefix);
            var gammaTrackBar = CreateParameterTrackBar(value => gammaValue = value, gammaLabel, 0.1, 10, 0.1, 1, gammaValuePrefix);
			tablePanel4 = CreateTableLayoutPanel(gammaLabel, gammaTrackBar, button4);

            string cValuePrefix = "C: ";
            button5 = CreateButton("LogTransform", button5_Click);
            var cLabel = CreateLabel(cValue, cValuePrefix);
            var cTrackBar = CreateParameterTrackBar(value => cValue = (int)value, cLabel, 5, 100, 1, 46, cValuePrefix);
			tablePanel5 = CreateTableLayoutPanel(cLabel, cTrackBar, button5);

			button6 = CreateButton("GrayScale", button6_Click);
			tablePanel6 = CreateTableLayoutPanel(button6);

			button7 = CreateButton("ComputeHist", button7_Click);
			tablePanel7 = CreateTableLayoutPanel(button7);

			button8 = CreateButton("EqualizeHist", button8_Click);
			tablePanel8 = CreateTableLayoutPanel(button8);

			string k1ValuePrefix = "Kernel: ";
            button9 = CreateButton("BoxFilter", button9_Click);
            var k1Label = CreateLabel(k1Value, k1ValuePrefix);
            var k1TrackBar = CreateParameterTrackBar(value => k1Value = (int)value, k1Label, 1, 15, 2, 3, k1ValuePrefix);
			tablePanel9 = CreateTableLayoutPanel(k1Label, k1TrackBar, button9);

			button10 = CreateButton("GaussFilter", button10_Click);
			tablePanel10 = CreateTableLayoutPanel(button10);

			button11 = CreateButton("SobelEdge", button11_Click);
			tablePanel11 = CreateTableLayoutPanel(button11);

			button12 = CreateButton("LaplacianEdge", button12_Click);
			tablePanel12 = CreateTableLayoutPanel(button12);

			button13 = CreateButton("KeyPoints", button13_Click);
			tablePanel13 = CreateTableLayoutPanel(button13);

			button14 = CreateButton("Left <- Right", button14_Click);
			tablePanel14 = CreateTableLayoutPanel(button14);

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
            flowLayoutPanel1.Controls.Add(tablePanel1);
            flowLayoutPanel1.Controls.Add(tablePanel2);
            flowLayoutPanel1.Controls.Add(tablePanel3);
            flowLayoutPanel1.Controls.Add(tablePanel4);
            flowLayoutPanel1.Controls.Add(tablePanel5);
            flowLayoutPanel1.Controls.Add(tablePanel6);
            flowLayoutPanel1.Controls.Add(tablePanel7);
            flowLayoutPanel1.Controls.Add(tablePanel8);
            flowLayoutPanel1.Controls.Add(tablePanel9);
            flowLayoutPanel1.Controls.Add(tablePanel10);
            flowLayoutPanel1.Controls.Add(tablePanel11);
            flowLayoutPanel1.Controls.Add(tablePanel12);
            flowLayoutPanel1.Controls.Add(tablePanel13);
            flowLayoutPanel1.Controls.Add(tablePanel14);
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

        private GroupBox CreateGroupBox(Button button = null)
        {
            var groupBox = new GroupBox
            {
                Location = new Point(3, 3),
                Margin = new Padding(3, 0, 3, 3),
                Padding = new Padding(3, 0, 3, 3),
                Size = new Size(130, 155),
                TabIndex = 0,
                TabStop = false
            };

            groupBox.Controls.Add(button);

            return groupBox;
        }

		private TableLayoutPanel CreateTableLayoutPanel(params Control[] controls)
		{
			var panel = new TableLayoutPanel
			{
				ColumnCount = 1,
				AutoSize = false,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				Margin = new Padding(0),
				Padding = new Padding(0),
				Dock = DockStyle.None
			};
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

			int dynamicCount = controls.Count(c => c is not Label);
			float percentEach = dynamicCount > 0 ? 100f / dynamicCount : 0f;

			panel.RowCount = controls.Length;

			for (int i = 0; i < controls.Length; i++)
			{
				var c = controls[i];
				c.Margin = new Padding(0);

				c.Dock = (c is Label) ? DockStyle.Top : DockStyle.Fill;

				if (c is Label)
					panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				else
					panel.RowStyles.Add(new RowStyle(SizeType.Percent, percentEach));

				panel.Controls.Add(c, 0, i);
			}

			if (dynamicCount > 0)
				panel.Height = 50;

			return panel;
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
            int stepsCount = (int)Math.Round((max - min) / step);

            var trackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = stepsCount,
                TickFrequency = Math.Max(1, stepsCount / 10),
                Value = (int)Math.Round((defaultValue - min) / step),
                SmallChange = 1,
                LargeChange = Math.Max(1, stepsCount / 10),
                Dock = DockStyle.Top,
                Width = 120
			};

            trackBar.Scroll += (s, e) =>
            {
                double value = min + trackBar.Value * step;
                onValueChanged(value);
                label.Text = $"{labelPrefix}{Math.Round(value,1)}";
            };

            return trackBar;
        }

        private Label CreateLabel(double defaultValue, string labelPrefix = "")
        {
            var label = new Label
            {
				TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
				Text = $"{labelPrefix}{Math.Round(defaultValue, 1)}",
                AutoSize = true
            };

            return label;
        }

        private FlowLayoutPanel flowLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;

        private TableLayoutPanel tablePanel1;
        private Button button1;
        public Label imgSizeLabel;

        private TableLayoutPanel tablePanel2;
        private Button button2;

        private TableLayoutPanel tablePanel3;
        private Button button3;

        private TableLayoutPanel tablePanel4;
        private Button button4;
        public double gammaValue = 1.0;

        private TableLayoutPanel tablePanel5;
        private Button button5;
        public int cValue = 46;

        private TableLayoutPanel tablePanel6;
        private Button button6;

        private TableLayoutPanel tablePanel7;
        private Button button7;

        private TableLayoutPanel tablePanel8;
        private Button button8;

        private TableLayoutPanel tablePanel9;
        private Button button9;
        public int k1Value = 3;

        private TableLayoutPanel tablePanel10;
        private Button button10;

        private TableLayoutPanel tablePanel11;
        private Button button11;

        private TableLayoutPanel tablePanel12;
        private Button button12;

        private TableLayoutPanel tablePanel13;
        private Button button13;

        private TableLayoutPanel tablePanel14;
        private Button button14;
    }
}
