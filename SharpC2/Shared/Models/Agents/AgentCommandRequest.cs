namespace Shared.Models
{
    public class AgentCommandRequest
    {
        public string AgentID { get; set; }
        public string Module { get; set; }
        public string Command { get; set; }
        public TaskDefinition Task { get; set; }
    }
}