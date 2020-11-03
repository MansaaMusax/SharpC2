using Client.Commands;
using Client.Services;
using Newtonsoft.Json;
using Shared.Models;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ListenerViewModel : BaseViewModel
    {
        readonly MainViewModel MainViewModel;

        public ObservableCollection<Listener> Listeners { get; set; } = new ObservableCollection<Listener>();
        public Listener SelectedListener { get; set; }
        
        public ICommand NewListener { get; }
        public ICommand RemoveListener { get; }

        public ListenerViewModel(MainViewModel MainViewModel)
        {
            this.MainViewModel = MainViewModel;

            SignalR.ServerEventReceived += SignalR_ServerEventReceived;

            //SignalR.NewHttpListenerReceived += SignalR_NewListenerReceived;
            //SignalR.NewTcpListenerReceived += SignalR_NewListenerReceived;
            //SignalR.NewSmbListenerReceived += SignalR_NewListenerReceived;

            //SignalR.RemoveListenerReceived += SignalR_RemoveListenerReceived;

            NewListener = new OpenWindowCommand(WindowType.NewListener);
            RemoveListener = new StopListenerCommand(this);

            DetachTab = new DetachTabCommand("Listeners", MainViewModel);
            CloseTab = new CloseTabCommand("Listeners", MainViewModel);
            RenameTab = new RenameTabCommand("Listerners", MainViewModel);

            GetActiveListeners();
        }

        private void SignalR_ServerEventReceived(ServerEvent ev)
        {
            Listener listener;

            switch (ev.Type)
            {
                case ServerEvent.EventType.ListenerStarted:

                    listener = JsonConvert.DeserializeObject<Listener>(ev.Data.ToString());

                    switch (listener.Type)
                    {
                        case Listener.ListenerType.HTTP:
                            listener = JsonConvert.DeserializeObject<ListenerHTTP>(ev.Data.ToString());
                            break;
                        case Listener.ListenerType.TCP:
                            listener = JsonConvert.DeserializeObject<ListenerTCP>(ev.Data.ToString());
                            break;
                        case Listener.ListenerType.SMB:
                            listener = JsonConvert.DeserializeObject<ListenerSMB>(ev.Data.ToString());
                            break;
                        default:
                            break;
                    }

                    Listeners.Add(listener);
                    break;

                case ServerEvent.EventType.ListenerStopped:

                    var name = ev.Data.ToString();
                    listener = Listeners.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    Listeners.Remove(listener);
                    
                    break;
                
                default:
                    break;
            }
        }

        async void GetActiveListeners()
        {
            var listeners = await SharpC2API.Listeners.GetAllListeners();

            if (listeners != null)
            {
                foreach (var listener in listeners)
                {
                    Listeners.Add(listener);
                }
            }
        }
    }
}