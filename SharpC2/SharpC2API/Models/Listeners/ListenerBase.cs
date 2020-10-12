using System;

namespace SharpC2.Listeners
{
    public class ListenerBase
    {
        public string ListenerGuid { get; set; } = Guid.NewGuid().ToString();
        public string ListenerName { get; set; }
        public ListenerType Type { get; set; }
    }

    public enum ListenerType
    {
        HTTP,
        TCP,
        SMB
    }
}