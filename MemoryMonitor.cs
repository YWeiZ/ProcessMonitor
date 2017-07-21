using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace ProcessMonitor
{
    public class MemoryMonitor
    {
        #region Private Static Var

        PerformanceCounter mCounter;
        ProcessData mTragetProcess;
        
        bool mStopRequest = false;
        bool mMonitorStop = false;

        double mUsage = 0;
        double mUsageMAX = 0;
        #endregion
        public bool MonitorStop { get { return mMonitorStop; } }
        
        public double UsageB { get { return mUsage; } }
        public double UsageKB { get { return mUsage / 1024; } }
        public double UsageMB { get { return mUsage / Math.Pow(1024, 2); } }
        public double UsageGB { get { return mUsage / Math.Pow(1024, 3); } }
        public double UsageMAXB { get { return mUsageMAX; } }
        public double UsageMAXKB { get { return mUsageMAX; } }
        public double UsageMAXMB { get { return mUsageMAX / Math.Pow(1024, 2); } }
        public double UsageMAXGB { get { return mUsageMAX / Math.Pow(1024, 3); } }



        public MemoryMonitor(string targeProcess, int pid)
        {
            mStopRequest = false;
            mMonitorStop = false;
            mTragetProcess = new ProcessData(targeProcess, pid);
            mCounter = new PerformanceCounter("Process", "Working Set", true);
        }

        public void Update()
        {
            while (!mStopRequest)
            {
                try
                { mCounter.InstanceName = mTragetProcess.GetInstanceName(); }
                catch (ArgumentException)
                {
                    Console.WriteLine("Process isn't exisit.");
                    mMonitorStop = true;
                    return;
                }
                UpdateUsage();
                System.Threading.Thread.Sleep(50);
            }
        }


        void UpdateUsage()
        {
            try { mUsage = (double)mCounter.NextValue(); }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Instance Name Change or Process isn't exisit.");
                return;
            };
            mUsageMAX = Math.Max(mUsageMAX, mUsage);
        }
        
        public void RequestStop()
        {
            mStopRequest = true;
        }
        
    }
}

