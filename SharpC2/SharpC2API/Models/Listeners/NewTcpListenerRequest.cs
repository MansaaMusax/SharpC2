namespace SharpC2.Models
{
    public class NewTcpListenerRequest
    {
        public string Name { get; set; }
        public string BindAddress { get; set; }
        public int BindPort { get; set; }
    }
}