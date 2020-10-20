using Client.API;
using Client.ViewModels;

using System;
using System.Linq;
using System.Windows.Input;

namespace Client.Commands
{
    class AgentRemoveCommand : ICommand
    {
        private MainViewModel MainViewModel;

        public event EventHandler CanExecuteChanged;

        public AgentRemoveCommand(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            var agent = MainViewModel.Agents.FirstOrDefault(a => a.AgentId.Equals(MainViewModel.SelectedAgent.AgentId));

            if (agent != null)
            {
                AgentAPI.RemoveAgent(agent.AgentId);
                MainViewModel.Agents.Remove(agent);
            }
        }
    }
}