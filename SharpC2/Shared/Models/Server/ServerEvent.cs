using System;

namespace Shared.Models
{
    public class ServerEvent
    {
        public DateTime Date { get; set; }
        public EventType Type { get; set; }
        public string Data { get; set; }
        public string Nick { get; set; }

        public ServerEvent(EventType Type, string Data, string Nick = "")
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
            FailedAuth,
            ListenerStarted,
            ListenerStopped,
            ServerModuleRegistered
        }
    }
}