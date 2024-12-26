using AuthService.Models.Dto;
using AuthService.Serivces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (!result) return BadRequest("Registration failed.");
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            if (token == null) return Unauthorized("Invalid email or password.");
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            if (userEmail == null) return Unauthorized();

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return NotFound("User not found.");

            return Ok(new
            {
                user.Id,
                user.Email,
                user.CreatedAt
            });
        }
    }
}
