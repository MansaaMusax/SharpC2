using Shared.Models;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Models
{
    public class Agent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string AgentID { get; set; }

        private string _hostname;
        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; NotifyPropertyChanged(nameof(Hostname)); }
        }

        private string _ipAddress { get; set; }
        public string IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; NotifyPropertyChanged(nameof(IPAddress)); }
        }

        private string _identity;
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; NotifyPropertyChanged(nameof(Identity)); }
        }

        private string _process;
        public string Process
        {
            get { return _process; }
            set { _process = value; NotifyPropertyChanged(nameof(Process)); }
        }

        private int _pid;
        public int PID
        {
            get { return _pid; }
            set { _pid = value; NotifyPropertyChanged(nameof(PID)); }
        }

        private AgentMetadata.Architecture _arch;
        public AgentMetadata.Architecture Arch
        {
            get { return _arch; }
            set { _arch = value; NotifyPropertyChanged(nameof(Arch)); }
        }

        private AgentMetadata.Integrity _integrity;
        public AgentMetadata.Integrity Integrity
        {
            get { return _integrity; }
            set { _integrity = value; NotifyPropertyChanged(nameof(Integrity)); }
        }

        public DateTime LastSeen { get; set; }

        private string _counter;
        public string Counter
        {
            get { return _counter; }
            set { _counter = value; NotifyPropertyChanged(nameof(LastSeen)); }
        }

        void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}