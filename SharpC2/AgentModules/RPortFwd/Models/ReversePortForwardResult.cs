using System.Collections.Generic;
using System.Threading;

namespace Agent.Models
{
    public class ReversePortForward
    {
        public int BindPort { get; set; }
        public string ForwardHost { get; set; }
        public int ForwardPort { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
    }

    public sealed class ReversePortForwardResult : SharpC2Result
    {
        public int BindPort { get; set; }
        public string ForwardHost { get; set; }
        public int ForwardPort { get; set; }
        public override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "BindPort", Value = BindPort },
                    new SharpC2ResultProperty { Name = "ForwardHost", Value = ForwardHost },
                    new SharpC2ResultProperty { Name = "ForwardPort", Value = ForwardPort },

                };
            }
        }
    }
}