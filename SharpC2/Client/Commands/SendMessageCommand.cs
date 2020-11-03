using Client.Services;
using Client.ViewModels;

using System;
using System.Windows.Input;

namespace Client.Commands
{
    public class SendMessageCommand : ICommand
    {
        readonly EventLogViewModel EventLogViewModel;

        public event EventHandler CanExecuteChanged;

        public SendMessageCommand(EventLogViewModel EventLogViewModel)
        {
            this.EventLogViewModel = EventLogViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await SignalR.SendChatMessage(EventLogViewModel.ChatMessage);
            EventLogViewModel.ChatMessage = string.Empty;
        }
    }
}