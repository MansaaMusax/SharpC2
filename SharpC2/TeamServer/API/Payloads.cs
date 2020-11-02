using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Models;

namespace TeamServer.API
{
    [Authorize]
    [Route("api/stager")]
    [ApiController]
    public class Payloads : ControllerBase
    {
        [HttpPost]
        public byte[] GenerateStager([FromBody]StagerRequest Request)
        {
            if (TeamServer.Server.GenerateStager(Request, out byte[] Stager))
            {
                return Stager;
            }
            else
            {
                return null;
            }
        }
    }
}