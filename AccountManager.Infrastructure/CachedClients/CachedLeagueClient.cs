using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.League.Responses;
using AccountManager.Core.Models.RiotGames.TeamFightTactics.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Infrastructure.CachedClients
{
    public class CachedLeagueClient : ILeagueClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILeagueClient _leagueClient;
        public CachedLeagueClient(IMemoryCache memoryCache, ILeagueClient leagueClient)
        {
            _memoryCache = memoryCache;
            _leagueClient = leagueClient;
        }

        public async Task<Rank> GetSummonerRankByPuuidAsync(Account account)
        {
            var cacheKey = $"{nameof(GetSummonerRankByPuuidAsync)}.{account.Username}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    return await _leagueClient.GetSummonerRankByPuuidAsync(account);
                }) ?? new();
        }

        public async Task<Rank> GetTFTRankByPuuidAsync(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetTFTRankByPuuidAsync)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    return await _leagueClient.GetTFTRankByPuuidAsync(account);
                }) ?? new();
        }

        public async Task<List<LeagueQueueMapResponse>?> GetLeagueQueueMappings()
        {
            var cacheKey = nameof(GetLeagueQueueMappings);

            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    return await _leagueClient.GetLeagueQueueMappings();
                }) ?? new();
        }

        public async Task<UserChampSelectHistory?> GetUserChampSelectHistory(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetUserChampSelectHistory)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   return await _leagueClient.GetUserChampSelectHistory(account);
               }) ?? new();
        }

        public async Task<MatchHistory?> GetUserLeagueMatchHistory(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetUserLeagueMatchHistory)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   return await _leagueClient.GetUserLeagueMatchHistory(account);
               }) ?? new();
        }

        public async Task<TeamFightTacticsMatchHistory?> GetUserTeamFightTacticsMatchHistory(Account account)
        {
            var cacheKey = $"{account.Username}.{account.AccountType}.{nameof(GetUserTeamFightTacticsMatchHistory)}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   return await _leagueClient.GetUserTeamFightTacticsMatchHistory(account);
               }) ?? new();
        }
    }
}
