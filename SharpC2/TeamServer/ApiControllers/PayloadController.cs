using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.Models;

namespace TeamServer.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PayloadController : ControllerBase
    {
        [HttpPost]
        public IActionResult GeneratePayload([FromBody]PayloadRequest request)
        {
            var user = HttpContext.User.Identity.Name;
            var payload = Controllers.PayloadControllerBase.GenerateAgentPayload(request);

            if (!string.IsNullOrEmpty(payload))
            {
                return Ok(payload);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}