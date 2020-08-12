using Agent.Controllers;
using Agent.Models;

namespace Agent.Interfaces
{
    public interface IAgentModule
    {
        void Init(AgentController agent, ConfigController config);
        AgentModule GetModuleInfo();
    }
}