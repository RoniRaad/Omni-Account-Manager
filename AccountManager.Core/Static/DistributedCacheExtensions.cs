using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AccountManager.Core.Static
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value)
        {
            var json = JsonSerializer.Serialize(value);
            await cache.SetStringAsync(key + typeof(T).Name, json);
        }

        public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key) where T : notnull, new()
        {
            var value = await cache.GetStringAsync(key + typeof(T).Name);

            if (value is null)
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }
    }
}