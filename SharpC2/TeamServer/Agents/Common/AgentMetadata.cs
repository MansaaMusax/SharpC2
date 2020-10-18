using System;

[Serializable]
public class AgentMetadata
{
    public string AgentID { get; set; }
    public string ParentAgentID { get; set; }
    public string Hostname { get; set; }
    public string IPAddress { get; set; }
    public string Identity { get; set; }
    public string ProcessName { get; set; }
    public int ProcessID { get; set; }
    public Arch Arch { get; set; }
    public Integrity Integrity { get; set; }
    public int CLR { get; set; }
}

public enum Arch
{
    Unknown,
    x64,
    x86
}

public enum Integrity
{
    Unknown,
    Medium,
    High,
    SYSTEM
}