using SharpC2.Models;

using System.Collections.Generic;

namespace Client.Models
{
    public sealed class ModuleHelpText : SharpC2Result
    {
        public string Module { get; set; }
        public string Command { get; set; }
        public string Description { get; set; } = "";
        public string Usage { get; set; } = "";
        public override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "Module", Value = Module },
                    new SharpC2ResultProperty { Name = "Command", Value = Command },
                    new SharpC2ResultProperty { Name = "Description", Value = Description },
                    new SharpC2ResultProperty { Name = "Usage", Value = Usage }
                };
            }
        }
    }
}