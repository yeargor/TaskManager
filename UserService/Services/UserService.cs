using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Dto;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Email = u.Email,
                    FullName = u.FullName
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Email = user.Email,
                FullName = user.FullName
            };
        }
    }
}
