using AW_DockerAPI.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace AW_DockerAPI.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define your DbSets here
        public DbSet<DockerContainer> DockerContainers { get; set; }
    }
}
