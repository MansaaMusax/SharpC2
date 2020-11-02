using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Models;

using System;
using System.Collections.Generic;

namespace TeamServer.API
{
    [Authorize]
    [Route("api/agents")]
    [ApiController]
    public class Agents : ControllerBase
    {
        [HttpGet]
        public IEnumerable<AgentMetadata> GetAgents()
        {
            return TeamServer.Server.GetAgents();
        }

        [HttpGet("events")]
        public IEnumerable<AgentEvent> GetAgentEvents(string AgentID, DateTime Date)
        {
            return TeamServer.Server.GetAgentEvents(AgentID, Date);
        }

        [HttpPost]
        public void SendAgentCommand([FromBody]AgentCommandRequest Request)
        {
            var nick = HttpContext.User.Identity.Name;
            TeamServer.Server.SendAgentCommand(Request, nick);
        }
    }
}