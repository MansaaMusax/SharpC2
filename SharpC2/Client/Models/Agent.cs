using Shared.Models;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Client.Models
{
    public class Agent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string AgentID { get; set; }
        public string Hostname { get; set; }
        public string IPAddress { get; set; }
        public string Identity { get; set; }
        public string Process { get; set; }
        public int PID { get; set; }
        public Native.Platform Arch { get; set; }
        public AgentMetadata.Integrity Integrity { get; set; }

        private DateTime _lastSeen;
        public DateTime LastSeen
        {
            get { return _lastSeen; }
            set { _lastSeen = value; CalculateTimeDiff(); }
        }

        private string _counter;
        public string Counter
        {
            get { return _counter; }
            set { _counter = value; NotifyPropertyChanged(nameof(Counter)); }
        }

        public Agent()
        {
            var timer = new Timer();

            timer.Interval = 500;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CalculateTimeDiff();
        }

        void CalculateTimeDiff()
        {
            var diff = (DateTime.UtcNow - LastSeen).TotalSeconds;

            var result = default(string);

            if (diff < 1)
            {
                result = $"{Math.Round(diff, 2)}s";
            }
            else if (diff > 1 && diff <= 59)
            {
                result = $"{Math.Round(diff, 0)}s";
            }
            else if (diff >= 60 && diff <= 3659)
            {
                var time = diff / 60;
                result = $"{Math.Round(time, 1)}m";
            }
            else if (diff >= 3600)
            {
                var time = diff / 3600;
                result = $"{Math.Round(time, 1)}h";
            }

            Counter = result;
        }

        void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}