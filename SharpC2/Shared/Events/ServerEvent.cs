using System;

public class ServerEvent
{
    public ServerEvent(ServerEventType Type, string Data, string Nick = "")
    {
        Date = DateTime.UtcNow;
        this.Type = Type;
        this.Data = Data;
        this.Nick = Nick;
    }

    public DateTime Date { get; set; }
    public ServerEventType Type { get; set; }
    public string Data { get; set; }
    public string Nick { get; set; }
}

public enum ServerEventType
{
    UserLogon,
    UserLogoff,
    FailedAuth,
    ListenerStarted,
    ListenerStopped,
    IdempotencyKeyError,
    ServerModuleRegistered,
    RosylnError
}