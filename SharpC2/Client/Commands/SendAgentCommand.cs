using Client.Models;
using Client.Services;
using Client.ViewModels;

using Newtonsoft.Json;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Client.Commands
{
    class SendAgentCommand : ICommand
    {
        readonly AgentInteractViewModel AgentInteractViewModel;
        readonly Agent Agent;
        readonly List<AgentTask> AgentTasks;

        public event EventHandler CanExecuteChanged;

        public SendAgentCommand(AgentInteractViewModel AgentInteractViewModel, Agent Agent, List<AgentTask> AgentTasks)
        {
            this.AgentInteractViewModel = AgentInteractViewModel;
            this.Agent = Agent;
            this.AgentTasks = AgentTasks;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var builder = new StringBuilder();

            if (AgentInteractViewModel.AgentCommand.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                builder.AppendLine("\nAgent Commands");
                builder.AppendLine("==============\n");

                var taskOutput = new SharpC2ResultList<AgentHelp>();

                foreach (var task in AgentTasks.OrderBy(t => t.Alias))
                {
                    taskOutput.Add(new AgentHelp
                    {
                        Alias = task.Alias,
                        Usage = task.Usage
                    });
                }

                builder.AppendLine(taskOutput.ToString());
            }
            else
            {
                var split = AgentInteractViewModel.AgentCommand.Split(" ");

                var alias = split[0];
                var args = split[1..].ToList();

                var task = AgentTasks.FirstOrDefault(t => t.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase));

                if (task == null)
                {
                    builder.AppendLine($"\n[-] Unknown command.\n");
                }
                else
                {
                    // null out current values first
                    if (task.Parameters != null)
                    {
                        foreach (var param in task.Parameters)
                        {
                            param.Value = null;
                        }
                    }

                    // add new values
                    for (var i = 0; i < args.Count; i++)
                    {
                        switch (task.Parameters[i].Type)
                        {
                            case AgentTask.Parameter.ParameterType.String:

                                task.Parameters[i].Value = args[i];
                                break;
                            
                            case AgentTask.Parameter.ParameterType.Integer:

                                task.Parameters[i].Value = Convert.ToInt32(args[i]);
                                break;

                            case AgentTask.Parameter.ParameterType.File:

                                var bytes = File.ReadAllBytes(args[i]);
                                task.Parameters[i].Value = bytes;

                                task.Parameters.Insert(i + 1, new AgentTask.Parameter
                                {
                                    Name = "Path",
                                    Value = args[i],
                                    Type = AgentTask.Parameter.ParameterType.String
                                });

                                args.Insert(i, string.Empty);

                                break;
                            case AgentTask.Parameter.ParameterType.Listener:
                                break;
                            case AgentTask.Parameter.ParameterType.ShellCode:
                                break;

                            default:
                                break;
                        }
                    }

                    var json = JsonConvert.SerializeObject(task);

                    SharpC2API.Agents.SubmitAgentCommand(Agent.AgentID, task.Module, task.Command, json);
                    AgentInteractViewModel.CommandHistory.Insert(0, AgentInteractViewModel.AgentCommand);
                }
            }

            AgentInteractViewModel.AgentOutput += builder.ToString();
            AgentInteractViewModel.AgentCommand = string.Empty;
        }
    }
}