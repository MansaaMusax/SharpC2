using Client.Services;

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
            SharpC2API.Users.ClientLogoff();
            Environment.Exit(0);
        }
    }
}