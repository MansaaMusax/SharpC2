using System;

namespace Shared.Models
{
    public class Listener
    {
        public string Name { get; set; }
        public ListenerType Type { get; set; }
        public DateTime KillDate { get; set; }

        public enum ListenerType
        {
            HTTP,
            TCP,
            SMB
        }
    }
}