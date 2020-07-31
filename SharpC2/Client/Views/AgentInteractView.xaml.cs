using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for AgentInteractView.xaml
    /// </summary>
    public partial class AgentInteractView : UserControl
    {
        private int currentIndex = -1;

        public AgentInteractView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            List<string> commandHistory = ((dynamic)sender).DataContext.CommandHistory;
            if (commandHistory.Count == 0) { return; }

            if (e.Key == Key.Up)
            {
                if ((currentIndex + 1) <= (commandHistory.Count - 1))
                {
                    currentIndex++;
                }

                ((dynamic)sender).DataContext.AgentCommand = commandHistory[currentIndex];
            }
            else if (e.Key == Key.Down)
            {
                if ((currentIndex -1 ) >= 0)
                {
                    currentIndex--;
                }

                ((dynamic)sender).DataContext.AgentCommand = commandHistory[currentIndex];
            }
        }
    }
}