using TeamServer.Controllers;
using TeamServer.Models;

namespace TeamServer.Interfaces
{
    public interface IServerModule
    {
        void Init(ServerController server, AgentController agent);
        ServerModule GetModuleInfo();
    }
}