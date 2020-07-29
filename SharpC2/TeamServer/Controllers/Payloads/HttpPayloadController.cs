using Microsoft.CodeAnalysis;

using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.IO;

using TeamServer.Agents;

namespace TeamServer.Controllers
{
    public class HttpPayloadController : PayloadControllerBase
    {
        private ListenerHttp Listener { get; set; }
        private string TempPath { get; set; }

        public HttpPayloadController(ListenerHttp listenerHttp)
        {
            Listener = listenerHttp;
        }

        public byte[] GenerateHttpPayload(PayloadRequest request)
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
                        File = "System.Net.dll",
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
            InsertConnectAddresses();
            InsertConnectPort();
            InsertKillDate(request.KillDate);
            InsertSleepInterval(request.SleepInterval);
            InsertSleepJitter(request.SleepJitter);
            InsertCryptoKey(Convert.ToBase64String(Program.ServerController.CryptoController.EncryptionKey));

            var result = Compiler.Compile(compilerRequest);

            RemoveTempDirectory(TempPath);

            return result;
        }

        private void InsertConnectAddresses()
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "Agent.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<ConnectHost>>", Listener.ConnectAddress);
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertConnectPort()
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "Agent.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<ConnectPort>>", Listener.ConnectPort.ToString());
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertKillDate(DateTime killDate)
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "Agent.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<KillDate>>", killDate.ToString());
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertSleepInterval(string interval)
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "Agent.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("\"<<SleepInterval>>\"", interval);
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertSleepJitter(string jitter)
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "Agent.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("\"<<SleepJitter>>\"", jitter);
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