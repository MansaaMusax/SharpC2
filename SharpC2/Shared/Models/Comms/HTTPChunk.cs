namespace Shared.Models
{
    public class HTTPChunk
    {
        public byte Data { get; set; }
        public bool Final { get; set; } = false;
    }
}