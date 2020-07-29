using System;

namespace SharpC2.Models
{
    public class WebLog
    {
        public string ListenerId { get; set; }
        public string Origin { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string WebRequest { get; set; }
    }
}