using Client.ViewModels;
using Client.Views;

using System;
using System.Linq;
using System.Windows.Input;

namespace Client.Commands
{
    public class RenameTabCommand : ICommand
    {
        private MainViewModel MainViewModel;
        private string TabName;

        public event EventHandler CanExecuteChanged;

        public RenameTabCommand(string tabName, MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            TabName = tabName;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var inputView = new InputView();
            var inputViewModel = new InputViewModel(inputView);
            inputView.DataContext = inputViewModel;

            inputView.ShowDialog();

            if (!string.IsNullOrEmpty(inputViewModel.Input))
            {
                var tab = MainViewModel.OpenTabs.FirstOrDefault(t => t.Name.Equals(TabName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

                if (tab != null)
                {
                    tab.Header = inputViewModel.Input;
                }
            }
        }
    }
}