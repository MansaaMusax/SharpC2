using Agent.Models;

using System.Collections.Generic;

namespace Agent.Controllers
{
    public class ConfigController
    {
        private Dictionary<ConfigSetting, object> ConfigSettings { get; set; } = new Dictionary<ConfigSetting, object>();

        public void SetOption(ConfigSetting setting, object value)
        {
            if (ConfigSettings.ContainsKey(setting))
            {
                ConfigSettings[setting] = value;
            }
            else
            {
                AddOption(setting, value);
            }
        }

        private void AddOption(ConfigSetting setting, object value)
        {
            ConfigSettings.Add(setting, value);
        }

        public object GetOption(ConfigSetting setting)
        {
            if (ConfigSettings.ContainsKey(setting))
            {
                return ConfigSettings[setting];
            }

            return null;
        }
    }
}