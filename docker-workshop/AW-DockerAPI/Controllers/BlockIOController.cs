using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System;
using System.IO;


namespace AW_DockerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockIOController : ControllerBase
    {
        [HttpGet("GenerateBlockIO")]
        public async Task<string> Get()
        {
            string filePath = "1mb_file.bin";

            // Define the size of the file in bytes (1MB).
            int sizeInBytes = 1 * 1024 * 1024; // 1MB

            // Create a buffer with the desired size.
            byte[] buffer = new byte[sizeInBytes];

            // Fill the buffer with random data.
            Random random = new Random();
            random.NextBytes(buffer);

            // Write the data to the file.
            try
            {
                System.IO.File.WriteAllBytes(filePath, buffer);
                Console.WriteLine($"Successfully written 1MB file to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing file: {ex.Message}");
            }

            return "file written";
        }
    }
}
