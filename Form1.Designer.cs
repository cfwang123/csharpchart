namespace Q {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.edatanum = new System.Windows.Forms.ComboBox();
			this.etimewatch = new System.Windows.Forms.CheckBox();
			this.pic = new Q.Chart.MyChart();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 15;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 4);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "Basic";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(189, 4);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(82, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Big Data";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(277, 4);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 2;
			this.button3.Text = "Date Axis";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// edatanum
			// 
			this.edatanum.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.edatanum.FormattingEnabled = true;
			this.edatanum.Location = new System.Drawing.Point(93, 6);
			this.edatanum.Name = "edatanum";
			this.edatanum.Size = new System.Drawing.Size(90, 20);
			this.edatanum.TabIndex = 3;
			// 
			// etimewatch
			// 
			this.etimewatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.etimewatch.AutoSize = true;
			this.etimewatch.Location = new System.Drawing.Point(735, 8);
			this.etimewatch.Name = "etimewatch";
			this.etimewatch.Size = new System.Drawing.Size(96, 16);
			this.etimewatch.TabIndex = 4;
			this.etimewatch.Text = "Measure Time";
			this.etimewatch.UseVisualStyleBackColor = true;
			// 
			// pic
			// 
			this.pic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pic.Location = new System.Drawing.Point(12, 33);
			this.pic.Name = "pic";
			this.pic.Size = new System.Drawing.Size(819, 462);
			this.pic.TabIndex = 0;
			this.pic.TabStop = false;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(843, 507);
			this.Controls.Add(this.etimewatch);
			this.Controls.Add(this.edatanum);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.pic);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Chart.MyChart pic;
		private Timer timer1;
		private Button button1;
		private Button button2;
		private Button button3;
		private ComboBox edatanum;
		private CheckBox etimewatch;
	}
}

