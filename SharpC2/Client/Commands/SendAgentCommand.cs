using Client.Models;
using Client.Services;
using Client.ViewModels;

using System;
using System.Text;
using System.Windows.Input;

namespace Client.Commands
{
    class SendAgentCommand : ICommand
    {
        private readonly AgentInteractViewModel ViewModel;
        private readonly Agent Agent;

        public event EventHandler CanExecuteChanged;

        public SendAgentCommand(AgentInteractViewModel viewModel, Agent agent)
        {
            ViewModel = viewModel;
            Agent = agent;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var builder = new StringBuilder();

            if (ViewModel.AgentCommand.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                builder.AppendLine("\nAgent Commands");
                builder.AppendLine("==============");
            }
            else if (ViewModel.AgentCommand.Equals("core clear", StringComparison.OrdinalIgnoreCase))
            {
                SharpC2API.Agents.ClearCommandQueue(Agent.AgentID);
                builder.AppendLine($"\n[*] Commands cleared\n");
            }
            else
            {
                var split = ViewModel.AgentCommand.Split(" ");

                if (split.Length < 2)
                {
                    builder.AppendLine($"\n[-] Invalid syntax. Usage: [module] [command] [args]\n");
                }
                else
                {
                    var mod = split[0];
                    var cmd = split[1];
                    var args = string.Join(" ", split[2..]);

                    SharpC2API.Agents.SubmitAgentCommand(Agent.AgentID, mod, cmd, args);
                    ViewModel.CommandHistory.Insert(0, $"{mod} {cmd} {args}");
                }
            }

            ViewModel.AgentOutput += builder.ToString();
            ViewModel.AgentCommand = string.Empty;
        }
    }
}