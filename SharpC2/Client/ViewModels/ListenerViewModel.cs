using Client.API;
using Client.Commands;
using Client.Services;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ListenerViewModel : BaseViewModel
    {
        private readonly MainViewModel MainViewModel;
        private readonly SignalR SignalR;

        public ObservableCollection<Listener> Listeners { get; set; } = new ObservableCollection<Listener>();
        public Listener SelectedListener { get; set; }
        
        public ICommand NewListener { get; }
        public ICommand RemoveListener { get; }

        public ListenerViewModel(MainViewModel mainViewModel, SignalR signalR)
        {
            MainViewModel = mainViewModel;
            SignalR = signalR;

            SignalR.NewHttpListenerReceived += SignalR_NewListenerReceived;
            SignalR.NewTcpListenerReceived += SignalR_NewListenerReceived;
            SignalR.NewSmbListenerReceived += SignalR_NewListenerReceived;

            SignalR.RemoveListenerReceived += SignalR_RemoveListenerReceived;

            NewListener = new OpenWindowCommand(WindowType.NewListener);
            RemoveListener = new StopListenerCommand(this);

            DetachTab = new DetachTabCommand("Listeners", MainViewModel);
            CloseTab = new CloseTabCommand("Listeners", MainViewModel);
            RenameTab = new RenameTabCommand("Listerners", MainViewModel);

            GetActiveListeners();
        }

        private void SignalR_RemoveListenerReceived(string name)
        {
            var listener = Listeners.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (listener != null)
            {
                Listeners.Remove(listener);
            }
        }

        private void SignalR_NewListenerReceived(ListenerHttp l)
        {
            Listeners.Add(l);
        }

        private void SignalR_NewListenerReceived(ListenerTcp l)
        {
            Listeners.Add(l);
        }

        private void SignalR_NewListenerReceived(ListenerSmb l)
        {
            Listeners.Add(l);
        }

        private async void GetActiveListeners()
        {
            var listeners = await ListenerAPI.GetAllListeners();

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