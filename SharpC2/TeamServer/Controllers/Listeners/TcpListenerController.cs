using Serilog;

using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamServer.Controllers
{
    public class TcpListenerController
    {
        private ListenerControllerBase ControllerBase { get; set; }
        public List<ListenerTcp> TcpListeners { get; private set; } = new List<ListenerTcp>();

        private event EventHandler<ServerEvent> OnServerEvent;

        public TcpListenerController(ListenerControllerBase controllerBase)
        {
            ControllerBase = controllerBase;
            OnServerEvent += ControllerBase.ServerController.ServerEventHandler;
        }

        public ListenerTcp StartTcpListener(NewTcpListenerRequest request)
        {
            var listener = new ListenerTcp
            {
                ListenerId = request.Name,
                Type = ListenerType.TCP,
                BindAddress = request.BindAddress,
                BindPort = request.BindPort
            };

            TcpListeners.Add(listener);

            return listener;
        }

        public List<ListenerTcp> GetTcpListeners()
        {
            return TcpListeners;
        }

        public bool StopTcpListener(string listenerId, string user)
        {
            var listener = TcpListeners.FirstOrDefault(l => l.ListenerId.Equals(listenerId));

            if (listener != null)
            {
                OnServerEvent?.Invoke(this, new ServerEvent(ServerEventType.ListenerStopped, listenerId));
                Log.Logger.Information("LISTENER {Event} {ListenerName} {Nick}", ServerEventType.ListenerStopped.ToString(), listenerId, user);
            }

            return TcpListeners.Remove(listener);
        }
    }
}