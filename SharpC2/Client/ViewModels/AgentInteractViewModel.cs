using Client.API;
using Client.Commands;
using Client.Models;
using Client.Services;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class AgentInteractViewModel : BaseViewModel
    {
        private readonly MainViewModel MainViewModel;
        private readonly SignalR SignalR;
        private readonly Agent Agent;

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

        public AgentInteractViewModel(MainViewModel viewModel, Agent agent, SignalR signalR)
        {
            MainViewModel = viewModel;
            Agent = agent;
            SignalR = signalR;

            SignalR.NewAgentEvenReceived += SignalR_NewAgentEvenReceived;

            AgentLabel = $"{Agent.AgentId} >";

            SendAgentCommand = new SendAgentCommand(this, Agent);

            GetAgentData();
        }

        private void SignalR_NewAgentEvenReceived(AgentEvent ev)
        {
            if (ev.AgentId.Equals(Agent.AgentId, StringComparison.OrdinalIgnoreCase))
            {
                AddEvent(ev);
            }
        }

        private async void GetAgentData()
        {
            var agentData = await AgentAPI.GetAgentData(Agent.AgentId);

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
                case AgentEventType.ModuleRegistered:
                    message.AppendLine();
                    message.AppendLine($"[+] Module Registered: {ev.Data}");
                    break;
                case AgentEventType.CommandRequest:
                    message.AppendLine();
                    message.AppendLine($"[*] <{ev.Nick}> tasked agent to run: {ev.Data}");
                    break;
                case AgentEventType.CommandResponse:
                    message.AppendLine();
                    message.AppendLine(ev.Data);
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

        private void ClearCommandInput()
        {
            AgentCommand = string.Empty;
        }
    }
}