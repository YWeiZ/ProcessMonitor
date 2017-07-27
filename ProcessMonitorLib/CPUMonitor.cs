using System;
using System.Diagnostics;

namespace ProcessMonitorLib
{
    public class CPUMonitor
    {
        #region Private Static Var

        static DateTime mLastUpdatTime;

        PerformanceCounter mCounter;
        PerformanceCounter mCounterTime;

        ProcessData mTragetProcess;

        bool mStopRequest = false;
        bool mMonitorStop = false;

        double mElapsedTime = 0;

        int mInterval = 10;

        double mUsage = 0;
        double mUsageMAX = 0;

        double mSample = 0;
        long mSampleTimes = 0;

        double mTotalSample = 0;
        long mTotalSampleTimes = 0;
        #endregion
        public bool MonitorStop { get { return mMonitorStop; } }


        /// <summary>
        /// 擷取時間點 程式總執行時間 (ms)
        /// </summary>
        public double ElapsedTime { get { return mElapsedTime; } }

        /// <summary>
        /// CPU 使用率 (%)
        /// </summary>
        public double Usage { get { return mUsage; } }
        /// <summary>
        /// CPU 平均使用率 (%)
        /// </summary>
        public double UsageAvg { get { return (mTotalSampleTimes == 0) ? 0 : (mTotalSample / (double)mTotalSampleTimes); } }
        /// <summary>
        /// CPU 瞬間最大使用率 (%)
        /// </summary>
        public double UsageMAX { get { return mUsageMAX; } }
        
        /// <summary>
        /// 監測間隔時間(ms) min = 10
        /// </summary>
        public int Interval { get { return mInterval; } set { mInterval = Math.Max(value, 10); } }

        public CPUMonitor(string targeProcess, int pid)
        {
            mStopRequest = false;
            mLastUpdatTime = DateTime.Now;
            mTragetProcess = new ProcessData(targeProcess, pid);
            mCounter = new PerformanceCounter("Process", "% Processor Time", true);
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
            double getValue = 0;
            try
            {
                getValue = (double)mCounter.NextValue();
                mElapsedTime = (double)mCounterTime.NextValue();
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Instance Name Change or Process isn't exisit.");
                return;
            };


            getValue /= (double)Environment.ProcessorCount;

            mSample += getValue;
            mTotalSample += getValue;

            mSampleTimes++;
            mTotalSampleTimes++;

            DateTime nowTime = DateTime.Now;
            DateTime nextUpdateTime = mLastUpdatTime.AddMilliseconds(mInterval);
            if (nowTime >= nextUpdateTime)
            {
                mUsage = (mSampleTimes == 0) ? 0 : mSample / (double)mSampleTimes;
                mUsageMAX = Math.Max(mUsageMAX, mUsage);

                mSample = 0;
                mSampleTimes = 0;
                mLastUpdatTime = nowTime;
            }
        }

        public void RequestStop()
        {
            mStopRequest = true;
        }
    }
}
