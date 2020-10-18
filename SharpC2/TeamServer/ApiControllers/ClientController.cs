using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;

namespace TeamServer.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ClientController : Controller
    {
        [HttpGet]
        public IEnumerable<string> GetConnectedClients()
        {
            return Program.ServerController.ClientController.ConnectedClients;
        }

        [AllowAnonymous]
        [HttpPost]
        public ClientAuthResponse ClientLogin([FromBody]ClientAuthRequest request)
        {
            return Program.ServerController.ClientController.ClientLogin(request);
        }

        [HttpDelete]
        public IActionResult ClientLogoff()
        {
            var user = HttpContext.User.Identity.Name;

            if (Program.ServerController.ClientController.ClientLogoff(user))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}