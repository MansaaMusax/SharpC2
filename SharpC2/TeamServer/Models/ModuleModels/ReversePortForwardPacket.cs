namespace TeamServer.Models
{
    public class ReversePortForwardPacket
    {
        public string ID { get; set; }
        public string ForwardHost { get; set; }
        public int ForwardPort { get; set; }
        public byte[] Data { get; set; }
    }
}