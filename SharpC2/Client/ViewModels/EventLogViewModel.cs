using Client.Commands;
using Client.Services;

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

            //SignalR.NewServerEventReceived += SignalR_NewServerEventReceived;
            //SignalR.NewAgentEvenReceived += SignalR_NewAgentEvenReceived;

            CloseTab = new CloseTabCommand("Event Log", mainViewModel);
            DetachTab = new DetachTabCommand("Event Log", mainViewModel);
            RenameTab = new RenameTabCommand("Event Log", mainViewModel);

            SendMessageCommand = new SendMessageCommand(this);

            GetServerEventData();
        }

        void SignalR_ChatMessageReceived(UserMessage msg)
        {
            Events.Insert(0, $"<{msg.Nick}>     {msg.Message}");
        }

        void SignalR_NewAgentEvenReceived(AgentEvent ev)
        {
            switch (ev.Type)
            {
                case AgentEvent.EventType.InitialAgent:
                    Events.Insert(0, $"[{ev.Date}]     Initial checkin from {ev.AgentID}");
                    break;
                default:
                    break;
            }
        }

        void SignalR_NewServerEventReceived(ServerEvent ev)
        {
            AddEvent(ev);
        }

        async void GetServerEventData()
        {
            var serverEvents = await SharpC2API.Server.GetServerEvents();

            if (serverEvents != null)
            {
                foreach (var ev in serverEvents)
                {
                    AddEvent(ev);
                }
            }
        }

        void AddEvent(ServerEvent ev)
        {
            switch (ev.Type)
            {
                case ServerEvent.EventType.UserLogon:
                    Events.Insert(0, $"[{ev.Date}]     {ev.Nick} has joined. Say hi!");
                    break;
                case ServerEvent.EventType.UserLogoff:
                    Events.Insert(0, $"[{ev.Date}]     {ev.Nick} has left. Goodbye!");
                    break;
                case ServerEvent.EventType.FailedAuth:
                    Events.Insert(0, $"[{ev.Date}]     {ev.Nick} has failed to login ({ev.Data}).");
                    break;
                case ServerEvent.EventType.ListenerStarted:
                    Events.Insert(0, $"[{ev.Date}]     {ev.Nick} has started listener {ev.Data}.");
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