using System;

namespace Shared.Models
{
    public class StagerRequest
    {
        public string Listener { get; set; }
        public DateTime KillDate { get; set; }
        public OutputType Type { get; set; }
        public string ExportName { get; set; }

        // HTTP only
        public int SleepInterval { get; set; }
        public int SleepJitter { get; set; }

        public enum OutputType
        {
            EXE,
            DLL
        }
    }
}