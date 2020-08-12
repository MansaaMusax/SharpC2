using Client.SharpC2API;
using Client.Views.Listeners;

using SharpC2.Models;

using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels.Listeners
{
    public class NewHttpListenerViewModel : BaseViewModel
    {
        private AddListenerView View { get; set; }
        private AddListenerViewModel ViewModel { get; set; }

        public int BindPort { get; set; } = 80;
        public string ConnectAddress { get; set; } = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork).ToString();
        public int ConnectPort { get; set; } = 80;

        private readonly DelegateCommand _startHttpListener;

        public ICommand StartHttpListener => _startHttpListener;

        public NewHttpListenerViewModel(AddListenerView view, AddListenerViewModel viewModel)
        {
            View = view;
            ViewModel = viewModel;

            _startHttpListener = new DelegateCommand(OnStartHttpListener);
        }

        private async void OnStartHttpListener(object obj)
        {
            var request = new NewHttpListenerRequest
            {
                Name = ViewModel.ListenerName,
                BindPort = BindPort,
                ConnectAddress = ConnectAddress,
                ConnectPort = ConnectPort,
            };

            var response = await ListenerAPI.StartHttpListener(request);

            var window = new Window
            {
                Height = 200,
                Width = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (string.IsNullOrEmpty(response.ListenerName))
            {
                window.Content = "Failed to start listener";
                window.ShowDialog();
            }
            
            View.Close();
        }
    }
}