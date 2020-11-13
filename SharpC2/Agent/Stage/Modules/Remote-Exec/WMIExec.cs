using System;
using System.Management;

namespace Agent.Modules
{
    public class WMIExec : IDisposable
    {
        string Target;
        ManagementScope Scope;

        public WMIExec(string Target)
        {
            this.Target = Target;
        }

        public string Execute(string Command)
        {
            Scope = new ManagementScope($@"\\{Target}\root\cimv2");
            Scope.Options.Impersonation = ImpersonationLevel.Impersonate;
            Scope.Options.EnablePrivileges = true;

            Scope.Connect();

            var mgmtClass = new ManagementClass(Scope, new ManagementPath("Win32_Process"), new ObjectGetOptions());
            var methodParams = mgmtClass.GetMethodParameters("Create");

            methodParams["CommandLine"] = Command;

            var result = mgmtClass.InvokeMethod("Create", methodParams, null);

            return string.Format("Return Value: {0}\nProcess ID: {1}", result["ReturnValue"], result["ProcessID"]);
        }

        public void Dispose()
        {
            Scope = null;
        }
    }
}