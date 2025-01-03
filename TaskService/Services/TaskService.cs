using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TaskService.Data;
using TaskService.Models.Dto;

namespace TaskService.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        private readonly IDistributedCache _cache;

        public TaskService(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Models.Task>> GetAllTasksAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<Models.Task> CreateTaskAsync(CreatedTaskDto dto)
        {
            var project = new Models.Task
            {
                Name = dto.Name,
                Description = dto.Description
            };
            _context.Tasks.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task UpdateTaskAsync(int id, UpdatedTaskDto dto)
        {
            var project = await _context.Tasks.FindAsync(id);
            if (project == null) throw new Exception("Task not found");

            await DeleteFromCache($"project:{project.Id}");

            project.Name = dto.Name;
            project.Description = dto.Description;
            await _context.SaveChangesAsync();
            await CacheInRedis(project, $"project:{project.Id}");
        }

        public async Task DeleteTaskAsync(int id)
        {
            var project = await _context.Tasks.FindAsync(id);
            if (project != null)
            {
                _context.Tasks.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Models.Task> FindTask(int id)
        {
            var cacheKey = $"task:{id}";
            Models.Task? task = null;

            var cachedTask = await _cache.GetStringAsync(cacheKey);
            if (cachedTask != null)
            {
                task = JsonSerializer.Deserialize<Models.Task>(cachedTask);
            }
            else
            {
                task = await _context.Tasks.FirstOrDefaultAsync(p => p.Id == id);

                if (task != null)
                {
                    await CacheInRedis(task, cacheKey);
                }
            }

            return task;
        }

        public async Task CacheInRedis(Models.Task task, string cacheKey)
        {
            var taskJson = JsonSerializer.Serialize(task);
            await _cache.SetStringAsync(
                        cacheKey,
                        taskJson,
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
