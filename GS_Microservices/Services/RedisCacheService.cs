using StackExchange.Redis;
using Microsoft.Extensions.Options;

namespace GS_Microservices.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _cache;

        public RedisCacheService(IOptions<RedisSettings> redisSettings)
        {
            if (string.IsNullOrEmpty(redisSettings.Value.ConnectionString))
            {
                throw new ArgumentNullException("Redis ConnectionString", "A string de conexão do Redis não foi configurada corretamente.");
            }

            var connection = ConnectionMultiplexer.Connect(redisSettings.Value.ConnectionString);
            _cache = connection.GetDatabase();
        }

        public T GetCache<T>(string key)
        {
            var value = _cache.StringGet(key);
            return value.IsNullOrEmpty ? default : System.Text.Json.JsonSerializer.Deserialize<T>(value);
        }

        public void SetCache<T>(string key, T value, TimeSpan expiry)
        {
            var jsonData = System.Text.Json.JsonSerializer.Serialize(value);
            _cache.StringSet(key, jsonData, expiry);
        }
    }

    public class RedisSettings
    {
        public string ConnectionString { get; set; }
    }
}
