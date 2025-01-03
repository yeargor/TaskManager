using AuthService.Models.Dto;
using AuthService.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using Microsoft.EntityFrameworkCore;
using AuthService.Serivces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;

        public AuthService(AppDbContext context, IConfiguration config, IDistributedCache cache)
        {
            _context = context;
            _config = config;
            _cache = cache;
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Persons.AnyAsync(u => u.Email == dto.Email)) return false;

            var user = new Person
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Persons.Add(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            Person user = await FindUserInCache(dto.Email);

            // Проверяем пароль
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = tokenHandler.WriteToken(token);

            await _cache.SetStringAsync(dto.Email, jwtToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            return jwtToken;
        }

        public async Task<Person?> GetUserByEmailAsync(string email)
        {
            Person user = await FindUserInCache(email);

            return user;
        }

        public async Task<Person> FindUserInCache(string email)
        {
            var cacheKey = $"user:{email}";
            Person? user = null;

            var cachedUser = await _cache.GetStringAsync(cacheKey);
            if (cachedUser != null)
            {
                user = JsonSerializer.Deserialize<Person>(cachedUser);
            }
            else
            {
                user = await _context.Persons.FirstOrDefaultAsync(u => u.Email == email);

                if (user != null)
                {
                    await CacheInRedis(user, cacheKey);
                }
            }

            return user;
        }

        public async Task CacheInRedis(Person user, string cacheKey)
        {

            var userJson = JsonSerializer.Serialize(user);
            await _cache.SetStringAsync(
                        cacheKey,
                        userJson,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                        });
        }

        public async Task<string?> GetTokenFromCacheAsync(string email)
        {
            return await _cache.GetStringAsync(email);
        }

        public async Task LogoutAsync(string email)
        {
            var token = await _cache.GetStringAsync(email);
            if (token != null)
            {
                await _cache.SetStringAsync($"blacklist:{token}", "revoked",
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                    });
                await _cache.RemoveAsync(email);
            }
        }
    }
}