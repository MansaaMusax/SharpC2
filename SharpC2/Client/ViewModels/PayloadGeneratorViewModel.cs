using Client.Commands;
using Client.Services;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    class PayloadGeneratorViewModel : BaseViewModel
    {
        public readonly Window Window;

        public List<Listener> Listeners { get; set; } = new List<Listener>();

        public IList<PayloadFormat> PayloadFormats
        {
            get
            {
                return Enum.GetValues(typeof(PayloadFormat)).Cast<PayloadFormat>().ToList();
            }
        }

        public Listener SelectedListener { get; set; }

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
    }

    public enum PayloadFormat
    {
        PowerShell,
        EXE,
        DLL
    }
}