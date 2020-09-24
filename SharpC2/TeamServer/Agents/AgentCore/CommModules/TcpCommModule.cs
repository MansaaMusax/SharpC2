using System;

class TcpCommModule : ICommModule
{
    public ModuleStatus GetStatus()
    {
        throw new NotImplementedException();
    }

    public void Init(ConfigController config, CryptoController crypto)
    {
        throw new NotImplementedException();
    }

    public bool RecvData(out AgentMessage message)
    {
        throw new NotImplementedException();
    }

    public void SendData(AgentMessage message)
    {
        throw new NotImplementedException();
    }

    public void SetMetadata(AgentMetadata metadata)
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}