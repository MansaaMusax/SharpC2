using Client.ViewModels;

using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    public partial class ConnectView : Window
    {
        public ConnectView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((dynamic)DataContext).Pass = (sender as PasswordBox).Password;
        }
    }
}