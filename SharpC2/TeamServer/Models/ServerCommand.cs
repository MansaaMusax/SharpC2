using TeamServer.Controllers;

namespace TeamServer.Models
{
    public class ServerCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ServerController.OnServerCommand CallBack { get; set; }
    }
}