using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;

using TeamServer.Modules;

namespace TeamServer.Controllers
{
    public class ListenerController
    {
        public ServerController ServerController { get; private set; }
        private AgentController AgentController { get; set; }
        private CryptoController CryptoController { get; set; }

        public List<HttpCommModule> HttpListeners { get; private set; } = new List<HttpCommModule>();
        public List<ListenerTcp> TcpListeners { get; private set; } = new List<ListenerTcp>();
        public List<ListenerSmb> SmbListeners { get; private set; } = new List<ListenerSmb>();

        private event EventHandler<ServerEvent> ServerEvent;

        public ListenerController(ServerController server, AgentController agent, CryptoController crypto)
        {
            ServerController = server;
            AgentController = agent;
            CryptoController = crypto;

            ServerEvent += ServerController.ServerEventHandler;
        }

        public Listener StartListener(NewListenerRequest request, string nick)
        {
            Listener listener = null;

            switch (request.Type)
            {
                case ListenerType.HTTP:
                    listener = StartHttpListener(request);
                    ServerController.HubContext.Clients.All.SendAsync("NewHttpListener", listener as ListenerHttp);
                    break;
                case ListenerType.TCP:
                    listener = StartTcpListener(request);
                    ServerController.HubContext.Clients.All.SendAsync("NewTcpListener", listener as ListenerTcp);
                    break;
                case ListenerType.SMB:
                    listener = StartSmbListener(request);
                    ServerController.HubContext.Clients.All.SendAsync("NewSmbListener", listener as ListenerSmb);
                    break;
            }

            ServerEvent?.Invoke(this, new ServerEvent(ServerEventType.ListenerStarted, listener.Name, nick));

            return listener;
        }

        private ListenerHttp StartHttpListener(NewListenerRequest request)
        {
            var listener = new ListenerHttp
            {
                Name = request.Name,
                BindPort = request.BindPort,
                ConnectAddress = request.ConnectAddress,
                ConnectPort = request.ConnectPort
            };

            var module = new HttpCommModule { Listener = listener };
            HttpListeners.Add(module);

            module.Init(AgentController, CryptoController);
            module.Start();

            return listener;
        }

        private ListenerTcp StartTcpListener(NewListenerRequest request)
        {
            var listener = new ListenerTcp
            {
                Name = request.Name,
                BindAddress = request.BindAddress,
                BindPort = request.BindPort
            };

            TcpListeners.Add(listener);

            return listener;
        }

        private ListenerSmb StartSmbListener(NewListenerRequest request)
        {
            var listener = new ListenerSmb
            {
                Name = request.Name,
                PipeName = request.PipeName
            };

            SmbListeners.Add(listener);

            return listener;
        }

        public IEnumerable<Listener> GetListeners()
        {
            var listeners = new List<Listener>();

            foreach (var module in HttpListeners)
            {
                listeners.Add(module.Listener);
            }

            listeners.AddRange(TcpListeners);
            listeners.AddRange(SmbListeners);

            return listeners;
        }

        public Listener GetListener(string name)
        {
            return GetListeners().FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
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

        public List<ListenerTcp> GetTcpListeners()
        {
            return TcpListeners;
        }

        public List<ListenerSmb> GetSmbListeners()
        {
            return SmbListeners;
        }

        public bool StopListener(string name, string nick)
        {
            var result = false;
            var listener = GetListeners().FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            switch (listener.Type)
            {
                case ListenerType.HTTP:

                    var module = HttpListeners.FirstOrDefault(l => l.Listener.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (module != null)
                    {
                        module.Stop();
                    }

                    result = HttpListeners.Remove(module);

                    break;

                case ListenerType.TCP:
                    result = TcpListeners.Remove(listener as ListenerTcp);
                    break;

                case ListenerType.SMB:
                    result = SmbListeners.Remove(listener as ListenerSmb);
                    break;

                default:
                    break;
            }

            if (result)
            {
                ServerEvent?.Invoke(this, new ServerEvent(ServerEventType.ListenerStopped, name, nick));
                ServerController.HubContext.Clients.All.SendAsync("RemoveListener", name);
            }

            return result;
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