using System;

namespace Shared.Models
{
    public class ServerEvent
    {
        public DateTime Date { get; set; }
        public EventType Type { get; set; }
        public object Data { get; set; }
        public string Nick { get; set; }

        public ServerEvent(EventType Type, object Data, string Nick = "")
        {
            this.Type = Type;
            this.Data = Data;
            this.Nick = Nick;

            Date = DateTime.UtcNow;
        }

        public enum EventType
        {
            UserLogon,
            UserLogoff,
            ListenerStarted,
            ListenerStopped,
            ServerModuleRegistered
        }
    }
}