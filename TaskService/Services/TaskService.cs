using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models.Dto;

namespace TaskService.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
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

            project.Name = dto.Name;
            project.Description = dto.Description;
            await _context.SaveChangesAsync();
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
    }
}
