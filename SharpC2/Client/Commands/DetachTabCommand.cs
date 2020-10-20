using Client.ViewModels;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Client.Commands
{
    public class DetachTabCommand : ICommand
    {
        private readonly MainViewModel MainViewModel;
        private readonly string TabName;

        public event EventHandler CanExecuteChanged;

        public DetachTabCommand(string tabName, MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            TabName = tabName;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var tab = MainViewModel.OpenTabs.FirstOrDefault(t => t.Name.Equals(TabName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

            var window = new Window
            {
                Title = (string)tab.Header,
                Content = tab.Content,
                DataContext = tab.DataContext
            };

            window.Show();

            var close = new CloseTabCommand(tab.Name, MainViewModel);
            close.Execute(null);
        }
    }
}