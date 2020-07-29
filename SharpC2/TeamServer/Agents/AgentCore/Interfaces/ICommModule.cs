using AgentCore.Controllers;

using Common.Models;

namespace AgentCore.Interfaces
{
    public interface ICommModule
    {
        void Init(ConfigController config, CryptoController crypto);
        void Start();
        void SendData(AgentMessage message);
        bool RecvData(out AgentMessage message);
        ModuleStatus GetStatus();
        void Stop();
    }
}