using Client.API;
using Client.Commands;
using Client.Models;
using Client.Services;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private MainView MainView { get; set; }
        private SignalR SignalR { get; set; }

        private ObservableCollection<Agent> _agents;
        public ObservableCollection<Agent> Agents
        {
            get
            {
                return _agents;
            }
            set
            {
                _agents = value;
                NotifyPropertyChanged(nameof(Agents));
            }
        } 

        public Agent SelectedAgent { get; set; }

        public ObservableCollection<TabItem> OpenTabs { get; set; } = new ObservableCollection<TabItem>();

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                _selectedTabIndex = value;
                NotifyPropertyChanged(nameof(SelectedTabIndex));
            }
        }

        public ICommand AgentInteract { get; }
        public ICommand AgentRemove { get; }
        public ICommand AgentExit { get; }

        public ICommand OpenEventLog { get; }
        public ICommand OpenWebLog { get; }
        public ICommand OpenListeners { get; }

        public ICommand OpenPayloadGenerator { get; }

        public ICommand ExitClient { get; }

        public MainViewModel(MainView mainView, SignalR signalR)
        {
            MainView = mainView;
            SignalR = signalR;

            Agents = new ObservableCollection<Agent>();

            AgentInteract = new AgentInteractCommand(this, SignalR);
            AgentRemove = new AgentRemoveCommand(this);

            OpenEventLog = new OpenTabCommand("Event Log", TabType.EventLog, this, SignalR);
            OpenWebLog = new OpenTabCommand("Web Log", TabType.WebLog, this, SignalR);
            OpenListeners = new OpenTabCommand("Listeners", TabType.Listeners, this, SignalR);

            OpenPayloadGenerator = new OpenWindowCommand(WindowType.PayloadGenerator);

            ExitClient = new ExitCommand();

            MainView.Closing += OnWindowClosing;

            OpenEventLog.Execute(null);

            GetAgentData();
        }

        private async void GetAgentData()
        {
            var agentData = await AgentAPI.GetAgentData();

            if (agentData != null)
            {
                foreach (var agent in agentData)
                {
                    Agents.Add(new Agent
                    {
                        AgentId = agent.Metadata.AgentID,
                        Hostname = agent.Metadata.Hostname,
                        IpAddress = agent.Metadata.IPAddress,
                        Identity = agent.Metadata.Identity,
                        ProcessName = agent.Metadata.ProcessName,
                        ProcessId = agent.Metadata.ProcessID,
                        Arch = agent.Metadata.Arch.ToString(),
                        CLR = agent.Metadata.CLR,
                        Integrity = agent.Metadata.Integrity.ToString(),
                        AgentModules = agent.LoadModules,
                        LastSeen = Core.Helpers.CalculateTimeDiff(agent.LastSeen)
                    });
                }
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            ExitClient.Execute(null);
        }
    }
}