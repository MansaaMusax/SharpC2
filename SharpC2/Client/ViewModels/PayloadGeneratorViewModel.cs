using Client.API;
using Client.Commands;
using Client.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    class PayloadGeneratorViewModel : BaseViewModel
    {
        public Window View { get; set; }

        public List<Listener> Listeners { get; set; } = new List<Listener>();
        public ContentControl PayloadCustomisation { get; set; } = new ContentControl();

        public string SleepInterval { get; set; } = "60";
        public string SleepJitter { get; set; } = "25";

        public DateTime KillDate { get; set; } = DateTime.UtcNow.AddDays(365);

        public IList<TargetFramework> Frameworks
        {
            get
            {
                return Enum.GetValues(typeof(TargetFramework)).Cast<TargetFramework>().ToList();
            }
        }

        public IList<PayloadFormat> PayloadFormats
        {
            get
            {
                return Enum.GetValues(typeof(PayloadFormat)).Cast<PayloadFormat>().ToList();
            }
        }

        private Listener _selectedListener;
        public Listener SelectedListener
        {
            get
            {
                return _selectedListener;
            }
            set
            {
                _selectedListener = value;
                UpdateSelectedPayloadView();
            }
        }

        public TargetFramework SelectedFramework { get; set; }
        public PayloadFormat SelectedPayloadFormat { get; set; }

        public ICommand GeneratePayloadCommand { get; }

        public PayloadGeneratorViewModel(Window view)
        {
            View = view;
            GeneratePayloadCommand = new GeneratePayloadCommand(this);
            GetListeners();
        }

        private async void GetListeners()
        {
            var listeners = await ListenerAPI.GetAllListeners();

            if (listeners != null)
            {
                Listeners.AddRange(listeners);
            }
        }

        private void UpdateSelectedPayloadView()
        {
            object content = null;

            if (SelectedListener.Type == ListenerType.HTTP)
            {
                content = new HttpPayloadView
                {
                    DataContext = this
                };
            }

            PayloadCustomisation.Content = content;
        }
    }

    public enum PayloadFormat
    {
        PowerShell,
        EXE,
        DLL
    }
}