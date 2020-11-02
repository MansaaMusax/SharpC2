using System.Collections.Generic;

namespace Stager.Models
{
    public class StagerModule
    {
        public List<StagerCommand> Commands { get; set; }

        public class StagerCommand
        {
            public string Name { get; set; }
            public Stager.StagerCommand Delegate { get; set; }
        }
    }
}