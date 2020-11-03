using Client.Views;

using System;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class CertThumprintViewModel
    {
        public string CertHash { get; set; }
        public bool Accept { get; set; } = true;

        public ICommand AcceptHash { get; }
        public ICommand RejectHash { get; }

        public CertThumprintViewModel(CertThumbprintView CertThumbprintView)
        {
            AcceptHash = new AcceptHashCommand(CertThumbprintView);
            RejectHash = new RejectHashCommand(this, CertThumbprintView);
        }
    }

    public class AcceptHashCommand : ICommand
    {
        private readonly CertThumbprintView CertThumbprintView;

        public event EventHandler CanExecuteChanged;

        public AcceptHashCommand(CertThumbprintView CertThumbprintView)
        {
            this.CertThumbprintView = CertThumbprintView;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            CertThumbprintView.Close();
        }
    }

    public class RejectHashCommand : ICommand
    {
        private readonly CertThumbprintView CertThumbprintView;
        private readonly CertThumprintViewModel CertThumprintViewModel;

        public event EventHandler CanExecuteChanged;

        public RejectHashCommand(CertThumprintViewModel CertThumprintViewModel, CertThumbprintView CertThumbprintView)
        {
            this.CertThumbprintView = CertThumbprintView;
            this.CertThumprintViewModel = CertThumprintViewModel;
        }

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
            CertThumprintViewModel.Accept = false;
            CertThumbprintView.Close();
        }
    }
}