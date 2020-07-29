namespace SharpC2.Models
{
    public class NewHttpListenerRequest
    {
        public string Name { get; set; }
        public int BindPort { get; set; }
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }
    }
}