using Client.Commands;
using Client.Services;

using Newtonsoft.Json;

using Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class EventLogViewModel : BaseViewModel
    {
        public ObservableCollection<string> Events { get; set; } = new ObservableCollection<string>();

        private string _chatMessage;
        public string ChatMessage
        {
            get { return _chatMessage; }
            set { _chatMessage = value; NotifyPropertyChanged(nameof(ChatMessage)); }
        }

        public ICommand SendMessageCommand { get; }

        public EventLogViewModel(MainViewModel mainViewModel)
        {
            SignalR.ChatMessageReceived += SignalR_ChatMessageReceived;

            SignalR.ServerEventReceived += SignalR_ServerEventReceived;
            SignalR.AgentEventReceived += SignalR_AgentEventReceived;

            CloseTab = new CloseTabCommand("Event Log", mainViewModel);
            DetachTab = new DetachTabCommand("Event Log", mainViewModel);
            RenameTab = new RenameTabCommand("Event Log", mainViewModel);

            SendMessageCommand = new SendMessageCommand(this);

            GetServerEventData();
        }

        void SignalR_ChatMessageReceived(UserMessage Message)
        {
            Events.Insert(0, $"<{Message.Nick}>     {Message.Message}");
        }

        void SignalR_AgentEventReceived(AgentEvent ev)
        {
            switch (ev.Type)
            {
                case AgentEvent.EventType.InitialAgent:
                    var agent = JsonConvert.DeserializeObject<AgentMetadata>(ev.Data.ToString());
                    Events.Insert(0, $"[{ev.Date}]     Initial checkin from {agent.Identity}@{agent.Hostname}");
                    break;
                default:
                    break;
            }
        }

        void SignalR_ServerEventReceived(ServerEvent ev)
        {
            AddServerEvent(ev);
        }

        async void GetServerEventData()
        {
            var serverEvents = await SharpC2API.Server.GetServerEvents();

            if (serverEvents != null)
            {
                foreach (var ev in serverEvents)
                {
                    AddServerEvent(ev);
                }
            }
        }

        void AddServerEvent(ServerEvent ev)
        {
            switch (ev.Type)
            {
                case ServerEvent.EventType.UserLogon:

                    var status = Enum.Parse<AuthResult.AuthStatus>(ev.Data.ToString());

                    switch (status)
                    {
                        case AuthResult.AuthStatus.LogonSuccess:
                            Events.Insert(0, $"[{ev.Date}]     {ev.Nick} has joined. Say hi!");
                            break;
                        case AuthResult.AuthStatus.NickInUse:
                            Events.Insert(0, $"[{ev.Date}]     {ev.Nick} tried to join again.");
                            break;
                        case AuthResult.AuthStatus.BadPassword:
                            Events.Insert(0, $"[{ev.Date}]     {ev.Nick} got the password wrong. Duh!");
                            break;
                        default:
                            break;
                    }

                    break;

                case ServerEvent.EventType.UserLogoff:
                    Events.Insert(0, $"[{ev.Date}]     {ev.Nick} has left. Say goodbye!");
                    break;
                case ServerEvent.EventType.ListenerStarted:
                    var listener = JsonConvert.DeserializeObject<Listener>(ev.Data.ToString());
                    Events.Insert(0, $"[{ev.Date}]     {ev.Nick} has started listener {listener.Name}.");
                    break;
                case ServerEvent.EventType.ListenerStopped:
                    Events.Insert(0, $"[{ev.Date}]     {ev.Nick} has stopped listener {ev.Data}.");
                    break;
                case ServerEvent.EventType.ServerModuleRegistered:
                    Events.Insert(0, $"[{ev.Date}]     {ev.Data} module has started.");
                    break;
                default:
                    break;
            }
        }
    }
}