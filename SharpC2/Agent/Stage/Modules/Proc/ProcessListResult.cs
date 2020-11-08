using Shared.Models;

using System.Collections.Generic;

namespace Agent.Modules
{
    public sealed class ProcessListResult : SharpC2Result
    {
        public int PID { get; set; } = 0;
        public int PPID { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public int SessionID { get; set; } = 0;
        public string Owner { get; set; } = "";
        public Native.Platform Architecture { get; set; } = Native.Platform.Unknown;

        protected internal override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "PID", Value = PID },
                    new SharpC2ResultProperty { Name = "PPID", Value = PPID },
                    new SharpC2ResultProperty { Name = "Name", Value = Name },
                    new SharpC2ResultProperty { Name = "SessionID", Value = SessionID },
                    new SharpC2ResultProperty { Name = "Owner", Value = Owner },
                    new SharpC2ResultProperty { Name = "Architecture", Value = Architecture },
                    new SharpC2ResultProperty { Name = "Path", Value = Path }
                };
            }
        }
    }
}