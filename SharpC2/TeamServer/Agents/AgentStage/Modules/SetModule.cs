using System;
using System.Collections.Generic;

namespace Agent.Modules
{
    class SetModule : IAgentModule
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
                Name = "set",
                Description = "Set options in the config controller",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse"},
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_"}
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "sleep",
                        Description = "Set the sleep interval and jitter",
                        HelpText = "set sleep [interval] [jitter]",
                        CallBack = SetSleep
                    }
                }
            };
        }

        private void SetSleep(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}