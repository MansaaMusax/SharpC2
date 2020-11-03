using Client.ViewModels;

using System;
using System.Windows.Input;

namespace Client.Commands
{
    public class AgentInteractCommand : ICommand
    {
        readonly MainViewModel MainViewModel;

        public event EventHandler CanExecuteChanged;

        public AgentInteractCommand(MainViewModel MainViewModel)
        {
            this.MainViewModel = MainViewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var agent = MainViewModel.SelectedAgent;

            var openTab = new OpenTabCommand(agent.AgentID, TabType.Agent, MainViewModel);
            openTab.Execute(null);
        }
    }
}