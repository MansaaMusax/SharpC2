using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using TeamServer.CommModules;

namespace TeamServer.Controllers
{
    public class ListenerController
    {
        AgentController Agent;

        public Dictionary<ListenerHTTP, HTTPCommModule> HTTPListeners = new Dictionary<ListenerHTTP, HTTPCommModule>();
        
        public List<ListenerTCP> TCPListeners = new List<ListenerTCP>();
        public List<ListenerSMB> SMBListeners = new List<ListenerSMB>();

        public ListenerController(AgentController Agent)
        {
            this.Agent = Agent;
        }

        public bool ValidRequest(ListenerRequest Request)
        {
            if (NameExists(Request.Name)) { return false; }
            if (BindPortExits(Request.BindPort, Request.Type)) { return false; }
            if (PipeNameExists(Request.PipeName)) { return false; }

            return true;
        }

        public Listener NewListener(ListenerRequest Request)
        {
            switch (Request.Type)
            {
                case Listener.ListenerType.HTTP:
                    return NewHTTPListener(Request);

                case Listener.ListenerType.TCP:
                    return NewTCPListener(Request);

                case Listener.ListenerType.SMB:
                    return NewSMBListener(Request);

                default:
                    return new Listener();
            }
        }

        public Listener GetListener(string Name)
        {
            var listeners = GetListeners();
            return listeners.FirstOrDefault(l => l.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));
        }

        public bool StopListener(string Name)
        {
            var listeners = GetListeners();
            var listener = listeners.FirstOrDefault(l => l.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));

            if (listener != null)
            {
                switch (listener.Type)
                {
                    case Listener.ListenerType.HTTP:
                        HTTPListeners[listener as ListenerHTTP].Stop();
                        return HTTPListeners.Remove(listener as ListenerHTTP);

                    case Listener.ListenerType.TCP:
                        return TCPListeners.Remove(listener as ListenerTCP);

                    case Listener.ListenerType.SMB:
                        return SMBListeners.Remove(listener as ListenerSMB);

                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        ListenerHTTP NewHTTPListener(ListenerRequest Request)
        {
            var listener = new ListenerHTTP
            {
                Name = Request.Name,
                BindPort = Request.BindPort,
                ConnectAddress = Request.ConnectAddress,
                ConnectPort = Request.ConnectPort
            };

            var module = new HTTPCommModule(listener);
            module.Init(Agent);
            module.Start();

            HTTPListeners.Add(listener, module);

            return listener;
        }

        ListenerTCP NewTCPListener(ListenerRequest Request)
        {
            var listener = new ListenerTCP
            {
                Name = Request.Name,
                BindAddress = Request.BindAddress,
                BindPort = Request.BindPort,
            };

            TCPListeners.Add(listener);

            return listener;
        }

        ListenerSMB NewSMBListener(ListenerRequest Request)
        {
            var listener = new ListenerSMB
            {
                Name = Request.Name,
                PipeName = Request.PipeName
            };

            SMBListeners.Add(listener);

            return listener;
        }

        List<Listener> GetListeners()
        {
            var newList = new List<Listener>();

            newList.AddRange(HTTPListeners.Keys);
            newList.AddRange(TCPListeners);
            newList.AddRange(SMBListeners);

            return newList;
        }

        bool NameExists(string Name)
        {
            var listeners = GetListeners();
            return listeners.Any(l => l.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));
        }

        bool BindPortExits(int BindPort, Listener.ListenerType Type)
        {
            switch (Type)
            {
                case Listener.ListenerType.HTTP:
                    return HTTPListeners.Keys.Any(l => l.BindPort == BindPort);

                case Listener.ListenerType.TCP:
                    return TCPListeners.Any(l => l.BindPort == BindPort);

                default:
                    return false;
            }
        }

        bool PipeNameExists(string PipeName)
        {
            return SMBListeners.Any(l => l.PipeName.Equals(PipeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}