using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;

namespace TeamServer.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PayloadController : ControllerBase
    {
        [HttpPost("stager")]
        public IActionResult GenerateStager([FromBody]StagerRequest request)
        {
            var stager = Controllers.PayloadController.GenerateStager(request);

            if (stager.Length > 0)
            {
                return Ok(Convert.ToBase64String(stager));
            }
            else
            {
                return BadRequest();
            }
        }
    }
}