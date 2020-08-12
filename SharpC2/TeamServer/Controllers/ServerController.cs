using Common.Models;
using Serilog;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Controllers
{
    public class ServerController
    {
        public ModuleStatus ServerStatus { get; private set; }
        public ClientController ClientController { get; private set; }
        public ListenerControllerBase ListenerController { get; private set; }
        public AgentController AgentController { get; private set; }
        public CryptoController CryptoController { get; private set; }
        private List<ServerModule> ServerModules { get; set; } = new List<ServerModule>();
        public List<ServerEvent> ServerEvents { get; private set; } = new List<ServerEvent>();
        private List<string> IdempotencyKeys { get; set; } = new List<string>();

        private event EventHandler<ServerEvent> OnServerEvent;

        public delegate void OnServerCommand(AgentMetadata metadata, C2Data c2Data);

        public ServerController()
        {
            ServerStatus = ModuleStatus.Starting;

            ClientController = new ClientController(this);
            CryptoController = new CryptoController();
            AgentController = new AgentController(this, CryptoController);
            ListenerController = new ListenerControllerBase(this, AgentController, CryptoController);

            OnServerEvent += ServerEventHandler;
        }

        public void ServerEventHandler(object sender, ServerEvent e)
        {
            ServerEvents.Add(e);
        }

        public void RegisterServerModule(IServerModule module)
        {
            module.Init(this, AgentController);
            var info = module.GetModuleInfo();
            ServerModules.Add(info);
            OnServerEvent?.Invoke(this, new ServerEvent(ServerEventType.ServerModuleRegistered, info.Name));
            Log.Logger.Information("SERVER {Event} {ModuleName}", ServerEventType.ServerModuleRegistered.ToString(), info.Name);
        }

        public void Start()
        {
            ServerStatus = ModuleStatus.Running;


            Task.Factory.StartNew(delegate ()
            {
                while (ServerStatus == ModuleStatus.Running)
                {
                    var commModules = ListenerController.HttpListenerController.HttpListeners.ToList();

                    foreach (var commModule in commModules)
                    {
                        if (commModule != null && commModule.RecvData(out Tuple<AgentMetadata, List<AgentMessage>> data))
                        {
                            foreach (var message in data.Item2)
                            {
                                if (!IdempotencyKeys.Contains(message.IdempotencyKey))
                                {
                                    IdempotencyKeys.Add(message.IdempotencyKey);

                                    var checkinCallback = ServerModules
                                        .Where(m => m.Name.Equals("Core", StringComparison.OrdinalIgnoreCase))
                                        .Select(m => m.ServerCommands).FirstOrDefault()
                                        .Where(c => c.Name.Equals("AgentCheckIn", StringComparison.OrdinalIgnoreCase))
                                        .Select(c => c.CallBack).FirstOrDefault();

                                    // checkin the parent agent
                                    checkinCallback?.Invoke(data.Item1, null);

                                    if (!message.Data.Command.Equals("AgentCheckIn", StringComparison.OrdinalIgnoreCase))
                                    {
                                        HandleC2Data(message.Metadata, message.Data);
                                    }

                                    // checkin the p2p agent
                                    if (!string.IsNullOrEmpty(message.Metadata.ParentAgentID))
                                    {
                                        checkinCallback?.Invoke(message.Metadata, message.Data);
                                    }
                                }
                                else
                                {
                                    OnServerEvent?.Invoke(this, new ServerEvent(ServerEventType.IdempotencyKeyError, $"Duplicate Idempotency Key received for {message.Metadata.AgentID}"));
                                }
                            }
                        }
                    }
                }
            });
        }

        private void HandleC2Data(AgentMetadata metadata, C2Data c2Data)
        {
            OnServerCommand CallBack;

            try
            {
                CallBack = ServerModules
                .Where(m => m.Name.Equals(c2Data.Module, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.ServerCommands).FirstOrDefault()
                .Where(c => c.Name.Equals(c2Data.Command, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.CallBack).FirstOrDefault();
            }
            catch
            {
                return;
            }

            CallBack?.Invoke(metadata, c2Data);
        }

        public void Stop()
        {
            ServerStatus = ModuleStatus.Stopped;
        }
    }
}