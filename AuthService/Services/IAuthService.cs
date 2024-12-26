using AuthService.Models.Dto;
using AuthService.Models;

namespace AuthService.Serivces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterDto dto);
        Task<string?> LoginAsync(LoginDto dto);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
