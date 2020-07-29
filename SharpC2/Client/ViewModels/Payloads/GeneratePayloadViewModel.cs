using Client.SharpC2API;
using Client.Views;

using Microsoft.Win32;

using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    class GeneratePayloadViewModel : BaseViewModel
    {
        private GeneratePayloadView View { get; set; }
        public List<string> Listeners { get; set; } = new List<string>();
        public ContentControl PayloadType { get; set; } = new ContentControl();
        public string SleepInterval { get; set; } = "5";
        public string SleepJitter { get; set; } = "0";
        public DateTime KillDate { get; set; } = DateTime.UtcNow.AddDays(365);
        public List<string> Formats { get; set; } = new List<string> { "Windows .NET EXE", "Windows .NET DLL" };
        public List<string> Frameworks { get; set; } = new List<string> { ".NET Framework 4" };

        private string _selectedListener;
        public string SelectedListener
        {
            get { return _selectedListener; }
            set { _selectedListener = value; UpdateSelectedPayloadView(); }
        }

        public string SelectedFramework { get; set; }
        public string SelectedFormat { get; set; }

        private readonly DelegateCommand _generatePayload;
        public ICommand GeneratePayload => _generatePayload;

        public GeneratePayloadViewModel(GeneratePayloadView view)
        {
            View = view;
            _generatePayload = new DelegateCommand(OnGeneratePayload);

            GetListeners();
        }

        private async void GetListeners()
        {
            var listeners = await ListenerAPI.GetAllListeners();

            foreach (var listener in listeners)
            {
                Listeners.Add(string.Format("{0} : {1}", listener.ListenerId, listener.Type));
            }
        }

        private async void OnGeneratePayload(object obj)
        {
            var payloadReq = new PayloadRequest
            {
                ListenerId = SelectedListener.Split(":")[0].TrimEnd(),
                OutputType = OutputType.Dll,
                SleepInterval = SleepInterval,
                SleepJitter = SleepJitter,
                KillDate = KillDate,
                TargetFramework = TargetFramework.Net40
            };

            if (SelectedFormat == Formats[0])
            {
                payloadReq.OutputType = OutputType.Exe;
            }

            var window = new Window
            {
                Height = 100,
                Width = 360,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new ProgressBarView { DataContext = new ProgressBarViewModel { Label = "Building..."} }
            };

            window.Show();

            var payload = await PayloadAPI.GenerateAgentPayload(payloadReq);

            window.Close();
            
            if (payload.Length > 0)
            {
                var save = new SaveFileDialog();

                if (SelectedFormat == Formats[0])
                {
                    save.Filter = "EXE (*.exe)|*.exe";
                }
                else if (SelectedFormat == Formats[1])
                {
                    save.Filter = "DLL (*.dll)|*.dll";
                }

                if ((bool)save.ShowDialog())
                {
                    File.WriteAllBytes(save.FileName, payload);
                }
            }

            View.Close();
        }

        private void UpdateSelectedPayloadView()
        {
            object content = null;

            if (SelectedListener.Equals(Listeners[0]))
            {
                content = new GenerateHttpPayloadView
                {
                    DataContext = this
                };
            }

            PayloadType.Content = content;
        }
    }
}