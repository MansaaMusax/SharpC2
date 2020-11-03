using System;

namespace Shared.Models
{
    public class WebLog
    {
        public string Listener { get; set; }
        public string Origin { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string WebRequest { get; set; }
    }
}