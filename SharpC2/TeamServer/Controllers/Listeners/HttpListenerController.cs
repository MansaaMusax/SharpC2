using Serilog;

using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using TeamServer.Modules;

namespace TeamServer.Controllers
{
    public class HttpListenerController
    {
        private ListenerControllerBase ControllerBase { get; set; }

        private event EventHandler<ServerEvent> OnServerEvent;

        public HttpListenerController(ListenerControllerBase controllerBase)
        {
            ControllerBase = controllerBase;
            OnServerEvent += ControllerBase.ServerController.ServerEventHandler;
        }

        public List<HttpCommModule> HttpListeners { get; private set; } = new List<HttpCommModule>();

        public ListenerHttp StartHttpListener(NewHttpListenerRequest request, AgentController agentController, CryptoController cryptoController, string user)
        {
            var listener = new ListenerHttp
            {
                ListenerId = request.Name,
                Type = ListenerType.HTTP,
                BindPort = request.BindPort,
                ConnectAddress = request.ConnectAddress,
                ConnectPort = request.ConnectPort
            };

            var module = new HttpCommModule
            {
                Listener = listener
            };

            HttpListeners.Add(module);

            module.Init(agentController, cryptoController);
            module.Start();

            OnServerEvent?.Invoke(this, new ServerEvent(ServerEventType.ListenerStarted, request.Name));
            Log.Logger.Information("LISTENER {Event} {ListenerName} {Nick}", ServerEventType.ListenerStarted.ToString(), request.Name, user);

            return listener;
        }

        public List<ListenerHttp> GetHttpListeners()
        {
            var listeners = new List<ListenerHttp>();

            foreach (var module in HttpListeners)
            {
                listeners.Add(module.Listener);
            }

            return listeners;
        }

        public bool StopHttpListener(string listenerId, string user)
        {
            var module = HttpListeners.Where(m => m.Listener.ListenerId.Equals(listenerId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            
            if (module != null)
            {
                module.Stop();

                OnServerEvent?.Invoke(this, new ServerEvent(ServerEventType.ListenerStopped, listenerId));
                Log.Logger.Information("LISTENER {Event} {ListenerName} {Nick}", ServerEventType.ListenerStopped.ToString(), listenerId, user);
            }

            return HttpListeners.Remove(module);
        }

        public IOrderedEnumerable<WebLog> GetWebLogs()
        {
            var result = new List<WebLog>();
            var webLogs = HttpListeners.Select(l => l.WebLogs).ToList();
            
            foreach (var webLog in webLogs)
            {
                result.AddRange(webLog);
            }

            return result.OrderBy(log => log.Date);
        }
    }
}