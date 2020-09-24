using System;
using System.Collections.Generic;

namespace Agent.Modules
{
    class ConnectModule : IAgentModule
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
                Name = "link",
                Description = "Link to P2P Agents",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "tcp",
                        Description = "Link to a TCP Agent",
                        HelpText = "link tcp [address] [port]",
                        CallBack = LinkTcpAgent
                    },
                    new AgentCommand
                    {
                        Name = "smb",
                        Description = "Link to an SMB Agent",
                        HelpText = "link smb [address] [pipename]",
                        CallBack = LinkSmbAgent
                    }
                }
            };
        }

        private void LinkTcpAgent(byte[] data)
        {
            throw new NotImplementedException();
        }

        private void LinkSmbAgent(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
