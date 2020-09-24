using System;

namespace SharpC2.Models
{
    public class PayloadRequest
    {
        public TargetFramework TargetFramework { get; set; } = TargetFramework.Net40;
        public OutputType OutputType { get; set; } = OutputType.Dll;
        public DateTime KillDate { get; set; } = DateTime.UtcNow.AddDays(365);
    }

    public class HttpPayloadRequest : PayloadRequest
    {
        public string ListenerGuid { get; set; }
        public string SleepInterval { get; set; } = "30";
        public string SleepJitter { get; set; } = "0";
    }

    public class TcpPayloadRequest : PayloadRequest
    {
        public string ListenerGuid { get; set; }
    }

    public class SmbPayloadRequest : PayloadRequest
    {
        public string ListenerGuid { get; set; }
    }

    public class StageRequest
    {
        public TargetFramework TargetFramework { get; set; }
    }

    public enum TargetFramework
    {
        Net35,
        Net40
    }

    public enum OutputType
    {
        Exe = 0,
        Dll = 2
    }
}