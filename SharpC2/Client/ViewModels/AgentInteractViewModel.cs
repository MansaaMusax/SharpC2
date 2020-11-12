using Client.Commands;
using Client.Models;
using Client.Services;
using Shared.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class AgentInteractViewModel : BaseViewModel
    {
        readonly MainViewModel MainViewModel;
        readonly Agent Agent;

        public List<string> CommandHistory { get; set; } = new List<string>();

        private string _agentOutput;
        public string AgentOutput
        {
            get
            {
                return _agentOutput;
            }
            set
            {
                _agentOutput = value;
                NotifyPropertyChanged(nameof(AgentOutput));
            }
        }

        public string AgentLabel { get; set; }

        private string _agentCommand;
        public string AgentCommand
        {
            get
            {
                return _agentCommand;
            }
            set
            {
                _agentCommand = value;
                NotifyPropertyChanged(nameof(AgentCommand));
            }
        }

        public ICommand SendAgentCommand { get; }

        public AgentInteractViewModel(MainViewModel MainViewModel, Agent Agent)
        {
            this.MainViewModel = MainViewModel;
            this.Agent = Agent;

            SignalR.AgentEventReceived += SignalR_AgentEventReceived;

            AgentLabel = $"{Agent.AgentID} >";

            CloseTab = new CloseTabCommand(Agent.AgentID, MainViewModel);
            DetachTab = new DetachTabCommand(Agent.AgentID, MainViewModel);
            RenameTab = new RenameTabCommand(Agent.AgentID, MainViewModel);

            SendAgentCommand = new SendAgentCommand(this, Agent, MainViewModel.AgentTasks);

            GetAgentData();
        }

        private void SignalR_AgentEventReceived(AgentEvent ev)
        {
            if (ev.AgentID.Equals(Agent.AgentID, StringComparison.OrdinalIgnoreCase))
            {
                AddEvent(ev);
            }
        }

        private async void GetAgentData()
        {
            var agentData = await SharpC2API.Agents.GetAgentData(Agent.AgentID);

            if (agentData != null)
            {
                foreach (var ev in agentData)
                {
                    AddEvent(ev);
                }
            }
        }

        private void AddEvent(AgentEvent ev)
        {
            var message = new StringBuilder();

            switch (ev.Type)
            {
                case AgentEvent.EventType.ModuleRegistered:

                    message.AppendLine();
                    message.AppendLine($"[+] Module Registered: {ev.Data}");
                    break;

                case AgentEvent.EventType.CommandRequest:
                    message.AppendLine();
                    message.AppendLine($"[*] <{ev.Nick}> tasked agent to run: {ev.Data}");
                    break;

                case AgentEvent.EventType.AgentOutput:
                    message.AppendLine();
                    message.AppendLine(ev.Data.ToString());
                    break;
                case AgentEvent.EventType.AgentError:
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