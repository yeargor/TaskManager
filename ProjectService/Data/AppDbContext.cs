using Microsoft.EntityFrameworkCore;
using ProjectService.Model;
using System.Collections.Generic;

namespace ProjectService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
    }
}
