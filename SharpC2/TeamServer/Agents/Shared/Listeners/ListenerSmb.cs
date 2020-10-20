public class ListenerSmb : Listener
{
    public string PipeName { get; set; }

    public ListenerSmb()
    {
        Type = ListenerType.SMB;
    }
}