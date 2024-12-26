using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UserService.Models;

namespace UserService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) //TODO: разобраться с переменными appsettings и docker-compose.yml.
        {
            modelBuilder.HasDefaultSchema("users"); //разобраться со связью efcore и бд
        }
    }
}
