using Client.ViewModels;

using System;
using System.Linq;
using System.Windows.Input;

namespace Client.Commands
{
    public class CloseTabCommand : ICommand
    {
        private readonly MainViewModel MainViewModel;
        private readonly string TabName;

        public event EventHandler CanExecuteChanged;

        public CloseTabCommand(string tabName, MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            TabName = tabName;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var tab = MainViewModel.OpenTabs.FirstOrDefault(t => t.Name.Equals(TabName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

            if (tab != null)
            {
                MainViewModel.OpenTabs.Remove(tab);
            }
        }
    }
}