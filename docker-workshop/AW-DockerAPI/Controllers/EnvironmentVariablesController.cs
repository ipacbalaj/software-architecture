using AW_DockerAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AW_DockerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnvironmentVariablesController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public EnvironmentVariablesController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet("EnvironmentVariable")]
        public async Task<EnvironmentVariablesDto> Get()
        {
            return new EnvironmentVariablesDto()
            {
                ASPNETCORE_ENVIRONMENT = configuration["ASPNETCORE_ENVIRONMENT"],
                CustomEnvironmentVariable = configuration["CUSTOM_ENV"],
                NodeAPIUrl= configuration["NODE_API_URL"]
        };
        }
    }
}
