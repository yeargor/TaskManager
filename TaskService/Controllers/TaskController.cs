using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Models.Dto;
using TaskService.Services;

namespace TaskService.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    //[Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService projectService)
        {
            _taskService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _taskService.GetAllTasksAsync();
            return Ok(projects);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(CreatedTaskDto dto)
        {
            var project = await _taskService.CreateTaskAsync(dto);
            return CreatedAtAction(nameof(GetProjects), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdatedTaskDto dto)
        {
            await _taskService.UpdateTaskAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            return NoContent();
        }
    }
}
