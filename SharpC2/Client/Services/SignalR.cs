using Client.ViewModels;
using Client.Views;

using Microsoft.AspNetCore.SignalR.Client;
using Shared.Models;
using System;
using System.Threading.Tasks;

namespace Client.Services
{
    public class SignalR
    {
        private readonly HubConnection Connection;

        public event Action<UserMessage> ChatMessageReceived;
        public event Action<ServerEvent> NewServerEventReceived;
        public event Action<AgentEvent> NewAgentEvenReceived;
        public event Action<WebLog> NewWebEvenReceived;

        public event Action<ListenerHTTP> NewHttpListenerReceived;
        public event Action<ListenerTCP> NewTcpListenerReceived;
        public event Action<ListenerSMB> NewSmbListenerReceived;

        public event Action<string> RemoveListenerReceived;

        public SignalR(HubConnection connection)
        {
            Connection = connection;

            Connection.On<UserMessage>("MessageIn", (msg) => ChatMessageReceived?.Invoke(msg));
            Connection.On<ServerEvent>("NewServerEvent", (e) => NewServerEventReceived?.Invoke(e));
            Connection.On<AgentEvent>("NewAgentEvent", (e) => NewAgentEvenReceived?.Invoke(e));
            Connection.On<WebLog>("NewWebEvent", (e) => NewWebEvenReceived?.Invoke(e));
            
            Connection.On<ListenerHTTP>("NewHttpListener", (l) => NewHttpListenerReceived?.Invoke(l));
            Connection.On<ListenerTCP>("NewTcpListener", (l) => NewTcpListenerReceived?.Invoke(l));
            Connection.On<ListenerSMB>("NewSmbListener", (l) => NewSmbListenerReceived?.Invoke(l));

            Connection.On<string>("RemoveListener", (l) => RemoveListenerReceived?.Invoke(l));

            Connect().ContinueWith((task) =>
            {

                if (task.Exception != null)
                {
                    var window = new ErrorView
                    {
                        DataContext = new ErrorViewModel
                        {
                            Error = task.Exception.Message
                        }
                    };

                    window.Show();
                }

            });
        }

        public async Task Connect()
        {
            await Connection.StartAsync();
        }

        public async Task SendChatMessage(UserMessage message)
        {
            await Connection.SendAsync("MessageOut", message);
        }
    }
}