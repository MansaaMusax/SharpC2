public class ListenerHttp : Listener
{
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
    public string BindAddress { get; set; }
    public int BindPort { get; set; }

    public ListenerHttp()
    {
        Type = ListenerType.HTTP;
        BindAddress = "0.0.0.0";
    }
}