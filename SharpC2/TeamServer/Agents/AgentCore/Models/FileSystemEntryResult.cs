using System;
using System.Collections.Generic;

namespace AgentCore.Models
{
    public sealed class FileSystemEntryResult : SharpC2Result
    {
        public string Size { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public DateTime LastModified { get; set; }
        public override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "Size", Value = Size },
                    new SharpC2ResultProperty { Name = "Type", Value = Type },
                    new SharpC2ResultProperty { Name = "Last Modified", Value = LastModified },
                    new SharpC2ResultProperty { Name = "Name", Value = Name },
                };
            }
        }
    }
}