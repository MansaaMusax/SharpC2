using Agent.Models;

using System.Collections.Generic;

namespace Agent.Controllers
{
    public class ConfigController
    {
        Dictionary<AgentConfig, object> AgentConfigs = new Dictionary<AgentConfig, object>();

        public void Set(AgentConfig Config, object Value)
        {
            if (AgentConfigs.ContainsKey(Config))
            {
                AgentConfigs[Config] = Value;
            }
            else
            {
                AgentConfigs.Add(Config, Value);
            }
        }

        public T Get<T>(AgentConfig Config)
        {
            if (AgentConfigs.ContainsKey(Config))
            {
                return (T)AgentConfigs[Config];
            }
            else
            {
                return default;
            }
        }
    }
}