using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;
using Shared.Utilities;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Comms
{
    public class TCPCommModule : ICommModule
    {
        ModuleMode Mode;
        ModuleStatus Status;

        TcpListener Listener;

        readonly string Hostname;
        readonly int Port;

        ConfigController Config;
        AgentMetadata Metadata;

        Queue<AgentMessage> Inbound = new Queue<AgentMessage>();
        Queue<AgentMessage> Outbound = new Queue<AgentMessage>();

        ManualResetEvent Event = new ManualResetEvent(false);

        public TCPCommModule(string Hostname, int Port)
        {
            Status = ModuleStatus.Starting;
            
            this.Hostname = Hostname;
            this.Port = Port;
            
            Mode = ModuleMode.Client;
        }

        public TCPCommModule(IPAddress Address, int Port)
        {
            Status = ModuleStatus.Starting;
            Listener = new TcpListener(new IPEndPoint(Address, Port));
            Mode = ModuleMode.Server;
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

        public void Stop()
        {
            switch (Mode)
            {
                case ModuleMode.Client:
                    break;
                case ModuleMode.Server:
                    Listener.Stop();
                    break;
                default:
                    break;
            }

            Status = ModuleStatus.Stopped;
        }

        void StartServer()
        {
            Listener.Start();

            Task.Factory.StartNew(delegate ()
            {
                while (Status == ModuleStatus.Running)
                {
                    Event.Reset();
                    Listener.BeginAcceptTcpClient(new AsyncCallback(ServerAcceptCallback), Listener);
                    Event.WaitOne();
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

                    var client = new TcpClient();
                    client.BeginConnect(Hostname, Port, new AsyncCallback(ClientConnectCallback), client);
                    Event.WaitOne();

                    Thread.Sleep(1000);
                }
            });
        }

        #region Server
        void ServerAcceptCallback(IAsyncResult ar)
        {
            Event.Set();

            var listener = ar.AsyncState as TcpListener;

            if (Status == ModuleStatus.Running)
            {
                var handler = Listener.EndAcceptTcpClient(ar);
                var stream = handler.GetStream();
                var state = new CommStateObject { Worker = stream };

                stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ServerReadCallback), state);
            }
        }

        void ServerReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as CommStateObject;
            var stream = state.Worker as NetworkStream;

            var bytesRead = 0;

            try
            {
                bytesRead = stream.EndRead(ar);
            }
            catch { }

            if (bytesRead > 0)
            {
                var data = DataJuggle(bytesRead, stream, state);

                var inbound = Shared.Utilities.Utilities.DeserialiseData<AgentMessage>(data);

                if (inbound != null)
                {
                    Inbound.Enqueue(inbound);
                }

                var outbound = new AgentMessage();

                if (Outbound.Count > 0)
                {
                    outbound = Outbound.Dequeue();
                }

                var serialised = Shared.Utilities.Utilities.SerialiseData(outbound);
                stream.BeginWrite(serialised, 0, serialised.Length, new AsyncCallback(ServerWriteCallback), stream);
            }
        }

        void ServerWriteCallback(IAsyncResult ar)
        {
            var stream = ar.AsyncState as NetworkStream;

            stream.EndWrite(ar);
            stream.Close();
        }
        #endregion

        #region Client
        void ClientConnectCallback(IAsyncResult ar)
        {
            Event.Set();

            var client = ar.AsyncState as TcpClient;

            try
            {
                client.EndConnect(ar);

                var stream = client.GetStream();

                var outbound = new AgentMessage();

                if (Outbound.Count > 0)
                {
                    outbound = Outbound.Dequeue();
                }

                var serialised = Shared.Utilities.Utilities.SerialiseData(outbound);
                var state = new CommStateObject { Handler = client, Worker = stream };
                stream.BeginWrite(serialised, 0, serialised.Length, new AsyncCallback(ClientWriteCallback), state);
            }
            catch
            {
                // Agent has probably been closed or killed
                Status = ModuleStatus.Terminated;
            }
        }

        void ClientWriteCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as CommStateObject;
            var stream = state.Worker as NetworkStream;

            stream.EndWrite(ar);
            stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ClientReadCallback), state);
        }

        void ClientReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as CommStateObject;
            var client = state.Handler as TcpClient;
            var stream = state.Worker as NetworkStream;

            var bytesRead = 0;

            try
            {
                bytesRead = stream.EndRead(ar);
            }
            catch { }

            if (bytesRead > 0)
            {
                var data = DataJuggle(bytesRead, stream, state);

                var inbound = Shared.Utilities.Utilities.DeserialiseData<AgentMessage>(data);

                if (inbound != null && inbound.Data != null)
                {
                    Inbound.Enqueue(inbound);
                }
            }

            stream.Close();
            client.Close();
        }

        #endregion

        byte[] DataJuggle(int bytesRead, NetworkStream stream, CommStateObject state)
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
    }
}