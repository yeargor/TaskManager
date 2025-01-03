using UserService.Models.Dto;

namespace UserService.Services
{
    public interface IUserService
    {
        //Task<IEnumerable<PersonDto>> GetAllUsersAsync();
        Task<PersonDto?> GetUserByIdAsync(int id);
        Task<PersonDto?> GetUserByEmailAsync(string email);
    }
}
