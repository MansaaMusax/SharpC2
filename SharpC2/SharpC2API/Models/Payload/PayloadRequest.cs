using System;

namespace SharpC2.Models
{
    public class PayloadRequest
    {
        public string ListenerId { get; set; }
        public TargetFramework TargetFramework { get; set; }
        public DateTime KillDate { get; set; } = DateTime.UtcNow.AddDays(365);
        public string SleepInterval { get; set; } = "30";
        public string SleepJitter { get; set; } = "0";
        public OutputType OutputType { get; set; }
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