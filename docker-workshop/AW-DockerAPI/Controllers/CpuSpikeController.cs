using AW_DockerAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AW_DockerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CpuSpikeController : ControllerBase
    {
        private readonly ILogger<CpuSpikeController> logger;

        public CpuSpikeController(ILogger<CpuSpikeController> logger)
        {
            this.logger = logger;
        }

        [HttpPost(Name = "GenerateCPUSpike")]
        public void Post([FromBody] CpuSpikeConfig cpuSpikeConfig)
        {
            CreateCpuSpike(cpuSpikeConfig.TimeOfSpikeInSeconds);
        }

        private void CreateCpuSpike(int durationInSeconds)
        {
            string requestId = HttpContext.TraceIdentifier;
            int threadId = Thread.CurrentThread.ManagedThreadId;
            logger.LogInformation($"RequestId {requestId} Started the CreateCpuSpike on thread id {threadId}");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                // Perform complex calculations continuously
                double value = 0;
                for (int i = 0; i < int.MaxValue; i++)
                {
                    value += Math.Sqrt(i);
                    if (stopwatch.Elapsed.TotalSeconds >= durationInSeconds)
                    {
                        break;
                    }
                }
                if (stopwatch.Elapsed.TotalSeconds >= durationInSeconds)
                {
                    break;
                }
            }

            stopwatch.Stop();

            logger.LogInformation($"RequestId {requestId} Finished the CreateCpuSpike on thread id {threadId} in {stopwatch.ElapsedMilliseconds} milliseconds");
        }

    }
}
