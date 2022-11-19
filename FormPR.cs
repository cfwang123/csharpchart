using System;
using System.Collections.Generic;
using System.Windows.Forms;
﻿using System.Text;

namespace Q {
	public sealed partial class FormPR : Form {
		public static FormPR instance = null;
		public static bool IsOpened => instance != null && !instance.IsDisposed;
		public static void OpenForm() {
			if(!IsOpened) {
				instance = new FormPR();
				instance.Show(App.f);
			}
			instance.BringToFront();
		}
	
		public FormPR() {
			InitializeComponent();
			emsg.Text = sbmsg.ToString();
			emsg.Select(emsg.TextLength, 0);
			emsg.ScrollToCaret();
			sbmsg.Clear();
		}
	
		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			sbmsg.Clear().Append(emsg.Text);
			emsg.Text = "";
		}
	
		public static StringBuilder sbmsg = new StringBuilder(30000);
		public static void AppendMessage(string msg) {
			if(string.IsNullOrEmpty(msg))
				return;
			if(instance != null && !instance.IsDisposed && !instance.elock.Checked) {
				var emsg = instance.emsg;
				if(emsg.TextLength >= 50000)
					emsg.Text = emsg.Text.Substring(25000);
				emsg.Select(emsg.TextLength, 0);
				emsg.Paste(msg);
				emsg.Paste("\r\n");
				emsg.Select(emsg.TextLength, 0);
				emsg.ScrollToCaret();
			}
			else {
				if(sbmsg.Length >= 50000)
					sbmsg.Remove(0, 25000);
				sbmsg.Append(msg).Append("\r\n");
			}
		}
	
		private void Blockmsg_Click(object sender, EventArgs e) {
			if(elock.Checked)
				sbmsg.Clear().Append(emsg.Text);
			else {
				emsg.Text = sbmsg.ToString();
				emsg.Select(emsg.TextLength, 0);
				emsg.ScrollToCaret();
				sbmsg.Clear();
			}
		}
	
		private void Bclearmsg_Click(object sender, EventArgs e) {
			sbmsg.Clear();
			emsg.Text = "";
		}
	
		private void Bclose_Click(object sender, EventArgs e) {
			Close();
		}
	}
}