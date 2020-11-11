using System.Collections.Generic;

namespace Shared.Models
{
    public class AgentTask
    {
        public string Alias { get; set; }
        public string Module { get; set; }
        public string Command { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}