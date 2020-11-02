namespace Shared.Models
{
    public class AgentMessage
    {
        public string AgentID { get; set; }
        public byte[] Data { get; set; }
        public byte[] IV { get; set; }
    }

    public class C2Data
    {
        public string Module { get; set; }
        public string Command { get; set; }
        public byte[] Data { get; set; }
    }
}