using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using UserService.Data;
using UserService.Models;
using UserService.Models.Dto;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public UserService(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        //public async Task<IEnumerable<PersonDto>> GetAllUsersAsync()
        //{
        //    var users = await _context.Persons.ToListAsync();
        //    return users.Select(user => new PersonDto
        //    {
        //        Id = user.Id,
        //        Email = user.Email
        //    });
        //}

        public async Task<PersonDto?> GetUserByIdAsync(int id)
        {
            var user = await FindUser(id);
            if (user == null) return null;

            return new PersonDto
            {
                Id = user.Id,
                Email = user.Email
            };
        }

        public async Task<PersonDto?> GetUserByEmailAsync(string email)
        {
            var user = await FindUser(email);
            if (user == null) return null;

            return new PersonDto
            {
                Id = user.Id,
                Email = user.Email
            };
        }

        public async Task<string?> GetTokenFromCacheAsync(string email)
        {
            return await _cache.GetStringAsync(email);
        }

        public async Task<Person> FindUser(int id)
        {
            var cacheKey = $"user:{id}";
            Person? person = null;

            var cachedTask = await _cache.GetStringAsync(cacheKey);
            if (cachedTask != null)
            {
                person = JsonSerializer.Deserialize<Person>(cachedTask);
            }
            else
            {
                person = await _context.Persons.FirstOrDefaultAsync(p => p.Id == id);

                if (person != null)
                {
                    await CacheInRedis(person, cacheKey);
                }
            }

            return person;
        }

        public async Task<Person> FindUser(string email)
        {
            var cacheKey = $"user:{email}";
            Person? person = null;

            var cachedTask = await _cache.GetStringAsync(cacheKey);
            if (cachedTask != null)
            {
                person = JsonSerializer.Deserialize<Person>(cachedTask);
            }
            else
            {
                person = await _context.Persons.FirstOrDefaultAsync(p => p.Email == email);

                if (person != null)
                {
                    await CacheInRedis(person, cacheKey);
                }
            }

            return person;
        }

        public async Task CacheInRedis(Person task, string cacheKey)
        {
            var userJson = JsonSerializer.Serialize(task);
            await _cache.SetStringAsync(
                        cacheKey,
                        userJson,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                        });
        }
    }
}
