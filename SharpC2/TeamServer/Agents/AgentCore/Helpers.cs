using Common.Models;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace AgentCore
{
    public static class Helpers
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

        public static bool FileHasValidSignature(string FilePath)
        {
            X509Certificate2 FileCertificate;
            try
            {
                X509Certificate signer = X509Certificate.CreateFromSignedFile(FilePath);
                FileCertificate = new X509Certificate2(signer);
            }
            catch
            {
                return false;
            }

            X509Chain CertificateChain = new X509Chain();
            CertificateChain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            CertificateChain.ChainPolicy.RevocationMode = X509RevocationMode.Offline;
            CertificateChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            return CertificateChain.Build(FileCertificate);
        }
    }
}