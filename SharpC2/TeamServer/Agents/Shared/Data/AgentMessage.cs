using System;

[Serializable]
public class AgentMessage
{
    public string IdempotencyKey { get; set; }
    public AgentMetadata Metadata { get; set; }
    public C2Data Data { get; set; }

    public AgentMessage()
    {
        IdempotencyKey = Guid.NewGuid().ToString();
        Metadata = new AgentMetadata();
        Data = new C2Data();
    }
}