using System;
using System.Runtime.InteropServices;

namespace Agent.PInvoke
{
    public class NativeMethods
    {
        public class Kernel32
        {
            [DllImport("kernel32.dll")]
            public static extern bool CreateProcess(
                string lpApplicationName,
                string lpCommandLine,
                ref SECURITY_ATTRIBUTES lpProcessAttributes,
                ref SECURITY_ATTRIBUTES lpThreadAttributes,
                bool bInheritHandles,
                uint dwCreationFlags,
                IntPtr lpEnvironment,
                string lpCurrentDirectory,
                [In] ref STARTUPINFOEX lpStartupInfo,
                out PROCESS_INFORMATION lpProcessInformation);

            [DllImport("kernel32.dll")]
            public static extern int WaitForSingleObject(
                IntPtr handle,
                int milliseconds);

            [DllImport("kernel32.dll")]
            public static extern bool UpdateProcThreadAttribute(
                IntPtr lpAttributeList,
                uint dwFlags,
                IntPtr Attribute,
                IntPtr lpValue,
                IntPtr cbSize,
                IntPtr lpPreviousValue,
                IntPtr lpReturnSize);

            [DllImport("kernel32.dll")]
            public static extern bool InitializeProcThreadAttributeList(
                IntPtr lpAttributeList,
                int dwAttributeCount,
                int dwFlags,
                ref IntPtr lpSize);

            [DllImport("kernel32.dll")]
            public static extern bool DeleteProcThreadAttributeList(
                IntPtr lpAttributeList);

            [DllImport("kernel32.dll")]
            public static extern bool SetHandleInformation(
                IntPtr hObject,
                HandleFlags dwMask,
                HandleFlags dwFlags);

            [DllImport("kernel32.dll")]
            public static extern bool PeekNamedPipe(
                IntPtr handle,
                IntPtr buffer,
                IntPtr nBufferSize,
                IntPtr bytesRead,
                ref uint bytesAvail,
                IntPtr BytesLeftThisMessage);

            [DllImport("kernel32.dll")]
            public static extern bool CloseHandle(
                IntPtr hObject);

            [DllImport("kernel32.dll")]
            public static extern bool DuplicateHandle(
                IntPtr hSourceProcessHandle,
                IntPtr hSourceHandle,
                IntPtr hTargetProcessHandle,
                ref IntPtr lpTargetHandle,
                uint dwDesiredAccess,
                bool bInheritHandle,
                DuplicateOptions dwOptions);

            [DllImport("kernel32.dll")]
            public static extern bool CreatePipe(
                out IntPtr hReadPipe,
                out IntPtr hWritePipe,
                ref SECURITY_ATTRIBUTES lpPipeAttributes,
                uint nSize);

            [DllImport("kernel32.dll")]
            public static extern uint ResumeThread(
                IntPtr hThread);

            [DllImport("kernel32.dll")]
            public static extern void RtlZeroMemory(
                IntPtr pBuffer,
                int length);

            [DllImport("kernel32.dll")]
            public static extern bool ReadProcessMemory(
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                IntPtr lpBuffer,
                uint dwSize,
                out uint lpNumberOfBytesRead);

            [DllImport("kernel32.dll")]
            public static extern bool VirtualProtectEx(
                IntPtr hProcess,
                IntPtr lpAddress,
                uint dwSize,
                AllocationProtect flNewProtect,
                out AllocationProtect lpflOldProtect);

            [DllImport("kernel32.dll")]
            public static extern bool WriteProcessMemory(
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                IntPtr lpBuffer,
                uint nSize,
                out uint lpNumberOfBytesWritten);

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibraryEx(
                string lpFileName,
                IntPtr hReservedNull,
                uint dwFlags);

            [DllImport("kernel32")]
            public static extern IntPtr GetProcAddress(
                IntPtr hModule,
                string procName);

            [DllImport("kernel32.dll")]
            public static extern bool VirtualProtectEx(
                IntPtr hProcess,
                IntPtr lpAddress,
                UIntPtr dwSize,
                uint flNewProtect,
                out uint lpflOldProtect);

            [DllImport("kernel32.dll")]
            public static extern IntPtr VirtualAllocEx(
                IntPtr hProcess,
                IntPtr lpAddress,
                uint dwSize,
                uint flAllocationType,
                uint flProtect);

            [DllImport("kernel32.dll")]
            public static extern bool WriteProcessMemory(
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                byte[] lpBuffer,
                uint nSize,
                out UIntPtr lpNumberOfBytesWritten);

            [DllImport("kernel32.dll")]
            public static extern IntPtr CreateRemoteThread(
                IntPtr hProcess,
                IntPtr lpThreadAttributes,
                uint dwStackSize,
                IntPtr lpStartAddress,
                IntPtr lpParameter,
                uint dwCreationFlags,
                IntPtr lpThreadId);
        }

        public class Ntdll
        {
            [DllImport("ntdll.dll")]
            public static extern uint NtQueryInformationProcess(
                IntPtr processHandle,
                uint processInformationClass,
                ref PROCESS_BASIC_INFORMATION processInformation,
                int processInformationLength,
                out uint returnLength);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr ExitStatus;
            public IntPtr PebBaseAddress;
            public IntPtr AffinityMask;
            public IntPtr BasePriority;
            public UIntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [Flags]
        public enum HandleFlags : uint
        {
            None = 0,
            Inherit = 1,
            ProtectFromClose = 2
        }

        [Flags]
        public enum DuplicateOptions : uint
        {
            DuplicateCloseSource = 0x00000001,
            DuplicateSameAccess = 0x00000002
        }

        [Flags]
        public enum AllocationProtect : uint
        {
            NONE = 0x00000000,
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }
    }
}