using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Agent.Modules
{
    class EnvModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController agent, ConfigController config)
        {
            Agent = agent;
            Config = config;
        }

        public AgentModuleInfo GetModuleInfo()
        {
            return new AgentModuleInfo
            {
                Name = "env",
                Description = "Environment variables",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "get",
                        Description = "Get all environment variables",
                        HelpText = "env get",
                        CallBack = GetEnvironmentVariables
                    },
                    new AgentCommand
                    {
                        Name = "set",
                        Description = "Set an environment variable",
                        HelpText = "env set [key] [value]",
                        CallBack = SetEnvironmentValue
                    },
                }
            };
        }

        private void GetEnvironmentVariables(byte[] data)
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

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void SetEnvironmentValue(byte[] data)
        {
            try
            {
                var arguments = Encoding.UTF8.GetString(data).Split(' ');
                Environment.SetEnvironmentVariable(arguments[0], arguments[1]);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}