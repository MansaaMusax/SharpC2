using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Models;
using System.Collections.Generic;

namespace TeamServer.API
{
    [Authorize]
    [Route("api/server")]
    [ApiController]
    public class Server : ControllerBase
    {
        [HttpGet("events")]
        public IEnumerable<ServerEvent> GetEvents()
        {
            return TeamServer.Server.GetServerEvents();
        }
    }
}