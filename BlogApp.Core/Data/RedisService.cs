using StackExchange.Redis;
using System.Text.Json;

namespace BlogApp.Core.Data
{
    public interface IRedisService
    {
        IDatabase GetDatabase();
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<bool> DeleteAsync(string key);
        Task<long> IncrementAsync(string key);
        Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern);
    }

    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisService(IConfiguration configuration)
        {
            var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
        }

        public IDatabase GetDatabase() => _database;

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serialized = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serialized, expiry);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<long> IncrementAsync(string key)
        {
            return await _database.StringIncrementAsync(key);
        }

        public async Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).Select(k => k.ToString());
            return await Task.FromResult(keys);
        }
    }
}
