using Client.SharpC2API;
using Client.Views;

using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.ViewModels
{
    class AgentInteractViewModel : BaseViewModel
    {
        private MainWindowViewModel MainViewModel { get; set; }
        public string AgentId { get; set; }

        public List<string> CommandHistory { get; set; } = new List<string>();

        public ObservableCollection<AgentEvent> AgentEvents { get; set; }

        private readonly DelegateCommand _detachTab;
        private readonly DelegateCommand _closeTab;

        public ICommand DetachTab => _detachTab;
        public ICommand CloseTab => _closeTab;


        private string _agentOutput;
        public string AgentOutput
        {
            get { return _agentOutput; }
            set { _agentOutput = value; NotifyPropertyChanged("AgentOutput"); }
        }

        public string AgentLabel { get; set; }

        private string _agentCommand;
        public string AgentCommand
        {
            get { return _agentCommand; }
            set { _agentCommand = value; NotifyPropertyChanged("AgentCommand"); }
        }

        private readonly DelegateCommand _submitAgentCommand;

        public ICommand SubmitAgentCommand => _submitAgentCommand;

        private readonly object _lock = new object();

        public AgentInteractViewModel(MainWindowViewModel viewModel, string agentId)
        {
            MainViewModel = viewModel;
            AgentId = agentId;

            AgentLabel = string.Format("{0} >", AgentId);

            AgentEvents = new ObservableCollection<AgentEvent>();
            BindingOperations.EnableCollectionSynchronization(AgentEvents, _lock);

            _submitAgentCommand = new DelegateCommand(OnAgentCommand);
            _detachTab = new DelegateCommand(OnDetachTab);
            _closeTab = new DelegateCommand(OnCloseTab);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    GetAgentData(AgentId);
                    Thread.Sleep(1000);
                }
            });
        }

        private void OnAgentCommand(object obj)
        {
            var agent = MainViewModel.Agents.FirstOrDefault(a => a.AgentId.Equals(AgentId, StringComparison.OrdinalIgnoreCase));
            var command = AgentCommand;

            if (command.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                AgentOutput += "\nAgent Commands\n";
                AgentOutput += "==============\n\n";
                AgentOutput += AgentCommands.GetModuletHelpText(agent.AgentModules);
                AgentOutput += "\n\n";
                ClearCommandInput();
                return;
            }
            else if (command.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                AgentAPI.ClearCommandQueue(AgentId);
                AgentOutput += $"\n[*] Commands cleared\n";
                ClearCommandInput();
                return;
            }

            var split = command.Split(" ");

            if (split.Length < 2)
            {
                AgentOutput += $"\n[-] Invalid syntax. Usage: [module] [command] [args]\n";
                ClearCommandInput();
                return;
            }

            var mod = split[0];
            var cmd = split[1];

            if (!agent.AgentModules.Any(m => m.Name.Equals(mod, StringComparison.OrdinalIgnoreCase)))
            {
                AgentOutput += $"\n[-] {mod} module not found\n";
                ClearCommandInput();
                return;
            }

            var moduleCommands = agent.AgentModules.Where(m => m.Name.Equals(mod, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Commands).FirstOrDefault();

            if (!moduleCommands.Any(c => c.Name.Equals(cmd, StringComparison.OrdinalIgnoreCase)))
            {
                AgentOutput += $"\n[-] {cmd} not found in module {mod}\n";
                ClearCommandInput();
                return;
            }

            var args = string.Join(" ", split[2..]);

            if (cmd.Equals("load-module", StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(args))
                {
                    args = Convert.ToBase64String(File.ReadAllBytes(args));
                }
                else
                {
                    AgentOutput += $"\n[-] {args} not found\n";
                    ClearCommandInput();
                    return;
                }
            }
            else if (cmd.Equals("execute-assembly", StringComparison.OrdinalIgnoreCase))
            {
                var tmpArgs = args.Split(" ");
                if (File.Exists(tmpArgs[0]))
                {
                    tmpArgs[0] = Convert.ToBase64String(File.ReadAllBytes(tmpArgs[0]));
                    args = string.Join(" ", tmpArgs[0..]);
                }
                else
                {
                    AgentOutput += $"\n[-] {args} not found\n";
                    ClearCommandInput();
                    return;
                }
            }
            else if (cmd.Equals("execute-dll", StringComparison.OrdinalIgnoreCase))
            {
                var tmpArgs = args.Split(" ");
                if (File.Exists(tmpArgs[0]))
                {
                    tmpArgs[0] = Convert.ToBase64String(File.ReadAllBytes(tmpArgs[0]));
                    args = string.Join(" ", tmpArgs[0..]);
                }
                else
                {
                    AgentOutput += $"\n[-] {args} not found\n";
                    ClearCommandInput();
                    return;
                }
            }
            else if (cmd.Equals("execute-exe", StringComparison.OrdinalIgnoreCase))
            {
                var tmpArgs = args.Split(" ");
                if (File.Exists(tmpArgs[0]))
                {
                    tmpArgs[0] = Convert.ToBase64String(File.ReadAllBytes(tmpArgs[0]));
                    args = string.Join(" ", tmpArgs[0..]);
                }
                else
                {
                    AgentOutput += $"\n[-] {args} not found\n";
                    ClearCommandInput();
                    return;
                }
            }

            AgentAPI.SubmitAgentCommand(AgentId, mod, cmd, args);
            CommandHistory.Insert(0, $"{mod} {cmd} {args}");

            ClearCommandInput();
        }

        private async void GetAgentData(string agentId)
        {
            try
            {
                var agentData = await AgentAPI.GetAgentData(agentId);

                if (agentData != null)
                {
                    foreach (var ev in agentData)
                    {
                        if (!AgentEvents.Any(e => e.EventTime == ev.EventTime && e.EventType == ev.EventType && e.Data == ev.Data))
                        {
                            AgentEvents.Add(ev);

                            var message = new StringBuilder();

                            switch (ev.EventType)
                            {
                                case AgentEventType.ModuleRegistered:
                                    message.AppendLine();
                                    message.AppendLine($"[+] Module Registered: {ev.Data}");
                                    break;
                                case AgentEventType.CommandRequest:
                                    message.AppendLine();
                                    message.AppendLine($"[*] Tasked agent to run: {ev.Data}");
                                    break;
                                case AgentEventType.CommandResponse:
                                    message.AppendLine(ev.Data);
                                    message.AppendLine();
                                    break;
                                case AgentEventType.AgentError:
                                    message.AppendLine();
                                    message.AppendLine($"[-] {ev.Data}");
                                    break;
                                default:
                                    break;
                            }

                            AgentOutput += message.ToString();
                        }
                    }
                }
            }
            catch { }
        }

        private void ClearCommandInput()
        {
            AgentCommand = string.Empty;
        }

        public void OnCloseTab(object obj)
        {
            var tab = MainViewModel.TabItems.FirstOrDefault(t => t.Header.Equals(AgentId));
            MainViewModel.TabItems.Remove(tab);
        }

        public void OnDetachTab(object obj)
        {
            var window = new Window
            {
                Title = AgentId,
                Content = new AgentInteractView(),
                DataContext = new AgentInteractViewModel(MainViewModel, AgentId)
            };

            window.Show();
            OnCloseTab(null);
        }
    }
}