using Client.Models;
using Client.Services;
using Client.ViewModels;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Client.Commands
{
    class SendAgentCommand : ICommand
    {
        readonly AgentInteractViewModel AgentInteractViewModel;
        readonly Agent Agent;
        readonly List<TaskDefinition> AgentTasks;

        public event EventHandler CanExecuteChanged;

        public SendAgentCommand(AgentInteractViewModel AgentInteractViewModel, Agent Agent, List<TaskDefinition> AgentTasks)
        {
            this.AgentInteractViewModel = AgentInteractViewModel;
            this.Agent = Agent;
            this.AgentTasks = AgentTasks;
        }

        public bool CanExecute(object parameter)
            => true;

        public async void Execute(object parameter)
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
                        Description = task.Description,
                        Usage = task.Usage,
                        OPSEC = task.OpSec
                    });
                }

                builder.AppendLine(taskOutput.ToString());
            }
            else
            {
                var split = AgentInteractViewModel.AgentCommand.Split(" ");

                var alias = split[0];

                var args = Regex.Matches(AgentInteractViewModel.AgentCommand, @"[\""].+?[\""]|[^ ]+")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToList();

                args.Remove(alias);

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
                            case TaskDefinition.Parameter.ParameterType.String:

                                if (args[i].StartsWith("\"") && args[i].EndsWith("\""))
                                {
                                    args[i] = args[i].Remove(0, 1);
                                    args[i] = args[i].Remove(args[i].Length - 1, 1);
                                }

                                task.Parameters[i].Value = args[i];
                                break;

                            case TaskDefinition.Parameter.ParameterType.Integer:

                                task.Parameters[i].Value = Convert.ToInt32(args[i]);
                                break;

                            case TaskDefinition.Parameter.ParameterType.Boolean:

                                task.Parameters[i].Value = Convert.ToBoolean(args[i]);
                                break;

                            case TaskDefinition.Parameter.ParameterType.File:

                                var bytes = File.ReadAllBytes(args[i]);
                                task.Parameters[i].Value = bytes;

                                task.Parameters.Insert(i + 1, new TaskDefinition.Parameter
                                {
                                    Name = "Path",
                                    Value = args[i],
                                    Type = TaskDefinition.Parameter.ParameterType.String
                                });

                                args.Insert(i, string.Empty);

                                break;

                            case TaskDefinition.Parameter.ParameterType.Listener:

                                var listeners = await SharpC2API.Listeners.GetAllListeners();
                                var listener = listeners.FirstOrDefault(l => l.Name.Equals(args[i], StringComparison.OrdinalIgnoreCase));

                                task.Parameters[i].Value = listener.Name;

                                var stager = await SharpC2API.Payloads.GenerateStager(new StagerRequest
                                {
                                    Listener = listener.Name,
                                    Type = StagerRequest.OutputType.EXE
                                });

                                task.Parameters.Insert(i + 1, new TaskDefinition.Parameter
                                {
                                    Name = "Assembly",
                                    Value = stager,
                                    Type = TaskDefinition.Parameter.ParameterType.File
                                });

                                break;

                            case TaskDefinition.Parameter.ParameterType.ShellCode:
                                break;

                            default:
                                break;
                        }
                    }

                    SharpC2API.Agents.SubmitAgentCommand(Agent.AgentID, task.Module, task.Command, task);
                    AgentInteractViewModel.CommandHistory.Insert(0, AgentInteractViewModel.AgentCommand);
                }
            }

            AgentInteractViewModel.AgentOutput += builder.ToString();
            AgentInteractViewModel.AgentCommand = string.Empty;
        }
    }
}