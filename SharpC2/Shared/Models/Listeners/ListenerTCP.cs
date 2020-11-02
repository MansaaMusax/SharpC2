namespace Shared.Models
{
    public class ListenerTCP : Listener
    {
        public string BindAddress { get; set; }
        public int BindPort { get; set; }

        public ListenerTCP()
        {
            Type = ListenerType.TCP;
        }
    }
}