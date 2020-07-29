using System.Collections.Generic;

using Common.Models;

namespace TeamServer.Models
{
    public class ServerModule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Developer> Developers { get; set; }
        public List<ServerCommand> ServerCommands { get; set; }
    }
}