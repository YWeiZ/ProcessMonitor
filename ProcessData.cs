using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProcessMonitor
{
    public class ProcessData
    {
        Dictionary<int, string> mProcessInstanceName = new Dictionary<int, string>();
        PerformanceCounter mIDCounter = new PerformanceCounter("Process", "ID Process", true);
        string mProcessName;
        int mPID = -1;

        string ProcessName
        {
            get { return mProcessName; }
            set
            {
                mProcessName = value;
                UpdateProcessName();
            }
        }
        int PID { get { return mPID; } set { mPID = value; } }

        public ProcessData(string trace_pName, int pid = -1)
        {
            mProcessName = trace_pName;
            mPID = pid;
        }

        void UpdateProcessName()
        {
            mProcessInstanceName.Clear();
            var instanceNames = new PerformanceCounterCategory("Process").GetInstanceNames();
            for (int i = 0; i < instanceNames.Length; i++)
            {
                if (instanceNames[i].Contains(mProcessName))
                {
                    mIDCounter.InstanceName = instanceNames[i];
                    int pid = 0;
                    try
                    { pid = (int)mIDCounter.NextValue(); }
                    catch (InvalidOperationException) // 註:會跳這個例外是因為在抓到process的這一瞬間process被關掉 所以在更新一次直到被關光光就不會有問題了
                    { UpdateProcessName(); }
                    mProcessInstanceName.Add(pid, instanceNames[i]);
                }
            }
        }

        public string GetInstanceName(int pid = -1)
        {
            int id = (pid != -1) ? pid : mPID;

            if (id == -1)
                throw new ArgumentException("Doesn't set target PID.");

            UpdateProcessName();
            string instanceName = "";
            if (!mProcessInstanceName.TryGetValue(id, out instanceName))
                throw new ArgumentException("Invalid pid:" + id + ".");
            else
                return instanceName;
        }
    }
}
