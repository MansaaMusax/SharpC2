using Client.Services;
using Client.ViewModels;

using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Client.Commands
{
    public class LoadAgentModuleCommand : ICommand
    {
        readonly MainViewModel MainViewModel;

        public event EventHandler CanExecuteChanged;

        public LoadAgentModuleCommand(MainViewModel MainViewModel)
        {
            this.MainViewModel = MainViewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var openFile = new OpenFileDialog();

            if (openFile.ShowDialog() == true)
            {
                var module = File.ReadAllBytes(openFile.FileName);

                var task = MainViewModel.AgentTasks.FirstOrDefault(t => t.Command.Equals("LoadModule", StringComparison.OrdinalIgnoreCase));
                
                task.Parameters[0].Value = openFile.FileName;
                task.Parameters[1].Value = module;

                var json = JsonConvert.SerializeObject(task);

                SharpC2API.Agents.SubmitAgentCommand(MainViewModel.SelectedAgent.AgentID, "Core", "LoadModule", json);
            }   
        }
    }
}