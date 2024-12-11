using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AW_DockerAPI.HealthChecks
{
    public class NodeAPIHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NodeAPIHealthCheck> _logger;

        public NodeAPIHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NodeAPIHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory;
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task<string> GetDataAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var nodeAPIURL = _configuration["NODE_API_URL"];

            _logger.LogInformation($"NODE API url is set from ENV files as {nodeAPIURL}");

            var response = await client.GetAsync(string.IsNullOrEmpty(nodeAPIURL) ? "http://localhost:3000" : nodeAPIURL);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await GetDataAsync();
                return HealthCheckResult.Healthy("Everything is working fine");
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy("Some components have issues");

            }
        }
    }
}