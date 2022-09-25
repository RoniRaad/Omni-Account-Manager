using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Clients;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Principal;

namespace AccountManager.Infrastructure.CachedClients
{
    public sealed class CachedValorantClient : IValorantClient
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
                   var value = await _valorantClient.GetValorantCompetitiveHistory(account);
                   if (value is null)
                       entry.SetAbsoluteExpiration(DateTimeOffset.Now);

                   return value;
               }) ?? new();
        }

        public async Task<IEnumerable<ValorantMatch>?> GetValorantGameHistory(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetValorantGameHistory)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   var value = await _valorantClient.GetValorantGameHistory(account);
                   if (value is null)
                       entry.SetAbsoluteExpiration(DateTimeOffset.Now);

                   return value;
               }) ?? new List<ValorantMatch>();
        }

        public async Task<ValorantOperatorsResponse> GetValorantOperators()
        {
            var cacheKey = $"{nameof(GetValorantOperators)}";
            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    var value = await _valorantClient.GetValorantOperators();
                    if (value is null)
                        entry.SetAbsoluteExpiration(DateTimeOffset.Now);

                    return value;
                }) ?? new();
        }

        public async Task<Rank> GetValorantRank(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetValorantRank)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    var value = await _valorantClient.GetValorantRank(account);
                    if (value is null)
                        entry.SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds(1));

                    return value;
                }) ?? new();
        }

        public async Task<List<ValorantSkinLevelResponse>> GetValorantShopDeals(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetValorantShopDeals)}";

            var expireDate = DateTimeOffset.UtcNow;
            expireDate = expireDate
                .AddHours(-expireDate.Hour)
                .AddMinutes(-expireDate.Minute)
                .AddSeconds(-expireDate.Second)
                .AddHours(12);

            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () =>
                {
                    return await _valorantClient.GetValorantShopDeals(account);
                }, expireDate) ?? new();
        }
    }
}
