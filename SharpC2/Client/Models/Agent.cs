using SharpC2.Models;

using System.Collections.Generic;
using System.ComponentModel;

namespace Client.Models
{
    public class Agent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string AgentId { get; set; }
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public string Identity { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string Arch { get; set; }
        public string Integrity { get; set; }
        public int CLR { get; set; }
        public List<AgentModule> AgentModules { get; set; }

        private List<Agent> _childAgents;
        public List<Agent> ChildAgents
        {
            get { return _childAgents; }
            set { _childAgents = value; NotifyPropertyChanged("ChildAgents"); }
        }

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