using Shared.Models;

using System.Collections.Generic;

namespace Client.Models
{
    public class AgentHelp : SharpC2Result
    {
        public string Alias { get; set; } = "";
        public string Description { get; set; } = "";
        public string Usage { get; set; } = "";
        public TaskDefinition.OpsecStyle OPSEC { get; set; } = TaskDefinition.OpsecStyle.NA;

        protected override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "Alias", Value = Alias },
                    new SharpC2ResultProperty { Name = "Description", Value = Description },
                    new SharpC2ResultProperty { Name = "Usage", Value = Usage },
                    new SharpC2ResultProperty { Name = "OPSEC", Value = OPSEC },
                };
            }
        }
    }
}