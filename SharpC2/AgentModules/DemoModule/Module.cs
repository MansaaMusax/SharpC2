using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System.Collections.Generic;

namespace Agent
{
    public class Module : IAgentModule
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
                Name = "Demo",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Demo",
                        Delegate = ExecuteDemo
                    }
                }
            };
        }

        private void ExecuteDemo(string AgentID, C2Data C2Data)
        {
            Agent.SendMessage("This is a demo module");
        }
    }
}