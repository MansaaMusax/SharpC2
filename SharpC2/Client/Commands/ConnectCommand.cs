using Client.API;
using Client.Services;
using Client.ViewModels;
using Client.Views;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.Commands
{
    public class ConnectCommand : ICommand
    {
        private ConnectViewModel ConnectViewModel;
        public event EventHandler CanExecuteChanged;

        public ConnectCommand(ConnectViewModel connectWiewModel)
        {
            ConnectViewModel = connectWiewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public async void Execute(object parameter)
        {
            var progressBar = new ProgressBarView
            {
                Height = 60,
                Width = 300,

                DataContext = new ProgressBarViewModel
                {
                    Label = "Authenticating..."
                }
            };

            progressBar.Show();

            var result = await ClientAPI.ClientLogin(ConnectViewModel.Host, ConnectViewModel.Port, ConnectViewModel.Nick, ConnectViewModel.Pass);

            progressBar.Close();

            if (result.Result == ClientAuthResult.LoginSuccess)
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl($"https://{ConnectViewModel.Host}:{ConnectViewModel.Port}/MessageHub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(result.Token);
                        options.HttpMessageHandlerFactory = (x) => new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // yolo
                        };
                    })
                    .Build();

                var signalR = new SignalR(connection);

                var mainView = new MainView();
                var mainViewModel = new MainViewModel(mainView, signalR);
                mainView.DataContext = mainViewModel;

                mainView.Show();
                ConnectViewModel.ConnectView.Close();
            }
            else
            {
                var errorText = result.Result switch
                {
                    ClientAuthResult.BadPassword => "Incorrect Password",
                    ClientAuthResult.NickInUse => "This nick is already in use",
                    ClientAuthResult.InvalidRequest => "Invalid request",
                    _ => result.Result.ToString(),
                };

                var errorView = new ErrorView
                {
                    DataContext = new ErrorViewModel
                    {
                        Error = errorText
                    }
                };

                errorView.ShowDialog();
            }
        }
    }
}