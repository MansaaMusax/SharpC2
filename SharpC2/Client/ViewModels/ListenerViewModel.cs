using Client.Commands;
using Client.Services;

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

        void SignalR_RemoveListenerReceived(string name)
        {
            var listener = Listeners.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (listener != null)
            {
                Listeners.Remove(listener);
            }
        }

        void SignalR_NewListenerReceived(ListenerHTTP l)
        {
            Listeners.Add(l);
        }

        void SignalR_NewListenerReceived(ListenerTCP l)
        {
            Listeners.Add(l);
        }

        void SignalR_NewListenerReceived(ListenerSMB l)
        {
            Listeners.Add(l);
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