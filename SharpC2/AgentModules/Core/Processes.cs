using Agent.Models;

using System.Diagnostics;
using System.Linq;

namespace Agent
{
    class Processes
    {
        public static void KillProcess(int pid)
        {
            var process = Process.GetProcessById(pid);
            process.Kill();
        }

        public static string GetRunningProcesses()
        {
            var result = new SharpC2ResultList<ProcessListResult>();
            var processes = Process.GetProcesses().OrderBy(p => p.Id);

            foreach (var process in processes)
            {
                result.Add(new ProcessListResult
                {
                    PID = process.Id,
                    Name = process.ProcessName,
                    Session = process.SessionId
                });
            }

            return result.ToString();
        }
    }
}