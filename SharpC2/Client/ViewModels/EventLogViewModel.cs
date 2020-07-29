using Client.SharpC2API;
using Client.Views;

using SharpC2.Models;

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.ViewModels
{
    class EventLogViewModel : BaseViewModel
    {
        private MainWindowViewModel MainView { get; set; }

        public ObservableCollection<ServerEvent> EventLogs { get; set; }
        private readonly object _lock = new object();

        private readonly DelegateCommand _detachTab;
        private readonly DelegateCommand _closeTab;

        public ICommand DetachTab => _detachTab;
        public ICommand CloseTab => _closeTab;

        public EventLogViewModel(MainWindowViewModel mainView)
        {
            MainView = mainView;

            EventLogs = new ObservableCollection<ServerEvent>();
            BindingOperations.EnableCollectionSynchronization(EventLogs, _lock);

            _detachTab = new DelegateCommand(OnDetachTab);
            _closeTab = new DelegateCommand(OnCloseTab);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    GetServerEventData();
                    Thread.Sleep(1000);
                }
            });
        }

        private async void GetServerEventData()
        {
            var serverEvents = await ServerAPI.GetServerEvents();

            if (serverEvents != null)
            {
                foreach (var ev in serverEvents)
                {
                    if (!EventLogs.Any(e => e.EventTime == ev.EventTime && e.EventType == ev.EventType && e.Data == ev.Data))
                    {
                        EventLogs.Insert(0, ev);
                    }
                }
            }
        }

        public void OnCloseTab(object obj)
        {
            var tab = MainView.TabItems.FirstOrDefault(t => t.Header.Equals("Event Logs"));
            MainView.TabItems.Remove(tab);
        }

        public void OnDetachTab(object obj)
        {
            var window = new Window
            {
                Title = "Event Logs",
                Content = new EventLogView(),
                DataContext = this
            };

            window.Show();
            OnCloseTab(null);
        }
    }
}