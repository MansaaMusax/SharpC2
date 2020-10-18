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
        [HttpGet]
        public IEnumerable<Listener> GetHttpListeners()
        {
            return Program.ServerController.ListenerController.GetListeners();
        }

        [HttpGet("weblogs")]
        public IOrderedEnumerable<WebLog> GetWebLogs()
        {
            return Program.ServerController.ListenerController.GetWebLogs();
        }

        [HttpPost]
        public Listener NewHttpListener([FromBody] NewListenerRequest request)
        {
            var user = HttpContext.User.Identity.Name;
            return Program.ServerController.ListenerController.StartListener(request, user);
        }

        [HttpDelete("{name}")]
        public void StopListener(string name, ListenerType type)
        {
            var user = HttpContext.User.Identity.Name;
            Program.ServerController.ListenerController.StopListener(name, user);
        }
    }
}