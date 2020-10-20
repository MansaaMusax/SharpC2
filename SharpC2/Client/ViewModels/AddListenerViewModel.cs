using Client.Commands;
using Client.Controls;

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

        // TCP
        public bool BindLocal { get; set; }
        public int TcpBindPort { get; set; } = 4444;

        // SMB
        public string PipeName { get; set; } = "sharp_pipe";

        public ContentControl NewListenerContent { get; set; } = new ContentControl();

        public IList<ListenerType> ListenerTypes
        {
            get
            {
                return Enum.GetValues(typeof(ListenerType)).Cast<ListenerType>().ToList();
            }
        }

        private ListenerType _selectedListener;
        public ListenerType SelectedListener
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

        public AddListenerViewModel(Window window)
        {
            StartListener = new StartListenerCommand(window, this);

            UpdateSelectedListenerView();
        }

        private void UpdateSelectedListenerView()
        {
            object content = null;

            switch (SelectedListener)
            {
                case ListenerType.HTTP:
                    content = new HttpListenerControl { DataContext = this };
                    break;
                case ListenerType.TCP:
                    content = new TcpListenerControl { DataContext = this };
                    break;
                case ListenerType.SMB:
                    content = new SmbListenerControl { DataContext = this };
                    break;
                default:
                    break;
            }

            NewListenerContent.Content = content;
        }
    }
}