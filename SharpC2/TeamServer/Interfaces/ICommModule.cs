using Common.Models;

using System;
using System.Collections.Generic;

using TeamServer.Controllers;

namespace TeamServer.Interfaces
{
    public interface ICommModule
    {
        void Init(AgentController agentController, CryptoController cryptoController);
        void Start();
        void Stop();
        bool RecvData(out Tuple<AgentMetadata, List<AgentMessage>> data);
    }
}