using Common;

using Microsoft.CodeAnalysis;

using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using TeamServer.Agents;

namespace TeamServer.Controllers
{
    public class PayloadControllerBase
    {
        public static string RootDirectory { get; set; } = Assembly.GetExecutingAssembly().Location.Split("TeamServer")[0];
        public static string AgentDirectory { get; set; } = Path.Combine(RootDirectory, "TeamServer", "Agents");
        public static string ReferencesDirectory { get; set; } = Path.Combine(AgentDirectory, "References");

        public static byte[] GenerateHttpAgent(HttpPayloadRequest request)
        {
            var listener = GetListener(request.ListenerGuid);
            var controller = new HttpPayloadController(listener as ListenerHttp);
            return controller.GenerateAgentStager(request);
        }

        public static byte[] GenerateTcpAgent(TcpPayloadRequest request)
        {
            var listener = GetListener(request.ListenerGuid);
            var controller = new TcpPayloadController(listener as ListenerTcp);
            return controller.GenerateStager(request);
        }

        public static byte[] GenerateSmbAgent(SmbPayloadRequest request)
        {
            var listener = GetListener(request.ListenerGuid);
            var controller = new SmbPayloadController(listener as ListenerSmb);
            return controller.GenerateStager(request);
        }

        public static byte[] GenerateStageOne(StageRequest request)
        {
            var tmpPath = CreateTempDirectory();
            var compilerRequest = new Compiler.CompilationRequest
            {
                AssemblyName = "AgentStage",
                OutputKind = OutputKind.DynamicallyLinkedLibrary,
                Platform = Platform.AnyCpu,
                ReferenceDirectory = request.TargetFramework == TargetFramework.Net35 ? ReferencesDirectory + Path.DirectorySeparatorChar + "net35" : ReferencesDirectory + Path.DirectorySeparatorChar + "net40",
                TargetDotNetVersion = (Compiler.DotNetVersion)request.TargetFramework,
                SourceDirectory = tmpPath,
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
                    },
                    new Compiler.Reference
                    {
                        File = "System.Management.Automation.dll",
                        Framework = (Compiler.DotNetVersion)request.TargetFramework,
                        Enabled = true
                    }
                }
            };
            
            CloneStageOneSourceCode(tmpPath);

            return Compiler.Compile(compilerRequest);
        }

        private static ListenerBase GetListener(string listenerGuid)
        {
            return Program.ServerController.ListenerController.GetListener(listenerGuid);
        }

        protected static string CreateTempDirectory()
        {
            var temp = Path.GetTempPath() + Helpers.GeneratePseudoRandomString(6);
            Directory.CreateDirectory(temp);
            return temp;
        }

        protected static void CloneAgentStagerSourceCode(ListenerType listenerType, string tempPath)
        {
            var srcPath = default(string);

            switch (listenerType)
            {
                case ListenerType.HTTP:
                    srcPath = Path.Combine(AgentDirectory, "HTTPStager");
                    break;
                case ListenerType.TCP:
                    srcPath = Path.Combine(AgentDirectory, "TCPStager");
                    break;
                case ListenerType.SMB:
                    srcPath = Path.Combine(AgentDirectory, "SMBStager");
                    break;
                default:
                    break;
            }

            // AgentType
            var srcFiles = Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories);

            foreach (var filePath in srcFiles)
            {
                if (filePath.Contains("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) ||
                    filePath.Contains("AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase)) { continue; }
                var fileName = Path.GetFileName(filePath);
                var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                File.Copy(filePath, finalPath, true);
            }

            // Stager Core
            srcPath = Path.Combine(AgentDirectory, "StagerCore");

            foreach (var filePath in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(filePath);
                var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;

                if (File.Exists(finalPath))
                {
                    fileName = fileName.Insert(fileName.Length - 3, Helpers.GeneratePseudoRandomString(6));
                    finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                }

                File.Copy(filePath, finalPath, true);
            }

            // Common
            //srcPath = Path.Combine(AgentDirectory, "Common");

            //foreach (var filePath in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
            //{
            //    var fileName = Path.GetFileName(filePath);
            //    var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;

            //    if (File.Exists(finalPath))
            //    {
            //        fileName = fileName.Insert(fileName.Length - 3, Helpers.GeneratePseudoRandomString(6));
            //        finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
            //    }

            //    File.Copy(filePath, finalPath, true);
            //}
        }

        protected static void CloneStageOneSourceCode(string tempPath)
        {
            // AgentStage
            var srcPath = Path.Combine(AgentDirectory, "AgentStage");
            var srcFiles = Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories);

            foreach (var filePath in srcFiles)
            {
                if (filePath.Contains("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) ||
                    filePath.Contains("AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase)) { continue; }
                var fileName = Path.GetFileName(filePath);
                var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                File.Copy(filePath, finalPath, true);
            }

            // AgentCore
            srcPath = Path.Combine(AgentDirectory, "AgentCore");
            srcFiles = Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories);
            foreach (var filePath in srcFiles)
            {
                if (filePath.Contains("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) ||
                    filePath.Contains("AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase)) { continue; }
                var fileName = Path.GetFileName(filePath);
                var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                File.Copy(filePath, finalPath, true);
            }
        }

        protected static void RemoveTempDirectory(string path)
        {
            Directory.Delete(path, true);
        }
    }
}