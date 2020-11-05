using Client.Commands;
using Client.Models;
using Client.Services;

using Newtonsoft.Json;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

using YamlDotNet.Serialization;

namespace Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        readonly MainView MainView;

        public ObservableCollection<Agent> Agents { get; set; } = new ObservableCollection<Agent>();

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

        public List<AgentTask> AgentTasks = new List<AgentTask>();

        public ICommand AgentInteract { get; }
        public ICommand LoadAgentModule { get; }
        public ICommand AgentRemove { get; }
        public ICommand AgentExit { get; }

        public ICommand OpenEventLog { get; }
        public ICommand OpenWebLog { get; }
        public ICommand OpenListeners { get; }

        public ICommand OpenPayloadGenerator { get; }

        public ICommand OpenAbout { get; }
        public ICommand ExitClient { get; }

        public MainViewModel(MainView MainView)
        {
            this.MainView = MainView;

            Agents = new ObservableCollection<Agent>();

            SignalR.AgentEventReceived += SignalR_AgentEventReceived;

            AgentInteract = new AgentInteractCommand(this);
            LoadAgentModule = new LoadAgentModuleCommand(this);
            AgentRemove = new AgentRemoveCommand(this);

            OpenEventLog = new OpenTabCommand("Event Log", TabType.EventLog, this);
            OpenWebLog = new OpenTabCommand("Web Log", TabType.WebLog, this);
            OpenListeners = new OpenTabCommand("Listeners", TabType.Listeners, this);

            OpenPayloadGenerator = new OpenWindowCommand(WindowType.PayloadGenerator);

            OpenAbout = new OpenWindowCommand(WindowType.About);
            ExitClient = new ExitCommand();

            MainView.Closing += OnWindowClosing;

            OpenEventLog.Execute(null);

            LoadTaskDefinitions();
            GetAgentData();
        }

        private void SignalR_AgentEventReceived(AgentEvent ev)
        {
            switch (ev.Type)
            {
                case AgentEvent.EventType.InitialAgent:

                    var metadata = JsonConvert.DeserializeObject<AgentMetadata>(ev.Data.ToString());

                    if (!Agents.Any(a => a.AgentID.Equals(metadata.AgentID, StringComparison.OrdinalIgnoreCase)))
                    {
                        AddNewAgent(metadata);
                    }

                    break;

                case AgentEvent.EventType.AgentCheckin:

                    var lastSeen = DateTime.Parse(ev.Data.ToString());
                    var agent = Agents.FirstOrDefault(a => a.AgentID.Equals(ev.AgentID, StringComparison.OrdinalIgnoreCase));

                    if (agent != null)
                    {
                        agent.LastSeen = lastSeen;
                    }    

                    break;

                default:
                    break;
            }
        }

        async void GetAgentData()
        {
            var agentData = await SharpC2API.Agents.GetAgentData();

            if (agentData != null)
            {
                foreach (var agent in agentData)
                {
                    AddNewAgent(agent);
                }
            }
        }

        void AddNewAgent(AgentMetadata AgentMetadata)
        {
            Agents.Add(new Agent
            {
                AgentID = AgentMetadata.AgentID,
                Hostname = AgentMetadata.Hostname,
                IPAddress = AgentMetadata.IPAddress,
                Identity = AgentMetadata.Identity,
                Process = AgentMetadata.Process,
                PID = AgentMetadata.PID,
                Arch = AgentMetadata.Arch,
                Integrity = AgentMetadata.Elevation,
                LastSeen = AgentMetadata.LastSeen
            });
        }

        void OnWindowClosing(object sender, CancelEventArgs e)
        {
            ExitClient.Execute(null);
        }

        void LoadTaskDefinitions()
        {
            var path = Path.Combine(Assembly.GetExecutingAssembly().Location.Replace("SharpC2.dll", ""), "Core", "Tasks");
            var files = Directory.GetFiles(path);
            var yamlDotNet = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

            foreach (var file in files)
            {
                var yaml = File.ReadAllText(file);
                var tasks = yamlDotNet.Deserialize<List<AgentTask>>(yaml);
                AgentTasks.AddRange(tasks);
            }
        }
    }
}