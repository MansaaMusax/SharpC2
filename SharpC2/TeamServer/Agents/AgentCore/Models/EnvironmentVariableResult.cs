using System.Collections.Generic;

namespace AgentCore.Models
{
    public sealed class EnvironmentVariableResult : SharpC2Result
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "Key", Value = Key },
                    new SharpC2ResultProperty { Name = "Value", Value = Value }
                };
            }
        }
    }
}