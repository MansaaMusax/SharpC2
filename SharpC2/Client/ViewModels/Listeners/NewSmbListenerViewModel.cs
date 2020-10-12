using Client.SharpC2API;
using Client.Views.Listeners;

using SharpC2.Models;

using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels.Listeners
{
    public class NewSmbListenerViewModel : BaseViewModel
    {
        private AddListenerView View { get; set; }
        private AddListenerViewModel ViewModel { get; set; }

        public string PipeName { get; set; } = "SharpC2Pipe";

        private readonly DelegateCommand _startSmbListener;
        public ICommand StartSmbListener => _startSmbListener;

        public NewSmbListenerViewModel(AddListenerView view, AddListenerViewModel viewModel)
        {
            View = view;
            ViewModel = viewModel;

            _startSmbListener = new DelegateCommand(OnStartSmbListener);
        }

        private async void OnStartSmbListener(object obj)
        {
            var request = new NewSmbListenerRequest
            {
                Name = ViewModel.ListenerName,
                PipeName = PipeName
            };

            var response = await ListenerAPI.StartSmbListener(request);

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