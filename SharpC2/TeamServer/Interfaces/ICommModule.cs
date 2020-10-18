using System;

using TeamServer.Controllers;

namespace TeamServer.Interfaces
{
    public interface ICommModule
    {
        void Init(AgentController agentController, CryptoController cryptoController);
        void Start();
        void Stop();
        bool RecvData(out Tuple<AgentMetadata, AgentMessage> data);
    }
}