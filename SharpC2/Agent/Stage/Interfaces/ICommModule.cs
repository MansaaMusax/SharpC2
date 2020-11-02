using Agent.Controllers;

using Shared.Models;

namespace Agent.Interfaces
{
    public interface ICommModule
    {
        void Init(ConfigController Config);
        void Start();
        void Stop();
        void SendData(AgentMessage Message);
        bool RecvData(out AgentMessage Message);
    }
}