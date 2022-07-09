using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Clients;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.GraphServices.Cached
{
    public class CachedValorantGraphService : IValorantGraphService
    {
        private readonly IDistributedCache _persistantCache;
        private readonly IValorantGraphService _valorantGraphService;
        const string cacheKeyFormat = "{0}.{1}.{2}";
        public CachedValorantGraphService(IDistributedCache persistantCache, IValorantGraphService valorantGraphService)
        {
            _persistantCache = persistantCache;
            _valorantGraphService = valorantGraphService;
        }

        public async Task<BarChart> GetRankedACS(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, account.AccountType, nameof(GetRankedACS));
            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () => await _valorantGraphService.GetRankedACS(account), TimeSpan.FromHours(1)) ?? new();
        }

        public async Task<LineGraph> GetRankedRRChangeLineGraph(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, account.AccountType, nameof(GetRankedRRChangeLineGraph));
            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () => await _valorantGraphService.GetRankedRRChangeLineGraph(account), TimeSpan.FromHours(1)) ?? new();
        }

        public async Task<LineGraph> GetRankedWinsLineGraph(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, account.AccountType, nameof(GetRankedWinsLineGraph));
            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () => await _valorantGraphService.GetRankedWinsLineGraph(account), TimeSpan.FromHours(1)) ?? new();
        }

        public async Task<PieChart> GetRecentlyUsedOperatorsPieChartAsync(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, account.AccountType, nameof(GetRankedACS));
            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () => await _valorantGraphService.GetRecentlyUsedOperatorsPieChartAsync(account), TimeSpan.FromHours(1)) ?? new();
        }
    }
}
