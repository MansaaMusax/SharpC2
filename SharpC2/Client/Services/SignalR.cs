using Microsoft.AspNetCore.SignalR.Client;

using Shared.Models;

using System;
using System.Threading.Tasks;

namespace Client.Services
{
    public class SignalR
    {
        static HubConnection Connection;

        public static event Action<UserMessage> ChatMessageReceived;

        public static event Action<ServerEvent> ServerEventReceived;
        public static event Action<AgentEvent> AgentEventReceived;

        //public event Action<WebLog> NewWebEvenReceived;

        //public event Action<ListenerHTTP> NewHttpListenerReceived;
        //public event Action<ListenerTCP> NewTcpListenerReceived;
        //public event Action<ListenerSMB> NewSmbListenerReceived;

        public event Action<string> RemoveListenerReceived;

        public SignalR(HubConnection connection)
        {
            Connection = connection;

            Connection.On<UserMessage>("RecvChatMessage", (msg) => ChatMessageReceived?.Invoke(msg));

            Connection.On<ServerEvent>("ServerEvent", (e) => ServerEventReceived?.Invoke(e));
            Connection.On<AgentEvent>("AgentEvent", (e) => AgentEventReceived?.Invoke(e));
            
            
            //Connection.On<WebLog>("NewWebEvent", (e) => NewWebEvenReceived?.Invoke(e));
            
            //Connection.On<ListenerHTTP>("NewHttpListener", (l) => NewHttpListenerReceived?.Invoke(l));
            //Connection.On<ListenerTCP>("NewTcpListener", (l) => NewTcpListenerReceived?.Invoke(l));
            //Connection.On<ListenerSMB>("NewSmbListener", (l) => NewSmbListenerReceived?.Invoke(l));

            //Connection.On<string>("RemoveListener", (l) => RemoveListenerReceived?.Invoke(l));

            Connect().ContinueWith((task) =>
            {
                if (task.Exception != null)
                {
                    var message = task.Exception.Message;
                }
            });
        }

        public async Task Connect()
        {
            await Connection.StartAsync();
        }

        public static async Task SendChatMessage(string Message)
        {
            await Connection.SendAsync("SendChatMessage", Message);
        }
    }
}