using System;
using System.Diagnostics;

namespace ProcessMonitorLib
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


        int mInterval = 10;

        double mUsage = 0;
        double mUsageMAX = 0;
        #endregion

        /// <summary>
        /// Monitor 是否監控中
        /// </summary>
        public bool MonitorStop { get { return mMonitorStop; } }

        /// <summary>
        /// 擷取時間點 程式總執行時間 (ms)
        /// </summary>
        public double ElapsedTime { get { return mElapsedTime * 1000; } }

        /// <summary>
        /// 記憶體使用量(B)
        /// </summary>
        public double UsageB { get { return mUsage; } }
        /// <summary>
        /// 記憶體使用量(KB)
        /// </summary>
        public double UsageKB { get { return mUsage / 1024.0; } }
        /// <summary>
        /// 記憶體使用量(MB)
        /// </summary>
        public double UsageMB { get { return mUsage / Math.Pow(1024, 2); } }
        /// <summary>
        /// 記憶體使用量(GB)
        /// </summary>
        public double UsageGB { get { return mUsage / Math.Pow(1024, 3); } }
        /// <summary>
        /// 記憶體歷史最大使用量(B)
        /// </summary>
        public double UsageMAXB { get { return mUsageMAX; } }
        /// <summary>
        /// 記憶體歷史最大使用量(KB)
        /// </summary>
        public double UsageMAXKB { get { return mUsageMAX / 1024.0; } }
        /// <summary>
        /// 記憶體歷史最大使用量(MB)
        /// </summary>
        public double UsageMAXMB { get { return mUsageMAX / Math.Pow(1024, 2); } }
        /// <summary>
        /// 記憶體歷史最大使用量(GB)
        /// </summary>
        public double UsageMAXGB { get { return mUsageMAX / Math.Pow(1024, 3); } }

        /// <summary>
        /// 監測間隔時間(ms) min = 10
        /// </summary>
        public int Interval { get { return mInterval; } set { mInterval = Math.Max(value, 10); } }

        public MemoryMonitor(string targeProcess, int pid)
        {
            mStopRequest = false;
            mMonitorStop = false;
            mTragetProcess = new ProcessData(targeProcess, pid);
            mCounter = new PerformanceCounter("Process", "Working Set", true);
            mCounterTime = new PerformanceCounter("Process", "Elapsed Time", true);
        }

        public void Watching()
        {
            while (!mStopRequest && !mMonitorStop)
            {
                Update();
                System.Threading.Thread.Sleep(mInterval);
            }
        }

        public void Update()
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
