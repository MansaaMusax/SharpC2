namespace Shared.Models
{
    public class HTTPChunk
    {
        public string AgentID { get; set; }
        public string ChunkID { get; set; }
        public string Data { get; set; }
        public bool Final { get; set; }
    }
}