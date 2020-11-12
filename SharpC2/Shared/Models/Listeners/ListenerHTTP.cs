namespace Shared.Models
{
    public class ListenerHTTP : Listener
    {
        public string BindAddress { get; set; }
        public int BindPort { get; set; }
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }
        public int SleepInterval { get; set; }
        public int SleepJitter { get; set; }

        public ListenerHTTP()
        {
            Type = ListenerType.HTTP;
            BindAddress = "0.0.0.0";
        }
    }
}