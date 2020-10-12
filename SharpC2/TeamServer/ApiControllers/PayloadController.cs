using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.Models;

using System;

namespace TeamServer.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PayloadController : ControllerBase
    {
        [HttpPost("http")]
        public IActionResult GenerateHttpAgent([FromBody]HttpPayloadRequest request)
        {
            var payload = Controllers.PayloadControllerBase.GenerateHttpAgent(request);

            if (payload.Length > 0)
            {
                return Ok(Convert.ToBase64String(payload));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("tcp")]
        public IActionResult GenerateTcpStager([FromBody]TcpPayloadRequest request)
        {
            var payload = Controllers.PayloadControllerBase.GenerateTcpAgent(request);

            if (payload.Length > 0)
            {
                return Ok(Convert.ToBase64String(payload));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("smb")]
        public IActionResult GenerateSmbStager([FromBody]SmbPayloadRequest request)
        {
            var payload = Controllers.PayloadControllerBase.GenerateSmbAgent(request);

            if (payload.Length > 0)
            {
                return Ok(Convert.ToBase64String(payload));
            }
            else
            {
                return BadRequest();
            }
        }
    }
}