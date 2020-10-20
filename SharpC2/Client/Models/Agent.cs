using System.Collections.Generic;
using System.ComponentModel;

namespace Client.Models
{
    public class Agent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string AgentId { get; set; }

        private string _hostname;
        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; NotifyPropertyChanged("Hostname"); }
        }

        private string _ipAddress { get; set; }
        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; NotifyPropertyChanged("IpAddress"); }
        }

        private string _identity;
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; NotifyPropertyChanged("Identity"); }
        }

        private string _processName;
        public string ProcessName
        {
            get { return _processName; }
            set { _processName = value; NotifyPropertyChanged("ProcessName"); }
        }

        private int _processId;
        public int ProcessId
        {
            get { return _processId; }
            set { _processId = value; NotifyPropertyChanged("ProcessId"); }
        }

        private string _arch;
        public string Arch
        {
            get { return _arch; }
            set { _arch = value; NotifyPropertyChanged("Arch"); }
        }

        private string _integrity;
        public string Integrity
        {
            get { return _integrity; }
            set { _integrity = value; NotifyPropertyChanged("Integrity"); }
        }

        private int _clr;
        public int CLR
        {
            get { return _clr; }
            set { _clr = value; NotifyPropertyChanged("CLR"); }
        }

        public List<AgentModule> AgentModules { get; set; }

        private string _lastSeen;
        public string LastSeen
        {
            get { return _lastSeen; }
            set { _lastSeen = value; NotifyPropertyChanged("LastSeen"); }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}