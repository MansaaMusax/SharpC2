﻿using Client.Controls;
using Client.Services;
using Client.ViewModels;

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Commands
{
    public class OpenTabCommand : ICommand
    {
        readonly MainViewModel MainViewModel;
        readonly string TabName;
        readonly TabType TabType;
        readonly SignalR SignalR;

        public event EventHandler CanExecuteChanged;

        public OpenTabCommand(string TabName, TabType TabType, MainViewModel MainViewModel)
        {
            this.TabName = TabName;
            this.TabType = TabType;
            this.MainViewModel = MainViewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            TabItem tab;

            if (!MainViewModel.OpenTabs.Any(t => t.Name.Equals(TabName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase)))
            {
                tab = new TabItem
                {
                    Header = TabName,
                    Name = TabName.Replace(" ", "")
                };

                switch (TabType)
                {
                    case TabType.EventLog:
                        tab.Content = new EventLogControl();
                        tab.DataContext = new EventLogViewModel(MainViewModel);
                        break;
                    case TabType.WebLog:
                        tab.Content = new WebLogControl();
                        tab.DataContext = new WebLogViewModel(MainViewModel, SignalR);
                        break;
                    case TabType.Listeners:
                        tab.Content = new ListenerControl();
                        tab.DataContext = new ListenerViewModel(MainViewModel);
                        break;
                    case TabType.Agent:
                        tab.Content = new AgentInteractControl();
                        tab.DataContext = new AgentInteractViewModel(MainViewModel, MainViewModel.SelectedAgent);
                        break;
                    default:
                        break;
                }

                MainViewModel.OpenTabs.Add(tab);
            }
            else
            {
                tab = MainViewModel.OpenTabs.FirstOrDefault(t => t.Name.Equals(TabName.Replace(" ", "")));
            }

            MainViewModel.SelectedTabIndex = MainViewModel.OpenTabs.IndexOf(tab);
        }
    }

    public enum TabType
    {
        EventLog,
        WebLog,
        Listeners,
        Agent
    }
}