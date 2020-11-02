using Shared.Models;

using TeamServer.Controllers;

namespace TeamServer.Interfaces
{
    public interface ICommModule
    {
        void Init(AgentController Agent);
        void Start();
        void Stop();
        bool RecvData(out AgentMessage Message);
    }
}