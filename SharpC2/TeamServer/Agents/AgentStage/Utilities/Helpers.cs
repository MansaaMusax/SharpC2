using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

static class Helpers
{
    public static string GetHostname
    {
        get
        {
            return Dns.GetHostName();
        }
    }

    public static string GetIpAddress
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

    public static int GetProcessId
    {
        get
        {
            return Process.GetCurrentProcess().Id;
        }
    }

    public static Arch GetArch
    {
        get
        {
            return IntPtr.Size == 8 ? Arch.x64 : Arch.x86;
        }
    }

    public static Integrity GetIntegrity
    {
        get
        {
            var integrity = Integrity.Medium;
            var identity = WindowsIdentity.GetCurrent();
            if (Environment.UserName.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
            {
                integrity = Integrity.SYSTEM;
            }
            else if (identity.User != identity.Owner)
            {
                integrity = Integrity.High;
            }
            return integrity;
        }
    }

    public static int GetCLRVersion
    {
        get
        {
            return Environment.Version.Major;
        }
    }

    public static string GetCurrentDirectory
    {
        get
        {
            return Directory.GetCurrentDirectory();
        }
    }
}