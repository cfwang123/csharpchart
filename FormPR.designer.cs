using System.Windows.Forms;
﻿namespace Q {
	partial class FormPR {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
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
			this.emsg = new System.Windows.Forms.TextBox();
			this.elock = new System.Windows.Forms.CheckBox();
			this.bclear = new System.Windows.Forms.Button();
			this.bclose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// emsg
			// 
			this.emsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.emsg.Location = new System.Drawing.Point(12, 12);
			this.emsg.Multiline = true;
			this.emsg.Name = "emsg";
			this.emsg.ReadOnly = true;
			this.emsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.emsg.Size = new System.Drawing.Size(394, 256);
			this.emsg.TabIndex = 0;
			// 
			// elock
			// 
			this.elock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.elock.AutoSize = true;
			this.elock.Location = new System.Drawing.Point(139, 282);
			this.elock.Name = "elock";
			this.elock.Size = new System.Drawing.Size(48, 16);
			this.elock.TabIndex = 1;
			this.elock.Text = "锁定";
			this.elock.UseVisualStyleBackColor = true;
			this.elock.CheckedChanged += new System.EventHandler(this.Blockmsg_Click);
			// 
			// bclear
			// 
			this.bclear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.bclear.Location = new System.Drawing.Point(193, 278);
			this.bclear.Name = "bclear";
			this.bclear.Size = new System.Drawing.Size(75, 23);
			this.bclear.TabIndex = 2;
			this.bclear.Text = "清空";
			this.bclear.UseVisualStyleBackColor = true;
			this.bclear.Click += new System.EventHandler(this.Bclearmsg_Click);
			// 
			// bclose
			// 
			this.bclose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bclose.Location = new System.Drawing.Point(331, 278);
			this.bclose.Name = "bclose";
			this.bclose.Size = new System.Drawing.Size(75, 23);
			this.bclose.TabIndex = 3;
			this.bclose.Text = "关闭";
			this.bclose.UseVisualStyleBackColor = true;
			this.bclose.Click += new System.EventHandler(this.Bclose_Click);
			// 
			// FormPR
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(418, 313);
			this.Controls.Add(this.bclose);
			this.Controls.Add(this.bclear);
			this.Controls.Add(this.elock);
			this.Controls.Add(this.emsg);
			this.Name = "FormPR";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "输出";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox emsg;
		private System.Windows.Forms.CheckBox elock;
		private System.Windows.Forms.Button bclear;
		private System.Windows.Forms.Button bclose;
	}
}