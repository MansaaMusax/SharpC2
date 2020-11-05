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

        public static string ConvertFileLength(long size)
        {
            var result = size.ToString();

            if (size < 1024) { result = $"{size}b"; }
            else if (size > 1024 && size <= 1048576) { result = $"{size / 1024}kb"; }
            else if (size > 1048576 && size <= 1073741824) { result = $"{size / 1048576}mb"; }
            else if (size > 1073741824 && size <= 1099511627776) { result = $"{size / 1073741824}gb"; }
            else if (size > 1099511627776) { result = $"{size / 1099511627776}tb"; }

            return result;
        }
    }
}