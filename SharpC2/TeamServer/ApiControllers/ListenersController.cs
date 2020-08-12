using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.Listeners;
using SharpC2.Models;

using System.Collections.Generic;
using System.Linq;

namespace TeamServer.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ListenersController : Controller
    {
        [HttpGet("http")]
        public IEnumerable<ListenerBase> GetHttpListeners()
        {
            return Program.ServerController.ListenerController.GetHttpListeners();
        }

        [HttpGet("tcp")]
        public IEnumerable<ListenerTcp> GetTcpListeners()
        {
            return Program.ServerController.ListenerController.GetTcpListeners();
        }

        [HttpGet("smb")]
        public IEnumerable<ListenerSmb> GetSmbListeners()
        {
            return Program.ServerController.ListenerController.GetSmbListeners();
        }

        [HttpGet("weblogs")]
        public IOrderedEnumerable<WebLog> GetWebLogs()
        {
            return Program.ServerController.ListenerController.GetWebLogs();
        }

        [HttpPost("http")]
        public ListenerHttp NewHttpListener([FromBody] NewHttpListenerRequest request)
        {
            var user = HttpContext.User.Identity.Name;
            return Program.ServerController.ListenerController.StartHttpListener(request, user);
        }

        [HttpPost("tcp")]
        public ListenerTcp NewTcpListener([FromBody] NewTcpListenerRequest request)
        {
            var user = HttpContext.User.Identity.Name;
            return Program.ServerController.ListenerController.StartTcpListener(request);
        }

        [HttpPost("smb")]
        public ListenerSmb NewSmbListener([FromBody] NewSmbListenerRequest request)
        {
            var user = HttpContext.User.Identity.Name;
            return Program.ServerController.ListenerController.StartSmbListener(request);
        }

        [HttpDelete("{id}")]
        public void StopListener(string id, ListenerType type)
        {
            var user = HttpContext.User.Identity.Name;
            Program.ServerController.ListenerController.StopListener(id, type, user);
        }
    }
}