using System;

public class AgentEvent
{
    public AgentEvent(string AgentId, AgentEventType Type, string Data = "", string Nick = "")
    {
        Date = DateTime.UtcNow;
        this.Type = Type;
        this.AgentId = AgentId;
        this.Data = Data;
        this.Nick = Nick;
    }

    public DateTime Date { get; set; }
    public string AgentId { get; set; }
    public AgentEventType Type { get; set; }
    public string Data { get; set; }
    public string Nick { get; set; }
}

public enum AgentEventType
{
    InitialAgent,
    ModuleRegistered,
    CommandRequest,
    CommandResponse,
    AgentError,
    CryptoError
}