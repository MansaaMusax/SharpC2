using Client.Services;
using Client.ViewModels;

using Shared.Models;

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
            var request = new ListenerRequest
            {
                Name = ViewModel.ListenerName
            };

            switch (ViewModel.SelectedListener)
            {
                case Listener.ListenerType.HTTP:
                    request.Type = Listener.ListenerType.HTTP;
                    request.ConnectAddress = ViewModel.ConnectAddress;
                    request.ConnectPort = ViewModel.ConnectPort;
                    request.BindPort = ViewModel.HttpBindPort;
                    break;
                case Listener.ListenerType.TCP:
                    request.Type = Listener.ListenerType.TCP;
                    request.BindAddress = ViewModel.BindLocal ? "127.0.0.1" : "0.0.0.0";
                    request.BindPort = ViewModel.TcpBindPort;
                    break;
                case Listener.ListenerType.SMB:
                    request.Type = Listener.ListenerType.SMB;
                    request.PipeName = ViewModel.PipeName;
                    break;
                default:
                    break;
            }

            await SharpC2API.Listeners.StartListener(request);

            Window.Close();
        }
    }
}