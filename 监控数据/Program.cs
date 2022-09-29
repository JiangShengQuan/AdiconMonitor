using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace 监控数据
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int WS_SHOWNORMAL = 1;

        [STAThread]
        static void Main(string[] args)
        {
            //得到正在运行的例程
            Process instance = RunningInstance();
            if (instance == null)
            {
                //如果没有其它例程，就新建一个窗体
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //Application.Run(new FormMain());
                Application.Run(new FormMain());
            }
            else
            {
                //处理发现的例程
                HandleRunningInstance(instance);
            }            
        }

        // <summary>
        /// 得到正在运行的进程
        /// </summary>
        /// <returns></returns>
        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);

            //遍历正在有相同名字运行的进程
            foreach (Process process in processes)
            {
                //忽略现有的进程
                if (process.Id != current.Id)
                {
                    //确保进程从EXE文件运行
                    if (process.MainModule.FileName == current.MainModule.FileName)
                    {
                        // 返回另一个进程实例
                        return process;
                    }
                }
            }
            //没有其它的进程，返回Null
            return null;
        }
        /// <summary>
        /// 处理正在运行的进程,也就是将其激活
        /// </summary>
        /// <param name="instance">要处理的进程</param>
        public static void HandleRunningInstance(Process instance)
        {
            MessageBox.Show("程序已经在运行了，一次只能运行一个实例！");
            //确保窗口没有被最小化或最大化
            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);
            //设置真实进程为foreground window
            SetForegroundWindow(instance.MainWindowHandle);
        }

    }
}
