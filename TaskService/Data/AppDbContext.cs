using Microsoft.EntityFrameworkCore;

namespace TaskService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskService.Models.Task> Tasks { get; set; }
    }
}
