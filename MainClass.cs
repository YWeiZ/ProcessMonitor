

//#define WRITE_MODE


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
            /*Process process = new Process();
            process.StartInfo.FileName = @"D:\JenkinsTestOutput\test_forJenkins\2017-07-18\X64\b123\FPS Tester.exe";
            process.Start();
            //Thread.Sleep(3000);
            */
            string processName = "SpaceWalker";
            int pid = -1;

            CPUMonitor cpu_monitor = new CPUMonitor(processName, pid);
            MemoryMonitor mem_monitor = new MemoryMonitor(processName, pid);
            Thread cpu_monitor_t = new Thread(cpu_monitor.Update);
            Thread mem_monitor_t = new Thread(mem_monitor.Update);

#if WRITE_MODE
            StreamWriter sWriter = new StreamWriter(@"D:\SpaceWalker_CPU_MEM_LOG.txt");
            sWriter.AutoFlush = true;
            sWriter.Write("CPUElapsedTime\tCPUUsage\tCPUUsageMAX\t");
            sWriter.WriteLine("MemElapsedTime\tMemoryUsage\tMemoryUsageMAX");
#endif
            cpu_monitor_t.Start();
            mem_monitor_t.Start();
            while (!cpu_monitor_t.IsAlive || !mem_monitor_t.IsAlive)
                Thread.Sleep(10);
            
            while (!cpu_monitor.MonitorStop && !mem_monitor.MonitorStop)
            {
                double times = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                Console.Write(times.ToString("0.000") + "ms\t" + cpu_monitor.Usage.ToString("0.00") + "\t" + cpu_monitor.UsageMAX.ToString("0.00") + "\t");
                Console.WriteLine(mem_monitor.UsageMB.ToString("0.00") + "\t" + mem_monitor.UsageMAXMB.ToString("0.00"));
#if WRITE_MODE
                sWriter.Write(cpu_monitor.ElapsedTime.ToString("0.0") + "\t" + cpu_monitor.Usage.ToString("0.00") + "\t" + cpu_monitor.UsageMAX.ToString("0.00") + "\t");
                sWriter.WriteLine(mem_monitor.ElapsedTime.ToString("0.0") + "\t" + mem_monitor.UsageMB.ToString("0.00") + "\t" + mem_monitor.UsageMAXMB.ToString("0.00"));
#endif
                Thread.Sleep(100);
            }
            cpu_monitor.RequestStop();
            mem_monitor.RequestStop();


            cpu_monitor_t.Join();
            mem_monitor_t.Join();
            Console.WriteLine("main thread: Worker thread has terminated.");
#if WRITE_MODE
            sWriter.Close();
#endif
            /*
            process.CloseMainWindow();
            process.Close();
            */
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
