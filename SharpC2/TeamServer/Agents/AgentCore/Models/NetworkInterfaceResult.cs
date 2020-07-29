using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace AgentCore.Models
{
    public sealed class NetworkInterfaceResult : SharpC2Result
    {
        public string Name { get; set; }
        public string Unicast { get; set; }
        public PhysicalAddress MAC { get; set; }
        public string Gateways { get; set; }
        public string DNS { get; set; }
        public string DHCP { get; set; }
        public override IList<SharpC2ResultProperty> ResultProperties
        {
            get
            {
                return new List<SharpC2ResultProperty>
                {
                    new SharpC2ResultProperty { Name = "Name", Value = Name },
                    new SharpC2ResultProperty { Name = "Unicast", Value = Unicast },
                    new SharpC2ResultProperty { Name = "MAC", Value = MAC },
                    new SharpC2ResultProperty { Name = "Gateways", Value = Gateways },
                    new SharpC2ResultProperty { Name = "DNS Servers", Value = DNS },
                    new SharpC2ResultProperty { Name = "DHCP Servers", Value = DHCP },
                };
            }
        }
    }
}