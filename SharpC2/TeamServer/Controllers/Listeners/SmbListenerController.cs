using Serilog;

using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamServer.Controllers
{
    public class SmbListenerController
    {
        private ListenerControllerBase ControllerBase { get; set; }
        public List<ListenerSmb> SmbListeners { get; private set; } = new List<ListenerSmb>();

        private event EventHandler<ServerEvent> OnServerEvent;

        public SmbListenerController(ListenerControllerBase controllerBase)
        {
            ControllerBase = controllerBase;
            OnServerEvent += ControllerBase.ServerController.ServerEventHandler;
        }

        public ListenerSmb StartSmbListener(NewSmbListenerRequest request)
        {
            var listener = new ListenerSmb
            {
                ListenerName = request.Name,
                PipeName = request.PipeName,
                Type = ListenerType.SMB
            };

            SmbListeners.Add(listener);

            return listener;
        }

        public List<ListenerSmb> GetSmbListeners()
        {
            return SmbListeners;
        }

        public bool StopSmbListener(string listenerName, string user)
        {
            var listener = SmbListeners.FirstOrDefault(l => l.ListenerName.Equals(listenerName, StringComparison.OrdinalIgnoreCase));

            if (listener != null)
            {
                OnServerEvent?.Invoke(this, new ServerEvent(ServerEventType.ListenerStopped, listenerName));
                Log.Logger.Information("LISTENER {Event} {ListenerName} {Nick}", ServerEventType.ListenerStopped.ToString(), listenerName, user);
            }

            return SmbListeners.Remove(listener);
        }
    }
}
