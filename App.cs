using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Q {
	public static class App {
		public static Form f;
		[STAThread]
		public static void Main(string[] args) {
			SetProcessDPIAware();
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += (o, e) => onException(e.Exception);
			AppDomain.CurrentDomain.UnhandledException += (o, e) => onException(e.ExceptionObject as Exception);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(f = new Form1());
		}
		[DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
		public static void mbox(string s) => MessageBox.Show(s);

		public static void pr(string s) {
			if (!FormPR.IsOpened) FormPR.OpenForm();
			FormPR.AppendMessage(s);
		}


		static int errcount = 0;
		static void onException(Exception exo) {
			Exception ex = exo.InnerException ?? exo;
			string msg = $"Error({ex.GetType().Name})：{ex.Message}\r\nTrace:\r\n{ex.StackTrace}";
			MessageBox.Show(null, msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			errcount++;
			if (errcount == 1) {
				if (MessageBox.Show("Close？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
					Application.Exit();
			}
			errcount--;
		}
	}
}
