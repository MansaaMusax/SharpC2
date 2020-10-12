using SharpC2.Listeners;

namespace Client.Models
{
    public class Listener
    {
        public string ListenerName { get; set; }
        public string ListenerGuid { get; set; }
        public ListenerType ListenerType { get; set; }
    }
}