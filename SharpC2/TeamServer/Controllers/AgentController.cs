using Microsoft.AspNetCore.SignalR;

using Serilog;

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

        private event EventHandler<AgentEvent> AgentEvent;

        public AgentController(ServerController server, CryptoController crypto)
        {
            Server = server;
            Crypto = crypto;

            AgentEvent += AgentEventHandler;
        }

        public void AgentEventHandler(object sender, AgentEvent e)
        {
            AgentEvents.Add(e);
            Server.HubContext.Clients.All.SendAsync("NewAgentEvent", e);
            Log.Logger.Information("{Event} {AgentId} {Data} {Nick}", e.Type, e.AgentId, e.Data, e.Nick);
        }

        public void WebEventHandler(object sender, WebLog l)
        {
            Server.HubContext.Clients.All.SendAsync("NewWebEvent", l);
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

            AgentEvent?.Invoke(this, new AgentEvent(metadata.AgentID, AgentEventType.InitialAgent));
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
            AgentEvent?.Invoke(this, new AgentEvent(agent.Metadata.AgentID, AgentEventType.ModuleRegistered, module.Name));
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

                var cmd = $"{request.Module} {request.Command} {request.Data}";
                AgentEvent?.Invoke(this, new AgentEvent(request.AgentId, AgentEventType.CommandRequest, cmd, user));
            }
        }

        public void ClearAgentCommandQueue(string agentId)
        {
            var agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(agentId, StringComparison.OrdinalIgnoreCase));

            if (agent != null)
            {
                agent.QueuedCommands.Clear();
            }
        }

        public bool RemoveAgent(string agentId)
        {
            var agent = ConnectedAgents.FirstOrDefault(a => a.Metadata.AgentID.Equals(agentId, StringComparison.OrdinalIgnoreCase));
            return ConnectedAgents.Remove(agent);
        }
    }
}