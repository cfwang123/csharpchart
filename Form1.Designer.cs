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
			this.emove = new System.Windows.Forms.CheckBox();
			this.pic = new Q.Chart.MyChart();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 16;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// emove
			// 
			this.emove.AutoSize = true;
			this.emove.Location = new System.Drawing.Point(31, 8);
			this.emove.Name = "emove";
			this.emove.Size = new System.Drawing.Size(48, 16);
			this.emove.TabIndex = 1;
			this.emove.Text = "move";
			this.emove.UseVisualStyleBackColor = true;
			// 
			// pic
			// 
			this.pic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pic.Location = new System.Drawing.Point(12, 33);
			this.pic.Name = "pic";
			this.pic.Size = new System.Drawing.Size(847, 480);
			this.pic.TabIndex = 0;
			this.pic.TabStop = false;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(466, 4);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(871, 547);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.emove);
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
		private CheckBox emove;
		private Button button1;
	}
}

