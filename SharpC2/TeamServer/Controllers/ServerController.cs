using Microsoft.AspNetCore.SignalR;

using Shared.Models;
using Shared.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Controllers
{
    public class ServerController
    {
        UserController Users;
        ListenerController Listeners;
        CryptoController Crypto;
        AgentController Agent;

        IHubContext<MessageHub> HubContext;

        List<ServerModule> ServerModules = new List<ServerModule>();

        public delegate void ServerCommand(string AgentID, C2Data C2Data);

        public ServerController(UserController Users, IHubContext<MessageHub> HubContext)
        {
            this.Users = Users;
            this.HubContext = HubContext;

            Crypto = new CryptoController();
            Agent = new AgentController(Crypto, HubContext);
            Listeners = new ListenerController(Agent);
        }

        public void RegisterServerModule(IServerModule Module)
        {
            Module.Init(this, Agent);
            ServerModules.Add(Module.GetModuleInfo());
        }

        public void Start()
        {
            Task.Factory.StartNew(delegate ()
            {
                while (true)
                {
                    var listeners = Listeners.HTTPListeners.Values.ToArray();

                    foreach (var listener in listeners)
                    {
                        if (listener != null && listener.RecvData(out AgentMessage Message))
                        {
                            HandleC2Data(Message);
                        }
                    }
                }
            });
        }

        void HandleC2Data(AgentMessage Message)
        {
            if (Message == null || Message.Data == null)  // I don't know why this happens
            {
                return;
            }

            C2Data c2Data;

            if (Message.IV == null)
            {
                try
                {
                    c2Data = Utilities.DeserialiseData<C2Data>(Message.Data);
                }
                catch
                {
                    c2Data = Crypto.Decrypt(Message.Data);
                }
                
            }
            else
            {
                var sessionKey = Crypto.GetSessionKey(Message.AgentID);
                c2Data = Utilities.DecryptData<C2Data>(Message.Data, sessionKey, Message.IV);
            }

            var callback = ServerModules.FirstOrDefault(m => m.Name.Equals(c2Data.Module, StringComparison.OrdinalIgnoreCase)).Commands
                .FirstOrDefault(c => c.Name.Equals(c2Data.Command, StringComparison.OrdinalIgnoreCase))
                .Delegate;

            callback?.Invoke(Message.AgentID, c2Data);
        }

        // Users

        public AuthResult UserLogon(AuthRequest Request)
        {
            return Users.UserLogon(Request);
        }

        public bool UserLogoff(string Nick)
        {
            return Users.RemoveUser(Nick);
        }

        // Listeners

        public IEnumerable<Listener> GetListeners(Listener.ListenerType Type)
        {
            switch (Type)
            {
                case Listener.ListenerType.HTTP:
                    return Listeners.HTTPListeners.Keys;

                case Listener.ListenerType.TCP:
                    return Listeners.TCPListeners;

                case Listener.ListenerType.SMB:
                    return Listeners.SMBListeners;

                default:
                    return new List<Listener>();
            }
        }

        public bool StartListener(ListenerRequest Request, out Listener Listener)
        {
            if (!Listeners.ValidRequest(Request))
            {
                Listener = null;
                return false;
            }
            else
            {
                Listener = Listeners.NewListener(Request);
                return true;
            }
        }

        public bool StopListener(string Name)
        {
            return Listeners.StopListener(Name);
        }

        // Payloads

        public bool GenerateStager(StagerRequest Request, out byte[] Stager)
        {
            var listener = Listeners.GetListener(Request.Listener);
            var payload = new Payload(listener, Crypto);
            var stager = payload.GenerateStager();

            if (stager != null)
            {
                Stager = stager;
                return true;
            }
            else
            {
                Stager = null;
                return false;
            }
        }

        // Agents

        public IEnumerable<AgentMetadata> GetAgents()
        {
            return Agent.GetAgents();
        }

        public void SendAgentCommand(AgentCommandRequest Request, string Nick)
        {
            Agent.SendAgentCommand(Request, Nick);
        }

        public IEnumerable<AgentEvent> GetAgentEvents(string AgentID, DateTime Date)
        {
            var events = Agent.GetEventsSince(Date);
            return events.Where(a => a.AgentID.Equals(AgentID, StringComparison.OrdinalIgnoreCase));
        }
    }
}