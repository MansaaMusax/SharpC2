using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Agent.Modules
{
    class DirectoryModule : IAgentModule
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
                Name = "dir",
                Description = "Do stuff with directories",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "print",
                        Description = "Prints the current working directory.",
                        HelpText = "dir print",
                        CallBack = PrintWorkingDirectory
                    },
                    new AgentCommand
                    {
                        Name = "set",
                        Description = "Change the current working directory",
                        HelpText = "dir set [path]",
                        CallBack = ChangeDirectory
                    },
                    new AgentCommand
                    {
                        Name = "list",
                        Description = "List a directory",
                        HelpText = "dir list [path]",
                        CallBack = ListDirectory
                    },
                    new AgentCommand
                    {
                        Name = "rm",
                        Description = "Delete a directory",
                        HelpText = "dir rm [path]",
                        CallBack = RemoveDirectory
                    },
                    new AgentCommand
                    {
                        Name = "mk",
                        Description = "Create a directory",
                        HelpText = "dir mk [path]",
                        CallBack = CreateDirectory
                    },
                }
            };
        }

        private void PrintWorkingDirectory(byte[] data)
        {
            try
            {
                var result = Helpers.GetCurrentDirectory;
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ChangeDirectory(byte[] data)
        {
            try
            {
                var path = Encoding.UTF8.GetString(data);
                Directory.SetCurrentDirectory(path);

                var result = Helpers.GetCurrentDirectory;
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ListDirectory(byte[] data)
        {
            try
            {
                var path = data.Length < 1 ? Directory.GetCurrentDirectory() : Encoding.UTF8.GetString(data);
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

                Agent.SendOutput(result.ToString());
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void RemoveDirectory(byte[] data)
        {
            try
            {
                var directory = Encoding.UTF8.GetString(data);
                Directory.Delete(directory, true);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void CreateDirectory(byte[] data)
        {
            try
            {
                var path = Encoding.UTF8.GetString(data);
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
    }
}