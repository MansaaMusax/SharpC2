public class NewListenerRequest
{
    public string Name { get; set; }
    public ListenerType Type { get; set; }
    public int BindPort { get; set; }
    public string BindAddress { get; set; }
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
    public string PipeName { get; set; }
}