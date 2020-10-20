using Client.API;

using System;
using System.Windows.Input;

namespace Client.Commands
{
    class ExitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            ClientAPI.ClientLogoff();
            Environment.Exit(0);
        }
    }
}