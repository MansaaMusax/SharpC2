using Client.API;
using Client.Models;
using Client.ViewModels;

using System;
using System.Linq;
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
                builder.AppendLine(GetModuletHelpText());
            }
            else if (ViewModel.AgentCommand.Equals("core clear", StringComparison.OrdinalIgnoreCase))
            {
                AgentAPI.ClearCommandQueue(Agent.AgentId);
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

                    AgentAPI.SubmitAgentCommand(Agent.AgentId, mod, cmd, args);
                    ViewModel.CommandHistory.Insert(0, $"{mod} {cmd} {args}");
                }
            }

            ViewModel.AgentOutput += builder.ToString();
            ViewModel.AgentCommand = string.Empty;
        }

        private string GetModuletHelpText()
        {
            var result = new SharpC2ResultList<ModuleHelpText>
            {
                new ModuleHelpText
                {
                    Module = "core",
                    Command = "clear",
                    Description = "Clear the queued commands for this agent",
                    Usage = "core clear"
                }
            };

            foreach (var module in Agent.AgentModules.OrderBy(m => m.Name))
            {
                foreach (var cmd in module.Commands.OrderBy(c => c.Name))
                {
                    if (cmd.Visible)
                    {
                        result.Add(new ModuleHelpText
                        {
                            Module = module.Name,
                            Command = cmd.Name,
                            Description = cmd.Description,
                            Usage = cmd.HelpText
                        });
                    }
                }
            }

            return result.ToString();
        }
    }
}