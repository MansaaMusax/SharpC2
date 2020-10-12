using Common.Models;

using Serilog;

using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamServer.Controllers
{
    public class AgentController
    {
        private ServerController Server { get; set; }
        private CryptoController Crypto { get; set; }
        public List<AgentSessionData> ConnectedAgents { get; private set; } = new List<AgentSessionData>();
        public List<AgentEvent> AgentEvents { get; set; } = new List<AgentEvent>();

        private event EventHandler<AgentEvent> OnAgentEvent;
        private event EventHandler<ServerEvent> OnServerEvent;

        public AgentController(ServerController server, CryptoController crypto)
        {
            Server = server;
            Crypto = crypto;

            OnAgentEvent += AgentEventHandler;
            OnServerEvent += Server.ServerEventHandler;
        }

        public void AgentEventHandler(object sender, AgentEvent e)
        {
            AgentEvents.Add(e);
        }

        public void UpdateSession(AgentMetadata metadata)
        {
            if (!ConnectedAgents.Any(a => a.Metadata.AgentID.Equals(metadata.AgentID, StringComparison.OrdinalIgnoreCase)))
            {
                CreateSession(metadata);
            }
            else
            {
                ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(metadata.AgentID, StringComparison.OrdinalIgnoreCase)).Metadata = metadata;
                ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(metadata.AgentID, StringComparison.OrdinalIgnoreCase)).LastSeen = DateTime.UtcNow;
            }
        }

        private void CreateSession(AgentMetadata metadata)
        {
            ConnectedAgents.Add(new AgentSessionData
            {
                Metadata = metadata,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            });

            var data = string.Format("{0}@{1} ({2})", metadata.Identity, metadata.IPAddress, metadata.Hostname);
            OnServerEvent?.Invoke(this, new ServerEvent(ServerEventType.InitialAgent, data));
            Log.Logger.Information("AGENT {Event} {AgentID} {Hostname}", ServerEventType.InitialAgent.ToString(), metadata.AgentID, metadata.Hostname);
        }

        public AgentSessionData GetSession(string agentId)
        {
            return ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(agentId, StringComparison.OrdinalIgnoreCase));
        }

        public void RegisterAgentModule(AgentMetadata metadata, AgentModule module)
        {
            var agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(metadata.AgentID, StringComparison.OrdinalIgnoreCase));

            if (agent == null)
            {
                CreateSession(metadata);
                agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(metadata.AgentID, StringComparison.OrdinalIgnoreCase));
            }

            if (agent.LoadModules.Any(m => m.Name.Equals(module.Name, StringComparison.OrdinalIgnoreCase)))
            {
                agent.LoadModules.Remove(agent.LoadModules.FirstOrDefault(m => m.Name.Equals(module.Name, StringComparison.OrdinalIgnoreCase)));
            }

            agent.LoadModules.Add(module);
            OnAgentEvent?.Invoke(this, new AgentEvent(agent.Metadata.AgentID, AgentEventType.ModuleRegistered, module.Name));
            Log.Logger.Information("AGENT {Event} {ModuleName}", AgentEventType.ModuleRegistered.ToString(), module.Name);
        }

        public void SendDataToAgent(string agentId, string module, string command, byte[] data)
        {
            var agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(agentId, StringComparison.OrdinalIgnoreCase));

            if (agent != null)
            {
                while (true)
                {
                    if (!string.IsNullOrEmpty(agent.Metadata.ParentAgentID))
                    {
                        var parentAgent = agent.Metadata.ParentAgentID;
                        agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(parentAgent, StringComparison.OrdinalIgnoreCase));
                        if (string.IsNullOrEmpty(agent.Metadata.ParentAgentID))
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                agent.QueuedCommands.Enqueue(new AgentMessage
                {
                    IdempotencyKey = Guid.NewGuid().ToString(),
                    Metadata = new AgentMetadata(),
                    Data = new C2Data { AgentID = agentId, Module = module, Command = command, Data = data }
                });
            }
        }

        public void SendAgentCommand(AgentCommandRequest request, string user)
        {
            var agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(request.AgentId, StringComparison.OrdinalIgnoreCase));

            if (agent != null)
            {
                while (true)
                {
                    if (!string.IsNullOrEmpty(agent.Metadata.ParentAgentID))
                    {
                        var parentAgent = agent.Metadata.ParentAgentID;
                        agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(parentAgent, StringComparison.OrdinalIgnoreCase));
                        if (string.IsNullOrEmpty(agent.Metadata.ParentAgentID))
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                agent.QueuedCommands.Enqueue(new AgentMessage
                {
                    IdempotencyKey = Guid.NewGuid().ToString(),
                    Metadata = new AgentMetadata(),
                    Data = new C2Data { AgentID = request.AgentId, Module = request.Module, Command = request.Command, Data = Encoding.UTF8.GetBytes(request.Data) }
                });

                OnAgentEvent?.Invoke(this, new AgentEvent(request.AgentId, AgentEventType.CommandRequest, request.Command));
                Log.Logger.Information("AGENT {Event} {AgentID} {Command} {Nick}", AgentEventType.CommandRequest.ToString(), request.AgentId, request.Command, user);
            }
        }

        public void ClearAgentCommandQueue(string agentId)
        {
            try
            {
                ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(agentId, StringComparison.OrdinalIgnoreCase)).QueuedCommands.Clear();
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        public void RemoveAgent(string agentId)
        {
            try
            {
                var agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(agentId, StringComparison.OrdinalIgnoreCase));
                ConnectedAgents.Remove(agent);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }
    }
}