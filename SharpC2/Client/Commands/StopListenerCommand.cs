using Client.API;
using Client.ViewModels;

using System;
using System.Windows.Input;

namespace Client.Commands
{
    public class StopListenerCommand : ICommand
    {
        private readonly ListenerViewModel ViewModel;

        public event EventHandler CanExecuteChanged;

        public StopListenerCommand(ListenerViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            ListenerAPI.StopListener(ViewModel.SelectedListener.Name);
        }
    }
}