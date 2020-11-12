using Microsoft.AspNetCore.SignalR;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TeamServer.Hubs;

namespace TeamServer.Controllers
{
    public class AgentController
    {
        public CryptoController Crypto;

        IHubContext<MessageHub> HubContext;

        List<AgentMetadata> Agents = new List<AgentMetadata>();
        Dictionary<string, Queue<AgentMessage>> AgentTasks = new Dictionary<string, Queue<AgentMessage>>();
        List<AgentEvent> AgentEvents = new List<AgentEvent>();

        event EventHandler<AgentEvent> OnAgentEvent;

        public AgentController(CryptoController Crypto, IHubContext<MessageHub> HubContext)
        {
            this.HubContext = HubContext;
            this.Crypto = Crypto;

            OnAgentEvent += AgentController_OnAgentEvent;
        }

        public void SendAgentCommand(AgentCommandRequest Request, string Nick)
        {
            var builder = new StringBuilder();
            builder.Append(Request.Task.Alias.ToLower());

            var parameters = Request.Task.Parameters;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param.Value != null)
                    {
                        if (param.Name.Equals("Assembly", StringComparison.OrdinalIgnoreCase) || param.Name.Equals("Script", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        };

                        builder.Append(" " + param.Value);

                        // strip paths
                        if (param.Name.Equals("LocalPath", StringComparison.OrdinalIgnoreCase))
                        {
                            param.Value = null;
                        }
                    }
                }
            }

            var task = new AgentTask
            {
                Module = Request.Task.Module,
                Command = Request.Task.Command,
                Parameters = new Dictionary<string, object>()
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (!task.Parameters.ContainsKey(param.Name))
                    {
                        task.Parameters.Add(param.Name, param.Value);
                    }
                    else
                    {
                        task.Parameters[param.Name] = param.Value;
                    }
                    
                }
            }

            var data = Crypto.Encrypt(task, out byte[] iv);

            SendAgentMessage(new AgentMessage
            {
                AgentID = Request.AgentID,
                Data = data,
                IV = iv
            });

            OnAgentEvent?.Invoke(this, new AgentEvent(Request.AgentID, AgentEvent.EventType.CommandRequest, builder.ToString(), Nick));
        }

        public void SendAgentMessage(AgentMessage Message)
        {
            if (!AgentTasks.ContainsKey(Message.AgentID))
            {
                AgentTasks.Add(Message.AgentID, new Queue<AgentMessage>());
            }

            AgentTasks[Message.AgentID].Enqueue(Message);
        }

        public AgentMessage GetAgentTask(string AgentID)
        {
            AgentCheckin(AgentID);

            //var destinationAgent = Agents.FirstOrDefault(a => a.AgentID.Equals(AgentID, StringComparison.OrdinalIgnoreCase));

            //while (true)
            //{
            //    if (!string.IsNullOrEmpty(destinationAgent.ParentAgentID))
            //    {
                    
            //    }
            //}

            if (AgentTasks.ContainsKey(AgentID))
            {
                if (AgentTasks[AgentID].Count > 0)
                {
                    return AgentTasks[AgentID].Dequeue();
                }
            }

            return null;
        }

        public void AddNewAgent(AgentMetadata Metadata)
        {
            if (!Agents.Contains(Metadata))
            {
                Agents.Add(Metadata);
                OnAgentEvent?.Invoke(this, new AgentEvent(Metadata.AgentID, AgentEvent.EventType.InitialAgent, Metadata));
            }
        }

        public IEnumerable<AgentMetadata> GetAgents()
        {
            return Agents;
        }

        public void AddNewEvent(AgentEvent Event)
        {
            OnAgentEvent?.Invoke(this, Event);
        }

        public IEnumerable<AgentEvent> GetEventsSince(DateTime Date)
        {
            return AgentEvents.Where(e => e.Date >= Date);
        }

        void AgentCheckin(string AgentID)
        {
            var agent = Agents.FirstOrDefault(a => a.AgentID.Equals(AgentID, StringComparison.OrdinalIgnoreCase));

            if (agent != null)
            {
                var lastSeen = DateTime.UtcNow;
                agent.LastSeen = lastSeen;
                OnAgentEvent?.Invoke(this, new AgentEvent(AgentID, AgentEvent.EventType.AgentCheckin, lastSeen));
            }
        }

        public async void AgentController_OnAgentEvent(object sender, AgentEvent e)
        {
            AgentEvents.Add(e);
            await HubContext.Clients.All.SendAsync("AgentEvent", e);
        }
    }
}