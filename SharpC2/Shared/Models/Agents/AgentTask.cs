using System.Collections.Generic;

namespace Shared.Models
{
    public class AgentTask
    {
        public string Alias { get; set; }
        public string Usage { get; set; }
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
    }
}