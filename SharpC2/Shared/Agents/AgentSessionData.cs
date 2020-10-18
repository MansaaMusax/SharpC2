using System;
using System.Collections.Generic;

public class AgentSessionData
{
    public AgentMetadata Metadata { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public List<AgentModule> LoadModules { get; set; } = new List<AgentModule>();
    public Queue<AgentMessage> QueuedCommands { get; set; } = new Queue<AgentMessage>();
}