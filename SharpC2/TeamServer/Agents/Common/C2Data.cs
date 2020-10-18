using System;

[Serializable]
public class C2Data
{
    public string AgentId { get; set; }
    public string Module { get; set; }
    public string Command { get; set; }
    public byte[] Data { get; set; }
}