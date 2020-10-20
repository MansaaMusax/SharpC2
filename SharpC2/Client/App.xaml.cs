using Client.ViewModels;
using Client.Views;

using System.Windows;

namespace Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var view = new ConnectView();
            var viewModel = new ConnectViewModel(view);
            view.DataContext = viewModel;

            view.ShowDialog();
        }
    }
}