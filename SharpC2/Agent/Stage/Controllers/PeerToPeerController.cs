using Agent.Interfaces;

using Shared.Models;

using System.Collections.Generic;
using System.Linq;
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
                    ICommModule[] modules;

                    lock (ConnectedAgents)
                    {
                        modules = ConnectedAgents.Values.ToArray();
                    }

                    foreach (var module in modules)
                    {
                        if (module.RecvData(out AgentMessage Message))
                        {
                            if (Message != null)
                            {
                                // These messages come from connected P2P Agents.
                                // We either need to forward it on to the TS
                                //    or pass it to ourselves (e.g. if it's part of the link process).

                                if (Message.AgentID.Equals(Agent.AgentID))
                                {
                                    Agent.HandleAgentMessage(Message);
                                }
                                else
                                {
                                    Agent.SendMessage(Message);
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

            var task = Agent.Crypto.Encrypt(new AgentTask
            {
                Module = "Link",
                Command = "Link0Request",
                Parameters = new Dictionary<string, object>
                {
                    { "Placeholder", placeholder },
                    { "ParentAgentID", Agent.AgentID }
                }
            },
            out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                Data = task,
                IV = iv
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

        public void ForwardMessage(AgentMessage Message)
        {
            ICommModule[] modules;

            lock (ConnectedAgents)
            {
                modules = ConnectedAgents.Values.ToArray();
            }

            foreach (var module in modules)
            {
                module.SendData(Message);
            }
        }
    }
}