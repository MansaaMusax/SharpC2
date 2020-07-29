using System.Collections.Generic;

namespace AgentCore.Models
{
    public sealed class ProcessListResult : SharpC2Result
    {
        public int PID { get; set; }
        public string Name { get; set; }
        public int Session { get; set; }
        public override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "PID", Value = PID },
                    new SharpC2ResultProperty { Name = "Key", Value = Name },
                    new SharpC2ResultProperty { Name = "Session", Value = Session },

                };
            }
        }
    }
}