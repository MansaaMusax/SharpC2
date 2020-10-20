using System;

[Serializable]
public class AgentMetadata
{
    public string AgentID { get; set; } = "";
    public string ParentAgentID { get; set; } = "";
    public string Hostname { get; set; } = "";
    public string IPAddress { get; set; } = "";
    public string Identity { get; set; } = "";
    public string ProcessName { get; set; } = "";
    public int ProcessID { get; set; } = 0;
    public Arch Arch { get; set; } = Arch.Unknown;
    public Integrity Integrity { get; set; } = Integrity.Unknown;
    public int CLR { get; set; } = 0;
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