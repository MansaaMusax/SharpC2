using Client.ViewModels;

using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for ConnectView.xaml
    /// </summary>
    public partial class ConnectView : Window
    {
        public ConnectView()
        {
            InitializeComponent();
            DataContext = new ConnectViewModel(this);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((dynamic)DataContext).Pass = (sender as PasswordBox).Password;
        }
    }
}