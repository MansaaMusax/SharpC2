using System;

public class StagerRequest
{
    public string Listener { get; set; }

    public string SleepInterval { get; set; }
    public string SleepJitter { get; set; }

    public TargetFramework TargetFramework { get; set; }
    public OutputType OutputType { get; set; }
    public DateTime KillDate { get; set; }
}

public class StageRequest
{
    public TargetFramework TargetFramework { get; set; }
}

public enum TargetFramework
{
    NET40
}

public enum OutputType
{
    EXE = 0,
    DLL = 2
}