using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using RadminSavePassword.Hook;

namespace RadminSavePassword
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool canCreateNew;
            Mutex mutexLock = new Mutex(true, Application.ProductName, out canCreateNew);
            if (canCreateNew)
            {
                Application.ThreadException += Application_ThreadException;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());

                mutexLock.ReleaseMutex();//退出锁定.然后再退出程序.
            }
            else
            {
                IntPtr windowHandle = RuningWindow();//获取本程序的运行实例.
                HandleRunningWindow(windowHandle);//激活该程序,并显示在最前端.
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(string.Format("未处理的异常：{0}", e.Exception.Message), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(string.Format("未处理的异常：{0}", ex.Message), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 将指定的窗口 显示到最前端.
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        private static void HandleRunningWindow(IntPtr handle)
        {
            WindowsApi.ShowWindowAsync(handle, WindowsApi.SW_SHOWNOMAL);//显示
            WindowsApi.SetForegroundWindow(handle);//当到最前端
        }

        /// <summary>
        /// 获取运行实例.
        /// </summary>
        /// <returns></returns>
        private static IntPtr RuningWindow()
        {
            return WindowsApi.FindWindow(null, Global.ProgramText);
        }
    }
}
