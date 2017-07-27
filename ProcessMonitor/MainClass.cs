

#define WRITE_MODE

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;
using System.Runtime.InteropServices;
using System.Threading;



namespace ProcessMonitor
{
    class MainClass
    {
        static string sLogFile = "";
        static string sProcessFile = "";
        static string sGPUCapture = "";
        static int sInterval = 10;

        static DateTime sProcessStartTime;

#if WRITE_MODE
        static StreamWriter sWriter;
#endif
        static int DuringTime = 60;

        static void Main(string[] args)
        {
            CustomizedCommandLine(args);
            Process[] proc = Process.GetProcessesByName("FCAT_VR_Capture");
            if (proc.Length > 0)
            {
                proc[0].Kill();
                proc[0].Close();
            }

            CreatSettingForGPUCapture();
            Process gpu_caputure = new Process();
            #region GPU Capture Start
            gpu_caputure.StartInfo.FileName = sGPUCapture;
            gpu_caputure.Start();
            #endregion

            Process target_process = new Process();
            #region Target Process Start
            target_process.Exited += new EventHandler(Target_process_Exited);
            target_process.EnableRaisingEvents = true;
            target_process.StartInfo.FileName = sProcessFile;
            target_process.Start();
            #endregion

            // 等待10 直到讓就緒
            Thread.Sleep(20000);



#if WRITE_MODE
            #region Log File 起始設定
            var logFileName = Path.Combine(sLogFile, "cpu_mem.log");
            sWriter = new StreamWriter(logFileName);
            sWriter.AutoFlush = true;
            //sWriter.Write("CPUElapsedTime\tCPUUsage\tCPUUsageMAX\t");
            //sWriter.WriteLine("MemElapsedTime\tMemoryUsage\tMemoryUsageMAX");
            sWriter.Write("Time\tCPUUsage\tCPUUsageMAX\t");
            sWriter.WriteLine("MemoryUsage\tMemoryUsageMAX");
            #endregion
#endif
            // 紀錄 GPU

            ProcessMonitorLib.CPUMonitor cpu_monitor = new ProcessMonitorLib.CPUMonitor(target_process.ProcessName, target_process.Id);
            ProcessMonitorLib.MemoryMonitor mem_monitor = new ProcessMonitorLib.MemoryMonitor(target_process.ProcessName, target_process.Id);
            #region 執行 CPU Memory 監視
            cpu_monitor.Interval = sInterval;
            mem_monitor.Interval = sInterval;
            Thread t_cpu_monitor = new Thread(cpu_monitor.Watching);
            Thread t_mem_monitor = new Thread(mem_monitor.Watching);
            t_cpu_monitor.Start();
            t_mem_monitor.Start();
            while (!t_cpu_monitor.IsAlive || !t_mem_monitor.IsAlive)
                Thread.Sleep(10);
            #endregion


            Tools.PressScrollLock();
            sProcessStartTime = DateTime.Now;
            while (!cpu_monitor.MonitorStop && !mem_monitor.MonitorStop)
            {
                double times = (DateTime.Now.Ticks - sProcessStartTime.Ticks) / TimeSpan.TicksPerMillisecond;
                Console.Write(times.ToString("0.000") + "ms\t" + cpu_monitor.Usage.ToString("0.00") + "\t" + cpu_monitor.UsageMAX.ToString("0.00") + "\t");
                Console.WriteLine(mem_monitor.UsageMB.ToString("0.00") + "\t" + mem_monitor.UsageMAXMB.ToString("0.00"));
#if WRITE_MODE
                sWriter.Write(times.ToString() + "ms\t" + cpu_monitor.Usage.ToString() + "\t" + cpu_monitor.UsageMAX.ToString() + "\t");
                sWriter.WriteLine(mem_monitor.UsageB.ToString() + "\t" + mem_monitor.UsageMAXB.ToString());
#endif

                if (DateTime.Now >= sProcessStartTime.AddSeconds(DuringTime))
                    break;
                Thread.Sleep(sInterval);
            }

            // 停止紀錄 GPU
            Tools.PressScrollLock();
            Thread.Sleep(3000);

            cpu_monitor.RequestStop();
            mem_monitor.RequestStop();
            t_cpu_monitor.Join();
            t_cpu_monitor.Join();

            if (!gpu_caputure.HasExited)
                gpu_caputure.Kill();

            if (!target_process.HasExited)
                target_process.Kill();

            gpu_caputure.Close();
            target_process.Close();

            Console.WriteLine("Monitor Process Done!!");

#if WRITE_MODE
            sWriter.Close();
#endif
            System.Console.ReadLine();
        }

        private static void Target_process_Exited(object sender, EventArgs e)
        {
            if (DateTime.Now < sProcessStartTime.AddSeconds(DuringTime))
            {
                string detail = "ERROR 執行時間：" + (DateTime.Now - sProcessStartTime).TotalMilliseconds + " 不足設定 " + DuringTime * 1000 + " ms";
#if WRITE_MODE
                sWriter.WriteLine(detail);
#else
                Debug.Assert(false, "程式在監控期間關閉", detail);
#endif

            }


        }

        static void CustomizedCommandLine(string[] args)
        {
            Dictionary<string, Action<string>> cmdActions = new Dictionary<string, Action<string>>
            {
                {"-LogFile", delegate (string argument) {sLogFile = argument;}} ,
                {"-ProcessFile", delegate (string argument) {sProcessFile = argument;}} ,
                {"-Interval", delegate (string argument) {sInterval = int.Parse(argument);}},
                {"-GPUCapture", delegate (string argument) {sGPUCapture = argument;}}
            };

            Action<string> actionCache;
            for (int idx = 0; idx < args.Length; idx++)
            {
                if (cmdActions.ContainsKey(args[idx]))
                {
                    actionCache = cmdActions[args[idx]];
                    actionCache(args[idx + 1]);
                }
            }

            var timepath = string.Format("{0:yyyy-MM-dd-hh-mm}", DateTime.Now);
            sLogFile = Path.Combine(sLogFile, timepath);
            if (!System.IO.Directory.Exists(sLogFile))
                System.IO.Directory.CreateDirectory(sLogFile);



            if (string.IsNullOrEmpty(sProcessFile))
                throw new ArgumentException("Error No Process File Argument. The Monitor can't monitor traget process.", "-ProcessFile");

            if (!File.Exists(sProcessFile))
                throw new FileNotFoundException("Can't find the process.", sProcessFile);

            if (string.IsNullOrEmpty(sGPUCapture))
                throw new ArgumentException("Error The GPU capture has NO setting. The Monitor can't monitor traget process.", "-GPUCapture");

            if (!File.Exists(sGPUCapture))
                throw new FileNotFoundException("Can't find the GPU capture.", sProcessFile);
            
        }

        static void CreatSettingForGPUCapture()
        {
            StreamWriter writer = new StreamWriter("Settings.ini");
            writer.WriteLine("[Setting]");
            writer.WriteLine("BenchmarkDirectory=" + sLogFile);
            writer.WriteLine("BenchmarkingHotkey=145");
            writer.WriteLine("IsHotkeyEnabled=False");
            writer.WriteLine("CPUPresentTimeChecked=True");
            writer.WriteLine("FlipTimeChecked=True");
            writer.WriteLine("AnimationTimeChecked=True");
            writer.WriteLine("StopBenchmarkingChecked=True");
            writer.WriteLine("BenchmarkingTime="+sInterval);
            writer.WriteLine("CaptureDelay=0");
            writer.WriteLine("CaptureDuration=600");
            writer.Close();
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
