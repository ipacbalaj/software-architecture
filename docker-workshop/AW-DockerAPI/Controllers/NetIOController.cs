using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AW_DockerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetIOController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NetIOController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("GenerateNETIO")]
        public async Task<string> Get()
        {
            //https://jsonplaceholder.typicode.com/comments

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://jsonplaceholder.typicode.com/comments");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
