using Client.Services;
using Client.ViewModels;

using System;
using System.Windows.Input;

namespace Client.Commands
{
    public class AgentInteractCommand : ICommand
    {
        private readonly MainViewModel ViewModel;
        private readonly SignalR SignalR;

        public event EventHandler CanExecuteChanged;

        public AgentInteractCommand(MainViewModel viewModel, SignalR signalR)
        {
            ViewModel = viewModel;
            SignalR = signalR;

        }

        public bool CanExecute(object parameter)
        {
            if (ViewModel.SelectedAgent == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Execute(object parameter)
        {
            var agent = ViewModel.SelectedAgent;

            var openTab = new OpenTabCommand(agent.AgentId, TabType.Agent, ViewModel, SignalR);
            openTab.Execute(null);
        }
    }
}
