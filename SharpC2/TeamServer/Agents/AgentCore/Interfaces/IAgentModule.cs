using AgentCore.Controllers;
using AgentCore.Models;

namespace AgentCore.Interfaces
{
    public interface IAgentModule
    {
        void Init(AgentController agentController, ConfigController configController);
        AgentModule GetModuleInfo();
    }
}