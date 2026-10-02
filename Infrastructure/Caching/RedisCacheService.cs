using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<T?> GetData<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            if (data == null)
            {
                return default;
            }   

            return JsonSerializer.Deserialize<T>(data)!;
        }

        public async Task SetData<T>(string key, T value)
        {
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
        }
    }
}
