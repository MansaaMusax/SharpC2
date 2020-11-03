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
        readonly MainView MainView;

        ObservableCollection<Agent> _agents;
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

        public MainViewModel(MainView MainView)
        {
            this.MainView = MainView;

            Agents = new ObservableCollection<Agent>();

            AgentInteract = new AgentInteractCommand(this);
            AgentRemove = new AgentRemoveCommand(this);

            OpenEventLog = new OpenTabCommand("Event Log", TabType.EventLog, this);
            OpenWebLog = new OpenTabCommand("Web Log", TabType.WebLog, this);
            OpenListeners = new OpenTabCommand("Listeners", TabType.Listeners, this);

            OpenPayloadGenerator = new OpenWindowCommand(WindowType.PayloadGenerator);

            ExitClient = new ExitCommand();

            MainView.Closing += OnWindowClosing;

            OpenEventLog.Execute(null);

            GetAgentData();
        }

        async void GetAgentData()
        {
            var agentData = await SharpC2API.Agents.GetAgentData();

            if (agentData != null)
            {
                foreach (var agent in agentData)
                {
                    Agents.Add(new Agent
                    {
                        AgentID = agent.AgentID,
                        Hostname = agent.Hostname,
                        IPAddress = agent.IPAddress,
                        Identity = agent.Identity,
                        Process = agent.Process,
                        PID = agent.PID,
                        Arch = agent.Arch,
                        Integrity = agent.Elevation,
                        LastSeen = agent.LastSeen
                    });
                }
            }
        }

        void OnWindowClosing(object sender, CancelEventArgs e)
        {
            ExitClient.Execute(null);
        }
    }
}