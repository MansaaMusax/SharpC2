interface ICommModule
{
    void Init(ConfigController config, CryptoController crypto);
    void SetMetadata(AgentMetadata metadata);
    void Start();
    void Stop();
    ModuleStatus GetStatus();
    void SendData(AgentMessage message);
    bool RecvData(out AgentMessage message);
}