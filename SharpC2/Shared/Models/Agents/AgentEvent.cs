using System;

namespace Shared.Models
{
    public class AgentEvent
    {
        public string AgentID { get; set; }
        public DateTime Date { get; set; }
        public EventType Type { get; set; }
        public string Data { get; set; }
        public string Nick { get; set; }

        public AgentEvent(string AgentID, EventType Type, string Data, string Nick = "")
        {
            this.AgentID = AgentID;
            this.Type = Type;
            this.Data = Data;
            this.Nick = Nick;

            Date = DateTime.UtcNow;
        }

        public enum EventType
        {
            InitialAgent,
            CommandRequest,
            AgentOutput,
            AgentError,
            ModuleRegistered
        }
    }
}