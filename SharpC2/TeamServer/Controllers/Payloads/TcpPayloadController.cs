using Microsoft.CodeAnalysis;

using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.IO;

using TeamServer.Agents;

namespace TeamServer.Controllers
{
    public class TcpPayloadController : PayloadControllerBase
    {
        private ListenerTcp Listener { get; set; }
        private string TempPath { get; set; }

        public TcpPayloadController(ListenerTcp listenerTcp)
        {
            Listener = listenerTcp;
        }

        public byte[] GenerateTcpPayload(PayloadRequest request)
        {
            TempPath = CreateTempDirectory();

            var compilerRequest = new Compiler.CompilationRequest
            {
                AssemblyName = "Agent",
                OutputKind = (OutputKind)request.OutputType,
                Platform = Platform.AnyCpu,
                ReferenceDirectory = request.TargetFramework == TargetFramework.Net35 ? ReferencesDirectory + Path.DirectorySeparatorChar + "net35" : ReferencesDirectory + Path.DirectorySeparatorChar + "net40",
                TargetDotNetVersion = (Compiler.DotNetVersion)request.TargetFramework,
                SourceDirectory = TempPath,
                References = new List<Compiler.Reference>
                {
                    new Compiler.Reference
                    {
                        File = "mscorlib.dll",
                        Framework = (Compiler.DotNetVersion)request.TargetFramework,
                        Enabled = true
                    },
                    new Compiler.Reference
                    {
                        File = "System.dll",
                        Framework = (Compiler.DotNetVersion)request.TargetFramework,
                        Enabled = true
                    },
                    new Compiler.Reference
                    {
                        File = "System.Core.dll",
                        Framework = (Compiler.DotNetVersion)request.TargetFramework,
                        Enabled = true
                    },
                    new Compiler.Reference
                    {
                        File = "System.XML.dll",
                        Framework = (Compiler.DotNetVersion)request.TargetFramework,
                        Enabled = true
                    },
                    new Compiler.Reference
                    {
                        File = "System.Runtime.Serialization.dll",
                        Framework = (Compiler.DotNetVersion)request.TargetFramework,
                        Enabled = true
                    }
                }
            };

            CloneAgentSourceCode(Listener.Type, TempPath);
            InsertBindAddress();
            InsertBindPort();
            InsertKillDate(request.KillDate);
            InsertCryptoKey(Convert.ToBase64String(Program.ServerController.CryptoController.EncryptionKey));

            var result = Compiler.Compile(compilerRequest);

            RemoveTempDirectory(TempPath);

            return result;
        }

        private void InsertBindAddress()
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "Agent.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<BindAddress>>", Listener.BindAddress);
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertBindPort()
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "Agent.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("\"<<BindPort>>\"", Listener.BindPort.ToString());
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertKillDate(DateTime killDate)
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "Agent.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<KillDate>>", killDate.ToString());
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertCryptoKey(string key)
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "CryptoController.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<EncKey>>", key);
            File.WriteAllText(srcPath, newSrc);
        }
    }
}