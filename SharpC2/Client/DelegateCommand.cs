using System;
using System.Windows.Input;

namespace Client
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private ICommand command;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
        }

        public DelegateCommand(ICommand command)
        {
            this.command = command;
        }

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _executeAction(parameter);
    }
}