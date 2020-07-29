using Client.Views.Listeners;

using System.Collections.Generic;
using System.Windows.Controls;

namespace Client.ViewModels.Listeners
{
    public class AddListenerViewModel : BaseViewModel
    {
        private AddListenerView View { get; set; }
        public ContentControl NewListenerContent { get; set; } = new ContentControl();
        public List<string> ListenerTypes { get; set; }
        public string ListenerName { get; set; }

        private string _selectedListener;
        public string SelectedListener
        {
            get { return _selectedListener; }
            set { _selectedListener = value; UpdateSelectedListenerView(); }
        }

        public AddListenerViewModel(AddListenerView view)
        {
            View = view;

            ListenerTypes = new List<string>
            {
                "HTTP",
                "TCP"
            };
        }

        private void UpdateSelectedListenerView()
        {
            object content = null;

            if (SelectedListener.Equals(ListenerTypes[0]))
            {
                content = new NewHttpListenerView
                {
                    DataContext = new NewHttpListenerViewModel(View, this)
                };
            }
            else if (SelectedListener.Equals(ListenerTypes[1]))
            {
                content = new NewTcpListenerView
                {
                    DataContext = new NewTcpListenerViewModel(View, this)
                };
            }

            NewListenerContent.Content = content;
        }
    }
}