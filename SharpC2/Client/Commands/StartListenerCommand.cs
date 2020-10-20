using Client.API;
using Client.ViewModels;

using System;
using System.Windows;
using System.Windows.Input;

namespace Client.Commands
{
    public class StartListenerCommand : ICommand
    {
        private readonly Window Window;
        private readonly AddListenerViewModel ViewModel;

        public event EventHandler CanExecuteChanged;

        public StartListenerCommand(Window window, AddListenerViewModel viewModel)
        {
            Window = window;
            ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public async void Execute(object parameter)
        {
            var request = new NewListenerRequest
            {
                Name = ViewModel.ListenerName
            };

            switch (ViewModel.SelectedListener)
            {
                case ListenerType.HTTP:
                    request.Type = ListenerType.HTTP;
                    request.ConnectAddress = ViewModel.ConnectAddress;
                    request.ConnectPort = ViewModel.ConnectPort;
                    request.BindPort = ViewModel.HttpBindPort;
                    break;
                case ListenerType.TCP:
                    request.Type = ListenerType.TCP;
                    request.BindAddress = ViewModel.BindLocal ? "127.0.0.1" : "0.0.0.0";
                    request.BindPort = ViewModel.TcpBindPort;
                    break;
                case ListenerType.SMB:
                    request.Type = ListenerType.SMB;
                    request.PipeName = ViewModel.PipeName;
                    break;
                default:
                    break;
            }

            await ListenerAPI.StartListener(request);

            Window.Close();
        }
    }
}