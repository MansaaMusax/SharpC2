using Agent.Controllers;
using Agent.Interfaces;

using Shared.Models;

using System;

namespace Agent.Comms
{
    public class SMBCommModule : ICommModule
    {
        readonly string PipeName;

        public SMBCommModule(string PipeName)
        {
            this.PipeName = PipeName;
        }

        public void Init(ConfigController Config)
        {
            throw new NotImplementedException();
        }

        public bool RecvData(out AgentMessage Message)
        {
            throw new NotImplementedException();
        }

        public void SendData(AgentMessage Message)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
