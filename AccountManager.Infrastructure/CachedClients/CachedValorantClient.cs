using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Clients;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Infrastructure.CachedClients
{
    public class CachedValorantClient : IValorantClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        private readonly IValorantClient _valorantClient;
        public CachedValorantClient(IMemoryCache memoryCache, IDistributedCache persistantCache, IValorantClient valorantClient)
        {
            _memoryCache = memoryCache;
            _persistantCache = persistantCache;
            _valorantClient = valorantClient;
        }

        public async Task<ValorantRankedHistoryResponse?> GetValorantCompetitiveHistory(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetValorantCompetitiveHistory)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   return await _valorantClient.GetValorantCompetitiveHistory(account);
               }) ?? new();
        }

        public async Task<IEnumerable<ValorantMatch>?> GetValorantGameHistory(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetValorantGameHistory)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   return await _valorantClient.GetValorantGameHistory(account);
               }) ?? new List<ValorantMatch>();
        }

        public async Task<Rank> GetValorantRank(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetValorantRank)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    return await _valorantClient.GetValorantRank(account);
                }) ?? new();
        }

        public async Task<List<ValorantSkinLevelResponse>> GetValorantShopDeals(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetValorantShopDeals)}";

            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () =>
                {
                    return await _valorantClient.GetValorantShopDeals(account);
                }, TimeSpan.FromHours(1)) ?? new();
        }

        public async Task<string?> GetValorantToken(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetValorantToken)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(55);
                    return await _valorantClient.GetValorantToken(account);
                }) ?? "";
        }
    }
}
