using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AW_DockerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExceptionsController : ControllerBase
    {
        // GET: api/<ExceptionsController>
        [HttpGet(Name = "Generate Exception")]
        public IEnumerable<string> Get()
        {
            throw new Exception("Custom exception content");
        }

    }
}
