using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TeamServer.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<AgentSessionData> GetConnectedAgents()
        {
            return Program.ServerController.AgentController.ConnectedAgents;
        }

        [HttpGet("events")]
        public IEnumerable<AgentEvent> GetAgentEvents(string agentId)
        {
            return Program.ServerController.AgentController.AgentEvents.Where(a => a.AgentId.Equals(agentId, StringComparison.OrdinalIgnoreCase));
        }

        [HttpPost("command")]
        public void SendAgentCommand([FromBody]AgentCommandRequest request)
        {
            var user = HttpContext.User.Identity.Name;
            Program.ServerController.AgentController.SendAgentCommand(request, user);
        }

        [HttpDelete("clear")]
        public IActionResult ClearAgentCommandQueue(string agentId)
        {
            var user = HttpContext.User.Identity.Name;

            try
            {
                Program.ServerController.AgentController.ClearAgentCommandQueue(agentId);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("remove")]
        public IActionResult RemoveAgent(string agentId)
        {
            var user = HttpContext.User.Identity.Name;

            try
            {
                Program.ServerController.AgentController.RemoveAgent(agentId);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}