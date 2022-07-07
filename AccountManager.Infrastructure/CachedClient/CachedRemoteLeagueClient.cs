using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.League.Responses;
using AccountManager.Core.Models.RiotGames.TeamFightTactics.Responses;
using AccountManager.Infrastructure.Clients;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Infrastructure.CachedClient
{
    public class CachedRemoteLeagueClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IUserSettingsService<UserSettings> _settings;
        private readonly ILeagueClient _leagueClient;

        public CachedRemoteLeagueClient(IMemoryCache memoryCache, ILeagueClient leagueClient,
            ICurlRequestBuilder curlRequestBuilder, IUserSettingsService<UserSettings> settings)
        {
            _memoryCache = memoryCache;
            _settings = settings;
            _leagueClient = leagueClient;
        }

        public async Task<Rank> GetSummonerRankByPuuidAsync(Account account)
        {
            if (!_settings.Settings.UseAccountCredentials)
                return new Rank();

            var cacheKey = $"{nameof(GetSummonerRankByPuuidAsync)}.{account.Username}";

            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    return await _leagueClient.GetSummonerRankByPuuidAsync(account);
                }) ?? new();
        }

        public async Task<Rank> GetTFTRankByPuuidAsync(Account account)
        {
            var cacheKey = $"{nameof(GetTFTRankByPuuidAsync)}.{account.Username}";

            if (!_settings.Settings.UseAccountCredentials)
                return new Rank();

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

        public async Task<UserChampSelectHistory?> GetUserChampSelectHistory(Account account, int startIndex, int endIndex)
        {
            var cacheKey = $"{nameof(GetUserChampSelectHistory)}.{account.Username}.{startIndex}.{endIndex}";
            
            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   return await _leagueClient.GetUserChampSelectHistory(account, startIndex, endIndex);
               }) ?? new();
        }

        public async Task<MatchHistory?> GetUserLeagueMatchHistory(Account account, int startIndex, int endIndex)
        {
            var cacheKey = $"{nameof(GetUserLeagueMatchHistory)}.{account.Username}.{startIndex}.{endIndex}";
            
            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   return await _leagueClient.GetUserLeagueMatchHistory(account, startIndex, endIndex);
               }) ?? new();
        }

        public async Task<TeamFightTacticsMatchHistory?> GetUserTeamFightTacticsMatchHistory(Account account, int startIndex, int endIndex)
        {
            var cacheKey = $"{nameof(GetUserTeamFightTacticsMatchHistory)}.{account.Username}.{startIndex}.{endIndex}";
            
            return await _memoryCache.GetOrCreateAsync(cacheKey,
               async (entry) =>
               {
                   return await _leagueClient.GetUserTeamFightTacticsMatchHistory(account, startIndex, endIndex);
               }) ?? new();
        }
    }
}
