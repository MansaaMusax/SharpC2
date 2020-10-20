using Client.Views;

using System;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class InputViewModel
    {
        public string Input { get; set; }

        public ICommand AcceptInput { get; }
        public ICommand RejectInput { get; }

        public InputViewModel(InputView inputView)
        {
            AcceptInput = new AcceptInputCommand(inputView);
            RejectInput = new RejectInputCommand(this, inputView);
        }
    }

    public class AcceptInputCommand : ICommand
    {
        private readonly InputView InputView;

        public event EventHandler CanExecuteChanged;

        public AcceptInputCommand(InputView inputView)
        {
            InputView = inputView;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            InputView.Close();
        }
    }

    public class RejectInputCommand : ICommand
    {
        private readonly InputView InputView;
        private readonly InputViewModel InputViewModel;

        public event EventHandler CanExecuteChanged;

        public RejectInputCommand(InputViewModel inputViewModel, InputView inputView)
        {
            InputView = inputView;
            InputViewModel = inputViewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            InputViewModel.Input = string.Empty;
            InputView.Close();
        }
    }
}