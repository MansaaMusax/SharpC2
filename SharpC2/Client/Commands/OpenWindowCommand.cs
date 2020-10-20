using Client.ViewModels;
using Client.Views;

using System;
using System.Windows;
using System.Windows.Input;

namespace Client.Commands
{
    public class OpenWindowCommand : ICommand
    {
        private readonly WindowType WindowType;

        public event EventHandler CanExecuteChanged;

        public OpenWindowCommand(WindowType windowType)
        {
            WindowType = windowType;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            Window window = null;
            BaseViewModel viewModel = null;

            switch (WindowType)
            {
                case WindowType.NewListener:
                    window = new AddListenerView();
                    viewModel = new AddListenerViewModel(window);
                    break;
                case WindowType.PayloadGenerator:
                    window = new PayloadGeneratorView();
                    viewModel = new PayloadGeneratorViewModel(window);
                    break;
            }

            window.DataContext = viewModel;
            window.Show();
        }
    }

    public enum WindowType
    {
        NewListener,
        PayloadGenerator
    }
}