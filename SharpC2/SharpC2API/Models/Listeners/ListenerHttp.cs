namespace SharpC2.Listeners
{
    public class ListenerHttp : ListenerBase
    {
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }
        public int BindPort { get; set; }
    }
}