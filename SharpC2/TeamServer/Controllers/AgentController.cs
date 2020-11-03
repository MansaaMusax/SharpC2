using Microsoft.AspNetCore.SignalR;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;

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
            //var sessionKey = Crypto.GetSessionKey(Request.AgentID);

            //Utilities.EncryptData(sessionKey, new C2Data
            //{
            //    Module = Request.Module,
            //    Command = Request.Command,
            //    Data = Request.Data
            //},
            //out byte[] encrypted, out byte[] iv);

            //var message = new AgentMessage
            //{
            //    AgentID = Request.AgentID,
            //    Data = encrypted,
            //    IV = iv
            //};

            //SendAgentMessage(message);

            //var task = $"{Request.Module} {Request.Command}";
            //OnAgentEvent?.Invoke(this, new AgentEvent(Request.AgentID, AgentEvent.EventType.CommandRequest, task, Nick));
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

        async void AgentController_OnAgentEvent(object sender, AgentEvent e)
        {
            AgentEvents.Add(e);
            await HubContext.Clients.All.SendAsync("AgentEvent", e);
        }
    }
}