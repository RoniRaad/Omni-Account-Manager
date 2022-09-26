using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.GraphServices
{
    public sealed class CachedLeagueGraphService : ILeagueGraphService
    {
        private readonly IDistributedCache _persistantCache;
        private readonly ILeagueGraphService _graphService;
        const AccountType accountType = AccountType.League;
        const string cacheKeyFormat = "{0}.{1}.{2}";

        public CachedLeagueGraphService(IDistributedCache persistantCache, ILeagueGraphService graphService)
        {
            _persistantCache = persistantCache;
            _graphService = graphService;
        }

        public async Task<PieChart?> GetRankedChampSelectPieChart(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, accountType, nameof(GetRankedChampSelectPieChart));

            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () => await _graphService.GetRankedChampSelectPieChart(account), TimeSpan.FromHours(1)) ?? new();
        }

        public async Task<BarChart?> GetRankedCsRateByChampBarChartAsync(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, accountType, nameof(GetRankedCsRateByChampBarChartAsync));

            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () => await _graphService.GetRankedCsRateByChampBarChartAsync(account), TimeSpan.FromHours(1)) ?? new();
        }

        public async Task<BarChart?> GetRankedWinrateByChampBarChartAsync(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, accountType, nameof(GetRankedWinrateByChampBarChartAsync));

            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () => await _graphService.GetRankedWinrateByChampBarChartAsync(account), TimeSpan.FromHours(1)) ?? new();
        }

        public async Task<LineGraph?> GetRankedWinsGraph(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, accountType, nameof(GetRankedWinsGraph));

            return await _persistantCache.GetOrCreateAsync(cacheKey,
                async () => await _graphService.GetRankedWinsGraph(account), TimeSpan.FromHours(1)) ?? new();
        }
    }
}
