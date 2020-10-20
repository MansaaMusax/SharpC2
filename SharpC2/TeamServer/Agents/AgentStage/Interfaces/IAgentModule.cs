public interface IAgentModule
{
    void Init(AgentController agent, ConfigController config);
    AgentModuleInfo GetModuleInfo();
}