using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;

namespace TeamServer.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ListenersController : Controller
    {
        [HttpGet("http")]
        public IEnumerable<ListenerHttp> GetHttpListeners()
        {
            var listeners = Program.ServerController.ListenerController.GetHttpListeners();
            return listeners;
        }

        [HttpGet("tcp")]
        public IEnumerable<ListenerTcp> GetTcpListeners()
        {
            var listeners = Program.ServerController.ListenerController.GetTcpListeners();
            return listeners;
        }

        [HttpGet("smb")]
        public IEnumerable<ListenerSmb> GetSmbListeners()
        {
            var listeners = Program.ServerController.ListenerController.GetSmbListeners();
            return listeners;
        }

        [HttpGet("weblogs")]
        public IOrderedEnumerable<WebLog> GetWebLogs()
        {
            return Program.ServerController.ListenerController.GetWebLogs();
        }

        [HttpPost]
        public Listener NewListener([FromBody] NewListenerRequest request)
        {
            var user = HttpContext.User.Identity.Name;
            return Program.ServerController.ListenerController.StartListener(request, user);
        }

        [HttpDelete("{name}")]
        public void StopListener(string name)
        {
            var user = HttpContext.User.Identity.Name;
            Program.ServerController.ListenerController.StopListener(name, user);
        }
    }
}