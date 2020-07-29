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
    class WebLogViewModel : BaseViewModel
    {
        private MainWindowViewModel MainView { get; set; }

        public ObservableCollection<WebLog> WebLogs { get; set; }
        private readonly object _lock = new object();

        private readonly DelegateCommand _detachTab;
        private readonly DelegateCommand _closeTab;

        public ICommand DetachTab => _detachTab;
        public ICommand CloseTab => _closeTab;

        public WebLogViewModel(MainWindowViewModel mainView)
        {
            MainView = mainView;

            WebLogs = new ObservableCollection<WebLog>();
            BindingOperations.EnableCollectionSynchronization(WebLogs, _lock);

            _detachTab = new DelegateCommand(OnDetachTab);
            _closeTab = new DelegateCommand(OnCloseTab);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    GetWebLogs();
                    Thread.Sleep(1000);
                }
            });
        }

        private async void GetWebLogs()
        {
            var logs = await ListenerAPI.GetWebLogs();

            if (logs != null)
            {
                foreach (var log in logs)
                {
                    if (!WebLogs.Any(l => l.Date == log.Date))
                    {
                        WebLogs.Insert(0, log);
                    }
                }
            }
        }

        public void OnCloseTab(object obj)
        {
            var tab = MainView.TabItems.FirstOrDefault(t => t.Header.Equals("Web Logs"));
            MainView.TabItems.Remove(tab);
        }

        public void OnDetachTab(object obj)
        {
            var window = new Window
            {
                Title = "Web Logs",
                Content = new WebLogView(),
                DataContext = this
            };

            window.Show();
            OnCloseTab(null);
        }
    }
}