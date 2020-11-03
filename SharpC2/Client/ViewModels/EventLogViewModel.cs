using Client.API;
using Client.Commands;
using Client.Services;

using Shared.Models;

using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    class EventLogViewModel : BaseViewModel
    {
        private readonly SignalR SignalR;

        public ObservableCollection<string> Events { get; set; } = new ObservableCollection<string>();

        public EventLogViewModel(MainViewModel mainViewModel, SignalR signalR)
        {
            SignalR = signalR;

            SignalR.NewServerEventReceived += SignalR_NewServerEventReceived;
            SignalR.NewAgentEvenReceived += SignalR_NewAgentEvenReceived;

            CloseTab = new CloseTabCommand("Event Log", mainViewModel);
            DetachTab = new DetachTabCommand("Event Log", mainViewModel);
            RenameTab = new RenameTabCommand("Event Log", mainViewModel);

            GetServerEventData();
        }

        private void SignalR_NewAgentEvenReceived(AgentEvent ev)
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

        private void SignalR_NewServerEventReceived(ServerEvent ev)
        {
            AddEvent(ev);
        }

        private async void GetServerEventData()
        {
            var serverEvents = await ServerAPI.GetServerEvents();

            if (serverEvents != null)
            {
                foreach (var ev in serverEvents)
                {
                    AddEvent(ev);
                }
            }
        }

        private void AddEvent(ServerEvent ev)
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