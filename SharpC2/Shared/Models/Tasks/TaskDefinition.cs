using Newtonsoft.Json;

using System.Collections.Generic;

namespace Shared.Models
{
    public class TaskDefinition
    {
        public string Alias { get; set; }

        [JsonIgnore]
        public string Description { get; set; }

        [JsonIgnore]
        public string Usage { get; set; }
        public OpsecStyle OpSec { get; set; }
        public string Module { get; set; }
        public string Command { get; set; }
        public List<Parameter> Parameters { get; set; }

        public class Parameter
        {
            public string Name { get; set; }
            public object Value { get; set; }
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

        public enum OpsecStyle
        {
            NA,
            Inline,
            ForkAndRun
        }
    }
}