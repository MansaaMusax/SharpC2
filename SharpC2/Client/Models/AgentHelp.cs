using Shared.Models;

using System.Collections.Generic;

namespace Client.Models
{
    public class AgentHelp : SharpC2Result
    {
        public string Alias { get; set; } = "";
        public string Usage { get; set; } = "";

        protected override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty> {
                        new SharpC2ResultProperty { Name = "Alias", Value = Alias },
                        new SharpC2ResultProperty { Name = "Usage", Value = Usage }
                    };
            }
        }
    }
}