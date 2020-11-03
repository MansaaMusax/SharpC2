using System;

namespace Shared.Models
{
    public class WebLog
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Listener { get; set; }
        public string Origin { get; set; }
        public string WebRequest { get; set; }
    }
}