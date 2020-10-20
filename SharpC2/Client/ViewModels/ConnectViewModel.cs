using Client.Commands;
using Client.Views;

using System.Windows.Input;

namespace Client.ViewModels
{
    public class ConnectViewModel
    {
        public ConnectView ConnectView { get; set; }
        public string WelcomeMessage { get; set; } = "Welcome to SharpC2";
        public string Host { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "7443";
        public string Nick { get; set; } = "neo";
        public string Pass { get; set; }

        public ICommand ConnectCommand { get; }

        public ConnectViewModel(ConnectView connectView)
        {
            ConnectView = connectView;
            ConnectCommand = new ConnectCommand(this);
        }
    }
}