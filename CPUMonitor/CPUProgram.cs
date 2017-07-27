using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CPUMonitor
{
    class CPUProgram
    {
        static string sLogFile = "";
        static string sProcessName = "";
        static int sInterval = 10;
        static int sPID = -1;


        static void Main(string[] args)
        {
            CustomizedCommandLine(args);
            ProcessMonitorLib.CPUMonitor cpuMonitor = new ProcessMonitorLib.CPUMonitor(sProcessName, sPID);
            while (!cpuMonitor.MonitorStop)
            {
                cpuMonitor.Update();
                
            }
        }


        static void CustomizedCommandLine(string[] args)
        {
            Dictionary<string, Action<string>> cmdActions = new Dictionary<string, Action<string>>
            {
                {"-LogFile", delegate (string argument) {sLogFile = argument;}} ,
                {"-ProcessName", delegate (string argument) {sProcessName = argument;}} ,
                {"-PID", delegate (string argument) {sPID = int.Parse(argument);}} ,
                {"-Interval", delegate (string argument) {sInterval = int.Parse(argument);}}
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

            if (string.IsNullOrEmpty(sLogFile))
                sLogFile = "cpu.log";

            Debug.Fail("Error No Process Name Ser.", "The Monitor can't monitor process CPU usage.");
            if (string.IsNullOrEmpty(sProcessName))
                Debug.Fail("No Process Name.");

        }
    }
}
