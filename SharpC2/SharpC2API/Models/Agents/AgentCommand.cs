using System;

namespace SharpC2.Models
{
    [Serializable]
    public class AgentCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string HelpText { get; set; }
        public bool Visible { get; set; }
    }
}