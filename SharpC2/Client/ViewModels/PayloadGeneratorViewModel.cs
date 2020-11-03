using Client.Commands;
using Client.Services;
using Client.Views;

using Shared.Models;

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
        public readonly Window Window;

        public List<Listener> Listeners { get; set; } = new List<Listener>();
        public ContentControl PayloadCustomisation { get; set; } = new ContentControl();

        public int SleepInterval { get; set; } = 60;
        public int SleepJitter { get; set; } = 25;
        public DateTime KillDate { get; set; } = DateTime.UtcNow.AddDays(30);

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
            get { return _selectedListener; }
            set { _selectedListener = value; UpdateSelectedPayloadView(); }
        }

        public PayloadFormat SelectedPayloadFormat { get; set; }

        public ICommand GeneratePayloadCommand { get; }

        public PayloadGeneratorViewModel(Window Window)
        {
            this.Window = Window;

            GeneratePayloadCommand = new GeneratePayloadCommand(this);
            
            GetListeners();
        }

        async void GetListeners()
        {
            var listeners = await SharpC2API.Listeners.GetAllListeners();

            if (listeners != null)
            {
                Listeners.AddRange(listeners);
            }
        }

        void UpdateSelectedPayloadView()
        {
            object content = null;

            if (SelectedListener.Type == Listener.ListenerType.HTTP)
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