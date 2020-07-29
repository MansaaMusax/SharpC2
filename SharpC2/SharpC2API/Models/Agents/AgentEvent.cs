using System;

namespace SharpC2.Models
{
    public class AgentEvent
    {
        public AgentEvent(string agentId, AgentEventType type, string data)
        {
            EventTime = DateTime.UtcNow;
            EventType = type;
            AgentId = agentId;
            Data = data;
        }

        public DateTime EventTime { get; set; }
        public string AgentId { get; set; }
        public AgentEventType EventType { get; set; }
        public string Data { get; set; }
    }

    public enum AgentEventType
    {
        ModuleRegistered,
        CommandRequest,
        CommandResponse,
        AgentError,
        CryptoError
    }
}