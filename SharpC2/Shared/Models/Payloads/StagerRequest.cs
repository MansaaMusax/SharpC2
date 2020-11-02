namespace Shared.Models
{
    public class StagerRequest
    {
        public string Listener { get; set; }
        public int SleepInterval { get; set; }
        public int SleepJitter { get; set; }
    }
}