namespace SharpC2.Listeners
{
    public class ListenerTcp : ListenerBase
    {
        public string BindAddress { get; set; }
        public int BindPort { get; set; }
    }
}