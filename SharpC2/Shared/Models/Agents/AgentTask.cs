using Newtonsoft.Json;

using System.Collections.Generic;

namespace Shared.Models
{
    public class AgentTask
    {
        [JsonIgnore]
        public string Alias { get; set; }
        [JsonIgnore]
        public string Usage { get; set; }
        [JsonIgnore]
        public string Module { get; set; }
        [JsonIgnore]
        public string Command { get; set; }
        public List<Parameter> Parameters { get; set; }

        public class Parameter
        {
            public string Name { get; set; }
            public object Value { get; set; }
            [JsonIgnore]
            public ParameterType Type { get; set; }

            public enum ParameterType
            {
                String,
                Integer,
                Boolean,
                File,
                Listener,
                ShellCode
            }
        }
    }
}