using AuthenticationApp.Infra.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace AuthenticationApp.Infra
{
    public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis) : IRedisCacheService
    {
        public Task<string?> GetAsync(string key)
        {
            return cache.GetStringAsync(key);
        }

        public async Task MassLogoutAsync(string email, string ip)
        {
            const string instancePrefix = "AuthenticationApp:";
            var pattern = $"{instancePrefix}loggedUser:{email}:{ip}*";

            var endpoint = redis.GetEndPoints().First();
            var server = redis.GetServer(endpoint);

            await foreach (var fullKey in server.KeysAsync(pattern: pattern))
            {
                // fullKey ex: "AuthenticationApp:loggedUser:joao@ex.com:1.2.3.4"
                var cacheKey = fullKey.ToString().Substring(instancePrefix.Length);

                // busca o refreshToken
                var refreshToken = await GetAsync(cacheKey);

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await RemoveAsync($"refresh:{refreshToken}");
                }

                await RemoveAsync(cacheKey);
            }
        }

        public Task RemoveAsync(string key)
        {
            return cache.RemoveAsync(key);
        }

        public Task SetAsync(string key, string value, TimeSpan? expiration = null)
        {
            expiration ??= TimeSpan.FromHours(8);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            return cache.SetStringAsync(key, value, options);
        }
    }
}
