public class Listener
{
    public string Name { get; set; }
    public ListenerType Type { get; set; }
}

public enum ListenerType
{
    HTTP,
    TCP,
    SMB
}