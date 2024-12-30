using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/users")]
    //[Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IDistributedCache _cache;

        public UserController(IUserService userService, IDistributedCache cache)
        {
            _userService = userService;
            _cache = cache;
        }

        [HttpGet("/search")]
        public async Task<IActionResult> FindUser(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (string.IsNullOrEmpty(email)) return Unauthorized("User not found.");

            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);

            var user = await _userService.GetUserByEmailAsync(userEmail);
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}
