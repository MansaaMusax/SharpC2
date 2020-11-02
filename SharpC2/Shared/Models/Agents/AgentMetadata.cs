using System;

namespace Shared.Models
{
    public class AgentMetadata
    {
        public string AgentID { get; set; }
        public string IPAddress { get; set; }
        public string Hostname { get; set; }
        public string Identity { get; set; }
        public string Process { get; set; }
        public int PID { get; set; }
        public Architecture Arch { get; set; }
        public Integrity Elevation { get; set; }
        public DateTime LastSeen { get; set; }

        public enum Architecture
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
    }
}