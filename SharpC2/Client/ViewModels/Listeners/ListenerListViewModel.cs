using Client.SharpC2API;
using Client.ViewModels.Listeners;
using Client.Views.Listeners;

using SharpC2.Listeners;

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ListenerListViewModel : BaseViewModel
    {
        private MainWindowViewModel MainView { get; set; }
        public ObservableCollection<ListenerBase> ActiveListeners { get; set; }
        public ListenerBase SelectedListener { get; set; }
        private readonly object _lock = new object();

        private readonly DelegateCommand _addListener;
        private readonly DelegateCommand _removeListener;
        private readonly DelegateCommand _detachTab;
        private readonly DelegateCommand _closeTab;

        public ICommand AddListener => _addListener;
        public ICommand RemoveListener => _removeListener;
        public ICommand DetachTab => _detachTab;
        public ICommand CloseTab => _closeTab;

        public ListenerListViewModel(MainWindowViewModel mainView)
        {
            MainView = mainView;

            ActiveListeners = new ObservableCollection<ListenerBase>();
            BindingOperations.EnableCollectionSynchronization(ActiveListeners, _lock);

            _addListener = new DelegateCommand(OnAddListener);
            _removeListener = new DelegateCommand(OnRemoveListener);
            _detachTab = new DelegateCommand(OnDetachTab);
            _closeTab = new DelegateCommand(OnCloseTab);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    GetActiveListeners();
                    Thread.Sleep(1000);
                }
            });
        }

        private async void GetActiveListeners()
        {
            var httpListeners = await ListenerAPI.GetHttpListeners();
            var tcpListeners = await ListenerAPI.GetTcpListeners();

            if (httpListeners != null)
            {
                foreach (var http in httpListeners)
                {
                    if (!ActiveListeners.Any(l => l.ListenerId.Equals(http.ListenerId)))
                    {
                        ActiveListeners.Add(http);
                    }
                }

                foreach (var listener in ActiveListeners.Where(l => l.Type == ListenerType.HTTP).ToList())
                {
                    if (!httpListeners.Any(l => l.ListenerId == listener.ListenerId))
                    {
                        ActiveListeners.Remove(listener);
                    }
                }
            }

            if (tcpListeners != null)
            {
                foreach (var tcp in tcpListeners)
                {
                    if (!ActiveListeners.Any(l => l.ListenerId.Equals(tcp.ListenerId)))
                    {
                        ActiveListeners.Add(tcp);
                    }
                }

                foreach (var listener in ActiveListeners.Where(l => l.Type == ListenerType.TCP).ToList())
                {
                    if (!tcpListeners.Any(l => l.ListenerId == listener.ListenerId))
                    {
                        ActiveListeners.Remove(listener);
                    }
                }
            }
        }

        private void OnRemoveListener(object obj)
        {
            ListenerAPI.StopListener(SelectedListener.ListenerId, SelectedListener.Type);
        }

        private void OnAddListener(object obj)
        {
            var window = new AddListenerView();
            window.DataContext = new AddListenerViewModel(window);
            window.ShowDialog();
        }

        public void OnCloseTab(object obj)
        {
            var tab = MainView.TabItems.FirstOrDefault(t => t.Header.Equals("Listeners"));
            MainView.TabItems.Remove(tab);
        }

        public void OnDetachTab(object obj)
        {
            var window = new Window
            {
                Title = "Listeners",
                Content = new ListenerListView(),
                DataContext = this
            };

            window.Show();
            OnCloseTab(null);
        }
    }
}