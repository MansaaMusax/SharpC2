using System;
using System.Threading;

using static Agent.PInvoke.NativeMethods;

namespace Agent.Modules
{
    public class PsExec : IDisposable
    {
        string Target;
        string ServiceName;

        IntPtr hManager;
        IntPtr hService;

        public PsExec(string Target)
        {
            this.Target = Target;

            ServiceName = Shared.Utilities.Utilities.GetRandomString(6);
        }

        public void Execute(string Command)
        {
            hManager = Advapi.OpenSCManager(
                Target,
                null,
                SCM_ACCESS.SC_MANAGER_ALL_ACCESS);

            hService = Advapi.CreateService(
                hManager,
                ServiceName,
                null,
                SERVICE_ACCESS.SERVICE_ALL_ACCESS,
                SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS,
                SERVICE_START.SERVICE_DEMAND_START,
                SERVICE_ERROR.SERVICE_ERROR_NORMAL,
                Command,
                null,
                null,
                null,
                null,
                null);

            Advapi.StartService(
                hService,
                0,
                null);

            Thread.Sleep(1000);

            Advapi.DeleteService(hService);
        }

        public void Dispose()
        {
            Advapi.CloseServiceHandle(hService);
            Advapi.CloseServiceHandle(hManager);
        }
    }
}