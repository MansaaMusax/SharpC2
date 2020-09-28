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

    public static byte[] TrimBytes(this byte[] bytes)
    {
        var index = bytes.Length - 1;
        while (bytes[index] == 0) { index--; }
        byte[] copy = new byte[index + 1];
        Array.Copy(bytes, copy, index + 1);
        return copy;
    }
}