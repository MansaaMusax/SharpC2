using System;

namespace SharpC2.Models
{
    public class ServerEvent
    {
        public ServerEvent(ServerEventType type, string data = "")
        {
            EventTime = DateTime.UtcNow;
            EventType = type;
            Data = data;
        }

        public DateTime EventTime { get; set; }
        public ServerEventType EventType { get; set; }
        public string Data { get; set; }
    }

    public enum ServerEventType
    {
        UserLogon,
        UserLogoff,
        ListenerStarted,
        ListenerStopped,
        InitialAgent,
        IdempotencyKeyError,
        ServerModuleRegistered,
        RosylnError
    }
}