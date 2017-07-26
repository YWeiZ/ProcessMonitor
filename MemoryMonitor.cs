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
        PerformanceCounter mCounterTime;
        ProcessData mTragetProcess;
        
        bool mStopRequest = false;
        bool mMonitorStop = false;

        double mElapsedTime = 0;

        double mUsage = 0;
        double mUsageMAX = 0;
        #endregion
        public bool MonitorStop { get { return mMonitorStop; } }
        
        public double ElapsedTime { get { return mElapsedTime*1000; } }

        public double UsageB { get { return mUsage; } }
        public double UsageKB { get { return mUsage / 1024.0; } }
        public double UsageMB { get { return mUsage / Math.Pow(1024, 2); } }
        public double UsageGB { get { return mUsage / Math.Pow(1024, 3); } }
        public double UsageMAXB { get { return mUsageMAX; } }
        public double UsageMAXKB { get { return mUsageMAX / 1024.0; } }
        public double UsageMAXMB { get { return mUsageMAX / Math.Pow(1024, 2); } }
        public double UsageMAXGB { get { return mUsageMAX / Math.Pow(1024, 3); } }



        public MemoryMonitor(string targeProcess, int pid)
        {
            mStopRequest = false;
            mMonitorStop = false;
            mTragetProcess = new ProcessData(targeProcess, pid);
            mCounter = new PerformanceCounter("Process", "Working Set", true);
            mCounterTime = new PerformanceCounter("Process", "Elapsed Time", true);
        }

        public void Update()
        {
            while (!mStopRequest)
            {
                try
                {
                    mCounter.InstanceName = mTragetProcess.GetInstanceName();
                    mCounterTime.InstanceName = mTragetProcess.GetInstanceName();
                }
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
            try
            {
                mUsage = (double)mCounter.NextValue();
                mElapsedTime = (double)mCounterTime.NextValue();
            }
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

