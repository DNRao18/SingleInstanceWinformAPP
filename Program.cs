using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace SingleInstanceWinformAPP
{

	public static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += Application_ThreadException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			if (!new Mutex(false, Application.ProductName).WaitOne(1, true) && Program.CheckOtherInstance(Application.ProductName))
			{
				return;
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
		private static bool CheckOtherInstance(string mainWindowTitle)
		{
			bool result = false;
			Process currentProcess = Process.GetCurrentProcess();
			Process[] processesByName = Process.GetProcessesByName(currentProcess.ProcessName);
			foreach (Process process in processesByName)
			{
				if (process.Id == currentProcess.Id)
				{
					continue;
				}
				IntPtr mainWindowHandle = process.MainWindowHandle;
				if (mainWindowHandle != IntPtr.Zero && process.ProcessName != null && process.ProcessName.Contains(mainWindowTitle))
				{
					if (TcWindow.IsIconic(mainWindowHandle) != 0)
					{
						TcWindow.MinimizeWindow(mainWindowHandle, false);
					}
					result = TcWindow.SetForegroundWindow(mainWindowHandle) != 0;
				}
			}
			return result;
		}
		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
	    }
		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
		}
	}
}
