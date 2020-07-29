using System;

namespace Common.Models
{
    [Serializable]
    public class AgentMessage
    {
        public string IdempotencyKey { get; set; }
        public AgentMetadata Metadata { get; set; }
        public C2Data Data { get; set; }
    }
}