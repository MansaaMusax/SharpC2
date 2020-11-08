using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;
using Shared.Utilities;

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Comms
{
    public class SMBCommModule : ICommModule
    {
        readonly string PipeName;
        readonly string ConnectHost;

        ModuleMode Mode;
        ModuleStatus Status;

        ConfigController Config;

        Queue<AgentMessage> Inbound = new Queue<AgentMessage>();
        Queue<AgentMessage> Outbound = new Queue<AgentMessage>();

        ManualResetEvent Event = new ManualResetEvent(false);

        public SMBCommModule(string PipeName)
        {
            Status = ModuleStatus.Starting;
            this.PipeName = PipeName;
            Mode = ModuleMode.Server;
        }

        public SMBCommModule(string ConnectHost, string PipeName)
        {
            Status = ModuleStatus.Starting;

            this.ConnectHost = ConnectHost;
            this.PipeName = PipeName;

            Mode = ModuleMode.Client;
        }

        public void Init(ConfigController Config)
        {
            this.Config = Config;
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

        public void SendData(AgentMessage Message)
        {
            Outbound.Enqueue(Message);
        }

        public void Start()
        {
            Status = ModuleStatus.Running;

            switch (Mode)
            {
                case ModuleMode.Client:
                    StartClient();
                    break;
                case ModuleMode.Server:
                    StartServer();
                    break;
                default:
                    break;
            }
        }

        void StartServer()
        {
            var ps = new PipeSecurity();

            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                PipeAccessRights.FullControl, AccessControlType.Allow));

            Task.Factory.StartNew(delegate ()
            {
                while (Status == ModuleStatus.Running)
                {
                    Event.Reset();
                    
                    var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 1024, 1024, ps);
                    pipe.BeginWaitForConnection(new AsyncCallback(ServerConnectCallback), pipe);
                    
                    Event.WaitOne();

                    Thread.Sleep(1000);
                }
            });
        }

        void StartClient()
        {
            Task.Factory.StartNew(delegate ()
            {
                while (Status == ModuleStatus.Running)
                {
                    Event.Reset();
                    var pipe = new NamedPipeClientStream(ConnectHost, PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                    pipe.Connect(5000);

                    var outbound = new AgentMessage();

                    if (Outbound.Count > 0)
                    {
                        outbound = Outbound.Dequeue();
                    }

                    var dataToSend = Shared.Utilities.Utilities.SerialiseData(outbound);

                    pipe.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(ClientWriteCallback), pipe);

                    Event.WaitOne();

                    Thread.Sleep(1000);
                }
            });
        }

        #region Server
        void ServerConnectCallback(IAsyncResult ar)
        {
            Event.Set();

            var pipe = ar.AsyncState as NamedPipeServerStream;
            pipe.EndWaitForConnection(ar);

            var state = new CommStateObject
            {
                Worker = pipe
            };

            pipe.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ServerReadCallback), state);
        }

        void ServerReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as CommStateObject;
            var pipe = state.Worker as NamedPipeServerStream;

            var bytesRead = 0;

            try
            {
                bytesRead = pipe.EndRead(ar);
            }
            catch { }

            if (bytesRead > 0)
            {
                var data = ServerDataJuggle(bytesRead, pipe, state);
                var messageIn = Shared.Utilities.Utilities.DeserialiseData<AgentMessage>(data);

                if (messageIn != null)
                {
                    Inbound.Enqueue(messageIn);
                }
            }

            if (Outbound.Count > 0)
            {
                var messageOut = Outbound.Dequeue();
                var serialised = Shared.Utilities.Utilities.SerialiseData(messageOut);

                pipe.BeginWrite(serialised, 0, serialised.Length, new AsyncCallback(ServerWriteCallback), pipe);
            }
        }

        void ServerWriteCallback(IAsyncResult ar)
        {
            var pipe = ar.AsyncState as NamedPipeServerStream;

            pipe.EndWrite(ar);
            pipe.Close();
        }

        byte[] ServerDataJuggle(int bytesRead, NamedPipeServerStream stream, CommStateObject state)
        {
            var final = new byte[] { };

            var dataReceived = state.Buffer.TrimBytes();

            if (bytesRead == state.Buffer.Length)
            {
                if (state.SwapBuffer != null)
                {
                    var tmp = state.SwapBuffer;
                    state.SwapBuffer = new byte[tmp.Length + dataReceived.Length];
                    Buffer.BlockCopy(tmp, 0, state.SwapBuffer, 0, tmp.Length);
                    Buffer.BlockCopy(dataReceived, 0, state.SwapBuffer, tmp.Length, dataReceived.Length);
                }
                else
                {
                    state.SwapBuffer = new byte[dataReceived.Length];
                    Buffer.BlockCopy(dataReceived, 0, state.SwapBuffer, 0, dataReceived.Length);
                }

                Array.Clear(state.Buffer, 0, state.Buffer.Length);
                stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ServerReadCallback), state);
            }
            else
            {
                if (state.SwapBuffer != null)
                {
                    final = new byte[state.SwapBuffer.Length + dataReceived.Length];
                    Buffer.BlockCopy(state.SwapBuffer, 0, final, 0, state.SwapBuffer.Length);
                    Buffer.BlockCopy(dataReceived, 0, final, state.SwapBuffer.Length, dataReceived.Length);
                }
                else
                {
                    final = new byte[dataReceived.Length];
                    Buffer.BlockCopy(dataReceived, 0, final, 0, dataReceived.Length);
                }
            }

            return final.TrimBytes();
        }
        #endregion

        #region Client
        void ClientWriteCallback(IAsyncResult ar)
        {
            Event.Set();

            var pipe = ar.AsyncState as NamedPipeClientStream;
            pipe.EndWrite(ar);

            var state = new CommStateObject
            {
                Worker = pipe
            };

            pipe.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ClientReadCallBack), state);
        }

        void ClientReadCallBack(IAsyncResult ar)
        {
            var state = ar.AsyncState as CommStateObject;
            var pipe = state.Worker as NamedPipeClientStream;

            var bytesRead = 0;

            try
            {
                bytesRead = pipe.EndRead(ar);
            }
            catch { }

            if (bytesRead > 0)
            {
                var data = ClientDataJuggle(bytesRead, pipe, state);
                var inbound = Shared.Utilities.Utilities.DeserialiseData<AgentMessage>(data);

                if (inbound != null)
                {
                    Inbound.Enqueue(inbound);
                }
            }

            pipe.Close();
        }

        byte[] ClientDataJuggle(int bytesRead, NamedPipeClientStream stream, CommStateObject state)
        {
            var final = new byte[] { };

            var dataReceived = state.Buffer.TrimBytes();

            if (bytesRead == state.Buffer.Length)
            {
                if (state.SwapBuffer != null)
                {
                    var tmp = state.SwapBuffer;
                    state.SwapBuffer = new byte[tmp.Length + dataReceived.Length];
                    Buffer.BlockCopy(tmp, 0, state.SwapBuffer, 0, tmp.Length);
                    Buffer.BlockCopy(dataReceived, 0, state.SwapBuffer, tmp.Length, dataReceived.Length);
                }
                else
                {
                    state.SwapBuffer = new byte[dataReceived.Length];
                    Buffer.BlockCopy(dataReceived, 0, state.SwapBuffer, 0, dataReceived.Length);
                }

                Array.Clear(state.Buffer, 0, state.Buffer.Length);
                stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ServerReadCallback), state);
            }
            else
            {
                if (state.SwapBuffer != null)
                {
                    final = new byte[state.SwapBuffer.Length + dataReceived.Length];
                    Buffer.BlockCopy(state.SwapBuffer, 0, final, 0, state.SwapBuffer.Length);
                    Buffer.BlockCopy(dataReceived, 0, final, state.SwapBuffer.Length, dataReceived.Length);
                }
                else
                {
                    final = new byte[dataReceived.Length];
                    Buffer.BlockCopy(dataReceived, 0, final, 0, dataReceived.Length);
                }
            }

            return final.TrimBytes();
        }
        #endregion

        public void Stop()
        {
            switch (Mode)
            {
                case ModuleMode.Client:
                    break;
                case ModuleMode.Server:
                    break;
                default:
                    break;
            }

            Status = ModuleStatus.Stopped;
        }
    }
}