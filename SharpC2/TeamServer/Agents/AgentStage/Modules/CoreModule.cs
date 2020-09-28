using System.Collections.Generic;
using System.Linq;

namespace Agent.Modules
{
    class CoreModule : IAgentModule
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
                Name = "core",
                Description = "Not a lot :)",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "nop",
                        Visible = false,
                        CallBack = NOP
                    },
                    new AgentCommand
                    {
                        Name = "exit",
                        Description = "Kill the current agent.",
                        HelpText = "core exit",
                        CallBack = ExitAgent
                    },
                    new AgentCommand
                    {
                        Name = "checkin",
                        Description = "Resend all metadata and loaded modules",
                        HelpText = "core checkin",
                        CallBack = CheckIn
                    },  
                }
            };
        }

        private void CheckIn(byte[] data)
        {
            var modules = Agent.AgentModules.ToList();

            foreach (var module in modules)
            {
                if (module.NotifyTeamServer)
                {
                    Agent.SendModuleData("Core", "RegisterAgentModule", Serialisation.SerialiseData(module));
                }
            }
        }

        private void ExitAgent(byte[] data)
        {
            Agent.Stop();
        }

        private void NOP(byte[] data)
        {
            // nothing
        }
    }
}