namespace Shared.Models
{
    public class ListenerSMB : Listener
    {
        public string PipeName { get; set; }

        public ListenerSMB()
        {
            Type = ListenerType.SMB;
        }
    }
}