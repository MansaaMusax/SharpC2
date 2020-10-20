using System;
using System.Collections.Generic;
using System.IO;

namespace Agent.Modules
{
    class DrivesModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController agent, ConfigController config)
        {
            Agent = agent;
            Config = config;
        }

        public AgentModuleInfo GetModuleInfo()
        {
            return new AgentModuleInfo
            {
                Name = "drives",
                Description = "Drives",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "list",
                        Description = "Get current drives",
                        HelpText = "drives list",
                        CallBack = GetDrives
                    },
                }
            };
        }

        private void GetDrives(byte[] data)
        {
            try
            {
                var result = new SharpC2ResultList<DriveInfoResult>();
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    var info = new DriveInfoResult
                    {
                        Name = drive.Name,
                        Type = drive.DriveType
                    };

                    if (drive.IsReady)
                    {
                        info.Label = drive.VolumeLabel;
                        info.Format = drive.DriveFormat;
                        info.Capacity = SharedHelpers.ConvertFileLength(drive.TotalSize);
                        info.FreeSpace = SharedHelpers.ConvertFileLength(drive.AvailableFreeSpace);
                    }

                    result.Add(info);
                }

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}