using Client.Models;
using Client.SharpC2API;
using Client.Views;
using Client.Views.Listeners;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private Window MainWindow { get; set; }

        private ObservableCollection<Agent> _agents;
        public ObservableCollection<Agent> Agents
        {
            get { return _agents; }
            set { _agents = value; NotifyPropertyChanged("Agents"); }
        }

        public ObservableCollection<TabItem> TabItems { get; set; }
        public Agent SelectedAgent { get; set; }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { _selectedTabIndex = value; NotifyPropertyChanged("SelectedTabIndex"); }
        }

        private readonly object _lock = new object();

        private readonly DelegateCommand _agentInteract;
        private readonly DelegateCommand _agentRemove;
        private readonly DelegateCommand _agentExit;
        private readonly DelegateCommand _openListeners;
        private readonly DelegateCommand _openEventLog;
        private readonly DelegateCommand _openWebLogs;
        private readonly DelegateCommand _openPayloadGenerator;

        public ICommand AgentInteract => _agentInteract;
        public ICommand AgentRemove => _agentRemove;
        public ICommand AgentExit => _agentExit;
        public ICommand OpenEventLog => _openEventLog;
        public ICommand OpenWebLogs => _openWebLogs;
        public ICommand OpenListeners => _openListeners;
        public ICommand OpenPayloadGenerator => _openPayloadGenerator;

        public MainWindowViewModel(Window window)
        {
            MainWindow = window;
            MainWindow.Closing += OnWindowClosing;
            
            Agents = new ObservableCollection<Agent>();
            TabItems = new ObservableCollection<TabItem>();
            BindingOperations.EnableCollectionSynchronization(Agents, _lock);
            BindingOperations.EnableCollectionSynchronization(TabItems, _lock);

            _agentInteract = new DelegateCommand(OnAgentInteract);
            _agentRemove = new DelegateCommand(OnAgentRemove);
            _agentExit = new DelegateCommand(OnAgentExit);
            _openEventLog = new DelegateCommand(OnOpenEventLog);
            _openWebLogs = new DelegateCommand(OnOpenWebLogs);
            _openListeners = new DelegateCommand(OnOpenListeners);
            _openPayloadGenerator = new DelegateCommand(OnOpenPayloadGenerator);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    GetAgentData();
                    Thread.Sleep(1000);
                }
            });

            AddEventLogTab();
        }

        private void OnOpenWebLogs(object obj)
        {
            TabItem tab;

            if (!TabItems.Any(t => t.Header.Equals("Web Logs")))
            {
                tab = new TabItem
                {
                    Header = "Web Logs",
                    Content = new WebLogView(),
                    DataContext = new WebLogViewModel(this)
                };

                TabItems.Add(tab);
            }
            else
            {
                tab = TabItems.FirstOrDefault(t => t.Header.Equals("Web Logs"));
            }

            SelectedTabIndex = TabItems.IndexOf(tab);
        }

        private void OnOpenEventLog(object obj)
        {
            TabItem tab;

            if (!TabItems.Any(t => t.Header.Equals("Event Logs")))
            {
                tab = new TabItem
                {
                    Header = "Event Logs",
                    Content = new EventLogView(),
                    DataContext = new EventLogViewModel(this)
                };

                TabItems.Add(tab);
            }
            else
            {
                tab = TabItems.FirstOrDefault(t => t.Header.Equals("Event Logs"));
            }

            SelectedTabIndex = TabItems.IndexOf(tab);
        }

        private void OnAgentExit(object obj)
        {
            AgentAPI.SubmitAgentCommand(SelectedAgent.AgentId, "core", "exit");
        }

        private void OnAgentRemove(object obj)
        {
            var agent = Agents.FirstOrDefault(a => a.AgentId.Equals(SelectedAgent.AgentId));

            AgentAPI.RemoveAgent(agent.AgentId);
            Agents.Remove(agent);
        }

        private void OnOpenPayloadGenerator(object obj)
        {
            var window = new GeneratePayloadView();
            window.DataContext = new GeneratePayloadViewModel(window);
            
            window.ShowDialog();
        }

        private void OnOpenListeners(object obj)
        {
            TabItem tab;

            if (!TabItems.Any(t => t.Header.Equals("Listeners")))
            {
                tab = new TabItem
                {
                    Header = "Listeners",
                    Content = new ListenerListView(),
                    DataContext = new ListenerListViewModel(this)
                };

                TabItems.Add(tab);
            }
            else
            {
                tab = TabItems.FirstOrDefault(t => t.Header.Equals("Listeners"));
            }

            SelectedTabIndex = TabItems.IndexOf(tab);
        }

        private void OnAgentInteract(object obj)
        {
            TabItem tab;

            if (!TabItems.Any(t => t.Header.Equals(SelectedAgent.AgentId)))
            {
                tab = new TabItem
                {
                    Header = SelectedAgent.AgentId,
                    Content = new AgentInteractView(),
                    DataContext = new AgentInteractViewModel(this, SelectedAgent.AgentId)
                };

                TabItems.Add(tab);
            }
            else
            {
                tab = TabItems.FirstOrDefault(t => t.Header.Equals(SelectedAgent.AgentId));
            }

            SelectedTabIndex = TabItems.IndexOf(tab);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            ClientAPI.ClientLogoff();
            Environment.Exit(0);
        }

        private async void GetAgentData()
        {
            var agentData = await AgentAPI.GetAgentData();

            if (agentData != null)
            {
                foreach (var agent in agentData)
                {
                    if (!Agents.Any(a => a.AgentId.Equals(agent.Metadata.AgentID, StringComparison.OrdinalIgnoreCase)))
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
                            ChildAgents = new List<Agent>(),
                            LastSeen = Helpers.CalculateTimeDiff(agent.LastSeen)
                        });
                    }
                    else
                    {
                        Agents.FirstOrDefault(a => a.AgentId.Equals(agent.Metadata.AgentID, StringComparison.OrdinalIgnoreCase))
                            .AgentModules = agent.LoadModules;

                        Agents.FirstOrDefault(a => a.AgentId.Equals(agent.Metadata.AgentID, StringComparison.OrdinalIgnoreCase))
                            .LastSeen = Helpers.CalculateTimeDiff(agent.LastSeen);
                    }
                }
            }
        }

        private void AddEventLogTab()
        {
            OnOpenEventLog(null);
        }
    }
}