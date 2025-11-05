using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Core.Data
{
    public interface IRedisUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(string id);
        Task<ApplicationUser> GetByUsernameAsync(string username);
        Task<ApplicationUser> CreateAsync(ApplicationUser user, string password);
        Task<bool> UpdateAsync(ApplicationUser user);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    }

    public class RedisUserRepository : IRedisUserRepository
    {
        private readonly IRedisService _redis;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private const string USER_PREFIX = "user:";
        private const string USERNAME_INDEX = "username:";

        public RedisUserRepository(IRedisService redis, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _redis = redis;
            _passwordHasher = passwordHasher;
        }

        public async Task<ApplicationUser> GetByIdAsync(string id)
        {
            return await _redis.GetAsync<ApplicationUser>($"{USER_PREFIX}{id}");
        }

        public async Task<ApplicationUser> GetByUsernameAsync(string username)
        {
            var userId = await _redis.GetAsync<string>($"{USERNAME_INDEX}{username.ToLower()}");
            if (string.IsNullOrEmpty(userId))
                return null;

            return await GetByIdAsync(userId);
        }

        public async Task<ApplicationUser> CreateAsync(ApplicationUser user, string password)
        {
            // Generate ID if not set
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            // Hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            // Save user
            await _redis.SetAsync($"{USER_PREFIX}{user.Id}", user);

            // Create username index
            await _redis.SetAsync($"{USERNAME_INDEX}{user.UserName.ToLower()}", user.Id);

            return user;
        }

        public async Task<bool> UpdateAsync(ApplicationUser user)
        {
            var existing = await GetByIdAsync(user.Id);
            if (existing == null)
                return false;

            await _redis.SetAsync($"{USER_PREFIX}{user.Id}", user);
            return true;
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
