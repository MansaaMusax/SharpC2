using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Models;

using System.Collections.Generic;

namespace TeamServer.API
{
    [Authorize]
    [Route("api/listeners")]
    [ApiController]
    public class Listeners : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Listener> GetListeners(Listener.ListenerType Type)
        {
            return TeamServer.Server.GetListeners(Type);
        }

        [HttpPost]
        public Listener StartListener([FromBody] ListenerRequest Request)
        {
            if (TeamServer.Server.StartListener(Request, out Listener Listener))
            {
                return Listener;
            }
            else
            {
                return null;
            }
        }

        [HttpDelete]
        public bool StopListener(string Name)
        {
            return TeamServer.Server.StopListener(Name);
        }
    }
}