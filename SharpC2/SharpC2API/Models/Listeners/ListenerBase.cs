using Common;

namespace SharpC2.Listeners
{
    public class ListenerBase
    {
        public string ListenerId { get; set; }
        public ListenerType Type { get; set; }
        public int BindPort { get; set; }
    }

    public enum ListenerType
    {
        HTTP,
        TCP
    }
}