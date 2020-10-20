using Client.Views;

using System;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class PowerShellPayloadViewModel
    {
        public PowerShellPayloadView View { get; set; }

        public string Launcher { get; set; }
        public string EncodedLauncher { get; set; }

        public ICommand CloseView { get; }

        public PowerShellPayloadViewModel(PowerShellPayloadView view)
        {
            View = view;
            CloseView = new CloseViewCommand(View);
        }

        public class CloseViewCommand : ICommand
        {
            private readonly PowerShellPayloadView View;

            public event EventHandler CanExecuteChanged;

            public CloseViewCommand(PowerShellPayloadView view)
            {
                View = view;
            }

            public bool CanExecute(object parameter)
                => true;

            public void Execute(object parameter)
            {
                View.Close();
            }
        }
    }
}