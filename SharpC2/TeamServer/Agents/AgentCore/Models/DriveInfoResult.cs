using System.Collections.Generic;
using System.IO;

namespace AgentCore.Models
{
    public sealed class DriveInfoResult : SharpC2Result
    {
        public string Name { get; set; }
        public DriveType Type { get; set; }
        public string Label { get; set; } = "";
        public string Format { get; set; } = "";
        public string Capacity { get; set; } = "";
        public string FreeSpace { get; set; } = "";

        public override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "Name", Value = Name },
                    new SharpC2ResultProperty { Name = "Type", Value = Type },
                    new SharpC2ResultProperty { Name = "Label", Value = Label },
                    new SharpC2ResultProperty { Name = "Format", Value = Format },
                    new SharpC2ResultProperty { Name = "Capacity", Value = Capacity },
                    new SharpC2ResultProperty { Name = "FreeSpace", Value = FreeSpace },
                };
            }
        }
    }
}