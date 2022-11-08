global using System;
global using System.Collections.Generic;
global using System.Windows.Forms;
using Q;
using System.Runtime.InteropServices;

namespace Q {
	public static class App {
		[STAThread]
		public static void Main(string[] args) {
			SetProcessDPIAware();
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += (o, e) => onException(e.Exception);
			AppDomain.CurrentDomain.UnhandledException += (o, e) => onException(e.ExceptionObject as Exception);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
		[DllImport("user32.dll")] public static extern bool SetProcessDPIAware();


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

