using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Common.Models;

using System.Collections.Generic;
using System.Text;

namespace Agent.Modules
{
    public class LinkModule : IAgentModule
    {
        private AgentController Agent { get; set; }
        private ConfigController Config { get; set; }

        public AgentModule GetModuleInfo()
        {
            return new AgentModule
            {
                Name = "Link",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "link",
                        Visible = false,
                        CallBack = HandleIncomingLink
                    },
                    new AgentCommand
                    {
                        Name = "unlink",
                        Visible = false,
                        CallBack = HandleIncomingUnlink
                    }
                },
                NotifyTeamServer = false
            };
        }

        private void HandleIncomingUnlink(byte[] data)
        {
            var metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata;
            metadata.ParentAgentID = string.Empty;

            Config.SetOption(ConfigSetting.Metadata, metadata);
        }

        private void HandleIncomingLink(byte[] data)
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