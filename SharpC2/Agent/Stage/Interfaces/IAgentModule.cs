using Agent.Controllers;
using Agent.Models;

namespace Agent.Interfaces
{
    public interface IAgentModule
    {
        void Init(AgentController Agent, ConfigController Config);
        ModuleInfo GetModuleInfo();
    }
}