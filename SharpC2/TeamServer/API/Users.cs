using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Models;

namespace TeamServer.API
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class Users : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost]
        public AuthResult UserLogin([FromBody]AuthRequest Request)
        {
            return TeamServer.Server.UserLogon(Request);
        }

        [HttpDelete]
        public bool UserLogoff()
        {
            var nick = HttpContext.User.Identity.Name;
            return TeamServer.Server.UserLogoff(nick);
        }
    }
}