using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Agent.Modules
{
    public class WinRMExec
    {
        string Target;

        public WinRMExec(string Target)
        {
            this.Target = Target;
        }

        public string Execute(string Command)
        {
            var uri = new Uri($"http://{Target}:5985/WSMAN");
            var connection = new WSManConnectionInfo(uri);

            using (var runspace = RunspaceFactory.CreateRunspace(connection))
            {
                runspace.Open();

                using (var ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;
                    ps.AddScript(Command);
                    ps.AddCommand("Out-String");

                    var results = new PSDataCollection<object>();

                    ps.Streams.Error.DataAdded += (sender, e) =>
                    {
                        foreach (ErrorRecord er in ps.Streams.Error.ReadAll())
                        {
                            results.Add(er);
                        }
                    };
                    ps.Streams.Verbose.DataAdded += (sender, e) =>
                    {
                        foreach (VerboseRecord vr in ps.Streams.Verbose.ReadAll())
                        {
                            results.Add(vr);
                        }
                    };
                    ps.Streams.Debug.DataAdded += (sender, e) =>
                    {
                        foreach (DebugRecord dr in ps.Streams.Debug.ReadAll())
                        {
                            results.Add(dr);
                        }
                    };
                    ps.Streams.Warning.DataAdded += (sender, e) =>
                    {
                        foreach (WarningRecord wr in ps.Streams.Warning)
                        {
                            results.Add(wr);
                        }
                    };

                    ps.Invoke(null, results);

                    var output = string.Join(Environment.NewLine, results.Select(r => r.ToString()).ToArray());
                    ps.Commands.Clear();
                    return output;
                }
            }
        }
    }
}