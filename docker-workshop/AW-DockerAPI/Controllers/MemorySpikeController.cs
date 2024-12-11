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
    public class MemorySpikeController : ControllerBase
    {
        private readonly ILogger<MemorySpikeController> _logger;

        public MemorySpikeController(ILogger<MemorySpikeController> logger)
        {
            this._logger = logger;
        }

        [HttpPost(Name = "GenerateMemorySpike")]
        public void Post([FromBody] CpuSpikeConfig cpuSpikeConfig)
        {
            CreateMemorySpike(cpuSpikeConfig.TimeOfSpikeInSeconds);
        }

        private void CreateMemorySpike(int durationInSeconds)
        {
            List<byte[]> memoryList = new List<byte[]>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                while (true)
                {
                    // Allocate 10 MB of memory repeatedly
                    byte[] buffer = new byte[100 * 1024 * 1024];
                    _logger.LogInformation("Memory size increased with", buffer.Length);
                    memoryList.Add(buffer);

                    // Check if the specified duration has been reached
                    if (stopwatch.Elapsed.TotalSeconds >= durationInSeconds)
                    {
                        break;
                    }
                    _logger.LogInformation("memoryList", memoryList.Count);
                    Thread.Sleep(100); // Sleep to simulate gradual memory build-up
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error or memory limit reached: " + ex.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                // Optionally clear the allocated memory if necessary
                memoryList.Clear();
                GC.Collect(); // Force garbage collection to reclaim memory
            }
        }
    }
}
