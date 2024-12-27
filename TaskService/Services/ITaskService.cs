using TaskService.Models.Dto;

namespace TaskService.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<Models.Task>> GetAllTasksAsync();
        Task<Models.Task> CreateTaskAsync(CreatedTaskDto dto);
        Task UpdateTaskAsync(int id, UpdatedTaskDto dto);
        Task DeleteTaskAsync(int id);
    }
}
