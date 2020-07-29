using Client.SharpC2API;
using Client.Views.Listeners;

using SharpC2.Models;

using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels.Listeners
{
    public class NewTcpListenerViewModel : BaseViewModel
    {
        private AddListenerView View { get; set; }
        private AddListenerViewModel ViewModel { get; set; }

        public string BindAddress { get; set; } = "0.0.0.0";
        public int BindPort { get; set; } = 4444;
        

        private readonly DelegateCommand _startTcpListener;

        public ICommand StartTcpListener => _startTcpListener;

        public NewTcpListenerViewModel(AddListenerView view, AddListenerViewModel viewModel)
        {
            View = view;
            ViewModel = viewModel;

            _startTcpListener = new DelegateCommand(OnStartTcpListener);
        }

        private async void OnStartTcpListener(object obj)
        {
            var request = new NewTcpListenerRequest
            {
                Name = ViewModel.ListenerName,
                BindAddress = BindAddress,
                BindPort = BindPort,
            };

            var response = await ListenerAPI.StartTcpListener(request);

            var window = new Window
            {
                Height = 200,
                Width = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (string.IsNullOrEmpty(response.ListenerId))
            {
                window.Content = "Failed to start listener";
                window.ShowDialog();
            }
            
            View.Close();
        }
    }
}