using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ProcessMonitor
{
    class MainClass
    {
        static void Main(string[] args)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"D:\JenkinsTestOutput\test_forJenkins\2017-07-18\X64\b123\FPS Tester.exe";
            process.Start();
            //Thread.Sleep(3000);
            
            CPUMonitor cpu_monitor = new CPUMonitor(process.ProcessName, process.Id);
            MemoryMonitor mem_monitor = new MemoryMonitor(process.ProcessName, process.Id);


            Thread cpu_monitor_t = new Thread(cpu_monitor.Update);
            Thread mem_monitor_t = new Thread(mem_monitor.Update);

            cpu_monitor_t.Start();
            mem_monitor_t.Start();
            while (!cpu_monitor_t.IsAlive || !mem_monitor_t.IsAlive)
                Thread.Sleep(10);


            while (!Console.KeyAvailable && !cpu_monitor.MonitorStop && !mem_monitor.MonitorStop)
            {
                Console.WriteLine("CPU(val/max/avg)" + cpu_monitor.Usage.ToString("0.00") + "/" + cpu_monitor.UsageMAX.ToString("0.00") + "/" + cpu_monitor.UsageAvg.ToString("0.00"));
                Console.WriteLine("MEM(val/max)" + mem_monitor.UsageMB.ToString("0.00  MB") + "/" + mem_monitor.UsageMAXMB.ToString("0.00 MB"));
                Thread.Sleep(100);
            }

            cpu_monitor.RequestStop();
            mem_monitor.RequestStop();


            cpu_monitor_t.Join();
            mem_monitor_t.Join();
            Console.WriteLine("main thread: Worker thread has terminated.");
            process.CloseMainWindow();
            process.Close();
            System.Console.ReadLine();
        }
    }
    class Tools
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        public static void PressScrollLock()
        {
            byte SCROLLKEY = 145;  // magic num
            uint KEYEVENTF_KEYUP = 0x0002; // magic num
            keybd_event(SCROLLKEY, 0, 0, 0);
            System.Threading.Thread.Sleep(10);
            keybd_event(SCROLLKEY, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}
