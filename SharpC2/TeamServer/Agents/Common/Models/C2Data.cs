using System;

namespace Common.Models
{
    [Serializable]
    public class C2Data
    {
        public string AgentID { get; set; }
        public string Module { get; set; }
        public string Command { get; set; }
        public byte[] Data { get; set; }
    }
}