using Client.Models;

using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Client
{
    public class AgentCommands
    {
        public static string GetModuletHelpText(List<AgentModule> agentModules)
        {
            var result = new SharpC2ResultList<ModuleHelpText>
            {
                new ModuleHelpText
                {
                    Module = "core",
                    Command = "clear",
                    Description = "Clear the queued commands for this agent",
                    Usage = "clear"
                }
            };

            foreach (var module in agentModules.OrderBy(m => m.Name))
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