using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;

namespace TeamServer.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        [HttpGet("events")]
        public IEnumerable<ServerEvent> GetServerEvents()
        {
            return Program.ServerController.ServerEvents;
        }
    }
}