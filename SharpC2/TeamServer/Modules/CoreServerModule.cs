using System;
using System.Text;
using System.Collections.Generic;

using TeamServer.Controllers;
using TeamServer.Interfaces;
using TeamServer.Models;

using Common.Models;
using Common;

using SharpC2.Models;

namespace TeamServer.Modules
{
    public class CoreServerModule : IServerModule
    {
        private ServerController Server { get; set; }
        private AgentController Agent { get; set; }

        private event EventHandler<AgentEvent> OnAgentEvent;

        public void Init(ServerController server, AgentController agent)
        {
            Server = server;
            Agent = agent;

            OnAgentEvent += Agent.AgentEventHandler;
        }

        public ServerModule GetModuleInfo()
        {
            return new ServerModule
            {
                Name = "Core",
                Description = "Handles the mimimum core server functionality.",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                ServerCommands = new List<ServerCommand>
                {
                    new ServerCommand
                    {
                        Name = "AgentCheckIn",
                        Description = "Handles Agent checkin.",
                        CallBack = HandleAgentCheckin
                    },
                    new ServerCommand
                    {
                        Name = "RegisterAgentModule",
                        Description = "Registers a new agent module.",
                        CallBack = RegisterAgentModule
                    },
                    new ServerCommand
                    {
                        Name = "AgentOutput",
                        Description = "Handles standard agent command output.",
                        CallBack = HandleAgentOutput
                    },
                    new ServerCommand
                    {
                        Name = "AgentError",
                        Description = "Handles agent error messages.",
                        CallBack = HandleAgentError
                    }
                }
            };
        }

        private void HandleAgentOutput(AgentMetadata metadata, C2Data c2Data)
        {
            var output = Encoding.UTF8.GetString(c2Data.Data);
            OnAgentEvent?.Invoke(this, new AgentEvent(metadata.AgentID, AgentEventType.CommandResponse, output));
        }

        private void HandleAgentError(AgentMetadata metadata, C2Data c2Data)
        {
            var error = Encoding.UTF8.GetString(c2Data.Data);
            OnAgentEvent?.Invoke(this, new AgentEvent(metadata.AgentID, AgentEventType.AgentError, error));
        }

        private void RegisterAgentModule(AgentMetadata metadata, C2Data c2Data)
        {
            var moduleInfo = Serialisation.DeserialiseData<AgentModule>(c2Data.Data);
            Agent.RegisterAgentModule(metadata, moduleInfo);
        }

        private void HandleAgentCheckin(AgentMetadata metadata, C2Data c2Data)
        {
            Agent.UpdateSession(metadata);
        }
    }
}