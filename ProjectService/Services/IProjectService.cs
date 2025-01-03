using ProjectService.Model.Dto;
using ProjectService.Model;

namespace ProjectService.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project> CreateProjectAsync(CreateProjectDto dto);
        Task UpdateProjectAsync(int id, UpdateProjectDto dto);
        Task DeleteProjectAsync(int id);
    }
}
