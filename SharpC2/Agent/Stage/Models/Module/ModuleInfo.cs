using Agent.Controllers;

using System.Collections.Generic;

namespace Agent.Models
{
    public class ModuleInfo
    {
        public string Name { get; set; }
        public List<Command> Commands { get; set; }

        public class Command
        {
            public string Name { get; set; }
            public AgentController.AgentCommand Delegate { get; set; }
        }
    }
}