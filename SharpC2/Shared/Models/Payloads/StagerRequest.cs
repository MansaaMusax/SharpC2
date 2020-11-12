namespace Shared.Models
{
    public class StagerRequest
    {
        public string Listener { get; set; }
        public OutputType Type { get; set; }
        public string ExportName { get; set; }

        public enum OutputType
        {
            EXE,
            DLL
        }
    }
}