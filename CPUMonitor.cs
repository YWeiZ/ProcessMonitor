using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace ProcessMonitor
{
    public class CPUMonitor
    {
        #region Private Static Var

        static DateTime mLastUpdatTime;
        int mUpdateTimes = 100;

        PerformanceCounter mCounter;
        PerformanceCounter mCounterTime;

        ProcessData mTragetProcess;

        bool mStopRequest = false;
        bool mMonitorStop = false;

        double mElapsedTime = 0;

        double mUsage = 0;
        double mUsageMAX = 0;

        double mSample = 0;
        long mSampleTimes = 0;
        
        double mTotalSample = 0;
        long mTotalSampleTimes = 0;
        #endregion
        public bool MonitorStop { get { return mMonitorStop; } }

        public double ElapsedTime { get { return mElapsedTime; } }

        public double Usage { get { return mUsage; } }
        public double UsageAvg { get { return (mTotalSampleTimes == 0) ? 0 : (mTotalSample / (double)mTotalSampleTimes); } }
        public double UsageMAX { get { return mUsageMAX; } }

        public int UpdateTimes { get { return mUpdateTimes; } set { mUpdateTimes = value; } }

        public CPUMonitor(string targeProcess,int pid)
        {
            mStopRequest = false;
            mLastUpdatTime = DateTime.Now;
            mTragetProcess = new ProcessData(targeProcess,pid);
            mCounter = new PerformanceCounter("Process", "% Processor Time", true);
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
            DateTime nextUpdateTime = mLastUpdatTime.AddMilliseconds(mUpdateTimes);
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
