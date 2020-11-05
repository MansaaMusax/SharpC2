using Shared.Models;

using System.Collections.Generic;

namespace Agent.Modules
{
    public sealed class EnvironmentVariableResult : SharpC2Result
    {
        public string Key { get; set; }
        public string Value { get; set; }

        protected internal override IList<SharpC2ResultProperty> ResultProperties
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