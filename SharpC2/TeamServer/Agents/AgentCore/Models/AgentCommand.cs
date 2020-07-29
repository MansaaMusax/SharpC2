using System;

using AgentCore.Controllers;

namespace AgentCore.Models
{
    [Serializable]
    public class AgentCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string HelpText { get; set; }
        public bool Visible { get; set; } = true;

        [NonSerialized] private AgentController.OnAgentCommand _callback;

        public AgentController.OnAgentCommand CallBack {
            get { return _callback; }
            set { _callback = value; }
        }
    }
}