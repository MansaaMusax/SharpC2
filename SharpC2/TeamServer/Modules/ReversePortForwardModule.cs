using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using TeamServer.Controllers;
using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Modules
{
    public class ReversePortForwardModule : IServerModule
    {
        private ServerController Server { get; set; }
        private AgentController Agent { get; set; }

        public void Init(ServerController server, AgentController agent)
        {
            Server = server;
            Agent = agent;
        }

        public ServerModule GetModuleInfo()
        {
            return new ServerModule
            {
                Name = "ReversePortForward",
                Description = "Provides server-side reverse port forwarding",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" }
                },
                ServerCommands = new List<ServerCommand>
                {
                    new ServerCommand
                    {
                        Name = "DataFromAgent",
                        CallBack = DataFromAgent
                    }
                }
            };
        }

        private void DataFromAgent(AgentMetadata metadata, C2Data c2Data)
        {
            var packet = Serialisation.DeserialiseData<ReversePortForwardPacket>(c2Data.Data);

            if (!IPAddress.TryParse(packet.ForwardHost, out IPAddress ipAddress))
            {
                ipAddress = Dns.GetHostEntry(packet.ForwardHost).AddressList[0];
            }

            var endPoint = new IPEndPoint(ipAddress, packet.ForwardPort);
            var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.IP);
            
            sender.Connect(endPoint);
            sender.Send(packet.Data);

            var buffer = new byte[65535];
            var bytesRecv = sender.Receive(buffer);

            if (bytesRecv > 0)
            {
                packet.Data = buffer.TrimBytes();

                Agent.SendDataToAgent(c2Data.AgentId, "rportfwd", "DataFromTeamServer", Serialisation.SerialiseData(packet));
            }

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}