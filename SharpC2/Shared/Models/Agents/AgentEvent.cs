using System;

namespace Shared.Models
{
    public class AgentEvent
    {
        public string AgentID { get; set; }
        public DateTime Date { get; set; }
        public EventType Type { get; set; }
        public object Data { get; set; }
        public string Nick { get; set; }

        public AgentEvent(string AgentID, EventType Type, object Data = null, string Nick = "")
        {
            this.AgentID = AgentID;
            this.Type = Type;
            this.Data = Data;
            this.Nick = Nick;

            Date = DateTime.UtcNow;
        }

        public enum EventType
        {
            Stage0,
            Stage1,
            Stage2,
            InitialAgent,
            AgentCheckin,
            CommandRequest,
            AgentOutput,
            AgentError,
            ModuleRegistered
        }
    }
}