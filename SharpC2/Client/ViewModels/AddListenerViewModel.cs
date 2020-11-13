using Client.Commands;
using Client.Controls;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class AddListenerViewModel : BaseViewModel
    {
        public string ListenerName { get; set; }

        // HTTP
        public int HttpBindPort { get; set; } = 80;
        public string ConnectAddress { get; set; } = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork).ToString();
        public int ConnectPort { get; set; } = 80;
        public int SleepInterval { get; set; } = 1;
        public int SleepJitter { get; set; } = 0;
        public DateTime KillDate { get; set; } = DateTime.UtcNow.AddDays(30);

        // TCP
        public bool BindLocal { get; set; }
        public int TcpBindPort { get; set; } = 4444;

        // SMB
        public string PipeName { get; set; } = "sharp_pipe";

        public ContentControl NewListenerContent { get; set; } = new ContentControl();

        public IList<Listener.ListenerType> ListenerTypes
        {
            get
            {
                return Enum.GetValues(typeof(Listener.ListenerType)).Cast<Listener.ListenerType>().ToList();
            }
        }

        private Listener.ListenerType _selectedListener;
        public Listener.ListenerType SelectedListener
        {
            get
            {
                return _selectedListener;
            }
            set
            {
                _selectedListener = value;
                UpdateSelectedListenerView();
            }
        }

        public ICommand StartListener { get; }

        public AddListenerViewModel(Window Window)
        {
            StartListener = new StartListenerCommand(Window, this);

            UpdateSelectedListenerView();
        }

        private void UpdateSelectedListenerView()
        {
            object content = null;

            switch (SelectedListener)
            {
                case Listener.ListenerType.HTTP:
                    content = new HttpListenerControl { DataContext = this };
                    break;
                case Listener.ListenerType.TCP:
                    content = new TcpListenerControl { DataContext = this };
                    break;
                case Listener.ListenerType.SMB:
                    content = new SmbListenerControl { DataContext = this };
                    break;
                default:
                    break;
            }

            NewListenerContent.Content = content;
        }
    }
}