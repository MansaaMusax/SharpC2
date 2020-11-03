using Client.Commands;
using Client.Services;

using Newtonsoft.Json;

using Shared.Models;

using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    class WebLogViewModel : BaseViewModel
    {
        public ObservableCollection<WebLog> WebLog { get; set; } = new ObservableCollection<WebLog>();

        public WebLogViewModel(MainViewModel mainViewModel)
        {
            SignalR.ServerEventReceived += SignalR_ServerEventReceived;

            CloseTab = new CloseTabCommand("Web Log", mainViewModel);
            DetachTab = new DetachTabCommand("Web Log", mainViewModel);
            RenameTab = new RenameTabCommand("Web Log", mainViewModel);

            GetWebLogs();
        }

        void SignalR_ServerEventReceived(ServerEvent ev)
        {
            if (ev.Type == ServerEvent.EventType.WebLog)
            {
                var log = JsonConvert.DeserializeObject<WebLog>(ev.Data.ToString());
                WebLog.Insert(0, log);
            }
        }

        async void GetWebLogs()
        {
            var logs = await SharpC2API.Listeners.GetWebLogs();

            if (logs != null)
            {
                foreach (var log in logs)
                {
                    WebLog.Insert(0, log);
                }
            }
        }
    }
}