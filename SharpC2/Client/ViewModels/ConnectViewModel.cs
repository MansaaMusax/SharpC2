using Client.SharpC2API;
using Client.Views;
using SharpC2.Models;

using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    class ConnectViewModel
    {
        public Window ConnectView { get; set; }
        public string ConnectMessage { get; set; } = "Welcome to SharpC2";
        public string Host { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "7443";
        public string Nick { get; set; } = "neo";
        public string Pass { get; set; }


        private readonly DelegateCommand _clickLogin;
        public ICommand ClickLogin => _clickLogin;

        public ConnectViewModel(Window window)
        {
            ConnectView = window;
            _clickLogin = new DelegateCommand(OnClickLogin);
        }
        
        private async void OnClickLogin(object obj)
        {
            var progressWindow = new Window
            {
                Height = 100,
                Width = 360,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new ProgressBarView { DataContext = new ProgressBarViewModel { Label = "Authenticating..."} }
            };

            progressWindow.Show();

            var result = await ClientAPI.ClientLogin(Host, Port, Nick, Pass);

            progressWindow.Close();

            if (result.Result == ClientAuthenticationResult.AuthResult.LoginSuccess)
            {
                var mainWindow = new MainWindow
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                mainWindow.Show();
                ConnectView.Close();
            }
            else
            {
                var errorText = result.Result switch
                {
                    ClientAuthenticationResult.AuthResult.BadPassword => "Incorrect Password",
                    ClientAuthenticationResult.AuthResult.NickInUse => "This nick is already in use",
                    ClientAuthenticationResult.AuthResult.InvalidRequest => "Invalid request",
                    _ => result.Result.ToString(),
                };

                var errorWindow = new Window
                {
                    Height = 100,
                    Width = 360,
                    Title = "Authentication Error",
                    Content = errorText
                };

                errorWindow.ShowDialog();
            }
        }
    }
}