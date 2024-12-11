using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AW_DockerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KillController : ControllerBase
    {

        [HttpGet(Name = "Kill the API")]
        public IEnumerable<string> Get()
        {
            Environment.Exit(-1);
            throw new Exception("APP killed should not reach this");
        }
    }
}
