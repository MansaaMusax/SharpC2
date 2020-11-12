using Microsoft.AspNetCore.SignalR;

using Shared.Models;
using Shared.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        event EventHandler<ServerEvent> OnServerEvent;

        List<ServerModule> ServerModules = new List<ServerModule>();

        IHubContext<MessageHub> HubContext;
        List<ServerEvent> ServerEvents = new List<ServerEvent>();

        Dictionary<string, List<HTTPChunk>> HTTPChunks = new Dictionary<string, List<HTTPChunk>>();

        public delegate void ServerCommand(string AgentID, C2Data C2Data);

        public ServerController(UserController Users, IHubContext<MessageHub> HubContext)
        {
            this.Users = Users;
            this.HubContext = HubContext;

            OnServerEvent += ServerController_OnServerEvent;

            Crypto = new CryptoController();
            Agent = new AgentController(Crypto, HubContext);
            Listeners = new ListenerController(this, Agent);
        }

        public void ServerController_OnServerEvent(object sender, ServerEvent e)
        {
            ServerEvents.Add(e);
            HubContext.Clients.All.SendAsync("ServerEvent", e);
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
                        if (listener != null && listener.RecvData(out HTTPChunk Chunk))
                        {
                            if (Chunk == null) { return; }

                            if (!HTTPChunks.ContainsKey(Chunk.AgentID))
                            {
                                HTTPChunks.Add(Chunk.AgentID, new List<HTTPChunk>());
                            }

                            HTTPChunks[Chunk.AgentID].Add(Chunk);

                            if (Chunk.Final)
                            {
                                var allChunks = HTTPChunks[Chunk.AgentID].Where(c => c.ChunkID.Equals(Chunk.ChunkID, StringComparison.OrdinalIgnoreCase)).ToList();

                                var final = new StringBuilder();

                                foreach (var chunk in allChunks)
                                {
                                    final.Append(chunk.Data);
                                    HTTPChunks[Chunk.AgentID].Remove(chunk);
                                }

                                AgentMessage message;

                                try
                                {
                                    message = Utilities.DeserialiseData<AgentMessage>(Convert.FromBase64String(final.ToString()));
                                }
                                catch
                                {
                                    message = null;
                                }
                                
                                if (message != null)
                                {
                                    HandleC2Data(message);
                                }
                            }
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

            var c2Data = Crypto.Decrypt<C2Data>(Message.Data, Message.IV);

            var callback = ServerModules.FirstOrDefault(m => m.Name.Equals(c2Data.Module, StringComparison.OrdinalIgnoreCase)).Commands
                .FirstOrDefault(c => c.Name.Equals(c2Data.Command, StringComparison.OrdinalIgnoreCase))
                .Delegate;

            callback?.Invoke(Message.AgentID, c2Data);
        }

        // Server

        public IEnumerable<ServerEvent> GetServerEvents()
        {
            return ServerEvents;
        }

        // Users

        public AuthResult UserLogon(AuthRequest Request)
        {
            var logon = Users.UserLogon(Request);

            OnServerEvent?.Invoke(this, new ServerEvent(ServerEvent.EventType.UserLogon, logon.Status, Request.Nick));

            return logon;
        }

        public bool UserLogoff(string Nick)
        {
            var logout = Users.RemoveUser(Nick);

            if (logout)
            {
                OnServerEvent.Invoke(this, new ServerEvent(ServerEvent.EventType.UserLogoff, null, Nick));
            }

            return logout;
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

        public bool StartListener(ListenerRequest Request, string Nick, out Listener Listener)
        {
            if (!Listeners.ValidRequest(Request))
            {
                Listener = null;
                return false;
            }
            else
            {
                Listener = Listeners.NewListener(Request);
                OnServerEvent?.Invoke(this, new ServerEvent(ServerEvent.EventType.ListenerStarted, Listener, Nick));
                return true;
            }
        }

        public bool StopListener(string ListenerName, string Nick)
        {
            var stopped = Listeners.StopListener(ListenerName);

            if (stopped)
            {
                OnServerEvent?.Invoke(this, new ServerEvent(ServerEvent.EventType.ListenerStopped, ListenerName, Nick));
            }

            return stopped;
        }

        // Payloads

        public bool GenerateStager(StagerRequest Request, out byte[] Stager)
        {
            var listener = Listeners.GetListener(Request.Listener);
            var payload = new Payload(listener);
            var stager = payload.GenerateStager(Request);

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