using Client.API;
using Client.Commands;
using Client.Services;

using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    class WebLogViewModel : BaseViewModel
    {
        public ObservableCollection<string> WebLog { get; set; } = new ObservableCollection<string>();

        public WebLogViewModel(MainViewModel mainViewModel, SignalR signalR)
        {
            signalR.NewWebEvenReceived += SignalR_NewWebEvenReceived;

            CloseTab = new CloseTabCommand("Web Log", mainViewModel);
            DetachTab = new DetachTabCommand("Web Log", mainViewModel);
            RenameTab = new RenameTabCommand("Web Log", mainViewModel);

            GetWebLogs();
        }

        private void SignalR_NewWebEvenReceived(WebLog log)
        {
            AddWebLog(log);
        }

        private async void GetWebLogs()
        {
            var logs = await ListenerAPI.GetWebLogs();

            if (logs != null)
            {
                foreach (var log in logs)
                {
                    AddWebLog(log);
                }
            }
        }

        private void AddWebLog(WebLog l)
        {
            WebLog.Insert(0, $"[{l.Date}]   visit ({l.Listener}) from {l.Origin}\n\n{l.WebRequest}");
        }
    }
}