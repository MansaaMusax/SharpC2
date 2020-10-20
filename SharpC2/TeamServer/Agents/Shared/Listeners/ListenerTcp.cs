public class ListenerTcp : Listener
{
    public string BindAddress { get; set; }
    public int BindPort { get; set; }

    public ListenerTcp()
    {
        Type = ListenerType.TCP;
    }
}