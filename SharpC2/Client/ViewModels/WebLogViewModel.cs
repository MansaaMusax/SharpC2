using Client.Commands;
using Client.Services;

using Shared.Models;

using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    class WebLogViewModel : BaseViewModel
    {
        public ObservableCollection<string> WebLog { get; set; } = new ObservableCollection<string>();

        public WebLogViewModel(MainViewModel mainViewModel, SignalR signalR)
        {
            // signalR.NewWebEvenReceived += SignalR_NewWebEvenReceived;

            CloseTab = new CloseTabCommand("Web Log", mainViewModel);
            DetachTab = new DetachTabCommand("Web Log", mainViewModel);
            RenameTab = new RenameTabCommand("Web Log", mainViewModel);

            GetWebLogs();
        }

        void SignalR_NewWebEvenReceived(WebLog log)
        {
            AddWebLog(log);
        }

        async void GetWebLogs()
        {
            var logs = await SharpC2API.Listeners.GetWebLogs();

            if (logs != null)
            {
                foreach (var log in logs)
                {
                    AddWebLog(log);
                }
            }
        }

        void AddWebLog(WebLog l)
        {
            WebLog.Insert(0, $"[{l.Date}]   visit ({l.Listener}) from {l.Origin}\n\n{l.WebRequest}");
        }
    }
}