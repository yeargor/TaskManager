using ProjectService.Model.Dto;
using ProjectService.Model;
using ProjectService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ProjectService.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        private readonly IDistributedCache _cache;

        public ProjectService(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<Project> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            await CacheInRedis(project, $"project:{project.Id}");
            return project;
        }

        public async Task UpdateProjectAsync(int id, UpdateProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) throw new Exception("Task not found");

            await DeleteFromCache($"project:{project.Id}");

            project.Name = dto.Name;
            project.Description = dto.Description;
            await _context.SaveChangesAsync();
            await CacheInRedis(project, $"project:{project.Id}");
        }

        public async Task DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                await DeleteFromCache($"project:{project.Id}");

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Project> FindProject(int id)
        {
            var cacheKey = $"project:{id}";
            Project? project = null;

            var cachedProject = await _cache.GetStringAsync(cacheKey);
            if (cachedProject != null)
            {
                project = JsonSerializer.Deserialize<Project>(cachedProject);
            }
            else
            {
                project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

                if (project != null)
                {
                    await CacheInRedis(project, cacheKey);
                }
            }

            return project;
        }

        public async Task CacheInRedis(Project project, string cacheKey)
        {
            var projectJson = JsonSerializer.Serialize(project);
            await _cache.SetStringAsync(
                        cacheKey,
                        projectJson,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                        });
        }

        public async Task DeleteFromCache(string cacheKey)
        {
            await _cache.RemoveAsync(cacheKey);
        }
    }
}
