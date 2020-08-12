using Common;
using Common.Models;

using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using TeamServer.Controllers;
using TeamServer.Interfaces;

namespace TeamServer.Modules
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public byte[] data = null;
    }

    public class HttpCommModule : ICommModule
    {
        public ListenerHttp Listener { get; set; }
        private AgentController AgentController { get; set; }
        private CryptoController CryptoController { get; set; }
        private Socket Socket { get; set; }
        public ModuleStatus ModuleStatus { get; private set; }
        private Queue<Tuple<AgentMetadata, List<AgentMessage>>> InboundQueue { get; set; } = new Queue<Tuple<AgentMetadata, List<AgentMessage>>>();
        public List<WebLog> WebLogs { get; private set; } = new List<WebLog>();

        private event EventHandler<AgentEvent> OnAgentEvent;

        private static ManualResetEvent AllDone = new ManualResetEvent(false);

        public void Init(AgentController agentController, CryptoController cryptoController)
        {
            ModuleStatus = ModuleStatus.Starting;
            AgentController = agentController;
            CryptoController = cryptoController;

            Socket = new Socket(SocketType.Stream, ProtocolType.IP);

            OnAgentEvent += AgentController.AgentEventHandler;
        }

        public void Start()
        {
            ModuleStatus = ModuleStatus.Running;

            try
            {
                Socket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), Listener.BindPort));
                Socket.Listen(100);

                Task.Factory.StartNew(delegate ()
                {
                    while (ModuleStatus == ModuleStatus.Running)
                    {
                        AllDone.Reset();
                        Socket.BeginAccept(new AsyncCallback(AcceptCallback), Socket);
                        AllDone.WaitOne();
                    }
                });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();

            var listener = ar.AsyncState as Socket;

            if (ModuleStatus == ModuleStatus.Running)
            {
                var handler = listener.EndAccept(ar);
                var state = new StateObject { workSocket = handler };
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as StateObject;
            var handler = state.workSocket;
            var bytesRead = 0;

            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (SocketException)
            {
                // client socket has gone away for "reasons".
            }

            if (bytesRead > 0)
            {
                var dataReceived = state.buffer.TrimBytes();
                var webRequest = Encoding.UTF8.GetString(dataReceived);

                if (webRequest.Contains("Expect: 100-continue") || dataReceived.Length == state.buffer.Length)
                {
                    if (state.data != null)
                    {
                        var tmp = state.data;
                        state.data = new byte[tmp.Length + dataReceived.Length];
                        Buffer.BlockCopy(tmp, 0, state.data, 0, tmp.Length);
                        Buffer.BlockCopy(dataReceived, 0, state.data, tmp.Length, dataReceived.Length);
                    }
                    else
                    {
                        state.data = new byte[dataReceived.Length];
                        Buffer.BlockCopy(dataReceived, 0, state.data, 0, dataReceived.Length);
                    }

                    Array.Clear(state.buffer, 0, state.buffer.Length);
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    byte[] final;
                    if (state.data != null)
                    {
                        final = new byte[state.data.Length + dataReceived.Length];
                        Buffer.BlockCopy(state.data, 0, final, 0, state.data.Length);
                        Buffer.BlockCopy(dataReceived, 0, final, state.data.Length, dataReceived.Length);
                    }
                    else
                    {
                        final = new byte[dataReceived.Length];
                        Buffer.BlockCopy(dataReceived, 0, final, 0, dataReceived.Length);
                    }
                    
                    var finalRequest = Encoding.UTF8.GetString(final);

                    var agentMetadata = ExtractAgentMetadata(finalRequest);
                    if (agentMetadata != null)
                    {
                        var agentMessages = ExtractAgentMessage(finalRequest);

                        if (agentMessages != null)
                        {
                            var tuple = new Tuple<AgentMetadata, List<AgentMessage>>(agentMetadata, agentMessages);
                            InboundQueue.Enqueue(tuple);

                            var agentTasks = GetAgentTasks(agentMetadata.AgentID);
                            SendData(handler, agentTasks);
                        }
                    }
                    else
                    {
                        GenerateWebLog(webRequest, handler.RemoteEndPoint as IPEndPoint);
                    }
                }
            }
        }

        private void SendData(Socket handler, List<AgentMessage> messages)
        {
            var encrypted = CryptoController.Encrypt(messages);

            var response = new StringBuilder("HTTP/1.1 200 OK\r\n");
            response.Append(string.Format("X-Malware: SharpC2\r\n"));
            response.Append(string.Format("Content-Length: {0}\r\n", encrypted.Length));
            response.Append(string.Format("Date: {0}\r\n", DateTime.UtcNow.ToString("ddd, d MMM yyyy HH:mm:ss UTC")));
            response.Append("\r\n");

            var headers = Encoding.UTF8.GetBytes(response.ToString());
            var dataToSend = new byte[encrypted.Length + headers.Length];

            Buffer.BlockCopy(headers, 0, dataToSend, 0, headers.Length);
            Buffer.BlockCopy(encrypted, 0, dataToSend, headers.Length, encrypted.Length);

            try
            {
                handler.BeginSend(dataToSend, 0, dataToSend.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch
            {
                // socket may be forcibly closed if agent dies
            }
        }

        private List<AgentMessage> GetAgentTasks(string agentId)
        {
            var tasks = new List<AgentMessage>();
            var agent = AgentController.GetSession(agentId);

            if (agent != null)
            {
                if (agent.QueuedCommands.Count > 0)
                {
                    while (agent.QueuedCommands.Count != 0)
                    {
                        tasks.Add(agent.QueuedCommands.Dequeue());
                    }
                }
                else
                {
                    tasks.Add(new AgentMessage
                    {
                        IdempotencyKey = Guid.NewGuid().ToString(),
                        Metadata = new AgentMetadata { AgentID = agentId },
                        Data = new C2Data { Module = "Core", Command = "NOP" }
                    });
                }
            }

            return tasks;
        }

        private void GenerateWebLog(string webRequest, IPEndPoint remoteEndPoint)
        {
            WebLogs.Add(new WebLog
            {
                ListenerId = Listener.ListenerName,
                Origin = remoteEndPoint.Address.ToString(),
                WebRequest = webRequest.Replace("\0", "")
            });
        }

        private AgentMetadata ExtractAgentMetadata(string webRequest)
        {
            AgentMetadata metadata = null;

            var regex = Regex.Match(webRequest, "Cookie: Metadata=([^\\s].*)");
            if (regex.Captures.Count > 0)
            {
                var encrypted = Convert.FromBase64String(regex.Groups[1].Value);

                if (CryptoController.VerifyHMAC(encrypted))
                {
                    metadata = CryptoController.Decrypt<AgentMetadata>(encrypted);
                }
                else
                {
                    OnAgentEvent?.Invoke(this, new AgentEvent("", AgentEventType.CryptoError, "HMAC validation failed on AgentMetadata"));
                }
            }

            return metadata;
        }

        private List<AgentMessage> ExtractAgentMessage(string webRequest)
        {
            List<AgentMessage> message = null;

            var regex = Regex.Match(webRequest, "Message=([^\\s]+)");

            if (regex.Captures.Count > 0)
            {
                var encrypted = Convert.FromBase64String(regex.Groups[1].Value);

                if (CryptoController.VerifyHMAC(encrypted))
                {
                    message = CryptoController.Decrypt<List<AgentMessage>>(encrypted);
                }
                else
                {
                    OnAgentEvent?.Invoke(this, new AgentEvent("", AgentEventType.CryptoError, "HMAC validation failed on AgentMessage"));
                }
                
            }

            return message;
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = ar.AsyncState as Socket;
                var bytesSent = handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (SocketException e)
            {
                throw new Exception(e.Message);
            }
        }

        public void Stop()
        {
            ModuleStatus = ModuleStatus.Stopped;
            Socket.Close();
        }

        public bool RecvData(out Tuple<AgentMetadata, List<AgentMessage>> data)
        {
            if (InboundQueue.Count > 0)
            {
                data = InboundQueue.Dequeue();
                return true;
            }

            data = null;
            return false;
        }
    }
}