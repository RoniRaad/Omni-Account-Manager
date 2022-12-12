using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using LazyCache;

namespace AccountManager.Infrastructure.CachedClients
{
    public partial class CachedRiotClient : IRiotClient
    {
        private readonly IAppCache _memoryCache;
        private readonly IRiotClient _riotClient;
        public CachedRiotClient(IAppCache memoryCache,
            IRiotClient riotclient)
        {
            _memoryCache = memoryCache;
            _riotClient = riotclient;
        }

        public Task<string?> GetPuuId(Account account)
        {
            var cacheKey = $"{account.Username}.{nameof(GetPuuId)}";
            return _memoryCache.GetOrAddAsync(cacheKey,
                (entry) =>
                {
                    return _riotClient.GetPuuId(account);
                });
        }

        public Task<string?> GetExpectedClientVersion()
        {
            var cacheKey = nameof(GetExpectedClientVersion);
            return _memoryCache.GetOrAddAsync(cacheKey,
                (entry) =>
                {
                    return _riotClient.GetExpectedClientVersion();
                });
        }

        public async Task<RegionInfo> GetRegionInfo(Account account)
        {
            var cacheKey = $"{account.Username}.{nameof(GetRegionInfo)}";
            return await _memoryCache.GetOrAddAsync(cacheKey,
                (entry) =>
                {
                    return _riotClient.GetRegionInfo(account);
                }) ?? new();
        }
    }
}
