using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;

namespace Agent.Modules
{
    public class DriveModule : IAgentModule
    {
        AgentController Agent;
        ConfigController Config;

        public void Init(AgentController Agent, ConfigController Config)
        {
            this.Agent = Agent;
            this.Config = Config;
        }

        public ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo
            {
                Name = "Drives",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "List",
                        Delegate = ListDrives
                    }
                }
            };
        }

        private void ListDrives(string AgentID, C2Data C2Data)
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
                        info.Capacity = Helpers.ConvertFileLength(drive.TotalSize);
                        info.FreeSpace = Helpers.ConvertFileLength(drive.AvailableFreeSpace);
                    }

                    result.Add(info);
                }

                Agent.SendMessage(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}