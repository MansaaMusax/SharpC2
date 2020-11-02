using System.Collections.Generic;

using TeamServer.Controllers;

namespace TeamServer.Models
{
    public class ServerModule
    {
        public string Name { get; set; }
        public List<Command> Commands { get; set; }

        public class Command
        {
            public string Name { get; set; }
            public ServerController.ServerCommand Delegate { get; set; }
        }
    }
}